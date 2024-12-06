namespace Pmad.Cartography.Drawing.PdfRender
{
    /// <summary>
    /// Various size given in PostScript points
    /// </summary>
    public static class PaperSize
    {
        public const double OneInch = 72;
        public const double OneMilimeter = 2.8346456692913389; // == OneInch / 25.4 mm

        public const double OnePixelAt300Dpi = 0.24; // == OneInch / 300 px
        public const double OnePixelAt72Dpi = 1;

        // Most map / plan printing hardware is capable to print up to Arch E

        public const double ArchEWidth = 914 * OneMilimeter;
        public const double ArchEHeight = 1220 * OneMilimeter;

        public const double A0Width = 841 * OneMilimeter;
        public const double A0Height = 1189 * OneMilimeter;

        public const double A1Width = 594 * OneMilimeter;
        public const double A1Height = 841 * OneMilimeter;

        public const double A2Width = 420 * OneMilimeter;
        public const double A2Height = 594 * OneMilimeter;

        public const double A3Width = 297 * OneMilimeter;
        public const double A3Height = 420 * OneMilimeter;

        public const double A4Width = 210 * OneMilimeter;
        public const double A4Height = 297 * OneMilimeter;

    }
}
