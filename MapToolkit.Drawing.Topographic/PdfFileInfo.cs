using Pmad.Geometry;

namespace Pmad.Cartography.Drawing.Topographic
{
    public class PdfFileInfo
    {
        public PdfFileInfo(bool isBook, string fileName, string name, int scale, Vector2D paperSize, List<PdfPageInfo> pages)
        {
            Name = name;
            FileName = fileName;
            Scale = scale;
            PaperSize = paperSize;
            Pages = pages;
            IsBook = isBook;
        }

        public string Name { get; }

        public string FileName { get; }

        public int Scale { get; }

        public Vector2D PaperSize { get; }

        public List<PdfPageInfo> Pages { get; }

        public bool IsBook { get; }
    }
}
