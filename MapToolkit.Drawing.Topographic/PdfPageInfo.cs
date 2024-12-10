
using Pmad.Geometry;

namespace Pmad.Cartography.Drawing.Topographic
{
    public class PdfPageInfo
    {
        public PdfPageInfo(Coordinates start, Coordinates end, Vector2D mapTopLeft)
        {
            Start = start;
            End = end;
            MapTopLeft = mapTopLeft;
        }

        public Coordinates Start { get; }

        public Coordinates End { get; }

        public Vector2D MapTopLeft { get; }
    }
}