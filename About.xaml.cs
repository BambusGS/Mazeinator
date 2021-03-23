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
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About(bool rickroll = false)
        {
            InitializeComponent();

            if (rickroll == true)
            {
                this.WindowStyle = WindowStyle.None;
                wbSample.Navigate("https://www.youtube.com/watch?v=QtBDL8EiNZo");
            }
        }
    }
}
