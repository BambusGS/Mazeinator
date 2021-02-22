using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Mazeinator
{
    //https://www.wpf-tutorial.com/dialogs/creating-a-custom-input-dialog/
    //https://www.hanselman.com/blog/learning-wpf-with-babysmash-configuration-with-databinding
    
    /// <summary>
    /// Interaction logic for StyleSettings.xaml
    /// </summary>
    public partial class StyleSettings : Window
    {
        public StyleSettings()
        {
            InitializeComponent();
        }

        private void btnDialogOK_Click(object sender, RoutedEventArgs e)
        {                        
            this.DialogResult = true;            
        }

        private void DefaultValues(object sender, RoutedEventArgs e)
        {
            this.DataContext = new Style();
        }
    }
}
