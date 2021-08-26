using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Data.Primitive
{
    /// <summary>
    /// This is a wrapper for TPW BOSS Data Type: UZ
    /// </summary>
    public class TPWUnicodeString
    {
        public TPWUnicodeString(string data)
        {
            String = data;
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
    }
}
