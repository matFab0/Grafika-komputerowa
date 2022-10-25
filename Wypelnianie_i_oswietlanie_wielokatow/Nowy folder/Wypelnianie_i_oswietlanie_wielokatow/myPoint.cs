using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;

namespace Wypelnianie_i_oswietlanie_wielokatow
{
    public class myPoint
    {
        public Point point;
        public System.Drawing.Color color;

        public myPoint()
        {
            point = new Point();
            color = new System.Drawing.Color();
        }

        public myPoint(Point p, System.Drawing.Color c)
        {
            point = p;
            color = c;
        }
        public void DrawPoint(ref BmpPixelSnoop snoop)
        {
            for(int i=0;i<6;i++)
                for(int j=0;j<6;j++)
                {
                    if ((int)point.X - 3 + i > 0 && (int)point.Y - 3 + j > 0 && (int)point.X - 3 + i < snoop.Width && (int)point.Y - 3 + j < snoop.Height)
                        snoop.SetPixel((int)point.X - 3 + i, (int)point.Y - 3 + j, System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B));
                }    
        }
    }
}
