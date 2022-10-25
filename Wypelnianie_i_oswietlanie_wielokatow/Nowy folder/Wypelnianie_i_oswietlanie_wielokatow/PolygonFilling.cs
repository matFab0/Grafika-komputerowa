using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;


namespace Wypelnianie_i_oswietlanie_wielokatow
{
    public class Edge
    {
        public AETp p { get; set; }
        public int number { get; set; }
    }

    public class AETp
    {
        public int y_max { get; set; }
        public double x { get; set; }
        public double m { get; set; }

    }

    class myPair
    {
        public int index { get; set; }
        public myPoint mypoint { get; set; }
    }
    public class PolygonFilling
    {
        List<Edge> AET = new List<Edge>();

        static int ComparePointsByY(myPair p1, myPair p2)
        {
            if (p1.mypoint.point.Y < p2.mypoint.point.Y)
                return -1;
            if (p1.mypoint.point.Y == p2.mypoint.point.Y)
                return 0;
            return 1;
        }

        static int CompareEdgeByX(Edge e1, Edge e2)
        {
            if (e1.p.x < e2.p.x)
                return -1;
            if (e1.p.x == e2.p.x)
                return 0;
            return 1;
        }

        int[] CreateVerticesTable(List<myPoint> points)
        {
            List<myPair> pairs = new List<myPair>();
            for (int i = 0; i < points.Count; i++)
            {
                pairs.Add(new myPair()
                {
                    index = i,
                    mypoint = points[i]
                });
            }
            pairs.Sort(ComparePointsByY);
            int[] ind = new int[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                ind[i] = pairs[i].index;
            }
            return ind;
        }
        
        float Oswietlenie(float kd, float ks, float Il, float Io, Vector3 L, Vector3 N, int m) //I = kd*Il*Io*cos(kąt(N,L))+ks*Il*Io*(cos(kąt(V,R)))^m
        {
            Vector3 V = new Vector3(0, 0, 1);
            float cosNL = N.X * L.X + N.Y * L.Y + N.Z * L.Z;
            Vector3 R = 2 * cosNL * N - L;
            R = Vector3.Normalize(R);
            float cosVR = V.X * R.X + V.Y * R.Y + V.Z * R.Z;
            
            if (cosNL > 1)
                cosNL = 1;
            
            if (cosVR > 1)
                cosVR = 1;
            
            float I = kd * Il * Io * cosNL + ks * Il * Io * (float)Math.Pow(cosVR, m);
 
            return I;
        }

        System.Drawing.Color CalculateNewColor0to255(int r,int g,int b)
        {
            if (r > 255)
                r = 255;
            if (g > 255)
                g = 255;
            if (b > 255)
                b = 255;

            if (r < 0)
                r = 0;
            if (g < 0)
                g = 0;
            if (b < 0)
                b = 0;

            return System.Drawing.Color.FromArgb(r, g, b);
        }

