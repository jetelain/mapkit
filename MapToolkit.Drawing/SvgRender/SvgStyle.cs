using System;
using System.Collections.Generic;
using System.Text;

namespace Pmad.Cartography.Drawing.SvgRender
{
    internal class SvgStyle : IDrawStyle
    {
        public SvgStyle(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
