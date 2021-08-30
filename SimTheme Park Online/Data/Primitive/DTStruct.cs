using MiscUtil.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Data.Primitive
{
    [Serializable]
    public class TPWDTStruct
    {
        public ushort A { get; set; }
        public ushort B { get; set; }
        public ushort C { get; set; }

        public TPWDTStruct()
        {

        }

        public TPWDTStruct(ushort A, ushort B, ushort C)
        {
            this.A = A;
            this.B = B;
            this.C = C;
        }

        public byte[] GetBytes()
        {
            byte[] buffer = new byte[6];
            EndianBitConverter.Big.CopyBytes(A, buffer, 0);
            EndianBitConverter.Big.CopyBytes(B, buffer, 2);
            EndianBitConverter.Big.CopyBytes(C, buffer, 4);
            return buffer;
        }
    }
}
