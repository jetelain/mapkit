namespace Pmad.Cartography.Drawing.MemoryRender
{
    internal interface IRemapStyle
    {
        IDrawStyle MapStyle(MemDrawStyle style);
    }
}