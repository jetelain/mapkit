using Pmad.Geometry;
using SixLabors.ImageSharp;

namespace Pmad.Cartography.Drawing.Topographic.Test
{
    public class TopoMapRenderTest
    {
        [Fact]
        public void BridgeLimit()
        {
            var result = Render.ToSvgString(new Vector2D(100, 100), draw =>
            {
                TopoMapRender.BridgeLimit(draw, new Vector2D(20, 20), new Vector2D(80, 80), draw.AllocatePenStyle(Color.Black, 1));
            });

            Assert.Contains("<path class=\"s0\" d=\"M23.2,23.2 l-10,0\" /><path class=\"s0\" d=\"M23.2,23.2 l0,-10\" />", result);
        }

        [Fact]
        public void DrawRailway()
        {
            var result = Render.ToSvgString(new Vector2D(100, 100), draw =>
            {
                TopoMapRender.DrawRailway(draw, draw.AllocatePenStyle(Color.Black, 1), [ new Vector2D(10,10), new Vector2D(50,50), new Vector2D(90,90) ]);
            });

            Assert.Contains("<path class=\"s0\" d=\"M10,10 l40,40 l40,40\" /><path class=\"s0\" d=\"M11.8,15.3 l3.5,-3.5\" /><path class=\"s0\" d=\"M64.8,68.3 l3.5,-3.5\" />", result);
        }
    }
}
