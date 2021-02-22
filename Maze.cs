using System;
using System.Collections.Generic;   //List<T>
using System.Drawing;   //Bitmap and Graphics
using System.Drawing.Drawing2D;

namespace Mazeinator
{
    [Serializable]
    internal class Maze
    {
        #region Variables

        public Node[,] nodes = null;

        private int _nodeCountX, _nodeCountY;
        public int renderSizeX, renderSizeY;


        private Tuple<Color, float> _boxPenHolder = new Tuple<Color, float>(Color.LightGray, 0);
        private Tuple<Color, float> _pointPenHolder = new Tuple<Color, float>(Color.Yellow, 0);
        private Tuple<Color, float> _rootRootNodePenHolder = new Tuple<Color, float>(Color.LightGoldenrodYellow, 0);
        private Tuple<Color, Color, float> _rootPenHolder = new Tuple<Color, Color, float>(Color.Blue, Color.Black, 5);

        #endregion Variables

        /// <summary>
        /// Maze Node constructor
        /// </summary>
        /// <param name="width">Maze width in node count</param>
        /// <param name="height">Maze height in node count</param>
        /// <returns></returns>
        public Maze(int width, int height)
        {
            _nodeCountX = width;
            _nodeCountY = height;
            nodes = new Node[_nodeCountX, _nodeCountY];
            for (int column = 0; column < _nodeCountX; column++)
            {
                for (int row = 0; row < _nodeCountY; row++)
                {
                    nodes[column, row] = new Node(column, row);
                }
            }
        }

        /// <summary>
        /// Maze generating algorithm; requires the Maze class to have nodes array initialized
        /// </summary>
        /// <param name="startNode">Specific node to start generating from</param>
        public bool GenerateMaze(Node startNode = null)
        {
            Random rnd = new Random();
            //if start node wasn't defined, choose it randomly from the nodes array
            if (startNode == null)
            {
                startNode = nodes[rnd.Next(nodes.GetLength(0)), rnd.Next(nodes.GetLength(1))];
            }
            startNode.Root = startNode;

            //create the Visited array and the backtracking stack
            bool[,] Visited = new bool[_nodeCountX, _nodeCountY];
            int VisitedCellsCount = 0;
            Stack<Node> BackTheTrack = new Stack<Node>();
            List<int> UnvisitedNeighbours = new List<int>();

            //start with the starting node
            BackTheTrack.Push(startNode);
            Visited[startNode.X, startNode.Y] = true;
            VisitedCellsCount++;

            //loop until all the cells have been visited
            while (VisitedCellsCount < _nodeCountX * _nodeCountY)
            {
                UnvisitedNeighbours.Clear();
                if (BackTheTrack.Count == 0)    //not used, just for old safety
                    break;
                Node currentNode = BackTheTrack.Peek();

                //North check   not at border && the neighbouring cell has NOT been visited
                if (currentNode.Y > 0 && !Visited[currentNode.X, currentNode.Y - 1])
                    UnvisitedNeighbours.Add(Node.North);
                //East check
                if (currentNode.X < _nodeCountX - 1 && !Visited[currentNode.X + 1, currentNode.Y])
                    UnvisitedNeighbours.Add(Node.East);
                //South check
                if (currentNode.Y < _nodeCountY - 1 && !Visited[currentNode.X, currentNode.Y + 1])
                    UnvisitedNeighbours.Add(Node.South);
                //West check
                if (currentNode.X > 0 && !Visited[currentNode.X - 1, currentNode.Y])
                    UnvisitedNeighbours.Add(Node.West);

                if (UnvisitedNeighbours.Count != 0)
                {
                    int direction = UnvisitedNeighbours[rnd.Next(UnvisitedNeighbours.Count)];   //choose one of the unexplored directions
                    switch (direction)
                    {
                        case Node.North:
                            currentNode.Neighbours[Node.North] = nodes[currentNode.X, currentNode.Y - 1]; //set this node's neigbour to be the northern one
                            Visited[currentNode.X, currentNode.Y - 1] = true;                             //set the northern one to be also visited

                            currentNode.Neighbours[Node.North].Neighbours[Node.South] = currentNode;            //set the northern one's neighbour to be this node
                            currentNode.Neighbours[Node.North].Root = currentNode;

                            BackTheTrack.Push(currentNode.Neighbours[Node.North]);       //move to the northern node by pushing it onto the stack
                            break;

                        case Node.East:
                            currentNode.Neighbours[Node.East] = nodes[currentNode.X + 1, currentNode.Y];
                            Visited[currentNode.X + 1, currentNode.Y] = true;

                            currentNode.Neighbours[Node.East].Neighbours[Node.West] = currentNode;
                            currentNode.Neighbours[Node.East].Root = currentNode;

                            BackTheTrack.Push(currentNode.Neighbours[Node.East]);
                            break;

                        case Node.South:
                            currentNode.Neighbours[Node.South] = nodes[currentNode.X, currentNode.Y + 1];
                            Visited[currentNode.X, currentNode.Y + 1] = true;

                            currentNode.Neighbours[Node.South].Neighbours[Node.North] = currentNode;
                            currentNode.Neighbours[Node.South].Root = currentNode;

                            BackTheTrack.Push(currentNode.Neighbours[Node.South]);
                            break;

                        case Node.West:
                            currentNode.Neighbours[Node.West] = nodes[currentNode.X - 1, currentNode.Y];
                            Visited[currentNode.X - 1, currentNode.Y] = true;

                            currentNode.Neighbours[Node.West].Neighbours[Node.East] = currentNode;
                            currentNode.Neighbours[Node.West].Root = currentNode;

                            BackTheTrack.Push(currentNode.Neighbours[Node.West]);
                            break;
                    }
                    VisitedCellsCount++;
                }
                else { BackTheTrack.Pop(); }    //return one back, because there is nowhere to go
            }
            return true;
        }

