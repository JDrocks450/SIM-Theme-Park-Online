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

namespace TPWAPI.Frontend.Pages
{
    /// <summary>
    /// Interaction logic for StartupOptionsPage.xaml
    /// </summary>
    public partial class StartupOptionsPage : Page
    {
        public StartupOptionsPage()
        {
            InitializeComponent();
            PortWindow.Visibility = Visibility.Collapsed;
            var management = ApplicationResources.Management = new SimTheme_Park_Online.ServerManagement();
            Loginport.Text = management.Config.LOGIN_PORT.ToString();
            NewsPort.Text = management.Config.NEWS_PORT.ToString();
            CityPort.Text = management.Config.CITY_PORT.ToString();
            ChatPort.Text = management.Config.CHAT_PORT.ToString();
            FTPPort.Text = management.Config.FTP_PORT.ToString();

            LoginCheck.IsChecked = management.Config.LOGIN_ENABLE;
            NewsCheck.IsChecked = management.Config.NEWS_ENABLE;
            CityCheck.IsChecked = management.Config.CITY_ENABLE;
            ChatCheck.IsChecked = management.Config.CHAT_ENABLE;
            FTPCheck.IsChecked = management.Config.FTP_ENABLE;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            var management = ApplicationResources.Management;
            try
            {
                management.Profile.ConfigurationSettings = new SimTheme_Park_Online.TPWServerConfig()
                {
                    LOGIN_PORT = int.Parse(Loginport.Text),
                    NEWS_PORT = int.Parse(NewsPort.Text),
                    CITY_PORT = int.Parse(CityPort.Text),
                    CHAT_PORT = int.Parse(ChatPort.Text),
                    FTP_PORT = int.Parse(FTPPort.Text),

                    LOGIN_ENABLE = LoginCheck.IsChecked ?? false,
                    NEWS_ENABLE = NewsCheck.IsChecked ?? false,
                    CITY_ENABLE = CityCheck.IsChecked ?? false,
                    CHAT_ENABLE = ChatCheck.IsChecked ?? false,
                    FTP_ENABLE = FTPCheck.IsChecked ?? false,
                };
                ApplicationResources.Management.SaveProfile(management.Profile);
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error saving profile: " + exc.Message);
            }

            management.Initialize();
            management.StartAll();
            (Application.Current.MainWindow as MainWindow).SwitchScreen(new MainPage());
        }

        private void PortButton_Click(object sender, RoutedEventArgs e)
        {
            OptionsWindow.Visibility = Visibility.Collapsed;
            PortWindow.Visibility = Visibility.Visible;
        }

        private void PortOKButton_Click(object sender, RoutedEventArgs e)
        {
            OptionsWindow.Visibility = Visibility.Visible;
            PortWindow.Visibility = Visibility.Collapsed;
        }
    }
}
