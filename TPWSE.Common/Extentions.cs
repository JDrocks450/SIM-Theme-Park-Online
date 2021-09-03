using SimTheme_Park_Online.Data.Primitive;
using System;
using System.Collections.Generic;
using System.IO;
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
        public static DWORD ToDWORD(this ITPWBOSSSerializable Data) => ToDWORD((TPWZeroTerminatedString)Data);
        public static DWORD ToDWORD(this TPWZeroTerminatedString Data)
        {
            foreach (char c in Data.String)
                if (!char.IsDigit(c))
                    throw new Exception("This string is not all numbers!");
            return new DWORD(uint.Parse(Data.String));
        }
        /// <summary>
        /// An extention method to imitate the way this functions in .NET 5.0
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="Data"></param>
        public static void Write(this Stream Buffer, byte[] Data) => Buffer.Write(Data, 0, Data.Length);
    }
}
