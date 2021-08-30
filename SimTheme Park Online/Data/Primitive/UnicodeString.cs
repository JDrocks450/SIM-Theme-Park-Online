using MiscUtil.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Data.Primitive
{
    [Serializable]
    /// <summary>
    /// This is a wrapper for TPW BOSS Data Type: UZ
    /// </summary>    
    public class TPWUnicodeString : ITPWBOSSSerializable
    {
        public TPWUnicodeString(string data)
        {
            String = data;
        }
        public TPWUnicodeString(uint data) : this(data.ToString())
        {

        }

        public string String { get; }

        public static implicit operator string(TPWUnicodeString d)
        {
            return d.String;
        }

        public static implicit operator TPWUnicodeString(string d)
        {
            return new TPWUnicodeString(d);
        }

        public override string ToString()
        {
            return String;
        }

        public byte[] GetBytes(bool FullFormat = true)
        {
            var str = String;
            var strBuffer = Encoding.Unicode.GetBytes(str);
            byte[] buffer = new byte[strBuffer.Length + (FullFormat ? 2 : 0)];
            if (FullFormat)
                EndianBitConverter.Big.CopyBytes((ushort)strBuffer.Length, buffer, 0);
            strBuffer.CopyTo(buffer, FullFormat ? 2 : 0);
            return buffer;
        }
    }
}
