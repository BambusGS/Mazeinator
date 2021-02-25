using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;   //Color
using System.Drawing.Drawing2D; //LineCap

namespace Mazeinator
{
    /// <summary>
    /// Maze style class; holds the colors and thickness properties + booleans of what should be rendered
    /// </summary>
    public class Style : ICloneable
    {
        #region Variables
        private LineCap[] LineCapOptions = { LineCap.Square, LineCap.Triangle, LineCap.Round, LineCap.SquareAnchor, LineCap.DiamondAnchor, LineCap.RoundAnchor };
        public LineCap WallEndCap { get; set; } = LineCap.Triangle;

        //public Color WallColor { get; set; } = Color.Black;
        public Color WallColor { get; set; } = Colors.Black;

        public Color NodeColor { get; set; } = Colors.LightGray;
        public Color PointColor { get; set; } = Colors.Yellow;
        public Color RootColorBegin { get; set; } = Colors.Blue;
        public Color RootColorEnd { get; set; } = Colors.Black;
        public Color StartPointColor { get; set; } = Colors.LawnGreen;
        public Color EndPointColor { get; set; } = Colors.Red;
        public Color BackgroundColor { get; set; } = Colors.SteelBlue;

        public int WallThickness { get; set; } = 0;
        public int NodeThickness { get; set; } = 0;
        public int PointThickness { get; set; } = 0;
        public int RootThickness { get; set; } = 0;

        public bool RenderWall { get; set; } = true;
        public bool RenderNode { get; set; } = true;
        public bool RenderPoint { get; set; } = true;
        public bool RenderRoot { get; set; } = true;
        public bool RenderRootRootNode { get; set; } = true;
        #endregion

        //https://stackoverflow.com/questions/6569486/creating-a-copy-of-an-object-in-c-sharp
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
