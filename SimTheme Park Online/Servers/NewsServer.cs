using QuazarAPI;
using QuazarAPI.Networking.Data;
using QuazarAPI.Networking.Standard;
using SimTheme_Park_Online.Data;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SimTheme_Park_Online
{
    public class NewsServer : Component
    {
        uint MessageDirectorID;

        public NewsServer(int port) : base("NewsServer", port, SIMThemeParkWaypoints.NewsServer)
        {
            
        }

        protected override void OnIncomingPacket(uint ID, TPWPacket Data)
        {
            if (!HandleCommand(ID, Data))
            {
                if (Data.PacketQueue == 0x0C)
                {
                    if (true)
                    {
                        var packets = TPWPacket.ParseAll("Library\\News\\NewsPacket.dat");
                        QConsole.WriteLine(Name, "Sending NewsPackets...\n");
                        Send(ID, packets.ToArray());
                        return;
                    }
                    QConsole.WriteLine(Name, "Sending NewsPackets...\n");
                    Send(ID,
                        GenerateServerInfoPacket(),
                        GenerateGameInfoPacket(ServerManagement.Current.Profile.GameNewsString),
                        GenerateSystemInfoPacket(ServerManagement.Current.Profile.SystemNewsString));
                }
            }
        }

        private TPWPacket GenerateServerInfoPacket()
        {
            return new TPWPacket()
            {
                ResponseCode = TPWConstants.Bs_Header,
                MsgType = 0x0007,
                Language = 0x0809,
                Param2 = 0x0000,
                Param3 = 0x08,
                Body = Encoding.Unicode.GetBytes("System Information")
            };
        }
        private TPWPacket GenerateGameInfoPacket(string News)
        {
            return new TPWPacket()
            {
                ResponseCode = TPWConstants.Bs_Header,
                MsgType = 0x0005,
                Language = 0x0809,
                Param2 = 0x0000,
                Param3 = 0x00,
                PacketQueue = 0x0B,
                Body = Encoding.Unicode.GetBytes(News)
            };
        }
        private TPWPacket GenerateSystemInfoPacket(string News)
        {
            return new TPWPacket()
            {
                ResponseCode = TPWConstants.Bs_Header,
                MsgType = 0x0004,
                Language = 0x0809,
                Param2 = 0x0000,
                Param3 = 0x00,
                PacketQueue = 0x0C,
                Body = Encoding.Unicode.GetBytes(News)
            };
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
    }
}
