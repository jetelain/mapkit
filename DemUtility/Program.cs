using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using CommandLine;
using MapToolkit;
using MapToolkit.Databases;
using MapToolkit.DataCells;
using MapToolkit.DataCells.FileFormats;

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

    internal abstract class BoxBase
    {
        [Option('p', "path", Required = true, HelpText = "Database directory or URL.")]
        public string? Source { get; set; }

        [Option("lat1", Required = true)]
        public double Lat1 { get; set; }

        [Option("lat2", Required = true)]
        public double Lat2 { get; set; }

        [Option("lon1", Required = true)]
        public double Lon1 { get; set; }

        [Option("lon2", Required = true)]
        public double Lon2 { get; set; }
    }

    [Verb("image", HelpText = "Generate an image for a portion of a database")]
    internal class ImageOptions : BoxBase
    {
        [Option('t', "target", Required = true, HelpText = "Target PNG file.")]
        public string? Target { get; set; }
    }

    [Verb("export", HelpText = "Export data from a database into an other file format.")]
    internal class ExportOptions : BoxBase
    {
        [Option('t', "target", Required = true, HelpText = "Target file.")]
        public string? Target { get; set; }

        [Option('f', "format", HelpText = "Target file format.")]
        public string? Format { get; set; }
    }

    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            return await CommandLine.Parser.Default.ParseArguments<RepackOptions, IndexOptions, ImageOptions, ExportOptions>(args)
              .MapResult(
                (RepackOptions opts) => Repack(opts),
                (IndexOptions opts) => Index(opts),
                (ImageOptions opts) => Image(opts),
                (ExportOptions opts) => Export(opts),
                errs => Task.FromResult(1));
        }

        private static async Task<int> Export(ExportOptions opts)
        {
            if (string.IsNullOrEmpty(opts.Target))
            {
                throw new ArgumentNullException();
            }
            var extension = Path.GetExtension(opts.Target);
            if (!string.IsNullOrEmpty(opts.Format))
            {
                extension = "." + opts.Format;
            }
            switch(extension.ToLowerInvariant())
            {
                case EsriAsciiHelper.Extension:
                    using (var writer = File.CreateText(opts.Target))
                    {
                        EsriAsciiHelper.SaveDataCell(writer, (await GetView<float>(opts)).ToDataCell());
                    }
                    break;
                case ".xyz":
                    using (var writer = File.CreateText(opts.Target))
                    {
                        WriteXyz(writer, (await GetView<double>(opts)).ToDataCell());
                    }
                    break;
            }

            return 0;
        }

        private static void WriteXyz(StreamWriter writer, DemDataCellBase<double> view)
        {
            for(int lat = 0; lat < view.PointsLat; lat++)
            {
                int lon = 0;
                foreach (var point in view.GetPointsOnParallel(lat, 0, view.PointsLon))
                {
                    if (!double.IsNaN(point.Elevation))
                    {
                        writer.WriteLine(FormattableString.Invariant($"{point.Coordinates.Longitude}\t{point.Coordinates.Latitude}\t{point.Elevation}"));
                    }
                    lon++;
                }
            }
        }

        private static async Task<int> Image(ImageOptions opts)
        {
            if (string.IsNullOrEmpty(opts.Source))
            {
                throw new ArgumentNullException();
            }
            if (string.IsNullOrEmpty(opts.Target))
            {
                throw new ArgumentNullException();
            }
            DemDataView<double> view = await GetView<double>(opts);
            view.ToDataCell().SaveImagePreviewAbsolute(opts.Target);
            return 0;
        }

        private static async Task<DemDataView<TPixel>> GetView<TPixel>(BoxBase opts) where TPixel : unmanaged
        {
            if (string.IsNullOrEmpty(opts.Source))
            {
                throw new ArgumentNullException();
            }
            var source = new DemDatabase(GetStorage(opts.Source));
            var view = await source.CreateView<TPixel>(new Coordinates(opts.Lat1, opts.Lon1), new Coordinates(opts.Lat2, opts.Lon2));
            return view;
        }

        private static IDemStorage GetStorage(string source)
        {
            if (source.StartsWith("http:", StringComparison.OrdinalIgnoreCase) || source.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
            {
                return new DemHttpStorage(new Uri(source));
            }
            return new DemFileSystemStorage(source);
        }

        private static Task<int> Index(IndexOptions opts)
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
            return Task.FromResult(0);
        }

        private static Task<int> Repack(RepackOptions opts)
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
            return Task.FromResult(0);
        }
    }
}