        /// <summary>
        /// Blank maze generation algorithm that connects every cell's four sides
        /// </summary>
        /// <param name="startNode">Specific node to start generating from</param>
        /// <returns>Blank maze logic map</returns>
        public bool GenerateMazeBlank()
        {
            //fill in NORTH
            for (int column = 0; column < _nodeCountX; column++)
            {
                for (int row = 1; row < _nodeCountY; row++)
                {
                    nodes[column, row].Neighbours[Node.North] = nodes[column, row - 1];
                }
            }

            //fill in EAST
            for (int column = 0; column < _nodeCountX - 1; column++)
            {
                for (int row = 0; row < _nodeCountY; row++)
                {
                    nodes[column, row].Neighbours[Node.East] = nodes[column + 1, row];
                }
            }

            //fill in SOUTH
            for (int column = 0; column < _nodeCountX; column++)
            {
                for (int row = 0; row < _nodeCountY - 1; row++)
                {
                    nodes[column, row].Neighbours[Node.South] = nodes[column, row + 1];
                }
            }

            //fill in WEST
            for (int column = 1; column < _nodeCountX; column++)
            {
                for (int row = 0; row < _nodeCountY; row++)
                {
                    nodes[column, row].Neighbours[Node.West] = nodes[column - 1, row];
                }
            }

            foreach (Node node in nodes)
                node.Root = node;

            return true;
        }

