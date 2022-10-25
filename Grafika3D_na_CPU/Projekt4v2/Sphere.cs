using System;
using System.Collections.Generic;
using System.Numerics;

namespace Projekt4v2
{
    public class Sphere
    {
        int r;
        public int total;
        public Vector4[,] globe;
        public List<Triangle> triangles = new List<Triangle>();

        public Sphere()
        {
            r = 0;
            total = 0;
        }

        public Sphere(int _r, int _total, int W, int H)
        {
            r = _r;
            total = _total;
            globe = new Vector4[total + 1, total + 1];

        }

        public void Triangulate()
        {
            double d1 = Math.PI / total;
            double d2 = 2 * Math.PI / total;
            double lon = 0;
            double lat = 0;
            for (int i = 0; i < total + 1; i++)
            {
                for (int j = 0; j < total + 1; j++)
                {
                    double x = r * Math.Sin(lon) * Math.Cos(lat);
                    double y = r * Math.Cos(lon);
                    double z = r * Math.Sin(lon) * Math.Sin(lat);
                    lat += d2;
                    globe[i, j] = new Vector4((float)x, (float)y, (float)z, 1);
                }
                lon += d1;
            }
        }
    }
}
