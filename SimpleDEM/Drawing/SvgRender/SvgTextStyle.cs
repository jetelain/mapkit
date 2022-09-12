namespace SimpleDEM.Drawing.SvgRender
{
    internal class SvgTextStyle : SvgStyle, IDrawTextStyle
    {
        public SvgTextStyle(string name, string? bgName) : base(name)
        {
            BgName = bgName;
        }

        public string? BgName { get; }
    }
}
