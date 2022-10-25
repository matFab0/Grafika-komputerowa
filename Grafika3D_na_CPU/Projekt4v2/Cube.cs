using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Projekt4v2
{
    class Cube // (0,0,0) - punkt startowy
    {
        double a;
        public int total;
        public Vector4[,,] globe;
        public List<Triangle> triangles = new List<Triangle>();
        public Cube()
        {
            a = 0;
            total = 0;
        }

        public Cube(int _a, int _total)
        {
            a = _a;
            total = _total;
            globe = new Vector4[6,total + 1, total + 1];
        }

        public void Triangulate()
        {
            
            double d = a / total;
            double x = 0;
            double y = 0;
            double z = 0;
            for(int i=0;i<6;i++)
            {
                for(int j=0;j<total+1;j++)
                {
                    for(int k=0;k<total+1;k++)
                    {
                        if(i<2)
                        {
                            x = k;
                            y = j;
                            z = i == 0 ? a : 0;
                        }
                        else if(i<4)
                        {
                            x = k;
                            z = j;
                            y = i == 2 ? 0 : a;
                        }
                        else
                        {
                            z = k;
                            y = j;
                            x = i == 4 ? 0 : a;
                        }
                        globe[i,j, k] = new Vector4((float)x, (float)y, (float)z, 1);
                    }  
                }  
            }      
        }
    }
}
