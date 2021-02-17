using System;
using System.Windows;
using System.Windows.Media;

/*  ===TODO===
 *  Make every PEN/drawing ifs into correct order
 *  Export window
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
            DataContext = _controller;
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseApp(null, null);
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
                MessageBox.Show("Maze size error – must be an integer", "Maze size error", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                return;
            }

            _controller.MazeGeneration(NodeCountX, NodeCountY, GetCanvasSizePixels());
        }

        private Tuple<int, int> GetCanvasSizePixels()
        {
            Matrix m = PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformToDevice;
            double dx = m.M11, dy = m.M22;    //get the current "WPF DPI units"

            //get canvas size & work out the rectangular cell size from the current window size
            int canvasSizeX = (int)Math.Round((MainCanvas.ActualWidth * dx));
            int canvasSizeY = (int)Math.Round((MainCanvas.ActualHeight * dy));
            return new Tuple<int, int>(canvasSizeX, canvasSizeY);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //if(_controller.MainMaze != null)
            //    _controller.Render(GetCanvasSizePixels());
            
            //big button with [redraw]
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            _controller.MainMaze.Dijkstra();
        }

        private void pictureBox_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string text;
            //https://stackoverflow.com/questions/741956/pan-zoom-image
            text = (e.GetPosition(pictureBox).X).ToString() + " | " + e.GetPosition(pictureBox).Y.ToString();

            Console.WriteLine(text);
        }
    }
}