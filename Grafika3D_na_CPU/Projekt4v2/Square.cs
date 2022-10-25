using System.Collections.Generic;
using System.Numerics;

namespace Projekt4v2
{
    class Square
    {
        double a;
        public int total;
        public Vector4[,] globe;
        public List<Triangle> triangles = new List<Triangle>();
        public Square()
        {
            a = 0;
            total = 0;
        }

        public Square(int _a, int _total)
        {
            a = _a;
            total = _total;
            globe = new Vector4[total + 1, total + 1];
        }

        public void Triangulate()
        {
            double d = a / total;
            double x = 0;
            double y = 0;
            double z = 0;
           
                for (int j = 0; j < total + 1; j ++)
                {
                    for (int k = 0; k < total + 1; k ++)
                    {
                        x = j;
                        z = k;
                        globe[j, k] = new Vector4((float)x, (float)y, (float)z, 1);
                    }
                }
        }
    }
}

