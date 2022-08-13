using SimpleDEM;
using SimpleDEM.DataCells;

namespace DemFormat
{
    internal class Tester
    {
        static void Main(string[] args)
        {
            var cellSRTM = DemDataCell.Load(@"E:\Carto\SRTMv3\N29E095.SRTMGL1.hgt.zip");
            var cellAW3D30 = DemDataCell.Load(@"C:\temp\z\ALPSMLC30_N029E095_DSM.tif");

            var aSRTM = cellSRTM.GetRawElevation(new GeodeticCoordinates(29.1, 95.1));
            var aAW3D30 = cellAW3D30.GetRawElevation(new GeodeticCoordinates(29.1, 95.1));

            var bSRTM = cellSRTM.GetRawElevation(new GeodeticCoordinates(29.5,95.5));
            var bAW3D30 = cellAW3D30.GetRawElevation(new GeodeticCoordinates(29.5, 95.5));

            var cSRTM = cellSRTM.GetRawElevation(new GeodeticCoordinates(29.9, 95.9));
            var cAW3D30 = cellAW3D30.GetRawElevation(new GeodeticCoordinates(29.9, 95.9));

            var dSRTM = cellSRTM.GetLocalElevation(new GeodeticCoordinates(29.1, 95.1), new StandardInterpolation());
            var dAW3D30 = cellAW3D30.GetLocalElevation(new GeodeticCoordinates(29.1, 95.1), new StandardInterpolation());

            var eSRTM = cellSRTM.GetLocalElevation(new GeodeticCoordinates(29.5, 95.5), new StandardInterpolation());
            var eAW3D30 = cellAW3D30.GetLocalElevation(new GeodeticCoordinates(29.5, 95.5), new StandardInterpolation());

            var fSRTM = cellSRTM.GetLocalElevation(new GeodeticCoordinates(29.9, 95.9), new StandardInterpolation());
            var fAW3D30 = cellAW3D30.GetLocalElevation(new GeodeticCoordinates(29.9, 95.9), new StandardInterpolation());
        }

    }
}