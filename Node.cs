using System;
using System.Drawing;

namespace Mazeinator
{
    [Serializable]
    internal class Node
    {
        public const int North = 0;
        public const int East = 1;
        public const int South = 2;
        public const int West = 3;

        //cell's logical position in the array
        public int X, Y;

        //Node's neighbouring cells
        public Node[] Neighbours = new Node[4];

        //Node's preceeding node in the spanning tree generation
        public Node Root = null;

        //Node's bounds in pixels
        public Rectangle Bounds;

        //Node's center
        public PointF Center
        {
            get
            {
                float x = Bounds.X + (float)Bounds.Width / 2;
                float y = Bounds.Y + (float)Bounds.Height / 2;
                return new PointF(x, y);
            }
        }

        //Just the logic consturctor
        public Node(int x, int y)
        {
            X = x;
            Y = y;
        }

        //Overloaded full constructor
        public Node(int x, int y, int xPixel, int yPixel, int width, int height)
        {
            X = x;
            Y = y;
            Bounds = new Rectangle(xPixel, yPixel, width, height);
        }

        //Special method for setting the cell drawing properties
        public void SetBounds(int xPixel, int yPixel, int width, int height)
        {
            Bounds = new Rectangle(xPixel, yPixel, width, height);
        }

        public void DrawWall(Graphics gr, Pen pen)
        {
            for (int direction = 0; direction < Neighbours.Length; direction++)
            {
                //draws the walls
                if (Neighbours[direction] == null)
                {
                    switch (direction)
                    {
                        case North:
                            gr.DrawLine(pen, Bounds.Left, Bounds.Top, Bounds.Right, Bounds.Top);
                            break;

                        case East:
                            gr.DrawLine(pen, Bounds.Right, Bounds.Top, Bounds.Right, Bounds.Bottom);
                            break;

                        case South:
                            gr.DrawLine(pen, Bounds.Left, Bounds.Bottom, Bounds.Right, Bounds.Bottom);
                            break;

                        case West:
                            gr.DrawLine(pen, Bounds.Left, Bounds.Top, Bounds.Left, Bounds.Bottom);
                            break;
                    }
                }
            }
        }

        public void DrawBox(Graphics gr, Pen pen, int inset = 0)
        {
            float offset = (float)(pen.Width / 2.0) + inset;
            gr.DrawRectangle(pen, Bounds.X + offset, Bounds.Y + offset, Bounds.Width - 2 * offset, Bounds.Height - 2 * offset);
        }

        //Method for visualizing the node's centre
        public void DrawCentre(Graphics gr, Pen pen)
        {
            float size = pen.Width;
            gr.FillEllipse(pen.Brush, Center.X - size / 2, Center.Y - size / 2, size, size);
        }

        //Method for drawing node's root via a normal pen
        public void DrawRootNode(Graphics gr, Pen pen)
        {
            if (Root != null && Root != this)
            {
                gr.DrawLine(pen, Center, Root.Center);
            }
        }

        //Highlight the start node
        public void DrawRootRootNode(Graphics gr, Pen pen)
        {
            float size = pen.Width;
            if (Root == this)
                gr.FillEllipse(pen.Brush, Center.X - size / 2, Center.Y - size / 2, size, size);
        }

        //Method overload for a gradient drawing
        public void DrawRootNode(Graphics gr, Tuple<Color, Color, float> brushHolder)
        {
            if (Root != null && Root != this)
            {
                Pen pen = new Pen(new System.Drawing.Drawing2D.LinearGradientBrush(Root.Center, Center, brushHolder.Item1, brushHolder.Item2), brushHolder.Item3);
                gr.DrawLine(pen, Center, Root.Center);
            }
        }

        public override string ToString()
        {
            return base.ToString() + ",X:" + X + ",Y:" + Y;
        }
    }
}