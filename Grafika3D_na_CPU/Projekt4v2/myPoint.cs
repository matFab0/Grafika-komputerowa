using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Projekt4v2
{
    public class myPoint
    {
        public Point point;
        public System.Drawing.Color color;
        public double z;
        public myPoint()
        {
            point = new Point();
            color = new System.Drawing.Color();
        }

        public myPoint(Point p, System.Drawing.Color c, double z)
        {
            point = p;
            color = c;
            this.z = z;
        }  
    }
}
