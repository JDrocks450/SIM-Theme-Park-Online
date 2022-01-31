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
        /// A constant DWORD value that is used when the server responds with a NON-NUMERIC park id, such as a string.
        /// </summary>
        public const uint ERROR_PARKID = 9090;
        /// <summary>
        /// The name of the park
        /// </summary>
        public TPWUnicodeString ParkName { get; }
        /// <summary>
        /// The park's id
        /// </summary>
        public uint ParkID { get; }
        /// <summary>
        /// True if ParkID is a valid DWORD
        /// </summary>
        public bool IsParkIDValid => ParkID != ERROR_PARKID;
        /// <summary>
        /// The number of players currently in this park
        /// </summary>
        public uint NumberOfPlayers { get; }
        /// <summary>
        /// When a non-standard server responds with a ParkID that isn't numeric,
        /// this value can be populated with the response.
        /// </summary>
        public ITPWBOSSSerializable ParkIDResponse { get; }

        public TPWChatRoomInfo(TPWUnicodeString ParkName, uint ParkID, uint NumberOfPlayers)
        {
            this.ParkName = ParkName;
            this.ParkID = ParkID;
            this.NumberOfPlayers = NumberOfPlayers;
        }

        public TPWChatRoomInfo(in List<TPWChatParsedData> ParsedData) :
            this((TPWUnicodeString)ParsedData[1].Data, 
                (ParsedData[3].IsDWORDConvertable) ? ParsedData[3].Data.ToDWORD() : (DWORD)ERROR_PARKID,
                ParsedData[4].Data.ToDWORD())
        {
            if (ParkID == ERROR_PARKID)
                ParkIDResponse = ParsedData[3].Data;
        }
    }
}
