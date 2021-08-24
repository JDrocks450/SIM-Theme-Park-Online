using Microsoft.Win32;
using SimTheme_Park_Online;
using SimTheme_Park_Online.Data.Templating;
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
using System.Windows.Shapes;

namespace TPWAPI.Frontend.Pages
{
    /// <summary>
    /// Interaction logic for PacketProperties.xaml
    /// </summary>
    public partial class PacketProperties : Window
    {
        private TPWDataTemplate Templater = new TPWDataTemplate();
        public TPWPacket ViewingPacket { get; }

        public PacketProperties(SimTheme_Park_Online.TPWPacket ViewingPacket)
        {
            InitializeComponent();
            this.ViewingPacket = ViewingPacket;
            PopulateData();
            Templater.OnDefinitionsUpdated += Templater_OnDefinitionsUpdated;
            PopulateTemplateView();
        }

        private void PopulateData()
        {
            //Header
            HeaderPage.PopulateData(ViewingPacket);

            //Raw
            MemoryStream stream = new MemoryStream(ViewingPacket.Body);        
            HexEditorControl.Stream = stream;
        }

        public void SetTemplater(TPWDataTemplate Template)
        {
            Templater.OnDefinitionsUpdated -= Templater_OnDefinitionsUpdated;
            Templater = Template;
            PopulateData();
            PopulateTemplateView();
        }

        private void Templater_OnDefinitionsUpdated(object sender, EventArgs e) => PopulateTemplateView();

        private string ConvertToSelectedType(params byte[] Data)
        {
            if (ASCIIRadio.IsChecked ?? false)            
                return Encoding.ASCII.GetString(Data);            
            else if (UNICODERadio.IsChecked ?? false)            
                return Encoding.Unicode.GetString(Data);            
            else if (RawHexRadio.IsChecked ?? false)
                return string.Join(' ', Data.Select(x => x.ToString("X2")));
            else
                return string.Join(' ', Data);
        }

        private void ClearTemplateHexSelections() => HexEditorControl.ClearCustomBackgroundBlock();
        private void SelectTemplateInHexEditor(TPWTemplateDefinition Definition) => HexEditorControl.SelectionStart = Definition.StartOffset;
        private void SetTemplateBackground(SolidColorBrush Brush, int Start, int Length) => HexEditorControl.CustomBackgroundBlockItems.Add(new WpfHexaEditor.Core.CustomBackgroundBlock(Start, Length, Brush));

        private SolidColorBrush GetBrushFromString(string Color) => new SolidColorBrush((Color)ColorConverter.ConvertFromString(Color));

        private void PopulateTemplateView()
        {
            void Fill(byte[] buffer)
            {
                foreach (byte b in buffer)
                {
                    TemplateWrapView.Children.Add(
                        new TextBlock()
                        {
                            Text = ConvertToSelectedType(b),
                            VerticalAlignment = VerticalAlignment.Center
                        });
                }
            }
            if (TemplateWrapView == null) return;
            TemplateWrapView.Children.Clear();
            int lastDefIndex = 0;
            if (!Templater.Definitions.Any())
            {
                Fill(ViewingPacket.Body);
                return;
            }
            foreach (var def in Templater.Definitions)
            {
                int currentIndex = (int)def.StartOffset;
                byte[] buffer = ViewingPacket.Body.Skip(lastDefIndex).Take(currentIndex - lastDefIndex).ToArray();
                Fill(buffer);
                if (def.Color == "#FFFFFFFF")
                {
                    var c = new ColorConverter();
                    def.Color = c.ConvertToString(ApplicationResources.SystemSelectionBrushes[def.DataType].Color);
                }
                var item = new UserControl()
                {
                    Background = GetBrushFromString(def.Color),
                    Content =
                        new TextBlock()
                        {
                            Text = def.Represent(ViewingPacket.Body, RawHexRadio.IsChecked ?? false ? "HEX" : default),
                            Foreground = Brushes.White,                           
                        },
                    
                    ToolTip = new TextBlock()
                    {
                        Margin = new Thickness(5),
                        Foreground = Brushes.Gray,
                        Text = $"{def.Name}\n{def.Desc}\n[{def.DataType}]",
                    },
                    Cursor = Cursors.Hand,
                    Tag = def,
                    Margin = new Thickness(5)
                };
                item.MouseLeftButtonUp += delegate { SelectTemplateInHexEditor(item.Tag as TPWTemplateDefinition); };
                TemplateWrapView.Children.Add(item);
                var scb = GetBrushFromString(def.Color);
                SetTemplateBackground(scb, (int)def.StartOffset, (int)def.Length);
                lastDefIndex = (int)def.EndOffset;
            }
            Fill(ViewingPacket.Body.Skip(lastDefIndex).ToArray());
            PopulateLabels();
            Title = $"SIM Theme Park Packet Properties - Templated {Templater.Definitions.Count} Known Objects";
        }

