using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Data.Primitive
{
    /// <summary>
    /// Used to differentiate 
    /// </summary>
    public class TPWZeroTerminatedString
    {
        public TPWZeroTerminatedString(string data)
        {
            String = data;
        }

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
    }
}
