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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ChatResponseCode"></param>
        /// <param name="ParkName"></param>
        /// <param name="StrParam">THEME</param>
        /// <param name="ParkID"></param>
        /// <param name="NumberOfPlayers"></param>
        /// <param name="PacketLength"></param>
        /// <param name="Param"></param>
        public TPWChatRoomInfoPacket(TPWChatServerCommand ChatResponseCode, TPWUnicodeString ParkName,
                                     TPWUnicodeString Theme, DWORD ParkID, DWORD NumberOfPlayers, DWORD PacketLength,
                                     DWORD Param)
            : base((uint)ChatResponseCode,
                   ParkName,
                   Theme,
                   ParkID,
                   NumberOfPlayers,
                   PacketLength,
                   Param)
        {
            OriginCode = TPWConstants.Bs_Header;
            MessageType = 0x012D;
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

        /// <summary>
        /// Creates a <see cref="TPWChatRoomInfoPacket"/> from an existing <see cref="TPWChatRoomInfo"/> instance.
        /// </summary>
        /// <param name="RoomInfo">The room info</param>
        /// <param name="PacketLength">Unsure what this is</param>
        /// <param name="Param">Unsure what this is</param>
        public TPWChatRoomInfoPacket(TPWChatRoomInfo RoomInfo, DWORD PacketLength, DWORD Param) : 
            this(RoomInfo.ParkName, RoomInfo.ParkID, RoomInfo.NumberOfPlayers, PacketLength, Param)
        { }
    }
}
