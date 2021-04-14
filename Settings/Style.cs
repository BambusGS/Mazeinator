using System;
using System.Drawing.Drawing2D; //LineCap
using System.Windows.Media;   //Color

namespace Mazeinator
{
    /// <summary>
    /// Maze style class; holds the colors and thickness properties + booleans of what should be rendered
    /// </summary>
    public class Style : ICloneable
    {
        #region Variables

        public bool IsSquare { get; set; } = true;

        public LineCap[] LineCapOptions = { LineCap.Square, LineCap.Triangle, LineCap.Round, LineCap.SquareAnchor, LineCap.DiamondAnchor, LineCap.RoundAnchor };
        public LineCap WallEndCap { get; set; } = LineCap.Triangle;
        public LineCap PathEndCap { get; set; } = LineCap.Round;

        public Color WallColor { get; set; } = Colors.Black;

        public Color NodeColor { get; set; } = Colors.LightGray;
        public Color PointColor { get; set; } = Colors.Yellow;
        public Color RootColorBegin { get; set; } = Colors.Red;
        public Color RootColorEnd { get; set; } = Colors.DarkRed;
        public Color StartPointColor { get; set; } = Colors.Lime;
        public Color EndPointColor { get; set; } = Colors.Red;
        public Color BackgroundColor { get; set; } = Colors.SteelBlue;

        public int WallThickness { get; set; } = 100;
        public int NodeThickness { get; set; } = 100;
        public int PointThickness { get; set; } = 100;

        public int PathThickness { get; set; } = 200;
        public int RootThickness { get; set; } = 100;

        public bool RenderNode { get; set; } = false;
        public bool RenderPoint { get; set; } = false;
        public bool RenderRoot { get; set; } = true;
        public bool RenderRootRootNode { get; set; } = true;

        #endregion Variables

        //Implements the ICloneable interface; deeply inspired by the link below
        //https://stackoverflow.com/questions/6569486/creating-a-copy-of-an-object-in-c-sharp
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}