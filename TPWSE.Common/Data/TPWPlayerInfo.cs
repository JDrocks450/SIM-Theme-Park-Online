using SimTheme_Park_Online.Data.Primitive;
using SimTheme_Park_Online.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Data
{
    [Serializable]
    public class TPWPlayerInfo : IDatabaseObject
    {
        public uint PlayerID { get; }
        public uint CustomerID { get; }
        public TPWUnicodeString PlayerName { get; set; }
        public SecureString SecurePassword { get; } = new SecureString();
        
        public bool CurrentlyInPark { get; private set; }        
        public uint ParkID { get; private set; }

        public TPWPlayerInfo(uint PlayerID, uint CustomerID, TPWUnicodeString PlayerName)
        {
            this.PlayerID = PlayerID;
            this.CustomerID = CustomerID;
            this.PlayerName = PlayerName;
        }

        public void SetInParkStatus(bool InPark, uint? ParkID = null)
        {
            CurrentlyInPark = InPark;
            if (InPark)
            {
                if (ParkID == null)
                {
                    CurrentlyInPark = false;
                    ParkID = 0;                                        
                }
                else
                    this.ParkID = ParkID.Value;
            }
            QuazarAPI.QConsole.WriteLine("ChatServer", $"{PlayerName} In Park? {CurrentlyInPark} | ParkID: {this.ParkID}");
        }
    }
}
