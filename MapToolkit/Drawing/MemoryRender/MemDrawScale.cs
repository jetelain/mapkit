using System;
using System.Collections.Generic;
using System.Linq;

namespace MapToolkit.Drawing.MemoryRender
{
    internal class MemDrawScale : IRemapStyle
    {
        public double Scale { get; }

        private readonly Dictionary<MemDrawTextStyle, MemDrawTextStyle> textStyles;
        private readonly Dictionary<MemDrawStyle, MemDrawStyle> styles = new Dictionary<MemDrawStyle, MemDrawStyle>();
        private readonly Dictionary<MemDrawIcon, MemDrawIcon> icons = new Dictionary<MemDrawIcon, MemDrawIcon>();
        private readonly MemorySurface source;

        internal MemDrawScale(MemorySurface source, double scale, double penScale)
        {
            this.source = source;
            Scale = scale;
            foreach (var s in source.Styles)
            {
                styles[s] = new MemDrawStyle(Map(s.Fill), Map(s.Pen, penScale));
            }
            textStyles = source.TextStyles.ToDictionary(s => s, s => new MemDrawTextStyle(s.FontNames, s.Style, s.Size * scale, Map(s.Fill), s.Pen, s.FillCoverPen, s.TextAnchor));
            foreach (var i in source.Icons)
            {
                icons[i] = i.Scale(this);
            }
        }

        private Pen? Map(Pen? pen, double penScale)
        {
            if (pen != null)
            {
                return new Pen(pen.Brush, pen.Width * penScale, pen.Pattern);
            }
            return null;
        }

        private IBrush? Map(IBrush? fill)
        {
            if (fill is VectorBrush vector)
            {
                return new VectorBrush(vector.Width, vector.Height, s => vector.Draw(new TranslateStylesSurface(this, s)));
            }
            return fill;
        }

        public IEnumerable<MemDrawTextStyle> TextStyles => textStyles.Values;
        public IEnumerable<MemDrawIcon> Icons => icons.Values;

        public IEnumerable<MemDrawStyle> Styles => styles.Values;
        internal MemDrawStyle MapStyle(MemDrawStyle style)
        {
            return styles[style];
        }

        internal MemDrawTextStyle MapTextStyle(MemDrawTextStyle style)
        {
            return textStyles[style];
        }

        internal MemDrawIcon MapIcon(MemDrawIcon style)
        {
            return icons[style];
        }

        public MemorySurface ToMemorySurface(double lengthSquared = 9)
        {
            var target = new MemorySurface();
            target.Styles.AddRange(Styles);
            target.TextStyles.AddRange(TextStyles);
            target.Icons.AddRange(Icons);
            target.Operations.AddRange(source.Operations.SelectMany(o => o.Scale(this).Simplify(lengthSquared)));
            return target;
        }
        IDrawStyle IRemapStyle.MapStyle(MemDrawStyle style)

        {
            return MapStyle(style);
        }
    }
}