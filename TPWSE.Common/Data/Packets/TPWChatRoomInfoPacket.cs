using SimTheme_Park_Online.Data.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SimTheme_Park_Online.Data.TPWConstants;

namespace SimTheme_Park_Online.Data.Packets
{
    public sealed class TPWChatRoomInfoPacket : TPWChatPacket
    {
        public TPWChatRoomInfoPacket(TPWChatServerCommand ChatResponseCode, TPWUnicodeString ParkName,
                                     TPWUnicodeString StrParam, DWORD ParkID, DWORD NumberOfPlayers, DWORD PacketLength,
                                     DWORD Param)
            : base((uint)ChatResponseCode,
                   ParkName,
                   StrParam,
                   ParkID,
                   NumberOfPlayers,
                   PacketLength,
                   Param)
        {
            ResponseCode = TPWConstants.Bs_Header;
            MsgType = 0x012D;
            Language = 0x0000;
            PacketQueue = 0x0D;
        }

            /*EmplaceBody((uint)ChatResponseCode);

            EmplaceBlock(ParkName);
            EmplaceBlock(StrParam);
            EmplaceBlock(ParkID.ToSZ());
            EmplaceBlock(NumberOfPlayers.ToSZ());
            EmplaceBlock(PacketLength.ToSZ());
            EmplaceBlock(Param.ToSZ());

            EmplaceBody(00);*/        

        public TPWChatRoomInfoPacket(TPWUnicodeString ParkName, uint ParkID, uint NumberOfPlayers, uint PacketLength, uint Param)
            : this(
                     TPWConstants.TPWChatServerCommand.RoomInfo,
                     ParkName, "", ParkID, NumberOfPlayers, PacketLength, Param
                  )
        {

        }
    }
}
