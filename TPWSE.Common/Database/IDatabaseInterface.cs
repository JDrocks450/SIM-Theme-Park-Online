using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Database
{
    /// <summary>
    /// Allows a generic way to interact with a database in TPW-SE.
    /// <para>Each database in the game should implement this interface to allow compatibility and code clarity.</para>
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public interface IDatabase<T1, T2> where T2 : IDatabaseObject
    {
        bool TryGetValue(T1 Key, out T2 Value);
        bool TryGetValue(string SpecialListName, T1 Key, out T2 Value);
        IEnumerable<T2> GetAllData();
        bool AddData(T1 Key, T2 Value);
        bool RemoveData(T1 Key);
        IEnumerable<T2> Search(Func<T2, bool> SearchFunction);
        IEnumerable<T2> GetSpecialListEntries(string SpecialListName);
        bool ApplySpecialList(string SpecialListName, params T1[] Keys);
        bool SpecialListExists(string SpecialListName);
        bool AppendSpecialListEntry(string SpecialListName, T1 Value);
        int AmountOfEntries { get; }
        T1 CreateKey();
        T CreateValue<T>(string ValueName);
    }
}
