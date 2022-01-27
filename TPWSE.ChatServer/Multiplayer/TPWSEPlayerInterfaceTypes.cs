namespace TPWSE.ChatServer.Multiplayer
{
    internal enum TPWSEPlayerInterfaceTypes
    {
        /// <summary>
        /// The original client of the game, using the original BOSS 1.0 commandset, original game command compatible only.
        /// </summary>
        SIMThemeParkClient,
        /// <summary>
        /// A client that implements TPWSE.BOSS commandset, compatible with additional commands from the Quazar.TPWSE standard.
        /// </summary>
        QuazarClient
    }
}
