using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Pmad.Cartography.Drawing.SvgRender
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
        private readonly int rounding = 1;
        private bool isWrittingStyle = false;
        private readonly StringBuilder styles = new StringBuilder();
        private readonly string stylePrefix;

        public SvgSurface(XmlWriter writer, Vector size, string? path = null, string stylePrefix = "")
        {
            this.writer = writer;
            this.path = path;
            this.stylePrefix = stylePrefix;
            StartSvg(size);
        }

        public IDrawIcon AllocateIcon(Vector size, Action<IDrawSurface> draw)
        {
            return new SvgIcon(size, draw);
        }

        private readonly Dictionary<Tuple<IBrush?, Pen?>, SvgStyle> xstyles = new Dictionary<Tuple<IBrush?, Pen?>, SvgStyle>();

        public IDrawStyle AllocateStyle(IBrush? fill, Pen? pen)
        {
            var key = new Tuple<IBrush?, Pen?>(fill, pen);
            if (!xstyles.TryGetValue(key, out var svgStyle))
            {
                var name = TakeStyleId();
                StartClass(name);
                Append(fill, pen);
                EndClass();
                xstyles.Add(key, svgStyle = new SvgStyle(name));
            }
            return svgStyle;
        }

        private string TakeStyleId()
        {
            return stylePrefix + "s" + (nextStyleId++).ToString("x");
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
            Append("font", FormattableString.Invariant($"{size}px {string.Join(',', fontNames.Select(f => f.Contains(' ') ? '"' + f + '"' : f))}"));
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
                    Append("dominant-baseline", "hanging");
                    Append("text-anchor", "middle");
                    break;
                case TextAnchor.BottomCenter:
                    Append("text-anchor", "middle");
                    break;
                case TextAnchor.TopLeft:
                    Append("dominant-baseline", "hanging");
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
                ////var bgName = TakeStyleId();
                //StartClass(name + ".b");
                //Append(null, pen);
                //EndClass();
                //return new SvgTextStyle(name, name + " b" /*+ bgName*/);
                return new SvgTextStyle(name, name + " " + ((SvgStyle)AllocateStyle(null, pen)).Name);
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
            var icon = (SvgIcon)vector.Icon;
            var id = "b" + nextBrushId++;
            writer.WriteStartElement("pattern", SvgXmlns);
            writer.WriteAttributeString("id", id);
            writer.WriteAttributeString("height", icon.Size.Y.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("width", icon.Size.X.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("patternUnits", "userSpaceOnUse");
            icon.Draw(this);
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
            writer.WriteAttributeString("x", ToString(pos.X));
            writer.WriteAttributeString("y", ToString(pos.Y));
            writer.WriteAttributeString("width", size.X.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("height", size.Y.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("opacity", alpha.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("href", href);
            writer.WriteEndElement();
        }

        private string ToString(double value)
        {
            var str = value.ToString("0.0", CultureInfo.InvariantCulture);
            if (str.EndsWith(".0"))
            {
                return str.Substring(0, str.Length - 2);
            }
            return str;
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

        private string GeneratePathWithHoles(IEnumerable<Vector[]> paths)
        {
            sharedStringBuilder.Clear();
            KeepSharedStringBuilderLightweight();
            foreach(var hole in paths)
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
                var px = new Vector(Math.Round(p.X, rounding), Math.Round(p.Y, rounding));
                if (first)
                {
                    if (sharedStringBuilder.Length > 0)
                    {
                        sharedStringBuilder.Append(' ');
                    }
                    sharedStringBuilder.Append(FormattableString.Invariant($"M{ToString(px.X)},{ToString(px.Y)}"));
                    first = false;
                }
                else
                {
                    sharedStringBuilder.Append(FormattableString.Invariant($" l{ToString(px.X - previous.X)},{ToString(px.Y - previous.Y)}"));
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
                writer.WriteAttributeString("x", ToString(point.X));
                writer.WriteAttributeString("y", ToString(point.Y));
                writer.WriteString(text);
                writer.WriteEndElement();
            }

            writer.WriteStartElement("text", SvgXmlns);
            writer.WriteAttributeString("class", sstyle.Name);
            writer.WriteAttributeString("x", ToString(point.X));
            writer.WriteAttributeString("y", ToString(point.Y));
            writer.WriteString(text);
            writer.WriteEndElement();
        }

        public void Dispose()
        {
            EndSvg();
            writer.Dispose();
        }

        public void DrawPolygon(IEnumerable<Vector[]> paths, IDrawStyle style)
        {
            FlushStyles();

            writer.WriteStartElement("path", SvgXmlns);
            writer.WriteAttributeString("class", ((SvgStyle)style).Name);
            writer.WriteAttributeString("d", GeneratePathWithHoles(paths));
            writer.WriteEndElement();
        }

        public void DrawArc(Vector center, float radius, float startAngle, float sweepAngle, IDrawStyle style)
        {
            // Source : https://stackoverflow.com/questions/5736398/how-to-calculate-the-svg-path-for-an-arc-of-a-circle
            var start = PolarToCartesian(center, radius, startAngle + sweepAngle);
            var end = PolarToCartesian(center, radius, startAngle);
            var largeArcFlag = sweepAngle <= 180 ? "0" : "1";
            writer.WriteStartElement("path", SvgXmlns);
            writer.WriteAttributeString("class", ((SvgStyle)style).Name);
            writer.WriteAttributeString("d", FormattableString.Invariant($"M {start.X} {start.Y} A {radius} {radius} 0 {largeArcFlag} 0 {end.X} {end.Y}"));
            writer.WriteEndElement();
        }

        private static Vector PolarToCartesian(Vector center, float radius, float angleInDegrees)
        {
            var angleInRadians = angleInDegrees * Math.PI / 180.0;
            return new Vector(center.X + (radius * Math.Cos(angleInRadians)), center.Y + (radius * Math.Sin(angleInRadians)));
        }

        public void DrawIcon(Vector center, IDrawIcon icon)
        {
            var sicon = (SvgIcon)icon;
            var top = center - (sicon.Size / 2);
            sicon.Draw(new TranslateDraw(this, top.X, top.Y));
        }

        public void DrawRoundedRectangle(Vector topLeft, Vector bottomRight, IDrawStyle style, float radius)
        {
            FlushStyles();
            writer.WriteStartElement("rect", SvgXmlns);
            writer.WriteAttributeString("x", ToString(topLeft.X));
            writer.WriteAttributeString("y", ToString(topLeft.Y));
            writer.WriteAttributeString("width", ToString(bottomRight.X - topLeft.X));
            writer.WriteAttributeString("height", ToString(bottomRight.Y - topLeft.Y));
            writer.WriteAttributeString("rx", ToString(radius));
            writer.WriteAttributeString("class", ((SvgStyle)style).Name);
            writer.WriteEndElement();
        }
    }
}
