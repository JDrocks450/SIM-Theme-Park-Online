using SimTheme_Park_Online;
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
using System.Windows.Shapes;

namespace TPWAPI.Frontend.Pages
{
    /// <summary>
    /// Interaction logic for PacketProperties.xaml
    /// </summary>
    public partial class PacketProperties : Window
    {
        public PacketProperties(SimTheme_Park_Online.TPWPacket ViewingPacket)
        {
            InitializeComponent();
            this.ViewingPacket = ViewingPacket;
            PopulateData();
        }

        public TPWPacket ViewingPacket { get; }

        private void PopulateData()
        {
            //Header
            ResponseCodeField.Text = $"{string.Join(' ', ViewingPacket.ResponseCode.Select(x => (char)x))}";
            ResponseCodeEncode.Text = $"{{ { string.Join(',', ViewingPacket.ResponseCode) } }}";

            MsgCodeField.Text =        ViewingPacket.MsgType.ToString();
            Param1Field.Text =         ViewingPacket.Param1.ToString();
            Param2Field.Text =         ViewingPacket.Param2.ToString();
            Param3Field.Text =         ViewingPacket.Param3.ToString();
            BodyLenField.Text =        ViewingPacket.BodyLength.ToString();
            PacketPriorityField.Text = ViewingPacket.PacketQueue.ToString();
            PacketHeaderSizeRun.Text = ViewingPacket.HeaderLength.ToString();

            //Body


            //Raw
            TextView.Text = Encoding.ASCII.GetString(ViewingPacket.Body);
            ByteView.Text = string.Join(' ', ViewingPacket.Body);
        }

        private void PacketBodyButton_Click(object sender, RoutedEventArgs e)
        {
            TabSwitch.SelectedItem = BodyTab;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
