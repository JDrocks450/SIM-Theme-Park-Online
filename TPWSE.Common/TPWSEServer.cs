using QuazarAPI.Networking.Data;
using QuazarAPI.Networking.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online
{
    public abstract class TPWSEServer : QuazarServer<TPWPacket>
    {
        /// <summary>
        /// This waypoint in the server cluster, not currently used.
        /// </summary>
        public SIMThemeParkWaypoints ThisWaypoint
        {
            get; protected set;
        }
        public IEnumerable<uint> GetAllWaypointClients(SIMThemeParkWaypoints wayPoint) => _clientInfo.Where(x => x.Value.Me == (int)wayPoint).Select(x => x.Key);
        protected Dictionary<SIMThemeParkWaypoints, uint> KnownConnectionMap = new Dictionary<SIMThemeParkWaypoints, uint>();
        /// <summary>
        /// Creates a <see cref="QuazarServer"/> with the specified parameters.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="port"></param>
        /// <param name="Waypoint"></param>
        /// <param name="backlog"></param>
        protected TPWSEServer(string name, int port, SIMThemeParkWaypoints Waypoint, uint backlog = 1) : base(name, port, backlog)
        {
            ThisWaypoint = Waypoint;
        }       
    }
}
