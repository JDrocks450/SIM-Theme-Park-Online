using QuazarAPI;
using QuazarAPI.Networking.Data;
using QuazarAPI.Networking.Standard;
using SimTheme_Park_Online.Data;
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

        private Dictionary<uint, TPWParkInfo> _parks = new Dictionary<uint, TPWParkInfo>();
        private Dictionary<uint, TPWCityInfo> _cities = new Dictionary<uint, TPWCityInfo>();

        public IEnumerable<TPWParkInfo> Parks => _parks.Values;

        /// <summary>
        /// Returns a <see cref="TPWParkInfo"/> representing the park with this ID.
        /// </summary>
        /// <param name="ParkID"></param>
        /// <returns></returns>
        public bool TryGetParkByID(uint ParkID, out TPWParkInfo Park) => _parks.TryGetValue(ParkID, out Park);

        private TPWParkResponseStructure[] GetParkResponses(Data.TPWConstants.TPWServerListType Type) => GetParkResponses(Type, _parks.Values.ToArray());  
        private TPWParkResponseStructure[] GetParkResponses(Data.TPWConstants.TPWServerListType Type, params TPWParkInfo[] parks) => parks.Select(x => x.GetParkInfoResponse(Type)).ToArray();

        public CityServer(int port) : base("CityServer", port, SIMThemeParkWaypoints.CityServer)
        {
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
            _parks.Add(16, new TPWParkInfo()
            {
                OwnerName = "Bisquick",
                OwnerEmail = "admin@bullfrog.com",
                ParkID = 16,
                ParkName = "Bisquick's Testing Zone",
                Description = "This park is for testing purposes. Thanks for tuning in! The codebase behind this breakthrough is Quazar.TPW-SE, " +
                    "an API for interacting with an unmodified, SIM Theme Park 1.0 and 2.0 installation. - Jeremy, Bisquick",
                Visits = 0x10,
                Votes = 0x05,
                CityID = 0x0A,
                ChartPosition = 0x01,
                ThemeID = 0x01,
                InternalName = "Daphene"
            });
            _parks.Add(21, new TPWParkInfo()
            {
                OwnerName = "admin",
                OwnerEmail = "admin@bullfrog.com",
                ParkID = 21,
                ParkName = "Test Center 3",
                Description = "Not much is known about this park, but most people who enter it never" +
                " say much about what they saw...",
                Visits = 0x03,
                Votes = 0x00,
                CityID = 0x0B,
                ChartPosition = 0x02,
                ThemeID = 0x03,
                InternalName = "Apollo"
            });
            _parks.Add(24, new TPWParkInfo()
            {
                OwnerName = "TwistyT",
                OwnerEmail = "admin@bullfrog.com",
                ParkID = 24,
                ParkName = "Radical Twister",
                Description = "Founded on the idea that the only good theme park ride is one with serious whiplash," +
                " this park really does test the limits of the human body! Riding these rides is not recommended.",
                Visits = 0x20,
                Votes = 0x50,
                CityID = 0x0C,
                ChartPosition = 0x03,
                ThemeID = 0x02,
                InternalName = "Beta"
            });
            _parks.Add(32, new TPWParkInfo()
            {
                OwnerName = "Bisquick",
                OwnerEmail = "admin@bullfrog.com",
                ParkID = 32,
                ParkName = "Oceanic Adventures",
                Description = "Who doesn't like adventures in oceans? Most people. That's why we're different! " +
                "We take the oceans into space to create a life-changing experience!",
                Visits = 0x00,
                Votes = 0x00,
                CityID = 0x0D,
                ChartPosition = 0x05,
                ThemeID = 0x00,
                InternalName = "Delta"
            });
            _parks.Add(48, new TPWParkInfo()
            {
                OwnerName = "admin",
                OwnerEmail = "admin@bullfrog.com",
                ParkID = 48,
                ParkName = "Burrito Village",
                Description = "Our park prides itself on not having rides, but instead only food stands. We only sell " +
                "burritos. Before you ask, yes we have bathrooms - is there a fee? Absolutely.",
                Visits = 0x5C,
                Votes = 0x03,
                CityID = 0x0E,
                ChartPosition = 0x03,
                ThemeID = 0x01,
                InternalName = "Burrito"
            });

            foreach (var city in _cities)
                foreach (var park in _parks.Values)
                    if (park.CityID == city.Key)
                        city.Value.AddPark(park);
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
            var rideInfo = new Data.Structures.TPWRideInfoPacketStructure("RIDENAME", "SEC STR", new byte[] { 00, 00, 00, 00 }, 0x01, 0x00, "TESTSTR3", 0x02, 0x03, 0x04);
            rideInfo.List.IsEmptyList = false;
            var packet = Factory.TPWPacketFactory.GenerateRideInfoPacket(rideInfo);
            Factory.TPWPacketFactory.GenerateGeneric(ref packet, rideInfo.List);
            return packet;
        }

        private TPWPacket GetSearchPacket(TPWUnicodeString Username)
        {
            var results = _parks.Values.Where(x => x.OwnerName.String == Username.String);
            if (!results.Any())
                results = _parks.Values.Where(x => x.OwnerName.String.ToLower().StartsWith(Username.String.ToLower()));
            var packet = Factory.TPWPacketFactory.GenerateCityResponsePacket(GetParkResponses(Data.TPWConstants.TPWServerListType.SEARCH_RESULT, results.ToArray()));
            packet.PacketQueue = 0x0F;
            return packet;
        }
        private TPWPacket GetTop10Packet(TPWCityInfo City)
        {
            var packet = Factory.TPWPacketFactory.GenerateCityResponsePacket(City.GetTop10ParksStructures());
            packet.PacketQueue = 0x0E;
            return packet;
        }
        private TPWPacket GetChatInfoPacket() => Factory.TPWPacketFactory.GenerateCityResponsePacket(GetParkResponses(Data.TPWConstants.TPWServerListType.PARKS_INFO));

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
                char c = BodyText[index];
                string value = "";
                while(c != 00)
                {                    
                    c = BodyText[index];
                    value += c;
                    index++;
                }
                byte[] array = new byte[value.Length / 2];
                for(int i = 0; i < array.Length; i++)
                {
                    string str = value.Substring(0, 2);
                    array[i] = Convert.ToByte(str, 16);
                    value = value.Remove(0, 2);
                }
                return Encoding.Unicode.GetString(array);
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
