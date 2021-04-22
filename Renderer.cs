using System;
using System.Drawing;   //Bitmap and Graphics
using System.Drawing.Drawing2D;

namespace Mazeinator
{
    internal class Renderer
    {
        private Pen _wallsPen, _nodePen, _pointPen, _rootPen;

        /// <summary>
        /// Maze rendering function; works in real screen pixels
        /// </summary>
        /// <param name="canvasWidth">Deised image width in px</param>
        /// <param name="canvasHeight">Desired image height in px</param>
        ///<param name="style">Maze style class definition</param>
        ///<param name="isRendering">Enables measures in order to render more perfectly</param>
        /// <param name="fill">Specifies whether to fill background with solid color up to the specified size </param>
        /// <returns>Bitmap rendered maze of the specified size</returns>
        public Bitmap RenderMaze(int canvasWidth, int canvasHeight, Maze maze, Style style, bool isRendering = false, bool fill = false)
        {
            //!maze render size is compromised when isRendering == true -> auto in-window re-render needed

            //this pen's width is needed for tight cellSize calculation; therefore, I cannot use cellSize for it's width
            int cellWallWidthX = (int)((canvasWidth / (5 * (maze.NodeCountX + 4))) * style.WallThickness / 100.0);
            int cellWallWidthY = (int)((canvasHeight / (5 * (maze.NodeCountY + 4))) * style.WallThickness / 100.0);

            //prevent the cell wall from dissapearing
            if (cellWallWidthX <= 1) cellWallWidthX = 1;
            //else if (cellWallWidthX % 2 == 1) cellWallWidthX -= 1;
            if (cellWallWidthY <= 1) cellWallWidthY = 1;
            //else if (cellWallWidthY % 2 == 1) cellWallWidthY -= 1;

            int cellWallWidth = (cellWallWidthX < cellWallWidthY) ? cellWallWidthX : cellWallWidthY;
            Pen _wallsPen = new Pen(Utilities.ConvertColor(style.WallColor), cellWallWidth)
            {
                StartCap = style.WallEndCap,
                EndCap = style.WallEndCap
            };

            //calculate the needed cell size in the specific dimension + take into account the thickness of the walls
            int cellSizeX = (int)(canvasWidth - _wallsPen.Width) / maze.NodeCountX;
            int cellSizeY = (int)(canvasHeight - _wallsPen.Width) / maze.NodeCountY;

            //prevent the cell from dissapearing
            if (cellSizeX <= 1) cellSizeX = 1;
            if (cellSizeY <= 1) cellSizeY = 1;

            //finds out the smaller cell size in order for the cell to be square
            int cellSize = (cellSizeX < cellSizeY) ? cellSizeX : cellSizeY;
            if (style.IsSquare == true)
            {
                cellSizeX = cellSize;
                cellSizeY = cellSize;
            }

            //properly resize all the maze.nodes; works with INT so as to not render blurry cells
            int wallOffset = (int)(_wallsPen.Width / 2);
            for (int column = 0; column < maze.NodeCountX; column++)
            {
                for (int row = 0; row < maze.NodeCountY; row++)
                {
                    maze.nodes[column, row].SetBounds(column * cellSizeX + wallOffset, row * cellSizeY + wallOffset, cellSizeX, cellSizeY);
                }
            }

            //initialize all the pens that can be changed by serialization or by user
            Pen _nodePen = new Pen(Utilities.ConvertColor(style.NodeColor), cellSize / 16 * style.NodeThickness / 100);
            Pen _pointPen = new Pen(Utilities.ConvertColor(style.PointColor), cellSize / 4 * style.PointThickness / 100);
            Pen _rootPen = new Pen(Utilities.ConvertColor(style.RootColorBegin), cellSize / 16 * style.RootThickness / 100);
            Pen _backgroundPen = new Pen(Utilities.ConvertColor(style.BackgroundColor));

            //set the pen instances fot this entire class (are held until the next render)
            this._wallsPen = _wallsPen;
            this._nodePen = _nodePen;
            this._pointPen = _pointPen;
            this._rootPen = _rootPen;

            //generate a large bitmap as a multiple of maximum node width/height; use of integer division as flooring
            maze.renderSizeX = cellSizeX * maze.NodeCountX + (int)_wallsPen.Width;
            maze.renderSizeY = cellSizeY * maze.NodeCountY + (int)_wallsPen.Width;
            Bitmap bmp = new Bitmap(maze.renderSizeX, maze.renderSizeY);

            //draw the maze.nodes onto the bitmap
            using (Graphics gr = Graphics.FromImage(bmp))
            {
                //sets up graphics for smooth circles and fills the background with solid color
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                if (isRendering == true)
                    gr.CompositingQuality = CompositingQuality.HighQuality;
                else
                    gr.CompositingQuality = CompositingQuality.HighSpeed;

                gr.FillRectangle(_backgroundPen.Brush, 0, 0, maze.renderSizeX, maze.renderSizeY);

                //this for loop draws the single maze.nodes onto the image (with automatic disabling of features when cells get too small)
                if (style.RenderNode == true && cellSize > 3)
                    foreach (Node node in maze.nodes) { node.DrawBox(gr, _nodePen, (int)_wallsPen.Width / 2 + 1); }

                //if (style.RenderRoot == true && cellSize > 7)
                //    foreach (Node node in maze.nodes) { node.DrawRootNode(gr, Utilities.ConvertColor(style.RootColorBegin), Utilities.ConvertColor(style.RootColorEnd), _rootPen.Width); }

                if (style.RenderPoint == true && cellSize > 3)
                    foreach (Node node in maze.nodes) { node.DrawCentre(gr, _pointPen); }

                //if (style.RenderRootRootNode == true && cellSize > 7)
                //    foreach (Node node in maze.nodes) { node.DrawRootRootNode(gr, _startNodePen); }

                //fill edges(&corners) with wallColor 1px offset from the centre in both X and Y
                gr.DrawRectangle(_wallsPen, -1, -1, maze.renderSizeX + 1, maze.renderSizeY + 1);

                if (isRendering == true)   //slower render, but renders every cell (thus eliminating AntiAliasing glitches
                {
                    for (int column = 0; column < maze.NodeCountX; column++)
                    {
                        for (int row = 0; row < maze.NodeCountY; row++)
                        {
                            maze.nodes[column, row].DrawWall(gr, _wallsPen);
                        }
                    }
                }
                else
                {
                    //I draw every second wall (testing proved it to be 2× faster) and then fill the edges
                    //  ☒☒☒☒☒☒☒☒☒
                    //  ☒☐☒☐☒☐☒☐☒
                    //  ☒☒☐☒☐☒☐☒☒
                    //  ☒☐☒☐☒☐☒☐☒
                    //  ☒☒☐☒☐☒☐☒☒
                    //  ☒☐☒☐☒☐☒☐☒

                    if (maze.NodeCountX > 2 || maze.NodeCountY > 2)
                    {
                        for (int column = 1; column < maze.NodeCountX; column++)
                        {
                            for (int row = 1; row < maze.NodeCountY; row += 2)
                            {
                                maze.nodes[column, (column % 2 == 0) ? row - 1 : row].DrawWall(gr, _wallsPen);
                            }
                        }
                    }
                    //fill the horizontal edges
                    for (int column = 0; column < maze.NodeCountX; column++)
                    {
                        maze.nodes[column, 0].DrawWall(gr, _wallsPen);
                        maze.nodes[column, maze.NodeCountY - 1].DrawWall(gr, _wallsPen);
                    }
                    //fill the vertical edges
                    for (int row = 0; row < maze.NodeCountY; row++)
                    {
                        maze.nodes[0, row].DrawWall(gr, _wallsPen);
                        maze.nodes[maze.NodeCountX - 1, row].DrawWall(gr, _wallsPen);
                    }
                }
            }

            if (fill == true)
            {   //overlay the cell image onto a solid color one and center it
                Bitmap backBmp = new Bitmap(canvasWidth, canvasHeight);
                using (Graphics gr = Graphics.FromImage(backBmp))
                {
                    gr.FillRectangle(_backgroundPen.Brush, 0, 0, canvasWidth, canvasHeight);
                    gr.DrawImage(bmp, ((canvasWidth - maze.renderSizeX) / 2), ((canvasHeight - maze.renderSizeY) / 2));
                }
                bmp = backBmp;
            }           
            return bmp;
        }

