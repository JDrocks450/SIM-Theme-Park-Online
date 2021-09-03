using SimTheme_Park_Online.Data.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Data.Structures
{
    /// <summary>
    /// The structure for the logical server list
    /// </summary>
    public class TPWLogicalServerStructure : TPWListStructure
    {
        public TPWLogicalServerStructure(uint Param1,
                                         TPWUnicodeString Str1,
                                         uint Param2,
                                         TPWUnicodeString Str2,
                                         uint Param3,
                                         uint Param4,
                                         TPWUnicodeString Str3,
                                         TPWUnicodeString Str4,
                                         uint Param5,
                                         TPWUnicodeString Str5)

            : base((uint)TPWConstants.TPWCityServerListType.LOGICAL_SERVERS,
                   Param1,
                   Str1,
                   Param2,
                   Str2,
                   Param3,
                   Param4,
                   Str3,
                   Str4,
                   Param5,
                   Str5)
        {

        }
    }
}
