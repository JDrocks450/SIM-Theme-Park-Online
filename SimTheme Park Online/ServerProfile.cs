using QuazarAPI;
using SimTheme_Park_Online.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SimTheme_Park_Online
{
    /// <summary>
    /// The settings related to a certain user profile
    /// </summary>
    [Serializable]
    public class ServerProfile
    {
        public const string DefaultPath = "Resources\\profile.tpwprofile";
        private string _gameNews = "Hello World";
        private string _sysNews = "System says Hello World";

        [JsonIgnore]
        public static ServerProfile Default => new ServerProfile();

        [JsonInclude]
        public TPWServerConfig ConfigurationSettings { get; set; } = TPWServerConfig.Default;

        public string Name { get; set; } = "Untitled";

        public string GameNewsString
        {
            get => _gameNews;
            set => _gameNews = value;
        }

        public string SystemNewsString
        {
            get => _sysNews;
            set => _sysNews = value;
        }

        public IEnumerable<TPWParkInfo> Parks
        {
            get; set;
        }

        public IEnumerable<TPWCityInfo> Cities
        {
            get; set;
        }

        public void UpdateGameNews(string NewGameNews)
        {
            GameNewsString = NewGameNews;
            QConsole.WriteLine("ServerProfile", "Game News was updated.");
            Save();
        }
        public void UpdateSystemNews(string NewSysNews)
        {
            SystemNewsString = NewSysNews;
            QConsole.WriteLine("ServerProfile", "System News was updated.");
            Save();
        }

        internal void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ServerProfile.DefaultPath));
            using (var fs = File.Create(ServerProfile.DefaultPath))
                fs.Write(JsonSerializer.SerializeToUtf8Bytes(this));
             QConsole.WriteLine("ServerProfile", "Server Profile has been successfully saved.");
        }
    }
}
