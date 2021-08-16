using System;
using System.Collections.Generic;
using System.Text;

namespace SimTheme_Park_Online
{
    [Serializable]
    /// <summary>
    /// Data values that make up all sessions of TPWAPI
    /// </summary>
    public static class ServerContext
    {
        public const int
            LOGIN_PORT = 7598,
            NEWS_PORT = 7597;

        /// <summary>
        /// The maximum amount of connected clients for each server component
        /// </summary>
        public static uint Max_Connections { get; set; }
    }
}
