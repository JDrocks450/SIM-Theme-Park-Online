using QuazarAPI;
using QuazarAPI.Networking.Standard;
using SimTheme_Park_Online.Data;
using SimTheme_Park_Online.Data.Packets;
using SimTheme_Park_Online.Data.Primitive;
using SimTheme_Park_Online.Data.Structures;
using SimTheme_Park_Online.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TPWSE.ChatServer.Multiplayer;

namespace SimTheme_Park_Online
{
    public class ChatServer : QuazarServer
    {
        private const int CHAT_RECV_AMOUNT = 1024;

        private readonly Parsers.TPWChatPacketParser ChatPacketParser = new Parsers.TPWChatPacketParser();
        private readonly IDatabase<uint, TPWPlayerInfo> playerDatabase;
        private readonly IDatabase<uint, TPWParkInfo> ParksDatabase;
        private readonly OnlineRoomManager RoomManager = new OnlineRoomManager();

        /// <summary>
        /// ID, PlayerInfo
        /// </summary>
        private Dictionary<uint, TPWPlayerInfo> JoiningPlayers = new Dictionary<uint, TPWPlayerInfo>();
        /// <summary>
        /// ID, RoomInfo
        /// </summary>
        private Dictionary<uint, TPWOnlineRoom> PlayersInRooms = new Dictionary<uint, TPWOnlineRoom>();

        public ChatServer(int port, IDatabase<uint, TPWPlayerInfo> PlayerDatabase,
            IDatabase<uint, TPWParkInfo> ParksDatabase) : base("ChatServer", port, SIMThemeParkWaypoints.ChatServer)
        {
            playerDatabase = PlayerDatabase;
            this.ParksDatabase = ParksDatabase;
            SendAmount = ReceiveAmount = CHAT_RECV_AMOUNT;
            init();
        }

        private void init()
        {
            QConsole.WriteLine("ChatServer", "Online init starting...");
            var chatellites = ParksDatabase.GetSpecialListEntries("Chatellites");
            foreach (var chatellite in chatellites)
                RoomManager.CreateRoom(chatellite.ParkID);
            QConsole.WriteLine("ChatServer", "Online init success...");
        }

        private TPWChatPacket GetAFKPacket()
        {
            var packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.AFK,
                            (DWORD)0);
            packet.MessageType = 0x012D;
            return packet;
        }

