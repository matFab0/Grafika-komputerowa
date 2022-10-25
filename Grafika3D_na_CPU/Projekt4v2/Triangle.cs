using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Projekt4v2
{
    public class Triangle
    {
        public Vector3 p1;
        public Vector3 p2;
        public Vector3 p3;
        public Vector3 expectedNormal;
        public bool circle = false; //sprawdzam czy trójkąt jest z triangulacji sfery
        public System.Drawing.Color color;
        public Triangle()
        {
            p1 = new Vector3();
            p2 = new Vector3();
            p3 = new Vector3();
        }

        public Triangle(Vector3 _p1, Vector3 _p2, Vector3 _p3)
        {
            p1 = _p1;
            p2 = _p2;
            p3 = _p3;
            expectedNormal = Vector3.Cross(p2 - p1,p3 - p1);
            expectedNormal = Vector3.Normalize(expectedNormal); 
        }

        public double CalculateDistancePointToPoint(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
    }
}
