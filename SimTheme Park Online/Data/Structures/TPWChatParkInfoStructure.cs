using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Data.Structures
{
    public class TPWChatParkInfoStructure : TPWListStructure
    {
        public TPWChatParkInfoStructure(string CreatedBy,
                                        string Email,
                                        uint Population,
                                        byte[] IMAGE,
                                        string Theme,
                                        string Name,
                                        uint ChartPos,
                                        uint Votes,
                                        uint Visits,
                                        uint PARAM1,
                                        uint PARAM2,
                                        long SZ,
                                        DateTime DT,
                                        uint PARAM3,
                                        uint PARAM4,
                                        uint PARAM5,
                                        uint PARAM6,
                                        uint PARAM7)
            : base(0x02,
                   CreatedBy,
                   Email,
                   Population,
                   IMAGE,
                   Theme,
                   Name,
                   ChartPos,
                   Votes,
                   Visits,
                   PARAM1,
                   PARAM2,
                   SZ,
                   DT,
                   PARAM3,
                   PARAM4,
                   PARAM5,
                   PARAM6,
                   PARAM7)
        {

        }
    }
}
