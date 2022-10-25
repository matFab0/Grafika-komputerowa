using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Projekt4v2
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

        float Oswietlenie(float kd, float ks, float Il, float Io, List<Vector3> L,Vector3 V, Vector3 N, int m, float ka,List<double> distance) //I = kd*Il*Io*cos(kąt(N,L))+ks*Il*Io*(cos(kąt(V,R)))^m
        {
            //V = new Vector3(0, 0, 1);
            float pom = 0;
            for (int i = 0; i < L.Count; i++)
            {
                float cosNL = N.X * L[i].X + N.Y * L[i].Y + N.Z * L[i].Z;
                Vector3 R = 2 * cosNL * N - L[i];
                R = Vector3.Normalize(R);
                float cosVR = V.X * R.X + V.Y * R.Y + V.Z * R.Z;
                double If = 1 / (1 + 0.0014 * distance[i] + 0.000007 * distance[i] * distance[i]);

                if (cosNL > 1)
                    cosNL = 1;

                if (cosVR > 1)
                    cosVR = 1;
                pom += (kd * Il * Io * cosNL + ks * Il * Io * (float)Math.Pow(cosVR, m))*(float)If;
            }
            
            float I = ka * Io + pom;
           
            return I;
        }

        System.Drawing.Color CalculateNewColor0to255(int r, int g, int b)
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

        public void Fill(List<myPoint> vertices,Triangle triangle, ref BmpPixelSnoop snoop, float ks, float kd, int m, Vector3 Il, List<Vector3> light,System.Drawing.Color color,float ka,Shader shader,ref double[,] zBufor, Vector3 V)
        {
            AET.Clear();
            int[] ind = CreateVerticesTable(vertices);
            int ymin = (int)vertices[ind[0]].point.Y;
            int ymax = (int)vertices[ind[vertices.Count - 1]].point.Y;
            AETp p;
            Edge edge;

            System.Drawing.Color c = new System.Drawing.Color();
            List<Vector3> N = new List<Vector3>();
             
            if (shader == Shader.Flat)
            {
                int X = (int)((vertices[0].point.X + vertices[1].point.X + vertices[2].point.X) / 3);
                int Y = (int)((vertices[0].point.Y + vertices[1].point.Y + vertices[2].point.Y) / 3);
                int Z = (int)((vertices[0].z + vertices[1].z + vertices[2].z) / 3);
                Vector3 NN = triangle.expectedNormal;
            
                c = color;
                
                Vector3 pixelPoint = new Vector3(X, Y, Z);
                Vector3 pom = V - pixelPoint;
                pom = Vector3.Normalize(pom);

                List<Vector3> L = new List<Vector3>();
                List<double> distance = new List<double>();
                for(int i=0;i<light.Count;i++)
                {
                    L.Add(Vector3.Normalize(new Vector3(light[i].X - X, light[i].Y - Y, light[i].Z - Z)));
                    distance.Add(DistanceVector3Vector3(light[i],pixelPoint)) ;
                }

                
                float r = Oswietlenie(kd, ks, Il.X, (float)c.R / 255, L, pom , NN, m, ka, distance);
                float g = Oswietlenie(kd, ks, Il.Y, (float)c.G / 255, L, pom , NN, m, ka, distance);
                float b = Oswietlenie(kd, ks, Il.Z, (float)c.B / 255, L, pom , NN, m, ka, distance);

                c = CalculateNewColor0to255((int)(r * 255), (int)(g * 255), (int)(b * 255));
            }

            if (shader == Shader.Gourad || shader == Shader.Phong)
            {
                c = color;
                N = new List<Vector3>() {
                    Vector3.Normalize(triangle.p1),
                    Vector3.Normalize(triangle.p2),
                    Vector3.Normalize(triangle.p3)
                };
                if (!triangle.circle)
                {
                    N = new List<Vector3>() {
                        triangle.expectedNormal,
                        triangle.expectedNormal,
                        triangle.expectedNormal
                    };
                }
                if (shader == Shader.Gourad)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        List<Vector3> L1 = new List<Vector3>();
                        List<double> distance = new List<double>();
                        Vector3 pixelPoint = new Vector3((float)vertices[i].point.X, (float)vertices[i].point.Y, (float)vertices[i].z);

                        for (int j=0;j<light.Count;j++)
                        {
                            L1.Add(Vector3.Normalize(new Vector3(light[j].X - (float)vertices[i].point.X, light[j].Y - (float)vertices[i].point.Y, light[j].Z - (float)vertices[i].z)));
                            distance.Add(DistanceVector3Vector3(light[j],pixelPoint));
                        }

                        Vector3 pom = V - pixelPoint;
                        pom = Vector3.Normalize(pom);
                        float r4 = Oswietlenie(kd, ks, Il.X, (float)c.R / 255, L1, pom, N[i], m, ka, distance);
                        float g4 = Oswietlenie(kd, ks, Il.Y, (float)c.G / 255, L1, pom, N[i], m, ka, distance);
                        float b4 = Oswietlenie(kd, ks, Il.Z, (float)c.B / 255, L1, pom, N[i], m, ka, distance);

                        vertices[i].color = CalculateNewColor0to255((int)(r4 * 255), (int)(g4 * 255), (int)(b4 * 255));
                    }
                }
            }
            for (int y = ymin + 1; y <= ymax; y++)
            {
                for (int k = 0; k < vertices.Count; k++)
                {
                    if (vertices[ind[k]].point.Y == y - 1)
                    {
                        int l = ind[k] - 1 < 0 ? vertices.Count - 1 : ind[k] - 1; //poprzedni wierzchołek

                        if (vertices[l].point.Y > vertices[ind[k]].point.Y)
                        {
                            double pom = (vertices[ind[k]].point.X - vertices[l].point.X) / (vertices[ind[k]].point.Y - vertices[l].point.Y);
                            p = new AETp()
                            {
                                y_max = (int)vertices[l].point.Y,
                                x = vertices[ind[k]].point.X + pom,
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

                        int f = ind[k] + 1 >= vertices.Count ? 0 : ind[k] + 1; //następny wierzchołek


                        if (vertices[f].point.Y > vertices[ind[k]].point.Y)
                        {
                            double pom = (vertices[ind[k]].point.X - vertices[f].point.X) / (vertices[ind[k]].point.Y - vertices[f].point.Y);

                            p = new AETp()
                            {
                                y_max = (int)vertices[f].point.Y,
                                x = vertices[ind[k]].point.X + pom,
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

                // do interpolacji Gourada
                System.Drawing.Color colorP = new System.Drawing.Color(); //kolor punktu na jednej krawędzi
                System.Drawing.Color colorQ = new System.Drawing.Color(); //kolor punktu na drugiej krawędzi

                //do interpolacji Phonga
                Vector3 N1 = new Vector3();
                Vector3 N2 = new Vector3();

                //do interpolacji z-bufora
                int z1 = 0;
                int z2 = 0;

                if (AET.Count >= 2)
                {
                    
                    for (int i = 0; i <= AET.Count / 2; i += 2) //koloruję między krawędziami 0-1, 2-3 itd.
                    {
                            myPoint p1 = vertices[AET[i].number];
                           
                            int l = AET[i].number + 1 >= vertices.Count ? 0 : AET[i].number + 1;
                            myPoint p2 = vertices[l];
                            

                            myPoint p3 = vertices[AET[i + 1].number];
                            
                            int k = AET[i + 1].number + 1 >= vertices.Count ? 0 : AET[i + 1].number + 1;
                            
                            myPoint p4 = vertices[k];

                            Point P = new Point(AET[i].p.x, y);
                            Point Q = new Point(AET[i + 1].p.x, y);

                            double a = triangle.CalculateDistancePointToPoint(P, p1.point);
                            double e = triangle.CalculateDistancePointToPoint(P, p2.point);

                            double f = triangle.CalculateDistancePointToPoint(Q, p3.point);
                            double d = triangle.CalculateDistancePointToPoint(Q, p4.point);

                            z1 = (int)((a * p2.z + e * p1.z) / (a + e));
                            z2 = (int)((f * p4.z + d * p3.z) / (f + d));

                        if (shader == Shader.Gourad)
                            {
                               
                                int r = 0, g = 0, b = 0;

                                r = (int)((a * p2.color.R + e * p1.color.R) / (a + e));
                                g = (int)((a * p2.color.G + e * p1.color.G) / (a + e));
                                b = (int)((a * p2.color.B + e * p1.color.B) / (a + e));

                                colorP = CalculateNewColor0to255(r, g, b);

                                r = (int)((f * p4.color.R + d * p3.color.R) / (f + d));
                                g = (int)((f * p4.color.G + d * p3.color.G) / (f + d));
                                b = (int)((f * p4.color.B + d * p3.color.B) / (f + d));

                                colorQ = CalculateNewColor0to255(r, g, b);
                            }


                            if (shader == Shader.Phong)
                            {
                                Vector3 P1 = N[AET[i].number];
                                Vector3 P2 = N[l];
                                Vector3 P3 = N[AET[i+1].number];
                                Vector3 P4 = N[k];
                                
                                N1 = ((float)a * P2 + (float)e * P1) / (float)(a + e);
                                N1 = Vector3.Normalize(N1);
                                N2 = ((float)f * P4 + (float)d * P3) / (float)(f + d);
                                N2 = Vector3.Normalize(N2);

                                float r = 0, g = 0, b = 0;

                                r = (float)((a * P2.X + e * P1.X) / (a + e));
                                g = (float)((a * P2.Y + e * P1.Y) / (a + e));
                                b = (float)((a * P2.Z + e * P1.Z) / (a + e));

                                N1 = Vector3.Normalize(new Vector3(r, g, b));

                                r = (float)((f * P4.X + d * P3.X) / (f + d));
                                g = (float)((f * P4.Y + d * P3.Y) / (f + d));
                                b = (float)((f * P4.Z + d * P3.Z) / (f + d));

                                N2 = Vector3.Normalize(new Vector3(r, g, b));
                            }
                        
                        for (int j = (int)AET[i].p.x; j < AET[i + 1].p.x; j++)
                        {

                            if (j > 0 && y > 0 && j < snoop.Width && y < snoop.Height)
                            {

                                int z3 = (int)((Math.Abs(AET[i].p.x - j) * z2 + Math.Abs(AET[i + 1].p.x - j) * z1) / Math.Abs(AET[i + 1].p.x - AET[i].p.x));
                                
                                if (z3 >= zBufor[j, y])
                                {
                                    zBufor[j, y] = z3;

                                    if (shader != Shader.Flat)
                                    {
                                        
                                        c = color;
                                        if (shader == Shader.Gourad)
                                        {
                                            int r1 = (int)((Math.Abs(AET[i].p.x - j) * colorQ.R + Math.Abs(AET[i + 1].p.x - j) * colorP.R) / Math.Abs(AET[i + 1].p.x - AET[i].p.x));
                                            int g1 = (int)((Math.Abs(AET[i].p.x - j) * colorQ.G + Math.Abs(AET[i + 1].p.x - j) * colorP.G) / Math.Abs(AET[i + 1].p.x - AET[i].p.x));
                                            int b1 = (int)((Math.Abs(AET[i].p.x - j) * colorQ.B + Math.Abs(AET[i + 1].p.x - j) * colorP.B) / Math.Abs(AET[i + 1].p.x - AET[i].p.x));

                                            c = CalculateNewColor0to255(r1, g1, b1);
                                        }
                                        if (shader == Shader.Phong)
                                        {
                                            float r1 = (float)((Math.Abs(AET[i].p.x - j) * N2.X + Math.Abs(AET[i + 1].p.x - j) * N1.X) / Math.Abs(AET[i + 1].p.x - AET[i].p.x));
                                            float g1 = (float)((Math.Abs(AET[i].p.x - j) * N2.Y + Math.Abs(AET[i + 1].p.x - j) * N1.Y) / Math.Abs(AET[i + 1].p.x - AET[i].p.x));
                                            float b1 = (float)((Math.Abs(AET[i].p.x - j) * N2.Z + Math.Abs(AET[i + 1].p.x - j) * N1.Z) / Math.Abs(AET[i + 1].p.x - AET[i].p.x));


                                            Vector3 N3 = new Vector3(r1, g1, b1);
                                            N3 = Vector3.Normalize(N3);

                                            List<Vector3> L = new List<Vector3>();
                                            List<double> distance = new List<double>();
                                            Vector3 pixelPoint = new Vector3(j, y, z3);
                                            for (int t=0;t<light.Count;t++)
                                            {
                                                L.Add(Vector3.Normalize(new Vector3(light[t].X - j, light[t].Y - y, light[t].Z - z3)));
                                                distance.Add(DistanceVector3Vector3(light[t],pixelPoint));
                                            }
                                            
                                            Vector3 pom = V - pixelPoint;
                                            pom = Vector3.Normalize(pom);

                                            float r3 = Oswietlenie(kd, ks, Il.X, (float)c.R / 255, L,pom, N3, m, ka, distance);
                                            float g3 = Oswietlenie(kd, ks, Il.Y, (float)c.G / 255, L, pom, N3, m, ka, distance);
                                            float b3 = Oswietlenie(kd, ks, Il.Z, (float)c.B / 255, L, pom, N3, m, ka, distance);

                                            c = CalculateNewColor0to255((int)(r3 * 255), (int)(g3 * 255), (int)(b3 * 255));
                                        }
                                    }
                                    snoop.SetPixel(j, y, c);
                                }
                            }
                        }
                        AET[i].p.x += AET[i].p.m;
                        AET[i + 1].p.x += AET[i + 1].p.m;
                    }
                }
            }
        }
        
        public double DistanceVector3Vector3(Vector3 v1,Vector3 v2)
        {
            return Math.Sqrt(Math.Pow(v1.X-v2.X,2)+ Math.Pow(v1.Y - v2.Y, 2)+ Math.Pow(v1.Z - v2.Z, 2));
        }
    }
    
}
