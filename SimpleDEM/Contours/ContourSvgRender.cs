using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using SimpleDEM.Projections;

namespace SimpleDEM.Contours
{
    public class ContourSvgRender
    {
        private const string SvgXmlns = "http://www.w3.org/2000/svg";

        public void WriteSVG(TextWriter writer, ContourGraph graph, IProjectionArea projection)
        {
            using (var xml = XmlWriter.Create(writer))
            {
                WriteSVG(xml, graph, projection);
            }
        }

        public void WriteSVG(XmlWriter writer, ContourGraph graph, IProjectionArea projection)
        {
            var minLat = graph.Lines.SelectMany(l => l.Points).Min(p => p.Latitude);
            var minLon = graph.Lines.SelectMany(l => l.Points).Min(p => p.Longitude);
            var maxLat = graph.Lines.SelectMany(l => l.Points).Max(p => p.Latitude);
            var maxLon = graph.Lines.SelectMany(l => l.Points).Max(p => p.Longitude);

            /*
<filter id="blur">
  <feDropShadow dx="0" dy="0" stdDeviation="3" flood-color="#fff" />
</filter>
<style>
.lt { font: 18px "Gill Sans Extrabold",sans-serif; font-weight:bolder ; fill:#B29A94; filter:url(#blur);  }
.ls { fill: none; stroke: #D4C5BF; stroke-width: 2; }
.lm { fill: none; stroke: #B29A94; stroke-width: 2; }
</style>
            */

            writer.WriteStartElement("svg", SvgXmlns);
            writer.WriteAttributeString("viewBox", FormattableString.Invariant($"0 0 {projection.Size.X} {projection.Size.Y}"));

            writer.WriteString(Environment.NewLine);

            writer.WriteStartElement("filter", SvgXmlns);
            writer.WriteAttributeString("id", "blur");
            writer.WriteStartElement("feDropShadow", SvgXmlns);
            writer.WriteAttributeString("dx", "0");
            writer.WriteAttributeString("dy", "0");
            writer.WriteAttributeString("stdDeviation", "3");
            writer.WriteAttributeString("flood-color", "#fff");
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.WriteString(Environment.NewLine);
            writer.WriteStartElement("style", SvgXmlns);
            writer.WriteString(@"
.lt {font:18px ""Gill Sans Extrabold"",sans-serif;font-weight:bolder;fill:#B29A94;filter:url(#blur);}
.ls {fill:none;stroke:#D4C5BF;stroke-width:2;}
.lm {fill:none;stroke:#B29A94;stroke-width:2;}
");
            writer.WriteEndElement();
            writer.WriteString(Environment.NewLine);
            int id = 0;

            foreach (var line in graph.Lines)
            {
                if (line.Level % 50 == 0)
                {
                    RenderMasterLine(writer, projection, line, id);
                }
                else
                {
                    RenderLine(writer, projection, line);
                }
                id++;
                writer.WriteString(Environment.NewLine);
            }


            writer.WriteEndElement();
        }

        private static void RenderLine(XmlWriter writer, IProjectionArea projection, ContourLine line)
        {
            writer.WriteStartElement("polyline", SvgXmlns);
            writer.WriteAttributeString("class", "ls");
            var sb = new StringBuilder();
            foreach (var point in line.Points)
            {
                var p = projection.Project(point);
                sb.Append(FormattableString.Invariant($"{p.X} {p.Y} "));
            }
            writer.WriteAttributeString("points", sb.ToString());
            writer.WriteEndElement();
        }

        private static void RenderMasterLine(XmlWriter writer, IProjectionArea projection, ContourLine line, int id)
        {
            writer.WriteStartElement("polyline", SvgXmlns);
            writer.WriteAttributeString("class", "lm");


            var elevationMarks = new List<List<Vector>>();
            var elevationMark = new List<Vector>();

            var len = 0.0;
            var reg = 0.0;
            Vector? prev = null;
            var sb = new StringBuilder();
            foreach (var point in line.Points)
            {
                var p = projection.Project(point);
                sb.Append(FormattableString.Invariant($"{p.X} {p.Y} "));

                if (prev != null)
                {
                    var l = Math.Sqrt(Math.Pow(prev.X - p.X, 2) + Math.Pow(prev.Y - p.Y, 2));
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
            writer.WriteAttributeString("points", sb.ToString());
            writer.WriteEndElement();

            if (reg > 300 && elevationMarks.Count == 0)
            {
                elevationMarks.Add(elevationMark);
            }

            int subid = 0;

            foreach(var p in elevationMarks)
            {
                writer.WriteStartElement("defs", SvgXmlns);
                writer.WriteStartElement("path", SvgXmlns);
                writer.WriteAttributeString("id", "p" + id + "l" + subid);

                if (p[0].X > p[p.Count - 1].X)
                {
                    p.Reverse();
                }

                GeneratePath(sb, p.Take(1).Concat(p.Skip(p.Count-1)).ToList());
                writer.WriteAttributeString("d", sb.ToString());
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteStartElement("text", SvgXmlns);
                writer.WriteAttributeString("class", "lt");
                writer.WriteStartElement("textPath", SvgXmlns);
                writer.WriteAttributeString("href", "#p" + id + "l" + subid);

                writer.WriteString($"{line.Level}");
                writer.WriteEndElement();
                writer.WriteEndElement();

                subid++;
            }
            /*  <path id="MyPath" fill="none" stroke="red"
        d="M10,90 Q90,90 90,45 Q90,10 50,10 Q10,10 10,40 Q10,70 45,70 Q70,70 75,50" />
  <!-- </defs> -->

  <text>
    <textPath href="#MyPath">
      Quick brown fox jumps over the lazy dog.
    </textPath>
  </text>*/
            /*  
             *  
<path id="MyPath" fill="none" stroke="red"
        d="M10,90 Q90,90 90,45 Q90,10 50,10 Q10,10 10,40 Q10,70 45,70 Q70,70 75,50" />
  <!-- </defs> -->

  <text>
    <textPath href="#MyPath">
      Quick brown fox jumps over the lazy dog.
    </textPath>
  </text>
            
             
             */

        }

        private static void GeneratePath(StringBuilder sb, IReadOnlyList<Vector> points)
        {
            sb.Clear();
            sb.Append(FormattableString.Invariant($"M{points[0].X},{points[0].Y}"));
            foreach (var px in points.Skip(1))
            {
                sb.Append(FormattableString.Invariant($" L{px.X},{px.Y}"));
            }
        }

        private static double Cap(double v)
        {
            return ((v + 90) % 180) - 90;
        }
    }
}