        /// <summary>
        /// Maze rendering function
        /// </summary>
        /// <param name="canvasWidth">Deised image width in px</param>
        /// <param name="canvasHeight">Desired image height in px</param>
        /// <param name="square">Whether cells should be square</param>
        /// <param name="fill">Specifies whether to fill background with solid color up to specified parameters </param>
        /// <returns>Bitmap rendered maze of the specified size</returns>
        public Bitmap RenderMaze(int canvasWidth, int canvasHeight, Style style, bool square = true, bool fill = false)
        {
            //this pen's width is needed for tight cellSize calculation; therefore, I cannot use cellSize for it's width
            int cellWallWidthX = (int)((canvasWidth) / ((_nodeCountX + 4) * (5 + style.WallThickness)));
            int cellWallWidthY = (int)((canvasHeight) / ((_nodeCountY + 4) * (5 + style.WallThickness)));

            //prevent the cell from dissapearing
            if (cellWallWidthX <= 1) cellWallWidthX = 1;
            if (cellWallWidthY <= 1) cellWallWidthY = 1;

            int cellWallWidth = (cellWallWidthX < cellWallWidthY) ? cellWallWidthX : cellWallWidthY;
            Pen _wallsPen = new Pen(Utilities.ConvertColor(style.WallColor), cellWallWidth)
            {
                StartCap = style.WallEndCap,
                EndCap = style.WallEndCap
            };               

            //calculate the needed cell size in the specific dimension + take into account the thickness of the walls
            int cellSizeX = (int)(canvasWidth - _wallsPen.Width) / (_nodeCountX);
            int cellSizeY = (int)(canvasHeight - _wallsPen.Width) / (_nodeCountY);

            //prevent the cell from dissapearing
            if (cellSizeX <= 1) cellSizeX = 1;
            if (cellSizeY <= 1) cellSizeY = 1;

            //finds out the smaller cell size in order for the cell to be square
            int cellSize = (cellSizeX < cellSizeY) ? cellSizeX : cellSizeY;
            if (square == true)
            {
                cellSizeX = cellSize;
                cellSizeY = cellSize;
            }

            //properly resize all the nodes; works with INT so as to not render blurry cells
            int wallOffset = (int)(_wallsPen.Width / 2.0);
            for (int column = 0; column < _nodeCountX; column++)
            {
                for (int row = 0; row < _nodeCountY; row++)
                {
                    nodes[column, row].SetBounds(column * cellSizeX + wallOffset, row * cellSizeY + wallOffset, cellSizeX, cellSizeY);
                }
            }

            //initialize all the pens that can be changed by serialization or by user
            Pen _boxPen = new Pen(_boxPenHolder.Item1, cellSize / (16 + _boxPenHolder.Item2));
            Pen _pointPen = new Pen(_pointPenHolder.Item1, cellSize / (4 + _pointPenHolder.Item2));
            Pen _startNodePen = new Pen(_rootRootNodePenHolder.Item1, cellSize / (4 + _rootRootNodePenHolder.Item2));
            Pen _backgroundPen = new Pen(Utilities.ConvertColor(style.BackgroundColor));

            //generate a large bitmap as a multiple of maximum node width/height; use of integer division as flooring
            renderSizeX = cellSizeX * _nodeCountX + (int)_wallsPen.Width;
            renderSizeY = cellSizeY * _nodeCountY + (int)_wallsPen.Width;
            Bitmap bmp = new Bitmap(renderSizeX, renderSizeY);

            //draw the nodes onto the bitmap
            using (Graphics gr = Graphics.FromImage(bmp))
            {
                //sets up graphics for smooth circles and fills the background with solid color
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                gr.CompositingQuality = CompositingQuality.HighSpeed;
                gr.FillRectangle(_backgroundPen.Brush, 0, 0, renderSizeX, renderSizeY);

                //this for loop draws the single nodes onto the image (with automatic disabling of features when cells get too small)
                if (style.RenderNode == true && cellSize > 3)
                    foreach (Node node in nodes) { node.DrawBox(gr, _boxPen, (int)_wallsPen.Width / 2 + 1); }

                if (style.RenderRoot == true && cellSize > 7)
                    foreach (Node node in nodes) { node.DrawRootNode(gr, _rootPenHolder); }

                if (style.RenderPoint == true && cellSize > 3)
                    foreach (Node node in nodes) { node.DrawCentre(gr, _pointPen); }

                if (style.RenderRootRootNode == true && cellSize > 7)
                    foreach (Node node in nodes) { node.DrawRootRootNode(gr, _startNodePen); }

                if (style.RenderWall == true)
                {
                    //fill corners
                    {
                        gr.FillRectangle(_wallsPen.Brush, 0, 0, cellWallWidth, cellWallWidth);  //top-left
                        gr.FillRectangle(_wallsPen.Brush, 0, renderSizeY - cellWallWidth, cellWallWidth, cellWallWidth);    //top-right
                        gr.FillRectangle(_wallsPen.Brush, renderSizeX - cellWallWidth, 0, cellWallWidth, cellWallWidth);    //bottom-left
                        gr.FillRectangle(_wallsPen.Brush, renderSizeX - cellWallWidth, renderSizeY - cellWallWidth, cellWallWidth, cellWallWidth);  //bottom-right
                    }

                    //I draw every second wall (testing proved it to be 2× faster) and then fill the edges
                    //  ☒☒☒☒☒☒☒☒☒
                    //  ☒☐☒☐☒☐☒☐☒
                    //  ☒☒☐☒☐☒☐☒☒
                    //  ☒☐☒☐☒☐☒☐☒
                    //  ☒☒☐☒☐☒☐☒☒
                    //  ☒☐☒☐☒☐☒☐☒

                    if (_nodeCountX > 2 || _nodeCountY > 2)
                    {
                        for (int column = 1; column < _nodeCountX; column++)
                        {
                            for (int row = 1; row < _nodeCountY; row += 2)
                            {
                                nodes[column, (column % 2 == 0) ? row - 1 : row].DrawWall(gr, _wallsPen);
                            }
                        }
                    }
                    //fill the horizontal edges
                    for (int column = 0; column < _nodeCountX; column++)
                    {
                        nodes[column, 0].DrawWall(gr, _wallsPen);
                        nodes[column, _nodeCountY - 1].DrawWall(gr, _wallsPen);
                    }
                    //fill the vertical edges
                    for (int row = 0; row < _nodeCountY; row++)
                    {
                        nodes[0, row].DrawWall(gr, _wallsPen);
                        nodes[_nodeCountX - 1, row].DrawWall(gr, _wallsPen);
                    }
                }
            }

            if (fill == true)
            {   //overlay the cell image onto a solid color one and center it
                Bitmap backBmp = new Bitmap(canvasWidth, canvasHeight);
                using (Graphics gr = Graphics.FromImage(backBmp))
                {
                    gr.FillRectangle(_backgroundPen.Brush, 0, 0, canvasWidth, canvasHeight);
                    gr.DrawImage(bmp, ((canvasWidth - renderSizeX) / 2), ((canvasHeight - renderSizeY) / 2));
                }
                bmp = backBmp;
            }
            return bmp;
        }

