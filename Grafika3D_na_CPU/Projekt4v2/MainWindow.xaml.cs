using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Threading;

namespace Projekt4v2
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer timer;
        private readonly int maxFps = 60;
        System.Drawing.Bitmap bitmap;
        double W;
        double H;
        Sphere sphere;
        Cube cube;
        Square square;
        Vector3 cameraTarget;
        Vector3 cameraPosition;
        Matrix4x4 View;
        Matrix4x4 Proj;
        Matrix4x4 M;
        Matrix4x4 M2;
        Matrix4x4 M3;
        List<Matrix4x4> lightM;
        double rotation = 0;
        List<Vector4> lightPoints = new List<Vector4>();
        float z = 100;
        float kd = (float)0.5;
        float ks = (float)0.5;
        float ka = (float)0.5;
        int m = 1;
        float go = 0;
        bool goBack = false;
        Vector3 Il = new Vector3(1, 1, 1);
        PolygonFilling polygonFilling = new PolygonFilling();
        Shader shader = Shader.Flat;
        bool follow = false;
        bool moveCamera = false;
        double[,] zBufor;

        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, 0, 1000 / maxFps)
            };
            timer.Tick += TimerTick;
        }
        private void TimerTick(object sender, EventArgs e)
        {
            bitmap = new System.Drawing.Bitmap((int)W, (int)H);
            polygonFilling = new PolygonFilling();
            
            if (go >= 2)
                goBack = true;
            if (go < -2)
                goBack = false;
            if(!goBack)
            {
                go += (float)0.1;
                rotation -= 0.1;
            }
            else
            {
                go -= (float)0.1;
                rotation += (float)0.1;
            }
                
            Matrix4x4 T2 = Matrix4x4.CreateTranslation(new Vector3(-4, 0, 0 - go));
            
            Matrix4x4 R2 = Matrix4x4.CreateRotationX(radians: (float)rotation);
            Matrix4x4 T3 = Matrix4x4.CreateTranslation(new Vector3((float)-1.5,(float)-1.5,(float)-1.5));
            
            M2 = T3 * R2 * T2;

            if(follow)
            {
                cameraTarget = new Vector3(-4,0,-3-go);
                View = Matrix4x4.CreateLookAt(cameraPosition, cameraTarget, new Vector3(0, 1, 0));
            }
            if(moveCamera)
            {
                cameraPosition = new Vector3(-4, 5, 10-go);
                cameraTarget = new Vector3(-4, 1, -3 - go);
                View = Matrix4x4.CreateLookAt(cameraPosition, cameraTarget, new Vector3(0, 1, 0));
            }
            BmpPixelSnoop snoop = new BmpPixelSnoop(bitmap);
            Draw(snoop,cameraTarget,sphere,cube,square,M,M2,M3,View,Proj);
            myImage.Source = snoop.BmpImageFromBmp();
            snoop.Dispose();
        }

        //---------------------------------------------------Sliders------------------------------------------------------------------------------------------------------------------------------------
        private void kaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ka = (float)kaSlider.Value;
        }
        private void kdSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            kd = (float)kdSlider.Value;
        }

        private void ksSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ks = (float)ksSlider.Value;
        }

        private void mSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m = (int)mSlider.Value;
        }
        private void zSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            z = (float)zSlider.Value;
        }

        private void lightBSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Il.Z = (float)lightBSlider.Value;
        }

        private void lightGSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Il.Y = (float)lightGSlider.Value;
        }

        private void lightRSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Il.X = (float)lightRSlider.Value;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------
        
        //------------------------------------------------Buttons----------------------------------------------------------------------------------
        private void flatShaderButton_Click(object sender, RoutedEventArgs e)
        {
            shader = Shader.Flat;
        }

        private void gouradShaderButton_Click(object sender, RoutedEventArgs e)
        {
            shader = Shader.Gourad;
        }

        private void phongShaderButton_Click(object sender, RoutedEventArgs e)
        {
            shader = Shader.Phong;
        }
        private void mainCameraButton_Click(object sender, RoutedEventArgs e)
        {
            cameraPosition = new Vector3(10, 10, 7);
            cameraTarget = new Vector3(0, 0, 0);
            View = Matrix4x4.CreateLookAt(cameraPosition, cameraTarget, new Vector3(0, 1, 0));
            follow = false;
            moveCamera = false;
        }

        private void secondCameraButton_Click(object sender, RoutedEventArgs e)
        {
            cameraPosition = new Vector3(-4,5,10);
            cameraTarget = new Vector3(-4,0,-3);
            View = Matrix4x4.CreateLookAt(cameraPosition, cameraTarget, new Vector3(0, 1, 0));
            follow = true;
            moveCamera = false;
        }
        private void thirdCameraButton_Click(object sender, RoutedEventArgs e)
        {
            cameraPosition = new Vector3(-4,5,10);
            cameraPosition = new Vector3(-4,0,-3);
            View = Matrix4x4.CreateLookAt(cameraPosition, cameraTarget, new Vector3(0, 1, 0));
            follow = false; ;
            moveCamera = true;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------
        
        void Draw(BmpPixelSnoop snoop2,Vector3 cameraTarget, Sphere sphere,Cube cube,Square square, Matrix4x4 M,Matrix4x4 M2, Matrix4x4 M3,Matrix4x4 View, Matrix4x4 Proj)
        {
            zBufor = new double[(int)W, (int)H];
            for (int i = 0; i < W; i++)
                for (int j = 0; j < H; j++)
                    zBufor[i, j] = double.MinValue;
            
            List<Vector3> newLightPoints = new List<Vector3>();
            for(int i=0;i<lightPoints.Count;i++)
            {
                lightPoints[i] = Vector4.Transform(lightPoints[i],lightM[i]*View*Proj);
                lightPoints[i] = lightPoints[i] / lightPoints[i].W;
                Vector3 newLightPoint = new Vector3((float)((lightPoints[i].X + 1) * W / 2), (float)((1 - lightPoints[i].Y) * H / 2), (lightPoints[i].Z+1)/2);
                if (i==1)
                    newLightPoint = new Vector3((float)((lightPoints[i].X + 1) * W / 2), (float)((1 - lightPoints[i].Y) * H / 2), z);
                newLightPoints.Add(newLightPoint);
            }
            
            
            for (int i = 0; i < square.total; i++)
                for (int j = 0; j < square.total; j++)
                {
                    Vector4 v = square.globe[i, j];
                    Vector4 v2 = square.globe[i, j + 1];
                    Vector4 v3 = square.globe[i + 1, j];
                    Vector4 v4 = square.globe[i + 1, j + 1];
                    Matrix4x4 POM = M3 * View * Proj;
                    Vector4 pom = Vector4.Transform(v, POM);
                    Vector4 pom2 = Vector4.Transform(v2, POM);
                    Vector4 pom3 = Vector4.Transform(v3, POM);
                    Vector4 pom4 = Vector4.Transform(v4, POM);

                    pom = pom / pom.W;
                    pom2 = pom2 / pom2.W;
                    pom3 = pom3 / pom3.W;
                    pom4 = pom4 / pom4.W;

                    Point p = new Point((pom.X + 1) * W / 2, (int)((1 - pom.Y) * H / 2));
                    Point p2 = new Point((pom2.X + 1) * W / 2, (int)((1 - pom2.Y) * H / 2));
                    Point p3 = new Point((pom3.X + 1) * W / 2, (int)((1 - pom3.Y) * H / 2));
                    Point p4 = new Point((pom4.X + 1) * W / 2, (int)((1 - pom4.Y) * H / 2));

                    Triangle triangle = new Triangle(new Vector3(pom.X, pom.Y, pom.Z), new Vector3(pom2.X, pom2.Y, pom2.Z), new Vector3(pom3.X, pom3.Y, pom3.Z));
                    
                    polygonFilling.Fill(new List<myPoint>() { new myPoint(p, System.Drawing.Color.AliceBlue,(pom.Z + 1)/2), new myPoint(p2, System.Drawing.Color.AliceBlue,(pom2.Z+1)/2), new myPoint(p3, System.Drawing.Color.AliceBlue,(pom3.Z+1)/2) }, triangle, ref snoop2, ks, kd, m, Il, newLightPoints, System.Drawing.Color.LightGreen, ka, shader,ref zBufor,cameraTarget);
                    
                    triangle = new Triangle(new Vector3(pom4.X, pom4.Y, pom4.Z), new Vector3(pom3.X, pom3.Y, pom3.Z), new Vector3(pom2.X, pom2.Y, pom2.Z));
                    polygonFilling.Fill(new List<myPoint>() { new myPoint(p4, System.Drawing.Color.AliceBlue, (pom4.Z + 1) / 2), new myPoint(p2, System.Drawing.Color.AliceBlue, (pom2.Z + 1) / 2), new myPoint(p3, System.Drawing.Color.AliceBlue, (pom3.Z + 1) / 2) }, triangle, ref snoop2, ks, kd, m, Il, newLightPoints, System.Drawing.Color.LightGreen, ka, shader,ref zBufor, cameraTarget);
                    
                }
            for (int i = 0; i < 6; i++)
                 for (int j = 0; j < cube.total; j++)
                    for (int k = 0; k < cube.total; k++)
                    {
                        Vector4 v = cube.globe[i, j, k];
                        Vector4 v2 = cube.globe[i, j, k + 1];
                        Vector4 v3 = cube.globe[i, j + 1, k];
                        Vector4 v4 = cube.globe[i, j + 1, k + 1];
                        Matrix4x4 POM = M2 * View * Proj;
                        Vector4 pom = Vector4.Transform(v, POM);
                        Vector4 pom2 = Vector4.Transform(v2, POM);
                        Vector4 pom3 = Vector4.Transform(v3, POM);
                        Vector4 pom4 = Vector4.Transform(v4, POM)

                        pom = pom / pom.W;
                        pom2 = pom2 / pom2.W;
                        pom3 = pom3 / pom3.W;
                        pom4 = pom4 / pom4.W;
                        Vector3 N = new Vector3(pom3.X, pom3.Y, pom3.Z);

                        Point p = new Point((pom.X + 1) * W / 2, (int)((1 - pom.Y) * H / 2));
                        Point p2 = new Point((pom2.X + 1) * W / 2, (int)((1 - pom2.Y) * H / 2));
                        Point p3 = new Point((pom3.X + 1) * W / 2, (int)((1 - pom3.Y) * H / 2));
                        Point p4 = new Point((pom4.X + 1) * W / 2, (int)((1 - pom4.Y) * H / 2));

                        Triangle triangle;
                        if (i % 2 != 0)
                            triangle = new Triangle(new Vector3(pom.X, pom.Y, pom.Z), new Vector3(pom3.X, pom3.Y, pom3.Z), new Vector3(pom2.X, pom2.Y, pom2.Z));
                        else
                            triangle = new Triangle(new Vector3(pom3.X, pom3.Y, pom3.Z), new Vector3(pom.X, pom.Y, pom.Z), new Vector3(pom2.X, pom2.Y, pom2.Z));
                        
                        double w = Vector3.Dot(triangle.expectedNormal, cameraTarget - N);
                        
                        if (w <= 0)
                        {
                            polygonFilling.Fill(new List<myPoint>() { new myPoint(p, System.Drawing.Color.AliceBlue, (pom.Z + 1) / 2), new myPoint(p2, System.Drawing.Color.AliceBlue, (pom2.Z + 1) / 2), new myPoint(p3, System.Drawing.Color.AliceBlue, (pom3.Z + 1) / 2) }, triangle, ref snoop2, ks, kd, m, Il, newLightPoints, System.Drawing.Color.Blue, ka, shader, ref zBufor, cameraPosition);
                           
                            polygonFilling.Fill(new List<myPoint>() { new myPoint(p4, System.Drawing.Color.AliceBlue, (pom4.Z + 1) / 2), new myPoint(p2, System.Drawing.Color.AliceBlue, (pom2.Z + 1) / 2), new myPoint(p3, System.Drawing.Color.AliceBlue, (pom3.Z + 1) / 2) }, triangle, ref snoop2, ks, kd, m, Il, newLightPoints, System.Drawing.Color.Blue,ka, shader, ref zBufor, cameraPosition);
                        }
                    }
            for (int i = 0; i < sphere.total; i++)
                for (int j = 0; j < sphere.total; j++)
                {
                    Vector4 v = sphere.globe[i, j];
                    Vector4 v2 = sphere.globe[i, j + 1];
                    Vector4 v3 = sphere.globe[i + 1, j];
                    Vector4 v4 = sphere.globe[i + 1, j + 1];
                    Matrix4x4 POM = View * Proj;
                    Vector4 pom = Vector4.Transform(v, POM);
                    Vector4 pom2 = Vector4.Transform(v2, POM);
                    Vector4 pom3 = Vector4.Transform(v3, POM);
                    Vector4 pom4 = Vector4.Transform(v4, POM);
                    
                    pom = pom / pom.W;
                    pom2 = pom2 / pom2.W;
                    pom3 = pom3 / pom3.W;
                    pom4 = pom4 / pom4.W;
                    Vector3 N = Vector3.Normalize(new Vector3(pom.X, pom.Y, pom.Z));
                    Vector3 N2 = Vector3.Normalize(new Vector3(pom2.X, pom2.Y, pom2.Z));
                    
                    Point p = new Point((pom.X + 1) * W / 2, (int)((1 - pom.Y) * H / 2));
                    Point p2 = new Point((pom2.X + 1) * W / 2, (int)((1 - pom2.Y) * H / 2));
                    Point p3 = new Point((pom3.X + 1) * W / 2, (int)((1 - pom3.Y) * H / 2));
                    Point p4 = new Point((pom4.X + 1) * W / 2, (int)((1 - pom4.Y) * H / 2));
                    
                    Triangle triangle = new Triangle(new Vector3(pom.X, pom.Y, pom.Z), new Vector3(pom2.X, pom2.Y, pom2.Z), new Vector3(pom3.X, pom3.Y, pom3.Z));
                    triangle.circle = true;
                    double w = Vector3.Dot(triangle.expectedNormal, cameraTarget - N);
                    
                    if (w <= 0)
                    {
                       polygonFilling.Fill(new List<myPoint>() { new myPoint(p, System.Drawing.Color.AliceBlue, (pom.Z + 1) / 2), new myPoint(p2, System.Drawing.Color.AliceBlue, (pom2.Z + 1) / 2), new myPoint(p3, System.Drawing.Color.AliceBlue, (pom3.Z + 1) / 2) }, triangle, ref snoop2, ks, kd, m, Il, newLightPoints, System.Drawing.Color.Red, ka, shader, ref zBufor, cameraPosition);
                    }

                    triangle = new Triangle(new Vector3(pom4.X, pom4.Y, pom4.Z), new Vector3(pom3.X, pom3.Y, pom3.Z), new Vector3(pom2.X, pom2.Y, pom2.Z));
                    triangle.circle = true;
                    w = Vector3.Dot(triangle.expectedNormal, cameraTarget - N2);
                    
                    if (w <= 0)
                    {
                        polygonFilling.Fill(new List<myPoint>() { new myPoint(p4, System.Drawing.Color.AliceBlue, (pom4.Z + 1) / 2), new myPoint(p2, System.Drawing.Color.AliceBlue, (pom2.Z + 1) / 2), new myPoint(p3, System.Drawing.Color.AliceBlue, (pom3.Z + 1) / 2) }, triangle, ref snoop2, ks, kd, m, Il, newLightPoints, System.Drawing.Color.Red, ka, shader, ref zBufor, cameraPosition);
                    }

                }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            W = myCanvas.ActualWidth;
            H = myCanvas.ActualHeight;

            zBufor = new double[(int)W, (int)H];
            for (int i = 0; i < W; i++)
                for (int j = 0; j < H; j++)
                    zBufor[i, j] = double.MinValue;

            cameraPosition = new Vector3(10,10,7);
            cameraTarget = new Vector3(0, 0, 0);
            Vector3 upVector = new Vector3(0, 1, 0);
            
            View = Matrix4x4.CreateLookAt(cameraPosition,cameraTarget,upVector);
            Proj = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI/4,1,1,100);
            
            Matrix4x4 T = Matrix4x4.CreateTranslation(new Vector3(0,-2, 0));
            
            Matrix4x4 T2 = Matrix4x4.CreateTranslation(new Vector3(-4, 4, -3));
            Matrix4x4 T3 = Matrix4x4.CreateTranslation(new Vector3(-5, -2, -5));
            Matrix4x4 R = Matrix4x4.CreateRotationY((float)Math.PI/4);
            Matrix4x4 T4 = Matrix4x4.CreateTranslation(new Vector3(0, 5, 0));
            Matrix4x4 T5 = Matrix4x4.CreateTranslation(new Vector3(2, 5, 0));

            M =  T * R;
            
            M2 = T2;
            
            M3 = T3;
            
            bitmap = new System.Drawing.Bitmap((int)W, (int)H);
           
            sphere = new Sphere(2, 30, (int)W, (int)H);
            sphere.Triangulate();

            cube = new Cube(3,3);
            cube.Triangulate();

            square = new Square(10, 10);
            square.Triangulate();

            Vector4 lightPoint = new Vector4(0, 0, 0, 1);
            lightPoints.Add(lightPoint);
            lightPoints.Add(lightPoint);
            lightPoints.Add(lightPoint);
            lightM = new List<Matrix4x4>()
            {
                T3,
                T4,
                T5
            };

            timer.Start();
        }

        public void BresenhamLine(int x1, int y1, int x2, int y2, ref BmpPixelSnoop dbm)
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
