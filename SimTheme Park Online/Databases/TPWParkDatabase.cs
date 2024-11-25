using Cassandra;
using SimTheme_Park_Online.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Databases
{
    public class TPWParkDatabase : DatabaseBase<uint, TPWParkInfo>
    {
        uint highestKey = 0;
        public TPWParkDatabase(string FileName) : base("Parks Database", FileName)
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
                GenerateTestParkDB();
                ApplySpecialList("Chatellites", new uint[]{
                    16, 21, 24, 32, 48
                });
            }
        }

        public override bool AddData(uint Key, TPWParkInfo Park)
        {
            ApplySpecialList("CITYID=" + Park.CityID.ToString(), Key);
            return base.AddData(Key, Park);
        }
        public bool AddPark(TPWParkInfo Park) => AddData(Park.ParkID, Park);

        public override uint CreateKey() => QuazarAPI.Util.UniqueNumber.Generate(DataCollection.Keys);
        public override T CreateValue<T>(string ValueName) => default;

        private void GenerateTestParkDB()
        {
            AddPark(new TPWParkInfo()
            {
                OwnerName = "Bisquick",
                OwnerEmail = "admin@bullfrog.com",
                ParkID = 16,
                ParkName = "Bisquick's Testing Zone",
                Description = "This park is for testing purposes. Thanks for tuning in! The codebase behind this breakthrough is Quazar.TPW-SE, " +
                    "an API for interacting with an unmodified, SIM Theme Park 1.0 and 2.0 installation. - Jeremy, Bisquick",
                Visits = 0x10,
                Votes = 0x05,
                CityID = 0x0A,
                ChartPosition = 0x01,
                ThemeID = 0x01,
                DownloadedRides = "0",
                Key = TimeUuid.Parse("A423D062-26D7-11D4-87E5-0090271E1063")
            });
            AddPark(new TPWParkInfo()
            {
                OwnerName = "admin",
                OwnerEmail = "admin@bullfrog.com",
                ParkID = 21,
                ParkName = "Test Center 3",
                Description = "Not much is known about this park, but most people who enter it never" +
                " say much about what they saw...",
                Visits = 0x03,
                Votes = 0x00,
                CityID = 0x0B,
                ChartPosition = 0x02,
                ThemeID = 0x03,
                DownloadedRides = "0",
                Key = TimeUuid.Parse("F891EC60-3A3E-11D4-87E6-0090271E1063")
            });
            AddPark(new TPWParkInfo()
            {
                OwnerName = "TwistyT",
                OwnerEmail = "admin@bullfrog.com",
                ParkID = 24,
                ParkName = "Radical Twister",
                Description = "Founded on the idea that the only good theme park ride is one with serious whiplash," +
                " this park really does test the limits of the human body! Riding these rides is not recommended.",
                Visits = 0x20,
                Votes = 0x50,
                CityID = 0x0C,
                ChartPosition = 0x03,
                ThemeID = 0x02,
                DownloadedRides = "0"
            });
            AddPark(new TPWParkInfo()
            {
                OwnerName = "Bisquick",
                OwnerEmail = "admin@bullfrog.com",
                ParkID = 32,
                ParkName = "Oceanic Adventures",
                Description = "Who doesn't like adventures in oceans? Most people. That's why we're different! " +
                "We take the oceans into space to create a life-changing experience!",
                Visits = 0x00,
                Votes = 0x00,
                CityID = 0x0D,
                ChartPosition = 0x05,
                ThemeID = 0x00,
                DownloadedRides = "0"
            });
            AddPark(new TPWParkInfo()
            {
                OwnerName = "admin",
                OwnerEmail = "admin@bullfrog.com",
                ParkID = 48,
                ParkName = "Burrito Village",
                Description = "Our park prides itself on not having rides, but instead only food stands. We only sell " +
                "burritos. Before you ask, yes we have bathrooms - is there a fee? Absolutely.",
                Visits = 0x5C,
                Votes = 0x03,
                CityID = 0x0E,
                ChartPosition = 0x03,
                ThemeID = 0x01,
                DownloadedRides = "0"
            });
            AwaitAllChanges();

            QuazarAPI.QConsole.WriteLine("ParksDB", $"Generated {AmountOfEntries} Parks successfully.");
        }
    }
}
