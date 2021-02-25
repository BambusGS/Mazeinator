using System;
using System.Windows;
using System.Windows.Media;

/*  ===TODO===
 *  Make every PEN/drawing ifs into correct order
 *  add a MazeStyle class - that is saved/loaded indipendently from Maze class; RenderWall and Colors are in there -> save/load it to %appdata%
 *  Check if canvas size has changed and only then update
 *  add node edit option  
 *  add blank maze option
 *  Export window - resolution, (colors and fileformat)
 *  Either create new file or load another one
 *  Async save/loading/export
 *  progress bar is nonexistent as hell
 *  https://github.com/OneLoneCoder/videos/blob/master/OneLoneCoder_Mazes.cpp
 *  http://www.astrolog.org/labyrnth/algrithm.htm
 */

namespace Mazeinator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Omnipresent _controller = new Omnipresent();

        public MainWindow()
        {
            InitializeComponent();
            _controller = new Omnipresent();
            DataContext = _controller;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _controller.DPI = GetScaling();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //if(_controller.MainMaze != null)
            //    _controller.Render(GetCanvasSizePixels());

            //big button with [redraw]
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseApp(null, null);
        }

        private void NewMaze(object sender, RoutedEventArgs e)
        {
            _controller.NewMaze();
        }

        private void SaveMaze(object sender, RoutedEventArgs e)
        {
            _controller.SaveMaze();
        }

        private void LoadMaze(object sender, RoutedEventArgs e)
        {
            _controller.LoadMaze();
            _controller.Render(GetCanvasSizePixels());
        }

        private void ExportMaze(object sender, RoutedEventArgs e)
        {
            _controller.Export(GetCanvasSizePixels());
        }

        private void CloseApp(object sender, RoutedEventArgs e)
        {
            if (Utilities.isWorking) //do not exit the app when file is being saved/loaded
                MessageBox.Show("Cannot quit: File is being processed", "Unable to quit", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
            else { Application.Current.Shutdown(); }
        }

        private void MazeGeneration(object sender, RoutedEventArgs e)
        {
            // parse user specified maze parameters into intigers
            int.TryParse(WidthBox.Text, out int NodeCountX);
            int.TryParse(HeightBox.Text, out int NodeCountY);

            if (!(NodeCountX > 0 && NodeCountY > 0))
            {
                MessageBox.Show("Maze size error – must be a positive integer", "Maze size error", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                return;
            }

            _controller.MazeGeneration(NodeCountX, NodeCountY, GetCanvasSizePixels());
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_controller.MainMaze != null)
            {
                //_controller.MainMaze.GenerateMazeBlank();
                _controller.MainMaze.Dijkstra();
                _controller.RenderPath();
            }
        }

        private void SelectNode(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //https://stackoverflow.com/questions/741956/pan-zoom-image
            if (pictureBox.Source != null)
            {
                Point point = e.GetPosition(pictureBox);

                //Point * scaling * (real_Width / WPF_Width) -> x clicked coordinate in image
                double x = point.X * _controller.DPI * pictureBox.Source.Width / pictureBox.ActualWidth;
                double y = point.Y * _controller.DPI * pictureBox.Source.Height / pictureBox.ActualHeight;

                _controller.MazeNodeSelect(x, y, 0);
                //_controller.Render(GetCanvasSizePixels());
            }
        }

        private void SettingOpen(object sender, RoutedEventArgs e)
        {
            Style currentStyle = (Style)_controller.MazeStyle.Clone();

            StyleSettings settings = new StyleSettings(currentStyle, _controller.DPI);

            if (settings.ShowDialog() == true)
            {
                _controller.MazeStyle = settings.SettingsStyle;
                _controller.Status = "Setting applied";
            }
        }

        #region CustomFunctions

        /// <summary>
        /// Function that gets the current "WPF scaling"
        /// </summary>
        private double GetScaling()
        {
            //get the current "WPF DPI measure units"
            Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            double scaling = m.M11;

            //check if X and Y scaling are the same - if not (this should never happen) throw an error
            if (m.M11 / m.M22 != 1)
                throw new ApplicationException("Display scaling is not square?!");

            return scaling;
        }

        private Tuple<int, int> GetCanvasSizePixels()
        {
            //get canvas size & work out the rectangular cell size from the current window size
            int canvasSizeX = (int)Math.Round(MainCanvas.ActualWidth * _controller.DPI);
            int canvasSizeY = (int)Math.Round(MainCanvas.ActualHeight * _controller.DPI);
            return new Tuple<int, int>(canvasSizeX, canvasSizeY);
        }

        #endregion CustomFunctions
    }
}