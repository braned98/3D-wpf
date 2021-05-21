using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR24_2017_PZ3.Model
{
    public class Node
    {
        public Node Parent { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public Node()
        {

        }

        public Node(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
