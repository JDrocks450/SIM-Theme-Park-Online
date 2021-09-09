using QuazarAPI;
using SimTheme_Park_Online.Data;
using SimTheme_Park_Online.Data.Packets;
using SimTheme_Park_Online.Data.Primitive;
using SimTheme_Park_Online.Database;
using SimTheme_Park_Online.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TPWSE.ChatServer.Multiplayer
{

    /// <summary>
    /// Represents an online room that interacts with Theme Park World and SIM Theme Park.
    /// </summary>
    internal class TPWOnlineRoom
    {
        /// <summary>
        /// ChatServer connection ID, PlayerInfo
        /// </summary>
        private Dictionary<uint, TPWPlayerInfo> _players = new Dictionary<uint, TPWPlayerInfo>();
        /// <summary>
        /// A dictionary of PlayerID, PlayerGameState objects
        /// </summary>
        private Dictionary<uint, TPWPlayerGameState> _playerStates = new Dictionary<uint, TPWPlayerGameState>();

        /// <summary>
        /// The current players in this room
        /// </summary>
        public List<TPWPlayerInfo> Players { get; } = new List<TPWPlayerInfo>();

        /// <summary>
        /// The park to use for this Online game session.
        /// </summary>
        internal uint ParkID { get; set; }
        /// <summary>
        /// The amount of players currently in this park
        /// </summary>
        internal uint PlayerCount => (uint)_players.Count;

        internal TPWOnlineRoom(uint ParkID)
        {
            this.ParkID = ParkID;
        }

        /// <summary>
        /// Uses the provided database to get the park by the attached <see cref="ParkID"/>
        /// </summary>
        /// <param name="ParksDatabase">The database with the park contained inside</param>
        /// <param name="Park">The found park</param>
        /// <returns>True if the park was found</returns>
        internal bool TryGetOnlinePark(IDatabaseInterface<uint, TPWParkInfo> ParksDatabase, out TPWParkInfo Park) =>
            ParksDatabase.TryGetValue("Chatellites", ParkID, out Park);
        internal bool TryGetOnlinePlayerByConnection(uint ConnectionID, out TPWPlayerInfo Player) => 
            _players.TryGetValue(ConnectionID, out Player);
        internal bool AttemptTranslate(uint ConnectionID, out TPWPlayerGameState State)
        {
            State = null;
            if (!TryGetOnlinePlayerByConnection(ConnectionID, out var player))
                return false;
            return TryGetOnlineState(player.PlayerID, out State);
        }
        internal bool TryGetOnlineState(uint PlayerID, out TPWPlayerGameState State) => 
            _playerStates.TryGetValue(PlayerID, out State);

        /// <summary>
        /// Get information about this park's online game session
        /// </summary>
        /// <returns></returns>
        internal TPWChatRoomInfoPacket GetRoomInfoPacket(IDatabaseInterface<uint, TPWParkInfo> Database)
        {
            if (!TryGetOnlinePark(Database, out var Park))
                throw new Exception("Huh, couldn't find that park to get RoomInfo for.");
            QConsole.WriteLine("ChatServer", $"{ParkID}: Found info for my park, returning my RoomInfo...");
            return new TPWChatRoomInfoPacket(Park.ParkName, ParkID, PlayerCount, 1024, 30);
        }        

        /// <summary>
        /// Enters the player into this online game session
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        internal bool Admit(uint ConnectionID, TPWPlayerInfo player)
        {
            if (_players.ContainsKey(ConnectionID))
                return false;            
            player.SetInParkStatus(true, ParkID);
            Players.Add(player);
            _players.Add(ConnectionID, player);
            if (!AttemptTranslate(ConnectionID, out _))
            {
                _playerStates.Add(player.PlayerID, new TPWPlayerGameState(player.PlayerID));                
            }
            else
            {
                QConsole.WriteLine("ChatServer", $"{player.PlayerName}'s state was loaded from cache from a previous play session in this room.");
            }
            QConsole.WriteLine("ChatServer", $"{player.PlayerName} was allowed in {ParkID}!");
            return true;
        }

        /// <summary>
        /// Creates a request to move the player indicated by the ConnectionID.
        /// </summary>
        /// <param name="ConnectionID"></param>
        /// <param name="Location"></param>
        /// <param name="PlayerIndex"></param>
        /// <param name="MovementType"></param>
        /// <param name="NewPosition"></param>
        /// <returns></returns>
        internal bool MovePlayer(uint ConnectionID, TPWPosition Location, bool Teleport,
                                 out TPWConstants.TPWChatPlayerMovementTypes MovementType, 
                                 out TPWPosition NewPosition)
        {
            MovementType = TPWConstants.TPWChatPlayerMovementTypes.None;
            NewPosition = Location;
            if (!AttemptTranslate(ConnectionID, out var state))
                return false;
            _ = TryGetOnlinePlayerByConnection(ConnectionID, out var player);
            MovementType = state.MovePlayer(Location, Teleport, out bool isFromMovement);
            if (isFromMovement)
                QConsole.WriteLine("ChatServer", $"{player.PlayerName} is moving from " + Location);
            else
                switch (MovementType)
                {
                    case TPWConstants.TPWChatPlayerMovementTypes.Walk:
                        QConsole.WriteLine("ChatServer", $"{player.PlayerName} is walking to: " + NewPosition);
                        break;
                    case TPWConstants.TPWChatPlayerMovementTypes.Teleport:
                        QConsole.WriteLine("ChatServer", $"{player.PlayerName} teleported to: " + NewPosition);
                        break;
                }    
            return true; 
        }

        /// <summary>
        /// Removes a player from this room, note that this will not kick or force DC the client - just removes them from appearing in this room.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        internal bool Exit(uint ConnectionID)
        {
            if (!TryGetOnlinePlayerByConnection(ConnectionID, out var player))
                return false;
            player.SetInParkStatus(false);
            QConsole.WriteLine("ChatServer", $"{player.PlayerName} left {ParkID}!");
            _playerStates.Remove(player.PlayerID);
            return _players.Remove(ConnectionID);
        }

        internal List<(TPWPlayerInfo Player, TPWPlayerGameState State)> GetConnectedPlayersInfo()
        {
            var list = new List<(TPWPlayerInfo, TPWPlayerGameState)>();
            foreach(var player in Players)
            {
                if (!TryGetOnlineState(player.PlayerID, out var state))
                    state = new TPWPlayerGameState(player.PlayerID, new TPWPosition(600, 600));
                list.Add((player, state));
            }
            return list;
        }
    }
}
