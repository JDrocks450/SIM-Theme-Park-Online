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
using TPWSE.ClientServices;
using TPWSE.ClientServices.Clients;

namespace TPWSE.ClientApplication.Pages
{
    /// <summary>
    /// Interaction logic for OnlineParkPage.xaml
    /// </summary>
    public partial class OnlineParkPage : Page
    {
        private bool FollowMode = false;
        private string FollowingClient = default;

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
            client.OnErrorThrown += OnErrorThrown;
            client.OnPlayerMoved += OnPlayerMoved;
        }

        private void OnPlayerMoved(object sender, TPWChatMoveEventArgs e)
        {
            Dispatcher.Invoke(delegate
            {
                if (e.Player.String == FollowingClient)
                {
                    //AddChatLog("$INFO", $"You're moving with {FollowingClient} to: {e.NewLocation}");
                    if (!e.IsTeleporting)
                        chatClient.Move(e.NewLocation);
                }
            });
        }

        private void OnErrorThrown(object sender, TPWConnectionErrorEventArgs e)
        {
            Dispatcher.Invoke(delegate
            {
                return;
                AddChatLog("$ERROR", 
                    $"ERROR!\n\n{e.Data?.Message ?? "No error message."}\n{e.DynamicPacket?.ToString()}\n\n" +
                    $"{(e.PacketFields != null ? string.Join("\n", e.PacketFields) : "No fields found.")}");
            });
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
                if (e.IsChatMessage)
                    AddChatLog(e.Sender, e.Message);
                if (e.IsPrivateMessage)
                    AddPM(e.Sender, e.Message);
                if (e.IsShoutMessage)
                    AddShoutLog(e.Sender, e.Message);                    
            });            
        }        

        private void Setup()
        {
            PlayersView.Children.Clear();
            ChatLogView.Children.Clear();
            AddChatLog("$INFO", $"You are currently visiting {chatClient.ConnectedChatRoom.ParkName} as {PlayerInfo.PlayerName}...");
            ThemeParkNameLabel.Text = chatClient.ConnectedChatRoom.ParkName;
            chatClient.RequestAllPlayers();
            chatClient.Teleport(TPWClientConstants.InitialLocation);
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

        private void AddShoutLog(TPWUnicodeString Sender, TPWUnicodeString Content)
        {
            var chatText = new TextBlock();
            ChatLogView.Children.Add(new Border() { BorderThickness = new Thickness(4, 0, 0, 0), Child = chatText });
            chatText.Foreground = Brushes.Red;
            chatText.FontWeight = FontWeights.Bold;
            Brush foreGround = Brushes.Red;
            if (chatPlayerBrushes.ContainsKey(Sender))
                foreGround = chatPlayerBrushes[Sender];
            chatText.Inlines.Add(new Run(Sender + " shouts ") { Foreground = foreGround, FontWeight = FontWeights.Bold });
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
                Brush foreCol = Brushes.DarkTurquoise;
                contentBorder.BorderBrush = foreCol;
                if (chatPlayerBrushes.ContainsKey(Sender))
                {
                    if (Sender == PlayerInfo.PlayerName)
                        chatText.Foreground = foreCol;
                    //foreCol = chatPlayerBrushes[Sender];                    
                }                
                chatText.Inlines.Add(new Run(Sender + "  ") { FontWeight = FontWeights.Bold });
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
            chatPlayerBrushes.Add(PlayerName, Brushes.MediumPurple);
            double bThickness = 3;
            chatText.Inlines.Add(new Run(PlayerName));
            chatText.Inlines.Add(new Run(isQuazarPlayer ? " TPW-SE App" : " In-Game") { Foreground = chatPlayerBrushes[PlayerName], FontWeight = FontWeights.Regular });
            if (PlayerName != PlayerInfo.PlayerName)
            {
                chatText.Cursor = Cursors.Hand;
                chatText.ToolTip = "Click on this player's name to send them a Buddy request";
                chatText.MouseLeftButtonUp += delegate
                {
                    chatClient.AddBuddy(PlayerName);
                };
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
            if (MessageText.StartsWith("/follow"))
            {
                if (MessageText.Length > 8)
                {
                    MessageText = MessageText.Remove(0, 8);
                    try
                    {
                        if (chatClient.OnlinePlayers.Any(x => x.PlayerName == MessageText))
                        {
                            FollowingClient = MessageText;
                            FollowMode = true;
                            AddChatLog("$INFO", $"Following {FollowingClient}, everytime they move, your character will move to their location.");                            
                            return;
                        }
                    }
                    catch
                    {

                    }
                }
                FollowMode = false;
                if (FollowMode)
                {
                    AddChatLog("$INFO", "Follow-Mode is now off.");
                    return;
                }
                AddChatLog("$ERROR", "Couldn't follow that person. Make sure to spell their name right - with capitalization, please. (/follow <name>)");
                return;

            }
            if (MessageText.StartsWith("/tell"))
            {
                if (MessageText.Length > 6)
                {
                    MessageText = MessageText.Remove(0, 6);
                    string[] numbers = MessageText.Split(' ');
                    if (numbers.Length == 2)
                    {
                        try
                        {
                            chatClient.SendTellMessage(numbers[0], numbers[1]);
                            Chatbox.Text = "";
                            return;
                        }
                        catch
                        {

                        }
                    }
                }
                AddChatLog("$ERROR", "You wanna tell who? Make sure to use proper syntax. (/tell <Name> <Message>)");
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
