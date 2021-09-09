using SimTheme_Park_Online.Data.Primitive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
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

        public static bool IsEqualTo(this SecureString ss1, SecureString ss2)
        {
            IntPtr bstr1 = IntPtr.Zero;
            IntPtr bstr2 = IntPtr.Zero;
            try
            {
                bstr1 = Marshal.SecureStringToBSTR(ss1);
                bstr2 = Marshal.SecureStringToBSTR(ss2);
                int length1 = Marshal.ReadInt32(bstr1, -4);
                int length2 = Marshal.ReadInt32(bstr2, -4);
                if (length1 == length2)
                {
                    for (int x = 0; x < length1; ++x)
                    {
                        byte b1 = Marshal.ReadByte(bstr1, x);
                        byte b2 = Marshal.ReadByte(bstr2, x);
                        if (b1 != b2) return false;
                    }
                }
                else return false;
                return true;
            }
            finally
            {
                if (bstr2 != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr2);
                if (bstr1 != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr1);
            }
        }
    }
}
