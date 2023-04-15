﻿using System;
using System.Diagnostics;
using System.Threading;

namespace DemUtility
{
    internal class ProgressReport : IDisposable
    {
        private readonly string taskName;
        private readonly int itemsToDo;
        private readonly Stopwatch sw;
        private readonly Stopwatch lastReport;
        private readonly object locker = new object();
        private int lastDone = 0;

        public ProgressReport(string taskName, int itemsToDo = 1)
        {
            this.taskName = taskName;
            this.itemsToDo = itemsToDo;
            this.sw = Stopwatch.StartNew();
            this.lastReport = Stopwatch.StartNew();

            Trace.WriteLine(string.Empty);
            Trace.WriteLine($"Begin task {taskName}");
            Console.Write(taskName);
            WritePercent(0);
        }

        public int Total
        {
            get { return itemsToDo; }
        }

        public void ReportOneDone()
        {
            Interlocked.Increment(ref lastDone);
            DrawDone(lastDone);
        }

        public void ReportItemsDone(int done)
        {
            lastDone = done;
            DrawDone(done);
        }

        private void DrawDone(int done)
        {
            if (lastReport.ElapsedMilliseconds > 500)
            {
                lock (locker)
                {
                    if (lastReport.ElapsedMilliseconds > 500)
                    {
                        lastReport.Restart();
                        WritePercent(done * 100.0 / itemsToDo);

                        if (done > 0)
                        {
                            var milisecondsLeft = sw.ElapsedMilliseconds * (itemsToDo - done) / done;
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
                }
            }
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

        public void TaskDone()
        {
            if (Console.IsOutputRedirected)
            {
                Console.Write(' ');
            }
            sw.Stop();
            WritePercent(100d);
            Console.Write($"Done in {Math.Ceiling(sw.ElapsedMilliseconds / 1000d)} sec");
            CleanEndOfLine();
            Console.WriteLine();
            Trace.WriteLine($"Task {taskName} took {sw.ElapsedMilliseconds} msec");
            Trace.Flush();
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
            TaskDone();
        }
    }
}
