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
        public ushort Year { get; set; }
        public ushort Month { get; set; }
        public ushort Day { get; set; }

        public TPWDTStruct()
        {

        }

        public TPWDTStruct(ushort Year, ushort Month, ushort Day)
        {
            this.Year = Year;
            this.Month = Month;
            this.Day = Day;
        }

        public byte[] GetBytes()
        {
            byte[] buffer = new byte[6];
            EndianBitConverter.Big.CopyBytes(Year, buffer, 0);
            EndianBitConverter.Big.CopyBytes(Month, buffer, 2);
            EndianBitConverter.Big.CopyBytes(Day, buffer, 4);
            return buffer;
        }

        public static implicit operator DateTime(TPWDTStruct Struct)
        {
            return new DateTime(Struct.Year, Struct.Month, Struct.Day);
        }
        public static implicit operator TPWDTStruct(DateTime Struct)
        {
            return new TPWDTStruct((ushort)Struct.Year, (ushort)Struct.Month, (ushort)Struct.Day);
        }
    }
}
