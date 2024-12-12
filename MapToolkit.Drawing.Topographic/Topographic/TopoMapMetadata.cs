
namespace Pmad.Cartography.Drawing.Topographic
{
    public class TopoMapMetadata
    {
        public TopoMapMetadata(string attribution, string title, string licenseNotice, string exportCreator, string upperTitle)
        {
            Attribution = attribution;
            Title = title;
            LicenseNotice = licenseNotice;
            ExportCreator = exportCreator;
            UpperTitle = upperTitle;
        }

        public static TopoMapMetadata None { get; } = new TopoMapMetadata("", "(none)", "", "", "");

        public string Attribution { get; }

        public string Title { get; }

        public string LicenseNotice { get; }

        public string ExportCreator { get; }

        public string UpperTitle { get; }

        internal TopoMapMetadata WithTitle(string title)
        {
            return new TopoMapMetadata(Attribution, title, LicenseNotice, ExportCreator, UpperTitle);
        }
    }
}
