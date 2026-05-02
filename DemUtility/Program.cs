using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Pmad.Cartography;
using Pmad.Cartography.Databases;
using Pmad.Cartography.DataCells;
using Pmad.ProgressTracking;

namespace DemUtility
{
    internal class Program
    {
        static int Main(string[] args)
        {
            var rootCommand = new RootCommand("DEM database utility.");

            // repack
            var repackSourceOption = new Option<string>("--source", "-s") { Description = "Source directory.", Required = true };
            var repackTargetOption = new Option<string>("--target", "-t") { Description = "Target directory.", Required = true };
            var repackCompressionOption = new Option<Compression>("--compression", "-c") { Description = "Compression to use: 'GZip', 'ZSTD', 'Brotli', or 'None' (ZSTD by default).", DefaultValueFactory = _ => Compression.ZSTD };
            var repackMaxCpuOption = new Option<int>("--max-cpu", "-m") { Description = "Number of CPU Cores that can be used for process.", DefaultValueFactory = _ => -1 };
            var repackKeepOption = new Option<bool>("--keep", "-k") { Description = "Keep existing files." };

            var repackCommand = new Command("repack", "Create a copy of a DEM database with specified compression.");
            repackCommand.Options.Add(repackSourceOption);
            repackCommand.Options.Add(repackTargetOption);
            repackCommand.Options.Add(repackCompressionOption);
            repackCommand.Options.Add(repackMaxCpuOption);
            repackCommand.Options.Add(repackKeepOption);
            repackCommand.SetAction(parseResult =>
            {
                using var render = ConsoleProgessHelper.Create();
                return Repack(
                    parseResult.GetValue(repackSourceOption)!,
                    parseResult.GetValue(repackTargetOption)!,
                    parseResult.GetValue(repackCompressionOption),
                    parseResult.GetValue(repackMaxCpuOption),
                    parseResult.GetValue(repackKeepOption),
                    render);
            });

            // index
            var indexPathOption = new Option<string>("--path", "-p") { Description = "Database directory.", Required = true };

            var indexCommand = new Command("index", "Build index.");
            indexCommand.Options.Add(indexPathOption);
            indexCommand.SetAction(async parseResult =>
            {
                using var render = ConsoleProgessHelper.Create();
                return await Index(parseResult.GetValue(indexPathOption)!, render);
            });

            // update-index
            var updateIndexPathOption = new Option<string>("--path", "-p") { Description = "Database directory.", Required = true };

            var updateIndexCommand = new Command("update-index", "Add missing SHA-256 checksums to an existing index file.");
            updateIndexCommand.Options.Add(updateIndexPathOption);
            updateIndexCommand.SetAction(async parseResult =>
            {
                return await UpdateIndex(parseResult.GetValue(updateIndexPathOption)!);
            });

            // check
            var checkPathOption = new Option<string?>("--path", "-p") { Description = "Local database directory." };
            var checkUrlOption = new Option<string?>("--url", "-u") { Description = "HTTP base URL of the database." };
            var checkVerifyOption = new Option<bool>("--verify", "-v") { Description = "Verify SHA-256 checksums of files (local or downloaded via HTTP)." };
            var checkSampleOption = new Option<int?>("--sample", "-n") { Description = "Number of randomly sampled cells to verify (verifies all cells when omitted)." };

            var checkCommand = new Command("check", "Check a DEM database (local or HTTP) and report its content.");
            checkCommand.Options.Add(checkPathOption);
            checkCommand.Options.Add(checkUrlOption);
            checkCommand.Options.Add(checkVerifyOption);
            checkCommand.Options.Add(checkSampleOption);
            checkCommand.SetAction(async parseResult =>
            {
                var path = parseResult.GetValue(checkPathOption);
                var url = parseResult.GetValue(checkUrlOption);
                var verify = parseResult.GetValue(checkVerifyOption);
                var sample = parseResult.GetValue(checkSampleOption);
                if (path == null && url == null)
                {
                    Console.Error.WriteLine("Either --path or --url must be specified.");
                    return 1;
                }
                if (path != null && url != null)
                {
                    Console.Error.WriteLine("Only one of --path or --url can be specified.");
                    return 1;
                }
                if (sample.HasValue && sample.Value <= 0)
                {
                    Console.Error.WriteLine("--sample must be a positive integer.");
                    return 1;
                }
                return await Check(path, url, verify, sample);
            });

            rootCommand.Subcommands.Add(repackCommand);
            rootCommand.Subcommands.Add(indexCommand);
            rootCommand.Subcommands.Add(updateIndexCommand);
            rootCommand.Subcommands.Add(checkCommand);

            return rootCommand.Parse(args).Invoke();
        }

