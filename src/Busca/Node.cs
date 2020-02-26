using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algoritmos
{
    public struct Node
    {
        public int F; //custo real
        public int G; //custo altual + heuristico
        public int H; 
        public int X;
        public int Y;
        public int PX;  // antecessor
        public int PY;
    }
}
