using System;
using System.Collections.Generic;
using System.Linq;
using SimpleDEM.Drawing;
using SimpleDEM.Projections;
using SixLabors.ImageSharp;

namespace SimpleDEM.Contours
{
    public class ContourRender
    {
        private readonly IDrawSurface writer;
        private readonly IDrawTextStyle lt;
        private readonly IDrawStyle ls;
        private readonly IDrawStyle lm;

        public ContourRender(IDrawSurface writer)
        {
            this.writer = writer;

            lt = writer.AllocateTextStyle(
                new[] { "Calibri", "sans-serif" },
                18,
                new SolidColorBrush(Color.ParseHex("B29A94")),
                new Pen(new SolidColorBrush(Color.ParseHex("FFFFFFCC")), 3),
                true,
                "lt"); 

            ls = writer.AllocateStyle(null, new Pen(new SolidColorBrush(Color.ParseHex("D4C5BF")), 2), "ls");

            lm = writer.AllocateStyle(null, new Pen(new SolidColorBrush(Color.ParseHex("B29A94")), 2), "lm");
        }


        public void Render(ContourGraph graph, IProjectionArea projection, Image? hillshade)
        {
            if (hillshade != null)
            {
                writer.DrawImage(hillshade, Vector.Zero, projection.Size, 0.5);
            }
            var masters = new List<ContourLine>();
            foreach (var line in graph.Lines)
            {
                if (line.Level % 50 == 0)
                {
                    masters.Add(line);
                }
                else
                {
                    RenderLine( projection, line);
                }
            }
            foreach(var line in masters)
            {
                RenderMasterLine(projection, line);
            }
        }

        private void RenderLine(IProjectionArea projection, ContourLine line)
        {
            writer.DrawPolyline(line.Points.Select(p => projection.Project(p)), ls);
        }

        private void RenderMasterLine(IProjectionArea projection, ContourLine line)
        {
            var points = new List<Vector>();
            var elevationMarks = new List<List<Vector>>();
            var elevationMark = new List<Vector>();

            var len = 0.0;
            var reg = 0.0;
            Vector? prev = null;
            foreach (var point in line.Points)
            {
                var p = projection.Project(point);
                points.Add(p);

                if (prev != null)
                {
                    var l = Math.Sqrt(Math.Pow(prev.X - p.X, 2) + Math.Pow(prev.Y - p.Y, 2)) / writer.Scale;
                    len += l;
                    reg += l;
                }
                if (reg < 75)
                {
                    elevationMark.Add(p);
                }
                if (reg > 1000)
                {
                    elevationMarks.Add(elevationMark);
                    elevationMark = new List<Vector>();
                    reg = 0;
                }
                prev = p;
            }

            writer.DrawPolyline(points, lm);

            if (reg > 300 && elevationMarks.Count == 0)
            {
                elevationMarks.Add(elevationMark);
            }

            foreach(var p in elevationMarks)
            {
                writer.DrawTextPath(p.Take(1).Concat(p.Skip(p.Count - 1)).ToList(), $"{line.Level}", lt);
            }
        }
    }
}
