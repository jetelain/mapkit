using System;
using GeoJSON.Text.Geometry;
using Pmad.Geometry;

namespace MapToolkit.Projections
{
    public class WebMercatorArea : IProjectionArea
    {
        private readonly int rounding;
        private readonly int halfFullSize;
        private readonly double dX;
        private readonly double dY;

        public WebMercatorArea(int fullSize, int rounding = -1)
        {
            this.rounding = rounding;
            halfFullSize = fullSize / 2;
            Size = new Vector(fullSize, fullSize);
        }

        public WebMercatorArea(int fullSize, double dX, double dY, Vector size, int rounding = 0)
        {
            this.rounding = rounding;
            halfFullSize = fullSize / 2;
            Size = size;
            this.dX = dX;
            this.dY = dY;
        }

        public Vector Min => Vector.Zero;

        public Vector Size { get; }

        public double Scale => 1;

        public Vector Project(CoordinatesS point)
        {
            var y = halfFullSize * (point.Longitude + 180) / 180;
            var x = halfFullSize * (Math.PI - Math.Log(Math.Tan((point.Latitude + 90) * MathConstants.PIDiv180 / 2))) / Math.PI;
            if (rounding != -1)
            {
                return new Vector(Math.Round(x - dX, rounding), Math.Round(y - dY, rounding));
            }
            return new Vector(x - dX, y - dY);
        }

        public Vector Project(Vector2D point)
        {
            var y = halfFullSize * (point.Longitude() + 180) / 180;
            var x = halfFullSize * (Math.PI - Math.Log(Math.Tan((point.Latitude() + 90) * MathConstants.PIDiv180 / 2))) / Math.PI;
            if (rounding != -1)
            {
                return new Vector(Math.Round(x - dX, rounding), Math.Round(y - dY, rounding));
            }
            return new Vector(x - dX, y - dY);
        }

        public Vector[] Project(ReadOnlySpan<CoordinatesS> coordinates)
        {
            var result = new Vector[coordinates.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Project(coordinates[i]);
            }
            return result;
        }
    }
}
