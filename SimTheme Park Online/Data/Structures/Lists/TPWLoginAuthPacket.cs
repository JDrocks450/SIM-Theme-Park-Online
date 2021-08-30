using MiscUtil.Conversion;
using SimTheme_Park_Online.Data.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Data.Structures
{
    public class TPWLoginAuthPacket : TPWPacket
    {
        public const uint AUTH_BODYSIZE = 0x0140;

        public enum TPWLoginMsgCodes : ushort
        {
            /// <summary>
            /// Login Success code
            /// </summary>
            SUCCESS = 0x09,
            /// <summary>
            /// Error code 01
            /// </summary>
            ERROR_1 = 0x01,
            /// <summary>
            /// Error code 02
            /// </summary>
            ERROR_2 = 0x02
        }

        public TPWLoginAuthPacket(TPWLoginMsgCodes LoginCode, uint PlayerID, uint CustomerID, 
            TPWUnicodeString Str1, TPWUnicodeString Email, uint ChildlockFlags)            
        {
            ResponseCode = TPWConstants.Bs_Header;
            MsgType = (ushort)LoginCode;
            PacketQueue = 0x0A;
            AllocateBody(0x0140);
            EmplaceBody(PlayerID);
            EmplaceBody(CustomerID);
            EmplaceBodyAt(0x30, Encoding.Unicode.GetBytes(Str1.String));
            EmplaceBody(new byte[4]);
            EmplaceBody(Encoding.Unicode.GetBytes(Email.String));                  
            var footer = new byte[]
            {
                00,00,00,0x3D,
                00,00,00,0x3E,
                00,00,00,00,
                00,00,00,00
            };
            EndianBitConverter.Big.CopyBytes(ChildlockFlags, footer, 8);
            SetPosition((int)(BodyLength - footer.Length));
            EmplaceBody(footer);
#if false
            EmplaceBodyAt(0x58, 0x3A);
            EmplaceBodyAt(0x5C, 0x3B);
            EmplaceBodyAt(0x60, 0x3C);
            EmplaceBodyAt(0x64, 0xFF);
            EmplaceBodyAt(0x68, 0x01);
#endif
        }
    }
}
