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
        /// <summary>
        /// The current position of this player
        /// </summary>
        public TPWPosition CurrentPosition
        {
            get; private set;
        }
        /// <summary>
        /// Is this player currently walking / moving somewhere?
        /// </summary>
        public bool IsCurrentlyMoving => CurrentMovement == TPWChatPlayerMovementTypes.Walk;
        /// <summary>
        /// The position they are currently at
        /// </summary>
        public TPWChatPlayerMovementTypes CurrentMovement { get; private set; }
        /// <summary>
        /// The position this player wants to be at.
        /// </summary>
        public TPWPosition DesiredPosition
        {
            get; private set;
        }
        /// <summary>
        /// This player's PlayerID
        /// </summary>
        public uint PlayerID { get; }
        /// <summary>
        /// The originating client's interface for connecting to TPWSE.
        /// </summary>
        internal TPWSEPlayerInterfaceTypes ClientInterface { get; private set; }

        private bool _fromPositionFlag = true;

        /// <summary>
        /// Creates a new <see cref="TPWPlayerGameState"/> for this PlayerID and initializes it.
        /// </summary>
        /// <param name="PlayerID">The player ID of the player this game state represents in this <see cref="TPWOnlineRoom"/></param>
        /// <param name="InitialPosition">The initial position of this person.</param>
        /// <param name="Interface">The Client this player joined with, either the original game client or a modified QuazarClient from a different source.</param>
        public TPWPlayerGameState(uint PlayerID, TPWPosition InitialPosition = default, TPWSEPlayerInterfaceTypes Interface = TPWSEPlayerInterfaceTypes.SIMThemeParkClient)
        {
            ClientInterface = Interface;
            CurrentPosition = InitialPosition;
            this.PlayerID = PlayerID;
        }

        /// <summary>
        /// Submits a request to move this player, which passes through some verification
        /// </summary>
        /// <param name="DesiredPosition">The position you want this person to be at</param>
        /// <param name="Teleport">Whether or not they are teleporting there</param>
        /// <param name="IsFromPosition">No longer used</param>
        /// <returns></returns>
        public bool MovePlayer(TPWPosition DesiredPosition, TPWChatPlayerMovementTypes Teleport, out bool IsFromPosition)
        {
            IsFromPosition = false;
            bool SetWalkTo()
            {
                this.DesiredPosition = DesiredPosition;
                CurrentMovement = TPWChatPlayerMovementTypes.Walk;
                return true;
            }
            if (Teleport != TPWChatPlayerMovementTypes.Teleport) // player is starting to move
                return SetWalkTo();
            else
            {
                CurrentMovement = TPWChatPlayerMovementTypes.Teleport;
                CurrentPosition = this.DesiredPosition = DesiredPosition;
                return true;
            }
        }
    }
}
