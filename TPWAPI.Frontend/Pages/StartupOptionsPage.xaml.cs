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
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            var management = ApplicationResources.Management = new SimTheme_Park_Online.ServerManagement();
            management.StartAll();
            (Application.Current.MainWindow as MainWindow).SwitchScreen(new MainPage());
        }
    }
}
