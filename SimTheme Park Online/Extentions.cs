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
        public static TPWUnicodeString ToUZ(this string Data)
        {
            return new TPWUnicodeString(Data);
        }
        public static TPWZeroTerminatedString ToSZ(this string Data)
        {
            return new TPWZeroTerminatedString(Data);
        }
    }
}
