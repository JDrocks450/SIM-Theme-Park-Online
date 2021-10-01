using SimTheme_Park_Online.Data;
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
    /// Interaction logic for LoginScreen.xaml
    /// </summary>
    public partial class LoginScreen : Page
    {
        public LoginScreen()
        {
            InitializeComponent();
        }

        LoginClient LoginClient; 

        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            StatusBarText.Text = "Connecting...";
            IsEnabled = false;
            string username = UsernameBox.Text,
                password = PasswordBox.Password;
            LoginClient = new LoginClient(System.Net.IPAddress.Loopback, 7598);
            try
            {
                var response = await LoginClient.AttemptLogin(username, password);
                if (response == null)
                    throw new Exception("TPW-SE Servers are not reachable right now. Please try again later.");
                if (response.IsSuccessfulLogin)
                {
                    UsernameField.Text = username;
                    PasswordField.Text = new string('*', password.Length);
                    PlayerIDField.Text = response.PlayerID.ToString();
                    CustIDField.Text = response.CustomerID.ToString();
                    StatusBarText.Text = "Sign-In Successful";
                    TPWPlayerInfo player = new TPWPlayerInfo(response.PlayerID, response.CustomerID, username);
                    OnSuccessfulAuth(player);
                }
                else
                    throw new Exception("There was an error logging in: Auth Error, Bad Username / Password.");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                StatusBarText.Text = "Not Connected";
            }
            LoginClient.Dispose();
            IsEnabled = true;            
        }

        private void OnSuccessfulAuth(TPWPlayerInfo PlayerInfo)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow == null)
                return;
            mainWindow.ChangeScreen(new OnlineWorldScreen(PlayerInfo));
        }
    }
}
