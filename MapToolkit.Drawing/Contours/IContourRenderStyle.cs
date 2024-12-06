using System;
using System.Collections.Generic;
using System.Text;
using Pmad.Cartography.Drawing;

namespace Pmad.Cartography.Drawing.Contours
{
    public interface IContourRenderStyle
    {
        IDrawTextStyle MajorContourText { get; }

        IDrawStyle MajorContourLine { get; }

        IDrawStyle MinorContourLine { get; }
    }
}
