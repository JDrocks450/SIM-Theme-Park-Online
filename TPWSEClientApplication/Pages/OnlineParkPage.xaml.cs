using SimTheme_Park_Online.Data;
using SimTheme_Park_Online.Data.Primitive;
using SimTheme_Park_Online.Primitive;
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
    /// Interaction logic for OnlineParkPage.xaml
    /// </summary>
    public partial class OnlineParkPage : Page
    {
        private readonly TPWPlayerInfo PlayerInfo;
        private ChatClient chatClient;
        private Dictionary<string, Brush> chatPlayerBrushes = new Dictionary<string, Brush>();        

        public OnlineParkPage()
        {
            InitializeComponent();
        }

        public OnlineParkPage(TPWPlayerInfo Info, ChatClient Client) : this()
        {
            AttachClient(Client);
            PlayerInfo = Info;
            Setup();
        }

        public void AttachClient(ChatClient client)
        {
            chatClient = client;
            client.OnOnlineChatReceived += OnChatReceived;
            client.OnPlayerJoin += OnPlayerJoin;
            client.OnDisconnect += OnDisconnect;
        }

        private void OnDisconnect(object sender, QuazarAPI.Networking.Standard.QEventArgs<Exception> e)
        {
            MessageBox.Show("You have been disconnected from SIM Theme Park Online. Please try again in a little while.");
            Environment.Exit(1);
        }

        private void OnPlayerJoin(object sender, QuazarAPI.Networking.Standard.QEventArgs<TPWPlayerInfo> e)
        {
            bool result = chatClient.TryGetIsQuazarClient(e.Data.PlayerID, out bool isQuazar);
            Dispatcher.Invoke(delegate
            {                
                AddPlayer(e.Data.PlayerName, result ? isQuazar : false);
            });
        }

        private void OnChatReceived(object sender, TPWChatEventArgs e)
        {
            Dispatcher.Invoke(delegate
            {
                if (e.IsPrivateMessage)
                    AddPM(e.Sender, e.Message);
                else
                    AddChatLog(e.Sender, e.Message);
            });            
        }        

        private void Setup()
        {
            PlayersView.Children.Clear();
            ChatLogView.Children.Clear();
            AddChatLog("$INFO", $"You are currently visiting {chatClient.ConnectedChatRoom.ParkName} as {PlayerInfo.PlayerName}...");
            ThemeParkNameLabel.Text = chatClient.ConnectedChatRoom.ParkName;
            chatClient.RequestAllPlayers();
        }

        private void AddPM(TPWUnicodeString Sender, TPWUnicodeString Content)
        {
            var chatText = new TextBlock();
            ChatLogView.Children.Add(new Border() { BorderThickness = new Thickness(4,0,0,0), Child = chatText });
            chatText.Foreground = Brushes.Silver;
            chatText.FontStyle = FontStyles.Italic;
            Brush foreGround = Brushes.Silver;
            if (chatPlayerBrushes.ContainsKey(Sender))
                foreGround = chatPlayerBrushes[Sender];
            chatText.Inlines.Add(new Run("privately from " + Sender + "  ") { Foreground = foreGround, FontWeight = FontWeights.Bold });
            chatText.ToolTip = new TextBlock() { Text = $"Respond with /tell {Sender} <PMsg>" };
            chatText.Inlines.Add(new Run(Content));
        }

        private void AddChatLog(string Sender, string Content)
        {
            var chatText = new TextBlock();
            var contentBorder = new Border() { Child = chatText };
            ChatLogView.Children.Add(contentBorder);
            if (Sender == "$INFO")
            {
                chatText.Foreground = Brushes.Silver;
                chatText.FontStyle = FontStyles.Italic;
            }
            else if (Sender == "$ERROR")
            {
                chatText.Foreground = Brushes.Silver;
                chatText.FontStyle = FontStyles.Italic;
                contentBorder.BorderBrush = Brushes.Red;
            }
            else
            {
                Brush foreCol = Brushes.Silver;
                if (chatPlayerBrushes.ContainsKey(Sender))
                {
                    if (Sender == PlayerInfo.PlayerName)
                        chatText.Foreground = chatPlayerBrushes[Sender];
                    foreCol = chatPlayerBrushes[Sender];
                    contentBorder.BorderBrush = chatPlayerBrushes[Sender];
                }
                else chatText.Foreground = foreCol;
                chatText.Inlines.Add(new Run(Sender + "  ") { Foreground = foreCol, FontWeight = FontWeights.Bold });
            }
            chatText.Inlines.Add(new Run(Content));
        }

        /// <summary>
        /// Adds a player to the roster with the option of displaying that they're in the TPWSE app.
        /// </summary>
        /// <param name="PlayerName">The name of this player</param>
        /// <param name="isQuazarPlayer">Is this player on Quazar or not?</param>
        private void AddPlayer(string PlayerName, bool isQuazarPlayer = false)
        {
            if (chatPlayerBrushes.ContainsKey(PlayerName))
                return;
            var chatText = new TextBlock();
            chatPlayerBrushes.Add(PlayerName, Brushes.Turquoise);
            double bThickness = 3;
            chatText.Inlines.Add(new Run(PlayerName));
            chatText.Inlines.Add(new Run(isQuazarPlayer ? " TPW-SE App" : " In-Game") { Foreground = Brushes.Turquoise, FontWeight = FontWeights.Regular });
            if (PlayerName == PlayerInfo.PlayerName)
            {
                chatText.Foreground = chatPlayerBrushes[PlayerName];
                bThickness = 5;
            }
            PlayersView.Children.Add(new Border()
            {
                BorderThickness = new Thickness(bThickness, 0, 0, 0),
                BorderBrush = chatPlayerBrushes[PlayerName],
                Child = chatText
            });
        }

        private void ShowMapBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ShowMapBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private async void MoveVirtualPlayer(TPWPosition Position, bool Teleport = false)
        {
            await chatClient.Move(Position);
        }

        private void SendChatMessage(string MessageText)
        {
            if (MessageText.StartsWith("/move"))
            {
                if (MessageText.Length > 6)
                {
                    MessageText = MessageText.Remove(0, 6);
                    string[] numbers = MessageText.Split(',');
                    if (numbers.Length == 2)
                    {
                        try
                        {
                            TPWPosition position = new TPWPosition(uint.Parse(numbers[0]), uint.Parse(numbers[1]));
                            MoveVirtualPlayer(position);
                            Chatbox.Text = "";
                            return;
                        }
                        catch
                        {

                        }
                    }
                }
                AddChatLog("$ERROR", "Couldn't move your character since the parameters weren't 2 numbers separated by a comma. (/move <number>,<number>)");
                return;
            }
            chatClient.SendChatMessage(PlayerInfo.PlayerName, MessageText);
            Chatbox.Text = "";
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendChatMessage(Chatbox.Text);   
        }

        private void Chatbox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendChatMessage(Chatbox.Text);
        }
    }
}
