using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Util
{
    public static class HexConverter
    {
        public static byte[] FromString(string HexBytes)
        {
            byte[] array = new byte[HexBytes.Length / 2];
            for (int i = 0; i < array.Length; i++)
            {
                string str = HexBytes.Substring(0, 2);
                array[i] = Convert.ToByte(str, 16);
                HexBytes = HexBytes.Remove(0, 2);
            }
            return array;
        }        
        public static string ByteStringToUnicode(string HexBytes)
        {
            var array = FromString(HexBytes);
            return Encoding.Unicode.GetString(array);
        }
    }
}
