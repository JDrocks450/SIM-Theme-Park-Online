using QuazarAPI;
using QuazarAPI.Networking.Standard;
using SimTheme_Park_Online;
using System;

namespace TPWSE.FTP
{
    public class FTPServer : TPWSEServer
    {
        public FTPServer(int port = 21, uint Backlog = 1) : base("FileTransferServer", port, SIMThemeParkWaypoints.FTPServer, Backlog)
        {

        }

        public override void Start()
        {
            QConsole.WriteLine(Name, "Starting...");
            BeginListening();
        }

        public override void Stop()
        {
            QConsole.WriteLine(Name, "Stopping...");
            StopListening();
        }
    }
}
