using SimTheme_Park_Online.Data.Templating;
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

namespace TPWAPI.Frontend.Controls.UX
{
    /// <summary>
    /// Interaction logic for TemplaterControl.xaml
    /// </summary>
    public partial class TemplaterControl : UserControl
    {
        public TPWDataTemplate Template { get; private set; }
        public TPWTemplateDefinition EditingDef { get; private set; }
        public bool EditingMode => EditingDef != null;

        public TemplaterControl()
        {
            InitializeComponent();

            TypeSwitch.ItemsSource = Enum.GetNames(typeof(TPWSystemTypes));
        }

        public void Close()
        {
            EditingDef = null;
            //Visibility = Visibility.Collapsed;
            IsEnabled = false;
            DeleteButton.Visibility = Visibility.Hidden;
        }

        private void _open(TPWDataTemplate Templater)
        {
            Template = Templater;
            Visibility = Visibility.Visible;
            IsEnabled = true;
            TypeSwitch.ItemsSource = Enum.GetNames(typeof(TPWSystemTypes));
            TypeSwitch.SelectedIndex = 0;
        }

        public void Open(TPWDataTemplate Templater, TPWTemplateDefinition Editing)
        {
            _open(Templater);
            if (Editing != null)
                Setup(Editing);
        }

        public void Open(TPWDataTemplate Templater, uint Start, uint End)
        {
            if (End - Start <= 0) { Close(); return; }
            _open(Templater);
            if (Template.TryGetByOffsets(Start, End, out var t))
            {
                Setup(t);
                return;
            }
            DeleteButton.Visibility = Visibility.Hidden;
            NameBox.Text = "";
            DescBox.Text = "";
            ColorBox.Text = "";
            ReflectSelection(Start, End);
        }

        private void Setup(TPWTemplateDefinition Editing)
        {
            ReflectSelection(Editing.StartOffset, Editing.EndOffset);
            EditingDef = Editing;
            NameBox.Text = EditingDef.Name;
            DescBox.Text = EditingDef.Desc;
            ColorBox.Text = EditingDef.Color;
            TypeSwitch.SelectedItem = EditingDef.DataType;
            DeleteButton.Visibility = Visibility.Visible;
        }

        public void ReflectSelection(uint Start, uint End)
        {            
            Startindex.Text = Start.ToString();
            Endindex.Text = End.ToString();
            lengthBox.Text = (End - Start).ToString();

            TypeSwitch.ItemsSource = TPWTemplateDefinition.GetPossibleTypes(End - Start).Select(x => x.ToString());
            TypeSwitch.SelectedIndex = 0;            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private TPWSystemTypes ParseFromComboBox() => Enum.Parse<TPWSystemTypes>(TypeSwitch?.SelectedItem as string ?? "NONE");

        private void TemplateButton_Click(object sender, RoutedEventArgs e)
        {
            if (ParseFromComboBox() == TPWSystemTypes.NONE)
            {
                MessageBox.Show("Data Type is NONE, you have to select data that makes a complete Type from the dropdown menu.");
                return;
            }
            if (string.IsNullOrWhiteSpace(NameBox.Text) || string.IsNullOrWhiteSpace(DescBox.Text))
            {
                MessageBox.Show("Name or Description boxes are left empty. Please give these a descriptive value for other prying eyes... !_!");
                return;
            }
            if (string.IsNullOrWhiteSpace(ColorBox.Text))
            {
                MessageBox.Show("The color being empty is going to cause issues displaying text. Please select a color or type one in manually.");
                return;
            }
            //this function already checks for parity dummy dont need to double the amount of code here.
            Template.Add(new TPWTemplateDefinition()
            {
                Name = NameBox.Text,
                Desc = DescBox.Text,
                Color = ColorBox.Text,
                StartOffset = uint.Parse(Startindex.Text),
                EndOffset = uint.Parse(Endindex.Text),
                DataType = ParseFromComboBox()
            });
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Template.Remove(EditingDef);
        }

        private void ColorBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ColorBox == null) return; // InitializeComponent isn't finished yet!
            try
            {
                ColorBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ColorBox.Text));
            }
            catch
            { // very slow lmao should probably fix this

            }
        }

        private void TypeSwitch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = ParseFromComboBox();
            if (selected == TPWSystemTypes.NONE)
                return;
            var c = new ColorConverter();
            ColorBox.Text = c.ConvertToString(ApplicationResources.SystemSelectionBrushes[selected].Color);
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var c = new ColorConverter();
            ColorBox.Text = c.ConvertToString(((SolidColorBrush)(sender as Border).Background).Color);
        }
    }
}
