using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Interop
{
    public class TPWStringDecoder
    {
        public static string FromMultibyte(byte[] MBString)
        {
            return Encoding.Unicode.GetString(MBString);
        }
    }
}
