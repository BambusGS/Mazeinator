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

        private void IntegerUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (MaintainAspectRatio == true)
            {
                switch ((sender as Xceed.Wpf.Toolkit.IntegerUpDown).Name)
                {
                    case "XIUD":
                        YIUD.Value = (int)(ExportSizeX * _aspectRatio);
                        //ExportSizeY = (int)(ExportSizeX * _aspectRatio);
                        Console.WriteLine("XV" + XIUD.Value);
                        Console.WriteLine("X" + ExportSizeX);
                        Console.WriteLine(_aspectRatio);
                        break;

                    case "YIUD":
                        XIUD.Value = (int)(ExportSizeY * _aspectRatio);

                        Console.WriteLine("XV" + XIUD.Value);
                        Console.WriteLine("X" + ExportSizeX);
                        Console.WriteLine(_aspectRatio);
                        break;

                    default:
                        break;
                }
            }
        }

        private void AspectRatio(object sender, RoutedEventArgs e)
        {
            _aspectRatio = (double)ExportSizeX / (double)ExportSizeY;
        }

        private void XIUD_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (MaintainAspectRatio == true && XIUD.Value != null && YIUD.Value != null)
            {
                Console.WriteLine("XIUD");
                Console.WriteLine("YIUD:" + YIUD.Value);
                Console.WriteLine("XIUD_calc:" + (XIUD.Value / _aspectRatio));
                Console.WriteLine("DIFF: " + (XIUD.Value / _aspectRatio - YIUD.Value));
                Console.WriteLine("aspectRatio: " + (1 / _aspectRatio) + "\n");

                if (Math.Abs((double)(XIUD.Value / _aspectRatio - YIUD.Value)) >= (1 / _aspectRatio))
                    YIUD.Value = (int)(XIUD.Value / _aspectRatio);
            }
        }

        private void YIUD_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (MaintainAspectRatio == true && XIUD.Value != null && YIUD.Value != null)
            {
                Console.WriteLine("YIUD");
                Console.WriteLine("XIUD:" + XIUD.Value);
                Console.WriteLine("YIUD_calc:" + (YIUD.Value * _aspectRatio));
                Console.WriteLine("DIFF: " + (YIUD.Value * _aspectRatio - XIUD.Value));
                Console.WriteLine("aspectRatio: " + (_aspectRatio) + "\n");

                if (Math.Abs((double)(YIUD.Value * _aspectRatio - XIUD.Value)) >= (_aspectRatio))
                    XIUD.Value = (int)(YIUD.Value * _aspectRatio);
            }
        }
    }
}