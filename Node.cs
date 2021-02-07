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
        public Point Center
        {
            get
            {
                int x = Bounds.X + Bounds.Width / 2;
                int y = Bounds.Y + Bounds.Height / 2;
                return new Point(x, y);
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

        public void DrawWall(Graphics gr, Pen pen, bool smooth = true)
        {
            float size = pen.Width;
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

                    //draws circles in the corners of the walls IF the line is thick enough
                    if (smooth && size > 2)
                    {
                        switch (direction)
                        {
                            case North:
                                gr.FillEllipse(pen.Brush, Bounds.Right - size / 2, Bounds.Top - size / 2, size, size);
                                break;

                            case East:
                                gr.FillEllipse(pen.Brush, Bounds.Right - size / 2, Bounds.Bottom - size / 2, size, size);
                                break;

                            case South:
                                gr.FillEllipse(pen.Brush, Bounds.Left - size / 2, Bounds.Bottom - size / 2, size, size);
                                break;

                            case West:
                                gr.FillEllipse(pen.Brush, Bounds.Left - size / 2, Bounds.Top - size / 2, size, size);
                                break;
                        }
                    }
                }
            }
        }

        public void DrawBox(Graphics gr, Pen pen, int inset = 0)
        {
            int offset = (int)(pen.Width / 2.0) + inset;
            gr.DrawRectangle(pen, Bounds.X + offset, Bounds.Y + offset, Bounds.Width - 2 * offset, Bounds.Height - 2 * offset);
        }

        //Method for visualizing the node's centre
        public void DrawCentre(Graphics gr, Pen pen)
        {
            int size = (int)pen.Width;
            gr.FillEllipse(pen.Brush, Center.X - size / 2, Center.Y - size / 2, size, size);
        }

        //Method for drawing node's root via a gradient
        public void DrawRootNode(Graphics gr, Pen pen)
        {
            if (Root != null && Root != this)
            {
                Pen pen2 = new Pen(new System.Drawing.Drawing2D.LinearGradientBrush(Center, Root.Center, Color.Black, Color.Blue), pen.Width);
                gr.DrawLine(pen2, Center, Root.Center);
            }
        }

        //Highlight the start node
        public void DrawStartNode(Graphics gr, Pen pen)
        {
            int size = (int)pen.Width;
            if (Root == this)
                gr.FillEllipse(pen.Brush, Center.X - size / 2, Center.Y - size / 2, size, size);
        }
    }
}