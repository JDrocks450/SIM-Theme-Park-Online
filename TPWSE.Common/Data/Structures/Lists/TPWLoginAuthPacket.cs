using MiscUtil.Conversion;
using SimTheme_Park_Online.Data.Primitive;
using System.Text;
using static SimTheme_Park_Online.Data.TPWConstants;

namespace SimTheme_Park_Online.Data.Structures
{
    public class TPWLoginAuthPacket : TPWPacket
    {
        public const uint AUTH_BODYSIZE = 0x0140;

        public TPWLoginAuthPacket(TPWLoginMsgCodes LoginCode, uint PlayerID, uint CustomerID,
            TPWUnicodeString Str1, TPWUnicodeString Email, uint ChildlockFlags)
        {
            OriginCode = TPWConstants.Bs_Header;
            MessageType = (ushort)LoginCode;
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

            this.PlayerID = PlayerID;
            this.CustomerID = CustomerID;
            this.Str1 = Str1;
            this.Email = Email;
#if false
            EmplaceBodyAt(0x58, 0x3A);
            EmplaceBodyAt(0x5C, 0x3B);
            EmplaceBodyAt(0x60, 0x3C);
            EmplaceBodyAt(0x64, 0xFF);
            EmplaceBodyAt(0x68, 0x01);
#endif
        }

        public TPWLoginMsgCodes LoginCode => (TPWLoginMsgCodes)MessageType;
        public DWORD PlayerID { get; }
        public uint CustomerID { get; }
        public TPWUnicodeString Str1 { get; }
        public TPWUnicodeString Email { get; }
        public bool IsSuccessfulLogin => LoginCode == TPWLoginMsgCodes.AUTH_SUCCESS;

        public TPWLoginAuthPacket(in TPWPacket packet)
        {
            MessageType = packet.MessageType;
            PacketQueue = packet.PacketQueue;
            if (packet.MessageType == (ushort)TPWLoginMsgCodes.AUTH_SUCCESS)
            {
                packet.SetPosition(0);
                PlayerID = packet.ReadBodyDword();
                CustomerID = packet.ReadBodyDword();
                SetPosition(0x30);
                Email = (TPWUnicodeString)packet.ReadBodyUnicodeString();
            }
        }
    }
}
