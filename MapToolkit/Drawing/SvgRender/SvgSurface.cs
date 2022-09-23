using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace MapToolkit.Drawing.SvgRender
{

    internal sealed class SvgSurface : IDrawSurface, IDisposable
    {
        public const string SvgXmlns = "http://www.w3.org/2000/svg";

        private readonly XmlWriter writer;
        private readonly string? path;
        private readonly StringBuilder sharedStringBuilder = new StringBuilder(1204);
        private int nextPathId = 0;
        private int nextStyleId = 0;
        private int nextBrushId = 0;
        private int nextImageId = 0;

        private bool isWrittingStyle = false;
        private readonly StringBuilder styles = new StringBuilder();

        public SvgSurface(XmlWriter writer, Vector size, string? path = null)
        {
            this.writer = writer;
            this.path = path;
            StartSvg(size);
        }

        public IDrawStyle AllocateStyle(IBrush? fill, Pen? pen)
        {
            var name = TakeStyleId();
            StartClass(name);
            Append(fill, pen);
            EndClass();
            return new SvgStyle(name);
        }

        private string TakeStyleId()
        {
            return "s" + (nextStyleId++).ToString("x");
        }

        private void EndClass()
        {
            styles.AppendLine("}");
            isWrittingStyle = false;
        }

        private void StartClass(string? name)
        {
            isWrittingStyle = true;
            styles.Append(".");
            styles.Append(name);
            styles.AppendLine("{");
        }

        private void Append(IBrush? fill, Pen? stroke)
        {
            Append("fill", Serialize(fill) ?? "none");
            if (stroke != null && stroke.Width > 0 && stroke.Brush != null)
            {
                Append("stroke", Serialize(stroke.Brush));
                Append("stroke-width", stroke.Width.ToString(CultureInfo.InvariantCulture));
                if (stroke.Pattern != null)
                {
                    Append("stroke-dasharray", string.Join(' ', stroke.Pattern.Select(n => n.ToString(CultureInfo.InvariantCulture))));
                }
            }
        }

        public IDrawTextStyle AllocateTextStyle(string[] fontNames, FontStyle style, double size, IBrush? fill, Pen? pen, bool fillCoverPen = false, TextAnchor textAnchor = TextAnchor.CenterLeft)
        {
            var name = TakeStyleId();
            StartClass(name);
            Append("font", FormattableString.Invariant($"{size}pt {string.Join(',', fontNames.Select(f => f.Contains(' ') ? '"' + f + '"' : f))}"));
            Append(fill, fillCoverPen ? null : pen);
            switch (textAnchor)
            {
                case TextAnchor.CenterLeft:
                    Append("dominant-baseline", "middle");
                    break;
                case TextAnchor.CenterRight:
                    Append("dominant-baseline", "middle");
                    Append("text-anchor", "end");
                    break;
                case TextAnchor.TopCenter:
                    Append("dominant-baseline", "auto");
                    Append("text-anchor", "middle");
                    break;
                case TextAnchor.BottomCenter:
                    Append("dominant-baseline", "hanging");
                    Append("text-anchor", "middle");
                    break;
                default:
                    break;
            }
            switch (style)
            {
                case FontStyle.Bold:
                    Append("font-weight", "bold");
                    break;
                case FontStyle.Italic:
                    Append("font-style", "italic");
                    break;
                case FontStyle.BoldItalic:
                    Append("font-weight", "bold");
                    Append("font-style", "italic");
                    break;
            }
            EndClass();
            if (fillCoverPen && pen != null)
            {
                //var bgName = TakeStyleId();
                StartClass(name + ".b");
                Append(null, pen);
                EndClass();
                return new SvgTextStyle(name, name + " b" /*+ bgName*/);
            }
            return new SvgTextStyle(name, null);
        }

        private void StartSvg(Vector size)
        {
            writer.WriteStartElement("svg", SvgXmlns);
            writer.WriteAttributeString("viewBox", FormattableString.Invariant($"0 0 {size.X} {size.Y}"));

            writer.WriteStartElement("rect", SvgXmlns);
            writer.WriteAttributeString("x", "0");
            writer.WriteAttributeString("y", "0");
            writer.WriteAttributeString("width", size.X.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("height", size.X.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("fill", "#fff");
            writer.WriteEndElement();
        }

        private void EndSvg()
        {
            FlushStyles();

            writer.WriteEndElement();
        }

        private void FlushStyles()
        {
            if (!isWrittingStyle && styles.Length > 0)
            {
                writer.WriteStartElement("style", SvgXmlns);
                writer.WriteString(styles.ToString());
                writer.WriteEndElement();
                styles.Clear();
            }
        }

        private void Append(string name, string? value)
        {
            if (value != null)
            {
                styles.Append(name);
                styles.Append(": ");
                styles.Append(value);
                styles.AppendLine(";");
            }
        }

        private string? Serialize(IBrush? fill)
        {
            switch (fill)
            {
                case SolidColorBrush solid:
                    return "#" + solid.Color.ToHex();
                case VectorBrush vector:
                    return Serialize(vector);
            }
            return null;
        }

        private string Serialize(VectorBrush vector)
        {
            var id = "b" + nextBrushId++;
            writer.WriteStartElement("pattern", SvgXmlns);
            writer.WriteAttributeString("id", id);
            writer.WriteAttributeString("height", vector.Height.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("width", vector.Width.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("patternUnits", "userSpaceOnUse");
            vector.Draw(this);
            writer.WriteEndElement();
            return $"url(#{id})";
        }

        public void DrawCircle(Vector center, float radius, IDrawStyle style)
        {
            FlushStyles();
            writer.WriteStartElement("circle", SvgXmlns);
            writer.WriteAttributeString("cx", center.X.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("cy", center.Y.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("r", radius.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("class", ((SvgStyle)style).Name);
            writer.WriteEndElement();
        }

        public void DrawImage(Image image, Vector pos, Vector size, double alpha)
        {
            FlushStyles();
            string href;
            //if ((image.Width > 250 || image.Height > 250) && path != null)
            //{
            //    var target = Path.ChangeExtension(path, ".i"+(nextImageId++)+".png");
            //    image.SaveAsPng(target);
            //    href = Path.GetFileName(target);
            //}
            //else
            //{
                var mem = new MemoryStream();
                image.SaveAsPng(mem);
                href = "data:image/png;base64," + Convert.ToBase64String(mem.ToArray());
            //}
            writer.WriteStartElement("image", SvgXmlns);
            writer.WriteAttributeString("x", Math.Round(pos.X).ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("y", Math.Round(pos.Y).ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("width", size.X.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("height", size.Y.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("opacity", alpha.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("href", href);
            writer.WriteEndElement();
        }

        public void DrawPolygon(IEnumerable<Vector> points, IDrawStyle style)
        {
            FlushStyles();
            writer.WriteStartElement("path", SvgXmlns);
            writer.WriteAttributeString("class", ((SvgStyle)style).Name);
            writer.WriteAttributeString("d", GeneratePath(points, true));
            writer.WriteEndElement();
        }

        public void DrawPolyline(IEnumerable<Vector> points, IDrawStyle style)
        {
            FlushStyles();

            writer.WriteStartElement("path", SvgXmlns);
            writer.WriteAttributeString("class", ((SvgStyle)style).Name);
            writer.WriteAttributeString("d", GeneratePath(points, false));
            writer.WriteEndElement();
        }

        private void KeepSharedStringBuilderLightweight()
        {
            if (sharedStringBuilder.Capacity > 1024 * 1024 * 10)
            {
                sharedStringBuilder.Capacity = 1204;
            }
        }

        private string GeneratePath(IEnumerable<Vector> points, bool closed)
        {
            sharedStringBuilder.Clear();
            KeepSharedStringBuilderLightweight();
            AppendPath(points, closed);
            return sharedStringBuilder.ToString();
        }

        private string GeneratePathWithHoles(IEnumerable<Vector> points, IEnumerable<IEnumerable<Vector>> holes)
        {
            sharedStringBuilder.Clear();
            KeepSharedStringBuilderLightweight();
            AppendPath(points, true);
            foreach(var hole in holes)
            {
                AppendPath(hole, true);
            }
            return sharedStringBuilder.ToString();
        }

        private void AppendPath(IEnumerable<Vector> points, bool closed)
        {
            Vector previous = Vector.Zero;
            bool first = true;
            foreach (var p in points)
            {
                var px = new Vector(Math.Round(p.X), Math.Round(p.Y));
                if (first)
                {
                    if (sharedStringBuilder.Length > 0)
                    {
                        sharedStringBuilder.Append(' ');
                    }
                    sharedStringBuilder.Append(FormattableString.Invariant($"M{px.X},{px.Y}"));
                    first = false;
                }
                else
                {
                    sharedStringBuilder.Append(FormattableString.Invariant($" l{px.X - previous.X},{px.Y - previous.Y}"));
                }
                previous = px;
            }
            if (closed)
            {
                sharedStringBuilder.Append(" z");
            }
        }

        public void DrawTextPath(IEnumerable<Vector> points, string text, IDrawTextStyle style)
        {
            var sstyle = (SvgTextStyle)style;
            FlushStyles();
            var id = "p" + nextPathId++;

            writer.WriteStartElement("defs", SvgXmlns);
            writer.WriteStartElement("path", SvgXmlns);
            writer.WriteAttributeString("id", id);
            writer.WriteAttributeString("d", GeneratePath(points, false));
            writer.WriteEndElement();
            writer.WriteEndElement();

            if (!string.IsNullOrEmpty(sstyle.BgName))
            {
                writer.WriteStartElement("text", SvgXmlns);
                writer.WriteAttributeString("class", sstyle.BgName);
                writer.WriteStartElement("textPath", SvgXmlns);
                writer.WriteAttributeString("href", "#" + id);
                writer.WriteString(text);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            writer.WriteStartElement("text", SvgXmlns);
            writer.WriteAttributeString("class", sstyle.Name);
            writer.WriteStartElement("textPath", SvgXmlns);
            writer.WriteAttributeString("href", "#" + id);
            writer.WriteString(text);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public void DrawText(Vector point, string text, IDrawTextStyle style)
        {
            var sstyle = (SvgTextStyle)style;
            FlushStyles();

            if (!string.IsNullOrEmpty(sstyle.BgName))
            {
                writer.WriteStartElement("text", SvgXmlns);
                writer.WriteAttributeString("class", sstyle.BgName);
                writer.WriteAttributeString("x", Math.Round(point.X).ToString(CultureInfo.InvariantCulture));
                writer.WriteAttributeString("y", Math.Round(point.Y).ToString(CultureInfo.InvariantCulture));
                writer.WriteString(text);
                writer.WriteEndElement();
            }

            writer.WriteStartElement("text", SvgXmlns);
            writer.WriteAttributeString("class", sstyle.Name);
            writer.WriteAttributeString("x", Math.Round(point.X).ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("y", Math.Round(point.Y).ToString(CultureInfo.InvariantCulture));
            writer.WriteString(text);
            writer.WriteEndElement();
        }

        public void Dispose()
        {
            EndSvg();
            writer.Dispose();
        }

        public void DrawPolygon(IEnumerable<Vector> contour, IEnumerable<IEnumerable<Vector>> holes, IDrawStyle style)
        {
            FlushStyles();

            writer.WriteStartElement("path", SvgXmlns);
            writer.WriteAttributeString("class", ((SvgStyle)style).Name);
            writer.WriteAttributeString("d", GeneratePathWithHoles(contour, holes));
            writer.WriteEndElement();
        }
    }
}
