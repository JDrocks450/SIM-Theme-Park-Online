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

namespace TPWSE.ClientApplication.Pages.ParksControl
{
    /// <summary>
    /// Interaction logic for ParksSearchPage.xaml
    /// </summary>
    public partial class ParksSearchPage : Page
    {
        public event EventHandler<string> OnSearch;

        public ParksSearchPage()
        {
            InitializeComponent();
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                OnSearch?.Invoke(this, SearchBox.Text);
        }
    }
}
