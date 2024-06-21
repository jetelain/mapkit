using System.Collections.Generic;
using System.Linq;
using GeoJSON.Text.Geometry;

namespace MapToolkit.Test
{
    public class GeometryExtensionsTest
    {
        private static Polygon Square100x100()
        {
            return new Polygon([[[100, 100], [0, 100], [0, 0], [100, 0], [100, 100]]]);
        }

        private static Polygon Square100x100WithHole()
        {
            return new Polygon([[[100, 100], [0, 100], [0, 0], [100, 0], [100, 100]], [[75, 75], [75, 25], [25, 25], [25, 75], [75, 75]]]);
        }

        private static Polygon Square10x10()
        {
            return new Polygon([[[55, 55], [55, 45], [45, 45], [45, 55], [55, 55]]]);
        }

        private static Polygon Square50x50()
        {
            return new Polygon([[[75, 75], [75, 25], [25, 25], [25, 75], [75, 75]]]);
        }

        private static List<Polygon> SquareBands100x100WithHole()
        {
            return new List<Polygon> {
                new Polygon([[[0, 0 ], [100, 0 ], [100, 25 ], [0, 25 ], [0, 0 ]]]),
                new Polygon([[[0, 75], [100, 75], [100, 100], [0, 100], [0, 75]]]),
                new Polygon([[[0, 0 ], [0, 100 ], [25,100  ], [25, 0 ], [0, 0 ]]]),
                new Polygon([[[75, 0], [75, 100], [100, 100], [100, 0], [75, 0]]])
            };
        }

        [Fact]
        public void ShellArea()
        {
            Assert.Equal(10000, Square100x100().GetShellArea());
            Assert.Equal(100, Square10x10().GetShellArea());
            Assert.Equal(2500, Square50x50().GetShellArea());
        }

        [Fact]
        public void SubstractNoOverlap()
        {
            var result = Square100x100().SubstractNoOverlap(new[] { Square50x50() });
            var polygon = Assert.Single(result);
            Assert.Equal("POLYGON ((100 100, 0 100, 0 0, 100 0, 100 100), (25 25, 25 75, 75 75, 75 25, 25 25))", ToString(polygon));

            result = Square50x50().SubstractNoOverlap(new[] { Square100x100() });
            Assert.Empty(result);
        }

        [Fact]
        public void Substract()
        {
            var result = Square100x100().Substract(new[] { Square50x50() });
            var polygon = Assert.Single(result);
            Assert.Equal("POLYGON ((100 100, 0 100, 0 0, 100 0, 100 100), (25 25, 25 75, 75 75, 75 25, 25 25))", ToString(polygon));

            result = Square100x100().Substract(SquareBands100x100WithHole());
            polygon = Assert.Single(result);
            Assert.Equal("POLYGON ((75 75, 25 75, 25 25, 75 25, 75 75))", ToString(polygon));

            result = Square50x50().Substract(new[] { Square100x100() });
            Assert.Empty(result);

            result = Square100x100().Substract(new[] { Square100x100WithHole() });
            polygon = Assert.Single(result);
            Assert.Equal("POLYGON ((75 75, 25 75, 25 25, 75 25, 75 75))", ToString(polygon));
        }

        [Fact]
        public void UnionToMultiPolygon()
        {
            var result = new[] { Square100x100(), Square50x50() }.UnionToMultiPolygon();
            var polygon = Assert.Single(result.Coordinates);
            Assert.Equal("POLYGON ((100 100, 0 100, 0 0, 100 0, 100 100))", ToString(polygon));

            result = new[] { Square100x100WithHole(), Square50x50(), Square10x10() }.UnionToMultiPolygon();
            polygon = Assert.Single(result.Coordinates);
            Assert.Equal("POLYGON ((100 100, 0 100, 0 0, 100 0, 100 100))", ToString(polygon));

            result = SquareBands100x100WithHole().UnionToMultiPolygon();
            polygon = Assert.Single(result.Coordinates);
            Assert.Equal("POLYGON ((0 100, 0 0, 100 0, 100 100, 0 100), (75 25, 25 25, 25 75, 75 75, 75 25))", ToString(polygon));

            result = SquareBands100x100WithHole().Concat(new[] { Square10x10() }).ToList().UnionToMultiPolygon();
            Assert.Equal(2, result.Coordinates.Count);
            polygon = result.Coordinates[0];
            Assert.Equal("POLYGON ((100 100, 0 100, 0 0, 100 0, 100 100), (25 25, 25 75, 75 75, 75 25, 25 25))", ToString(polygon));
            polygon = result.Coordinates[1];
            Assert.Equal("POLYGON ((55 55, 45 55, 45 45, 55 45, 55 55))", ToString(polygon));
        }

        private string ToString(Polygon polygon)
        {
            return $"POLYGON ({string.Join(", ", polygon.Coordinates.Select(ToStringContent))})";
        }

        private string ToStringContent(LineString lineString)
        {
            return $"({string.Join(", ", lineString.Coordinates.Select(p => $"{p.Longitude} {p.Latitude}"))})";
        }
    }
}
