using SimTheme_Park_Online.Data.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Primitive
{
    public struct TPWPosition
    {
        public DWORD X, Y;
        public TPWPosition(DWORD X, DWORD Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }
}
