using SimTheme_Park_Online.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TPWSE.ClientApplication
{
    internal static class UXResources
    {       
        /// <summary>
        /// Creates a ParkInfo control with all the standard fields supplied. Additionally, this can check an array
        /// of OnlineChatRooms to see if this park is Online or not.
        /// </summary>
        /// <param name="Control"></param>
        /// <param name="Info"></param>
        /// <param name="OnlineChatRooms"></param>
        public static void CreateParkControl(ref ContentControl Control, TPWParkInfo Info, IEnumerable<TPWChatRoomInfo> OnlineChatRooms = default, bool showOffline = false)
        {
            var park = Info;
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock()
            {
                FontWeight = FontWeights.Bold,
                Text = $"{park.ParkName} | Rank #{park.ChartPosition}",
                Margin = new Thickness(0, 5, 0, 0)
            });

            stack.Children.Add(new Separator() { Margin = new Thickness(0, 5, 0, 5) });

            stack.Children.Add(new TextBlock() { Text = "Owner: " + park.OwnerName });
            stack.Children.Add(new TextBlock() { Text = "Owner Email: " + park.OwnerEmail });
            stack.Children.Add(new TextBlock() { Text = "Created: " + park.DateCreated });
            stack.Children.Add(new TextBlock() { Text = "Theme: " + park.ThemeID });

            stack.Children.Add(new Separator() { Margin = new Thickness(0, 5, 0, 5) });

            stack.Children.Add(new TextBox() { Foreground = Brushes.White, BorderThickness = new Thickness(0), Background = Brushes.Transparent, MaxHeight = 50, TextWrapping = TextWrapping.Wrap, Text = park.Description });

            stack.Children.Add(new Separator() { Margin = new Thickness(0, 5, 0, 5) });

            var roomInfo = OnlineChatRooms?.FirstOrDefault(x => x.ParkID == park.ParkID) ?? null;
            if (roomInfo != null)
            {
                stack.Children.Add(new TextBlock() { FontWeight = FontWeights.Bold, Foreground = Brushes.LightGreen, Text = "Status: Online" });
                stack.Children.Add(new TextBlock() { Text = $"{roomInfo.NumberOfPlayers} in this park right now" });
            }
            else if (showOffline)
                stack.Children.Add(new TextBlock() { FontWeight = FontWeights.Bold, Foreground = Brushes.Red, Text = "Status: Offline" });

            stack.Children.Add(new Separator() { Margin = new Thickness(0, 5, 0, 5) });

            stack.Children.Add(new TextBlock() { FontStyle = FontStyles.Italic, Text = "Park ID: " + park.ParkID.ToString() });

            Control.Content = stack;
        }
    }
}
