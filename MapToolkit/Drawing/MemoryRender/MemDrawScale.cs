using System;
using System.Collections.Generic;
using System.Linq;

namespace MapToolkit.Drawing.MemoryRender
{
    internal class MemDrawScale : IRemapStyle
    {
        public double Scale { get; }

        private readonly Dictionary<MemDrawTextStyle, MemDrawTextStyle> textStyles = new Dictionary<MemDrawTextStyle, MemDrawTextStyle>();
        private readonly Dictionary<MemDrawStyle, MemDrawStyle> styles = new Dictionary<MemDrawStyle, MemDrawStyle>();
        private readonly Dictionary<MemDrawIcon, MemDrawIcon> icons = new Dictionary<MemDrawIcon, MemDrawIcon>();
        private readonly MemorySurface source;
        private readonly double penScale;

        internal MemDrawScale(MemorySurface source, double scale, double penScale)
        {
            this.source = source;
            this.penScale = penScale;
            Scale = scale;
        }

        private Pen? MapPen(Pen? pen)
        {
            if (pen != null)
            {
                return new Pen(pen.Brush, pen.Width * penScale, pen.Pattern);
            }
            return null;
        }

        private IBrush? MapBrush(IBrush? fill)
        {
            if (fill is VectorBrush vector)
            {
                return new VectorBrush(MapIcon((MemDrawIcon)vector.Icon));
            }
            return fill;
        }

        public IEnumerable<MemDrawTextStyle> TextStyles => textStyles.Values;

        public IEnumerable<MemDrawIcon> Icons => icons.Values;

        public IEnumerable<MemDrawStyle> Styles => styles.Values;

        internal MemDrawStyle MapStyle(MemDrawStyle style)
        {
            if (!styles.TryGetValue(style, out var mapped))
            {
                styles.Add(style, mapped = new MemDrawStyle(MapBrush(style.Fill), MapPen(style.Pen)));
            }
            return mapped;
        }

        internal MemDrawTextStyle MapTextStyle(MemDrawTextStyle textStyle)
        {
            if (!textStyles.TryGetValue(textStyle, out var mapped))
            {
                textStyles.Add(textStyle, mapped = new MemDrawTextStyle(textStyle.FontNames, textStyle.Style, textStyle.Size * Scale, MapBrush(textStyle.Fill), textStyle.Pen, textStyle.FillCoverPen, textStyle.TextAnchor));
            }
            return mapped;
        }

        internal MemDrawIcon MapIcon(MemDrawIcon icon)
        {
            if (!icons.TryGetValue(icon, out var mapped))
            {
                icons.Add(icon, mapped = icon.Scale(this));
            }
            return mapped;
        }

        public MemorySurface ToMemorySurface(double lengthSquared = 9)
        {
            var target = new MemorySurface();
            target.Operations.AddRange(source.Operations.SelectMany(o => o.Scale(this).Simplify(lengthSquared)));
            target.Styles.AddRange(Styles);
            target.TextStyles.AddRange(TextStyles);
            target.Icons.AddRange(Icons);
            return target;
        }

        IDrawStyle IRemapStyle.MapStyle(MemDrawStyle style)

        {
            return MapStyle(style);
        }
    }
}