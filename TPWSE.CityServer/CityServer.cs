using Cassandra;
using QuazarAPI;
using QuazarAPI.Networking.Data;
using QuazarAPI.Networking.Standard;
using SimTheme_Park_Online.Data;
using SimTheme_Park_Online.Data.Primitive;
using SimTheme_Park_Online.Data.Structures;
using SimTheme_Park_Online.Database;
using SimTheme_Park_Online.Util;
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

        private Dictionary<uint, TPWCityInfo> _cities = new Dictionary<uint, TPWCityInfo>();
        private readonly IDatabaseInterface<uint, TPWParkInfo> parksDatabase;

        /// <summary>
        /// Returns a <see cref="TPWParkInfo"/> representing the park with this ID.
        /// </summary>
        /// <param name="ParkID"></param>
        /// <returns></returns>
        public bool TryGetParkByID(uint ParkID, out TPWParkInfo Park) => parksDatabase.TryGetValue(ParkID, out Park);

        private TPWParkResponseStructure[] GetParkResponses(Data.TPWConstants.TPWCityServerListType Type) => GetParkResponses(Type, parksDatabase.GetAllData().ToArray());  
        private TPWParkResponseStructure[] GetParkResponses(Data.TPWConstants.TPWCityServerListType Type, params TPWParkInfo[] parks) => parks.Select(x => x.GetParkInfoResponse(Type)).ToArray();

        public bool AddPark(TPWParkInfo ParkInfo) => parksDatabase.AddData(ParkInfo.ParkID, ParkInfo);

        public CityServer(int port, IDatabaseInterface<uint, TPWParkInfo> ParksDatabase) : base("CityServer", port, SIMThemeParkWaypoints.CityServer)
        {
            parksDatabase = ParksDatabase;

            //city locations
            Vector3 Loc1 = Util.SphericalCoordinateConverter.ToCartesian(120, 0, 0);
            Vector3 Loc2 = Util.SphericalCoordinateConverter.ToCartesian(120, 90, 20);
            Vector3 Loc3 = Util.SphericalCoordinateConverter.ToCartesian(120, 120, 90);

            //cities
            _cities.Add(0x0A, new TPWCityInfo(0x0000000A, "North Pole", "bullfrog", 2.140f, 0.0f, 119.98f, 0x0B, 100, 0x00, "bullfrog", 0x01, 0x0F));
            _cities.Add(0x0B, new TPWCityInfo(0x0000000B, "Bloaty Land", "Bisquick", 100, 70, 20, 0x20, 0x03, 0x05, "bullfrog", 0x01, 0x10));
            _cities.Add(0x0C, new TPWCityInfo(0x0000000C, "Radical Jungle", "System", Loc2.X, Loc2.Y, Loc2.Z, 0x20, 0x03, 0x05, "bullfrog", 0x01, 0x10));
            _cities.Add(0x0D, new TPWCityInfo(0x0000000D, "Sector 7", "System", Loc1.X, Loc1.Y, Loc1.Z, 0x20, 0x03, 0x05, "bullfrog", 0x01, 0x10));
            _cities.Add(0x0E, new TPWCityInfo(0x0000000E, "Charvatia", "System", Loc3.X, Loc3.Y, Loc3.Z, 0x20, 0x03, 0x05, "bullfrog", 0x01, 0x10));
            
            //parks                        

            foreach (var city in _cities)
                foreach (var park in parksDatabase.GetAllData())
                {
                    if (park.CityID == city.Key)
                        city.Value.AddPark(park);
                    QConsole.WriteLine("CityServer", park.GetInformation());
                }            
        }

        protected override void OnIncomingPacket(uint ID, TPWPacket Data)
        {
            if (Data.PacketQueue == 13)
            {
                Send(ID,
                    GetThemeInfoPacket(),
                    GetLogicalServerPacket(),
                    GetRideInfoPacket(),
                    GetChatInfoPacket()
                );
            }
            else if (Data.PacketQueue > 13)
            {
                if (_tryGenerateResponse(Data, out var outgoing))
                {
                    outgoing.PacketQueue = Data.PacketQueue;
                    Send(ID, outgoing);
                }
                else if (Data.PacketQueue == 14)
                    Send(ID, GetCityInfoPacket());
            }
            else
            {
                ;
            }
        }

        private TPWPacket GetRideInfoPacket()
        {
            var rideInfo = new Data.Structures.TPWRideInfoPacketStructure("RIDENAME", "SEC STR", TimeUuid.NewId(), 0x01, 0x00, "TESTSTR3", 0x02, 0x03, 0x04);
            rideInfo.List.IsEmptyList = false;
            var packet = Factory.TPWPacketFactory.GenerateRideInfoPacket(rideInfo);
            Factory.TPWPacketFactory.GenerateGeneric(ref packet, rideInfo.List);
            Factory.TPWPacketFactory.ExportToDisk("Library\\City\\Generated\\G_RideInfo", packet);
            return packet;
        }

        private TPWPacket GetSearchPacket(TPWUnicodeString Username)
        {
            var results = parksDatabase.Search(x => x.OwnerName.String == Username.String);
            if (!results.Any())
                results = parksDatabase.Search(x => x.OwnerName.String.ToLower().StartsWith(Username.String.ToLower()));
            var packet = Factory.TPWPacketFactory.GenerateCityResponsePacket(GetParkResponses(Data.TPWConstants.TPWCityServerListType.SEARCH_RESULT, results.ToArray()));
            packet.PacketQueue = 0x0F;
             Factory.TPWPacketFactory.ExportToDisk("Library\\City\\Generated\\G_SearchPacket", packet);
            return packet;
        }
        public TPWPacket GetTop10Packet(uint CityID) => GetTop10Packet(_cities[CityID]);
        internal TPWPacket GetTop10Packet(TPWCityInfo City)
        {
            var packet = Factory.TPWPacketFactory.GenerateCityResponsePacket(City.GetTop10ParksStructures(parksDatabase));
            packet.PacketQueue = 0x0E;
            Factory.TPWPacketFactory.ExportToDisk("Library\\City\\Generated\\G_Top10", packet);
            return packet;
        }
        private TPWPacket GetChatInfoPacket() => Factory.TPWPacketFactory.GenerateCityResponsePacket(GetParkResponses(Data.TPWConstants.TPWCityServerListType.PARKS_INFO));

        public TPWPacket GetCityInfoPacket() => Factory.TPWPacketFactory.GenerateCityInfoPacket(
                _cities.Select(x => x.Value.GetCityInfoResponseStructure()).ToArray()
        );

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

        private bool _tryGenerateResponse(TPWPacket incoming, out TPWPacket outgoing)
        {
            string BodyText = Encoding.ASCII.GetString(incoming.Body);
            uint GetInt(string FieldName)
            {
                int index = BodyText.IndexOf(FieldName) + FieldName.Length;
                char c = BodyText[index];
                string value = "";
                while(c != 00)
                {                    
                    c = BodyText[index];
                    value += c;
                    index++;
                }
                return uint.Parse(value);
            }        
            string GetString(string FieldName)
            {
                int index = BodyText.IndexOf(FieldName) + FieldName.Length + 2;
                incoming.SetPosition(index);
                string value = incoming.ReadBodyTerminatedString();
                return HexConverter.ByteStringToUnicode(value);
            }
            if(BodyText.Contains("CITYID="))
            {
                uint CityID = GetInt("CITYID=");
                if (CityID == 1)
                {
                    outgoing = GetCityInfoPacket();
                    return true;
                }
                if (_cities.TryGetValue(CityID, out TPWCityInfo City))
                {
                    outgoing = GetTop10Packet(City);
                    return true;
                }
            }
            else if (BodyText.Contains("PARKID="))
            {
                uint ParkID = GetInt("PARKID=");

            }
            else if (BodyText.Contains("OWNERID="))
            {

            }
            else if (BodyText.Contains("OWNERNAME="))
            {
                TPWUnicodeString userName = GetString("OWNERNAME=");
                outgoing = GetSearchPacket(userName);
                return true;
            }
            outgoing = null;
            return false;
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
            
        }
    }
}
