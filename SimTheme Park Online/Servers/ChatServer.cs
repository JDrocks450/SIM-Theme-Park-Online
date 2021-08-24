using QuazarAPI;
using QuazarAPI.Networking.Standard;
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
        uint MessageDirectorID;

        public ChatServer(int port) : base("ChatServer", port, SIMThemeParkWaypoints.ChatServer)
        {
            
        }

        protected override void OnIncomingPacket(uint ID, TPWPacket Data)
        {
            if (!HandleCommand(ID, Data))
            {
                
            }
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
    }
}
