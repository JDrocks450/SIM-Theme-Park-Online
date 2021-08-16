using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SimTheme_Park_Online
{
    /// <summary>
    /// Manages the Theme Park World / Sim Theme Park Server Components
    /// <para>This should be the main entrypoint to the Server Frontend software</para>
    /// </summary>
    public class ServerManagement
    {
        public readonly Thread LoginThread, NewsThread;
        public readonly LoginServer LoginServer;
        public readonly NewsServer NewsServer;

        /// <summary>
        /// Creates a new <see cref="ServerManagement"/> object and initializes the server threads without starting them.
        /// </summary>
        public ServerManagement()
        {
            LoginServer = new LoginServer(7598);
            LoginThread = new Thread((ThreadStart)delegate { LoginServer.Start(); });
            NewsServer = new NewsServer(7597);
            NewsThread = new Thread((ThreadStart)delegate { NewsServer.Start(); });
        }

        public void StartAll()
        {            
            LoginThread.Start();            
            NewsThread.Start();
        }

        public void AbortAll()
        {
            LoginThread.Abort();            
            NewsThread.Abort();
        }
    }
}
