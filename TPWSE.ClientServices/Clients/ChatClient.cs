using QuazarAPI;
using QuazarAPI.Networking.Standard;
using SimTheme_Park_Online;
using SimTheme_Park_Online.Data;
using SimTheme_Park_Online.Data.Packets;
using SimTheme_Park_Online.Data.Primitive;
using SimTheme_Park_Online.Parsers;
using SimTheme_Park_Online.Primitive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TPWSE.ClientServices.Clients
{
    public class TPWChatEventArgs : QEventArgs<TPWUnicodeString>
    {
        public bool IsChatMessage => !IsPrivateMessage && !IsShoutMessage;
        public bool IsPrivateMessage { get; set; }
        public bool IsShoutMessage { get; set; }
        public readonly TPWUnicodeString Sender, Message;
        public TPWChatEventArgs(TPWUnicodeString Sender, TPWUnicodeString Message, bool PrivateMessage = false, bool Shouting = false)
        {
            this.Sender = Sender;
            this.Message = Data = Message;
            IsPrivateMessage = PrivateMessage;
            IsShoutMessage = Shouting;
        }
    }
    public class TPWChatMoveEventArgs : QEventArgs<TPWPosition>
    {
        public TPWPosition NewLocation { get; set; }
        public bool IsTeleporting { get; set; }
        public readonly TPWUnicodeString Player;
        public TPWChatMoveEventArgs(TPWUnicodeString Sender, TPWPosition Position, bool Teleporting = false)
        {
            Player = Sender;
            NewLocation = Position;
            IsTeleporting = Teleporting;
        }
    }
    public class TPWConnectionErrorEventArgs : QEventArgs<Exception>
    {
        public readonly TPWPacket DynamicPacket;
        public readonly List<TPWChatParsedData> PacketFields;

        public TPWConnectionErrorEventArgs(Exception Error, TPWPacket dynamicPacket, List<TPWChatParsedData> packetFields)
        {
            DynamicPacket = dynamicPacket;
            PacketFields = packetFields;
            Data = Error;
        }
    }
    public class ChatClient : QuazarClient
    {
        /// <summary>
        /// This is updated everytime <see cref="DownloadChatRoomInfo(DWORD[])"/> is called.
        /// </summary>
        public IEnumerable<TPWChatRoomInfo> OnlineChatRooms { get; private set; }
        /// <summary>
        /// Is this client actively connected to an online room
        /// </summary>
        public bool IsOnlineRoomConnected { get; private set; }
        /// <summary>
        /// The currently connected chat room. See: <see cref="IsOnlineRoomConnected"/>
        /// </summary>
        public TPWChatRoomInfo ConnectedChatRoom { get; private set; }
        /// <summary>
        /// The list of all connected clients. Changing this has no effect on anything server related. 
        /// and also should be avoided.
        /// </summary>
        public List<TPWPlayerInfo> OnlinePlayers { get; private set; } = new List<TPWPlayerInfo>();
        /// <summary>
        /// Is a dictionary of players in this game joining from a quazar client.
        /// </summary>
        private Dictionary<uint, bool> _quazarClients = new Dictionary<uint, bool>();
        /// <summary>
        /// Checks whether the player id in this online room is a QuazarClient or not.
        /// </summary>
        /// <param name="PlayerID"></param>
        /// <param name="isQuazar"></param>
        /// <returns></returns>
        public bool TryGetIsQuazarClient(uint PlayerID, out bool isQuazar) => _quazarClients.TryGetValue(PlayerID, out isQuazar);

        //EVENTS
        public event EventHandler<TPWChatEventArgs> OnOnlineChatReceived;
        public event EventHandler<QEventArgs<TPWPlayerInfo>> OnPlayerJoin;
        public event EventHandler<TPWConnectionErrorEventArgs> OnErrorThrown;
        public event EventHandler<TPWChatMoveEventArgs> OnPlayerMoved;

        public ChatClient(IPAddress Address, int Port) : base("ChatClient", Address, Port)
        {
            OnPacketReceived += ChatClient_OnPacketReceived;           
        }        

        /// <summary>
        /// Sends a text message to all players in the room.
        /// </summary>
        /// <param name="Message"></param>
        /// <returns></returns>
        public async void SendChatMessage(string SenderName, string Message)
        {
            TPWChatPacket chatMsgPacket = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.Chat,
                (TPWUnicodeString)Message, (TPWUnicodeString)SenderName);
            await SendPacket(chatMsgPacket);            
        }
        /// <summary>
        /// Sends a message to the chat server - pay attention to the 
        /// </summary>
        /// <param name="Message"></param>
        /// <returns></returns>
        public async void SendTellMessage(string ToPlayer, string Message)
        {
            TPWChatPacket chatMsgPacket = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.Tell,
                (TPWUnicodeString)ToPlayer, (TPWUnicodeString)Message);
            await SendPacket(chatMsgPacket);
        }

        /// <summary>
        /// Downloads information about an online park session from the game server and updates <see cref="OnlineChatRooms"/>
        /// </summary>
        /// <param name="ParkIDs"></param>
        /// <returns></returns>
        public async Task<TPWChatRoomInfo[]> DownloadChatRoomInfo(params DWORD[] ParkIDs)
        {
            if (!IsConnected)
                await Connect();            
            TPWChatRoomInfo[] roomInfos = new TPWChatRoomInfo[ParkIDs.Length];
            int index = 0;
            foreach (DWORD ID in ParkIDs)
            {
                TPWChatPacket packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.RoomInfo, ID);
                await SendPacket(packet);
                var response = await AwaitPacket();
                TPWChatPacketParser.Parse(response, out List<TPWChatParsedData> values);
                TPWChatRoomInfo roomInfo = new TPWChatRoomInfo(values);
                roomInfos[index] = roomInfo;
                index++;
            }
            OnlineChatRooms = roomInfos;
            return roomInfos;
        }

        /// <summary>
        /// Submits a request for all players, thus flushing <see cref="OnlinePlayers"/> until the server
        /// fulfills this request to completion.
        /// <para>Generally this should be avoided except when there is believed to be a discrepency.</para>
        /// </summary>
        public async void RequestAllPlayers()
        {
            OnlinePlayers = new List<TPWPlayerInfo>();
            TPWPacket requestPacket = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.GetPlayers);
            await SendPacket(requestPacket);
        }

        /// <summary>
        /// Attempts to connect to the online room using the supplied PlayerInfo and ChatRoomInfo
        /// </summary>
        /// <param name="JoiningPlayerInfo">The player to join as</param>
        /// <param name="JoiningRoomInfo">The room you are intending to connect to</param>
        /// <exception cref="InvalidOperationException">This client cannot be already 
        /// connected to an online room, if it is, an exception is thrown advising you to dispose of the object.</exception>
        public async Task<bool> OnlineRoomConnect(TPWPlayerInfo JoiningPlayerInfo, TPWChatRoomInfo JoiningRoomInfo)
        {
            if (IsOnlineRoomConnected)
            {
                throw new InvalidOperationException("This ChatClient instance is already connected to a room. " +
                    "You need to Dispose this object and make a new one, to ensure code integrity.");
            }
            Strategy = ClientRecvStrategy.ASYNC_AWAIT;
            if (!IsConnected)
                await Connect();
            if (!await AttemptCreatePlayer(JoiningPlayerInfo))
                return false;
            if (!await AttemptCreatePark(JoiningRoomInfo))
                return false;
            IsOnlineRoomConnected = true;
            ConnectedChatRoom = JoiningRoomInfo;
            Strategy = ClientRecvStrategy.EVENT_BASED;            
            return true;
        }

        public Task AddBuddy(TPWUnicodeString PlayerName)
        {
            if (!IsOnlineRoomConnected)
            {
                throw new InvalidOperationException("This ChatClient instance is not connected to a room. ");
            }
            return SendPacket(new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.AddBuddy, PlayerName));
        }

        public Task Move(TPWPosition DestinationPosition)
        {
            if (!IsOnlineRoomConnected)
            {
                throw new InvalidOperationException("This ChatClient instance is not connected to a room. ");
            }
            return SendMoveCommand(DestinationPosition, TPWConstants.TPWChatPlayerMovementTypes.Walk);
        }
        public Task Teleport(TPWPosition DestinationPosition)
        {
            if (!IsOnlineRoomConnected)
            {
                throw new InvalidOperationException("This ChatClient instance is not connected to a room. ");
            }
            return SendMoveCommand(DestinationPosition, TPWConstants.TPWChatPlayerMovementTypes.Teleport);
        }

        private Task SendMoveCommand(TPWPosition DestinationPosition, TPWConstants.TPWChatPlayerMovementTypes Teleport)
        {
            TPWChatPacket packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.MovePlayer,
                            (DWORD)DestinationPosition.X, (DWORD)DestinationPosition.Y, (DWORD)(uint)Teleport);
            return SendPacket(packet);
        }

        private void ChatClient_OnPacketReceived(object sender, QEventArgs<TPWPacket> e)
        {
            TPWPacket packet = e.Data;
            if (!TPWChatPacketParser.Parse(packet, out List<TPWChatParsedData> Items))
            {
                QConsole.WriteLine("ChatClient", "Error on dynamic packet received.");
                OnErrorThrown?.Invoke(sender, new TPWConnectionErrorEventArgs(null, packet, null));
                return;
            }
            packet.SetPosition(0);
            DWORD messageType = packet.ReadBodyDword();
            try
            {
                if (packet.MessageType == (uint)TPWConstants.TPWChatServerChannel.PUBLIC) // PUBLIC DOMAIN
                {
                    switch (messageType)
                    {
                        case (uint)TPWConstants.TPWChatServerResponseCodes.CHAT_PLAYER_INFO:
                            TPWPlayerInfo playerInfo = new TPWPlayerInfo(
                                Items[2].Data.ToDWORD(), Items[3].Data.ToDWORD(), (TPWUnicodeString)Items[1].Data);
                            bool isQuazar = packet.Param3 == TPWConstants.TPWSE_QuazarClientMagicNumber;
                            if (!OnlinePlayers.Contains(playerInfo))
                                OnlinePlayers.Add(playerInfo);
                            if (!_quazarClients.ContainsKey(playerInfo.PlayerID))
                                _quazarClients.Add(playerInfo.PlayerID, isQuazar);
                            OnPlayerJoin?.Invoke(sender, new QEventArgs<TPWPlayerInfo>() { Data = playerInfo });
                            break;
                        case (uint)TPWConstants.TPWChatServerCommand.Chat:
                            {
                                TPWUnicodeString Sender = (TPWUnicodeString)Items[1].Data;
                                TPWUnicodeString Message = (TPWUnicodeString)Items[2].Data;
                                OnOnlineChatReceived?.Invoke(sender, new TPWChatEventArgs(Sender, Message));
                            }
                            break;
                        case (uint)TPWConstants.TPWChatServerCommand.SystemAnnouncement:
                            {
                                TPWUnicodeString Message = (TPWUnicodeString)Items[1].Data;
                                OnOnlineChatReceived?.Invoke(sender, new TPWChatEventArgs("$INFO", Message));
                            }
                            break;
                        case (uint)TPWConstants.TPWChatServerCommand.Tell:
                            {
                                TPWUnicodeString Sender = (TPWUnicodeString)Items[1].Data;
                                TPWUnicodeString Message = (TPWUnicodeString)Items[2].Data;
                                OnOnlineChatReceived?.Invoke(sender, new TPWChatEventArgs(Sender, Message, true));
                            }
                            break;
                        case (uint)TPWConstants.TPWChatServerCommand.Shout:
                            {
                                TPWUnicodeString Sender = (TPWUnicodeString)Items[1].Data;
                                TPWUnicodeString Message = (TPWUnicodeString)Items[2].Data;
                                OnOnlineChatReceived?.Invoke(sender, new TPWChatEventArgs(Sender, Message, false, true));
                            }
                            break;
                        case (uint)TPWConstants.TPWChatServerResponseCodes.BOSS_CHAT_MOVE:
                            {
                                TPWUnicodeString Sender = (TPWUnicodeString)Items[1].Data;
                                DWORD X = Items[2].Data.ToDWORD();
                                DWORD Z = Items[3].Data.ToDWORD();
                                DWORD Teleport = Items[4].Data.ToDWORD();
                                OnPlayerMoved?.Invoke(Sender, new TPWChatMoveEventArgs(Sender,
                                    new TPWPosition(X, Z), Teleport == 0 ? false : true));
                            }
                            break;
                        default:
                            throw new Exception($"StreamMsg: {messageType} Received");
                    }
                }
                else throw new Exception($"DirectMsg: {messageType} Received"); // Private Domain
            }
            catch(Exception exception)
            {
                OnErrorThrown?.Invoke(sender, new TPWConnectionErrorEventArgs(exception, packet, Items));
            }
        }

        private async Task<bool> AttemptCreatePark(TPWChatRoomInfo joiningRoomInfo)
        {
            TPWChatPacket joinRoomPacket = default;
            bool nonStandardAllowed = false; // true only for servers that aren't using TPW-SE standard command formatting.
            if (joiningRoomInfo.IsParkIDValid)
            { // USE CLASSIC STRATEGY FOR TPW-SE STANDARD SERVERS
                joinRoomPacket = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.CreatePark,
                joiningRoomInfo.ParkName, (DWORD)0, (DWORD)0, (DWORD)joiningRoomInfo.ParkID);
            }
            else
            { // ATTEMPT TO MIRROR THE SERVER'S RESPONSE HERE
                joinRoomPacket = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.CreatePark,
                joiningRoomInfo.ParkName, (DWORD)0, (DWORD)0, joiningRoomInfo.ParkIDResponse);
                nonStandardAllowed = true;
            }
            joinRoomPacket.Param3 = TPWConstants.TPWSE_QuazarClientMagicNumber; // allows the Server to know this is a QuazarClient
            await SendPacket(joinRoomPacket);
            TPWPacket responsePacket = await AwaitPacket();            
            if (responsePacket != null) {
                responsePacket.SetPosition(0);
                if (responsePacket.ReadBodyDword() == (uint)TPWConstants.TPWChatServerResponseCodes.PARK_CREATE)
                    return true; // CREATE PARK SUCCESS
                responsePacket.SetPosition(0);
                if (nonStandardAllowed && responsePacket.ReadBodyDword() == (uint)TPWConstants.TPWChatServerCommand.CreatePark)
                    return true; // NON-STANDARD, ACCEPTABLE RESPONSE
            }
            return false;
        }

        /// <summary>
        /// The routine for creating a player on the server is handled by this method.
        /// <para>Send CreatePlayer and SetPlayerInfo packets and handles their respective responses.</para>
        /// </summary>
        /// <param name="PlayerInfo"></param>
        /// <returns></returns>
        private async Task<bool> AttemptCreatePlayer(TPWPlayerInfo PlayerInfo)
        {
            TPWChatPacket joinRoomPacket = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.CreatePlayer,
                PlayerInfo.PlayerName, (DWORD)PlayerInfo.CustomerID, (TPWUnicodeString)"PASSWORD", (DWORD)PlayerInfo.PlayerID);
            await SendPacket(joinRoomPacket);
            List<TPWChatParsedData> responsePacketFields = null;
            bool success = TPWChatPacketParser.Parse(await AwaitPacket(), out responsePacketFields);
            if (!success)
                return false;
            //TODO: Use these received values for error checking
            TPWChatPacket setPlayerData = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.SetPlayerData);
            await SendPacket(setPlayerData);
            TPWPacket responsePacket = await AwaitPacket();
            responsePacket.SetPosition(0);
            if (responsePacket != null && responsePacket.ReadBodyDword() == (uint)TPWConstants.TPWChatServerCommand.SetPlayerData)
                return true;
            return false;
        }
    }
}