        private static async Task<int> Index(string sourcePath, IProgressScope render)
        {
            var source = new DemFileSystemStorage(sourcePath);
            DemDatabaseIndex index;
            using (var progress = render.CreatePercent("Build index"))
            {
                index = await source.BuildIndexAsync(progress).ConfigureAwait(false);
            }
            using (var file = File.Create(Path.Combine(sourcePath, "index.json")))
            {
                JsonSerializer.Serialize(file, index);
            }
            return 0;
        }

        private static int Repack(string sourcePath, string targetPath, Compression targetCompression, int maxCPU, bool keep, IProgressScope render)
        {
            var files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);

            var demFiles = new List<string>();
            var zipFiles = new List<string>();

            foreach (var file in files)
            {
                if (DemDataCell.IsDemDataCellFile(file))
                {
                    demFiles.Add(file);
                }
                else if (file.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    zipFiles.Add(file);
                }
            }

            var parallel = new ParallelOptions();
            if (maxCPU > 0)
            {
                parallel.MaxDegreeOfParallelism = maxCPU;
            }

            Directory.CreateDirectory(targetPath);

            if (demFiles.Count > 0)
            {
                using (var report = render.CreateInteger("DEM", demFiles.Count))
                {
                    Parallel.ForEach(demFiles, parallel, file =>
                    {
                        var filename = CompressionHelper.GetFileName(Path.GetFileName(file)) + CompressionHelper.GetExtension(targetCompression);
                        var target = Path.Combine(targetPath, filename);
                        if (!keep || !File.Exists(target))
                        {
                            CompressionHelper.Write(target, targetCompression,
                                output => CompressionHelper.Read(file, input => input.CopyTo(output)));
                        }
                        report.ReportOneDone();
                    });
                }
            }

            if (zipFiles.Count > 0)
            {
                using (var report = render.CreateInteger("ZIP", zipFiles.Count))
                {
                    Parallel.ForEach(zipFiles, parallel, file =>
                    {
                        using (var archive = new ZipArchive(File.OpenRead(file), ZipArchiveMode.Read))
                        {
                            foreach (var entry in archive.Entries)
                            {
                                if (entry.Name.EndsWith("_DSM.tif", StringComparison.OrdinalIgnoreCase))
                                {
                                    var filename = entry.Name + CompressionHelper.GetExtension(targetCompression);
                                    var target = Path.Combine(targetPath, filename);
                                    if (!keep || !File.Exists(target))
                                    {
                                        using (var input = entry.Open())
                                        {
                                            CompressionHelper.Write(target, targetCompression,
                                                output => input.CopyTo(output));
                                        }
                                    }
                                }
                            }
                        }
                        report.ReportOneDone();
                    });
                }
            }
            return 0;
        }

