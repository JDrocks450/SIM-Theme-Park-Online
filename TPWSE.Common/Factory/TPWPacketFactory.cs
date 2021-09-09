using QuazarAPI;
using SimTheme_Park_Online.Data;
using SimTheme_Park_Online.Data.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SimTheme_Park_Online.Data.TPWServersideList;

namespace SimTheme_Park_Online.Factory
{
    public static class TPWPacketFactory
    {
        public static void ExportToDisk(string FileName, TPWPacket packet)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FileName));
                using (var fs = File.Create(FileName))
                    packet.Write(fs);
            }
            catch(Exception e)
            {
                QConsole.WriteLine("TPWPacketFactory.cs", "Error while writing to disk: " + e.Message);
            }
        }

        public static TPWPacket CreateServerResponse(ushort MsgType, ushort Param2, uint Param3, uint Queue)
        {
            return new TPWPacket()
            {
                ResponseCode = TPWConstants.Bs_Header,
                Param2 = Param2,
                Param3 = Param3,
                MsgType = MsgType,      
                PacketQueue = Queue
            };
        }

        public static TPWPacket GenerateThemeInfoPacket(params TPWThemeInfoStructure[] Themes)
        {
            var packet = CreateServerResponse(0x044C, 00, 00, 0x0A);
            GenerateGeneric(ref packet, Themes);
            ExportToDisk(Path.Combine("Library\\City\\Generated\\G_ThemeInfo.dat"), packet);
            return packet;
        }

        public static void GenerateGeneric(ref TPWPacket Packet, params TPWListStructure[] Lists) => 
            GenerateGeneric(ref Packet, MergeAll(Lists.Select(x => x.List).ToArray()));

        public static void GenerateGeneric(ref TPWPacket Packet, TPWServersideList List)
        {
            var definitions = List;
            //Packet.SetTemplate(definitions?.GetTemplate());
            definitions.CreatePacket(ref Packet);   
        }

        public static TPWPacket GenerateRideInfoPacket(params TPWRideInfoPacketStructure[] Rides)
        {
            var packet = CreateServerResponse(0x044C, 00, 00, 0x0C);
            GenerateGeneric(ref packet, Rides);
            return packet;
        }

        public static TPWPacket GenerateCityInfoPacket(params TPWCityInfoStructure[] CityInfoLists)
        {
            var packet = CreateServerResponse(0x044C, 00, 00, 0x0E);
            GenerateGeneric(ref packet, CityInfoLists);
            return packet;
        }

        public static TPWPacket GenerateLogicalServerPacket(params TPWLogicalServerStructure[] LogicalServers)
        {
            var packet = CreateServerResponse(0x044C, 00, 00, 0x0B);
            GenerateGeneric(ref packet, LogicalServers);
            return packet;
        }

        public static TPWPacket GenerateCityResponsePacket(params TPWParkResponseStructure[] ChatParks)
        {
            var packet = CreateServerResponse(0x044C, 00, 00, 0x0D);
            GenerateGeneric(ref packet, ChatParks);
            return packet;
        }
    }
}
