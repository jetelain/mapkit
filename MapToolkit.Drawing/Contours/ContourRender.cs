using MapToolkit.Contours;
using MapToolkit.Projections;
using SixLabors.ImageSharp;

namespace MapToolkit.Drawing.Contours
{
    public class ContourRender
    {
        private readonly IDrawSurface writer;
        private readonly IContourRenderStyle style;

        public ContourRender(IDrawSurface writer) 
            : this(writer, new ContourRenderStyle(writer))
        {

        }

        public ContourRender(IDrawSurface writer, IContourRenderStyle style)
        {
            this.writer = writer;
            this.style = style;
        }

        public void Render(ContourGraph graph, IProjectionArea projection, Image? hillshade, IProgress<int>? progress = null)
        {
            if (hillshade != null)
            {
                writer.DrawImage(hillshade, Vector.Zero, projection.Size, 0.5);
            }
            var masters = new List<ContourLine>();
            var done = 0;
            foreach (var line in graph.Lines)
            {
                if (line.Level % 50 == 0)
                {
                    masters.Add(line);
                }
                else
                {
                    RenderLine( projection, line);
                    done++;
                    if (done % 100 == 0)
                    {
                        progress?.Report(done);
                    }
                }
            }
            foreach(var line in masters)
            {
                RenderMajorLine(projection, line);
                done++; 
                if (done % 100 == 0)
                {
                    progress?.Report(done);
                }
            }
            progress?.Report(done);
        }

        public void RenderSimpler(ContourGraph graph, IProjectionArea projection, Image? hillshade)
        {
            if (hillshade != null)
            {
                writer.DrawImage(hillshade, Vector.Zero, projection.Size, 0.5);
            }
            foreach (var line in graph.Lines)
            {
                if (line.Level % 50 == 0)
                {
                    RenderLine(projection, line);
                }
            }
        }

        private void RenderLine(IProjectionArea projection, ContourLine line)
        {
            writer.DrawPolyline(line.Points.Select(p => projection.Project(p)), style.MinorContourLine);
        }

        private void RenderMajorLine(IProjectionArea projection, ContourLine line)
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
                    var l = Math.Sqrt(Math.Pow(prev.X - p.X, 2) + Math.Pow(prev.Y - p.Y, 2)); // It's pixels
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

            writer.DrawPolyline(points, style.MajorContourLine);

            if (reg > 300 && elevationMarks.Count == 0)
            {
                elevationMarks.Add(elevationMark);
            }

            foreach(var p in elevationMarks)
            {
                writer.DrawTextPath(p.Take(1).Concat(p.Skip(p.Count - 1)).ToList(), $"{line.Level}", style.MajorContourText);
            }
        }
    }
}
