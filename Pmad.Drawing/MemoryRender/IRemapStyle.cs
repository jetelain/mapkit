namespace Pmad.Drawing.MemoryRender
{
    internal interface IRemapStyle
    {
        IDrawStyle MapStyle(MemDrawStyle style);
    }
}