using SimTheme_Park_Online;
using SimTheme_Park_Online.Data;
using SimTheme_Park_Online.Data.Primitive;
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

namespace TPWSE.ClientApplication.Pages.ParksControl
{
    /// <summary>
    /// Interaction logic for ParksControl.xaml
    /// </summary>
    public partial class ParksControl : UserControl
    {
        public event EventHandler<string> OnSearch;
        ParksSearchPage SearchPage = new ParksSearchPage();

        public ParksControl()
        {
            InitializeComponent();
            SearchPage.OnSearch += (object sender, string SearchTerm) => OnSearch?.Invoke(sender, SearchTerm);
            ShowSearch();
        }

        public void ShowSearch()
        {                        
            ParksControlContent.Navigate(SearchPage);
        }

        /// <summary>
        /// Invokes this control to enter a waiting state while a task is running.
        /// </summary>
        public void ShowWait()
        {
            ParksControlContent.Navigate(this, new ParksWaitingPage());
        }

        /// <summary>
        /// Shows the list of parks supplied to this function in the results list with a Title
        /// </summary>
        /// <param name="Parks"></param>
        public void ShowParks(string Title, IEnumerable<TPWParkInfo> Parks, IEnumerable<TPWChatRoomInfo> OnlineRooms = default)
        {
            TitleBlock.Text = Title;
            var resultsPage = new ParksResultsPage();
            foreach(var park in Parks)
            {
                var parkControl = new ContentControl();
                UXResources.CreateParkControl(ref parkControl, park, OnlineRooms, false);
                resultsPage.ParksView.Children.Add(parkControl);
            }
            ParksControlContent.Navigate(resultsPage);
        }
    }
}
