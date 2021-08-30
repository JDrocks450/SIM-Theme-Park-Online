using SimTheme_Park_Online.Data.Primitive;
using SimTheme_Park_Online.Data.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Data
{
    [Serializable]
    public class TPWParkInfo
    {
        public TPWUnicodeString OwnerName
        {
            get; set;
        } = "System";
        public TPWUnicodeString OwnerEmail
        {
            get; set;
        } = "admin@bullfrog.com";
        public TPWUnicodeString ParkName
        {
            get; set;
        }
        public TPWUnicodeString Description
        {
            get; set;
        }
        public uint ParkID
        {
            get; set;
        }
        public byte[] Key
        {
            get; set;
        } = new byte[]
               {
                        01, 02, 00, 03,
                        04, 05,
                        06, 07,
                        08, 09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
               };
        public uint ThemeID
        {
            get; set;
        } = 0x02;
        public uint Visits
        {
            get; set;
        }
        public uint Votes
        {
            get; set;
        }
        public uint CityID
        {
            get; set;
        }
        public uint ChartPosition
        {
            get; set;
        }
        public TPWDTStruct DateCreated
        {
            get; set;
        } = new TPWDTStruct(0x2021, 0x0008, 0x0017);
        public TPWZeroTerminatedString InternalName
        {
            get; set;
        } = "Daphene";

        public TPWParkInfo()
        {

        }

        public TPWChatRoomInfoPacket GetRoomInfoPacket(uint PlayersInPark = 0)
        {
            return new TPWChatRoomInfoPacket(ParkName, ParkID, PlayersInPark, 1024, 30);
        }

        public TPWParkResponseStructure GetParkInfoResponse(TPWConstants.TPWServerListType ListType) => new TPWParkResponseStructure(
            ListType,
            OwnerName,
            OwnerEmail,
            ParkID,
            Key,
            ParkName,
            Description,
            Visits,
            Votes,
            ThemeID,
            0x0A,
            0x01,
            InternalName,
            DateCreated,
            CityID,
            0x01,
            ChartPosition,
            0x0D,
            0x00);
    }
}
