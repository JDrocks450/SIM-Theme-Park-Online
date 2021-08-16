using Microsoft.Win32;
using QuazarAPI;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace TPWAPI.Frontend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();            
        }

        public void SwitchScreen(Page page)
        {
            WindowContent.Content = page;
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
                    var packets = SimTheme_Park_Online.TPWPacket.ParseAll(fileBuffer);
                    foreach(var packet in packets)
                        ApplicationResources.ShowPropertiesWindow(packet);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
