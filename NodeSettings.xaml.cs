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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            selector = 0;
            this.DialogResult = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            selector = 1;
            this.DialogResult = true;
        }
    }
}