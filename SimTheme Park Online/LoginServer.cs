using MiscUtil.Conversion;
using QuazarAPI;
using QuazarAPI.Networking.Data;
using QuazarAPI.Networking.Standard;
using SimTheme_Park_Online.Data;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace SimTheme_Park_Online
{
    public class LoginServer : Component
    {
        uint MessageDirectorID;

        public LoginServer(int port) : base("LoginServer", port, SIMThemeParkWaypoints.LoginServer, 8)
        {
            var Packet = GenerateLoginSuccessPacket();

            //export to file
            using (FileStream stream = File.Create("login.dat"))
                Packet.Write(stream);
        }

        protected override void OnIncomingPacket(Socket Sender, uint ID, TPWPacket Data)
        {
            if (!HandleCommand(ID, Data))
            {
                
            }
        }

        public override void Start()
        {
            QConsole.WriteLine("[LoginServer] Starting...");
            BeginListening();
        }

        protected TPWPacket GenerateLoginSuccessPacket()
        {
            //Create a new response packet in TPW format
            TPWPacket Packet = new TPWPacket()
            {
                //Response type is set to Bs
                ResponseCode = TPWConstants.Bs_Header,
                //The type of message is response 9
                MsgType = 0009,                
                Param1 = 0x0809,
                Param2 = 0x0000,
                Param3 = 0x0000,
                //Set a footer for this packet of <DWORD>.MaxValue
                Footer = new byte[4] { 255, 255, 255, 255 }
            };

            // allocate a body buffer
            byte[] body = new byte[50];
            EndianBitConverter converter = EndianBitConverter.Big;
            converter.CopyBytes((uint)0x01234, body, 0); // emplace Player and Customer ID
            converter.CopyBytes((uint)0x05678, body, 4);
            Packet.Body = body;            

            return Packet;
        }

        protected override bool OnReceive(uint ID, byte[] dataBuffer)
        {
            if (!base.OnReceive(ID, dataBuffer)) return false;

            var Packet = GenerateLoginSuccessPacket();

            // Send the packet to this client
            Send(ID, Packet);
            return true;

            /*
            byte[] buffer = new byte[20 + 50];
            buffer[0] = (byte)'B';
            buffer[1] = (byte)'s';            
            converter.CopyBytes((ushort)9,buffer, 0);
            converter.CopyBytes((ushort)0x0809, buffer, 2);
            converter.CopyBytes((ushort)0x0000, buffer, 4);
            converter.CopyBytes((uint)50, buffer, 6);
            converter.CopyBytes((uint)0x0000, buffer, 10);            
            EndianBitConverter.Little.CopyBytes((uint)0x000A, buffer, 14);            
            converter.CopyBytes((uint)uint.MaxValue, buffer, buffer.Length - 4);
            Send(ID, buffer);
            */            
        }

        protected override void OnClientConnect(TcpClient Connection, uint ID)
        {            
            base.OnClientConnect(Connection, ID);
            //Connection.GetStream().Write(new byte[] { (byte)'B', (byte)'s' }, 0, 2);            
        }

        public override void Stop()
        {
            QConsole.WriteLine("[LoginServer] Stopping...");
            StopListening();
        }
    }
}
