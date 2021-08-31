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
    /// The structure of a ride info packet
    /// </summary>
    public class TPWRideInfoPacketStructure : TPWListStructure
    {
        public TPWRideInfoPacketStructure(
            TPWUnicodeString Str1, 
            TPWUnicodeString Str2, 
            TimeUuid BG, 
            uint Param1,
            uint Param2,
            TPWUnicodeString Str3,
            uint Param3,
            uint Param4,
            uint Param5)
            : base(
                      (uint)TPWConstants.TPWServerListType.RIDE_INFO,
                      Str1, Str2, BG, Param1, Param2, Str3, Param3, Param4, Param5
                  )
        {

        }
    }
}