        private void PopulateLabels()
        {
            LabelDisplay.Children.Clear();
            foreach (var label in Templater.Definitions)
            {
                var stack = new StackPanel();
                stack.Children.Add(new TextBlock()
                {
                    Text = label.Name,
                    TextWrapping = TextWrapping.Wrap
                });
                stack.Children.Add(new TextBlock()
                {
                    Text = label.Desc,
                    TextWrapping = TextWrapping.Wrap,
                    FontWeight = FontWeights.Regular
                });
                stack.Children.Add(new TextBlock()
                {
                    Text = $"[{label.DataType}] 0x{label.StartOffset.ToString("X")} -> 0x{label.EndOffset.ToString("X")}",
                    TextWrapping = TextWrapping.Wrap,
                    FontWeight = FontWeights.Regular
                });
                var usr = new UserControl()
                {
                    Style = (Style)FindResource("TPWUX_TemplateObjectBorderStyle"),
                    Background = GetBrushFromString(label.Color),
                    Content = stack,
                    Tag = label,
                    Cursor = Cursors.Hand,
                };
                usr.MouseLeftButtonUp += delegate { SelectTemplateInHexEditor(usr.Tag as TPWTemplateDefinition); };
                LabelDisplay.Children.Add(usr);
            }
        }        

        private void ResyncTemplater()
        {
            if (HexEditorControl.SelectionLength == 0)
            {
                TemplaterControl.Close();
                return;
            }
            else TemplaterControl.IsEnabled = true;
            TemplaterControl.Open(Templater,
                (uint)HexEditorControl.SelectionStart, 
                (uint)(HexEditorControl.SelectionStart +
                HexEditorControl.SelectionLength));
        }

        private void LoadTemplate()
        {
            try
            {
                OpenFileDialog diag = new OpenFileDialog()
                {
                    Title = "Load *.tpwtemplate From Where Exactly?",
                    AddExtension = true,
                    Filter = "TPWAPI Packet Template|*.tpwtemplate",
                    Multiselect = false,
                    CheckFileExists = true,
                    CheckPathExists = true
                };
                if (!(diag.ShowDialog() ?? false))
                    return;
                Templater = TPWDataTemplate.Load(diag.FileName);
                TOpenBlock.Text = System.IO.Path.GetFileName(diag.FileName);
                PopulateTemplateView();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "Load Exception");
            }
        }

        private void SaveTemplate()
        {
            try
            {
                string FileName = Templater.FileName;
                if (string.IsNullOrWhiteSpace(FileName))
                {
                    SaveFileDialog diag = new SaveFileDialog()
                    {
                        Title = "Save *.tpwtemplate Where Exactly?",
                        AddExtension = true,
                        Filter = "TPWAPI Packet Template|*.tpwtemplate"
                    };
                    if (!(diag.ShowDialog() ?? false))
                        return;
                    FileName = diag.FileName;
                }
                Templater.Save(FileName);
                TOpenBlock.Text = System.IO.Path.GetFileName(FileName);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "Save Exception");
            }
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

        private void HexEditor_SelectionLengthChanged(object sender, EventArgs e)
        {
            ResyncTemplater();
        }

        private void HexEditor_SelectionStartChanged(object sender, EventArgs e)
        {
            ResyncTemplater();
        }

        private void HexEditor_SelectionStopChanged(object sender, EventArgs e)
        {
            ResyncTemplater();
        }

        private void ASCIIRadio_Checked(object sender, RoutedEventArgs e)
        {
            PopulateTemplateView();
        }

        private void TLoad_Click(object sender, RoutedEventArgs e)
        {
            LoadTemplate();
        }        

        private void TemplateSave_Click(object sender, RoutedEventArgs e)
        {
            SaveTemplate();
        }

        private void TSaveAs_Click(object sender, RoutedEventArgs e)
        {
            Templater.FileName = null; // ah a little trick for ya
            SaveTemplate();
        }

        private void TUnload_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Unloading the template reloads this dialog. Are you sure there's" +
                " no changes you're going to lose doing this?", "Losing Everything",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                ApplicationResources.ShowPropertiesWindow(ViewingPacket);
                Close();
                //what an efficient workaround!!
            }
        }

        private void PacketExportDAT_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog diag = new SaveFileDialog()
            {
                Title = "Save *.dat to the Disk For Later Viewing and Or Studing or Perhaps...",
                AddExtension = true,
                Filter = "Raw Packet Data|*.dat"
            };
            if (!(diag.ShowDialog() ?? false))
                return;
            try
            {

                SimTheme_Park_Online.Factory.TPWPacketFactory.ExportToDisk(diag.FileName, ViewingPacket);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "An Error? How?!");
            }
        }
    }
}
