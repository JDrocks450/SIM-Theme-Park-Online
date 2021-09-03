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
    public class DWORD : ITPWBOSSSerializable
    {
        public DWORD(uint data)
        {
            UInt32 = data;
        }

        public uint UInt32 { get; }

        public static implicit operator uint(DWORD d)
        {
            return d.UInt32;
        }

        public static implicit operator DWORD(uint d)
        {
            return new DWORD(d);
        }

        public override string ToString()
        {
            return UInt32.ToString();
        }

        public byte[] GetBytes(bool FullFormat = true)
        {
            return EndianBitConverter.Big.GetBytes(UInt32);             
        }
    }
}
