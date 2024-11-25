using Microsoft.Win32;
using QuazarAPI;
using SimTheme_Park_Online;
using SimTheme_Park_Online.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static TPWAPI.Frontend.ApplicationResources;

namespace TPWAPI.Frontend.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private Dictionary<string, TextBox> Consoles = new Dictionary<string, TextBox>();

        public MainPage()
        {
            InitializeComponent();

            ConsoleOutput.Text = "";
            QConsole.OnLogUpdated += delegate(string channel, string orig, string formatted)
            {
                Dispatcher.Invoke(delegate
                {
                    AddConsoleOutput(channel, formatted);
                });
            };
            foreach (var entry in QConsole.TotalLog)
                AddConsoleOutput("", entry);
        }

        private void AddConsoleOutput(string channel, string formatted)
        {
            if (!string.IsNullOrWhiteSpace(channel))
            {
                if (Consoles.TryGetValue(channel, out TextBox box))
                {
                    box.Text += formatted + "\n";
                    box.ScrollToEnd();
                }
                else
                {
                    AddChannel(channel, formatted);
                }
            }
            ConsoleOutput.Text += formatted + "\n";
            ConsoleOutput.ScrollToEnd();
        }

        private void PacketMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FileDialog dialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                Multiselect = false
            };
            if (dialog.ShowDialog() ?? false)
            {
                byte[] fileBuffer = null;
                using (var fs = File.OpenRead(dialog.FileName))
                {
                    fileBuffer = new byte[fs.Length];
                    fs.Read(fileBuffer, 0, (int)fs.Length);
                }
                try
                {
                    var packets = SimTheme_Park_Online.TPWPacket.ParseAll(ref fileBuffer);
                    foreach(var packet in packets)
                        ApplicationResources.ShowPropertiesWindow(packet);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void AddChannel(string channel, string formatted)
        {
            TabItem newItem;
            ConsoleTabs.Items.Insert(1, newItem = new TabItem()
            {
                Header = channel
            });
            newItem.Content = new Border()
            {
                Child = new TextBox()
                {
                    Text = formatted + "\n"
                }
            };
            Consoles.Add(channel, (newItem.Content as Border).Child as TextBox);
        }        

        private void LoginInfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Management.Config.LOGIN_ENABLE) { 
                MessageBox.Show("This component is disabled. Restart the application and enable this component to view it.");
                return;
            }
            ShowInformationWindow(ApplicationResources.Management.LoginServer);
        }

        private void NewsInfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Management.Config.NEWS_ENABLE) { 
                MessageBox.Show("This component is disabled. Restart the application and enable this component to view it.");
                return;
            }
            ShowInformationWindow(ApplicationResources.Management.NewsServer);
        }

        private void CityServerInfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Management.Config.CITY_ENABLE)
            {
                MessageBox.Show("This component is disabled. Restart the application and enable this component to view it.");
                return;
            }
            ShowInformationWindow(ApplicationResources.Management.CityServer);
        }

        private void ChatServerInfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Management.Config.CHAT_ENABLE)
            {
                MessageBox.Show("This component is disabled. Restart the application and enable this component to view it.");
                return;
            }
            ShowInformationWindow(Management.ChatServer);
        }

        private void GameNewsEdit_Click(object sender, RoutedEventArgs e)
        {
            TextEdit textEdit = new TextEdit();
            textEdit.InputtedText = ApplicationResources.CurrentProfile.GameNewsString;
            textEdit.Closed += delegate
            {
                if (textEdit.Cancelled) return;
                CurrentProfile.UpdateGameNews(textEdit.InputtedText);
            };
            textEdit.Owner = Application.Current.MainWindow;
            textEdit.Show();
        }

        private void SystemNewsEdit_Click(object sender, RoutedEventArgs e)
        {
            TextEdit textEdit = new TextEdit();
            textEdit.InputtedText = ApplicationResources.CurrentProfile.SystemNewsString;
            textEdit.Closed += delegate
            {
                if (textEdit.Cancelled) return;
                CurrentProfile.UpdateSystemNews(textEdit.InputtedText);
            };
            textEdit.Owner = Application.Current.MainWindow;
            textEdit.Show();
        }

        private void WorldViewButton_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow).SwitchScreen(new OnlineWorldPage());
        }

        private void STRFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FileDialog dialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                Multiselect = false,
                Title = "BFST file you want to view"
            };
            if (dialog.ShowDialog() ?? false)
            {
                var bytes = File.ReadAllBytes(dialog.FileName);
                dialog = new OpenFileDialog()
                {
                    CheckFileExists = true,
                    Multiselect = false,
                    Title = "Locate MBtoUni.dat in your game folder"
                };
                if (dialog.ShowDialog() ?? false) {
                    PresentSTRWindow(bytes, new Uri(dialog.FileName));
                }
            }
        }

        private void PresentSTRWindow(byte[] STRFileData, Uri MBtoUniPath)
        {
            var strings = SimTheme_Park_Online.Util.FileFormats.BFSTReader.GetStrings(STRFileData, MBtoUniPath, true);
            var window = new Window()
            {
                WindowStyle = WindowStyle.ToolWindow,
                Title = $"Showing {strings.Count} Strings",
                Content = new ListBox()
                {
                    ItemsSource = strings
                }
            };
            window.Show();
        }

        private void ServerTellItem_Click(object sender, RoutedEventArgs e)
        {
            TextEdit editor = new TextEdit();
            editor.ShowDialog();
            ServerManagement.Current.ChatServer.ServerSay(editor.InputtedText);
        }
    }
}
