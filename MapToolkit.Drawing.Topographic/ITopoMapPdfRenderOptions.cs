namespace MapToolkit.Drawing.Topographic
{
    public interface ITopoMapPdfRenderOptions
    {
        public string FileName { get; }

        public string TargetDirectory { get; }

        public string Attribution { get; }
    }
}
