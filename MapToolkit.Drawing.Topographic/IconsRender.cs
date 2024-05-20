using MapToolkit.Drawing;
using SixLabors.ImageSharp;

namespace MapToolkit.Drawing.Topographic
{
    internal class IconsRender
    {
        internal static IDrawIcon Dot(IDrawSurface w)
        {
            var style = w.AllocateBrushStyle(Color.Black);
            return w.AllocateIcon(new Vector(5, 5), (target) =>
            {
                target.DrawCircle(new Vector(2.5, 2.5), 2, style);
            });
        }

        internal static IDrawIcon Hospital(IDrawSurface w)
        {
            var border = w.AllocateStyle("FFFFFF", "FF0033");
            var cross = w.AllocatePenStyle("FF0033", 2);
            return w.AllocateIcon(new Vector(12, 12), (target) =>
            {
                target.DrawRectangle(new Vector(0, 0), new Vector(11, 11), border);
                target.DrawPolyline(new[] { new Vector(0, 5.5), new Vector(11, 5.5) }, cross);
                target.DrawPolyline(new[] { new Vector(5.5, 0), new Vector(5.5, 11) }, cross);
            });
        }

        internal static IDrawIcon WaterTower(IDrawSurface w)
        {
            var style = w.AllocateBrushStyle("0080FF");
            return w.AllocateIcon(new Vector(13, 13), (target) => target.DrawCircle(new Vector(6, 6), 6, style));
        }

        internal static IDrawIcon TechnicalTower(IDrawSurface w)
        {
            var style = w.AllocateStyle(Color.White, Color.Black, 1);
            var stylea = w.AllocatePenStyle(Color.Black, 1);
            return w.AllocateIcon(new Vector(13, 13), (target) =>
            {
                target.DrawCircle(new Vector(6, 6), 6, style);
                target.DrawPolyline(new[] { new Vector(1.76, 1.76), new Vector(10.24, 10.24) }, stylea);
                target.DrawPolyline(new[] { new Vector(10.24, 1.76), new Vector(1.76, 10.24) }, stylea);
            });
        }
        internal static IDrawIcon Transmitter(IDrawSurface w)
        {
            var full = w.AllocateBrushStyle(Color.White);
            var center = w.AllocateStyle(Color.Black, Color.Black, 1);
            var line = w.AllocatePenStyle(Color.Black, 2);

            return w.AllocateIcon(new Vector(13, 13), (target) =>
            {
                var c = new Vector(6, 6);
                target.DrawCircle(c, 6, full);
                target.DrawCircle(c, 3, center);
                target.DrawArc(c, 6, 50, 70, line);
                target.DrawArc(c, 6, 140, 70, line);
                target.DrawArc(c, 6, 230, 70, line);
                target.DrawArc(c, 6, 320, 70, line);
            });
        }

        internal static IDrawIcon WindTurbine(IDrawSurface w)
        {
            var style = w.AllocatePenStyle(Color.Black, 2);
            var stylea = w.AllocatePenStyle(Color.Black, 1);
            var stylec = w.AllocateBrushStyle(Color.Black);
            return w.AllocateIcon(new Vector(32, 32), (target) => WindTurbine(target, style, stylea, stylec));
        }

        internal static void WindTurbine(IDrawSurface w, IDrawStyle style, IDrawStyle stylea, IDrawStyle stylec)
        {
            var center = new Vector(16, 16);
            w.DrawPolyline(
                new[]
                {
                    center + new Vector( 0.9961946 * 2.5, -0.0871557 * 2.5),
                    center + new Vector( 0.6427876 *  15,  0.7660444 *  15),
                    center + new Vector( 0.5000000 *  15,  0.8660254 *  15),
                    center + new Vector(-0.4226182 * 2.5,  0.9063077 * 2.5),
                    center + new Vector(-0.9848077 *  15,  0.1736481 *  15),
                    center + new Vector(-1.0       *  15,  0         *  15),
                    center + new Vector(-0.5735764 * 2.5, -0.8191520 * 2.5),
                    center + new Vector( 0.3420201 *  15, -0.9396926 *  15),
                    center + new Vector( 0.5000000 *  15, -0.8660254 *  15),
                    center + new Vector( 0.9961946 * 2.5, -0.0871557 * 2.5)
                }, style);
            w.DrawArc(center, 10, 75, 80, stylea);
            w.DrawArc(center, 15, 70, 90, stylea);
            w.DrawArc(center, 10, 195, 80, stylea);
            w.DrawArc(center, 15, 190, 90, stylea);
            w.DrawArc(center, 10, -45, 80, stylea);
            w.DrawArc(center, 15, -50, 90, stylea);
            w.DrawCircle(center, 1, stylec);
        }
    }
}
