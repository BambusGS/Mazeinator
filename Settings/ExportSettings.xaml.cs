using System;
using System.ComponentModel;
using System.Windows;

namespace Mazeinator
{
    /// <summary>
    /// Interaction logic for ExportSettings.xaml
    /// </summary>

    public partial class ExportSettings : Window, INotifyPropertyChanged
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

        private int _exportSizeX = 0; public int ExportSizeX { get => _exportSizeX; set { _exportSizeX = value; OnPropertyChanged(nameof(ExportSizeX)); } }
        private int _exportSizeY = 0; public int ExportSizeY { get => _exportSizeY; set { _exportSizeY = value; OnPropertyChanged(nameof(ExportSizeY)); } }
        private double _aspectRatio;

        //diagnostic render indicators
        public int nodeCountX { get; set; }

        public int nodeCountY { get; set; }
        private int _cellSizeX = 0; public int cellSizeX { get => _cellSizeX; set { _cellSizeX = value; OnPropertyChanged(nameof(cellSizeX)); } }
        private int _cellSizeY = 0; public int cellSizeY { get => _cellSizeY; set { _cellSizeY = value; OnPropertyChanged(nameof(cellSizeY)); } }
        private int _renderSizeX = 0; public int renderSizeX { get => _renderSizeX; set { _renderSizeX = value; OnPropertyChanged(nameof(renderSizeX)); } }
        private int _renderSizeY = 0; public int renderSizeY { get => _renderSizeY; set { _renderSizeY = value; OnPropertyChanged(nameof(renderSizeY)); } }
        private int _wallThickness;
        private bool _isSquare = true; public bool IsSquare { get => _isSquare; set { _isSquare = value; OnPropertyChanged(nameof(IsSquare)); } }
        private bool _maintanAspectRatio = true; public bool MaintainAspectRatio { get => _maintanAspectRatio; set { _maintanAspectRatio = value; OnPropertyChanged(nameof(MaintainAspectRatio)); } }

        #endregion Variables

        public ExportSettings(int SizeX, int SizeY, int NodesX, int NodesY, int wallThickness, bool square)
        {
            InitializeComponent();
            this.DataContext = this;

            ExportSizeX = SizeX;
            ExportSizeY = SizeY;
            _aspectRatio = (double)ExportSizeX / (double)ExportSizeY;

            nodeCountX = NodesX;
            nodeCountY = NodesY;
            _wallThickness = wallThickness;
            IsSquare = square;

            CalculatePerfectRenderSize();
        }

        private void Export(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void AutoSize(object sender, RoutedEventArgs e)
        {
            MaintainAspectRatio = false;
            ExportSizeX = renderSizeX;
            ExportSizeY = renderSizeY;
        }

        private void SquareNodes(object sender, RoutedEventArgs e)
        {
            CalculatePerfectRenderSize();
            MaintainAspectRatio = IsSquare;
        }

        #region MaintainAspectRation

        private void AspectRatio(object sender, RoutedEventArgs e)
        {
            _aspectRatio = (double)ExportSizeX / (double)ExportSizeY;
        }

        //stop recursion from occuring by checking if the value change actually occurred
        private bool _isChangingX = false;

        private bool _isChangingY = false;

        private void XIUD_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (XIUD.Value != null && YIUD.Value != null)
            {
                _isChangingX = true;
                CalculatePerfectRenderSize();

                if (MaintainAspectRatio == true)
                {
                    //stop recursion from occuring by checking if the value change actually occurred
                    if (_isChangingY == false)
                    {
                        ExportSizeY = (int)Math.Round((double)XIUD.Value / _aspectRatio);
                    }
                }
                _isChangingX = false;
            }
        }

        private void YIUD_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (XIUD.Value != null && YIUD.Value != null)
            {
                _isChangingY = true;

                CalculatePerfectRenderSize();

                if (MaintainAspectRatio == true)
                {
                    //stop recursion from occuring by checking if the value change actually occurred
                    if (_isChangingX == false)
                    {
                        ExportSizeX = (int)Math.Round((double)YIUD.Value * _aspectRatio);
                    }
                }
                _isChangingY = false;
            }

            #endregion MaintainAspectRation
        }

        //function for cellSize calculation; exactly the same as in Maze.RenderMaze()
        private void CalculatePerfectRenderSize()
        {
            
                //this pen's width is needed for tight cellSize calculation; therefore, I cannot use cellSize for it's width
                int cellWallWidthX = (int)((ExportSizeX / (5 * (nodeCountX + 4))) * _wallThickness / 100.0);
                int cellWallWidthY = (int)((ExportSizeY / (5 * (nodeCountY + 4))) * _wallThickness / 100.0);

                //prevent the cell wall from dissapearing
                if (cellWallWidthX <= 1)
            {
                cellWallWidthX = 1;
            }
            //else if (cellWallWidthX % 2 == 1) cellWallWidthX -= 1;
            if (cellWallWidthY <= 1)
            {
                cellWallWidthY = 1;
            }
            //else if (cellWallWidthY % 2 == 1) cellWallWidthY -= 1;

            int cellWallWidth = (cellWallWidthX < cellWallWidthY) ? cellWallWidthX : cellWallWidthY;

                //calculate the needed cell size in the specific dimension + take into account the thickness of the walls
                cellSizeX = (int)(ExportSizeX - cellWallWidth) / nodeCountX;
                cellSizeY = (int)(ExportSizeY - cellWallWidth) / nodeCountY;

                //prevent the cell from dissapearing
                if (cellSizeX <= 1)
            {
                cellSizeX = 1;
            }

            if (cellSizeY <= 1)
            {
                cellSizeY = 1;
            }

            //finds out the smaller cell size in order for the cell to be square
            int cellSize = (cellSizeX < cellSizeY) ? cellSizeX : cellSizeY;
                if (IsSquare == true)
                {
                    cellSizeX = cellSize;
                    cellSizeY = cellSize;
                }

                //generate a large bitmap as a multiple of maximum node width/height; use of integer division as flooring
                renderSizeX = cellSizeX * nodeCountX + cellWallWidth;
                renderSizeY = cellSizeY * nodeCountY + cellWallWidth;
            
        }
    }
}