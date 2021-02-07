using System;
using System.Collections.Generic;
using System.Drawing;   //Bitmap and Graphics

namespace Mazeinator
{
    [Serializable]
    internal class Maze
    {
        public Node[,] nodes = null;

        private int _nodeCountX, _nodeCountY;
        public int renderSizeX, renderSizeY;

        private Tuple<Color, int> _wallPenHolder = new Tuple<Color, int>(Color.Black, 10);
        private Tuple<Color, int> _boxPenHolder = new Tuple<Color, int>(Color.LightGray, 2);
        private Tuple<Color, int> _rootPenHolder = new Tuple<Color, int>(Color.Blue, 3);
        private Tuple<Color, int> _pointPenHolder = new Tuple<Color, int>(Color.Yellow, 8);
        private Tuple<Color, int> _startNodePenHolder = new Tuple<Color, int>(Color.LightBlue, 30);
        private Tuple<Color, int> _backGroundPenHolder = new Tuple<Color, int>(Color.SteelBlue, 1);

        [NonSerialized] //why save graphics or pens, just the values are important
        private Pen _wallsPen = null, _boxPen = null, _rootPen = null, _pointPen = null, _startNodePen = null, _backgroundPen = null;

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
        public void GenerateMaze(Node startNode = null)
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
        }

        /// <summary>
        /// Maze rendering function
        /// </summary>
        /// <param name="canvasWidth"></param>
        /// <param name="canvasHeight"></param>
        /// <param name="square">Whether cells should be square</param>
        /// <param name="fill">Specifies whether to fill background with solid color up to specified parameters </param>
        /// <returns>Bitmap of the specified size</returns>
        public Bitmap DisplayMaze(int canvasWidth, int canvasHeight, bool square = true, bool fill = false)
        {
            _wallsPen = new Pen(_wallPenHolder.Item1, _wallPenHolder.Item2);   //initialize all the pens that can be changed by serialization or by user
            _boxPen = new Pen(_boxPenHolder.Item1, _boxPenHolder.Item2);
            _rootPen = new Pen(_rootPenHolder.Item1, _rootPenHolder.Item2);
            _pointPen = new Pen(_pointPenHolder.Item1, _pointPenHolder.Item2);
            _startNodePen = new Pen(_startNodePenHolder.Item1, _startNodePenHolder.Item2);
            _backgroundPen = new Pen(_backGroundPenHolder.Item1);

            //calculate the needed cell size in the specific dimension + take into account the thickness of the walls
            int cellSizeX = (canvasWidth - (int)_wallsPen.Width) / _nodeCountX;
            int cellSizeY = (canvasHeight - (int)_wallsPen.Width) / _nodeCountY;

            //finds out the smaller cell size in order for the cell to be square
            if (square == true)
            {
                int cellSize = (cellSizeX < cellSizeY) ? cellSizeX : cellSizeY;
                cellSizeX = cellSize;
                cellSizeY = cellSize;
            }

            renderSizeX = cellSizeX * _nodeCountX + (int)_wallsPen.Width;
            renderSizeY = cellSizeY * _nodeCountY + (int)_wallsPen.Width;

            //properly resize all the nodes
            int wallOffset = (int)(_wallsPen.Width / 2.0);
            for (int column = 0; column < _nodeCountX; column++)
            {
                for (int row = 0; row < _nodeCountY; row++)
                {
                    nodes[column, row].SetBounds(column * cellSizeX + wallOffset, row * cellSizeY + wallOffset, cellSizeX, cellSizeY);
                }
            }

            //generate a large bitmap as a multiple of maximum node width/height; use of integer division as flooring
            Bitmap bmp = new Bitmap(renderSizeX, renderSizeY);

            //draw the notes onto the bitmap
            using (Graphics gr = Graphics.FromImage(bmp))
            {
                //sets up graphics for smooth circles and fills the background with solid color
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                gr.FillRectangle(_backgroundPen.Brush, 0, 0, renderSizeX, renderSizeY);

                //this for loop draws the single nodes onto the image
                for (int column = 0; column < _nodeCountX; column++)
                {
                    for (int row = 0; row < _nodeCountY; row++)
                    {
                        nodes[column, row].DrawWall(gr, _wallsPen, true);
                        nodes[column, row].DrawBox(gr, _boxPen, (int)_wallsPen.Width / 2 + 1);
                        nodes[column, row].DrawRootNode(gr, _rootPen);
                        nodes[column, row].DrawCentre(gr, _pointPen);
                        nodes[column, row].DrawStartNode(gr, _startNodePen);
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
    }

    //Percent = (column * 100 / (_nodeCountX - 1));    //https://designforge.wordpress.com/2008/07/03/wpf-data-binding-to-a-simple-c-class/
}