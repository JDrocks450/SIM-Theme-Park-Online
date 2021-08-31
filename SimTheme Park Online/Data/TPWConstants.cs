using Cassandra;
using SimTheme_Park_Online.Data.Primitive;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimTheme_Park_Online.Data
{
    public static class TPWConstants
    {
        public static readonly byte[] Bc_Header = new byte[2] { (byte)'B', (byte)'c' };
        public static readonly byte[] Bs_Header = new byte[2] { (byte)'B', (byte)'s' };

        public struct TPWServerListConstants
        {
            public const string I4 = "i4",
                                UZ = "uz",
                                SZ = "sz",
                                F4 = "f4",
                                BG = "bg",
                                DT = "dt",
                                XX = "xx";
            public static string GetDataTypeByType<T>(T Data)
            {
                if (Data is uint)
                    return I4;
                if (Data is float)
                    return F4;
                if (Data is TPWUnicodeString)
                    return UZ;
                if (Data is TimeUuid)
                    return BG;
                if (Data is TPWZeroTerminatedString)
                    return SZ;
                if (Data is TPWDTStruct)
                    return DT;
                return XX;
            }
        }

        public enum TPWServerListType : uint
        {
            LOGICAL_SERVERS = 0x07,
            THEME_INFO = 0x08,
            PARKS_INFO = 0x02,
            CITY_INFO = 0x01,
            RIDE_INFO = 0x09,
            SEARCH_RESULT = 0x06,
            TOP10_RESULT = 0x03
        }

        public enum TPWChatServerChannel : uint
        {
            CHAT = 0x012D,
            GLOBAL = 0x012E
        }

        public enum TPWChatServerResponseCode : uint
        {
            RoomInfo = 07
        }
    }
}
