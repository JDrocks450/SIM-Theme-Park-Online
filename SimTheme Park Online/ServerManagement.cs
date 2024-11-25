using QuazarAPI;
using QuazarAPI.Networking.Standard;
using SimTheme_Park_Online.Databases;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using TPWSE.FTP;

namespace SimTheme_Park_Online
{
    /// <summary>
    /// Manages the Theme Park World / Sim Theme Park Server Components
    /// <para>This should be the main entrypoint to the Server Frontend software</para>
    /// </summary>
    public class ServerManagement
    {
        public static ServerManagement Current { get; private set; }
        /// <summary>
        /// The currently opened <see cref="ServerProfile"/>
        /// </summary>
        public ServerProfile Profile { get; set; } = ServerProfile.Default;

        /// <summary>
        /// The server configuration settings.
        /// </summary>
        public TPWServerConfig Config => Profile.ConfigurationSettings;

        /// <summary>
        /// Each server component runs on its own thread
        /// </summary>
        public Thread LoginThread, NewsThread, CityThread, ChatThread;

        /// <summary>
        /// The FTP server instance for this session.
        /// </summary>
        public FTPServer FTPServer;
        private Thread FTPThread;

        /// <summary>
        /// The login server instance for this session.
        /// </summary>
        public LoginServer LoginServer;
        /// <summary>
        /// The news server instance for this session.
        /// </summary>
        public NewsServer NewsServer;
        /// <summary>
        /// The city server instance for this session.
        /// </summary>
        public CityServer CityServer;
        /// <summary>
        /// The chat server instance for this session.
        /// </summary>
        public ChatServer ChatServer;

        /// <summary>
        /// The database storing the parks in the game
        /// </summary>
        public TPWParkDatabase ParkDatabase;
        public TPWCityDatabase CityDatabase;
        public TPWPlayerDatabase PlayerDatabase;

        /// <summary>
        /// Creates a new <see cref="ServerManagement"/> object and initializes the server threads without starting them.
        /// </summary>
        public ServerManagement()
        {
            if (Current != null)
                throw new Exception("Uhh thats not supposed to happen. ");
            Current = this;
            LoadProfile();                
        }

        /// <summary>
        /// Initializes each server component.
        /// <para>
        /// This MUST be called before <see cref="StartAll"/> and after configuration settings are solidified.
        /// Settings CANNOT be applied while the server is already running.
        /// </para>
        /// </summary>
        public void Initialize()
        {
            ParkDatabase = new TPWParkDatabase(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "TPW-SE", "Database", "tpwse_parksdb.db"
                    )
                );
            CityDatabase = new TPWCityDatabase(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "TPW-SE", "Database", "tpwse_citiesdb.db"
                    )
                );
            PlayerDatabase = new TPWPlayerDatabase(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "TPW-SE", "Database", "tpwse_playersdb.db"
                    )
                );

            LoginServer = new LoginServer(Config.LOGIN_PORT, PlayerDatabase);
            LoginThread = new Thread((ThreadStart)delegate { LoginServer.Start(); });
            NewsServer = new NewsServer(Config.NEWS_PORT);
            NewsThread = new Thread((ThreadStart)delegate { NewsServer.Start(); });
            CityServer = new CityServer(Config.CITY_PORT, CityDatabase, ParkDatabase) { };
            CityThread = new Thread((ThreadStart)delegate { CityServer.Start(); });
            ChatServer = new ChatServer(Config.CHAT_PORT, PlayerDatabase, ParkDatabase);
            ChatThread = new Thread((ThreadStart)delegate { ChatServer.Start(); });
            FTPServer = new FTPServer(Config.FTP_PORT);
            FTPThread = new Thread((ThreadStart)delegate { FTPServer.Start(); });
        }

        /// <summary>
        /// Loads the config file from the disk
        /// </summary>
        public void LoadProfile()
        {
            try
            {
                string configPath = ServerProfile.DefaultPath;
                string json = File.ReadAllText(configPath);
                Profile = JsonSerializer.Deserialize<ServerProfile>(json);
            }
            catch(Exception e)
            {
                QConsole.WriteLine("System", e.Message);
            }
        }

        /// <summary>
        /// Saves the config file to the disk.
        /// </summary>
        /// <param name="profile"></param>
        public void SaveProfile(ServerProfile profile)
        {
            Profile = profile;
            profile.Save();
        }

        /// <summary>
        /// Starts all the components
        /// </summary>
        public void StartAll()
        {
            if (Config.LOGIN_ENABLE)
                LoginThread.Start();
            if (Config.NEWS_ENABLE)
                NewsThread.Start();
            if (Config.CITY_ENABLE)
                CityThread.Start();
            if (Config.CHAT_ENABLE)
                ChatThread.Start();
            if (Config.FTP_ENABLE)
                FTPThread.Start();
        }
        /// <summary>
        /// Stops all the components
        /// </summary>
        public void StopAll()
        {
            if (Config.LOGIN_ENABLE)
                LoginServer.Stop();
            if (Config.NEWS_ENABLE)
                NewsServer.Stop();
            if (Config.CITY_ENABLE)
                CityServer.Stop();
            if (Config.CHAT_ENABLE)
                ChatServer.Stop();
            if (Config.FTP_ENABLE)
                FTPServer.Stop();
        }
    }
}
