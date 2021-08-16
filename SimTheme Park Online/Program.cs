using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using QuazarAPI;
using QuazarAPI.Networking;
using QuazarAPI.Networking.Standard;

namespace SimTheme_Park_Online
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Quazar for SimTheme Park Starting...");
            ServerManagement Management = new ServerManagement();
            Management.StartAll();
            while (true)
            {
                string command = Console.ReadLine();
                //Management.LoginServer.Send(0, "sB", command);
            }
        }
    }
}
