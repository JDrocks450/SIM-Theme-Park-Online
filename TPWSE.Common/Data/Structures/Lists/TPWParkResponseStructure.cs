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

        }
    }
}
