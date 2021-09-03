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

namespace SimTheme_Park_Online
{
    public class ChatServer : Component
    {        
        private Parsers.TPWChatPacketParser ChatPacketParser = new Parsers.TPWChatPacketParser();
        private readonly IDatabaseInterface<uint, TPWParkInfo> ParksDatabase;

        public ChatServer(int port, IDatabaseInterface<uint, TPWParkInfo> ParksDatabase) : base("ChatServer", port, SIMThemeParkWaypoints.ChatServer)
        {
            this.ParksDatabase = ParksDatabase;
        }

        private IEnumerable<TPWChatRoomInfoPacket> GetRoomInfoPackets()
        {
            List<TPWChatRoomInfoPacket> list = new List<TPWChatRoomInfoPacket>();
            foreach (TPWParkInfo park in ParksDatabase.GetAllData())
            {
                list.Add(park.GetRoomInfoPacket());
            }
            return list;
        }

        protected override void OnIncomingPacket(uint ID, TPWPacket Data)
        {
            ChatPacketParser.TryParse(Data, out List<Parsers.TPWChatParsedData> ParsedData);
            uint MessageType = ParsedData[0].Data.ToDWORD();

            switch ((TPWConstants.TPWChatServerCommand)MessageType)
            {
                case TPWConstants.TPWChatServerCommand.RoomInfo:
                    {
                        if (ParksDatabase.TryGetValue(ParsedData[1].Data.ToDWORD(), out var parkInfo))
                        {                            
                            var packet = parkInfo.GetRoomInfoPacket(Data.PacketQueue);
                            packet.PacketQueue = Data.PacketQueue;
                            Send(ID, packet);
                        }
                    }
                    return;
                case TPWConstants.TPWChatServerCommand.CreatePlayer:
                    {
                        var packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.Bs_CreatePlayerResponse, 
                            (TPWUnicodeString)"Bisquick", (TPWUnicodeString)"password",
                            (DWORD)0789, (DWORD)1456, (DWORD)1024, (DWORD)34);
                        packet.MsgType = 0x012D;
                        packet.Param3 = Data.Param3;
                        packet.PacketQueue = Data.PacketQueue;
                        Send(ID, packet);
                    }
                    break;
                case TPWConstants.TPWChatServerCommand.SetPlayerData:
                    { 
                        var packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.SetPlayerData);
                        packet.MsgType = 0x012D;                        
                        packet.Param3 = Data.Param3;
                        packet.PacketQueue = Data.PacketQueue;
                        Send(ID, packet);
                    }
                    break;
                case TPWConstants.TPWChatServerCommand.CreatePark:
                    {
                        var packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.CreateParkSuccess
                            );
                        packet.MsgType = 0x012D;
                        packet.Param3 = Data.Param3;
                        packet.PacketQueue = Data.PacketQueue;
                        Send(ID, packet);
                    }
                    break;
                case TPWConstants.TPWChatServerCommand.MovePlayer:
                    {
                        var packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.MovePlayer,
                                (DWORD)16512, (DWORD)1190, (DWORD)1);
                        packet.MsgType = 0x012D;
                        packet.Param3 = Data.Param3;
                        packet.PacketQueue = Data.PacketQueue;
                        Send(ID, packet);
                    }
                    break;
                case TPWConstants.TPWChatServerCommand.AFK:
                    {
                        var packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.AFK,
                            (DWORD)0);
                        packet.MsgType = 0x012D;
                        packet.Param3 = Data.Param3;
                        packet.PacketQueue = Data.PacketQueue;
                        Send(ID, packet);
                    }
                    break;
                case TPWConstants.TPWChatServerCommand.HearingRange:
                    {
                        var packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.HearingRange,
                            (DWORD)1);
                        packet.MsgType = 0x012D;
                        packet.Param3 = Data.Param3;
                        packet.PacketQueue = Data.PacketQueue;
                        Send(ID, packet);
                    }
                    break;
                case TPWConstants.TPWChatServerCommand.GetPlayers:
                    {
                        var packet = new TPWChatPacket((uint)TPWConstants.TPWChatServerCommand.EnumPlayer,
                                (TPWUnicodeString)"Bisquick", (DWORD)0, (DWORD)0, (DWORD)0, (DWORD)0, (DWORD)0, (DWORD)10627, (DWORD)17439, (DWORD)100);
                        packet.MsgType = 0x012E;
                        packet.Param3 = Data.Param3;
                        packet.PacketQueue = Data.PacketQueue;
                        Send(ID, packet);
                    }
                    break;
            }            
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
    }
}
