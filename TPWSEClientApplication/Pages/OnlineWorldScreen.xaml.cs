using SimTheme_Park_Online.Data;
using SimTheme_Park_Online.Data.Primitive;
using SimTheme_Park_Online.Data.Structures;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TPWSE.ClientServices.Clients;

namespace TPWSE.ClientApplication.Pages
{
    /// <summary>
    /// Interaction logic for OnlineWorldScreen.xaml
    /// </summary>
    public partial class OnlineWorldScreen : Page
    {
        public readonly TPWPlayerInfo PlayerInfo;
        private CityClient cityClient;
        private ChatClient chatClient;

        public OnlineWorldScreen()
        {
            InitializeComponent();
            cityClient = new CityClient(System.Net.IPAddress.Loopback, 7591);
            cityClient.StatusChanged += CityClient_StatusChanged;
            chatClient = new ChatClient(System.Net.IPAddress.Loopback, 7593);

            init();
            ParksControl.OnSearch += ParksControl_OnSearch;
        }

        private async void ParksControl_OnSearch(object sender, string username)
        {
            ParksControl.ShowWait();
            IEnumerable<TPWParkInfo> parks = await cityClient.GetParksByUser(username);
            ParksControl.ShowParks($"Parks by {username}", parks, chatClient.OnlineChatRooms);
        }

        private async void init()
        {
            bool connectionResult = await AwaitConnectionAsync();
            if (!connectionResult) return;            
            IEnumerable<DWORD> parkIDs = 
                cityClient.OnlineParks.Select(x => (DWORD)x.ParkID);
            while (true)
            {
                try
                {
                    await chatClient.Connect();
                    break;
                }
                catch (Exception ex) // CONNECTION ERROR
                {
                    MessageBox.Show(ex.Message);
                }
            }
            TPWChatRoomInfo[] RoomInfos = await chatClient.DownloadChatRoomInfo(parkIDs.ToArray());
            _ = Dispatcher.InvokeAsync(delegate
            {
                PopulateCities();
                PopulateOnlineParks(RoomInfos);
                OnlineGameConnectionStatus.Text = "Downloading Online Chat Room Info...";
                OnlineGameConnectionStatus.Visibility = Visibility.Collapsed;
            });
        }

        private void CityClient_StatusChanged(object sender, CityClient.StatusCodes e)
        {
            Dispatcher.Invoke(delegate
            {
                CityStatusLabel.Text = $"Downloading Online Cities ({e.ToString()}) ...";
            });
        }

        public OnlineWorldScreen(TPWPlayerInfo playerInfo) : this()
        {
            PlayerInfo = playerInfo;
        }        

        private async Task<bool> AwaitConnectionAsync()
        {
            try
            {
                await cityClient.AttemptConnectionAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                ((MainWindow)Application.Current.MainWindow).ChangeScreen(new LoginScreen()); // boot to login screen
                                                                                              
                return false;
            }
            return true;
        }

        private void PopulateCities()
        {            
            foreach(var city in cityClient.Cities)
            {
                var stack = new StackPanel();
                stack.Children.Add(new TextBlock() { FontWeight = FontWeights.Bold, Text = city.CityName, Margin = new Thickness(0,5,0,0) });
                stack.Children.Add(new Separator() { Margin = new Thickness(0,5,0,5) });
                stack.Children.Add(new TextBlock() { Text = "Amount of Parks: " + city.AmountOfParks });
                stack.Children.Add(new TextBlock() { FontStyle = FontStyles.Italic, Text = "City ID: " + city.CityID.ToString() });

                var control = new ContentControl()
                {
                    Content = stack,
                    Cursor = Cursors.Hand,
                    Tag = city
                };
                control.MouseLeftButtonUp += CitySelected;

                CitiesView.Children.Add(control);
            }
            CityStatusLabel.Visibility = Visibility.Collapsed;
        }

        private async void CitySelected(object sender, MouseButtonEventArgs e)
        {
            var city = (sender as FrameworkElement).Tag as TPWCityInfo;
            IEnumerable<TPWParkInfo> Parks = await cityClient.GetParksByCity(city);
            ParksControl.ShowParks($"Top 10 Parks in {city.CityName}", Parks, chatClient.OnlineChatRooms);
        }
        /// <summary>
        /// Link online parks with a list of chatrooms and display the result.
        /// </summary>
        /// <param name="OnlineChatRooms"></param>
        private void PopulateOnlineParks(TPWChatRoomInfo[] OnlineChatRooms)
        {
            int notFoundParks = 0;
            foreach (var park in cityClient.OnlineParks)
            {
                var roomInfo = OnlineChatRooms?.FirstOrDefault(x => x.ParkID == park.ParkID) ?? null;
                if (roomInfo == null) notFoundParks++;
                var parkControl = new ContentControl() { Tag = roomInfo };
                UXResources.CreateParkControl(ref parkControl, park, OnlineChatRooms, true);
                parkControl.Cursor = Cursors.Hand;
                parkControl.MouseLeftButtonUp += OnlineParkSelected;
                OnlineSessionsView.Children.Add(parkControl);
            }
            if (notFoundParks > 0)
                DelistedChatRoomsButton.Content += $" ({OnlineChatRooms.Length})";
            else DelistedChatRoomsButton.IsEnabled = false;
        }
        /// <summary>
        /// Display just online chat rooms without consulting the CityServer's resulting online parks.
        /// </summary>
        private void PopulateOnlineRooms(IEnumerable<TPWChatRoomInfo> OnlineChatRooms)
        {
            OnlineSessionsView.Children.Clear();
            foreach(var room in OnlineChatRooms)
            {
                var parkControl = new ContentControl() { Tag = room };
                UXResources.CreateChatInfoControl(ref parkControl, room);
                parkControl.Cursor = Cursors.Hand;
                parkControl.MouseLeftButtonUp += OnlineParkSelected;
                OnlineSessionsView.Children.Add(parkControl);
            }
        }

        private async void OnlineParkSelected(object sender, MouseButtonEventArgs e)
        {
            if (chatClient.IsOnlineRoomConnected)            
                return;
            IsEnabled = false;
            TPWChatRoomInfo room = (TPWChatRoomInfo)(sender as ContentControl).Tag;
            if (room == null) return;
            bool success = await chatClient.OnlineRoomConnect(PlayerInfo, room);
            IsEnabled = true;
            if (!success)
            {
                MessageBox.Show("There was an internal server error connecting to that chat room. " +
                    "Please try your request again later.");
                return;
            }
            MainWindow window = (MainWindow)Application.Current.MainWindow;
            window.ChangeScreen(new OnlineParkPage(PlayerInfo, chatClient));
        }

        private void DelistedChatRoomsButton_Click(object sender, RoutedEventArgs e)
        {
            DelistedChatRoomsButton.Visibility = Visibility.Collapsed;
            PopulateOnlineRooms(chatClient.OnlineChatRooms);
        }
    }
}
