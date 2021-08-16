using QuazarAPI.Networking.Standard;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuazarAPI.Networking.Data
{
    [Serializable]
    /// <summary>
    /// The body of a <see cref="Commands.CLIENTINFO"/> command
    /// </summary>
    public class ClientInfo
    {
        public SIMThemeParkWaypoints Me
        {
            get; set;
        }
        public string Name
        {
            get; set;
        } = "User";
        public uint ID
        {
            get; set;
        }

        public ClientInfo()
        {

        }

        public ClientInfo(SIMThemeParkWaypoints me, string name)
        {
            Me = me;
            Name = name;
        }

        public override string ToString()
        {
            return $"ClientInfo [{Enum.GetName(typeof(SIMThemeParkWaypoints), Me)}, {Name}]";
        }
    }
}
