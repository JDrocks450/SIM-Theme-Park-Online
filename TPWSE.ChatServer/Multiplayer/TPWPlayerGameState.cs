using System;
using System.Numerics;
using static SimTheme_Park_Online.Data.TPWConstants;
using SimTheme_Park_Online.Primitive;

namespace TPWSE.ChatServer.Multiplayer
{
    /// <summary>
    /// Represents the state of a player in a <see cref="TPWOnlineRoom"/>
    /// </summary>
    internal class TPWPlayerGameState
    {        
        public TPWPosition CurrentPosition
        {
            get; private set;
        }
        public bool IsCurrentlyMoving => CurrentMovement == TPWChatPlayerMovementTypes.Walk;
        public TPWChatPlayerMovementTypes CurrentMovement { get; private set; }
        public TPWPosition DesiredPosition
        {
            get; private set;
        }
        public uint PlayerID { get; }

        private bool _fromPositionFlag = true;

        public TPWPlayerGameState(uint PlayerID, TPWPosition InitialPosition = default)
        {
            CurrentPosition = InitialPosition;
            this.PlayerID = PlayerID;
        }

        public TPWChatPlayerMovementTypes MovePlayer(TPWPosition DesiredPosition, bool Teleport, out bool IsFromPosition)
        {
            IsFromPosition = false;
            TPWChatPlayerMovementTypes SetWalkTo()
            {
                this.DesiredPosition = DesiredPosition;
                CurrentMovement = TPWChatPlayerMovementTypes.Walk;
                return TPWChatPlayerMovementTypes.Walk;
            }
            if (!Teleport) // player is starting to move
                return SetWalkTo();
            else
            {
                CurrentMovement = TPWChatPlayerMovementTypes.None;
                CurrentPosition = this.DesiredPosition = DesiredPosition;
                return TPWChatPlayerMovementTypes.Teleport;
            }
        }
    }
}
