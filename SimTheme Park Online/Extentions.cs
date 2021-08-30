using SimTheme_Park_Online.Data.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online
{
    public static class Extentions
    {
        public static TPWUnicodeString ToUZ(this string Data) => new TPWUnicodeString(Data);
        public static TPWUnicodeString ToUZ(this uint Data) => new TPWUnicodeString(Data);
        public static TPWZeroTerminatedString ToSZ(this string Data) => new TPWZeroTerminatedString(Data);
        public static TPWZeroTerminatedString ToSZ(this uint Data) => new TPWZeroTerminatedString(Data);
        public static TPWZeroTerminatedString ToSZ(this int Data) => new TPWZeroTerminatedString((uint)Data);
    }
}
