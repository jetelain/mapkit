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

            writer.WriteStartElement("svg", SvgXmlns);
            writer.WriteAttributeString("viewBox", FormattableString.Invariant($"0 0 {projection.Size.X} {projection.Size.Y}"));

            foreach (var line in graph.Lines)
            {
                writer.WriteStartElement("polyline", SvgXmlns);
                writer.WriteAttributeString("fill", "none");
                writer.WriteAttributeString("stroke", "#000");

                if (line.Level % 50 == 0)
                {
                    writer.WriteAttributeString("stroke-width", "2");
                }

                var sb = new StringBuilder();
                foreach (var point in line.Points)
                {
                    var p = projection.Project(point);
                    sb.Append(FormattableString.Invariant($"{p.X} {p.Y} "));
                }
                writer.WriteAttributeString("points", sb.ToString());
                writer.WriteEndElement();
            }


            writer.WriteEndElement();
        }

    }
}