        public Node startNode, endNode;
        public List<Node> path = new List<Node>();

        public bool Dijkstra()
        {
            //4TESTING↓
            startNode = nodes[_nodeCountX / 2, _nodeCountY / 2];
            endNode = nodes[_nodeCountX - 1, 0];

            if (startNode == null || endNode == null)
                return false;

            if (nodes == null)
                return false;

            int edgeLength = 1;
            List<Tuple<int, Node>> frontier = new List<Tuple<int, Node>>();

            bool[,] frontierWasHere = new bool[_nodeCountX, _nodeCountY];
            int[,] distanceToNode = new int[_nodeCountX, _nodeCountY];
            //4TESTING↓ ?
            Node[,] WhereDidIComeFrom = new Node[_nodeCountX, _nodeCountY];

            //add the starting node
            frontier.Add(new Tuple<int, Node>(0, startNode));

            //4TESTING↓
            foreach (Node node in nodes)
                node.Root = null;

            //try to find distance from the startnode for all reachable nodes
            while (frontier[0].Item2 != endNode)   /*(frontier.Count > 0)*/
            {
                int currentNodeDistance = frontier[0].Item1;
                Node currentNode = frontier[0].Item2;
                frontier.RemoveAt(0);

                frontierWasHere[currentNode.X, currentNode.Y] = true;
                for (int i = 0; i < 4; i++)
                {
                    Node nodeToVisit = currentNode.Neighbours[i];
                    if (nodeToVisit != null && !frontierWasHere[nodeToVisit.X, nodeToVisit.Y])  //!ADD check for path length; if it's smaller -> rewrite
                    {
                        frontier.Add(new Tuple<int, Node>(currentNodeDistance + edgeLength, nodeToVisit));    //add the node for further exploration
                        frontierWasHere[nodeToVisit.X, nodeToVisit.Y] = true;       //mark it as frontierWasHere so they do not duplicate in the frontier

                        distanceToNode[nodeToVisit.X, nodeToVisit.Y] = currentNodeDistance + edgeLength;
                        WhereDidIComeFrom[nodeToVisit.X, nodeToVisit.Y] = currentNode;
                        nodeToVisit.Root = currentNode;
                    }
                }

                frontier.Sort((t1, t2) => t1.Item1.CompareTo(t2.Item1));    //try it from the closest nodes first; unnecessary for this square maze

                //4TESTING↓
                for (int i = 0; i < frontier.Count; i++)
                {
                    Console.Write(frontier[i].Item2.ToString() + "|d:" + frontier[i].Item1 + " \t");
                }
                Console.WriteLine();
            }

            _rootPenHolder = new Tuple<Color, Color, float>(Color.Red, Color.Black, 8);
            //clear and write the backtracked shortest path
            path.Clear();
            path.Add(endNode);
            Node backTrackNode = endNode;
            while (backTrackNode != startNode && backTrackNode != null)
            {
                path.Add(WhereDidIComeFrom[backTrackNode.X, backTrackNode.Y]);
                backTrackNode = WhereDidIComeFrom[backTrackNode.X, backTrackNode.Y];
            }

            return true;
        }

        public Bitmap RenderPath(int canvasWidth, int canvasHeight, bool square = true)
        {
            Console.WriteLine("PATH" + path.Count);
            //4TESTING↓
            for (int i = 0; i < path.Count; i++)        //real length is Count-1
            {
                if (path[i] != null)
                    Console.Write(path[i].ToString() + " \t");
            }
            Console.WriteLine();

            return null;
        }
    }

    //Percent = (column * 100 / (_nodeCountX - 1));    //https://designforge.wordpress.com/2008/07/03/wpf-data-binding-to-a-simple-c-class/
}