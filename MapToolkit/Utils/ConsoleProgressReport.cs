using System;
using System.Diagnostics;

namespace DemUtility
{
    [Obsolete("Use Pmad.ProgressTracking instead. See https://github.com/jetelain/ProgressToolkit")]
    public class ConsoleProgressReport : IDisposable, IProgress<double>
    {
        private readonly string taskName;
        private readonly double total;
        private readonly Stopwatch sw;
        private readonly Stopwatch lastReport;
        private readonly object locker = new object();

        public ConsoleProgressReport(string taskName, double maximum = 100.0)
        {
            this.taskName = taskName;
            this.total = maximum;
            this.sw = Stopwatch.StartNew();
            this.lastReport = Stopwatch.StartNew();
            Console.Write(taskName);
            WritePercent(0);
        }

        public void Report(double done)
        {
            if (done == total)
            {
                Finished();
                return;
            }

            if (lastReport.ElapsedMilliseconds > 500)
            {
                lock (locker)
                {
                    if (lastReport.ElapsedMilliseconds > 500)
                    {
                        ReportSafe(done);
                    }
                }
            }
        }

        private void ReportSafe(double done)
        {
            lastReport.Restart();
            WritePercent(done * 100.0 / total);
            if (done > 0)
            {
                var milisecondsLeft = sw.ElapsedMilliseconds * (total - done) / done;
                if (milisecondsLeft > 120000d)
                {
                    Console.Write($"{Math.Round(milisecondsLeft / 60000d)} min left");
                }
                else
                {
                    Console.Write($"{Math.Ceiling(milisecondsLeft / 1000d)} sec left");
                }
            }
            CleanEndOfLine();
        }

        private void WritePercent(double percent)
        {
            if (!Console.IsOutputRedirected)
            {
                var cols = Math.Max(0, Math.Min(20, (int)(percent / 5)));
                Console.CursorLeft = 20;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(new string('#', cols));
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(new string('-', 20 - cols));
                Console.Write(' ');
                Console.Write($"{percent,6:0.00} % ");
            }
        }

        private void Finished()
        {
            if (!sw.IsRunning)
            {
                return;
            }

            if (Console.IsOutputRedirected)
            {
                Console.Write(' ');
            }
            sw.Stop();
            WritePercent(100d);
            if (sw.ElapsedMilliseconds < 10000)
            {
                Console.Write($"Done in {sw.ElapsedMilliseconds} msec");
            }
            else
            {
                Console.Write($"Done in {Math.Ceiling(sw.ElapsedMilliseconds / 1000d)} sec");
            }
            CleanEndOfLine();
            Console.WriteLine();
        }

        private static void CleanEndOfLine()
        {
            if (!Console.IsOutputRedirected)
            {
                Console.Write(new string(' ', Console.BufferWidth - Console.CursorLeft - 1));
            }
        }

        public void Dispose()
        {
            Finished();
        }

    }
}
