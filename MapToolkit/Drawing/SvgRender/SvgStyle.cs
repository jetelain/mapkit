using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDEM.Drawing.SvgRender
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
