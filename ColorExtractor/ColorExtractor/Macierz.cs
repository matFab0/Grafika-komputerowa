using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorExtractor
{
    class Macierz //klasa imitująca macierz, która ma tylko funkcjonalności potzrebne do projektu
    {
        int n; //kolumny
        int m; //wiersze
        public double[,] tab;

        public Macierz()
        {
            n = 0;
            m = 0;
            tab = new double[n, m];
        }

        public Macierz(int n,int m)
        {
            this.n = n;
            this.m = m;
            tab = new double[n, m];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                    tab[i, j] = 0;
        }

        public double det3x3() //wskaźnik macierzy 3x3
        {
            if (n == 3 && m == 3)
            {
                return tab[0, 0] * tab[1, 1] * tab[2, 2] + tab[1, 0] * tab[2, 1] * tab[0, 2] + tab[2, 0] * tab[0, 1] * tab[1, 2] - tab[0, 0] * tab[2, 1] * tab[1, 2] - tab[2, 0] * tab[1, 1] * tab[0, 2] - tab[1, 0] * tab[0, 1] * tab[2, 2];
            }
            else return 0;
        }

        public Macierz Inverse3x3() //odwrócona macierz 3x3
        {
            double a = det3x3();
            Macierz pom = new Macierz(3, 3);
            if (a == 0)
                throw new IrreversibleMatrixException();
            else
            {
                pom.tab[0, 0] = tab[1, 1] * tab[2, 2] - tab[1, 2] * tab[2, 1];
                pom.tab[0, 1] = tab[0, 2] * tab[2, 1] - tab[0, 1] * tab[2, 2];
                pom.tab[0, 2] = tab[0, 1] * tab[1, 2] - tab[0, 2] * tab[1, 1];
                pom.tab[1, 0] = tab[1, 2] * tab[2, 0] - tab[1, 0] * tab[2, 2];
                pom.tab[1, 1] = tab[0, 0] * tab[2, 2] - tab[0, 2] * tab[2, 0];
                pom.tab[1, 2] = tab[0, 2] * tab[1, 0] - tab[0, 0] * tab[1, 2];
                pom.tab[2, 0] = tab[1, 0] * tab[2, 1] - tab[1, 1] * tab[2, 0];
                pom.tab[2, 1] = tab[0, 1] * tab[2, 0] - tab[0, 0] * tab[2, 1];
                pom.tab[2, 2] = tab[0, 0] * tab[1, 1] - tab[0, 1] * tab[1, 0];

                pom.Multiply(1 / a);
            }
            return pom;
        }

        public void Multiply(double a)
        {
            for(int i=0;i<n;i++)
                for(int j=0;j<m;j++)
                {
                    tab[i, j] = a * tab[i, j];
                }
        }

        public Macierz Multiply1x3(Macierz m1, Macierz m2) //mnoży macierze [3,3] i [1,3] i tylko takie
        {
            Macierz pom = new Macierz(m1.n, m1.m);

            for (int k = 0; k < m2.m; k++)
            {
                for (int j = 0; j < m1.m; j++)
                {
                    pom.tab[0, k] += m1.tab[0, j] * m2.tab[k, j];
                }
            }
            return pom;
        }

        public void Power(double a)
        {
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                {
                    tab[i, j] = Math.Pow(tab[i, j],a);
                }
        }
    }
}
