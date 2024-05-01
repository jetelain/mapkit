using System;
using System.Collections.Generic;
using System.Text;
using MapToolkit.Drawing;

namespace MapToolkit.Drawing.Contours
{
    public interface IContourRenderStyle
    {
        IDrawTextStyle MajorContourText { get; }

        IDrawStyle MajorContourLine { get; }

        IDrawStyle MinorContourLine { get; }
    }
}
