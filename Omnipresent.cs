using Microsoft.Win32;  //FileDialogs
using System;
using System.ComponentModel;    //INotifyPropertyChanged
using System.Diagnostics;   //Stopwatch
using System.Threading.Tasks;   //Tasks so as not to freeze the UI  //https://stackoverflow.com/questions/27089263/how-to-run-and-interact-with-an-async-task-from-a-wpf-gui
using System.Windows;   //MessageBox
using System.Windows.Input; //ICommands
using System.Windows.Media.Imaging; //BitmapImage

namespace Mazeinator
{
    /// <summary>
    /// Class that implements global shortcut binding and subsequent commands (Ctrl+S -> save file)
    /// </summary>
    public class ActionCommand : ICommand
    {
        private readonly Action _action;

        public ActionCommand(Action action)
        {
            _action = action;
        }

        public void Execute(object parameter)
        {
            _action();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }

    /// <summary>
    /// Controller for the main window's action; holds objects and performs maze manipulation
    /// </summary>
    internal class Omnipresent : INotifyPropertyChanged
    {
        #region Variables

        //define the powerful Maze class
        public Maze MainMaze = null;

        //defines the internal private variable; AND their "public variable wrapper" for WPF binding
        private bool _isSquare = true; public bool IsSquare { get => _isSquare; set { _isSquare = value; OnPropertyChanged(nameof(IsSquare)); } }

        private int _nodeCount = 0; public int NodeCount { get => _nodeCount; set { _nodeCount = value; OnPropertyChanged(nameof(NodeCount)); } }
        private long _lastGenTime = 0; public long LastGenTime { get => _lastGenTime; set { _lastGenTime = value; OnPropertyChanged(nameof(LastGenTime)); } }
        private long _lastRenderTime = 0; public long LastRenderTime { get => _lastRenderTime; set { _lastRenderTime = value; OnPropertyChanged(nameof(LastRenderTime)); } }
        private long _renderSizeX = 0; public long RenderSizeX { get => _renderSizeX; set { _renderSizeX = value; OnPropertyChanged(nameof(RenderSizeX)); } }
        private long _renderSizeY = 0; public long RenderSizeY { get => _renderSizeY; set { _renderSizeY = value; OnPropertyChanged(nameof(RenderSizeY)); } }
        private long _canvasSizeX = 0; public long CanvasSizeX { get => _canvasSizeX; set { _canvasSizeX = value; OnPropertyChanged(nameof(CanvasSizeX)); } }
        private long _canvasSizeY = 0; public long CanvasSizeY { get => _canvasSizeY; set { _canvasSizeY = value; OnPropertyChanged(nameof(CanvasSizeY)); } }
        private int _percent = 20; public int Percent { get => _percent; set { _percent = value; OnPropertyChanged(nameof(Percent)); } }

        private string _status = "Ready"; public string Status { get => _status; set { _status = value; OnPropertyChanged(nameof(Status)); } }
        private string _currentFilePath = null;
        private BitmapImage _maze = null; public BitmapImage Maze { get => _maze; set { _maze = value; OnPropertyChanged(nameof(Maze)); } }

        public long AvgGenTime = 0, AvgRenderTime = 0;

        #endregion Variables

