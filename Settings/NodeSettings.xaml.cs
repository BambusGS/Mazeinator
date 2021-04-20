using System.Windows;
using System.Windows.Controls;  //Button

namespace Mazeinator
{
    /// <summary>
    /// Interaction logic for NodeSettings.xaml
    /// </summary>

    public enum NodeAction
    {
        NorthNodeSelect,
        EastNodeSelect,
        SouthNodeSelect,
        WestNodeSelect,
        StartNodeSelect,
        EndNodeSelect,
        AUX
    }

    public partial class NodeSettings : Window
    {
        public int selector = 0;
        public NodeAction nodeAction;
        public AlgoType lastAlgorithm;  //set in window_loaded

        public NodeSettings(int nodeSize)
        {
            InitializeComponent();
            SetVisualIndicator(Utilities.Clamp(nodeSize, 2, 12));
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Tag)
            {
                case "NorthNodeSelect":
                    nodeAction = NodeAction.NorthNodeSelect;
                    break;

                case "EastNodeSelect":
                    nodeAction = NodeAction.EastNodeSelect;
                    break;

                case "SouthNodeSelect":
                    nodeAction = NodeAction.SouthNodeSelect;
                    break;

                case "WestNodeSelect":
                    nodeAction = NodeAction.WestNodeSelect;
                    break;

                case "StartNodeSelect":
                    nodeAction = NodeAction.StartNodeSelect;
                    break;

                case "EndNodeSelect":
                    nodeAction = NodeAction.EndNodeSelect;
                    break;

                case "AUX":
                    nodeAction = NodeAction.AUX;
                    break;
            }

            this.DialogResult = true;
        }

        private void SetVisualIndicator(int nodeSize)
        {
            //works in WPF units
            const int gridWidth = 40;

            lineTL.X2 = gridWidth / 2 - nodeSize;
            lineTL.Y2 = gridWidth / 2 - nodeSize;

            lineTR.X2 = gridWidth / 2 + nodeSize;
            lineTR.Y2 = gridWidth / 2 - nodeSize;

            lineBL.X2 = gridWidth / 2 - nodeSize;
            lineBL.Y2 = gridWidth / 2 + nodeSize;

            lineBR.X2 = gridWidth / 2 + nodeSize;
            lineBR.Y2 = gridWidth / 2 + nodeSize;
        }

        public void TargetSwap(int swapDirection = -1)
        {
            switch (swapDirection)
            {
                case Node.North:
                    this.Top += this.Height / 3;

                    NorthBtn.SetValue(Grid.RowProperty, 1);
                    TargetGrid.SetValue(Grid.RowProperty, 0);
                    break;

                case Node.East:
                    this.Left -= this.Width / 3;

                    EastBtn.SetValue(Grid.ColumnProperty, 1);
                    TargetGrid.SetValue(Grid.ColumnProperty, 2);
                    break;

                case Node.South:
                    this.Top -= this.Height / 3;

                    SouthBtn.SetValue(Grid.RowProperty, 1);
                    TargetGrid.SetValue(Grid.RowProperty, 2);
                    break;

                case Node.West:
                    this.Left += this.Width / 3;

                    WestBtn.SetValue(Grid.ColumnProperty, 1);
                    TargetGrid.SetValue(Grid.ColumnProperty, 0);
                    break;

                default:    //reset it
                    NorthBtn.SetValue(Grid.RowProperty, 0);
                    EastBtn.SetValue(Grid.ColumnProperty, 2);
                    SouthBtn.SetValue(Grid.RowProperty, 2);
                    WestBtn.SetValue(Grid.ColumnProperty, 0);

                    TargetGrid.SetValue(Grid.ColumnProperty, 1);
                    TargetGrid.SetValue(Grid.RowProperty, 1);
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            switch (lastAlgorithm)
            {
                case AlgoType.Greedy:
                    AUXbtn.Content = "Greedy";
                    break;

                case AlgoType.Dijkstra:
                    AUXbtn.Content = "Dijkstra";
                    break;

                case AlgoType.Astar:
                    AUXbtn.Content = "A☆";
                    break;

                default:
                    break;
            }
        }
    }
}