        public void Fill(myPolygon polygon, ref BmpPixelSnoop snoop, float ks, float kd,float z,int m,Vector3 Il,Vector2 light)
        {
            AET.Clear();
            int[] ind = CreateVerticesTable(polygon.vertices);
            int ymin = (int)polygon.vertices[ind[0]].point.Y;
            int ymax = (int)polygon.vertices[ind[polygon.vertices.Count - 1]].point.Y;
            AETp p;
            Edge edge;
                       
            BmpPixelSnoop snoop2 = new BmpPixelSnoop(new System.Drawing.Bitmap(1, 1)); // wartość snoop2, gdyby polygon nie miał bitmapy(wtedy po porstu nie jest ta zmienna używana, bo nie ma kolorowania z użyciem bitmapy)
            BmpPixelSnoop snoop3 = new BmpPixelSnoop(new System.Drawing.Bitmap(1, 1));
            if (polygon.bitmap!=null)
                snoop2 = new BmpPixelSnoop(polygon.bitmap);
            if (polygon.heightBitmap != null)
                snoop3 = new BmpPixelSnoop(polygon.heightBitmap);

            for (int y = ymin + 1; y <= ymax; y++)
            {
                for (int k = 0; k < polygon.vertices.Count; k++)
                {
                    if (polygon.vertices[ind[k]].point.Y == y - 1)
                    {
                        int l = ind[k] - 1 < 0 ? polygon.vertices.Count - 1 : ind[k] - 1; //poprzedni wierzchołek
                        
                        if (polygon.vertices[l].point.Y > polygon.vertices[ind[k]].point.Y)
                        {
                            double pom = (polygon.vertices[ind[k]].point.X - polygon.vertices[l].point.X) / (polygon.vertices[ind[k]].point.Y - polygon.vertices[l].point.Y);
                            p = new AETp()
                            {
                                y_max = (int)polygon.vertices[l].point.Y,
                                x = polygon.vertices[ind[k]].point.X + pom,
                                m = pom
                            };
                            edge = new Edge()
                            {
                                p = p,
                                number = l
                            };
                            AET.Add(edge);
                        }    
                        else
                            AET.RemoveAll(h => h.number == l);

                        int g = ind[k] + 1 >= polygon.vertices.Count ? 0 : ind[k] + 1; //następny wierzchołek
                        

                        if (polygon.vertices[g].point.Y > polygon.vertices[ind[k]].point.Y)
                        {
                            double pom = (polygon.vertices[ind[k]].point.X - polygon.vertices[g].point.X) / (polygon.vertices[ind[k]].point.Y - polygon.vertices[g].point.Y);
                            
                            p = new AETp()
                            {
                                y_max = (int)polygon.vertices[g].point.Y,
                                x = polygon.vertices[ind[k]].point.X + pom,
                                m = pom
                            };
                            edge = new Edge()
                            {
                                p = p,
                                number = ind[k]
                            };
                            AET.Add(edge);
                        }
                        else
                            AET.RemoveAll(h => h.number == ind[k]);
                    }
                }
                AET.Sort(CompareEdgeByX);
                
                // do interpolacji
                System.Drawing.Color colorP = new System.Drawing.Color(); //kolor punktu na jednej krawędzi
                System.Drawing.Color colorQ = new System.Drawing.Color(); //kolor punktu na drugiej krawędzi
                
                if (AET.Count >= 2)
                {
                    for (int i = 0; i <= AET.Count / 2; i += 2) //koloruję między krawędziami 0-1, 2-3 itd.
                    {
                        if(polygon.typKolorowania==ColoringType.interpolation)
                        {
                            myPoint p1 = polygon.vertices[AET[i].number];
                            int l = AET[i].number + 1 >= polygon.vertices.Count ? 0 : AET[i].number + 1;
                            myPoint p2 = polygon.vertices[l];

                            myPoint p3 = polygon.vertices[AET[i+1].number];
                            l = AET[i+1].number + 1 >= polygon.vertices.Count ? 0 : AET[i+1].number + 1;
                            myPoint p4 = polygon.vertices[l];

                            Point P = new Point(AET[i].p.x, y);
                            Point Q = new Point(AET[i + 1].p.x,y);

                            double a = polygon.CalculateDistancePointToPoint(P, p1.point);
                            double e = polygon.CalculateDistancePointToPoint(P, p2.point);

                            double c = polygon.CalculateDistancePointToPoint(Q, p3.point);
                            double d = polygon.CalculateDistancePointToPoint(Q, p4.point);

                            int r = 0, g = 0, b = 0;

                            r = (int)((a * p2.color.R + e * p1.color.R) / (a + e));
                            g = (int)((a * p2.color.G + e * p1.color.G) / (a + e));
                            b = (int)((a * p2.color.B + e * p1.color.B) / (a + e));

                            colorP = CalculateNewColor0to255(r, g, b);

                            r = (int)((c * p4.color.R + d * p3.color.R) / (c + d));
                            g = (int)((c * p4.color.G + d * p3.color.G) / (c + d));
                            b = (int)((c * p4.color.B + d * p3.color.B) / (c + d));

                            colorQ = CalculateNewColor0to255(r, g, b);


                        }
                        for (int j = (int)AET[i].p.x; j < AET[i + 1].p.x; j++)
                        {
                            
                            if (j > 0 && y > 0 && j < snoop.Width && y < snoop.Height)
                            {
                                
                                Vector3 N = new Vector3(0, 0, 1);
                                System.Drawing.Color c = new System.Drawing.Color();
                                Vector3 L = new Vector3(light.X-j,light.Y-y,z);
                                switch(polygon.typKolorowania)
                                {
                                    case ColoringType.color:
                                        c = polygon.color;
                                        break;
                                    case ColoringType.interpolation:
                                        int r1 = (int)((Math.Abs(AET[i].p.x-j) * colorQ.R + Math.Abs(AET[i + 1].p.x-j) * colorP.R) / Math.Abs(AET[i + 1].p.x - AET[i].p.x));
                                        int g1 = (int)((Math.Abs(AET[i].p.x - j) * colorQ.G + Math.Abs(AET[i + 1].p.x - j) * colorP.G) / Math.Abs(AET[i + 1].p.x - AET[i].p.x));
                                        int b1 = (int)((Math.Abs(AET[i].p.x - j) * colorQ.B + Math.Abs(AET[i + 1].p.x - j) * colorP.B) / Math.Abs(AET[i + 1].p.x - AET[i].p.x));

                                        c = CalculateNewColor0to255(r1, g1, b1);
                                        
                                        break;
                                    case ColoringType.bitmap:
                                        if((j - polygon.left) >0 && (j - polygon.left) < polygon.width && (y - polygon.top)>0 && (y - polygon.top)<polygon.height)
                                            c = snoop2.GetPixel(j - polygon.left, y - polygon.top);
                                        if(polygon.withHeightMap)
                                        {
                                            // zmienne pomocnicze do obliczania wektorów z heightMapy
                                            int z1 = j - polygon.left + 1;
                                            int z2 = y - polygon.top;
                                            int z3 = j - polygon.left - 1;
                                            int z4 = j - polygon.left;
                                            int z5 = y - polygon.top - 1;
                                            int z6 = y - polygon.top + 1;
                                            
                                            // sprawdzam czy zmienne nie wychodzą poza granice bitmapy
                                            if (z1 > 0 && z2 > 0 && z3 > 0 && z4 > 0 && z5 > 0 && z6 > 0 && z1 < polygon.width - 1  && z3 < polygon.width && z4 < polygon.width && z2 < polygon.height && z5 < polygon.height && z6 < polygon.height - 1)
                                            {
                                                N = new Vector3(polygon.z[z1, z2] - polygon.z[z3, z2], polygon.z[z4, z5] - polygon.z[z4, z6], 1);
                                                L = new Vector3(light.X - j, light.Y - y, z - polygon.z[z4, z2]);
                                                N = Vector3.Normalize(N);
                                            }
                                        }
                                            
                                        break;
                                }
                                    
                                L = Vector3.Normalize(L);
                                
                                float r = Oswietlenie(kd, ks,Il.X,(float)c.R/255,L,N,m);
                                float g = Oswietlenie(kd, ks, Il.Y,(float)c.G/255, L, N, m);
                                float b = Oswietlenie(kd, ks, Il.Z,(float)c.B/255, L, N, m);
                                  
                                c = CalculateNewColor0to255((int)(r * 255), (int)(g * 255), (int)(b * 255));
                                
                                snoop.SetPixel(j, y, c);
                            }
                                
                        }

                        AET[i].p.x += AET[i].p.m;
                        AET[i + 1].p.x += AET[i + 1].p.m;
                    }
                }
            }
            snoop2.Dispose();
            snoop3.Dispose();
        }
    }
}
