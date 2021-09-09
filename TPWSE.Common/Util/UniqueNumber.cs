using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Util
{
    public static class UniqueNumber
    {
        private static Random random = new Random();
        public static uint Generate(IEnumerable<uint> IDs)
        {
            uint ID = (uint)random.Next(0, int.MaxValue);
            while (IDs.Contains(ID))            
                ID = (uint)random.Next(0, int.MaxValue);            
            return ID;
        }
    }
}
