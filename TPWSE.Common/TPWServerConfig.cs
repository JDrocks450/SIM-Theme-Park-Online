using System;

namespace SimTheme_Park_Online
{
    [Serializable]
    public class TPWServerConfig
    {
        public const string CONFIG_PATH = "Resources/config.tpwconfig";
        public static TPWServerConfig Default => new TPWServerConfig()
        {

        };
        public int LOGIN_PORT { get; set; } = 7598;
        public int CITY_PORT { get; set; } = 7591;
        public int NEWS_PORT { get; set; } = 7597;
        public int CHAT_PORT { get; set; } = 7593;

        public bool LOGIN_ENABLE { get; set; } = true;
        public bool NEWS_ENABLE { get; set; } = true;
        public bool CITY_ENABLE { get; set; } = true;
        public bool CHAT_ENABLE { get; set; } = true;

        public int MAX_CONNECTIONS { get; set; } = 1;
    }
}
