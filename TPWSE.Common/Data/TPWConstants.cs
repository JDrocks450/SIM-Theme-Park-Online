using Cassandra;
using SimTheme_Park_Online.Data.Primitive;

namespace SimTheme_Park_Online.Data
{
    /// <summary>
    /// A reference to the constants found throughout SIM THEME PARK ONLINE
    /// <para>Created by Jeremy Glazebrook for TPW-SE</para>
    /// </summary>
    public static class TPWConstants
    {
        public static readonly byte[] Bc_Header = new byte[2] { (byte)'B', (byte)'c' };
        public static readonly byte[] Bs_Header = new byte[2] { (byte)'B', (byte)'s' };

        public const ushort TPWSendLimit = 400;

        /// <summary>
        /// Constants for transmissions on the <see cref="CityServer"/>
        /// </summary>
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

        public enum TPWLoginMsgCodes : ushort
        {
            /// <summary>
            /// Login Success code
            /// </summary>
            SUCCESS = 0x09,
            /// <summary>
            /// Error code 01
            /// </summary>
            ERROR_1 = 0x01,
            /// <summary>
            /// Error code 02
            /// </summary>
            ERROR_2 = 0x02
        }

        /// <summary>
        /// The types of lists found on the <see cref="CityServer"/>
        /// </summary>
        public enum TPWCityServerListType : uint
        {
            LOGICAL_SERVERS = 0x07,
            THEME_INFO = 0x08,
            PARKS_INFO = 0x02,
            CITY_INFO = 0x01,
            RIDE_INFO = 0x09,
            SEARCH_RESULT = 0x06,
            TOP10_RESULT = 0x03
        }

        /// <summary>
        /// Channels for messages sent on the <see cref="ChatServer"/>
        /// </summary>
        public enum TPWChatServerChannel : uint
        {
            CHAT = 0x012D,
            GLOBAL = 0x012E
        }

        /// <summary>
        /// Response codes for <see cref="ChatServer"/> packets
        /// </summary>
        public enum TPWChatServerCommand : uint
        {
            /// <summary>
            /// This is already implemented here: <see cref="Data.Packets.TPWChatRoomInfoPacket"/>
            /// </summary>
            RoomInfo = 07,
            CreatePlayer = 00,
            CreatePlayerSuccess = 0x34,
            SetPlayerData = 01,
            CreatePark = 05,            
            MovePlayer = 04,
            AFK = 0x10,
            HearingRange = 0x02,
            GetPlayers = 0x08,
            EnumPlayer = 0x27,
            CreateParkSuccess = 0x36,
            ServerChatConfirmation = 0x43,
            Chat = 0x11,
            Tell = 0x13,
            ImAFK = 15,
            AddBuddy = 09,
            Shout = 22,
            EarmuffsOn = 11,
            EarmuffsOff = 12,
        }

        /// <summary>
        /// Codes for data types packaged into packets sent/received on the <see cref="ChatServer"/>
        /// </summary>
        public enum TPWChatTypeCodes : byte
        {
            NONE = 0x00,
            /// <summary>
            /// Represents data 2 bytes in length -- usually Unicode
            /// </summary>
            UNI = 0x02,
            /// <summary>
            /// Represents data that is 1 byte in length -- usually ASCII-encoded numbers.
            /// </summary>
            ASCII = 0x01
        }

        public enum TPWChatPlayerMovementTypes
        {
            None,
            Walk,
            Teleport
        }
    }
}
