using System;
using System.Diagnostics;
using MapToolkit;
using MapToolkit.DataCells;
using MapToolkit.GeodeticSystems;

namespace DemFormat
{
    internal class Tester
    {
        private static void Mesure(Func<double,double> func)
        {
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 100_000; ++i)
            {
                for (var lat = 0d; lat <= 90; ++lat)
                {
                    func(lat);
                }
            }
            sw.Stop();
            Console.WriteLine($"{sw.ElapsedMilliseconds} msec");
        }


        static void Main(string[] args)
        {
            var wantedResolution = 2.0;

            var t1 = 0L;
            var t2 = 0L;


            for (var lat = 0d; lat <= 90; ++lat)
            {
                var dl = WSG84.Delta1Long(lat);
                var x = Math.Ceiling(dl / wantedResolution);
                var y = RoundTo2Pow(dl / wantedResolution); // 3600

                var da = WSG84.Delta1Lat(lat);
                var u = Math.Ceiling(da / wantedResolution);
                var v = RoundTo2Pow(da / wantedResolution); // 3600

                Console.WriteLine($" {lat:00}° => {Math.Round(dl)}m {Math.Round(da)}m");


                //Console.WriteLine($" {lat:00}° => {x:000000}, {dl / x:0.0000}m, {y:000000}, {dl / y:0.0000}m| {u:0000000}, {da / u:0.0000}m, {v:0000000}, {da / v:0.0000}m");

                t1 += (long)(x * u);
                t2 += (long)(v * y);

                //Console.WriteLine($" {lat:00}° => {Math.Round(da)}");

                // 225
                // 450
                // 900
                // 1800
                // 3600
                // 7200
                // 14400
                // 28800
                // 57600
                // 115200
                // 230400
            }

            Console.WriteLine($"{t1} {Math.Round((double) t1 / (double) t2 * 100)}");
            Console.WriteLine($"{t2}");
            Mesure(WSG84.Delta1Long);
            Mesure(WSG84.Delta1Lat);

            //var cellSRTM = DemDataCell.Load(@"E:\Carto\SRTMv3\N29E095.SRTMGL1.hgt.zip");
            //var cellAW3D30 = DemDataCell.Load(@"C:\temp\z\ALPSMLC30_N029E095_DSM.tif");

            //var aSRTM = cellSRTM.GetRawElevation(new GeodeticCoordinates(29.1, 95.1));
            //var aAW3D30 = cellAW3D30.GetRawElevation(new GeodeticCoordinates(29.1, 95.1));

            //var bSRTM = cellSRTM.GetRawElevation(new GeodeticCoordinates(29.5,95.5));
            //var bAW3D30 = cellAW3D30.GetRawElevation(new GeodeticCoordinates(29.5, 95.5));

            //var cSRTM = cellSRTM.GetRawElevation(new GeodeticCoordinates(29.9, 95.9));
            //var cAW3D30 = cellAW3D30.GetRawElevation(new GeodeticCoordinates(29.9, 95.9));

            //var dSRTM = cellSRTM.GetLocalElevation(new GeodeticCoordinates(29.1, 95.1), new DefaultInterpolation());
            //var dAW3D30 = cellAW3D30.GetLocalElevation(new GeodeticCoordinates(29.1, 95.1), new DefaultInterpolation());

            //var eSRTM = cellSRTM.GetLocalElevation(new GeodeticCoordinates(29.5, 95.5), new DefaultInterpolation());
            //var eAW3D30 = cellAW3D30.GetLocalElevation(new GeodeticCoordinates(29.5, 95.5), new DefaultInterpolation());

            //var fSRTM = cellSRTM.GetLocalElevation(new GeodeticCoordinates(29.9, 95.9), new DefaultInterpolation());
            //var fAW3D30 = cellAW3D30.GetLocalElevation(new GeodeticCoordinates(29.9, 95.9), new DefaultInterpolation());
        }

        private static double RoundTo2Pow(double value, int scale = 225)
        {
            return scale * Math.Pow(2, Math.Round(Math.Log(value / scale) / Math.Log(2)));
        }
    }
}