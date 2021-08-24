using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Data.Structures
{

    public class TPWCityInfoStructure : TPWListStructure
    {        
        public TPWCityInfoStructure(uint Param1, string CityName, string Str2, float X, 
            float Y, float Z, uint Param2, uint Param3, uint Param4, string Str3, uint AmountOfParks, uint Param6)
            : base(0x01, Param1, CityName, Str2, X, Y, Z, Param2, Param3, Param4, Str3, AmountOfParks, Param6)
        {
            
        }
    }
}
