using System;
using System.Collections.Generic;

namespace Mazeinator
{
    [Serializable]
    internal class Path
    {
        public Node startNode, endNode;
        public List<Node> path;

        public Node[,] exploredNodes;

        public int PathLength { get { return (path != null) ? (path.Count - 1) : 0; } }
        public int ExploredCount { get { return CountNonNullNodes(); } }

        public Path()
        {
            this.startNode = null;
            this.endNode = null;
            this.path = null;
            this.exploredNodes = null;
        }

        public Path(Node[,] nodes)
        {
            this.startNode = null;
            this.endNode = null;
            this.path = null;
            this.exploredNodes = nodes;
        }

        public Path(Node startNode, Node endNode, List<Node> path, Node[,] nodes)
        {
            this.startNode = startNode;
            this.endNode = endNode;
            this.path = path;
            this.exploredNodes = nodes;
        }

        public void Clear()
        {
            startNode = null;
            endNode = null;
            if (path != null)
                path.Clear();
            if (exploredNodes != null)
                exploredNodes = new Node[exploredNodes.GetLength(0), exploredNodes.GetLength(1)];
        }

        private int CountNonNullNodes()
        {
            if (exploredNodes != null)
            {
                int count = 0;
                for (int column = 0; column < exploredNodes.GetLength(0); column++)
                {
                    for (int row = 0; row < exploredNodes.GetLength(1); row++)
                    {
                        if (exploredNodes[column, row] != null)
                            count++;
                    }
                }
                return count;
            }
            else
                return -+-+-+1; //this is funny, so it's going to stay here; functions the same way as "-1"
        }
    }
}