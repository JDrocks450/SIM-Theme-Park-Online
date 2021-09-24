using MiscUtil.Conversion;
using QuazarAPI;
using QuazarAPI.Networking.Data;
using QuazarAPI.Networking.Standard;
using SimTheme_Park_Online.Data;
using SimTheme_Park_Online.Database;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security;

namespace SimTheme_Park_Online
{
    public class LoginServer : QuazarServer
    {
        private readonly IDatabaseInterface<uint, TPWPlayerInfo> playerDatabase;

        public LoginServer(int port, IDatabaseInterface<uint, TPWPlayerInfo> PlayerDatabase) : base("LoginServer", port, SIMThemeParkWaypoints.LoginServer, 8)
        {
            playerDatabase = PlayerDatabase;
        }

        protected override void OnIncomingPacket(uint ID, TPWPacket Data)
        {
            if (AuthenticateUser(Data, out TPWPlayerInfo Player))
            {
                QConsole.WriteLine("LoginServer", "User was authenticated.");
                Send(ID, GenerateLoginSuccessPacket(Player));
                return;
            }
            QConsole.WriteLine("LoginServer", "User was not authenticated - Bad password.");
            var failedPacket = new TPWPacket()
            {
                MsgType = (ushort)TPWConstants.TPWLoginMsgCodes.ERROR_1,
                PacketQueue = 0x0A
            };
            Send(ID, failedPacket);
        }

        private bool AuthenticateUser(TPWPacket Data, out TPWPlayerInfo PlayerInfo)
        {
            bool autoVerify = false;
            var body = Data.Body;
            var username = Parsers.TPWParsedData.ReadBodyUnicodeString(body, 0, out var NewPos);
            SecureString password = new SecureString();
            Data.SetPosition((int)NewPos);

            var player = playerDatabase.Search(x => x.PlayerName.String == username).FirstOrDefault();
            if (player == default)
            {
                var playerID = playerDatabase.CreateKey();
                player = new TPWPlayerInfo(playerID, 0x10006789, username);
                playerDatabase.AddData(playerID, player);
                password.Dispose();
                password = player.SecurePassword;
                autoVerify = true;
            }

            Data.SetPosition((int)Data.BodyPosition - 1 + 0x31);
            var tempPass = Parsers.TPWParsedData.ReadBodyUnicodeString(body, (int)Data.BodyPosition, out NewPos);
            foreach (char c in tempPass.String)
                password.AppendChar(c);
            tempPass = null;

            PlayerInfo = player;

            if (!autoVerify)
            {
                if (password.IsEqualTo(player.SecurePassword))
                    return true;
                return false;
            }
            else return true;
        }

        public override void Start()
        {
            QConsole.WriteLine(Name, "Starting...");
            BeginListening();
        }

        protected TPWPacket GenerateLoginSuccessPacket(TPWPlayerInfo Player)
        {
            return new Data.Structures.TPWLoginAuthPacket(
                TPWConstants.TPWLoginMsgCodes.SUCCESS, Player.PlayerID, Player.CustomerID,
                Player.PlayerName, "admin@bullfrog.com", 0xFFFFFFFF);

        }

        protected override void OnClientConnect(TcpClient Connection, uint ID)
        {            
            base.OnClientConnect(Connection, ID);
            //Connection.GetStream().Write(new byte[] { (byte)'B', (byte)'s' }, 0, 2);            
        }

        public override void Stop()
        {
            QConsole.WriteLine(Name, "Stopping...");
            StopListening();
        }

        protected override void OnOutgoingPacket(uint ID, TPWPacket Data)
        {
            
        }
    }
}


#if false
            //Create a new response packet in TPW format
            TPWPacket Packet = new TPWPacket()
            {
                //Response type is set to Bs
                ResponseCode = TPWConstants.Bs_Header,
                //The type of message is response 9
                MsgType = 0009,                
                Language = 0x0809,
                Param2 = 0x0000,
                Param3 = 0x0000,
                //Set a footer for this packet of <DWORD>.MaxValue
                Footer = new byte[4] { 255, 255, 255, 255 }
            };

            // allocate a body buffer
            byte[] body = new byte[150];
            EndianBitConverter converter = EndianBitConverter.Big;
            converter.CopyBytes((uint)0x01234, body, 0); // emplace Player and Customer ID
            converter.CopyBytes((uint)0x05678, body, 4);
            Packet.Body = body;            

            return Packet;
#endif
