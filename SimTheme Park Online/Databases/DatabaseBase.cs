using QuazarAPI;
using SimTheme_Park_Online.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Databases
{
    /// <summary>
    /// A abstract class for the general functionality of a database.
    /// </summary>
    /// <typeparam name="T1">The means of retrieving data</typeparam>
    /// <typeparam name="T2">The data structure stored in this database.</typeparam>
    [Serializable]    
    public abstract class DatabaseBase<T1, T2> : IDatabaseInterface<T1, T2> where T2 : IDatabaseObject
    {
        public readonly String FileName;
        public readonly String Name;

        private Thread pushQueue;

        private bool _hasChanges = false;

        /// <summary>
        /// The general collection of data stored in this database. 
        /// <para>Inheriting types should use other functionality other than this to ensure data integrity.</para>
        /// </summary>
        protected readonly Dictionary<T1, T2> DataCollection = new Dictionary<T1, T2>();
        public int AmountOfEntries => DataCollection.Count;
        private readonly Dictionary<string, T1[]> SpecialCollections = new Dictionary<string, T1[]>();
        protected readonly Queue<KeyValuePair<T1, T2>> TaskQueue = new Queue<KeyValuePair<T1, T2>>();

        /// <summary>
        /// Creates a new Database with a name.
        /// </summary>
        /// <param name="Name"></param>
        protected DatabaseBase(string Name, string FileName)
        {
            if (string.IsNullOrWhiteSpace(Name))            
                throw new ArgumentException($"'{nameof(Name)}' cannot be null or whitespace.", nameof(Name));            
            this.Name = Name;
            this.FileName = FileName;
            pushQueue = new Thread(_doBackgroundWorker);
            pushQueue.Start();
        }

        /// <summary>
        /// Special lists allow you to specify lists of keys in this database that can be grouped together to make data easier to find. 
        /// </summary>
        /// <param name="Name">The name of this list</param>
        /// <param name="Keys">The keys to point to in this database</param>
        public bool ApplySpecialList(string Name, params T1[] Keys)
        {
            if (SpecialListExists(Name))
            {
                foreach (var key in Keys)
                    AppendSpecialListEntry(Name, key);
                return true;
            }
            QConsole.WriteLine(Name, $"The category '{Name}' has been applied with value(s): {string.Join(' ', Keys)}");
            SpecialCollections.Add(Name, Keys);
            return true;
        }
        /// <summary>
        /// Special lists allow you to specify lists of keys in this database that can be grouped together to make data easier to find. 
        /// </summary>
        /// <param name="Name">The name of this list</param>
        public IEnumerable<T2> GetSpecialListEntries(string Name)
        {
            foreach (var key in SpecialCollections[Name])            
                yield return DataCollection[key];            
        }

        protected void WriteDatabase(string FileName, params KeyValuePair<T1, T2>[] Data) {
            Directory.CreateDirectory(Path.GetDirectoryName(FileName));               
            using (var fs = File.Create(FileName)) WriteDatabase(fs, Data); 
        }
        protected void WriteDatabase(Stream Datastream, params KeyValuePair<T1, T2>[] Data)
        {                     
            Datastream.Write(JsonSerializer.SerializeToUtf8Bytes(Data));
            QuazarAPI.QConsole.WriteLine(Name, "Database has been written to file.");
        }

        public async Task LoadFromFile(string FileName)
        {
            using (var fs = File.OpenText(FileName))
                await LoadFromStream(fs);
        }
        public async Task<KeyValuePair<T1, T2>[]> LoadFromStream(StreamReader Datastream)
        {
            var array = JsonSerializer.Deserialize<KeyValuePair<T1, T2>[]>(await Datastream.ReadToEndAsync());
            DataCollection.Clear();
            foreach (var item in array)
                DataCollection.Add(item.Key, item.Value);
            return array;
        }

        public virtual bool TryGetValue(T1 Key, out T2 Value) => DataCollection.TryGetValue(Key, out Value);
        public virtual bool TryGetValue(string SpecialName, T1 Key, out T2 Value)
        {
            Value = default(T2);
            if (!SpecialCollections.TryGetValue(SpecialName, out T1[] values))
                return false;
            int index = Array.BinarySearch<T1>(values, Key);
            if (index >= 0)
            {
                Value = DataCollection[Key];
                return true;
            }
            return false;
        }
        public virtual IEnumerable<T2> GetAllData() => DataCollection.Values;

        public virtual bool AddData(T1 Key, T2 Value)
        {
            if (DataCollection.ContainsKey(Key))
                return false;
            TaskQueue.Enqueue(new KeyValuePair<T1, T2>(Key, Value));
            return true;
        }

        public bool RemoveData(T1 Key)
        {
            throw new NotImplementedException();
        }

        private void _doBackgroundWorker()
        {
            while (true)
            {
                if (TaskQueue.Count == 0) continue;
                _hasChanges = true;
                while (TaskQueue.TryDequeue(out var task))
                {
                    KeyValuePair<T1, T2>[] dataSource;
                    lock (DataCollection)
                    {
                        int index = DataCollection.Count;
                        dataSource = new KeyValuePair<T1, T2>[index + 1];
                        DataCollection.ToArray().CopyTo(dataSource, 0);
                        dataSource[index] = task;
                    }
                    //WriteDatabase(FileName, dataSource);
                    lock (DataCollection)
                    {
                        DataCollection.Add(task.Key, task.Value);                        
                    }
                    QConsole.WriteLine(Name, $"{task.Value.GetType().Name} has been added to the database.");
                }
                _hasChanges = false;
            }
        }

        public void AwaitAllChanges()
        {
            while (_hasChanges)
            {

            }
        }

        public IEnumerable<T2> Search(Func<T2, bool> SearchFunction) => DataCollection.Values.Where(SearchFunction);
        public abstract T1 CreateKey();
        public abstract T CreateValue<T>(string ValueName);
        public bool SpecialListExists(string SpecialListName) => SpecialCollections.TryGetValue(SpecialListName, out _);
        public bool AppendSpecialListEntry(string SpecialListName, T1 Value)
        {
            if (!SpecialListExists(SpecialListName))
                return false;
            if (TryGetValue(SpecialListName, Value, out _))
                return false;
            var array = SpecialCollections[SpecialListName];
            Array.Resize(ref array, array.Length + 1);
            array[array.Length - 1] = Value;
            SpecialCollections[SpecialListName] = array;
            QConsole.WriteLine(Name, $"Added to the category: {SpecialListName}, {Value}");
            return true;
        }
    }
}
