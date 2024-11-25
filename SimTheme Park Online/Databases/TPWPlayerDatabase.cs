using SimTheme_Park_Online.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Databases
{
    public class TPWPlayerDatabase : DatabaseBase<uint, TPWPlayerInfo>
    {
        uint _currentPlayerID = 0, _currentCustID = 0;
        public TPWPlayerDatabase(string FileName) : base("Player Database", FileName)
        {

        }

        public override uint CreateKey() => QuazarAPI.Util.UniqueNumber.Generate(DataCollection.Keys);
        public override T CreateValue<T>(string ValueName)
        {
            //if (ValueName == "CustomerID")
                //return _currentCustID;
            return default;
        }
    }
}
