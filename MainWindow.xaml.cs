using System;
using System.Windows;
using System.Runtime;
using System.Windows.Media;
using System.Windows.Threading; //DispatcherTimer   https://docs.microsoft.com/en-us/dotnet/api/system.windows.threading.dispatchertimer?view=net-5.0

/*  ===TODO===
 *  Export window - resolution
 *  right-click menus
 *  app ICON
 *  Either create new file or load another one
 *  add a MazeStyle class - that is saved/loaded indipendently from Maze class; RenderWall and Colors are in there -> save/load it to %appdata%
 *  add blank maze option
 *  Async save/loading/export
 *  progress bar is nonexistent as hell
 *  REMOVE ALL TESTING comments
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
        private DispatcherTimer _autoRenderTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            _controller = new Omnipresent();
            DataContext = _controller;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _controller.DPI = GetScaling();

            //preset these values for a possible MazeGeneration shortcut command
            _controller.CanvasSizeX = GetCanvasSizePixels().Item1;
            _controller.CanvasSizeY = GetCanvasSizePixels().Item2;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseApp(null, null);
        }

        #region AutoRe-Render

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_controller.MainMaze != null)
            {
                //triggers the timer
                if (_autoRenderTimer.IsEnabled == false)
                {
                    _autoRenderTimer.Tick += new EventHandler(autoRender);
                    _autoRenderTimer.Interval = new TimeSpan(0, 0, 1);
                    _autoRenderTimer.Start();
                }

                //restart the timer - window has just been resized
                if (_autoRenderTimer.IsEnabled == true)
                {
                    _autoRenderTimer.Stop();
                    _autoRenderTimer.Start();
                }
            }
            else
            {
                _controller.CanvasSizeX = GetCanvasSizePixels().Item1;
                _controller.CanvasSizeY = GetCanvasSizePixels().Item2;
            }
        }

        private void autoRender(object sender, EventArgs e)
        {
            _autoRenderTimer.Stop();
            _autoRenderTimer.Tick -= autoRender;
            _controller.Render(GetCanvasSizePixels());
        }

        #endregion AutoRe-Render

        #region Menu

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
            _controller.Export();
        }

        private void CloseApp(object sender, RoutedEventArgs e)
        {
            if (Utilities.isWorking) //do not exit the app when file is being saved/loaded
                MessageBox.Show("Cannot quit: File is being processed", "Unable to quit", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
            else { Application.Current.Shutdown(); }
        }

        #endregion Menu

        #region MazeFunctions

        private void MazeGeneration(object sender, RoutedEventArgs e)
        {
            _controller.MazeGeneration(GetCanvasSizePixels());
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            _controller.PathDijkstra();
        }

        private void SelectNode(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //https://stackoverflow.com/questions/741956/pan-zoom-image
            if (pictureBox.Source != null)
            {
                //TESTING
                //Console.WriteLine("SystemParameters.PrimaryScreenWidth:" + SystemParameters.PrimaryScreenWidth);
                //Console.WriteLine("SystemParameters.FullPrimaryScreenWidth:" + SystemParameters.FullPrimaryScreenWidth);
                //Console.WriteLine("SystemParameters.VirtualScreenWidth:" + SystemParameters.VirtualScreenWidth);
                //Console.WriteLine("SystemParameters.VirtualScreenLeft:" + SystemParameters.VirtualScreenLeft);
                //Console.WriteLine("e.GetPosition(this):" + e.GetPosition(this));

                Point pointPicture = e.GetPosition(pictureBox);
                Point monitorPoint = PointToScreen(e.GetPosition(this));
                //(real_Width / WPF_Width) -> image scaling
                double x = pictureBox.Source.Width / pictureBox.ActualWidth;
                double y = pictureBox.Source.Height / pictureBox.ActualHeight;

                Point transfrom = new Point(x, y);

                _controller.MazeNodeSelect(monitorPoint, pointPicture, transfrom);
            }
        }

        private void SettingOpen(object sender, RoutedEventArgs e)
        {
            _controller.SettingOpen();
        }

        #endregion MazeFunctions

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
                throw new ApplicationException("Display scaling is not square?! HOW!");

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