using QuazarAPI;
using QuazarAPI.Networking.Data;
using QuazarAPI.Networking.Standard;
using System;
using System.IO;
using System.Net.Sockets;

namespace SimTheme_Park_Online
{
    public class NewsServer : Component
    {
        uint MessageDirectorID;

        public NewsServer(int port) : base("NewsServer", port, SIMThemeParkWaypoints.NewsServer)
        {
            
        }

        protected override void OnIncomingPacket(Socket Sender, uint ID, TPWPacket Data)
        {
            if (!HandleCommand(ID, Data))
            {
                
            }
        }

        public override void Start()
        {
            QConsole.WriteLine("[NewsServer] Starting...");
            BeginListening();
        }

        protected override void OnClientConnect(TcpClient Connection, uint ID)
        {            
            base.OnClientConnect(Connection, ID);
            TPWPacket packet = new TPWPacket();
            byte[] buffer = File.ReadAllBytes("Library\\NewsPacket.dat");
            QConsole.WriteLine("Sending NewsPacket...\n" + string.Join(" ", buffer));
            Connection.Client.Send(buffer);
        }

        public override void Stop()
        {
            QConsole.WriteLine("[NewsServer] Stopping...");
            StopListening();
        }
    }
}
