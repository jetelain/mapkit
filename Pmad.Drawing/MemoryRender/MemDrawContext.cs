using System;
using System.Collections.Generic;
using System.Linq;

namespace Pmad.Drawing.MemoryRender
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

            // SVG Prefer pre-allocated styles
            source.Styles.ForEach(s => MapStyle(s));
            source.TextStyles.ForEach(s => MapTextStyle(s));
        }

        internal MemDrawContext(MemDrawContext other, IDrawSurface target)
        {
            Target = target;
            Source = other.Source;
            styles = other.styles;
            icons = other.icons;
            textStyles = other.textStyles;
        }

        private IBrush? Map(IBrush? fill)
        {
            if (fill is VectorBrush vector)
            {
                return new VectorBrush(MapIcon((MemDrawIcon)vector.Icon));
            }
            return fill;
        }

        public IDrawSurface Target { get; }

        public MemorySurface Source { get; }

        public IDrawStyle MapStyle(MemDrawStyle style)
        {
            if (!styles.TryGetValue(style, out var mapped))
            {
                styles.Add(style, mapped = Target.AllocateStyle(Map(style.Fill), style.Pen));
            }
            return mapped;
        }

        public IDrawTextStyle MapTextStyle(MemDrawTextStyle textStyle)
        {
            if (!textStyles.TryGetValue(textStyle, out var mapped))
            {
                textStyles.Add(textStyle, mapped = Target.AllocateTextStyle(textStyle.FontNames, textStyle.Style, textStyle.Size, Map(textStyle.Fill), textStyle.Pen, textStyle.FillCoverPen, textStyle.TextAnchor));
            }
            return mapped;
        }

        public IDrawIcon MapIcon(MemDrawIcon icon)
        {
            if (!icons.TryGetValue(icon, out var mapped))
            {
                icons.Add(icon, mapped = Target.AllocateIcon(icon.Size, w => icon.Draw(w, this)));
            }
            return mapped;
        }

    }
}