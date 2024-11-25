using Cassandra;
using QuazarAPI;
using SimTheme_Park_Online.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Databases
{
    public class TPWCityDatabase : DatabaseBase<uint, TPWCityInfo>
    {
        private uint highestKey = 0xA;

        public TPWCityDatabase(string FileName) : base("Cities Database", FileName)
        {        
            try
            {
                var task = LoadFromFile(FileName);
                task.Wait();
                if (!task.IsCompletedSuccessfully)
                    throw task.Exception;
            }
            catch
            {
                GenerateTestCityDB();
            }
        }
        public override bool AddData(uint Key, TPWCityInfo City) => base.AddData(Key, City);
        public bool AddCity(TPWCityInfo City) => this.AddData(City.CityID, City);

        public override uint CreateKey() => QuazarAPI.Util.UniqueNumber.Generate(DataCollection.Keys);
        public override T CreateValue<T>(string ValueName)
        {
            return default;
        }

        private void GenerateTestCityDB()
        {
            //city locations
            Vector3 Loc1 = Util.SphericalCoordinateConverter.ToCartesian(120, 0, 0);
            Vector3 Loc2 = Util.SphericalCoordinateConverter.ToCartesian(120, 90, 20);
            Vector3 Loc3 = Util.SphericalCoordinateConverter.ToCartesian(120, 120, 90);

            //cities
            AddCity(new TPWCityInfo(0x0000000A, "North Pole", "bullfrog", 2.140f, 0.0f, 119.98f, 0x0B, 100, 0x00, "bullfrog", 0x01, 0x0F));
            AddCity(new TPWCityInfo(0x0000000B, "Bloaty Land", "Bisquick", 100, 70, 20, 0x20, 0x03, 0x05, "bullfrog", 0x01, 0x10));
            AddCity(new TPWCityInfo(0x0000000C, "Radical Jungle", "System", Loc2.X, Loc2.Y, Loc2.Z, 0x20, 0x03, 0x05, "bullfrog", 0x01, 0x10));
            AddCity(new TPWCityInfo(0x0000000D, "Sector 7", "System", Loc1.X, Loc1.Y, Loc1.Z, 0x20, 0x03, 0x05, "bullfrog", 0x01, 0x10));
            AddCity(new TPWCityInfo(0x0000000E, "Charvatia", "System", Loc3.X, Loc3.Y, Loc3.Z, 0x20, 0x03, 0x05, "bullfrog", 0x01, 0x10));
            AwaitAllChanges();

            QConsole.WriteLine("CityDB", $"Generated {AmountOfEntries} Cities successfully.");
        }
    }
}
