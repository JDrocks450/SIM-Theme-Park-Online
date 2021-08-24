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

        public static TPWPacket GenerateThemeInfoPacket(string Theme1, string Theme2, string Theme3, out TPWServersideList List)
        {
            var packet = CreateServerResponse(0x044C, 00, 00, 0x0A);
            List = Create(0x08,
                new TPWServersideListDefinition(
                    (uint)0x00), // i4
                new TPWServersideListDefinition(
                    Theme1), //uz
                new TPWServersideListDefinition(
                    Theme2), //uz
                new TPWServersideListDefinition(
                    Theme3) //uz
            );            
            packet.Body = List.GetBytes();
            ExportToDisk(Path.Combine("Library\\City\\G_ThemeInfo.dat"), packet);
            return packet;
        }

        public static void GenerateGeneric(ref TPWPacket Packet, out Data.Templating.TPWDataTemplate Template, params TPWListStructure[] Lists)
        {
            var definitions = MergeAll(Lists.Select(x => x.List).ToArray());
            Template = definitions.GetTemplate();
            Packet.Body = definitions.GetBytes();
        }

        internal static TPWPacket GenerateCityInfoPacket(out Data.Templating.TPWDataTemplate Template, params TPWCityInfoStructure[] CityInfoLists)
        {
            var packet = CreateServerResponse(0x044C, 00, 00, 0x0E);
            GenerateGeneric(ref packet, out Template, CityInfoLists);
            return packet;
        }

        public static TPWPacket GenerateLogicalServerPacket(uint Param1, string Str1, uint Param2, string Str2, 
            uint Param3, uint Param4, string Str3, string Str4, uint Param5, string Str5)
        {
            var packet = CreateServerResponse(0x044C, 00, 00, 0x0B);
            var definitions = Create(
                0x07,
                Param1, Str1, Param2, Str2,
                Param3, Param4, Str3, Str4, 
                Param5, Str5
            );            
            packet.Body = definitions.GetBytes();
            return packet;
        }

        public static TPWPacket GenerateChatParkInfoPacket(params TPWChatParkInfoStructure[] ChatParks)
        {
            var packet = CreateServerResponse(0x044C, 00, 00, 0x0D);
            GenerateGeneric(ref packet, out _, ChatParks);
            return packet;
        }
    }
}