        protected override void OnIncomingPacket(uint ID, TPWPacket Data)
        {
            ChatPacketParser.TryParse(Data, out List<Parsers.TPWChatParsedData> ParsedData);
            uint MessageType = ParsedData[0].Data.ToDWORD();
            bool closerNecessary = true;

            switch ((TPWConstants.TPWChatServerCommand)MessageType)
            {
                case TPWConstants.TPWChatServerCommand.RoomInfo:
                    {
                        QConsole.WriteLine("ChatServer", "Client is requesting RoomInfo...");
                        DWORD parkID = ParsedData[1].Data.ToDWORD();                            
                        if (RoomManager.TryGetRoom(parkID, out var Room))
                        {
                            QConsole.WriteLine("ChatServer", $"Valid OnlineRoom requested, returning RoomInfo for {parkID}...");
                            var packet = Room.GetRoomInfoPacket(ParksDatabase);
                            packet.PacketQueue = Data.PacketQueue;
                            Send(ID, packet);
                        }
                        else QConsole.WriteLine("ChatServer", "Invalid Room requested, ignoring...");
                    }
                    return;
                case TPWConstants.TPWChatServerCommand.CreatePlayer:
                    {
                        QConsole.WriteLine("ChatServer", "Client wants to join a park...");
                        TPWUnicodeString username = (TPWUnicodeString)ParsedData[1].Data,
                            password = (TPWUnicodeString)ParsedData[3].Data;
                        DWORD PlayerID = ParsedData[4].Data.ToDWORD(),
                            CustID = ParsedData[2].Data.ToDWORD();

                        if (!playerDatabase.TryGetValue(PlayerID.UInt32, out var DBPlayerInfo))
                        {
                            //AUTH FAILURE !!!
                            QConsole.WriteLine("ChatServer", $"PlayerID not found in the database! ID: {PlayerID.UInt32}");
                            throw new Exception("This operation most certainly cannot continue.");
                        }
                        else if (username.String != DBPlayerInfo.PlayerName.String)
                        {
                            //AUTH FAILURE !!!
                            QConsole.WriteLine("ChatServer", $"PlayerName mismatch! ID: {PlayerID.UInt32}, " +
                                $"PlayerName: {username.String}, DBPlayerName: {DBPlayerInfo.PlayerName}");                            
                        }
                        JoiningPlayers.Add(ID, DBPlayerInfo);
                        var packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerResponseCodes.CREATE_PLAYER,
                            DBPlayerInfo.PlayerName, password, PlayerID, CustID, (DWORD)CHAT_RECV_AMOUNT, (DWORD)34);                            
                        packet.MessageType = 0x012D;
                        packet.Param3 = Data.Param3;
                        packet.PacketQueue = Data.PacketQueue;
                        Send(ID, packet);
                    }
                    break;
                case TPWConstants.TPWChatServerCommand.SetPlayerData:
                    {
                        QConsole.WriteLine("ChatServer", "This client is allowed to join the room, confirming transaction...");
                        var packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.SetPlayerData);
                        packet.MessageType = 0x012D;
                        packet.Param3 = Data.Param3;
                        packet.PacketQueue = Data.PacketQueue;
                        Send(ID, packet);
                    }
                    break;
                case TPWConstants.TPWChatServerCommand.CreatePark:
                    {
                        DWORD parkID = ParsedData[4].Data.ToDWORD();
                        if (RoomManager.TryGetRoom(parkID, out var Room))
                        {
                            QConsole.WriteLine("ChatServer", "Creating park for client...");
                            if (!JoiningPlayers.TryGetValue(ID, out var info))
                            {
                                QConsole.WriteLine("ChatServer", $"Client: {ID} managed to sneak by without being identified, exiting because i'm scared...");
                                break;
                            }
                            TPWSEPlayerInterfaceTypes _interface = TPWSEPlayerInterfaceTypes.SIMThemeParkClient;
                            if (Data.Param3 == TPWConstants.TPWSE_QuazarClientMagicNumber) // this is a QuazarClient
                                _interface = TPWSEPlayerInterfaceTypes.QuazarClient;
                            Room.Admit(ID, info, _interface);
                            JoiningPlayers.Remove(ID);
                            PlayersInRooms.Add(ID, Room);                            

                            var packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerResponseCodes.PARK_CREATE);                                
                            packet.MessageType = 0x012D;
                            packet.Param3 = Data.Param3;
                            packet.PacketQueue = Data.PacketQueue;
                            Send(ID, packet);

                            if (Room.TryGetOnlineStateByConnection(ID, out TPWPlayerGameState pState))
                            {
                                Broadcast(GetPlayerInfoPacket(info, pState, Data.Param3));
                                switch (_interface)
                                {
                                    default:
                                    case TPWSEPlayerInterfaceTypes.SIMThemeParkClient:
                                        ServerTell($"{info.PlayerName} is now visiting this park!");
                                        break;
                                    case TPWSEPlayerInterfaceTypes.QuazarClient:
                                        ServerTell($"{info.PlayerName} is now chatting in this park on the TPW-SE app!");
                                        break;
                                }
                                
                            }
                        }
                        else
                        {
                            QConsole.WriteLine("ChatServer", "Client trying to a join a room that doesn't exist. Kicking this person... " +
                                $"Room ID: {parkID}");
                            Disconnect(ID);
                        }
                    }
                    break;
                case TPWConstants.TPWChatServerCommand.MovePlayer:
                    {
                        SetPacketCaching(false);
                        if (!PlayersInRooms.TryGetValue(ID, out var Room))
                        {
                            QConsole.WriteLine("ChatServer", "Uhh, this guy isn't even in a park, yet making requests. So I'm gonna disconnect him.");
                            Disconnect(ID);
                            break;
                        }

                        uint XWise = ParsedData[1].Data.ToDWORD();
                        uint YWise = ParsedData[2].Data.ToDWORD();
                        DWORD Teleport = ParsedData[3].Data.ToDWORD();

                        if (!Room.MovePlayer(ID, new Primitive.TPWPosition(XWise, YWise), Teleport.UInt32 > 0, out var moveType, out var position))
                        {
                            QConsole.WriteLine("ChatServer", "Player could not be moved by the OnlineRoom because of an internal error.");
                            break;
                        }                        
                        if (!Room.TryGetPlayerInfoByConnection(ID, out var PlayerInfo))
                        {
                            QConsole.WriteLine("ChatServer", "Player could not be found by the OnlineRoom because of an internal error.");
                            break;
                        }

                        var packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.MovePlayer, PlayerInfo.PlayerName, (DWORD)0);
                        packet.MessageType = 0x012D;
                        packet.Param3 = Data.Param3;
                        packet.PacketQueue = Data.PacketQueue;
                        Send(ID, packet);

                        packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.LocatePlayer,
                            (DWORD)PlayerInfo.PlayerID,
                            (DWORD)XWise, (DWORD)YWise, Teleport);
                        packet.MessageType = 0x012E;
                        Broadcast(packet);                        

                        SetPacketCaching(true);
                        break;
                    }
                case TPWConstants.TPWChatServerCommand.AFK:
                    {
                        SetPacketCaching(false);
                        var packet = GetAFKPacket();
                        packet.Param3 = Data.Param3;
                        packet.PacketQueue = Data.PacketQueue;
                        Send(ID, packet);
                        QConsole.WriteLine("ChatServer", "AFK timeout updated...");
                        SetPacketCaching(true);
                    }
                    break;
                case TPWConstants.TPWChatServerCommand.HearingRange:
                    {
                        DWORD hearingRange = ParsedData[1].Data.ToDWORD();

                        var packet = new TPWChatPacket(
                            (uint)TPWConstants.TPWChatServerCommand.HearingRange,(DWORD)1);                            
                        packet.MessageType = 0x012D;
                        packet.Param3 = Data.Param3;
                        packet.PacketQueue = Data.PacketQueue;
                        Send(ID, packet);

                        QConsole.WriteLine("ChatServer", $"{ID}: Hearing Range updated to: {hearingRange}...");
                    }
                    break;
                case TPWConstants.TPWChatServerCommand.GetPlayers:
                    {
                        SetPacketCaching(false);
                        if (!PlayersInRooms.TryGetValue(ID, out var Room))
                        {
                            QConsole.WriteLine("ChatServer", "Uhh, this guy isn't even in a park, yet making requests. So I'm gonna disconnect him.");
                            Disconnect(ID);
                            break;
                        }

                        TPWPacket[] packets = new TPWPacket[Room.PlayerCount + (closerNecessary ? 1 : 0)];
                        uint i = 0;
                        foreach (var Tuple in Room.GetConnectedPlayersInfo())
                        {                            
                            packets[i] = GetPlayerInfoPacket(Tuple.Player, Tuple.State, Data.Param3);
                            i++;
                        }
                        var closer = new TPWPacket()
                        {
                            MessageType = 0x012D,
                            Language = 0x00,
                            Param3 = Data.Param3,
                            PacketQueue = Data.PacketQueue
                        };
                        closer.EmplaceBody((uint)0x4F);
                        if (closerNecessary)
                            packets[i] = closer;
                        Send(ID, packets);
                        SetPacketCaching(true);
                    }
                    break;
                case TPWConstants.TPWChatServerCommand.Chat:
                    {
                        if (!PlayersInRooms.TryGetValue(ID, out var Room))
                        {
                            QConsole.WriteLine("ChatServer", "Uhh, this guy isn't even in a park, yet making requests. So I'm gonna disconnect him.");
                            Disconnect(ID);
                            break;
                        }
                        if (!Room.TryGetPlayerInfoByConnection(ID, out var Player)) {
                            QConsole.WriteLine("ChatServer", "Found the park the guy is in, but couldn't load player data? That's a bug.");
                            break;
                        }

                        TPWUnicodeString message = (TPWUnicodeString)ParsedData[1].Data,
                               name = Player.PlayerName;
                        var packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.Chat, name, message);
                        packet.MessageType = 0x012E;                        
                        packet.PacketQueue = 0x00;
                        Broadcast(packet);

                        packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerResponseCodes.CHAT_RECEIVED, message);
                        packet.MessageType = 0x012D;
                        packet.Param3 = Data.Param3;
                        packet.PacketQueue = Data.PacketQueue;
                        Send(ID, packet);

                        message = $"[{name}] {message}";
                        QConsole.WriteLine("Chat Log", message);
                    }
                    break;
                case TPWConstants.TPWChatServerCommand.Tell:
                    {
                        if (!PlayersInRooms.TryGetValue(ID, out var Room))
                        {
                            QConsole.WriteLine("ChatServer", "Uhh, this guy isn't even in a park, yet making requests. So I'm gonna disconnect him.");
                            Disconnect(ID);
                            break;
                        }
                        if (!Room.TryGetPlayerInfoByConnection(ID, out var Player))
                        {
                            QConsole.WriteLine("ChatServer", "Found the park the guy is in, but couldn't load player data? That's a bug.");
                            break;
                        }

                        TPWUnicodeString message = (TPWUnicodeString)ParsedData[2].Data,
                               Recipient = (TPWUnicodeString)ParsedData[1].Data,
                               name = Player.PlayerName;

                        if (!Room.TryGetConnectionIDByName(Recipient, out uint ConnectionID))
                        {
                            QConsole.WriteLine("ChatServer", "TELL: Recipient name was not found in this room. Aborting this Tell request.");
                            break;
                        }
                        else
                        {
                            var tellPacket = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.Tell, name, message);
                            tellPacket.MessageType = 0x012E;
                            tellPacket.PacketQueue = 0x00;
                            Send(ConnectionID, tellPacket);
                        }

                        var packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerResponseCodes.CHAT_RECEIVED, message);
                        packet.MessageType = 0x012D;
                        packet.Param3 = Data.Param3;
                        packet.PacketQueue = Data.PacketQueue;
                        Send(ID, packet);

                        message = $"[{name} -> {Recipient}] {message}";
                        QConsole.WriteLine("Chat Log", message);
                    }
                    break;
                case TPWConstants.TPWChatServerCommand.Shout:
                    {
                        if (!PlayersInRooms.TryGetValue(ID, out var Room))
                        {
                            QConsole.WriteLine("ChatServer", "Uhh, this guy isn't even in a park, yet making requests. So I'm gonna disconnect him.");
                            Disconnect(ID);
                            break;
                        }
                        if (!Room.TryGetPlayerInfoByConnection(ID, out var Player))
                        {
                            QConsole.WriteLine("ChatServer", "Found the park the guy is in, but couldn't load player data? That's a bug.");
                            break;
                        }
                        TPWUnicodeString message = (TPWUnicodeString)ParsedData[1].Data,
                               name = Player.PlayerName;

                        var packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.Shout, name, message);
                        packet.MessageType = 0x012E;
                        Broadcast(packet);

                        packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerResponseCodes.CHAT_RECEIVED, (DWORD)0);
                        packet.MessageType = 0x012D;
                        packet.Param3 = Data.Param3;
                        packet.PacketQueue = Data.PacketQueue;
                        Send(ID, packet);
                    }
                    break;
                case TPWConstants.TPWChatServerCommand.Ignore:
                    {
                        if (!PlayersInRooms.TryGetValue(ID, out var Room))
                        {
                            QConsole.WriteLine("ChatServer", "Uhh, this guy isn't even in a park, yet making requests. So I'm gonna disconnect him.");
                            Disconnect(ID);
                            break;
                        }
                        TPWUnicodeString username = (TPWUnicodeString)ParsedData[1].Data;
                        var packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerResponseCodes.IGNORE_RECEIVED, (DWORD)0);
                        packet.MessageType = 0x012D;
                        packet.Param3 = Data.Param3;
                        packet.PacketQueue = Data.PacketQueue;
                        Send(ID, packet);

                        packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerResponseCodes.CHAT_IGNORE, username);
                        packet.MessageType = 0x012D;
                        packet.Param3 = Data.Param3;
                        packet.PacketQueue = Data.PacketQueue + 1;
                        Send(ID, packet);
                    }
                    break;
                case TPWConstants.TPWChatServerCommand.AddBuddy:
                    {
                        if (!PlayersInRooms.TryGetValue(ID, out var Room))
                        {
                            QConsole.WriteLine("ChatServer", "Uhh, this guy isn't even in a park, yet making requests. So I'm gonna disconnect him.");
                            Disconnect(ID);
                            break;
                        }
                        TPWUnicodeString username = (TPWUnicodeString)ParsedData[1].Data;
                        if (!Room.TryGetConnectionIDByName(username, out var connectID) ||
                            !Room.TryGetPlayerInfoByConnection(ID, out var PlayerInfo))
                        {
                            QConsole.WriteLine("ChatServer", "Couldn't locate player connection ID due to an internal error.");
                            Disconnect(ID);
                            break;
                        }

                        var packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.AddBuddy,
                            (DWORD)0);
                        packet.MessageType = 0x012D;
                        packet.Param3 = Data.Param3;
                        packet.PacketQueue = Data.PacketQueue;
                        Send(ID, packet);

                        packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.AddBuddy,
                            PlayerInfo.PlayerName);
                        packet.MessageType = 0x012E;
                        Send(connectID, packet);
                    }
                    break;
                case TPWConstants.TPWChatServerCommand.LocatePlayer:
                    {
                        if (!PlayersInRooms.TryGetValue(ID, out var Room))
                        {
                            QConsole.WriteLine("ChatServer", "Uhh, this guy isn't even in a park, yet making requests. So I'm gonna disconnect him.");
                            Disconnect(ID);
                            break;
                        }
                        TPWUnicodeString username = (TPWUnicodeString)ParsedData[1].Data;
                        if (!Room.TryGetPlayerInfoByName(username, out var PlayerInfo) ||
                            !Room.TryGetOnlineStateByID(PlayerInfo.PlayerID, out var PlayerState))
                        {
                            QConsole.WriteLine("ChatServer", "Couldn't locate player state/info due to an internal error.");
                            Disconnect(ID);
                            break;
                        }
                        var packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.LocatePlayer,
                            (DWORD)0);
                        packet.MessageType = 0x012D;
                        packet.Param3 = Data.Param3;
                        packet.PacketQueue = Data.PacketQueue;
                        Send(ID, packet);
                        break;
                    }
                default:
                    QConsole.WriteLine("ChatServer", "The Server ignored a message of type: " + (TPWConstants.TPWChatServerCommand)MessageType);
                    break;
            }
            QConsole.WriteLine("Telemetry", $"{(TPWConstants.TPWChatServerCommand)MessageType} \n" +
                $"{string.Join("\n", ParsedData)}");
        }
        
        private TPWChatPacket GetPlayerInfoPacket(TPWPlayerInfo PlayerInfo, TPWPlayerGameState GameState, uint PacketID)
        {
            TPWPlayerInfo player = PlayerInfo;
            TPWPlayerGameState state = GameState;
            TPWChatPacket packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerResponseCodes.CHAT_PLAYER_INFO,
                                                     player.PlayerName, (DWORD)player.PlayerID, (DWORD)player.CustomerID,
                                                     (DWORD)(state.CurrentPosition.X + 500),
                                                     (DWORD)(state.CurrentPosition.Y + 1000), (DWORD)0,
                                                     (DWORD)1, (DWORD)1, (DWORD)0);
            packet.MessageType = 0x012E;
            packet.Param3 = (GameState.ClientInterface == TPWSEPlayerInterfaceTypes.QuazarClient) ? 
                TPWConstants.TPWSE_QuazarClientMagicNumber : PacketID;
            packet.PacketQueue = 00; //Data.PacketQueue + (uint)i;
            return packet;
        }

        protected override void OnManualSend(uint ID, ref TPWPacket Data)
        {
            OnIncomingPacket(ID, Data);
            //base.OnManualSend(ID, ref Data);
        }

        public override void Start()
        {
            QConsole.WriteLine(Name, "Starting...");
            BeginListening();
        }

        protected override void OnClientConnect(TcpClient Connection, uint ID)
        {
            base.OnClientConnect(Connection, ID);
        }

        public override void Stop()
        {
            QConsole.WriteLine(Name, "Stopping...");
            StopListening();
        }

        protected override void OnOutgoingPacket(uint ID, TPWPacket Data)
        {
            //Data.PacketQueue = PacketQueue;
        }

        public void TellAll(TPWUnicodeString From, TPWUnicodeString Text)
        {
            var packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.Tell, From, Text);
            packet.MessageType = 0x012E;
            packet.PacketQueue = 0x00;
            Broadcast(packet);
        }

        public void ServerTell(TPWUnicodeString Text) => TellAll("System", Text);
    }
}
