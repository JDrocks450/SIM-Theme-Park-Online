using QuazarAPI;
using QuazarAPI.Networking.Data;
using QuazarAPI.Networking.Standard;
using SimTheme_Park_Online.Data.Primitive;
using SimTheme_Park_Online.Data.Structures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;

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

        private Queue<TPWPacket> PacketQueue = new Queue<TPWPacket>();
        private TPWCityResponseStructure[] GetCities(Data.TPWConstants.TPWServerListType Type) =>
            new TPWCityResponseStructure[]
        {
            new Data.Structures.TPWCityResponseStructure(Type,
                    "Bisquick", "admin@bullfrog.com", 16, new byte[]
                    {
                        01, 02, 00, 03,
                        04, 05,
                        06, 07,
                        08, 09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
                    },
                    "Bisquick's Testing Zone",
                    "This park is for testing purposes. Thanks for tuning in! The codebase behind this breakthrough is Quazar.TPW-SE, " +
                    "an API for interacting with an unmodified, SIM Theme Park 1.0 and 2.0 installation. - Jeremy, Bisquick",
                    0x0B, 0x03, 0x00, 0x02, 0x01, "Daphene", new TPWDTStruct(0x2021, 0x0008, 0x0017), 0x0000000A, 0x01, 0x01, 0x0D, 0x0E),

                    new Data.Structures.TPWCityResponseStructure(Type, "admin", "", 0x15, new byte[] { 05, 07, 07, 05 }, "Test Center 3",
                    "This is an alternate chat park for testing purposes.",
                    0x30, 0x1111, 0x01, 0x00, 0x02, "Apollo", new TPWDTStruct(0x0000, 0x0000, 0x000B), 0x0B, 0x01, 0x02, 0x0D, 0x0E)
        };

        public CityServer(int port) : base("CityServer", port, SIMThemeParkWaypoints.CityServer)
        {

        }

        protected override void OnIncomingPacket(uint ID, TPWPacket Data)
        {
            if (Data.PacketQueue == 10)
                PacketQueue.Enqueue(GetThemeInfoPacket());
            if (Data.PacketQueue == 11)
                PacketQueue.Enqueue(GetLogicalServerPacket());
            if (Data.PacketQueue == 12)
                PacketQueue.Enqueue(GetRideInfoPacket());
            if (Data.PacketQueue == 13)
                PacketQueue.Enqueue(GetChatInfoPacket());
            if (Data.PacketQueue == 14) // fifth and final packet accounted for
            {
                string BodyText = Encoding.ASCII.GetString(Data.Body);
                if (BodyText.Contains("CITYID=") && BodyText.Substring(BodyText.IndexOf("CITYID=") + 7, 2) == "11")
                {
                    QConsole.WriteLine("ATTENTION", "Game awaiting packet for a city!");
                    PacketQueue.Enqueue(GetTop10Packet());
                }
                else
                    PacketQueue.Enqueue(GetCityInfoPacket());
                Send(ID, PacketQueue.ToArray());
                PacketQueue.Clear();
            }
            if (Data.PacketQueue == 15)
            {
                Send(ID, GetSearchPacket());
            }
        }

        private TPWPacket GetRideInfoPacket() => Factory.TPWPacketFactory.GenerateRideInfoPacket(
                new Data.Structures.TPWRideInfoPacketStructure("RIDENAME", "SEC STR", new byte[] { 00, 00, 00, 00 }, 0x01, 0x00, "TESTSTR3", 0x02, 0x03, 0x04));

        private TPWPacket GetSearchPacket()
        {
            var packet = Factory.TPWPacketFactory.GenerateCityResponsePacket(GetCities(Data.TPWConstants.TPWServerListType.SEARCH_RESULT));
            packet.PacketQueue = 0x0F;
            return packet;
        }
        private TPWPacket GetTop10Packet()
        {
            var packet = Factory.TPWPacketFactory.GenerateCityResponsePacket(GetCities(Data.TPWConstants.TPWServerListType.TOP10_RESULT));
            packet.PacketQueue = 0x0E;
            return packet;
        }
        private TPWPacket GetChatInfoPacket() => Factory.TPWPacketFactory.GenerateCityResponsePacket(GetCities(Data.TPWConstants.TPWServerListType.PARKS_INFO));

        public static TPWPacket GetCityInfoPacket() => Factory.TPWPacketFactory.GenerateCityInfoPacket(
            //123.0 0 0
            new Data.Structures.TPWCityInfoStructure(0x0000000A, "North Pole", "bullfrog", 2.140f, 0.0f, 119.98f, 0x0B, 100, 0x00, "bullfrog", 0x01, 0x0F),
            new Data.Structures.TPWCityInfoStructure(0x0000000B, "Bloaty Land", "Bisquick", 100, 70, 20, 0x20, 0x03, 0x05, "Admin", 0x01, 0x10));

        private TPWPacket GetLogicalServerPacket() => Factory.TPWPacketFactory.GenerateLogicalServerPacket(
            new Data.Structures.TPWLogicalServerStructure( 
                0x01, "Daphene", 0x02, "Daphene", 0x03, 0x04, "Daphene", "Daphene", 0x05, "Daphene"),
            new Data.Structures.TPWLogicalServerStructure( 
                0x00, "TPW-SE Ls: B", 0x00, "STR2 TEST", 0x00, 0x00, "STR3 TEST", "STR4 TEST", 0x00, "STR5 TEST"),
            new Data.Structures.TPWLogicalServerStructure( 
                0x00, "TPW-SE Ls: C", 0x00, "STR2 TEST", 0x00, 0x00, "STR3 TEST", "STR4 TEST", 0x00, "STR5 TEST"),
            new Data.Structures.TPWLogicalServerStructure( 
                0x00, "TPW-SE Ls: D", 0x00, "STR2 TEST", 0x00, 0x00, "STR3 TEST", "STR4 TEST", 0x00, "STR5 TEST"),
            new Data.Structures.TPWLogicalServerStructure( 
                0x00, "TPW-SE Ls: 5", 0x00, "STR2 TEST", 0x00, 0x00, "STR3 TEST", "STR4 TEST", 0x00, "STR5 TEST")
            );

        private TPWPacket GetThemeInfoPacket() => Factory.TPWPacketFactory.GenerateThemeInfoPacket(
                new Data.Structures.TPWThemeInfoStructure(0x00, "SPACE", "STR2 TEST", "STR3 TEST"),
                new Data.Structures.TPWThemeInfoStructure(0x01, "JUNGLE", "STR2 TEST", "STR3 TEST"),
                new Data.Structures.TPWThemeInfoStructure(0x02, "FANTASY", "STR2 TEST", "STR3 TEST"),
                new Data.Structures.TPWThemeInfoStructure(0x03, "HALLOW", "STR2 TEST", "STR3 TEST")
            );

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
