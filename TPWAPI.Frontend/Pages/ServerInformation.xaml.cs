using QuazarAPI.Networking.Standard;
using SimTheme_Park_Online;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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
        }

        private void PopulateConnections()
        {
            var clients = serverComponent.GetAllConnectedClients();
            ConnectionsListBox.Items.Clear();
            foreach(var cx in clients)
            {
                var listBox = new ListViewItem()
                {
                    Content = cx.Name,
                    Background = Brushes.Turquoise,
                    Foreground = Brushes.White,
                    Tag = cx
                };
                ConnectionsListBox.Items.Add(listBox);
            }
            ConnectionsLabel.Text = $"Connections: {clients.Count()}";
            Title = $"STPOnline\\{serverComponent.Name} - {serverComponent.GetAllConnectedClients().Count()}/{serverComponent.BACKLOG} Connections";
        }

        private void ConnectionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
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
    }
}
