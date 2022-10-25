using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edytor_Graficzny
{
    class myLine //y = a*x+b
    {
        double a;
        double b;
        public myLine()
        {
            a = 0;
            b = 0;
        }

        public myLine(double a,double b)
        {
            this.a = a;
            this.b = b;
        }
    }
}
