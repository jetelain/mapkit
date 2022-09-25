using System;
using System.Collections.Generic;
using System.Linq;

namespace MapToolkit.Drawing.MemoryRender
{
    internal class MemDrawContext : IRemapStyle
    {
        private readonly Dictionary<MemDrawStyle, IDrawStyle> styles = new Dictionary<MemDrawStyle, IDrawStyle>();
        private readonly Dictionary<MemDrawIcon, IDrawIcon> icons = new Dictionary<MemDrawIcon, IDrawIcon>();
        private readonly Dictionary<MemDrawTextStyle, IDrawTextStyle> textStyles = new Dictionary<MemDrawTextStyle, IDrawTextStyle>();

        internal MemDrawContext(MemorySurface source, IDrawSurface target)
        {
            Target = target;
            Source = source;
            foreach (var s in source.Styles)
            {
                styles[s] = target.AllocateStyle(Map(s.Fill), s.Pen);
            }
            textStyles = source.TextStyles.ToDictionary(s => s, s => target.AllocateTextStyle(s.FontNames, s.Style, s.Size, Map(s.Fill), s.Pen, s.FillCoverPen, s.TextAnchor));
            foreach (var s in source.Icons)
            {
                icons[s] = target.AllocateIcon(s.Size, w => s.Draw(w, this));
            }
        }

        internal MemDrawContext(MemDrawContext other, IDrawSurface target)
        {
            Target = target;
            Source = other.Source;
            styles = other.styles;
            icons = other.icons;
            textStyles = other.textStyles;
        }

        internal IDrawIcon MapIcon(MemDrawIcon icon)
        {
            return icons[icon];
        }

        private IBrush? Map(IBrush? fill)
        {
            if (fill is VectorBrush vector)
            {
                return new VectorBrush(vector.Width, vector.Height, s => vector.Draw(new TranslateStylesSurface(this, s)));
            }
            return fill;
        }

        public IDrawSurface Target { get; }

        public MemorySurface Source { get; }


        public IDrawStyle MapStyle(MemDrawStyle style)
        {
            return styles[style];
        }

        internal IDrawTextStyle MapTextStyle(MemDrawTextStyle style)
        {
            return textStyles[style];
        }
    }
}