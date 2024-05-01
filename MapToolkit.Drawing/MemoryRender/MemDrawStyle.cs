namespace MapToolkit.Drawing.MemoryRender
{
    internal class MemDrawStyle : IDrawStyle
    {
        public MemDrawStyle(IBrush? fill, Pen? pen)
        {
            Fill = fill;
            Pen = pen;
        }

        public IBrush? Fill { get; }
        public Pen? Pen { get; }
    }
}