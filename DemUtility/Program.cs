using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using CommandLine;
using Pmad.Cartography;
using Pmad.Cartography.Databases;
using Pmad.Cartography.DataCells;

namespace DemUtility
{
    [Verb("repack", HelpText = "Create a copy of a DEM database with specified compresssion.")]
    internal class RepackOptions
    {
        [Option('s', "source", Required = true, HelpText = "Source directory.")]
        public string? Source { get; set; }

        [Option('t', "target", Required = true, HelpText = "Target directory.")]
        public string? Target { get; set; }

        [Option('c', "compression", Required = false, HelpText = "Compression to use: 'GZip', 'ZSTD', 'Brotli', or 'None' (ZSTD by default).")]
        public Compression TargetCompression { get; set; } = Compression.ZSTD;

        [Option('m', "max-cpu", Required = false, HelpText = "Number of CPU Cores that can be used for process.")]
        public int MaxCPU { get; set; } = -1;

        [Option('k', "keep", Required = false, HelpText = "Keep existing files.")]
        public bool Keep { get; set; }
    }

    [Verb("index", HelpText = "Build index.")]
    internal class IndexOptions
    {
        [Option('p', "path", Required = true, HelpText = "Database directory.")]
        public string? Source { get; set; }
    }

    internal class Program
    {
        static int Main(string[] args)
        {
            return CommandLine.Parser.Default.ParseArguments<RepackOptions, IndexOptions>(args)
              .MapResult(
                (RepackOptions opts) => Repack(opts),
                (IndexOptions opts) => Index(opts),
                errs => 1);
        }

        private static int Index(IndexOptions opts)
        {
            if (string.IsNullOrEmpty(opts.Source))
            {
                throw new ArgumentNullException();
            }
            var source = new DemFileSystemStorage(opts.Source);
            var index = source.BuildIndex();
            using(var file = File.Create(Path.Combine(opts.Source, "index.json")))
            {
                JsonSerializer.Serialize(file, index);
            }
            return 0;
        }

        private static int Repack(RepackOptions opts)
        {
            if (string.IsNullOrEmpty(opts.Source))
            {
                throw new ArgumentNullException();
            }
            if (string.IsNullOrEmpty(opts.Target))
            {
                throw new ArgumentNullException();
            }

            var files = Directory.GetFiles(opts.Source, "*.*", SearchOption.AllDirectories);

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
            if (opts.MaxCPU > 0)
            {
                parallel.MaxDegreeOfParallelism = opts.MaxCPU;
            }

            Directory.CreateDirectory(opts.Target);

            if (demFiles.Count > 0)
            {
                Console.WriteLine($"{demFiles.Count} DEM files to process.");
                using (var report = new ProgressReport("DEM", demFiles.Count))
                {
                    Parallel.ForEach(demFiles, parallel, file =>
                    {
                        var filename = CompressionHelper.GetFileName(Path.GetFileName(file)) + CompressionHelper.GetExtension(opts.TargetCompression);
                        var target = Path.Combine(opts.Target, filename);
                        if (!opts.Keep || !File.Exists(target))
                        {
                            CompressionHelper.Write(target, opts.TargetCompression,
                                output => CompressionHelper.Read(file, input => input.CopyTo(output)));
                        }
                        report.ReportOneDone();
                    });
                }
            }

            if (zipFiles.Count > 0)
            {
                Console.WriteLine($"{zipFiles.Count} ZIP files to scan.");
                using (var report = new ProgressReport("ZIP", zipFiles.Count))
                {
                    Parallel.ForEach(zipFiles, parallel, file =>
                    {
                        using (var archive = new ZipArchive(File.OpenRead(file), ZipArchiveMode.Read))
                        {
                            foreach(var entry in archive.Entries)
                            {
                                if (entry.Name.EndsWith("_DSM.tif", StringComparison.OrdinalIgnoreCase))
                                {
                                    var filename = entry.Name + CompressionHelper.GetExtension(opts.TargetCompression);
                                    var target = Path.Combine(opts.Target, filename);
                                    if (!opts.Keep || !File.Exists(target))
                                    {
                                        using (var input = entry.Open())
                                        {
                                            CompressionHelper.Write(target, opts.TargetCompression,
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
    }
}