using QuazarAPI;
using QuazarAPI.Networking.Standard;
using SimTheme_Park_Online.Data;
using SimTheme_Park_Online.Data.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online
{
    public class ChatServer : Component
    {
        uint MessageDirectorID;

        private CityServer CityServer => ServerManagement.Current.CityServer;

        public ChatServer(int port) : base("ChatServer", port, SIMThemeParkWaypoints.ChatServer)
        {
            
        }

        private IEnumerable<TPWChatRoomInfoPacket> GetRoomInfoPackets()
        {
            List<TPWChatRoomInfoPacket> list = new List<TPWChatRoomInfoPacket>();
            foreach (TPWParkInfo park in CityServer.Parks)
            {
                list.Add(park.GetRoomInfoPacket());
            }
            return list;
        }

        protected override void OnIncomingPacket(uint ID, TPWPacket Data)
        {
            var parkInfo = CityServer.Parks.ElementAt((int)(Data.PacketQueue - 0x0A));
            var packet = parkInfo.GetRoomInfoPacket(Data.PacketQueue);
            packet.PacketQueue = Data.PacketQueue;
            Send(ID, packet);
        }

        protected override void OnManualSend(uint ID, ref TPWPacket Data)
        {
            OnIncomingPacket(ID, Data);
            base.OnManualSend(ID, ref Data);
        }

        public override void Start()
        {
            QConsole.WriteLine(Name, "Starting...");
            BeginListening();
        }

        protected override void OnClientConnect(TcpClient Connection, uint ID)
        {
            base.OnClientConnect(Connection, ID);
        }

        public override void Stop()
        {
            QConsole.WriteLine(Name, "Stopping...");
            StopListening();
        }

        protected override void OnOutgoingPacket(uint ID, TPWPacket Data)
        {
            //Data.PacketQueue = PacketQueue;
        }
    }
}
