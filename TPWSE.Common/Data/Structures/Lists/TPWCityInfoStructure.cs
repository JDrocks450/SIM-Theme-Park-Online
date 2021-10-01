using SimTheme_Park_Online.Data.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Data.Structures
{
    /// <summary>
    /// The structure for a CityInfo packet
    /// </summary>
    public class TPWCityInfoStructure : TPWListStructure
    {        
        private TPWCityInfoStructure()
        {

        }

        /// <summary>
        /// The Theme Park World Online City Info Packet structure.
        /// </summary>
        /// <param name="CityID">The ID of the city. Is DWORD, but should be treated as first 2 bytes only.</param>
        /// <param name="CityName">The name of this city</param>
        /// <param name="Str2">Unknown</param>
        /// <param name="X">Cartesian X position</param>
        /// <param name="Y">Cartesian Y position</param>
        /// <param name="Z">Cartesian Z position</param>
        /// <param name="Param2"></param>
        /// <param name="Param3"></param>
        /// <param name="LimitedInfoMode">Only shows extremely basic amounts of information about a park when this is set.</param>
        /// <param name="Str3"></param>
        /// <param name="AmountOfParks">The amount of parks in this city.</param>
        /// <param name="Param6"></param>
        public TPWCityInfoStructure(uint CityID,
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

            : base((uint)TPWConstants.TPWCityServerListType.CITY_INFO, CityID, CityName, Str2, X, Y, Z, Param2, Param3, LimitedInfoMode, Str3, AmountOfParks, Param6)
        {
            this.CityID = CityID;
            this.CityName = CityName;
            this.Str2 = Str2;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.LimitedInfoMode = LimitedInfoMode;
            this.AmountOfParks = AmountOfParks;
        }

        public uint CityID { get; private set; }
        public TPWUnicodeString CityName { get; private set; }
        public TPWUnicodeString Str2 { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public uint LimitedInfoMode { get; private set; }
        public uint AmountOfParks { get; private set; }

        public TPWCityInfo ToCityInfo() => TPWCityInfo.FromStructure(this);

        protected override void _fromList(TPWServersideList TList)
        {
            base._fromList(TList);
            CityID = (uint)List.Definitions.ElementAt(0).Data;
            CityName = (TPWUnicodeString)List.Definitions.ElementAt(1).Data;
            Str2 = (TPWUnicodeString)List.Definitions.ElementAt(2).Data;
            X = (float)List.Definitions.ElementAt(3).Data;
            Y = (float)List.Definitions.ElementAt(4).Data;
            Z = (float)List.Definitions.ElementAt(5).Data;
            LimitedInfoMode = (uint)List.Definitions.ElementAt(8).Data;
            AmountOfParks = (uint)List.Definitions.ElementAt(10).Data;
        }

        public static IEnumerable<TPWCityInfoStructure> FromPacket(TPWPacket Packet)
        {
            var lists = TPWServersideList.Parse(Packet);
            foreach (var list in lists)
            {
                TPWCityInfoStructure tPWListStructure = new TPWCityInfoStructure();
                tPWListStructure._fromList(list);
                yield return tPWListStructure;
            }
        }
    }
}