        public Bitmap RenderNode(Bitmap originalBMP, Maze maze, Node node, Style style)
        {
            if (_wallsPen != null && _nodePen != null && _pointPen != null || _rootPen != null)
            {
                using (Graphics gr = Graphics.FromImage(originalBMP))
                {
                    gr.SmoothingMode = SmoothingMode.AntiAlias;
                    gr.CompositingQuality = CompositingQuality.HighSpeed;

                    int thickness = maze.nodes[0, 0].Bounds.X + 1;
                    //draw over current walls with background color; then switch it back
                    gr.FillRectangle(new Pen(Utilities.ConvertColor(style.BackgroundColor)).Brush, node.Bounds.Left - thickness, node.Bounds.Top - thickness, node.Bounds.Width + 2 * thickness, node.Bounds.Height + 2 * thickness);

                    //_wallsPen.Color = Utilities.ConvertColor(style.BackgroundColor);
                    //node.DrawWalls(gr, _wallsPen);
                    //_wallsPen.Color = Utilities.ConvertColor(style.WallColor);

                    //redraw the adjecent cell's walls (the ones that have a wall)
                    for (int i = 0; i < node.Neighbours.Length; i++)
                    {
                        switch (i)
                        {
                            case Node.North:
                                if (node.Y > 0)
                                {
                                    maze.nodes[node.X, node.Y - 1].DrawWall(gr, _wallsPen);
                                }
                                break;

                            case Node.East:
                                if (node.X < maze.nodes.GetLength(0) - 1)
                                {
                                    maze.nodes[node.X + 1, node.Y].DrawWall(gr, _wallsPen);
                                }
                                break;

                            case Node.South:
                                if (node.Y < maze.nodes.GetLength(1) - 1)
                                {
                                    maze.nodes[node.X, node.Y + 1].DrawWall(gr, _wallsPen);
                                }
                                break;

                            case Node.West:
                                if (node.X > 0)
                                {
                                    maze.nodes[node.X - 1, node.Y].DrawWall(gr, _wallsPen);
                                }
                                break;

                            default:
                                break;
                        }
                    }

                    //redraw the selected node's properties
                    int cellSize = (node.Bounds.Width < node.Bounds.Height) ? node.Bounds.Width : node.Bounds.Height;

                    if (style.RenderNode == true && cellSize > 3)
                        node.DrawBox(gr, _nodePen, (int)_wallsPen.Width / 2 + 1);

                    if (style.RenderPoint == true && cellSize > 3)
                        node.DrawCentre(gr, _pointPen);

                    node.DrawWall(gr, _wallsPen);
                }
            }

            return originalBMP;
        }

