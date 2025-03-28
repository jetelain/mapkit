﻿using Pmad.Drawing;
using SixLabors.ImageSharp;

namespace Pmad.Cartography.Drawing.Contours
{
    public class ContourRenderStyle : IContourRenderStyle
    {
        public ContourRenderStyle(IDrawSurface writer)
        {
            MajorContourText = writer.AllocateTextStyle(
                new[] { "Calibri", "sans-serif" },
                SixLabors.Fonts.FontStyle.Bold,
                18,
                new SolidColorBrush(Color.ParseHex("B29A94")),
                new Pen(new SolidColorBrush(Color.ParseHex("FFFFFFCC")), 3),
                true);
            MinorContourLine = writer.AllocatePenStyle("D4C5BF", 2);
            MajorContourLine = writer.AllocatePenStyle("B29A94", 2);
        }

        public IDrawTextStyle MajorContourText { get; }

        public IDrawStyle MajorContourLine { get; }

        public IDrawStyle MinorContourLine { get; }
    }
}