        #region INotify_Binding

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));  //= if(PropertyChanged != null) {}
        }

        #endregion INotify_Binding

        #region Global_shortcuts

        private ICommand _newFileCMD; public ICommand NewFileCMD { get { return _newFileCMD ?? (_newFileCMD = new ActionCommand(() => { NewMaze(); })); } }
        private ICommand _saveFileCMD; public ICommand SaveFileCMD { get { return _saveFileCMD ?? (_saveFileCMD = new ActionCommand(() => { SaveMaze(); })); } }

        #endregion Global_shortcuts

        #region Maze_generation

        public void MazeGeneration(int NodeCountX, int NodeCountY, Tuple<int, int> CanvasSize)
        {
            //stopwatch to measure the generation time and make user predictions
            Stopwatch GenTime = new Stopwatch();
            GenTime.Start();

            MainMaze = new Maze(NodeCountX, NodeCountY);
            MainMaze.GenerateMaze();

            GenTime.Stop();
            LastGenTime = GenTime.ElapsedMilliseconds;

            Status = "Generating done";
            NodeCount = MainMaze.nodes.Length;

            Render(CanvasSize);

            //new Task(() => { MainMaze.DisplayMaze(CanvasSize.Item1, CanvasSize.Item2, _isSquare, false); }).Start();
            //Bitmap Maze = MainMaze.DisplayMaze(CanvasSize.Item1, CanvasSize.Item2, _isSquare, false);
            //pictureBox.Source = Utilities.BitmapToImageSource(Maze);
        }

        public void Render(Tuple<int, int> CanvasSize)
        {
            Stopwatch RenderTime = new Stopwatch();
            RenderTime.Start();

            Maze = Utilities.BitmapToImageSource(MainMaze.RenderMaze(CanvasSize.Item1, CanvasSize.Item2, IsSquare));
            //new Task(() => { test = MainMaze.DisplayMaze(CanvasSize.Item1, CanvasSize.Item2, IsSquare); }).Start();

            RenderTime.Stop();
            LastRenderTime = RenderTime.ElapsedMilliseconds;

            Status = "Rendering done";
            RenderSizeX = MainMaze.renderSizeX;
            RenderSizeY = MainMaze.renderSizeY;
            CanvasSizeX = CanvasSize.Item1;
            CanvasSizeY = CanvasSize.Item2;
        }

        #endregion Maze_generation

        #region Data_manipulation

        public void NewMaze()
        {
            MainMaze = null;
            Maze = null;
        }

        public void SaveMaze()
        {
            if (MainMaze == null)   //only save maze if there is one
            {
                MessageBox.Show("Cannot save an empty maze", "Unable to save", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Save Maze As",
                Filter = "Maze files (*.maze)|*.maze|All files (*.*)|*.*",
                FilterIndex = 1,
                //dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                RestoreDirectory = true
            };

            try
            {
                if (_currentFilePath == null)
                {
                    if (dialog.ShowDialog() == true)
                    {
                        _currentFilePath = dialog.FileName;
                    }
                }

                if (_currentFilePath != null)
                    new Task(() => { Utilities.SaveBySerializing<Maze>(MainMaze, _currentFilePath); }).Start();

                Status = "Saving done";
            }
            catch (Exception exc)
            {
                MessageBox.Show("An unhandled exception just occured: " + exc.Message, "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadMaze()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Load Maze",
                Filter = "Maze files (*.maze)|*.maze|All files (*.*)|*.*",
                FilterIndex = 1,
                //dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                RestoreDirectory = true
            };
            try
            {
                if (dialog.ShowDialog() == true)
                {
                    MainMaze = Utilities.LoadFromTheDead<Maze>(dialog.FileName);
                    Status = "Loading done";
                    NodeCount = MainMaze.nodes.Length;
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("An unhandled exception just occured: " + exc.Message, "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Export(Tuple<int, int> CanvasSize)
        {
            if (MainMaze == null)   //only save maze if there is one
            {
                MessageBox.Show("Cannot export an empty maze", "Export failed", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Export file",
                Filter = "BMP files (*.bmp)|*.bmp|All files (*.*)|*.*",
                FilterIndex = 1,
                //dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                RestoreDirectory = true
            };

            try
            {
                if (dialog.ShowDialog() == true)
                {
                    //generate bitmap and save it to file
                    MainMaze.RenderMaze(CanvasSize.Item1 * 2, CanvasSize.Item2 * 2, false).Save(dialog.FileName);
                    Status = "Export done";
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("An unhandled exception just occured: " + exc.Message, "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion Data_manipulation
    }
}