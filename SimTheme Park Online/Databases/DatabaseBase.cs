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
        private readonly Dictionary<T1, T2> DataCollection = new Dictionary<T1, T2>();
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

        protected void WriteDatabase(string FileName, params KeyValuePair<T1, T2>[] Data) {
            Directory.CreateDirectory(Path.GetDirectoryName(FileName));               
            using (var fs = File.Create(FileName)) WriteDatabase(fs, Data); 
        }
        protected void WriteDatabase(Stream Datastream, params KeyValuePair<T1, T2>[] Data)
        {                     
            Datastream.Write(JsonSerializer.SerializeToUtf8Bytes(Data));
            QuazarAPI.QConsole.WriteLine("ServerProfile", "Server Profile has been successfully saved.");
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
        public virtual IEnumerable<T2> GetAllData() => DataCollection.Values;

        public bool AddData(T1 Key, T2 Value)
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
    }
}
