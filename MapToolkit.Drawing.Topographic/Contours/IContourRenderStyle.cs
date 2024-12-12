using Pmad.Drawing;

namespace Pmad.Cartography.Drawing.Contours
{
    public interface IContourRenderStyle
    {
        IDrawTextStyle MajorContourText { get; }

        IDrawStyle MajorContourLine { get; }

        IDrawStyle MinorContourLine { get; }
    }
}
