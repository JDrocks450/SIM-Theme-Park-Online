using GSQuazar.Prim;
using QuazarAPI.Messaging;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GSQuazar.Controls
{
    /// <summary>
    /// Interaction logic for StackableFlyoutWindowControl.xaml
    /// </summary>
    public partial class StackableFlyoutWindowControl : UserControl
    {
        private const double AnimTimespanSeconds = .5;

        /// <summary>
        /// The current stack of flyouts being displayed by this control.
        /// </summary>
        Stack<QuazarPage> flyouts = new Stack<QuazarPage>();
        Dictionary<QuazarPage, Frame> map = new Dictionary<QuazarPage, Frame>();
        bool dimmed = false;
        /// <summary>
        /// A map of all pages opened as Dialogs expecting responses
        /// </summary>
        Dictionary<QuazarPage, bool> dialogMap = new Dictionary<QuazarPage, bool>();

        internal void FlushAllWindows()
        {
            Dispatcher.Invoke(delegate
            {
                WindowsGrid.Children.Clear();
                Background = null;
            });
            List<QuazarPage> canRemoveList = new List<QuazarPage>();
            var threadSafeMap = dialogMap.Keys.ToList();
            foreach (var key in threadSafeMap)
            {
                var item = dialogMap[key];
                if (item)
                    dialogMap[key] = false;
                else canRemoveList.Add(key);
            }            
            foreach (var removeMe in canRemoveList)
                dialogMap.Remove(removeMe);
            canRemoveList.Clear();
            canRemoveList = null;
            dimmed = false;
            flyouts.Clear();
            map.Clear();
        }

        public StackableFlyoutWindowControl()
        {
            InitializeComponent();
        }

        public async Task<(bool? DialogResult, object Data)> ShowDialog(QuazarPage page)
        {
            Show(page);
            dialogMap.Add(page, true);
            try
            {
                while (dialogMap[page])
                    await Task.Delay(10);
            }
            catch { }
            return (page.DialogResult, page.Data);
        }

        /// <summary>
        /// Makes this page appear in the control and puts the active page to sleep.
        /// </summary>
        /// <param name="page"></param>
        public void Show(QuazarPage page)
        {
            void AddFlyout(Frame newWindow)
            {
                WindowsGrid.Children.Add(newWindow);
                map.Add(page, newWindow);
                flyouts.Push(page);
                Anim_FlyIn(newWindow);
                page.Focus();
            }
            if (!dimmed)
            {
                Anim_DimScreen();
                dimmed = true;
            }
            Frame newWindow = new Frame()
            {
                Content = page
            };
            page.OnClosed += Page_OnClosed;
            if (flyouts.Any())
            {
                var prev = map[flyouts.Peek()];
                var content = ((QuazarPage)prev.Content);
                if (content != null)
                    content.IsEnabled = false;
                Anim_FadeOut(prev);
            }
            AddFlyout(newWindow);
            DEBUGWINDOWS.Text = flyouts.Count.ToString();
        }

        private void Page_OnClosed(object sender, EventArgs e)
        {
            Close(sender as QuazarPage);
        }

        public void Close(QuazarPage page)
        {
            bool remaining = false;
            Anim_FlyOut(map[page]).ContinueWith(
                delegate
                {
                    if (dialogMap.TryGetValue(page, out _))
                        dialogMap[page] = false;
                    Dispatcher.Invoke(delegate { if (map.ContainsKey(page)) WindowsGrid.Children.Remove(map[page]); });
                    map.Remove(page);
                    //if (!flyouts.Any())
                      //  FlushAllWindows();
                });
            map.Remove(page);
            if (flyouts.Peek() == page)
            {
                flyouts.Pop();
                if (flyouts.Any() && map.ContainsKey(flyouts.Peek()))
                {
                    Anim_FadeIn(map[flyouts.Peek()]);
                    remaining = true;
                }
                else
                {
                    Anim_UndimScreen();
                    dimmed = false;
                }
            }
            DEBUGWINDOWS.Text = flyouts.Count.ToString();
        }

        private async Task Anim_FlyOut(UIElement target)
        {
            bool complete = false;
            double fromValue = ActualHeight * -(2.0 / 3);
            target.RenderTransform = new TranslateTransform();
            var anim = new DoubleAnimation(0, -fromValue, TimeSpan.FromSeconds(AnimTimespanSeconds));
            anim.AccelerationRatio = .1;
            anim.DecelerationRatio = .9;
            anim.Completed += delegate 
            {
                ((QuazarPage)(target as Frame).Content).Visibility = Visibility.Collapsed;
                complete = true; 
            };
            target.RenderTransform.BeginAnimation(TranslateTransform.YProperty, anim);
            anim.FillBehavior = FillBehavior.Stop;            
            while (!complete)
                await Task.Delay(100);
        }

        private void Anim_FlyIn(UIElement target)
        {
            double fromValue = ActualHeight * -(2.0 / 3);
            target.RenderTransform = new TranslateTransform();
            var anim = new DoubleAnimation(fromValue, 0, TimeSpan.FromSeconds(AnimTimespanSeconds));
            anim.AccelerationRatio = .1;
            anim.DecelerationRatio = .9;
            target.RenderTransform.BeginAnimation(TranslateTransform.YProperty, anim);                
        }

        private void Anim_FadeIn(UIElement target)
        {
            var page = ((QuazarPage)(target as Frame).Content);
            page.Visibility = Visibility.Visible;
            page.IsEnabled = true;
            var anim = new DoubleAnimation(0.0, 1.0, TimeSpan.FromSeconds(AnimTimespanSeconds));
            target.BeginAnimation(OpacityProperty, anim);
        }

        private async Task Anim_FadeOut(UIElement target)
        {
            bool complete = false;
            var anim = new DoubleAnimation(1.0, 0.0, TimeSpan.FromSeconds(AnimTimespanSeconds));
            anim.Completed += delegate { complete = true; };
            target.BeginAnimation(OpacityProperty, anim);            
            while (!complete)
                await Task.Delay(100);
        }

        private void Anim_UndimScreen()
        {
            var anim = new ColorAnimation((Color)ColorConverter.ConvertFromString("#00000000"),
                TimeSpan.FromSeconds(AnimTimespanSeconds));
            anim.FillBehavior = FillBehavior.Stop;
            anim.Completed += delegate 
            { 
                Background = null;
                FlushAllWindows();
            };
            Background.BeginAnimation(SolidColorBrush.ColorProperty, anim);            
        }

        private void Anim_DimScreen()
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00000000"));
            Background.BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation(Colors.Black * .5f, TimeSpan.FromSeconds(AnimTimespanSeconds)));
        }
    }
}
