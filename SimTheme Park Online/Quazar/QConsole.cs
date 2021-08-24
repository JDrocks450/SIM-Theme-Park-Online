using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuazarAPI
{
    public class QConsole
    {
        public static List<string> TotalLog { get; } = new List<string>();
        public delegate void OnOutputHandler(string Channel, string Unformatted, string Formatted);
        public static event OnOutputHandler OnLogUpdated;
        public static ushort CONSOLE_WIDTH = 50;

        private static Dictionary<string, List<string>> Channels = new Dictionary<string, List<string>>();

        public static void WriteLine(string Channel, string message)
        {
            List<string> Log = GetLogByChannel(Channel);
            var header = $"--- {DateTime.Now} ";
            var nmessage = (message.Contains('\n')) ?
                $"{header}{new string('-', CONSOLE_WIDTH - header.Length)} \n" +
                $"[{Channel}]: {message}\n" +
                $"{new string('-', CONSOLE_WIDTH)} \n" 
                :
                $"[{DateTime.Now}] - [{Channel}]: {message}";
            Console.WriteLine(nmessage);
            lock (Log)
            {
                Log.Add(nmessage);
            }
            TotalLog.Add(nmessage);
            OnLogUpdated?.Invoke(Channel, message, nmessage);            
        }

        public IEnumerable<string> GetAllOutput()
        {
            return TotalLog;
        }

        private static List<string> GetLogByChannel(string channel)
        {
            lock (Channels)
            {
                if (Channels.TryGetValue(channel, out var Log))
                    return Log;
                else
                {
                    Channels.Add(channel, new List<string>());
                    return Channels[channel];
                }
            }
        }
    }
}
