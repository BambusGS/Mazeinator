using System.Drawing.Imaging; //Bitmap encoders (export operation)
using System.Windows;

namespace Mazeinator
{
    /// <summary>
    /// Interaction logic for ExportSettings.xaml
    /// </summary>

    public partial class ExportSettings : Window
    {

        public ExportSettings()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        private void Export(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}