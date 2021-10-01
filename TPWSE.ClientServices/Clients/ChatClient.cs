using QuazarAPI.Networking.Standard;
using SimTheme_Park_Online.Data;
using SimTheme_Park_Online.Data.Packets;
using SimTheme_Park_Online.Data.Primitive;
using SimTheme_Park_Online.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TPWSE.ClientServices.Clients
{
    public class ChatClient : QuazarClient
    {
        /// <summary>
        /// This is updated everytime <see cref="DownloadChatRoomInfo(DWORD[])"/> is called.
        /// </summary>
        public IEnumerable<TPWChatRoomInfo> OnlineChatRooms { get; private set; }

        public ChatClient(IPAddress Address, int Port) : base("ChatClient", Address, Port)
        {

        }

        /// <summary>
        /// Downloads information about a park from the game server and updates <see cref="OnlineChatRooms"/>
        /// </summary>
        /// <param name="ParkIDs"></param>
        /// <returns></returns>
        public async Task<TPWChatRoomInfo[]> DownloadChatRoomInfo(params DWORD[] ParkIDs)
        {
            if (!IsConnected)
                await Connect();
            TPWChatRoomInfo[] roomInfos = new TPWChatRoomInfo[ParkIDs.Length];
            int index = 0;
            foreach (DWORD ID in ParkIDs)
            {
                TPWChatPacket packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.RoomInfo, ID);
                await SendPacket(packet);
                var response = await AwaitPacket();
                TPWChatPacketParser.Parse(response, out List<TPWChatParsedData> values);
                TPWChatRoomInfo roomInfo = new TPWChatRoomInfo(values);
                roomInfos[index] = roomInfo;
                index++;
            }
            OnlineChatRooms = roomInfos;
            return roomInfos;
        }
    }
}
