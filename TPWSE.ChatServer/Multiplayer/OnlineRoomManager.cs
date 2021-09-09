using QuazarAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPWSE.ChatServer.Multiplayer
{
    internal class OnlineRoomManager
    {
        private Dictionary<uint, TPWOnlineRoom> _rooms = new Dictionary<uint, TPWOnlineRoom>();

        internal TPWOnlineRoom CreateRoom(uint ParkID)
        {
            var room = new TPWOnlineRoom(ParkID);
            _rooms.Add(ParkID, room);
            QConsole.WriteLine("ChatServer", $"Created a new TPWOnlineRoom for the park: {ParkID}");
            return room;
        }

        internal bool TryGetRoom(uint ParkID, out TPWOnlineRoom Room) => _rooms.TryGetValue(ParkID, out Room);
    }
}
