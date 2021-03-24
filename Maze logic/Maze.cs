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

        public Node startNode, endNode;
        public Path DFSTree;

        [NonSerialized]
        public Path pathToRender;

        [NonSerialized]
        public Path GreedyPath = new Path();    //has to be assigned, because if pathfinding fails, there would be no path to be rendered!

        [NonSerialized]
        public Path DijkstraPath = new Path();

        [NonSerialized]
        public Path AStarPath = new Path();

        private int _nodeCountX, _nodeCountY;
        public int NodeCountX { get { return _nodeCountX; } }
        public int NodeCountY { get { return _nodeCountY; } }
        public int renderSizeX, renderSizeY;

        [NonSerialized]
        private Pen _wallsPen, _nodePen, _pointPen, _rootPen;

        //TESTING OLD
        private Tuple<Color, float> _rootRootNodePenHolder = new Tuple<Color, float>(Color.LightGoldenrodYellow, 0);

        #endregion Variables

        #region MazeGeneration

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
            ; ; ; //three lonely semicolons; they do absolutely nothing, just convey the emotions of a programmer
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
            DFSTree = new Path();
            DFSTree.exploredNodes = new Node[_nodeCountX, _nodeCountY];
            DFSTree.exploredNodes[startNode.X, startNode.Y] = startNode;                //startNode.Root = startNode;

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
                            //currentNode.Neighbours[Node.North].Root = currentNode.Neighbours[Node.North];
                            //currentNode.Neighbours[Node.North].Root = currentNode;
                            DFSTree.exploredNodes[currentNode.X, currentNode.Y - 1] = currentNode;

                            BackTheTrack.Push(currentNode.Neighbours[Node.North]);       //move to the northern node by pushing it onto the stack
                            break;

                        case Node.East:
                            currentNode.Neighbours[Node.East] = nodes[currentNode.X + 1, currentNode.Y];
                            Visited[currentNode.X + 1, currentNode.Y] = true;

                            currentNode.Neighbours[Node.East].Neighbours[Node.West] = currentNode;
                            //currentNode.Neighbours[Node.East].Root = currentNode.Neighbours[Node.East];
                            //currentNode.Neighbours[Node.East].Root = currentNode;
                            DFSTree.exploredNodes[currentNode.X + 1, currentNode.Y] = currentNode;

                            BackTheTrack.Push(currentNode.Neighbours[Node.East]);
                            break;

                        case Node.South:
                            currentNode.Neighbours[Node.South] = nodes[currentNode.X, currentNode.Y + 1];
                            Visited[currentNode.X, currentNode.Y + 1] = true;

                            currentNode.Neighbours[Node.South].Neighbours[Node.North] = currentNode;
                            //currentNode.Neighbours[Node.South].Root = currentNode.Neighbours[Node.South];
                            //currentNode.Neighbours[Node.South].Root = currentNode;
                            DFSTree.exploredNodes[currentNode.X, currentNode.Y + 1] = currentNode;

                            BackTheTrack.Push(currentNode.Neighbours[Node.South]);
                            break;

                        case Node.West:
                            currentNode.Neighbours[Node.West] = nodes[currentNode.X - 1, currentNode.Y];
                            Visited[currentNode.X - 1, currentNode.Y] = true;

                            currentNode.Neighbours[Node.West].Neighbours[Node.East] = currentNode;
                            //currentNode.Neighbours[Node.West].Root = currentNode.Neighbours[Node.West];
                            //currentNode.Neighbours[Node.West].Root = currentNode;
                            DFSTree.exploredNodes[currentNode.X - 1, currentNode.Y] = currentNode;

                            BackTheTrack.Push(currentNode.Neighbours[Node.West]);
                            break;
                    }
                    VisitedCellsCount++;
                }
                else { BackTheTrack.Pop(); }    //return one back, because there is nowhere to go
            }

            pathToRender = (Path)DFSTree.Clone();
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
            DFSTree = new Path(new Node[_nodeCountX, _nodeCountY]);
            pathToRender = (Path)DFSTree.Clone();
            //foreach (Node node in nodes)
            //    node.Root = node;

            return true;
        }

        /// <summary>
        /// Toggles node's neighbouring nodes
        /// </summary>
        /// <param name="node">The node to be edited</param>
        /// <param name="direction">the Node.direction of the neighbour to toggle</param>
        public void ToggleNeighbour(Node node, int direction)
        {
            switch (direction)
            {
                case Node.North:
                    if (node.Y > 0)
                    {
                        if (node.Neighbours[Node.North] == null)
                        {
                            node.Neighbours[Node.North] = nodes[node.X, node.Y - 1];
                            node.Neighbours[Node.North].Neighbours[Node.South] = node;
                        }
                        else
                        {
                            node.Neighbours[Node.North].Neighbours[Node.South] = null;
                            node.Neighbours[Node.North] = null;
                        }
                    }
                    break;

                case Node.East:
                    if (node.X < nodes.GetLength(0) - 1)
                    {
                        if (node.Neighbours[Node.East] == null)
                        {
                            node.Neighbours[Node.East] = nodes[node.X + 1, node.Y];
                            node.Neighbours[Node.East].Neighbours[Node.West] = node;
                        }
                        else
                        {
                            node.Neighbours[Node.East].Neighbours[Node.West] = null;
                            node.Neighbours[Node.East] = null;
                        }
                    }
                    break;

                case Node.South:
                    if (node.Y < nodes.GetLength(1) - 1)
                    {
                        if (node.Neighbours[Node.South] == null)
                        {
                            node.Neighbours[Node.South] = nodes[node.X, node.Y + 1];
                            node.Neighbours[Node.South].Neighbours[Node.North] = node;
                        }
                        else
                        {
                            node.Neighbours[Node.South].Neighbours[Node.North] = null;
                            node.Neighbours[Node.South] = null;
                        }
                    }
                    break;

                case Node.West:
                    if (node.X > 0)
                    {
                        if (node.Neighbours[Node.West] == null)
                        {
                            node.Neighbours[Node.West] = nodes[node.X - 1, node.Y];
                            node.Neighbours[Node.West].Neighbours[Node.East] = node;
                        }
                        else
                        {
                            node.Neighbours[Node.West].Neighbours[Node.East] = null;
                            node.Neighbours[Node.West] = null;
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        #endregion MazeGeneration

        #region Rendering

        /// <summary>
        /// Maze rendering function; works in real pixels
        /// </summary>
        /// <param name="canvasWidth">Deised image width in px</param>
        /// <param name="canvasHeight">Desired image height in px</param>
        ///<param name="style">Maze style class definition</param>
        ///<param name="isRendering">Enables measures in order to render more perfectly</param>
        /// <param name="fill">Specifies whether to fill background with solid color up to the specified size </param>
        /// <returns>Bitmap rendered maze of the specified size</returns>
        public Bitmap RenderMaze(int canvasWidth, int canvasHeight, Style style, bool isRendering = false, bool fill = false)
        {
            //this pen's width is needed for tight cellSize calculation; therefore, I cannot use cellSize for it's width
            int cellWallWidthX = (int)((canvasWidth / (5 * (_nodeCountX + 4))) * style.WallThickness / 100.0);
            int cellWallWidthY = (int)((canvasHeight / (5 * (_nodeCountY + 4))) * style.WallThickness / 100.0);

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
            int cellSizeX = (int)(canvasWidth - _wallsPen.Width) / _nodeCountX;
            int cellSizeY = (int)(canvasHeight - _wallsPen.Width) / _nodeCountY;

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

            //properly resize all the nodes; works with INT so as to not render blurry cells
            int wallOffset = (int)(_wallsPen.Width / 2);
            for (int column = 0; column < _nodeCountX; column++)
            {
                for (int row = 0; row < _nodeCountY; row++)
                {
                    nodes[column, row].SetBounds(column * cellSizeX + wallOffset, row * cellSizeY + wallOffset, cellSizeX, cellSizeY);
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
            renderSizeX = cellSizeX * _nodeCountX + (int)_wallsPen.Width;
            renderSizeY = cellSizeY * _nodeCountY + (int)_wallsPen.Width;
            Bitmap bmp = new Bitmap(renderSizeX, renderSizeY);

            //draw the nodes onto the bitmap
            using (Graphics gr = Graphics.FromImage(bmp))
            {
                //sets up graphics for smooth circles and fills the background with solid color
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                if (isRendering == true)
                    gr.CompositingQuality = CompositingQuality.HighQuality;
                else
                    gr.CompositingQuality = CompositingQuality.HighSpeed;

                gr.FillRectangle(_backgroundPen.Brush, 0, 0, renderSizeX, renderSizeY);

                //this for loop draws the single nodes onto the image (with automatic disabling of features when cells get too small)
                if (style.RenderNode == true && cellSize > 3)
                    foreach (Node node in nodes) { node.DrawBox(gr, _nodePen, (int)_wallsPen.Width / 2 + 1); }

                //if (style.RenderRoot == true && cellSize > 7)
                //    foreach (Node node in nodes) { node.DrawRootNode(gr, Utilities.ConvertColor(style.RootColorBegin), Utilities.ConvertColor(style.RootColorEnd), _rootPen.Width); }

                if (style.RenderPoint == true && cellSize > 3)
                    foreach (Node node in nodes) { node.DrawCentre(gr, _pointPen); }

                //if (style.RenderRootRootNode == true && cellSize > 7)
                //    foreach (Node node in nodes) { node.DrawRootRootNode(gr, _startNodePen); }

                //fill edges(&corners) with wallColor 1px offset from the centre in both X and Y
                gr.DrawRectangle(_wallsPen, -1, -1, renderSizeX + 1, renderSizeY + 1);

                if (isRendering == true)   //slower render, but render every cell (thus eliminating AntiAliasing glitches
                {
                    for (int column = 0; column < _nodeCountX; column++)
                    {
                        for (int row = 0; row < _nodeCountY; row++)
                        {
                            nodes[column, row].DrawWall(gr, _wallsPen);
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

        public Bitmap RenderNode(Bitmap originalBMP, Node node, Style style)
        {
            if (_wallsPen != null && _nodePen != null && _pointPen != null || _rootPen != null)
            {
                using (Graphics gr = Graphics.FromImage(originalBMP))
                {
                    gr.SmoothingMode = SmoothingMode.AntiAlias;
                    gr.CompositingQuality = CompositingQuality.HighSpeed;

                    int thickness = nodes[0, 0].Bounds.X + 1;
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
                                    nodes[node.X, node.Y - 1].DrawWall(gr, _wallsPen);
                                }
                                break;

                            case Node.East:
                                if (node.X < nodes.GetLength(0) - 1)
                                {
                                    nodes[node.X + 1, node.Y].DrawWall(gr, _wallsPen);
                                }
                                break;

                            case Node.South:
                                if (node.Y < nodes.GetLength(1) - 1)
                                {
                                    nodes[node.X, node.Y + 1].DrawWall(gr, _wallsPen);
                                }
                                break;

                            case Node.West:
                                if (node.X > 0)
                                {
                                    nodes[node.X - 1, node.Y].DrawWall(gr, _wallsPen);
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

        #endregion Rendering

        #region PathPlanning

        public bool GreedyBFS()
        {
            if (startNode == null || endNode == null || nodes == null)
                return false;

            int edgeLength = 1;
            List<Tuple<double, int, Node>> frontier = new List<Tuple<double, int, Node>>();         //holds sorting value, distance from start, actual Node

            bool pathFindErrored = false;
            bool[,] frontierWasHere = new bool[_nodeCountX, _nodeCountY];
            int[,] distanceToNode = new int[_nodeCountX, _nodeCountY];

            Node[,] WhereDidIComeFrom = new Node[_nodeCountX, _nodeCountY];
            GreedyPath = new Path();
            GreedyPath.startNode = startNode;
            GreedyPath.endNode = endNode;

            //add the starting node
            frontier.Add(new Tuple<double, int, Node>(0, 0, startNode));

            //try to find distance from the startnode for all reachable nodes
            while (frontier[0].Item3 != endNode)
            {
                int currentNodeDistance = frontier[0].Item2;
                Node currentNode = frontier[0].Item3;
                frontier.RemoveAt(0);

                frontierWasHere[currentNode.X, currentNode.Y] = true;
                for (int i = 0; i < 4; i++)
                {
                    Node nodeToVisit = currentNode.Neighbours[i];
                    if (nodeToVisit != null && !frontierWasHere[nodeToVisit.X, nodeToVisit.Y])  //!ADD check for path length; if it's smaller -> rewrite
                    {
                        int diffX = endNode.X - currentNode.X;
                        int diffY = endNode.Y - currentNode.Y;

                        double sortValue = Math.Sqrt(diffX * diffX + diffY * diffY);        //this line makes all the difference in algorithms

                        frontier.Add(new Tuple<double, int, Node>(sortValue, currentNodeDistance + edgeLength, nodeToVisit));    //add the node for further exploration
                        frontierWasHere[nodeToVisit.X, nodeToVisit.Y] = true;       //mark it as frontierWasHere so they do not duplicate in the frontier

                        distanceToNode[nodeToVisit.X, nodeToVisit.Y] = currentNodeDistance + edgeLength;
                        WhereDidIComeFrom[nodeToVisit.X, nodeToVisit.Y] = currentNode;
                    }
                }

                if (frontier.Count == 0)
                {
                    pathFindErrored = true;
                    GreedyPath.exploredNodes = WhereDidIComeFrom;
                    return false;
                }
                frontier.Sort((t1, t2) => t1.Item1.CompareTo(t2.Item1));    //try it from the closest nodes first (calculation of A* takes into account node distance + it's distance from the finish node)

                //4TESTING↓
                //for (int i = 0; i < frontier.Count; i++)
                //{
                //    Console.Write(frontier[i].Item2.ToString() + "|sort:" + frontier[i].Item3 + "|d:" + frontier[i].Item1 + " \t");
                //}
                //Console.WriteLine();
            }

            if (pathFindErrored == false)
            {
                //clear and write the backtracked shortest path
                GreedyPath.path = new List<Node>();

                GreedyPath.path.Add(endNode);
                Node backTrackNode = endNode;
                while (backTrackNode != startNode && backTrackNode != null)
                {
                    GreedyPath.path.Add(WhereDidIComeFrom[backTrackNode.X, backTrackNode.Y]);
                    backTrackNode = WhereDidIComeFrom[backTrackNode.X, backTrackNode.Y];
                }

                //add the spanning tree to the root visualized
                GreedyPath.exploredNodes = WhereDidIComeFrom;
            }
            return true;
        }

        public bool Dijkstra()
        {
            if (startNode == null || endNode == null || nodes == null)
                return false;

            int edgeLength = 1;
            List<Tuple<int, Node>> frontier = new List<Tuple<int, Node>>();         //holds distance from start (in here used as the sorting value), actual Node

            bool pathFindErrored = false;
            bool[,] frontierWasHere = new bool[_nodeCountX, _nodeCountY];
            int[,] distanceToNode = new int[_nodeCountX, _nodeCountY];

            Node[,] WhereDidIComeFrom = new Node[_nodeCountX, _nodeCountY];
            DijkstraPath = new Path();
            DijkstraPath.startNode = startNode;
            DijkstraPath.endNode = endNode;

            //add the starting node
            frontier.Add(new Tuple<int, Node>(0, startNode));

            //try to find distance from the startnode for all reachable nodes
            while (frontier[0].Item2 != endNode)
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
                    }
                }

                if (frontier.Count == 0)
                {
                    pathFindErrored = true;
                    DijkstraPath.exploredNodes = WhereDidIComeFrom;
                    return false;
                }
                frontier.Sort((t1, t2) => t1.Item1.CompareTo(t2.Item1));    //try it from the closest nodes first; almost unnecessary for this square maze

                //4TESTING↓
                //for (int i = 0; i < frontier.Count; i++)
                //{
                //    Console.Write(frontier[i].Item2.ToString() + "|d:" + frontier[i].Item1 + " \t");
                //}
                //Console.WriteLine();
            }

            if (pathFindErrored == false)
            {
                //clear and write the backtracked shortest path
                DijkstraPath.path = new List<Node>();

                DijkstraPath.path.Add(endNode);
                Node backTrackNode = endNode;
                while (backTrackNode != startNode && backTrackNode != null)
                {
                    DijkstraPath.path.Add(WhereDidIComeFrom[backTrackNode.X, backTrackNode.Y]);
                    backTrackNode = WhereDidIComeFrom[backTrackNode.X, backTrackNode.Y];
                }

                //add the spanning tree to the root visualized
                DijkstraPath.exploredNodes = WhereDidIComeFrom;
            }
            return true;
        }

        public bool AStar()
        {
            if (startNode == null || endNode == null || nodes == null)
                return false;

            int edgeLength = 1;
            List<Tuple<double, int, Node>> frontier = new List<Tuple<double, int, Node>>();         //holds sorting value, distance from start, actual Node

            bool pathFindErrored = false;
            bool[,] frontierWasHere = new bool[_nodeCountX, _nodeCountY];
            int[,] distanceToNode = new int[_nodeCountX, _nodeCountY];

            Node[,] WhereDidIComeFrom = new Node[_nodeCountX, _nodeCountY];
            AStarPath = new Path();
            AStarPath.startNode = startNode;
            AStarPath.endNode = endNode;

            //add the starting node
            frontier.Add(new Tuple<double, int, Node>(0, 0, startNode));

            //try to find distance from the startnode for all reachable nodes
            while (frontier[0].Item3 != endNode)
            {
                int currentNodeDistance = frontier[0].Item2;
                Node currentNode = frontier[0].Item3;
                frontier.RemoveAt(0);

                frontierWasHere[currentNode.X, currentNode.Y] = true;
                for (int i = 0; i < 4; i++)
                {
                    Node nodeToVisit = currentNode.Neighbours[i];
                    if (nodeToVisit != null && !frontierWasHere[nodeToVisit.X, nodeToVisit.Y])  //!ADD check for path length; if it's smaller -> rewrite
                    {
                        int diffX = endNode.X - currentNode.X;
                        int diffY = endNode.Y - currentNode.Y;
                        double sortValue = currentNodeDistance + edgeLength + Math.Sqrt(diffX * diffX + diffY * diffY);     //this line makes all the difference in algorithms

                        frontier.Add(new Tuple<double, int, Node>(sortValue, currentNodeDistance + edgeLength, nodeToVisit));    //add the node for further exploration
                        frontierWasHere[nodeToVisit.X, nodeToVisit.Y] = true;       //mark it as frontierWasHere so they do not duplicate in the frontier

                        distanceToNode[nodeToVisit.X, nodeToVisit.Y] = currentNodeDistance + edgeLength;
                        WhereDidIComeFrom[nodeToVisit.X, nodeToVisit.Y] = currentNode;
                    }
                }

                if (frontier.Count == 0)
                {
                    pathFindErrored = true;
                    AStarPath.exploredNodes = WhereDidIComeFrom;
                    return false;
                }
                frontier.Sort((t1, t2) => t1.Item1.CompareTo(t2.Item1));    //try it from the closest nodes first (calculation of A* takes into account node distance + it's distance from the finish node)

                //4TESTING↓
                //for (int i = 0; i < frontier.Count; i++)
                //{
                //    Console.Write(frontier[i].Item2.ToString() + "|sort:" + frontier[i].Item3 + "|d:" + frontier[i].Item1 + " \t");
                //}
                //Console.WriteLine();
            }

            if (pathFindErrored == false)
            {
                //clear and write the backtracked shortest path
                AStarPath.path = new List<Node>();

                AStarPath.path.Add(endNode);
                Node backTrackNode = endNode;
                while (backTrackNode != startNode && backTrackNode != null)
                {
                    AStarPath.path.Add(WhereDidIComeFrom[backTrackNode.X, backTrackNode.Y]);
                    backTrackNode = WhereDidIComeFrom[backTrackNode.X, backTrackNode.Y];
                }

                //add the spanning tree to the root visualized
                AStarPath.exploredNodes = WhereDidIComeFrom;
            }
            return true;
        }

        public Bitmap RenderPath(Bitmap originalBMP, Style style)
        {
            return RenderPath(originalBMP, style, pathToRender);
        }

        public Bitmap RenderPath(Bitmap originalBMP, Style style, Path currentPath)
        {
            pathToRender = currentPath;
            if (/*currentPath != null &&*/ nodes != null && (_wallsPen != null && _nodePen != null && _pointPen != null && _rootPen != null))
            {
                using (Graphics gr = Graphics.FromImage(originalBMP))
                {
                    gr.SmoothingMode = SmoothingMode.AntiAlias;
                    gr.CompositingQuality = CompositingQuality.HighSpeed;

                    //draw explored nodes (while checking if they are large enough)
                    if (currentPath.exploredNodes != null)
                    {
                        for (int column = 0; column < _nodeCountX; column++)
                        {
                            for (int row = 0; row < _nodeCountY; row++)
                            {
                                //assign the nodes's roots to the current path diagram
                                nodes[column, row].Root = currentPath.exploredNodes[column, row];

                                if (style.RenderRoot == true && (nodes[0, 0].Bounds.Width > 6 && nodes[0, 0].Bounds.Height > 6))
                                    nodes[column, row].DrawRootNode(gr, Utilities.ConvertColor(style.RootColorBegin), Utilities.ConvertColor(style.RootColorEnd), _rootPen.Width / 3, style.PathEndCap, style.PathEndCap);
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

                        //because root nodes are drawn - there is no need to draw the first root node
                        for (int i = 0; i < currentPath.path.Count - 1; i++)
                        {
                            Color startColor = Color.FromArgb(style.RootColorBegin.R + (int)(step_R * i), style.RootColorBegin.G + (int)(step_G * i), style.RootColorBegin.B + (int)(step_B * i));
                            Color endColor = Color.FromArgb(style.RootColorBegin.R + (int)(step_R * (i + 1)), style.RootColorBegin.G + (int)(step_G * (i + 1)), style.RootColorBegin.B + (int)(step_B * (i + 1)));

                            //reverse the drawing order -> we draw from the start
                            float thickness = (float)Math.Ceiling((_pointPen.Width / 2 - 1) * style.PathThickness / style.PointThickness);
                            currentPath.path[currentPath.path.Count - i - 2].DrawRootNode(gr, startColor, endColor, thickness, style.PathEndCap, style.PathEndCap);
                        }
                    }

                    if (startNode != null)
                    {
                        startNode.DrawBox(gr, new Pen(Utilities.ConvertColor(style.StartPointColor), _nodePen.Width), (int)_wallsPen.Width / 2);
                        startNode.DrawCentre(gr, new Pen(Utilities.ConvertColor(style.StartPointColor), (_pointPen.Width / 2) * style.PathThickness / style.PointThickness));
                    }

                    if (endNode != null)
                    {
                        endNode.DrawBox(gr, new Pen(Utilities.ConvertColor(style.EndPointColor), _nodePen.Width), (int)_wallsPen.Width / 2);
                        endNode.DrawCentre(gr, new Pen(Utilities.ConvertColor(style.EndPointColor), (_pointPen.Width / 2) * style.PathThickness / style.PointThickness));
                    }
                }
            }

            return originalBMP;
        }

        #endregion PathPlanning
    }

    //Percent = (column * 100 / (_nodeCountX - 1));    //https://designforge.wordpress.com/2008/07/03/wpf-data-binding-to-a-simple-c-class/
}