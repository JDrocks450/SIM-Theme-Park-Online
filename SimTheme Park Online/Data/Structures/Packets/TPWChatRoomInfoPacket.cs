using SimTheme_Park_Online.Data.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Data.Structures
{
    public class TPWChatRoomInfoPacket : TPWPacket
    {
        private enum TPWChatTypeCodes
        {
            UNI = 0x02,
            ASCII = 0x01
        }

        public void EmplaceBlock(TPWUnicodeString StringValue)
        {            
            EmplaceBody((byte)TPWChatTypeCodes.UNI);
            EmplaceBody(StringValue, false);
            EmplaceBody(0xFF, 0xFF);
        }
        public void EmplaceBlock(TPWZeroTerminatedString StringValue)
        {            
            EmplaceBody((byte)TPWChatTypeCodes.ASCII);
            EmplaceBody(StringValue, false);
            EmplaceBody(0xFF);
        }

        public TPWChatRoomInfoPacket(TPWUnicodeString ParkName, uint ParkID, uint NumberOfPlayers, uint PacketLength, uint Param)
        {
            ResponseCode = TPWConstants.Bs_Header;
            MsgType = 0x012D;
            Language = 0x0000;
            PacketQueue = 0x0D;

            EmplaceBody((uint)TPWConstants.TPWChatServerResponseCode.RoomInfo);

            EmplaceBlock(ParkName);
            EmplaceBlock(new TPWUnicodeString(""));
            EmplaceBlock(ParkID.ToSZ());
            EmplaceBlock(NumberOfPlayers.ToSZ());
            EmplaceBlock(PacketLength.ToSZ());
            EmplaceBlock(Param.ToSZ());

            EmplaceBody(00);
        }
    }
}
