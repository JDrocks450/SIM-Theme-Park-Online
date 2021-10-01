using SimTheme_Park_Online.Data.Packets;
using SimTheme_Park_Online.Data.Primitive;
using SimTheme_Park_Online.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Data
{
    public sealed class TPWChatRoomInfo
    {
        /// <summary>
        /// The name of the park
        /// </summary>
        public TPWUnicodeString ParkName { get; }
        /// <summary>
        /// The park's id
        /// </summary>
        public uint ParkID { get; }
        /// <summary>
        /// The number of players currently in this park
        /// </summary>
        public uint NumberOfPlayers { get; }

        public TPWChatRoomInfo(TPWUnicodeString ParkName, uint ParkID, uint NumberOfPlayers)
        {
            this.ParkName = ParkName;
            this.ParkID = ParkID;
            this.NumberOfPlayers = NumberOfPlayers;
        }

        public TPWChatRoomInfo(in List<TPWChatParsedData> ParsedData) :
            this((TPWUnicodeString)ParsedData[1].Data, ParsedData[3].Data.ToDWORD(), ParsedData[4].Data.ToDWORD())
        { }
    }
}
