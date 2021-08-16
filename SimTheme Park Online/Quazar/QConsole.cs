using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuazarAPI
{
    public class QConsole
    {
        public static HashSet<string> Log { get; } = new HashSet<string>();
        public static event EventHandler<(string original, string formatted)> OnLogUpdated;
        public static ushort CONSOLE_WIDTH = 50;
        public static void WriteLine(string message)
        {
            var header = $"--- {DateTime.Now} ";
            var nmessage = (message.Contains('\n')) ?
                $"{header}{new string('-', CONSOLE_WIDTH - header.Length)} \n" +
                $"{message}\n" +
                $"{new string('-', CONSOLE_WIDTH)} \n" 
                :
                $"[{DateTime.Now}] {message}";
            Console.WriteLine(nmessage);
            lock (Log)
            {
                Log.Add(nmessage);
            }
            OnLogUpdated?.Invoke(null, (message, nmessage));
        }
    }
}
