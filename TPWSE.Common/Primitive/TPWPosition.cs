using SimTheme_Park_Online.Data.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Primitive
{
    /// <summary>
    /// A position in TPW-SE
    /// </summary>
    public struct TPWPosition
    {
        public uint X, Y;
        public TPWPosition(uint X, uint Y)
        {
            this.X = X;
            this.Y = Y;
        }
        public override string ToString()
        {
            return $"({X},{Y})";
        }
    }
}
