using System;
using System.Windows;
using System.Windows.Media;

namespace Mazeinator
{
    //https://www.wpf-tutorial.com/dialogs/creating-a-custom-input-dialog/
    //https://www.hanselman.com/blog/learning-wpf-with-babysmash-configuration-with-databinding

    /// <summary>
    /// Interaction logic for StyleSettings.xaml
    /// </summary>
    public partial class StyleSettings : Window
    {
        public Style SettingsStyle = null;
        private Maze _maze = new Maze(3, 3);
        private double DPI;

        public StyleSettings(Style style, double DPI)
        {
            InitializeComponent();

            SettingsStyle = style;
            this.DataContext = SettingsStyle;
            this.DPI = DPI;

            //preview maze setup
            _maze.GenerateMazeBlank();

            _maze.startNode = _maze.nodes[0, 0];
            _maze.endNode = _maze.nodes[2, 2];

            _maze.ToggleNeighbour(_maze.nodes[1, 1], Node.North);
            _maze.ToggleNeighbour(_maze.nodes[1, 1], Node.West);
            _maze.ToggleNeighbour(_maze.nodes[1, 2], Node.West);
            _maze.ToggleNeighbour(_maze.nodes[2, 2], Node.North);

            _maze.Dijkstra();

            //combobox line end cap setup
            cmbLineCap.ItemsSource = SettingsStyle.LineCapOptions;
            cmbLineCap.SelectedIndex = Array.IndexOf(SettingsStyle.LineCapOptions, SettingsStyle.WallEndCap);
            cmbPathCap.ItemsSource = SettingsStyle.LineCapOptions;
            cmbPathCap.SelectedIndex = Array.IndexOf(SettingsStyle.LineCapOptions, SettingsStyle.PathEndCap);

            RedrawPreview();
        }

        private void btnDialogOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void PlainStyle(object sender, RoutedEventArgs e)
        {
            SettingsStyle = new Style()
            {
                BackgroundColor = Colors.White,
                RootColorBegin = Colors.Yellow,
                RootColorEnd = Colors.Orange,

                RenderNode = false,
                RenderPoint = false,
            };
            this.DataContext = SettingsStyle;

            cmbLineCap.SelectedIndex = Array.IndexOf(SettingsStyle.LineCapOptions, SettingsStyle.WallEndCap);
            cmbPathCap.SelectedIndex = Array.IndexOf(SettingsStyle.LineCapOptions, SettingsStyle.PathEndCap);

            RedrawPreview();
        }

        private void DefaultValues(object sender, RoutedEventArgs e)
        {
            SettingsStyle = new Style();
            this.DataContext = SettingsStyle;

            cmbLineCap.SelectedIndex = Array.IndexOf(SettingsStyle.LineCapOptions, SettingsStyle.WallEndCap);
            cmbPathCap.SelectedIndex = Array.IndexOf(SettingsStyle.LineCapOptions, SettingsStyle.PathEndCap);

            RedrawPreview();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            RedrawPreview();
        }

        private void SelectedColorChangedEvent(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            RedrawPreview();
        }

        private void IntegerUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            RedrawPreview();
        }

        private void cmbLineCap_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SettingsStyle.WallEndCap = SettingsStyle.LineCapOptions[cmbLineCap.SelectedIndex];
            RedrawPreview();
        }

        private void cmbPathCap_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SettingsStyle.PathEndCap = SettingsStyle.LineCapOptions[cmbPathCap.SelectedIndex];
            RedrawPreview();
        }

        private void RedrawPreview()
        {
            //get canvas size & work out the rectangular cell size from the current window size
            int canvasSizeX = (int)Math.Round((MainCanvas.ActualWidth * DPI));
            int canvasSizeY = (int)Math.Round((MainCanvas.ActualHeight * DPI));

            MazePreview.Source = Utilities.BitmapToImageSource(_maze.RenderPath(_maze.RenderMaze(canvasSizeX, canvasSizeY, SettingsStyle), SettingsStyle));
            GC.Collect();
        }
    }
}