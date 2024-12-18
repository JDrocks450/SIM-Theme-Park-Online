﻿using QuazarAPI.Networking.Standard;
using SimTheme_Park_Online;
using SimTheme_Park_Online.Data.Templating;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using TPWAPI.Frontend.Pages;

namespace TPWAPI.Frontend
{
    internal static class ApplicationResources
    {
        internal static ServerManagement Management { get; set; }
        public static ServerProfile CurrentProfile => Management.Profile;

        internal static Dictionary<TPWSystemTypes, SolidColorBrush > SystemSelectionBrushes =
            new Dictionary<SimTheme_Park_Online.Data.Templating.TPWSystemTypes, SolidColorBrush>()
        {
                { TPWSystemTypes.BYTE, Brushes.Purple },
                { TPWSystemTypes.WORD, Brushes.DarkTurquoise },
                { TPWSystemTypes.DWORD, Brushes.DarkCyan },
                { TPWSystemTypes.UNI_STR, Brushes.Green },
                { TPWSystemTypes.ASCII_STR, Brushes.Violet },
                { TPWSystemTypes.BYTE_ARR, Brushes.DarkBlue }
        };

        internal static void ShowPropertiesWindow(TPWPacket packet, TPWDataTemplate Template = default)
        {
            PacketProperties properties = new PacketProperties(packet);
            if (Template != null)
                properties.SetTemplater(Template);
            else if (packet.HasDataTemplate)
                properties.SetTemplater(packet.GetTemplate());
            properties.Owner = Application.Current.MainWindow;
            properties.Show();
        }

        internal static void ShowInformationWindow(TPWSEServer ServerComponent)
        {
            ServerInformation serverInfoWindow = new ServerInformation(ServerComponent);
            serverInfoWindow.Owner = Application.Current.MainWindow;
            serverInfoWindow.Show();            
        }
    }
}
