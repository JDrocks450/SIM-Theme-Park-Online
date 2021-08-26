using SimTheme_Park_Online.Data.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Data.Structures
{
    /// <summary>
    /// The structure of a ThemeInfo packet
    /// <para><c>i4,uz,uz,uz</c></para>
    /// </summary>
    public class TPWThemeInfoStructure : TPWListStructure
    {
        public TPWThemeInfoStructure(uint ThemeID,
                                     TPWUnicodeString Name,
                                     TPWUnicodeString Str2,
                                     TPWUnicodeString Str3)
            : base((uint)TPWConstants.TPWServerListType.THEME_INFO, ThemeID, Name, Str2, Str3)
        {

        }
    }
}
