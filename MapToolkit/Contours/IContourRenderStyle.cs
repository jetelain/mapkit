using System;
using System.Collections.Generic;
using System.Text;
using SimpleDEM.Drawing;

namespace SimpleDEM.Contours
{
    public interface IContourRenderStyle
    {
        IDrawTextStyle MajorContourText { get; }

        IDrawStyle MajorContourLine { get; }

        IDrawStyle MinorContourLine { get; }
    }
}
