using SimTheme_Park_Online;
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

namespace TPWAPI.Frontend.Pages.Packet_Properties
{
    /// <summary>
    /// Interaction logic for HeaderPage.xaml
    /// </summary>
    public partial class HeaderPage : Page
    {
        public HeaderPage()
        {
            InitializeComponent();
        }

        public void PopulateData(TPWPacket ViewingPacket)
        {
            //Header
            ResponseCodeField.Text = $"{string.Join(' ', ViewingPacket.ResponseCode.Select(x => (char)x))}";
            ResponseCodeEncode.Text = $"{{ { string.Join(',', ViewingPacket.ResponseCode) } }}";

            MsgCodeField.Text =        ViewingPacket.MsgType.ToString();
            Param1Field.Text =         ViewingPacket.Language.ToString();
            Param2Field.Text =         ViewingPacket.Param2.ToString();
            Param3Field.Text =         ViewingPacket.Param3.ToString();
            BodyLenField.Text =        ViewingPacket.BodyLength.ToString();
            PacketPriorityField.Text = ViewingPacket.PacketQueue.ToString();
            PacketHeaderSizeRun.Text = ViewingPacket.HeaderLength.ToString();
        }
    }
}
