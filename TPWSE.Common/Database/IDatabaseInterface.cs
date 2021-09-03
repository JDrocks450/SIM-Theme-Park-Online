using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Database
{
    public interface IDatabaseInterface<T1, T2> where T2 : IDatabaseObject
    {
        bool TryGetValue(T1 Key, out T2 Value);
        IEnumerable<T2> GetAllData();
        bool AddData(T1 Key, T2 Value);
        bool RemoveData(T1 Key);
        IEnumerable<T2> Search(Func<T2, bool> SearchFunction);
    }
}
