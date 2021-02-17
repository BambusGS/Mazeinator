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

        //Pen holders - hold color + user offset for thicker/slimmer lines
        private Tuple<Color, float, LineCap> _wallPenHolder = new Tuple<Color, float, LineCap>(Color.Black, 0, LineCap.Triangle);

        private Tuple<Color, float> _boxPenHolder = new Tuple<Color, float>(Color.LightGray, 0);
        private Tuple<Color, float> _pointPenHolder = new Tuple<Color, float>(Color.Yellow, 0);
        private Tuple<Color, float> _rootRootNodePenHolder = new Tuple<Color, float>(Color.LightGoldenrodYellow, 0);
        private Tuple<Color, float> _backGroundPenHolder = new Tuple<Color, float>(Color.SteelBlue, 1);
        private Tuple<Color, Color, float> _rootPenHolder = new Tuple<Color, Color, float>(Color.Blue, Color.Black, 5);

        public bool RenderWall = true;
        public bool RenderBox = true;
        public bool RenderCenter = true;
        public bool RenderRootRootNode = true;
        public bool RenderRoot = true;

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
        /// Maze rendering function
        /// </summary>
        /// <param name="canvasWidth">Deised image width in px</param>
        /// <param name="canvasHeight">Desired image height in px</param>
        /// <param name="square">Whether cells should be square</param>
        /// <param name="fill">Specifies whether to fill background with solid color up to specified parameters </param>
        /// <returns>Bitmap of the specified size</returns>
        public Bitmap RenderMaze(int canvasWidth, int canvasHeight, bool square = true, bool fill = false)
        {
            //this pen's width is needed for tight cellSize calculation; therefore, I cannot use cellSize for it's width
            int cellWallWidthX = (int)((canvasWidth) / ((_nodeCountX + 4) * (5 + _wallPenHolder.Item2)));
            int cellWallWidthY = (int)((canvasHeight) / ((_nodeCountY + 4) * (5 + _wallPenHolder.Item2)));

            //prevent the cell from dissapearing
            if (cellWallWidthX <= 1) cellWallWidthX = 1;
            if (cellWallWidthY <= 1) cellWallWidthY = 1;

            int cellWallWidth = (cellWallWidthX < cellWallWidthY) ? cellWallWidthX : cellWallWidthY;
            Pen _wallsPen = new Pen(_wallPenHolder.Item1, cellWallWidth)
            {
                StartCap = _wallPenHolder.Item3,
                EndCap = _wallPenHolder.Item3
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
            Pen _backgroundPen = new Pen(_backGroundPenHolder.Item1);

            //generate a large bitmap as a multiple of maximum node width/height; use of integer division as flooring
            renderSizeX = cellSizeX * _nodeCountX + (int)_wallsPen.Width;
            renderSizeY = cellSizeY * _nodeCountY + (int)_wallsPen.Width;
            Bitmap bmp = new Bitmap(renderSizeX, renderSizeY);

            //draw the notes onto the bitmap
            using (Graphics gr = Graphics.FromImage(bmp))
            {
                //sets up graphics for smooth circles and fills the background with solid color
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                gr.CompositingQuality = CompositingQuality.HighSpeed;
                gr.FillRectangle(_backgroundPen.Brush, 0, 0, renderSizeX, renderSizeY);
                //fill corners
                {
                    gr.FillRectangle(_wallsPen.Brush, 0, 0, cellWallWidth, cellWallWidth);  //top-left
                    gr.FillRectangle(_wallsPen.Brush, 0, renderSizeY - cellWallWidth, cellWallWidth, cellWallWidth);    //top-right
                    gr.FillRectangle(_wallsPen.Brush, renderSizeX - cellWallWidth, 0, cellWallWidth, cellWallWidth);    //bottom-left
                    gr.FillRectangle(_wallsPen.Brush, renderSizeX - cellWallWidth, renderSizeY - cellWallWidth, cellWallWidth, cellWallWidth);  //bottom-right
                }

                //this for loop draws the single nodes onto the image (with automatic disabling of features when cells get too small)
                if (RenderBox == true && cellSize > 3)
                    foreach (Node node in nodes) { node.DrawBox(gr, _boxPen, (int)_wallsPen.Width / 2 + 1); }

                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                if (RenderRoot == true && cellSize > 7)
                    foreach (Node node in nodes) { node.DrawRootNode(gr, _rootPenHolder); }

                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds);

                if (RenderCenter == true && cellSize > 3)
                    foreach (Node node in nodes) { node.DrawCentre(gr, _pointPen); }

                if (RenderRootRootNode == true && cellSize > 7)
                    foreach (Node node in nodes) { node.DrawRootRootNode(gr, _startNodePen); }

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
        public Node[] path = null;

        public bool Dijkstra()
        {
            //4TESTING↓
            startNode = nodes[0, 0];
            endNode = nodes[_nodeCountX - 1, _nodeCountY - 1];

            if (startNode == null || endNode == null)
                return false;

            if (nodes == null)
                return false;

            int edgeLength = 1;
            List<Tuple<int, Node>> frontier = new List<Tuple<int, Node>>();

            bool[,] visited = new bool[_nodeCountX, _nodeCountY];
            int[,] distanceToNode = new int[_nodeCountX, _nodeCountY];
            Node[,] WhereDidIComeFrom = new Node[_nodeCountX, _nodeCountY];

            //add the starting node
            frontier.Add(new Tuple<int, Node>(0, startNode));

            //try to find distance from the startnode for all reachable nodes
            while (frontier.Count > 0)
            {
                frontier.Sort((t1, t2) => t1.Item1.CompareTo(t2.Item1));    //try it from the closest nodes first

                //4TESTING↓
                for (int i = 0; i < frontier.Count; i++)
                {
                    Console.Write(frontier[i].Item2.ToString() + "|d:" + frontier[i].Item1 + " \t");
                }
                Console.WriteLine();

                int currentNodeDistance = frontier[0].Item1;
                Node currentNode = frontier[0].Item2;
                frontier.RemoveAt(0);

                visited[currentNode.X, currentNode.Y] = true;
                for (int i = 0; i < 4; i++)
                {
                    Node nodeToVisit = currentNode.Neighbours[i];
                    if (nodeToVisit != null && !visited[nodeToVisit.X, nodeToVisit.Y])
                    {
                        frontier.Add(new Tuple<int, Node>(currentNodeDistance + edgeLength, nodeToVisit));    //add the node for further exploration

                        distanceToNode[nodeToVisit.X, nodeToVisit.Y] = currentNodeDistance + edgeLength;
                        WhereDidIComeFrom[nodeToVisit.X, nodeToVisit.Y] = currentNode;
                    }
                }
            }



            //4TESTING↓
            for (int i = 0; i < path.Length; i++)
            {
                Console.Write(path[i].ToString() + " \t");
            }
            Console.WriteLine();

            return true;
        }

        public Bitmap RenderPath(int canvasWidth, int canvasHeight, bool square = true)
        {
            return null;
        }
    }

    //Percent = (column * 100 / (_nodeCountX - 1));    //https://designforge.wordpress.com/2008/07/03/wpf-data-binding-to-a-simple-c-class/
}