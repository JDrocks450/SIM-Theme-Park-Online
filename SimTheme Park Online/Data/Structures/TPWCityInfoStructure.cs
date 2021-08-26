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

            : base((uint)TPWConstants.TPWServerListType.CITY_INFO, CityID, CityName, Str2, X, Y, Z, Param2, Param3, LimitedInfoMode, Str3, AmountOfParks, Param6)
        {
            
        }
    }
}
