using QuazarAPI;
using QuazarAPI.Networking.Data;
using QuazarAPI.Networking.Standard;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;

namespace SimTheme_Park_Online
{
    public class CityServer : Component
    {
        public class TPWCityWaypoint
        {
            /// <summary>
            /// The name of bullfrog's City.
            /// </summary>
            public const string Bullfrog = "North Pole";

            public string Name
            {
                get; set;
            }

            public Vector3 Location
            {
                get; set;
            }

        }

        public CityServer(int port) : base("CityServer", port, SIMThemeParkWaypoints.CityServer)
        {
            
        }

        protected override void OnIncomingPacket(uint ID, TPWPacket Data)
        {
            if (!HandleCommand(ID, Data))
            {
                if (Data.PacketQueue == 14) // fifth and final packet accounted for
                {
                    var packets = TPWPacket.ParseAll(
                        "Library\\City\\CityServer-ThemeInfo.dat",
                        "Library\\City\\CityServer-LogicalServerInfo.dat",
                        "Library\\City\\CityServer-RideInfo.dat",
                        "Library\\City\\CityServer-ChatParksInfo.dat",
                        "Library\\City\\CityServer-CityInfo.dat"
                        ).ToArray();
                    var cityPacket = GetCityInfoPacket(out _);
                    Send(ID,
                        GetThemeInfoPacket(),
                        GetLogicalServerPacket(),
                        packets[2],
                        GetChatInfoPacket(),
                        cityPacket
                    );
                }
            }
        }

        private TPWPacket GetChatInfoPacket()
        {
            return Factory.TPWPacketFactory.GenerateChatParkInfoPacket(
                    new Data.Structures.TPWChatParkInfoStructure(
                    "Jeremy", "admin@bullfrog.com", 1, new byte[] { 00, 00, 00, 00 }, "SPACE", "Bisquick's Kingdom",
                    0x01, 0xAA, 0x10, 0x0A, 0x0B, 3, DateTime.Now, 0x0C, 0x0D, 0x0E, 0x0F, 0x09),
                    new Data.Structures.TPWChatParkInfoStructure(
                    "Jeremy", "admin@bullfrog.com", 15, new byte[] { 00, 00, 00, 00 }, "JUNGLE", "Bisquick's Kingdom",
                    0x00, 0x00, 0x00, 0x00, 0x00, 5, DateTime.Now, 0x00, 0x00, 0x00, 0x00, 0x00),
                    new Data.Structures.TPWChatParkInfoStructure(
                    "Jeremy", "admin@bullfrog.com", 17, new byte[] { 00, 00, 00, 00 }, "FANTASY", "Bisquick's Kingdom",
                    0x01, 0x00, 0x10, 0x0A, 0x0B, 1, DateTime.Now, 0x0C, 0x0D, 0x0E, 0x0F, 0x09),
                    new Data.Structures.TPWChatParkInfoStructure(
                    "Jeremy", "admin@bullfrog.com", 18, new byte[] { 00, 00, 00, 00 }, "FANTASY", "Bisquick's Kingdom",
                    0x01, 0x00, 0x10, 0x0A, 0x0B, 10, DateTime.Now, 0x0C, 0x0D, 0x0E, 0x0F, 0x09),
                    new Data.Structures.TPWChatParkInfoStructure(
                    "Jeremy", "admin@bullfrog.com", 5, new byte[] { 00, 00, 00, 00 }, "SPACE", "Bisquick's Kingdom",
                    0x01, 0x00, 0x10, 0x0A, 0x0B, 8, DateTime.Now, 0x0C, 0x0D, 0x0E, 0x0F, 0x09)
                );
        }

        public static TPWPacket GetCityInfoPacket(out Data.Templating.TPWDataTemplate Template) => Factory.TPWPacketFactory.GenerateCityInfoPacket(
            out Template,
            //123.0 0 0
            new Data.Structures.TPWCityInfoStructure(0x0A, "North Pole", "bullfrog", 2.140f, 0.0f, 122.98f, 0x0B, 100, 0x0D, "bullfrog", 0x01, 0x0F),
            new Data.Structures.TPWCityInfoStructure(0x0B, "Bloaty Land", "Bisquick", 100, 70, 20, 0x20, 0x03, 0x05, "Admin", 0x01, 0x10));

        private TPWPacket GetLogicalServerPacket() => Factory.TPWPacketFactory.GenerateLogicalServerPacket(
            0x0A, "SERVER1", 0x0B, "SERVERB", 0x0C, 0x0D, "SERVERC", "SERVERD", 0x0E, "SERVERE");
            
        private TPWPacket GetThemeInfoPacket() => Factory.TPWPacketFactory.GenerateThemeInfoPacket(
            "SPACE", "JUNGLE", "FANTASY", out Data.TPWServersideList List);

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
