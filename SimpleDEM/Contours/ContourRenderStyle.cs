using SimpleDEM.Drawing;
using SixLabors.ImageSharp;

namespace SimpleDEM.Contours
{
    public class ContourRenderStyle : IContourRenderStyle
    {
        public ContourRenderStyle(IDrawSurface writer)
        {
            MajorContourText = writer.AllocateTextStyle(
                new[] { "Calibri", "sans-serif" },
                18,
                new SolidColorBrush(Color.ParseHex("B29A94")),
                new Pen(new SolidColorBrush(Color.ParseHex("FFFFFFCC")), 3),
                true,
                "lt");

            MinorContourLine = writer.AllocateStyle(null, new Pen(new SolidColorBrush(Color.ParseHex("D4C5BF")), 2), "ls");

            MajorContourLine = writer.AllocateStyle(null, new Pen(new SolidColorBrush(Color.ParseHex("B29A94")), 2), "lm");
        }

        public IDrawTextStyle MajorContourText { get; }

        public IDrawStyle MajorContourLine { get; }

        public IDrawStyle MinorContourLine { get; }
    }
}
