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
            if(path != null)
                path.Clear();
            if (exploredNodes != null)
                exploredNodes = new Node[exploredNodes.GetLength(0), exploredNodes.GetLength(1)];
        }
    }
}