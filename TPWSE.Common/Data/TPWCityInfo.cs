using SimTheme_Park_Online.Data.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimTheme_Park_Online.Data
{
    [Serializable]
    public class TPWCityInfo
    {
        public uint CityID { get; set; }
        public TPWUnicodeString CityName { get; set; }
        public TPWUnicodeString Str2 { get;set; }
        public float X { get;set; }
        public float Y { get;set; }
        public float Z { get;set; }
        public uint Param2 { get;set; }
        public uint Param3 { get;set; }
        public uint LimitedInfoMode { get;set; }
        public TPWUnicodeString Str3 { get;set; }
        public uint AmountOfParks { get;set; }
        public uint Param6 { get;set; }

        /// <summary>
        /// Gets a list of park ids that belong to this City.
        /// </summary>
        public List<uint> Parks { get; set; } = new List<uint>();

        public TPWCityInfo( uint CityID,
                            TPWUnicodeString CityName,
                            TPWUnicodeString Str2,
                            float X,
                            float Y,
                            float Z,
                            uint Param2,
                            uint Param3,
                            uint LimitedInfoMode,
                            TPWUnicodeString Str3,
                            uint AmountOfParks,
                            uint Param6)
        {
            this.CityID = CityID;
            this.CityName = CityName;
            this.Str2 = Str2;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.Param2 = Param2;
            this.Param3 = Param3;
            this.LimitedInfoMode = LimitedInfoMode;
            this.Str3 = Str3;
            this.AmountOfParks = AmountOfParks;
            this.Param6 = Param6;
        }
        
        public void AddPark(TPWParkInfo Park)
        {
            Park.CityID = CityID;
            Parks.Add(Park.ParkID);
        }

        public Structures.TPWCityInfoStructure GetCityInfoResponseStructure()
        {
            return new Structures.TPWCityInfoStructure(
                CityID, CityName, Str2, X, Y, Z, Param2, Param3, LimitedInfoMode, Str3, AmountOfParks, Param6
            );
        }

        public Structures.TPWParkResponseStructure[] GetTop10ParksStructures(SimTheme_Park_Online.Database.IDatabaseInterface<uint, TPWParkInfo> DatabaseHandler)
        {
            List<Structures.TPWParkResponseStructure> parks = new List<Structures.TPWParkResponseStructure>();
            foreach (var parkID in Parks)
            {
                if (!DatabaseHandler.TryGetValue(parkID, out var ParkInfo))
                    continue;
                parks.Add(ParkInfo.GetParkInfoResponse(TPWConstants.TPWCityServerListType.TOP10_RESULT));
            }
            return parks.ToArray();
        }
    }
}