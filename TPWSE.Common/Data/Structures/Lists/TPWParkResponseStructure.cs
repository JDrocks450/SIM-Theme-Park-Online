using Cassandra;
using SimTheme_Park_Online.Data.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Data.Structures
{
    /// <summary>
    /// The structure for a ChatInfo packet
    /// </summary>
    public class TPWParkResponseStructure : TPWListStructure
    {
        public TPWUnicodeString CreatedBy { get; private set; }
        public TPWUnicodeString Email { get; private set; }
        public uint ParkID { get; private set; }
        public TimeUuid Guid { get; private set; }
        public TPWUnicodeString ParkName { get; private set; }
        public TPWUnicodeString Description { get; private set; }
        public uint NumVisits { get; private set; }
        public uint Votes { get; private set; }
        public uint ThemeID { get; private set; }
        public uint PARAM1 { get; private set; }
        public uint PARAM2 { get; private set; }
        public TPWZeroTerminatedString DownloadedRides { get; private set; }
        public TPWDTStruct DT { get; private set; }
        public uint CityID { get; private set; }
        public uint PARAM4 { get; private set; }
        public uint ChartPosition { get; private set; }
        public uint PARAM6 { get; private set; }
        public uint InstantAction { get; private set; }

        private TPWParkResponseStructure()
        {

        }

        /// <summary>
        /// Creates a ChatParkInfo list
        /// </summary>
        /// <param name="CreatedBy">The Username of the creator</param>
        /// <param name="Email">The email of the creator</param>
        /// <param name="ParkID">The ID of this park</param>
        /// <param name="Guid">The unique identifer of this park</param>
        /// <param name="ParkName">The name of the park</param>
        /// <param name="Description">The description of the park</param>
        /// <param name="NumVisits">The number of times its been visited</param>
        /// <param name="Votes">The number of votes its gotten</param>
        /// <param name="ThemeID">The theme of the park</param>
        /// <param name="PARAM1"></param>
        /// <param name="PARAM2"></param>
        /// <param name="SZ"></param>
        /// <param name="DT">DateTime when park was created</param>
        /// <param name="CityID">The city containing this park</param>
        /// <param name="PARAM4"></param>
        /// <param name="ChartPosition"></param>
        /// <param name="PARAM6"></param>
        /// <param name="InstantAction"></param>
        public TPWParkResponseStructure(TPWConstants.TPWCityServerListType ListType,
                                        TPWUnicodeString CreatedBy,
                                        TPWUnicodeString Email,
                                        uint ParkID,
                                        TimeUuid Guid,
                                        TPWUnicodeString ParkName,
                                        TPWUnicodeString Description,
                                        uint NumVisits,
                                        uint Votes,
                                        uint ThemeID,
                                        uint PARAM1,
                                        uint PARAM2,
                                        TPWZeroTerminatedString DownloadedRides,
                                        TPWDTStruct DT,
                                        uint CityID,
                                        uint PARAM4,
                                        uint ChartPosition,
                                        uint PARAM6,
                                        uint InstantAction)
            : base((uint)ListType,
                   CreatedBy,
                   Email,
                   ParkID,
                   Guid,
                   ParkName,
                   Description,
                   NumVisits,
                   Votes,
                   ThemeID,
                   PARAM1,
                   PARAM2,
                   DownloadedRides,
                   DT,
                   CityID,
                   PARAM4,
                   ChartPosition,
                   PARAM6,
                   InstantAction)
        {
            this.CreatedBy = CreatedBy;
            this.Email = Email;
            this.ParkID = ParkID;
            this.Guid = Guid;
            this.ParkName = ParkName;
            this.Description = Description;
            this.NumVisits = NumVisits;
            this.Votes = Votes;
            this.ThemeID = ThemeID;
            this.PARAM1 = PARAM1;
            this.PARAM2 = PARAM2;
            this.DownloadedRides = DownloadedRides;
            this.DT = DT;
            this.CityID = CityID;
            this.PARAM4 = PARAM4;
            this.ChartPosition = ChartPosition;
            this.PARAM6 = PARAM6;
            this.InstantAction = InstantAction;
        }

        public TPWParkInfo ToParkInfo() => TPWParkInfo.FromStructure(this);

        protected override void _fromList(TPWServersideList TList)
        {
            base._fromList(TList);
            CreatedBy = (TPWUnicodeString)List.Definitions.ElementAt(0).Data;
            Email = (TPWUnicodeString)List.Definitions.ElementAt(1).Data;
            ParkID = (uint)List.Definitions.ElementAt(2).Data;
            Guid = (TimeUuid)List.Definitions.ElementAt(3).Data;
            ParkName = (TPWUnicodeString)List.Definitions.ElementAt(4).Data;
            Description = (TPWUnicodeString)List.Definitions.ElementAt(5).Data;
            NumVisits = (uint)List.Definitions.ElementAt(6).Data;
            Votes = (uint)List.Definitions.ElementAt(7).Data;
            ThemeID = (uint)List.Definitions.ElementAt(8).Data;
            PARAM1 = (uint)List.Definitions.ElementAt(9).Data;
            PARAM2 = (uint)List.Definitions.ElementAt(10).Data;
            DownloadedRides = (TPWZeroTerminatedString)List.Definitions.ElementAt(11).Data;
            DT = (TPWDTStruct)List.Definitions.ElementAt(12).Data;
            CityID = (uint)List.Definitions.ElementAt(13).Data;
            PARAM4 = (uint)List.Definitions.ElementAt(14).Data;
            ChartPosition = (uint)List.Definitions.ElementAt(15).Data;
            PARAM6 = (uint)List.Definitions.ElementAt(16).Data;
            InstantAction = (uint)List.Definitions.ElementAt(17).Data;
        }

        public static IEnumerable<TPWParkResponseStructure> FromPacket(TPWPacket Packet)
        {
            var lists = TPWServersideList.Parse(Packet);
            foreach (var list in lists)
            {
                TPWParkResponseStructure tPWListStructure = new TPWParkResponseStructure();
                tPWListStructure._fromList(list);
                yield return tPWListStructure;
            }
        }
    }
}