        public Bitmap RenderPath(Bitmap originalBMP, Maze maze, Style style)
        {
            return RenderPath(originalBMP, maze, style, maze.pathToRender);
        }

        public Bitmap RenderPath(Bitmap originalBMP, Maze maze, Style style, Path currentPath)
        {
            maze.pathToRender = currentPath;
            if (currentPath != null && maze.nodes != null && (_wallsPen != null && _nodePen != null && _pointPen != null && _rootPen != null))
            {
                using (Graphics gr = Graphics.FromImage(originalBMP))
                {
                    gr.SmoothingMode = SmoothingMode.AntiAlias;
                    gr.CompositingQuality = CompositingQuality.HighSpeed;

                    //draw explored maze.nodes (while checking if they are large enough)
                    if (currentPath.exploredNodes != null)
                    {
                        for (int column = 0; column < maze.NodeCountX; column++)
                        {
                            for (int row = 0; row < maze.NodeCountY; row++)
                            {
                                //assign the maze.nodes's roots to the current path diagram
                                maze.nodes[column, row].Root = currentPath.exploredNodes[column, row];

                                if (style.RenderRoot == true && (maze.nodes[0, 0].Bounds.Width > 4 && maze.nodes[0, 0].Bounds.Height > 4))
                                    maze.nodes[column, row].DrawRootNode(gr, Utilities.ConvertColor(style.RootColorBegin), Utilities.ConvertColor(style.RootColorEnd), _rootPen.Width / 3, style.PathEndCap, style.PathEndCap);
                            }
                        }
                    }

                    //draw the shortest path
                    if (currentPath.path != null && currentPath.path.Count > 0)
                    {
                        //path has set rootnodes from END to START; path=4 -> we need 3 intervals
                        double step_R = (style.RootColorEnd.R - style.RootColorBegin.R) / (double)(currentPath.path.Count - 1);
                        double step_G = (style.RootColorEnd.G - style.RootColorBegin.G) / (double)(currentPath.path.Count - 1);
                        double step_B = (style.RootColorEnd.B - style.RootColorBegin.B) / (double)(currentPath.path.Count - 1);

                        //because root maze.nodes are drawn - there is no need to draw the first root node
                        for (int i = 0; i < currentPath.path.Count - 1; i++)
                        {
                            Color startColor = Color.FromArgb(style.RootColorBegin.R + (int)(step_R * i), style.RootColorBegin.G + (int)(step_G * i), style.RootColorBegin.B + (int)(step_B * i));
                            Color endColor = Color.FromArgb(style.RootColorBegin.R + (int)(step_R * (i + 1)), style.RootColorBegin.G + (int)(step_G * (i + 1)), style.RootColorBegin.B + (int)(step_B * (i + 1)));

                            //reverse the drawing order -> we draw from the start
                            float thickness = (float)Math.Ceiling((_pointPen.Width / 2 - 1) * style.PathThickness / style.PointThickness);
                            currentPath.path[currentPath.path.Count - i - 2].DrawRootNode(gr, startColor, endColor, thickness, style.PathEndCap, style.PathEndCap);
                        }
                    }

                    if (maze.startNode != null)
                    {
                        maze.startNode.DrawBox(gr, new Pen(Utilities.ConvertColor(style.StartPointColor), _nodePen.Width), (int)_wallsPen.Width / 2);
                        maze.startNode.DrawCentre(gr, new Pen(Utilities.ConvertColor(style.StartPointColor), (_pointPen.Width / 2) * style.PathThickness / style.PointThickness));
                    }

                    if (maze.endNode != null)
                    {
                        maze.endNode.DrawBox(gr, new Pen(Utilities.ConvertColor(style.EndPointColor), _nodePen.Width), (int)_wallsPen.Width / 2);
                        maze.endNode.DrawCentre(gr, new Pen(Utilities.ConvertColor(style.EndPointColor), (_pointPen.Width / 2) * style.PathThickness / style.PointThickness));
                    }
                }
            }

            return originalBMP;
        }
    }
}