using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Edytor_Graficzny
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<myPolygon> polygons = new List<myPolygon>();
        List<myEllipse> circles = new List<myEllipse>();
        List<Point> points = new List<Point>();
        List<myLine> lines = new List<myLine>();

        Painting rysowanie = Painting.none;
        bool przesuwanie = false;
        bool found = false;

        double old_mouse_dx = 0;
        double old_mouse_dy = 0;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void polygonButton_Click(object sender, RoutedEventArgs e)
        {
            if(rysowanie == Painting.none)
            {
                rysowanie = Painting.polygon;
                polygonButton.Content = "Rysuję...";
            }
        }
        private void circleButton_Click(object sender, RoutedEventArgs e)
        {
            if (rysowanie == Painting.none)
            {
                rysowanie = Painting.circle;
                circleButton.Content = "Rysuję...";
            }
        }
        private void pointButton_Click(object sender, RoutedEventArgs e)
        {
            przesuwanie = true;
            pointButton.Content = "Przesuwanie...";
        }
        myLine CalculateLine(Point p1,Point p2) // obliczam wzór linii: y=ax+b na podstawie dwóch punktów
        {
            double a = (p1.Y - p2.Y) / (p1.X - p2.X);
            double b = p1.Y - (a * p1.X);

            return new myLine(a, b);
        }
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int n = 0; //liczba wierzchołków dla wielokąta
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    if(rysowanie!=Painting.none)
                    {
                        Point position = e.GetPosition(this);
                        double px = position.X;
                        double py = position.Y;
                        points.Add(position);

                        Ellipse ellipse = new Ellipse();
                        ellipse.Fill = Brushes.Blue;
                        ellipse.Width = 10;
                        ellipse.Height = 10;
                        
                        Canvas.SetLeft(ellipse, px - 5);
                        Canvas.SetTop(ellipse, py - 5);
                        
                        n = points.Count;
                        if(n==1)
                            canvas.Children.Add(ellipse);
                        else if (n >= 2)
                        {
                            switch(rysowanie)
                            {
                                case Painting.polygon:
                                    BresenhamLine((int)points[n - 2].X, (int)points[n - 2].Y, (int)points[n - 1].X, (int)points[n - 1].Y);
                                    lines.Add(CalculateLine(points[n - 2], points[n - 1]));
                                    canvas.Children.Add(ellipse);
                                    break;
                                case Painting.circle:
                                    myEllipse circle = new myEllipse();
                                    circle.o = points[0];
                                    circle.el_point = ellipse;
                                    circle.r = Math.Sqrt(Math.Pow(points[n - 2].X-points[n - 1].X,2)+ Math.Pow(points[n - 2].Y - points[n - 1].Y, 2));
                                    ellipse = new Ellipse();
                                    ellipse.Width = ellipse.Height = 2 * circle.r;
                                    ellipse.Stroke = Brushes.Black;
                                    Canvas.SetLeft(ellipse, circle.o.X - circle.r);
                                    Canvas.SetTop(ellipse, circle.o.Y - circle.r);
                                    circle.el = ellipse;
                                    canvas.Children.Add(circle.el);
                                    circles.Add(circle);
                                    circleButton.Content = "Rysuj koło";
                                    rysowanie = Painting.none;
                                    points.Clear();
                                    break;
                            }
                        }
                    }
                    /*
                    if(przesuwanie)
                    {
                        Point position = e.GetPosition(this);
                        double px = position.X;
                        double py = position.Y;
                        myEllipse[] myEllipses = new myEllipse[circles.Count];
                        myEllipses = circles.ToArray();
                        for(int i=0;i<myEllipses.Length;i++)
                        {
                            if((myEllipses[i].o.X+5<=px || myEllipses[i].o.X - 5 >= px)&&(myEllipses[i].o.Y + 5 >= py || myEllipses[i].o.Y - 5 <= py))
                            {
                                found = true;
                                myEllipses[i].el.Fill = Brushes.Gray;
                                canvas.Children.Add(myEllipses[i].el);
                                break;
                            }
                        }
                        old_mouse_dx = px;
                        old_mouse_dy = py;
                    }
                    */
                    break;
                case MouseButton.Right:
                    n = points.Count;
                    if (n >= 3)
                    {
                        BresenhamLine((int)points[n - 1].X, (int)points[n - 1].Y, (int)points[0].X, (int)points[0].Y);
                        lines.Add(CalculateLine(points[n - 1], points[0]));
                        myPolygon polygon = new myPolygon();
                        polygon.vertices = points;
                        polygon.lines = lines;
                        polygons.Add(polygon);
                        points.Clear();
                        lines.Clear();
                        rysowanie = Painting.none;
                        polygonButton.Content = "Rysuj wielokąt";
                    }
                    break;
            }
        }
        void SetPixel(int x,int y)
        {
            Rectangle pixel2 = new Rectangle();
            pixel2.Fill = Brushes.Black;
            pixel2.Width = 4;
            pixel2.Height = 4;
            Canvas.SetLeft(pixel2, x - 2);
            Canvas.SetTop(pixel2, y - 2);
            canvas.Children.Add(pixel2);
        }
        void BresenhamLine(int x1, int y1, int x2, int y2)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;

            int d,incr1,incr2;

            int sx = dx < 0 ? -1 : 1;
            int sy = dy < 0 ? -1 : 1;

            dx = Math.Abs(dx);
            dy = Math.Abs(dy);

            int x = x1;
            int y = y1;

            SetPixel(x, y);

            if(dx>dy) //oś wiodąca OX
            {
                incr1 = 2 * (dy - dx);
                incr2 = 2 * dy;
                d = incr2 - dx;

                while(x!=x2)
                {
                    if(d>=0)
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
                    SetPixel(x, y);
                }
            }
            else //oś wiodąca OY
            {
                incr1 = 2 * (dx - dy);
                incr2 = 2 * dx;
                d = incr2 - dy;

                while(y!=y2)
                {
                    if(d>=0)
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
                    SetPixel(x, y);
                }
            }
        }
        
        
        // raised when the mouse pointer moves.
        // Expands the dimensions of an Ellipse when the mouse moves.
        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            /*
            // Get the x and y coordinates of the mouse pointer.
            System.Windows.Point position = e.GetPosition(this);
            double pX = position.X;
            double pY = position.Y;

            // Sets the Height/Width of the circle to the mouse coordinates.
            ellipse.Width = pX;
            ellipse.Height = pY;
            */
            if (e.LeftButton == MouseButtonState.Pressed && przesuwanie)
            {
                Point position = e.GetPosition(this);
                double px = position.X;
                double py = position.Y;
                myEllipse[] myEllipses = new myEllipse[circles.Count];
                myEllipses = circles.ToArray();
                for (int i = 0; i < myEllipses.Length; i++)
                {
                    if (position==myEllipses[i].o)//(myEllipses[i].o.X + 5 <= px || myEllipses[i].o.X - 5 >= px) && (myEllipses[i].o.Y + 5 >= py || myEllipses[i].o.Y - 5 <= py))
                    {
                        Point p = new Point(px - old_mouse_dx, py - old_mouse_dy);
                        myEllipses[i].o = p;
                        Canvas.SetLeft(myEllipses[i].el, px - myEllipses[i].r);
                        Canvas.SetTop(myEllipses[i].el, py - myEllipses[i].r);
                        Canvas.SetLeft(myEllipses[i].el, px - myEllipses[i].r);
                        Canvas.SetTop(myEllipses[i].el, py - myEllipses[i].r);
                    }
                }
                old_mouse_dx = px;
                old_mouse_dy = py;
                circles = myEllipses.ToList<myEllipse>();
            }
        }
        
    }
}
