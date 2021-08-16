using QuazarAPI;
using QuazarAPI.Networking.Standard;
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
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            ConsoleOutput.Text = "";
            QConsole.OnLogUpdated += delegate(object s, (string orig, string format) tuple)
            {
                Dispatcher.Invoke(delegate
                {
                    ConsoleOutput.Text += tuple.format + "\n";
                });
            };
            foreach (var entry in QConsole.Log)
                ConsoleOutput.Text += entry + "\n";
        }

        private void ShowInformationWindow(Component ServerComponent)
        {
            ServerInformation serverInfoWindow = new ServerInformation(ServerComponent);
            serverInfoWindow.Show();            
        }

        private void LoginInfoButton_Click(object sender, RoutedEventArgs e)
        {
            ShowInformationWindow(ApplicationResources.Management.LoginServer);
        }
    }
}
