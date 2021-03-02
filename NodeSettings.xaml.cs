using System;
using System.Windows;

namespace Mazeinator
{
    /// <summary>
    /// Interaction logic for NodeSettings.xaml
    /// </summary>
    public partial class NodeSettings : Window
    {
        public int selector = 0;

        public NodeSettings()
        {
            InitializeComponent();
        }


        private void NorthNodeSelect(object sender, RoutedEventArgs e)
        {
            selector = Node.North;
            this.DialogResult = true;
        } 
        
        private void EastNodeSelect(object sender, RoutedEventArgs e)
        {
            selector = Node.East;
            this.DialogResult = true;
        }
        
        private void SouthNodeSelect(object sender, RoutedEventArgs e)
        {
            selector = Node.South;
            this.DialogResult = true;
        }
        
        private void WestNodeSelect(object sender, RoutedEventArgs e)
        {
            selector = Node.West;
            this.DialogResult = true;
        }

        private void StartNodeSelect(object sender, RoutedEventArgs e)
        {
            selector = 10;
            this.DialogResult = true;
        }

        private void EndNodeSelect(object sender, RoutedEventArgs e)
        {
            selector = 11;
            this.DialogResult = true;
        }
    }
}