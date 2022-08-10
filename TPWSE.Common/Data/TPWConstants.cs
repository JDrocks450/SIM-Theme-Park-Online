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
        public const uint TPWSE_QuazarClientMagicNumber = 01001;
        public const ushort TPWSendLimit = 256;

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
            /// User authentication was completed successfully.
            /// </summary>
            AUTH_SUCCESS = 0x09,
            /// <summary>
            /// The password was incorrect or otherwise cannot be validated at this time. 
            /// </summary>
            AUTH_ERROR = 0x01,
            /// <summary>
            /// TPW-SE is down for maintainance or is otherwise experiencing high-load or server error.
            /// </summary>
            GENERIC_LOGIN_ERROR = 0x02
        }

        /// <summary>
        /// The types of lists found on the <see cref="CityServer"/>
        /// </summary>
        public enum TPWCityServerListType : uint
        {
            /// <summary>
            /// Unused in TPW-SE
            /// </summary>
            LOGICAL_SERVERS = 0x07,
            /// <summary>
            /// The list of Themes recognized by the CityServer
            /// </summary>
            THEME_INFO = 0x08,
            /// <summary>
            /// The list of parks currently in a search result or city.
            /// </summary>
            PARKS_INFO = 0x02,
            /// <summary>
            /// The list of cities currently in the online globe.
            /// </summary>
            CITY_INFO = 0x01,
            /// <summary>
            /// Unused in TPW-SE
            /// </summary>
            RIDE_INFO = 0x09,
            /// <summary>
            /// A list of parks.
            /// </summary>
            SEARCH_RESULT = 0x06,
            /// <summary>
            /// A list of parks.
            /// </summary>
            TOP10_RESULT = 0x03
        }

        /// <summary>
        /// Channels for messages sent on the <see cref="ChatServer"/>
        /// </summary>
        public enum TPWChatServerChannel : uint
        {
            /// <summary>
            /// The channel that is to be used when the ChatServer indicates this packet was intended for this client only.
            /// <para>This channel is rarely used for anything but confirming a command was received.</para>
            /// </summary>
            PERSONAL = 0x012D,
            /// <summary>
            /// The channel that is shared between all clients.
            /// <para>This channel is used for most commands sent from the ChatServer.</para>
            /// </summary>
            PUBLIC = 0x012E
        }

        /// <summary>
        /// A command that is to be issued by Clients of a ChatServer.
        /// </summary>
        public enum TPWChatServerCommand : uint
        {
            /// <summary>
            /// Indicates that this request is to get info on an Online Room.
            /// <para>Standard Type: <see cref="Data.Packets.TPWChatRoomInfoPacket"/></para>
            /// </summary>
            RoomInfo = 0x7,             // 07    
            /// <summary>
            /// Indicates that this request is to create a player when joining an Online Room in TPW BOSS.
            /// <para>Standard formatting is: [PlayerName] [CustomerID] [Password] [PlayerID]</para>
            /// <para>See: ChatClient.AttemptCreatePlayer for implementation.</para>
            /// </summary>
            CreatePlayer = 0x0,         // 00
            /// <summary>
            /// Indicates that this request is to validate a <see cref="CreatePlayer"/> packet request.
            /// <para>Standard formatting is: No additional fields.</para>
            /// <para>See: ChatClient.AttemptCreatePlayer for implementation.</para>
            /// </summary>
            SetPlayerData = 0x1,        // 01
            /// <summary>
            /// Indicates that this request is to create a new Online Room (Park) to join.
            /// <para>Standard formatting is: [ParkName] 0 0 [ParkID]</para>
            /// <para>See: ChatClient.AttemptCreatePark for implementation.</para>
            /// </summary>
            CreatePark = 0x5,           // 05
            /// <summary>
            /// Indicates that this request is to signal the user is changing the position of his Avatar.
            /// <para>Standard formatting is: [X] [Y] [IsTeleporting 0 : 1]</para>
            /// <para>See: ChatClient.SendMoveCommand for implementation.</para>
            /// </summary>
            MovePlayer = 0x4,           // 04
            /// <summary>
            /// Indicates that this request is to update the user's AFK timer
            /// <para>Standard formatting is: No additional fields.</para>
            /// <para>See: ChatClient.SetAFK for implementation.</para>
            /// </summary>
            AFK = 0x10,                 // 16
            /// <summary>
            /// Indicates that this request is to update the range at which this user gets chat messages.
            /// <para>Standard formatting is: [Value]</para>
            /// <para>See: ChatClient.SetHearingRange for implementation.</para>
            /// </summary>
            HearingRange = 0x02,        // 02
            /// <summary>
            /// Indicates that this client would like an enumeration of all current players in the room.
            /// <para>Standard formatting is: No additional fields.</para>
            /// <para>See: ChatClient.RequestAllPlayers for implementation.</para>
            /// </summary>
            GetPlayers = 0x08,          // 08
            /// <summary>
            /// Indicates that this request is for the user sending a chat message.
            /// <para>Standard formatting is: [Message] [SenderName]</para>
            /// <para>See: ChatClient.SendChatMessage for implementation.</para>
            /// </summary>
            Chat = 0x11,                // 17
            /// <summary>
            /// Indicates that this request is for the user privately sending a chat message to one other person.
            /// <para>Standard formatting is: [RecipientName] [Message]</para>
            /// <para>See: ChatClient.SendTellMessage for implementation.</para>
            /// <para>It is up to the ChatServer's implementation to honor this system. SenderName is inferred by
            /// the server based on the ConnectionID of the sender client. As in, SenderName is whomever sent this 
            /// request, it is not supplied in this command like <see cref="Chat"/> allows.</para>
            /// </summary>
            Tell = 0x13,                // 19
            /// <summary>
            /// Immediately sets AFK to true for this user.
            /// </summary>
            ImAFK = 0xF,                // 15
            /// <summary>
            /// Indicates that this request is adding someone as a Friend.
            /// <para>Standard formatting is: [BuddyName]</para>
            /// <para>See: ChatClient.AddBuddy for implementation.</para>
            /// </summary>
            AddBuddy = 0x9,             // 09
            Shout = 0x16,               // 22
            EarmuffsOn = 0xB,           // 11
            EarmuffsOff = 0xC,          // 12
            Ignore = 0xD,               // 13
            LocatePlayer = 0x52,        // 82
            SystemAnnouncement = 0x4E,  // 78
            Emote = 0x12
        }
        /// <summary>
        /// A standard API-ResponseCode to be sent from a ChatServer implementation.
        /// </summary>
        public enum TPWChatServerResponseCodes : uint
        {            
            PARK_CREATE = 0x36,             // 54
            CREATE_PLAYER = 0x34,           // 52
            ERROR_BLACKMARKED = 0x33,       //
            ERROR_AUTH_FAILURE = 0x32,      //
            ERROR_CHILDLOCK = 0x31,
            ERROR_PARAMETER = 0x30,
            ERROR_PARK_FULL = 0x2D,
            ERROR_DUPLICATE_PLAYER = 0x2B,
            ERROR_MEMORY = 0x29,
            ERROR_GENERIC = 0x28,
            CHAT_RECEIVED = 0x43,           // 67
            CHAT_PLAYER_INFO = 0x27,        // 39
            IGNORE_RECEIVED = 0x10,         // 16
            CHAT_IGNORE = 0x3A,             // 58            
            /// <summary>
            /// Indicates that a player has just successfully moved their current position.
            /// <para>Standard formatting is: [PlayerName] [X] [Y] [IsTeleporting]</para>
            /// <para>This is handled by the ChatServer implementation, and is Broadcasted to all players,
            /// including the user who just successfully moved.</para>
            /// </summary>
            BOSS_CHAT_MOVE = 0x22,          // 34
            BOSS_CHAT_CONNECTED = 0x23,
            BOSS_CHAT_ENTERED_PARK = 0x25,
            BOSS_CHAT_GOTO = 0x52,
            BOSS_CHAT_GOTO_ALT = 0x13,
            BOSS_CHAT_IGNORE = 0xD,
            BOSS_CHAT_BLACKMARK = 0xE
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

        /// <summary>
        /// Dictates whether a movement request was for walking or teleporting.
        /// </summary>
        public enum TPWChatPlayerMovementTypes : uint
        {
            Walk = 0,
            Teleport = 1
        }
    }
}
