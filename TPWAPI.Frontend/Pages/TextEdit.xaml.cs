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
    /// Interaction logic for TextEdit.xaml
    /// </summary>
    public partial class TextEdit : Window
    {
        public bool Cancelled = true;

        public string InputtedText
        {
            get => TextBox.Text;
            set => TextBox.Text = value;
        }

        public TextEdit()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Cancelled = false;
            Close();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                int index = TextBox.CaretIndex;
                TextBox.Text = TextBox.Text.Insert(index, "\n");
                TextBox.CaretIndex = index + 1;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
