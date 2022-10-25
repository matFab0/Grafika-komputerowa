using System;
using System.Collections.Generic;
using System.Windows;


namespace Wypelnianie_i_oswietlanie_wielokatow
{
    public class myPolygon
    {

        public List<myPoint> vertices;
        public List<myLine> lines;
        public System.Drawing.Color color;
        public System.Drawing.Bitmap bitmap;
        public System.Drawing.Bitmap heightBitmap;
        public ColoringType typKolorowania = ColoringType.none;
        public int top;
        public int left;
        public int width;
        public int height;
        public int[,] z;
        public double speed_x;
        public double speed_y;
        public bool withHeightMap = false;
        public int minSpeed = 10;
        public int maxSpeed = 20;
        readonly Random rand = new Random();

        public myPolygon()
        {
            vertices = new List<myPoint>();
            lines = new List<myLine>();
            color = new System.Drawing.Color();
            top = 0;
            left = 0;
            width = 0;
            height = 0;
            speed_x = rand.Next(minSpeed,maxSpeed);
            speed_y = rand.Next(minSpeed,maxSpeed);
        }

        public myPolygon(List<myPoint> v, List<myLine> l)
        {
            vertices = v;
            lines = l;
            CreatePolygonBitmap();
        }

        public double CalculateDistancePointToLine(myLine line, Point point) //y=ax+b => Ax + By + C=0, gdzie A=-a B=1 C = -b
        {
            return Math.Abs(-line.a * point.X + point.Y - line.b) / Math.Sqrt(line.a * line.a + line.b * line.b);
        }

        public double CalculateDistancePointToPoint(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        static int ComparePointsByY(myPoint p1, myPoint p2)
        {
            if (p1.point.Y < p2.point.Y)
                return -1;
            if (p1.point.Y == p2.point.Y)
                return 0;
            return 1;
        }

        static int ComparePointsByX(myPoint p1, myPoint p2)
        {
            if (p1.point.X < p2.point.X)
                return -1;
            if (p1.point.X == p2.point.X)
                return 0;
            return 1;
        }

        public void CreatePolygonBitmap()
        {
            List<myPoint> pom = new List<myPoint>(vertices);
            pom.Sort(ComparePointsByX);
            width = (int)(pom[pom.Count - 1].point.X - pom[0].point.X);
            left = (int)pom[0].point.X;
            pom.Sort(ComparePointsByY);
            height = (int)(pom[pom.Count - 1].point.Y - pom[0].point.Y);
            top = (int)pom[0].point.Y;
            if (bitmap == null)
                bitmap = new System.Drawing.Bitmap(width,height);
            if (heightBitmap == null)
                heightBitmap = new System.Drawing.Bitmap(width, height);
        }

        public void CreateZTable()
        {
            z = new int[heightBitmap.Width,heightBitmap.Height];
            BmpPixelSnoop snoop = new BmpPixelSnoop(heightBitmap);
            for(int i=0;i<heightBitmap.Width;i++)
                for(int j=0;j<heightBitmap.Height;j++)
                {
                    z[i, j] = snoop.GetPixel(i, j).R;
                }
            snoop.Dispose();
        }

        public void DrawPolygon(ref BmpPixelSnoop snoop)
        {
            if (vertices.Count > 1)
            {
                for (int i = 0; i < vertices.Count; i++)
                {
                    Point p = vertices[i].point;
                    vertices[i].DrawPoint(ref snoop);
                    
                } 
            }
        }

        public void BresenhamLine(int x1, int y1, int x2, int y2,ref BmpPixelSnoop dbm)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;

            int d, incr1, incr2;

            int sx = dx < 0 ? -1 : 1;
            int sy = dy < 0 ? -1 : 1;

            dx = Math.Abs(dx);
            dy = Math.Abs(dy);

            int x = x1;
            int y = y1;
            if (x > 0 && y > 0 && x < dbm.Width && y < dbm.Height)
                dbm.SetPixel(x, y, System.Drawing.Color.Black);

            if (dx > dy) //oś wiodąca OX
            {
                incr1 = 2 * (dy - dx);
                incr2 = 2 * dy;
                d = incr2 - dx;

                while (x != x2)
                {
                    if (d >= 0)
                    {
                        x += sx;
                        y += sy;
                        d += incr1;
                    }
                    else
                    {
                        d += incr2;
                        x += sx;
                    }
                    if (x > 0 && y > 0 && x < dbm.Width && y < dbm.Height)
                        dbm.SetPixel(x, y, System.Drawing.Color.Black);
                }
            }
            else //oś wiodąca OY
            {
                incr1 = 2 * (dx - dy);
                incr2 = 2 * dx;
                d = incr2 - dy;

                while (y != y2)
                {
                    if (d >= 0)
                    {
                        x += sx;
                        y += sy;
                        d += incr1;
                    }
                    else
                    {
                        d += incr2;
                        y += sy;
                    }
                    if (x > 0 && y > 0 && x < dbm.Width && y < dbm.Height)
                        dbm.SetPixel(x, y, System.Drawing.Color.Black);
                }
            }
        }
    }
}
