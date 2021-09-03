using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Data.Primitive
{
    [Serializable]
    /// <summary>
    /// Used to differentiate 
    /// </summary>
    public class TPWZeroTerminatedString : ITPWBOSSSerializable
    {
        public TPWZeroTerminatedString(string data)
        {
            String = data;
        }
        public TPWZeroTerminatedString(uint data) : this(data.ToString()) { }

        public string String { get; }

        public static implicit operator string(TPWZeroTerminatedString d)
        {
            return d.String;
        }

        public static implicit operator TPWZeroTerminatedString(string d)
        {
            return new TPWZeroTerminatedString(d);
        }

        public static TPWZeroTerminatedString Fill(int amount, char fillChar = 'A')
        {
            return new string(fillChar, amount);
        }

        public override string ToString()
        {
            return String;
        }

        public byte[] GetBytes(bool FullFormat = true)
        {
            var text = Encoding.ASCII.GetBytes(String);
            byte[] buffer = new byte[text.Length + (FullFormat ? 1 : 0)];
            text.CopyTo(buffer, 0);
            return buffer;
        }
    }
}
