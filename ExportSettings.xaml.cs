using System;
using System.Windows;

namespace Mazeinator
{
    /// <summary>
    /// Interaction logic for ExportSettings.xaml
    /// </summary>

    public partial class ExportSettings : Window
    {
        public int ExportSizeX { get; set; }
        public int ExportSizeY { get; set; }

        private double _aspectRatio;

        public bool PixelPerfectRender { get; set; } = true;
        public bool MaintainAspectRatio { get; set; } = true;

        public ExportSettings(int SizeX, int SizeY)
        {
            InitializeComponent();
            DataContext = this;

            ExportSizeX = SizeX;
            ExportSizeY = SizeY;
            _aspectRatio = (double)ExportSizeX / (double)ExportSizeY;
        }

        private void Export(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
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
            if (MaintainAspectRatio == true && XIUD.Value != null && YIUD.Value != null)
            {
                _isChangingX = true;

                //stop recursion from occuring by checking if the value change actually occurred
                if (_isChangingY == false)
                {
                    YIUD.Value = (int)Math.Round((double)XIUD.Value / _aspectRatio);
                }
            }
            _isChangingX = false;
        }

        private void YIUD_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (MaintainAspectRatio == true && XIUD.Value != null && YIUD.Value != null)
            {
                _isChangingY = true;

                //stop recursion from occuring by checking if the value change actually occurred
                if (_isChangingX == false)
                {
                    XIUD.Value = (int)Math.Round((double)YIUD.Value * _aspectRatio);
                }
            }
            _isChangingY = false;
        }

        #endregion MaintainAspectRation
    }
}