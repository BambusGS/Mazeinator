using Microsoft.Win32;  //FileDialogs
using System;
using System.ComponentModel;    //INotifyPropertyChanged
using System.Diagnostics;   //Stopwatch
using System.Drawing.Imaging; //ImageFormat (export operation)
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
        #region INotify_Binding

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));  //= if(PropertyChanged != null) {}
        }

        #endregion INotify_Binding

        #region Variables

        //define the powerful Maze class
        public Maze MainMaze = null;

        //define the maze style class and the renderer class
        public Style MazeStyle = new Style();

        public Renderer Renderer = new Renderer();

        //holds the current window scaling factor
        public double DPI = 1;

        //defines the internal private variable; AND their "public variable wrapper" for WPF binding
        private int _nodeCount = 0; public int NodeCount { get => _nodeCount; set { _nodeCount = value; OnPropertyChanged(nameof(NodeCount)); } }

        private int _nodeCountX = 32; public int NodeCountX { get => _nodeCountX; set { _nodeCountX = value; OnPropertyChanged(nameof(NodeCountX)); } }
        private int _nodeCountY = 18; public int NodeCountY { get => _nodeCountY; set { _nodeCountY = value; OnPropertyChanged(nameof(NodeCountY)); } }
        private long _lastGenTime = 0; public long LastGenTime { get => _lastGenTime; set { _lastGenTime = value; OnPropertyChanged(nameof(LastGenTime)); } }
        private long _lastRenderTime = 0; public long LastRenderTime { get => _lastRenderTime; set { _lastRenderTime = value; OnPropertyChanged(nameof(LastRenderTime)); } }
        private int _pathLength = 0; public int PathLength { get => _pathLength; set { _pathLength = value; OnPropertyChanged(nameof(PathLength)); } }
        private int _exploredNodes = 0; public int ExploredNodes { get => _exploredNodes; set { _exploredNodes = value; OnPropertyChanged(nameof(ExploredNodes)); } }
        private int _renderSizeX = 0; public int RenderSizeX { get => _renderSizeX; set { _renderSizeX = value; OnPropertyChanged(nameof(RenderSizeX)); } }
        private int _renderSizeY = 0; public int RenderSizeY { get => _renderSizeY; set { _renderSizeY = value; OnPropertyChanged(nameof(RenderSizeY)); } }
        private int _canvasSizeX = 0; public int CanvasSizeX { get => _canvasSizeX; set { _canvasSizeX = value; OnPropertyChanged(nameof(CanvasSizeX)); } }
        private int _canvasSizeY = 0; public int CanvasSizeY { get => _canvasSizeY; set { _canvasSizeY = value; OnPropertyChanged(nameof(CanvasSizeY)); } }

        //status bar program status string
        private string _status = "Ready"; public string Status { get => _status; set { _status = value; OnPropertyChanged(nameof(Status)); } }

        private string _currentFilePath = null;

        //one task object, so as one async task is run at a time
        private Task _oneToRunThemAll;

        private BitmapImage _maze = null; public BitmapImage Maze { get => _maze; set { _maze = value; OnPropertyChanged(nameof(Maze)); } }
        private System.Drawing.Bitmap _mazeBMP = null;      //stores raw rendered maze; used as a base image for further drawing of start/end nodes and path

        #endregion Variables

        #region Global_shortcuts

        private ICommand _newFileCMD; public ICommand NewFileCMD { get { return _newFileCMD ?? (_newFileCMD = new ActionCommand(() => { NewMaze(); })); } }
        private ICommand _saveFileCMD; public ICommand SaveFileCMD { get { return _saveFileCMD ?? (_saveFileCMD = new ActionCommand(() => { SaveMaze(); })); } }
        private ICommand _loadFileCMD; public ICommand LoadFileCMD { get { return _loadFileCMD ?? (_loadFileCMD = new ActionCommand(() => { LoadMaze(); })); } }
        private ICommand _generateCMD; public ICommand GenerateCMD { get { return _generateCMD ?? (_generateCMD = new ActionCommand(() => { MazeGeneration(new Tuple<int, int>(CanvasSizeX, CanvasSizeY)); ; })); } }
        private ICommand _generateBlankCMD; public ICommand GenerateBlankCMD { get { return _generateBlankCMD ?? (_generateBlankCMD = new ActionCommand(() => { MazeGenBlank(new Tuple<int, int>(CanvasSizeX, CanvasSizeY)); ; })); } }
        private ICommand _exportCMD; public ICommand ExportCMD { get { return _exportCMD ?? (_exportCMD = new ActionCommand(() => { Export(); })); } }
        private ICommand _quitCMD; public ICommand QuitCMD { get { return _quitCMD ?? (_quitCMD = new ActionCommand(() => { Application.Current.MainWindow.Close(); })); } }
        private ICommand _aboutCMD; public ICommand AboutCMD { get { return _aboutCMD ?? (_aboutCMD = new ActionCommand(() => { About(); })); } }

        #endregion Global_shortcuts

        #region Maze_functions

        public void MazeGeneration(Tuple<int, int> CanvasSize)
        {
            _currentFilePath = null; //reset the "save without asking" path

            //stopwatch to measure the generation time and make user predictions
            Stopwatch GenTime = new Stopwatch();
            GenTime.Start();

            MainMaze = new Maze(NodeCountX, NodeCountY);
            MainMaze.GenerateMaze();

            GenTime.Stop();
            LastGenTime = GenTime.ElapsedMilliseconds;

            Status = "Generating done";
            NodeCount = MainMaze.nodes.Length;

            RenderAsync(CanvasSize);
        }

        public void MazeGenBlank(Tuple<int, int> CanvasSize)
        {
            _currentFilePath = null; //reset the "save without asking" path

            //stopwatch to measure the generation time and make user predictions
            Stopwatch GenTime = new Stopwatch();
            GenTime.Start();

            MainMaze = new Maze(NodeCountX, NodeCountY);
            MainMaze.GenerateMazeBlank();

            GenTime.Stop();
            LastGenTime = GenTime.ElapsedMilliseconds;

            Status = "Generating done";
            NodeCount = MainMaze.nodes.Length;
            MazeStyle.RenderPoint = true;   //so the user can see the individual nodes

            RenderAsync(CanvasSize);
        }

        public void PathGreedy()
        {
            if (MainMaze != null)
            {
                //if it does not exist -> run; (check if the task exists) ONLY THEN check if there is not any other running task and run
                //[use of operator evaluation] for thread safety
                if (_oneToRunThemAll == null || _oneToRunThemAll.Status != TaskStatus.Running)
                    _oneToRunThemAll = Task.Run(() =>
                        {
                            Stopwatch ProcessTime = new Stopwatch();
                            ProcessTime.Start();
                            Status = "Started searching...";

                            if (!MainMaze.GreedyBFS())
                                Status = "Pathfinding failed";
                            else
                            {
                                Status = "Greedy best-first search done";
                                PathLength = MainMaze.GreedyPath.PathLength;
                                ExploredNodes = MainMaze.GreedyPath.ExploredCount;
                            }

                            LastGenTime = ProcessTime.ElapsedMilliseconds;
                            ProcessTime.Restart();

                            RenderPath(MainMaze.GreedyPath);

                            ProcessTime.Stop();
                            LastRenderTime = ProcessTime.ElapsedMilliseconds;
                        });
                else
                    MessageBox.Show("Other calculations are still in progress\nCould not perform the action", "Action in Progress", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
            }
        }

        public void PathDijkstra()
        {
            if (MainMaze != null)
            {
                //if it does not exist -> run; (check if the task exists) ONLY THEN check if there is not any other running task and run
                //[use of operator evaluation] for thread safety
                if (_oneToRunThemAll == null || _oneToRunThemAll.Status != TaskStatus.Running)
                    _oneToRunThemAll = Task.Run(() =>
                    {
                        Stopwatch ProcessTime = new Stopwatch();
                        ProcessTime.Start();
                        Status = "Started searching...";

                        if (!MainMaze.Dijkstra())
                            Status = "Pathfinding failed";
                        else
                        {
                            Status = "Dijkstra done";
                            PathLength = MainMaze.DijkstraPath.PathLength;
                            ExploredNodes = MainMaze.DijkstraPath.ExploredCount;
                        }
                        LastGenTime = ProcessTime.ElapsedMilliseconds;
                        ProcessTime.Restart();

                        RenderPath(MainMaze.DijkstraPath);

                        ProcessTime.Stop();
                        LastRenderTime = ProcessTime.ElapsedMilliseconds;
                    });
                else
                    MessageBox.Show("Other calculations are still in progress\nCould not perform the action", "Action in Progress", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
            }
        }

        public void PathAStar()
        {
            if (MainMaze != null)
            {
                //if it does not exist -> run; (check if the task exists) ONLY THEN check if there is not any other running task and run
                //[use of operator evaluation] for thread safety
                if (_oneToRunThemAll == null || _oneToRunThemAll.Status != TaskStatus.Running)
                    _oneToRunThemAll = Task.Run(() =>
                    {
                        Stopwatch ProcessTime = new Stopwatch();
                        ProcessTime.Start();
                        Status = "Started searching...";

                        if (!MainMaze.AStar())
                            Status = "Pathfinding failed";
                        else
                        {
                            Status = "A* done";
                            PathLength = MainMaze.AStarPath.PathLength;
                            ExploredNodes = MainMaze.AStarPath.ExploredCount;
                        }

                        LastGenTime = ProcessTime.ElapsedMilliseconds;
                        ProcessTime.Restart();

                        RenderPath(MainMaze.AStarPath);

                        ProcessTime.Stop();
                        LastRenderTime = ProcessTime.ElapsedMilliseconds;
                    });
                else
                    MessageBox.Show("Other calculations are still in progress\nCould not perform the action", "Action in Progress", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
            }
        }

        /// <summary>
        /// Function that selects the start and end Node in the Maze class
        /// </summary>
        /// <param name="monitorClick">RAW WPF units where I clicked in the image relative to the active monitor</param>
        /// <param name="imageClick">Image clicked pixels</param>
        /// <param name="transfrom">Current image scaling</param>
        public void MazeNodeSelect(Point monitorClick, Point imageClick, Point transfrom)
        {
            if (MainMaze != null) //means that the node[0,0] must exist
            {
                //Point * display_scaling * (real_Width / WPF_Width) -> x clicked coordinate in image
                double imageXinPX = imageClick.X * DPI * transfrom.X;
                double imageYinPX = imageClick.Y * DPI * transfrom.Y;

                //(x coordinates - wallThickness) / cell.width -> floor & clamp (0 and nodes length) -> maze node selector
                int cellWidth = MainMaze.nodes[0, 0].Bounds.Width;
                int cellHeight = MainMaze.nodes[0, 0].Bounds.Height;

                int cellXStart = MainMaze.nodes[0, 0].Bounds.X;
                int cellYStart = MainMaze.nodes[0, 0].Bounds.Y;

                int selectX = Utilities.Clamp((int)((imageXinPX - cellXStart) / cellWidth), 0, MainMaze.nodes.GetLength(0) - 1);
                int selectY = Utilities.Clamp((int)((imageYinPX - cellYStart) / cellHeight), 0, MainMaze.nodes.GetLength(1) - 1);

                //node snap_to_grid functionality - calculates offset (where_is_the_center - where_user_clicked)
                double gridSnapX = (selectX * cellWidth + cellXStart + (double)cellWidth / 2) - (imageXinPX) + 0.5;
                double gridSnapY = (selectY * cellHeight + cellYStart + (double)cellHeight / 2) - (imageYinPX) + 0.5;

                int cellSize = (cellWidth < cellHeight) ? cellWidth : cellHeight;
                NodeSettings NodeSelector = new NodeSettings((int)(cellSize / DPI / 5));
                //centers the window on the current cell - where user clicked + grid_snap_offset + half_the_window_width
                //divided by the current monitor_scaling_DPI -> get back to WPF units
                NodeSelector.Left = ((monitorClick.X + gridSnapX / transfrom.X) / DPI - NodeSelector.Width / 2);
                NodeSelector.Top = ((monitorClick.Y + gridSnapY / transfrom.Y) / DPI - NodeSelector.Height / 2);

                //window rendering near the edges screen fix
                //gets the active screen's X, Y, Width, Height properties in pixels
                System.Windows.Interop.WindowInteropHelper windowInteropHelper = new System.Windows.Interop.WindowInteropHelper(Application.Current.MainWindow);
                System.Windows.Forms.Screen Screen = System.Windows.Forms.Screen.FromHandle(windowInteropHelper.Handle);
                //Console.WriteLine(Screen.Bounds);

                //a little conversion from (pixels)Screen to (WPF unites)NodeSelector by dividing pixels/DPI
                if (NodeSelector.Left < Screen.Bounds.Left / DPI)
                    NodeSelector.TargetSwap(Node.West);
                else if (NodeSelector.Left + NodeSelector.Width > Screen.Bounds.Right / DPI)
                    NodeSelector.TargetSwap(Node.East);
                else if (NodeSelector.Top + NodeSelector.Height > Screen.Bounds.Bottom / DPI)
                    NodeSelector.TargetSwap(Node.South);
                else if (NodeSelector.Top < Screen.Bounds.Top / DPI)
                    NodeSelector.TargetSwap(Node.North);

                if (MainMaze.pathToRender != null && MainMaze.pathToRender.Algorithm != AlgoType.Other)
                {
                    NodeSelector.lastAlgorithm = MainMaze.pathToRender.Algorithm;   //select the last used algorithm
                }
                else
                    NodeSelector.lastAlgorithm = AlgoType.Astar;

                if (NodeSelector.ShowDialog() == true)
                {
                    MainMaze.pathToRender.Clear();

                    Node targetNode = MainMaze.nodes[selectX, selectY];

                    switch (NodeSelector.nodeAction)
                    {
                        case NodeAction.NorthNodeSelect:
                            MainMaze.ToggleNeighbour(targetNode, Node.North);
                            Renderer.RenderNode(_mazeBMP, MainMaze, targetNode, MazeStyle);
                            RenderPath();
                            break;

                        case NodeAction.EastNodeSelect:
                            MainMaze.ToggleNeighbour(targetNode, Node.East);
                            Renderer.RenderNode(_mazeBMP, MainMaze, targetNode, MazeStyle);
                            RenderPath();
                            break;

                        case NodeAction.SouthNodeSelect:
                            MainMaze.ToggleNeighbour(targetNode, Node.South);
                            Renderer.RenderNode(_mazeBMP, MainMaze, targetNode, MazeStyle);
                            RenderPath();
                            break;

                        case NodeAction.WestNodeSelect:
                            MainMaze.ToggleNeighbour(targetNode, Node.West);
                            Renderer.RenderNode(_mazeBMP, MainMaze, targetNode, MazeStyle);
                            RenderPath();
                            break;

                        case NodeAction.StartNodeSelect:
                            if (targetNode == MainMaze.endNode)
                                MainMaze.endNode = null;

                            if (targetNode == MainMaze.startNode)
                                MainMaze.startNode = null;
                            else
                                MainMaze.startNode = targetNode;
                            RenderPath();
                            break;

                        case NodeAction.EndNodeSelect:
                            if (targetNode == MainMaze.startNode)
                                MainMaze.startNode = null;

                            if (targetNode == MainMaze.endNode)
                                MainMaze.endNode = null;
                            else
                                MainMaze.endNode = targetNode;
                            RenderPath();
                            break;

                        case NodeAction.AUX:
                            switch (NodeSelector.lastAlgorithm)
                            {
                                case AlgoType.Greedy:
                                    PathGreedy();
                                    break;

                                case AlgoType.Dijkstra:
                                    PathDijkstra();
                                    break;

                                case AlgoType.Astar:
                                    PathAStar();
                                    break;

                                case AlgoType.Other:
                                    break;
                            }
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        public void SettingOpen()
        {
            Style currentStyle = (Style)MazeStyle.Clone();

            StyleSettings settings = new StyleSettings(currentStyle, DPI);
            if (settings.ShowDialog() == true)
            {
                MazeStyle = settings.SettingsStyle;
                RenderAsync();
                Status = "Setting applied";
            }
        }

        public void About()
        {
            About about = new About();
            about.Show();
        }

        #endregion Maze_functions

        #region Maze_render

        public void RenderAsync(bool nonAsync = false)
        {
            RenderAsync(new Tuple<int, int>(CanvasSizeX, CanvasSizeY), nonAsync);
        }

        public void RenderAsync(Tuple<int, int> CanvasSize, bool nonAsync = false)
        {
            if (nonAsync == true)
                Render(CanvasSize);
            else
            {
                //if it does not exist -> run; (check if the task exists) ONLY THEN check if there is not any other running task and run
                //[use of operator evaluation] for thread safety
                if (_oneToRunThemAll == null || _oneToRunThemAll.Status != TaskStatus.Running)
                    _oneToRunThemAll = Task.Run(() => { Render(CanvasSize); });
                else
                    MessageBox.Show("Other calculations are still in progress\nCould not render asynchronously", "Action in Progress", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
            }
        }

        public void Render(Tuple<int, int> CanvasSize)
        {
            if (MainMaze != null)
            {
                //_mazeBMP = MainMaze.RenderMaze(CanvasSize.Item1, CanvasSize.Item2, MazeStyle);
                //Maze = Utilities.BitmapToImageSource(MainMaze.RenderPath((System.Drawing.Bitmap)_mazeBMP.Clone(), MazeStyle));

                Stopwatch RenderTime = new Stopwatch();
                RenderTime.Start();

                _mazeBMP = Renderer.RenderMaze(CanvasSize.Item1, CanvasSize.Item2, MainMaze, MazeStyle);

                BitmapImage mazeRender = Utilities.BitmapToImageSource(Renderer.RenderPath((System.Drawing.Bitmap)_mazeBMP.Clone(), MainMaze, MazeStyle));
                mazeRender.Freeze();
                Maze = mazeRender;

                RenderTime.Stop();
                LastRenderTime = RenderTime.ElapsedMilliseconds;

                Status = "Rendering done";
                RenderSizeX = MainMaze.renderSizeX;
                RenderSizeY = MainMaze.renderSizeY;
                CanvasSizeX = CanvasSize.Item1;
                CanvasSizeY = CanvasSize.Item2;
                GC.Collect();
            }
        }

        public void RenderPath()
        {
            Stopwatch RenderTime = new Stopwatch();
            RenderTime.Start();

            //draw on Bitmap without changing the original
            BitmapImage mazeRender = Utilities.BitmapToImageSource(Renderer.RenderPath((System.Drawing.Bitmap)_mazeBMP.Clone(), MainMaze, MazeStyle));
            mazeRender.Freeze();
            Maze = mazeRender;

            RenderTime.Stop();
            LastRenderTime = RenderTime.ElapsedMilliseconds;
            GC.Collect();   //collect the leftovers
        }

        //overloaded method (similar to the one in Maze) to render specific paths
        public void RenderPath(Path path)
        {
            Stopwatch RenderTime = new Stopwatch();
            RenderTime.Start();

            BitmapImage mazeRender = Utilities.BitmapToImageSource(Renderer.RenderPath((System.Drawing.Bitmap)_mazeBMP.Clone(), MainMaze, MazeStyle, path));
            mazeRender.Freeze();
            Maze = mazeRender;

            RenderTime.Stop();
            LastRenderTime = RenderTime.ElapsedMilliseconds;
            GC.Collect();   //collect the leftovers
        }

        #endregion Maze_render

        #region Data_manipulation

        public void NewMaze()
        {
            if (_oneToRunThemAll == null || _oneToRunThemAll.Status != TaskStatus.Running)
            {
                _oneToRunThemAll = null;
                MainMaze = null;
                _currentFilePath = null;
                Maze = null;
                Status = "New file created";
            }
            else
                MessageBox.Show("Other calculations are still in progress\nCould not perform the action", "Action in Progress", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
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
                FileName = "Maze",
                Filter = "Maze files (*.maze)|*.maze|All files (*.*)|*.*",
                FilterIndex = 1,
                AddExtension = true,
                //dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                RestoreDirectory = true
            };

            if (_currentFilePath == null)
            {
                if (dialog.ShowDialog() == true)
                    _currentFilePath = dialog.FileName;
            }

            if (_currentFilePath != null)
            {
                //if it does not exist -> run; (check if the task exists) ONLY THEN check if there is not any other running task and run
                //[use of operator evaluation] for thread safety
                if (_oneToRunThemAll == null || _oneToRunThemAll.Status != TaskStatus.Running)
                    _oneToRunThemAll = Task.Run(() =>
                    {
                        Status = "Saving...";
                        try
                        {
                            Stopwatch ProcessTime = new Stopwatch();
                            ProcessTime.Start();

                            if (Utilities.SaveBySerializing<Maze>(MainMaze, _currentFilePath) == false) //means it failed
                                throw new FormatException();

                            ProcessTime.Stop();
                            LastGenTime = ProcessTime.ElapsedMilliseconds;
                            Status = "Saving done";
                        }
                        catch (Exception exc)
                        {
                            Status = "Saving failed";
                            MessageBox.Show("An unhandled saving exception just occured: " + exc.Message, "Unhandled saving exception", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                else
                    MessageBox.Show("Other calculations are still in progress\nCould not perform the action", "Action in Progress", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
            }
        }

        public void LoadMaze()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Load Maze",
                FileName = "Maze",
                Filter = "Maze files (*.maze)|*.maze|All files (*.*)|*.*",
                FilterIndex = 1,
                AddExtension = true,
                //dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                RestoreDirectory = true
            };

            _currentFilePath = null;
            if (dialog.ShowDialog() == true)
                _currentFilePath = dialog.FileName;

            //asynhronously load the maze, in a try/catch
            if (_currentFilePath != null)
            {
                //if it does not exist -> run; (check if the task exists) ONLY THEN check if there is not any other running task and run
                //[use of operator evaluation] for thread safety
                if (_oneToRunThemAll == null || _oneToRunThemAll.Status != TaskStatus.Running)
                    _oneToRunThemAll = Task.Run(() =>
                    {
                        Status = "Commencing loading operation...";
                        try
                        {
                            Stopwatch ProcessTime = new Stopwatch();
                            ProcessTime.Start();

                            MainMaze = Utilities.LoadFromTheDead<Maze>(dialog.FileName);

                            //fix after deserialization; [nonserialized] items are set to null instead of the type written in their class constructor
                            MainMaze.GreedyPath = new Path(AlgoType.Greedy);
                            MainMaze.DijkstraPath = new Path(AlgoType.Dijkstra);
                            MainMaze.AStarPath = new Path(AlgoType.Astar);

                            //if there is no start/end-node selected, load the generating tree
                            if (MainMaze.startNode == null || MainMaze.endNode == null)
                            {
                                MainMaze.pathToRender = (Path)MainMaze.DFSTree.Clone();
                            }
                            else
                            {
                                //recalculate the paths (using the best algorithm)
                                MainMaze.AStar();
                                MainMaze.pathToRender = MainMaze.AStarPath;
                            }
                            ProcessTime.Stop();
                            LastGenTime = ProcessTime.ElapsedMilliseconds;
                            Status = "Loading done";
                            NodeCountX = MainMaze.NodeCountX;
                            NodeCountY = MainMaze.NodeCountY;
                            NodeCount = MainMaze.nodes.Length;

                            RenderAsync(nonAsync: true);   //run normal render, because a parallel task is already running
                        }
                        catch (Exception exc)
                        {
                            Status = "Loading failed";
                            MessageBox.Show("An unhandled loading exception just occured: " + exc.Message, "Unhandled loading exception", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                else
                    MessageBox.Show("Other calculations are still in progress\nCould not perform the action", "Action in Progress", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
            }
        }

        public void Export()
        {
            if (MainMaze == null)   //only save maze if there is one
            {
                MessageBox.Show("Cannot export an empty maze", "Export failed", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ImageFormat[] ImageFormats = { ImageFormat.Bmp, ImageFormat.Jpeg, ImageFormat.Gif, ImageFormat.Tiff, ImageFormat.Png, ImageFormat.Icon };
            ExportSettings exportWindow = new ExportSettings(RenderSizeX, RenderSizeY, NodeCountX, NodeCountY, MazeStyle.WallThickness, MazeStyle.IsSquare);
            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Export image",
                FileName = "Labyrinth",
                //filter order is the same as in the ImageFormats array
                Filter = "BMP files (*.bmp)|*.bmp|JPG files (*.jpg)|*.jpg|GIF files (*.gif)|*.gif|TIFF files (*.tiff)|*.tiff|PNG files (*.png)|*.png|ICON files (*.ico)|*.ico",
                FilterIndex = 5,
                AddExtension = true,
                //dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                RestoreDirectory = true
            };

            if (dialog.ShowDialog() == true && exportWindow.ShowDialog() == true)
            {
                //if it does not exist -> run; (check if the task exists) ONLY THEN check if there is not any other running task and run
                //[use of operator evaluation] for thread safety
                if (_oneToRunThemAll == null || _oneToRunThemAll.Status != TaskStatus.Running)
                    _oneToRunThemAll = Task.Run(() =>
                    {
                        Status = "Commencing export...";
                        try
                        {
                            Stopwatch ProcessTime = new Stopwatch();
                            ProcessTime.Start();

                            MazeStyle.IsSquare = exportWindow.IsSquare;
                            //draws the path(generates bitmap)
                            System.Drawing.Bitmap mazeRender = Renderer.RenderPath(Renderer.RenderMaze(exportWindow.ExportSizeX, exportWindow.ExportSizeY, MainMaze, MazeStyle, isRendering: true), MainMaze, MazeStyle);

                            //resize the rendered bitmap (only does it, if it's needed) and saves it under the specified file format
                            new System.Drawing.Bitmap(mazeRender, exportWindow.ExportSizeX, exportWindow.ExportSizeY).Save(dialog.FileName, ImageFormats[dialog.FilterIndex - 1]);

                            ProcessTime.Stop();
                            LastGenTime = ProcessTime.ElapsedMilliseconds;
                            LastRenderTime = ProcessTime.ElapsedMilliseconds;

                            Status = "Export done";
                            MessageBox.Show("Export done", "Export done", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception exc)
                        {
                            Status = "Export failed";
                            MessageBox.Show("An unhandled exporting exception just occured: " + exc.Message, "Unhandled export exception", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                else
                    MessageBox.Show("Other calculations are still in progress\nCould not perform the action", "Action in Progress", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
            }
        }

        #endregion Data_manipulation
    }
}