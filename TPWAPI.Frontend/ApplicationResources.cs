using SimTheme_Park_Online;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPWAPI.Frontend.Pages;

namespace TPWAPI.Frontend
{
    internal static class ApplicationResources
    {
        internal static ServerManagement Management { get; set; } 

        internal static void ShowPropertiesWindow(TPWPacket packet)
        {
            PacketProperties properties = new PacketProperties(packet);
            properties.Show();
        }
    }
}
