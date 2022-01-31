using Cassandra;
using SimTheme_Park_Online.Data.Packets;
using SimTheme_Park_Online.Data.Primitive;
using SimTheme_Park_Online.Data.Structures;
using SimTheme_Park_Online.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Data
{
    [Serializable]
    public class TPWParkInfo : IDatabaseObject
    {
        /// <summary>
        /// The name of the person who published this park
        /// </summary>
        public TPWUnicodeString OwnerName
        {
            get; set;
        } = "System";
        /// <summary>
        /// The email of the publisher
        /// </summary>
        public TPWUnicodeString OwnerEmail
        {
            get; set;
        } = "admin@bullfrog.com";
        /// <summary>
        /// The name of this park
        /// </summary>
        public TPWUnicodeString ParkName
        {
            get; set;
        }
        /// <summary>
        /// The description of this park
        /// </summary>
        public TPWUnicodeString Description
        {
            get; set;
        }
        /// <summary>
        /// The park's ID, used to find it in a city.
        /// </summary>
        public uint ParkID
        {
            get; set;
        }
        /// <summary>
        /// The unique identifier of this park
        /// </summary>
        public TimeUuid Key
        {
            get; set;
        }
        = TimeUuid.NewId();
        /// <summary>
        /// The current theme this park uses
        /// </summary>
        public uint ThemeID
        {
            get; set;
        } = 0x02;
        /// <summary>
        /// The amount of times someone has visited this park
        /// </summary>
        public uint Visits
        {
            get; set;
        }
        /// <summary>
        /// The amount of votes this park has gotten
        /// </summary>
        public uint Votes
        {
            get; set;
        }
        /// <summary>
        /// The ID of the city this park belongs to
        /// </summary>
        public uint CityID
        {
            get; set;
        }
        /// <summary>
        /// The current position of this park in the global leaderboard
        /// </summary>
        public uint ChartPosition
        {
            get; set;
        }
        /// <summary>
        /// The date it was created, note that <see cref="Key"/> has this embedded, so this can be any value.
        /// </summary>
        public DateTime DateCreated => Key.GetDate().DateTime;
        public TPWZeroTerminatedString DownloadedRides
        {
            get; set;
        } = "Daphene";

        /// <summary>
        /// Creates a new instance of a <see cref="TPWParkInfo"/>, which represents a Park in SIM Theme Park
        /// </summary>
        public TPWParkInfo()
        {

        }

        /// <summary>
        /// Gets a server response packet - of structure: <see cref="TPWParkResponseStructure"/> - with park information. 
        /// </summary>
        /// <param name="ListType"></param>
        /// <returns></returns>
        public TPWParkResponseStructure GetParkInfoResponse(TPWConstants.TPWCityServerListType ListType) => new TPWParkResponseStructure(
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
            0x00,
            0x00,
            DownloadedRides,
            DateCreated,
            CityID,
            0x00,
            0x00,
            ChartPosition,
            0x00);

        public string GetInformation()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"TPWParkInfo Information");
            builder.AppendLine(ParkName);
            builder.AppendLine($"in City: {CityID}");
            builder.AppendLine(Key.ToString());
            builder.AppendLine(DateCreated.ToString());
            return builder.ToString();
        }

        public static TPWParkInfo FromStructure(TPWParkResponseStructure Structure)
        {
            return new TPWParkInfo()
            {
                ParkName = Structure.ParkName,
                ChartPosition = Structure.ChartPosition,
                CityID = Structure.CityID,
                Description = Structure.Description,
                DownloadedRides = Structure.DownloadedRides,
                OwnerEmail = Structure.Email,
                OwnerName = Structure.CreatedBy,
                ParkID = Structure.ParkID,
                ThemeID = Structure.ThemeID,
                Visits = Structure.NumVisits,
                Votes = Structure.Votes
            };
        }

        public override string ToString()
        {
            return $"[{GetInformation()}]";
        }
    }
}