        private static async Task<int> UpdateIndex(string sourcePath)
        {
            var indexFile = Path.Combine(sourcePath, "index.json");
            if (!File.Exists(indexFile))
            {
                Console.Error.WriteLine($"No index.json found in '{sourcePath}'. Run 'index' first.");
                return 1;
            }

            DemDatabaseIndex index;
            using (var file = File.OpenRead(indexFile))
            {
                index = JsonSerializer.Deserialize<DemDatabaseIndex>(file)!;
            }

            if (index.Cells.Count(c => c.Sha256 == null) == 0)
            {
                Console.WriteLine("All entries already have a SHA-256 checksum. Nothing to do.");
                return 0;
            }

            // Backup existing index
            File.Copy(indexFile, indexFile + $"-{DateTime.Now:yyyyMMddHHmmss}.bak");

            using var render = ConsoleProgessHelper.Create();
            var updated = new List<DemDatabaseFileInfos>();
            using (var report = render.CreateInteger("SHA-256", index.Cells.Count))
            {
                foreach (var cell in index.Cells)
                {
                    if (cell.Sha256 == null)
                    {
                        var fullPath = Path.Combine(sourcePath, cell.Path.Replace('/', Path.DirectorySeparatorChar));
                        var hash = await Sha256Helper.ComputeHexAsync(fullPath).ConfigureAwait(false);
                        updated.Add(new DemDatabaseFileInfos(cell.Path, cell.Metadata, hash));
                    }
                    else
                    {
                        updated.Add(cell);
                    }
                    report.ReportOneDone();
                }
            }

            var newIndex = new DemDatabaseIndex(updated);
            using (var file = File.Create(indexFile))
            {
                JsonSerializer.Serialize(file, newIndex);
            }
            return 0;
        }
        private static async Task<int> Check(string? localPath, string? url, bool verify, int? sample = null)
        {
            IDemStorage storage;
            if (localPath != null)
            {
                if (!Directory.Exists(localPath))
                {
                    Console.Error.WriteLine($"Directory '{localPath}' does not exist.");
                    return 1;
                }
                storage = new DemFileSystemStorage(localPath);
            }
            else
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    Console.Error.WriteLine($"Invalid URL '{url}'.");
                    return 1;
                }
                storage = new DemHttpStorage(uri);
            }

            Console.WriteLine("Reading index...");
            DemDatabaseIndex index;
            try
            {
                index = await storage.ReadIndex();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to read index: {ex.Message}");
                return 1;
            }

            Console.WriteLine($"Cells    : {index.Cells.Count}");

            if (index.Cells.Count > 0)
            {
                var withSha256 = index.Cells.Count(c => c.Sha256 != null);
                Console.WriteLine($"SHA-256  : {withSha256}/{index.Cells.Count}");

                var minLat = index.Cells.Min(c => c.Metadata.Start.Latitude);
                var maxLat = index.Cells.Max(c => c.Metadata.End.Latitude);
                var minLon = index.Cells.Min(c => c.Metadata.Start.Longitude);
                var maxLon = index.Cells.Max(c => c.Metadata.End.Longitude);
                Console.WriteLine($"Coverage : Lat [{minLat:F4} ; {maxLat:F4}], Lon [{minLon:F4} ; {maxLon:F4}]");
            }

            if (verify)
            {
                Console.WriteLine("Verifying checksums...");
                int errors = 0;
                int skipped = 0;
                var cellsWithHash = index.Cells.Where(c => c.Sha256 != null).ToList();
                IEnumerable<DemDatabaseFileInfos> cellsToVerify;
                if (sample.HasValue && sample.Value < cellsWithHash.Count)
                {
                    var rng = new Random();
                    cellsToVerify = cellsWithHash.OrderBy(_ => rng.Next()).Take(sample.Value).ToList();
                    Console.WriteLine($"  Sampling {sample.Value}/{cellsWithHash.Count} cells with a checksum.");
                }
                else
                {
                    cellsToVerify = cellsWithHash;
                }
                skipped = index.Cells.Count - cellsWithHash.Count;
                using (var render = ConsoleProgessHelper.Create())
                {
                    foreach (var cell in cellsToVerify.WithProgress(render, "Checksum"))
                    {
                        var actualHash = await storage.GetSha256Async(cell.Path).ConfigureAwait(false);
                        if (actualHash == null)
                        {
                            render.WriteLine($"  MISSING: {cell.Path}");
                            errors++;
                        }
                        else if (!string.Equals(actualHash, cell.Sha256, StringComparison.OrdinalIgnoreCase)) 
                        {
                            render.WriteLine($"  INVALID: Actual:'{actualHash}' Expected:'{cell.Sha256}'");
                            errors++;
                        }
                    }
                }
                if (skipped > 0)
                {
                    Console.WriteLine($"  Skipped {skipped} cell(s) without a stored checksum.");
                }
                if (errors == 0)
                {
                    Console.WriteLine($"  All checksums OK.");
                }
                else
                {
                    Console.Error.WriteLine($"  {errors} error(s) found.");
                    return 1;
                }
            }

            return 0;
        }
    }
}