
using System.Windows;

namespace Wypelnianie_i_oswietlanie_wielokatow
{
    public class myLine //y=ax+b
    {
        public double a;
        public double b;

        

        public myLine()
        {
            a = 0;
            b = 0;
        }

        public myLine(double _a, double _b)
        {
            this.a = _a;
            this.b = _b;
        }
        public void CalculateLine(Point p1, Point p2) // obliczam wzór linii: y=ax+b na podstawie dwóch punktów
        {
            a = (p1.Y - p2.Y) / (p1.X - p2.X);
            b = p2.Y - (a * p2.X);
        }
    }
}
