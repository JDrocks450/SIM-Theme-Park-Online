using Microsoft.Win32;
using QuazarAPI.Networking.Standard;
using SimTheme_Park_Online;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TPWAPI.Frontend.Pages
{
    /// <summary>
    /// Interaction logic for ServerInformation.xaml
    /// </summary>
    public partial class ServerInformation : Window
    {
        private readonly Component serverComponent;

        public ServerInformation(Component ServerComponent)
        {
            InitializeComponent();
            serverComponent = ServerComponent;

            serverComponent.OnConnectionsUpdated += delegate
            {
                Dispatcher.Invoke(delegate
                {
                    PopulateConnections();
                });
            };
            serverComponent.IncomingTrans.CollectionChanged += IncomingTrans_CollectionChanged;
            serverComponent.OutgoingTrans.CollectionChanged += OutgoingTrans_CollectionChanged;
            ConnectionsListBox.SelectionChanged += ConnectionsListBox_SelectionChanged;
            Dispatcher.Invoke(() =>
                OutgoingData.ItemsSource = serverComponent.OutgoingTrans.ToArray());
            Dispatcher.Invoke(() =>
                IncomingData.ItemsSource = serverComponent.IncomingTrans.ToArray());
            PopulateConnections();
            PopulateAdminPanel();
        }

        private void PopulateAdminPanel()
        {
            if (serverComponent.ManualMode)
                ManualModeButton.Content = "Disable Manual Mode";
            else ManualModeButton.Content = "Enable Manual Mode";
        }

        private void PopulateConnections()
        {
            var clients = serverComponent.GetAllConnectedClients();
            ConnectionsListBox.Items.Clear();
            foreach(var cx in clients)
            {
                var listBox = new ListViewItem()
                {
                    Content = cx.Client.Client.RemoteEndPoint.ToString(),
                    Background = Brushes.Turquoise,
                    Foreground = Brushes.White,
                    Tag = cx.ID
                };
                ConnectionsListBox.Items.Add(listBox);
            }
            ConnectionsGrid.ItemsSource = serverComponent.ConnectionHistory.ToList();
            ConnectionsLabel.Text = $"Connections: {clients.Count()}";
            Title = $"STPOnline\\{serverComponent.Name} - {clients.Count()}/{serverComponent.BACKLOG} Connections";
        }

        private void ConnectionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DisconnectButton.IsEnabled = true;
        }

        private void OutgoingTrans_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
                OutgoingData.ItemsSource = serverComponent.OutgoingTrans.ToArray());
        }

        private void IncomingTrans_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
                IncomingData.ItemsSource = serverComponent.IncomingTrans.ToArray());
        }

        public void ShowPropertiesWindow(TPWPacket packet) => ApplicationResources.ShowPropertiesWindow(packet);

        private void IncomingData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowPropertiesWindow(IncomingData.SelectedItem as TPWPacket);
        }

        private void OutgoingData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowPropertiesWindow(OutgoingData.SelectedItem as TPWPacket);
        }

        private void ManualModeButton_Click(object sender, RoutedEventArgs e)
        {
            serverComponent.ManualMode = !serverComponent.ManualMode;
            PopulateAdminPanel();
        }

        private void SendPacketButton_Click(object sender, RoutedEventArgs e)
        {
            if (ConnectionsListBox.SelectedIndex == -1)
            {
                MessageBox.Show("No selected connection found");
                return;
            }
            uint selectedID = (uint)(ConnectionsListBox.SelectedItem as ListBoxItem).Tag;
            void SendPacket(string path)
            {
                try
                {
                    var fs = File.ReadAllBytes(path);
                    var packets = TPWPacket.ParseAll(ref fs);
                    serverComponent.ManualSend(0, packets.ToArray());
                    MessageBox.Show($"Manually Sent {packets.Count()} Packets to Client: 0");
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
            if (string.IsNullOrEmpty(LibraryFileBox.Text))
            {
                OpenFileDialog dialog = new OpenFileDialog()
                {
                    Title = "Select a TPWPacket File"
                };
                if (dialog.ShowDialog() ?? false)
                {
                    SendPacket(dialog.FileName);
                }
                return;
            }
            else SendPacket(System.IO.Path.Combine("Library", LibraryFileBox.Text));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(ConnectionsListBox.SelectedItem == default)
            {
                MessageBox.Show("There is no selected client to disconnect.");
                return;
            }
            ListBoxItem item = (ConnectionsListBox.SelectedItem as ListBoxItem);
            uint client = (uint)item.Tag;
            serverComponent.Disconnect(client);
        }
    }
}
