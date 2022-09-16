using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDEM.Drawing.PdfRender
{
    public static class PaperSize
    {
        public const double MillimeterFactor = 2.8346456692913389;

        public const double A0Width = 841 * MillimeterFactor;
        public const double A0Height = 1189 * MillimeterFactor;

        public const double A1Width = 594 * MillimeterFactor;
        public const double A1Height = 841 * MillimeterFactor;

        public const double A2Width = 420 * MillimeterFactor;
        public const double A2Height = 594 * MillimeterFactor;

        public const double A3Width = 297 * MillimeterFactor;
        public const double A3Height = 420 * MillimeterFactor;

        public const double A4Width = 210 * MillimeterFactor;
        public const double A4Height = 297 * MillimeterFactor;
    }
}
