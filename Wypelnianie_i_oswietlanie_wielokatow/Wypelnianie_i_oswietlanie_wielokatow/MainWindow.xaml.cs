using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Wypelnianie_i_oswietlanie_wielokatow
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<myPolygon> polygons = new List<myPolygon>(); //lista wielokątów
        myPolygon polygon = new myPolygon(); //wielokąt pomocniczy
        myLine line = new myLine(); //linia pomocnicza

        int pol_num = -1; //numer wielokąta z listy
        int ver_num = -1; //numer wierzchołka wielokąta
        int ver_num2 = -1; //numer drugiego wierzchołka wielokąta
        int line_num = -1; //numer linii wielokąta

        double old_dx; //stara zmienna X myszki
        double old_dy; //stara zmienna Y myszki

        System.Drawing.Bitmap bmp; //bitmapa, na której będą rysowane wielokąty
        System.Drawing.Bitmap polygonBitmap;
        System.Drawing.Bitmap heightBitmap;

        bool rysowanie = false;
        Edit edytowanie = Edit.none;
        Coloring kolorowanie = Coloring.none;
        ColoringType typKolorowania = ColoringType.none;
        System.Drawing.Color color;

        float kd = 0;
        float ks = 0;
        Vector3 Il = new Vector3(1, 1, 1);
        float z = 1; //wysokość światła
        Vector2 light = new Vector2();
        int m = 1;
 
        bool zmienPozycjeSwiatla = false;
        bool withHeightMap = false;
        System.Drawing.Bitmap zarowka = new System.Drawing.Bitmap(@"zarowka.png");

        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        bool czasStop = true; // można zmieniać zakres prędkości tylko podczas zatrzymanej sceny

        int minSpeed = 10;
        int maxSpeed = 20;

        public MainWindow()
        {
            InitializeComponent();
            minSpeedTextBox.Text = minSpeed.ToString();
            maxSpeedTextBox.Text = maxSpeed.ToString();
        }
        //---------------------------------------------------Sliders------------------------------------------------------------------------------------------------------------------------------------
        private void kdSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            kd = (float)kdSlider.Value;
            if(canvas.ActualWidth != 0)
                Draw();
        }

        private void ksSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ks = (float)ksSlider.Value;
            if (canvas.ActualWidth != 0)
                Draw();
        }

        private void mSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m = (int)mSlider.Value;
            if (canvas.ActualWidth != 0)
                Draw();
        } 
        private void zSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            z = (float)zSlider.Value;
            if (canvas.ActualWidth!=0)
                Draw();
        }
        
        private void lightBSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Il.Z = (float)lightBSlider.Value;
            if (canvas.ActualWidth != 0)
                Draw();
        }

        private void lightGSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Il.Y = (float)lightGSlider.Value;
            if (canvas.ActualWidth != 0)
                Draw();
        }

        private void lightRSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Il.X = (float)lightRSlider.Value;
            if (canvas.ActualWidth != 0)
                Draw();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        //---------------------------------------Buttons---------------------------------------------------------------------------------------------------------------------------------------------------------
         private void changeLightPositionButton_Click(object sender, RoutedEventArgs e)
         {
            zmienPozycjeSwiatla = !zmienPozycjeSwiatla;
            if(zmienPozycjeSwiatla)
                changeLightPositionButton.Content = "Zmienianie pozycji światła...";
            else
                changeLightPositionButton.Content = "Zmień pozycję światła";
        }
        private void polygonButton_Click(object sender, RoutedEventArgs e)
        {
            if (!rysowanie)
            {
                rysowanie = true;
                polygonButton.Content = "Rysuję...";
            }
        }

        private void pointButton_Click(object sender, RoutedEventArgs e)
        {
            if (edytowanie == Edit.none)
            {
                edytowanie = Edit.moveVertice;
                pointButton.Content = "Przesuwanie...";
            }
            else if (edytowanie == Edit.moveVertice)
            {
                edytowanie = Edit.none;
                pointButton.Content = "Przesuń punkt";
            }

        }

        private void deletePointButton_Click(object sender, RoutedEventArgs e)
        {
            if (edytowanie == Edit.deleteVertice)
            {
                edytowanie = Edit.none;
                deletePointButton.Content = "Usuń punkt";
            }
            else if (edytowanie == Edit.none)
            {
                edytowanie = Edit.deleteVertice;
                deletePointButton.Content = "Usuwanie...";
            }
        }

        private void polygonButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (edytowanie == Edit.movePolygon)
            {
                edytowanie = Edit.none;
                polygonButton2.Content = "Przesuń wielokąt";
            }
            else if (edytowanie == Edit.none)
            {
                edytowanie = Edit.movePolygon;
                polygonButton2.Content = "Przesuwanie...";
            }
        }

        private void addPointButton_Click(object sender, RoutedEventArgs e)
        {
            if (edytowanie == Edit.addVertice)
            {
                edytowanie = Edit.none;
                addPointButton.Content = "Dodaj punkt";
            }
            else if (edytowanie == Edit.none)
            {
                edytowanie = Edit.addVertice;
                addPointButton.Content = "Dodawanie...";
            }
        }
        private void pointColorButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog MyDialog = new System.Windows.Forms.ColorDialog();
            
            // Allows the user to get help. (The default is false.)
            MyDialog.ShowHelp = true;

            // Update the text box color if the user clicks OK 
            if (MyDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.Cursor = Cursors.Pen;
                kolorowanie = Coloring.vertice;
                color = MyDialog.Color;
            }
        } 
        private void polygonColorButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog MyDialog = new System.Windows.Forms.ColorDialog();

            // Allows the user to get help. (The default is false.)
            MyDialog.ShowHelp = true;

            // Update the text box color if the user clicks OK 
            if (MyDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.Cursor = Cursors.Pen;
                kolorowanie = Coloring.polygon;
                typKolorowania = ColoringType.color;
                color = MyDialog.Color;
            }
        }

        private void polygonInterpolationButton_Click(object sender, RoutedEventArgs e)
        {
            if(typKolorowania == ColoringType.none)
            {
                typKolorowania = ColoringType.interpolation;
                this.Cursor = Cursors.Pen;
            }
        }

        private void polygonBitmapButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = string.Empty;

            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "jpeg files (*.jpg)|*.jpg|png files (*.png)|*.png|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 3;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    
                    polygonBitmap = new System.Drawing.Bitmap(filePath);

                    typKolorowania = ColoringType.bitmap;
                    this.Cursor = Cursors.Pen;
                    withHeightMap = false;
                    
                }
                
            }
        }
        private void heightMapButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = string.Empty;

            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "jpeg files (*.jpg)|*.jpg|png files (*.png)|*.png|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 3;
                openFileDialog.RestoreDirectory = true;
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    
                    polygonBitmap = new System.Drawing.Bitmap(filePath);

                    typKolorowania = ColoringType.bitmap;
                    withHeightMap = false;
                    this.Cursor = Cursors.Pen;
                    
                }

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    
                    heightBitmap = new System.Drawing.Bitmap(filePath);
                    typKolorowania = ColoringType.bitmap;
                    withHeightMap = true;
                    this.Cursor = Cursors.Pen;
                    
                }
            }
        }
        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            //  DispatcherTimer setup
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0,0,40);
            dispatcherTimer.Start();
            czasStop = false;

        }
        Random rand = new Random();
        public double GetRandomNumberInRange(double minNumber, double maxNumber)
        {
            return new Random().NextDouble() * (maxNumber - minNumber) + minNumber;
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            Parallel.ForEach(polygons, p => {
                for(int i=0;i<p.vertices.Count;i++)
                {
                    double pom = p.vertices[i].point.X + p.speed_x;

                    if (pom >= canvas.ActualWidth-1 || pom <= 0)
                    {
                        if (p.speed_x > 0)
                            p.speed_x = -GetRandomNumberInRange(p.minSpeed, p.maxSpeed);
                        else
                            p.speed_x = GetRandomNumberInRange(p.minSpeed, p.maxSpeed);
                    }
                  
                    pom = p.vertices[i].point.Y + p.speed_y;
                    if (pom >= canvas.ActualHeight-1 || pom <= 0)
                    {
                        if (p.speed_y > 0)
                            p.speed_y = -(int)GetRandomNumberInRange(p.minSpeed, p.maxSpeed);
                        else
                            p.speed_y = (int)GetRandomNumberInRange(p.minSpeed, p.maxSpeed);
                    }
                }
            });
            Parallel.ForEach(polygons, p => {
                for (int i = 0; i < p.vertices.Count; i++)
                {
                    p.vertices[i].point.X += p.speed_x;

                    p.vertices[i].point.Y += p.speed_y;
                }

                if (p.typKolorowania == ColoringType.bitmap)
                {
                    p.CreatePolygonBitmap();
                    p.heightBitmap = zmienRozmiarObrazu(p.heightBitmap, p.width, p.height);
                    p.CreateZTable();
                    p.bitmap = zmienRozmiarObrazu(p.bitmap, p.width, p.height);
                }
            });
            Draw();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            czasStop = true;
        }
        //-------------------------------------TextBoxes----------------------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void minSpeedTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            if(czasStop)
            {
                if (minSpeedTextBox.Text != "")
                    minSpeed = Convert.ToInt32(minSpeedTextBox.Text);
                else
                    minSpeed = 0;
                Parallel.ForEach(polygons, p => {
                    
                    p.minSpeed = minSpeed;

                });
            }
            
        }

        private void maxSpeedTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(czasStop)
            {
                if (maxSpeedTextBox.Text != "")
                    maxSpeed = Convert.ToInt32(maxSpeedTextBox.Text);
                else
                    maxSpeed = 0;
                Parallel.ForEach(polygons, p => {

                    p.maxSpeed = maxSpeed;

                }); 
            }
             
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        //------------------------------------------------Functions------------------------------------------------------------------------------------------------------------------------------------------------
        void Draw()
        {
            bmp = new System.Drawing.Bitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight);
            System.Drawing.Graphics graphics =  System.Drawing.Graphics.FromImage(bmp);
            graphics.DrawImage(zarowka, new System.Drawing.PointF(light.X-25,light.Y-25));
            BmpPixelSnoop snoop = new BmpPixelSnoop(bmp);
            
            Parallel.ForEach(polygons, p => {
                PolygonFilling filling = new PolygonFilling();
                p.DrawPolygon(ref snoop);
                filling.Fill(p, ref snoop,ks,kd,z,m,Il,light);
            });
            
            myImage.Source = snoop.BmpImageFromBmp();
            snoop.Dispose();
        }
        public System.Drawing.Bitmap zmienRozmiarObrazu(System.Drawing.Bitmap bitmapa, int nowaSzerokosc,int nowaWysokosc)
        {
            
            if (nowaSzerokosc == 0 || nowaWysokosc == 0)
                return bitmapa;
            
            System.Drawing.Image obraz = (System.Drawing.Image)bitmapa;

            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(nowaSzerokosc, nowaWysokosc);

            System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage((System.Drawing.Image)bmp);

            graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            graphic.DrawImage(obraz, 0, 0, nowaSzerokosc, nowaWysokosc);

            graphic.Dispose();

            return bmp;

        }
        
        bool SearchForPolygonPoint(int px, int py)
        {
            for (int i = 0; i < polygons.Count; i++)
            {
                for (int j = 0; j < polygons[i].vertices.Count; j++)
                {

                    if (polygons[i].vertices[j].point.X + 5 >= px && polygons[i].vertices[j].point.X - 5 <= px && polygons[i].vertices[j].point.Y + 5 >= py && polygons[i].vertices[j].point.Y - 5 <= py)
                    {
                        pol_num = i;
                        ver_num = j;
                        old_dx = px;
                        old_dy = py;
                        return true;
                    }
                }
            }
            return false;
        }
        bool SearchForPolygonLine(double px, double py)
        {
            Point position = new Point(px, py);
            for (int i = 0; i < polygons.Count; i++)
            {
                for (int k = 0; k < polygons[i].lines.Count; k++)
                {
                    int l = k >= polygons[i].lines.Count - 1 ? 0 : k + 1;//sprawdzam czy następny wierzchołek po k jest wierzchołkiem początkowym

                    double leftPoint = Math.Min(polygons[i].vertices[k].point.X, polygons[i].vertices[l].point.X);
                    double rightPoint = polygons[i].vertices[k].point.X == leftPoint ? polygons[i].vertices[l].point.X : polygons[i].vertices[k].point.X;
                    double topPoint = Math.Max(polygons[i].vertices[k].point.Y, polygons[i].vertices[l].point.Y);
                    double bottomPoint = polygons[i].vertices[k].point.Y == topPoint ? polygons[i].vertices[l].point.Y : polygons[i].vertices[k].point.Y;

                    if (polygon.CalculateDistancePointToLine(polygons[i].lines[k], position) <= 1 && py <= topPoint && py >= bottomPoint && px <= rightPoint && px >= leftPoint)
                    {
                        pol_num = i;
                        old_dx = px;
                        old_dy = py;
                        line_num = k;
                        ver_num = k;
                        ver_num2 = l;
                        return true;
                    }
                }
            }
            return false;
        }

        void ReCalculateBitmamps(int pol_num)
        {
            polygons[pol_num].CreatePolygonBitmap();
            polygons[pol_num].bitmap = zmienRozmiarObrazu(polygons[pol_num].bitmap, polygons[pol_num].width, polygons[pol_num].height);
            if (polygons[pol_num].withHeightMap)
            {
                polygons[pol_num].heightBitmap = zmienRozmiarObrazu(polygons[pol_num].heightBitmap, polygons[pol_num].width, polygons[pol_num].height);
                polygons[pol_num].CreateZTable();
            }
        }

        void CalculateBitmaps(int pol_num)
        {
            polygons[pol_num].typKolorowania = typKolorowania;
            polygons[pol_num].withHeightMap = withHeightMap;

            if (polygonBitmap != null)
                polygons[pol_num].bitmap = zmienRozmiarObrazu(polygonBitmap, polygons[pol_num].bitmap.Width, polygons[pol_num].bitmap.Height);
            if (heightBitmap != null)
            {
                polygons[pol_num].heightBitmap = zmienRozmiarObrazu(heightBitmap, polygons[pol_num].bitmap.Width, polygons[pol_num].bitmap.Height);
                polygons[pol_num].CreateZTable();
            }
        }

        System.Drawing.Color RandomColor() //zwraca losowy kolor
        {
            Random rand = new Random();
            int r = rand.Next(256);
            int g = rand.Next(256);
            int b = rand.Next(256);

            return System.Drawing.Color.FromArgb(r, g, b);
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        //-----------------------------------------------Drawing-on-Canvas-----------------------------------------------------------------------------------------------------------------------------------------
        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(this);
            int px = (int)position.X;
            int py = (int)position.Y;
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    {
                        
                        if(zmienPozycjeSwiatla)
                        {
                            light = new Vector2(px, py);
                            Draw();
                        }
                        
                        if (rysowanie)
                        {
                            int n = polygon.vertices.Count;
                            BmpPixelSnoop snoop = new BmpPixelSnoop(bmp);
                            myPoint point = new myPoint(new Point(px, py), RandomColor());
                            polygon.vertices.Add(point);
                            point.DrawPoint(ref snoop);

                            if (n >= 1)
                            {
                                line.CalculateLine(polygon.vertices[n - 1].point, polygon.vertices[n].point);
                                polygon.lines.Add(line);
                                polygon.BresenhamLine((int)polygon.vertices[n - 1].point.X, (int)polygon.vertices[n - 1].point.Y, (int)polygon.vertices[n].point.X, (int)polygon.vertices[n].point.Y, ref snoop);
                            }
                            myImage.Source = snoop.BmpImageFromBmp();
                            snoop.Dispose(); //unlock bits
                        }


                        if (edytowanie != Edit.none || kolorowanie != Coloring.none || typKolorowania!=ColoringType.none) //tryb edycji
                        {
                            ver_num = -1;
                            ver_num2 = -1;
                            pol_num = -1;
                            line_num = -1;

                            if (edytowanie == Edit.moveVertice || edytowanie == Edit.deleteVertice || edytowanie == Edit.movePolygon || kolorowanie == Coloring.vertice || kolorowanie == Coloring.polygon || typKolorowania == ColoringType.interpolation || typKolorowania == ColoringType.bitmap)
                            {
                                if (SearchForPolygonPoint(px, py))
                                {
                                    if (edytowanie == Edit.deleteVertice && polygons[pol_num].vertices.Count > 3)
                                    {
                                        int l = ver_num - 1 < 0 ? polygons[pol_num].lines.Count - 2 : ver_num - 1; //sprawdzam czy poprzedni wierzchołek po ver_num jest wierzchołkiem końcowym
                                        int k = ver_num + 1 >= polygons[pol_num].lines.Count - 1 ? 0 : ver_num + 1; //sprawdzam czy następny wierzchołek po ner_num jest wierzchołkiem początkowym
                                        polygons[pol_num].vertices.RemoveAt(ver_num);
                                        polygons[pol_num].lines.RemoveAt(ver_num);
                                        polygons[pol_num].lines.RemoveAt(l);
                                        line.CalculateLine(polygons[pol_num].vertices[l].point, polygons[pol_num].vertices[k].point);
                                        polygons[pol_num].lines.Insert(l, line);

                                        ReCalculateBitmamps(pol_num);
                                    }
                                    if(kolorowanie == Coloring.vertice)
                                    {
                                        polygons[pol_num].vertices[ver_num].color = color;
                                        this.Cursor = Cursors.Arrow;
                                        kolorowanie = Coloring.none;
                                        
                                    }
                                    if(kolorowanie == Coloring.polygon)
                                    {
                                        polygons[pol_num].color = color;
                                        polygons[pol_num].typKolorowania = typKolorowania;
                                        this.Cursor = Cursors.Arrow;
                                        kolorowanie = Coloring.none;
                                        typKolorowania = ColoringType.none;
                                        
                                    }
                                    if(typKolorowania == ColoringType.interpolation)
                                    {
                                        polygons[pol_num].typKolorowania = typKolorowania;
                                        this.Cursor = Cursors.Arrow;
                                        typKolorowania = ColoringType.none;
                                        
                                    }
                                    if (typKolorowania == ColoringType.bitmap)
                                    {
                                        
                                        CalculateBitmaps(pol_num);
                                        
                                        this.Cursor = Cursors.Arrow;
                                        typKolorowania = ColoringType.none;
                                    }
                                    Draw();
                                }
                            }

                            if (edytowanie == Edit.addVertice || edytowanie == Edit.movePolygon || kolorowanie == Coloring.polygon || typKolorowania == ColoringType.interpolation || typKolorowania == ColoringType.bitmap) //przesuwanie krawędzi wielokąta i dodawanie punktu
                            {
                                if (SearchForPolygonLine(px, py))
                                {
                                    if (edytowanie == Edit.addVertice)
                                    {
                                        Point p = new Point((polygons[pol_num].vertices[ver_num].point.X + polygons[pol_num].vertices[ver_num2].point.X) / 2, (polygons[pol_num].vertices[ver_num].point.Y + polygons[pol_num].vertices[ver_num2].point.Y) / 2);
                                        myPoint myPoint = new myPoint(p, RandomColor());
                                        polygons[pol_num].vertices.Insert(ver_num2, myPoint);
                                        polygons[pol_num].lines.RemoveAt(ver_num);
                                        line.CalculateLine(polygons[pol_num].vertices[ver_num].point, polygons[pol_num].vertices[ver_num2].point);
                                        polygons[pol_num].lines.Insert(ver_num, line);
                                        line.CalculateLine(polygons[pol_num].vertices[ver_num2].point, polygons[pol_num].vertices[ver_num2 + 1].point);
                                        polygons[pol_num].lines.Insert(ver_num2, line);
                                        
                                    }
                                    if (kolorowanie == Coloring.polygon)
                                    {
                                        polygons[pol_num].color = color;
                                        polygons[pol_num].typKolorowania = typKolorowania;
                                        this.Cursor = Cursors.Arrow;
                                        kolorowanie = Coloring.none;
                                        typKolorowania = ColoringType.none;
                                        
                                    }
                                    if (typKolorowania == ColoringType.interpolation)
                                    {
                                        polygons[pol_num].typKolorowania = typKolorowania;
                                        this.Cursor = Cursors.Arrow;
                                        typKolorowania = ColoringType.none;
                                        
                                    }
                                    if (typKolorowania == ColoringType.bitmap)
                                    {
                                       CalculateBitmaps(pol_num);
                                            
                                        this.Cursor = Cursors.Arrow;
                                        typKolorowania = ColoringType.none; 
                                    }
                                    Draw();
                                }
                            }
                        }
                    }
                    break;
                case MouseButton.Right:
                    {
                        if (rysowanie)
                        {
                            int n = polygon.vertices.Count;
                            line.CalculateLine(polygon.vertices[n - 1].point, polygon.vertices[0].point);
                            polygon.lines.Add(line);
                            polygon.CreatePolygonBitmap();
                            polygon.typKolorowania = ColoringType.color;
                            polygon.color = RandomColor();
                            polygon.minSpeed = minSpeed;
                            polygon.maxSpeed = maxSpeed;
                            rysowanie = false;
                            polygonButton.Content = "Rysuj";
                            polygons.Add(polygon);
                            polygon = new myPolygon();
                            Draw();
                        }
                    }
                    break;
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && (edytowanie == Edit.moveVertice || edytowanie == Edit.movePolygon))
            {
                Point position = e.GetPosition(this);
                int px = (int)position.X;
                int py = (int)position.Y;

                if (pol_num >= 0 && ver_num >= 0 && line_num == -1 && edytowanie != Edit.movePolygon) //przesuwanie punktu
                {
                    polygons[pol_num].vertices[ver_num].point = new Point(px, py);
                    int n = ver_num - 1, m = ver_num + 1;
                    if (n < 0)
                        n = polygons[pol_num].vertices.Count - 1;
                    if (m > polygons[pol_num].vertices.Count - 1)
                        m = 0;

                    line.CalculateLine(polygons[pol_num].vertices[n].point, polygons[pol_num].vertices[ver_num].point);
                    polygons[pol_num].lines[n] = line;

                    line.CalculateLine(polygons[pol_num].vertices[ver_num].point, polygons[pol_num].vertices[m].point);
                    polygons[pol_num].lines[ver_num] = line;

                    old_dx = px;
                    old_dy = py;
                    
                    if(polygons[pol_num].typKolorowania == ColoringType.bitmap)
                    {
                        ReCalculateBitmamps(pol_num);
                    }  
                }

                if ((edytowanie == Edit.movePolygon) && pol_num >= 0) //przesuwanie wielokątu
                {
                    myPoint[] pom = new myPoint[polygons[pol_num].vertices.Count];
                    pom = polygons[pol_num].vertices.ToArray();

                    for (int i = 0; i < polygons[pol_num].vertices.Count; i++)
                    {
                        pom[i].point.X += px - old_dx;
                        pom[i].point.Y += py - old_dy;
                    }
                    old_dx = px;
                    old_dy = py;
                    polygons[pol_num].vertices = pom.ToList<myPoint>();
                    
                    if (polygons[pol_num].typKolorowania == ColoringType.bitmap)
                    {
                        ReCalculateBitmamps(pol_num);
                    }      
                }
                Draw();
            }
        }
        //----------------------------------------------Window-------------------------------------------------------------------------------------------------------------
        private void canvas_Loaded(object sender, RoutedEventArgs e)
        {
            bmp = new System.Drawing.Bitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight);
            light = new Vector2((int)canvas.ActualWidth/2, (int)canvas.ActualHeight/2);
            zarowka = zmienRozmiarObrazu(zarowka, 50, 50);
            Draw();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            bmp = new System.Drawing.Bitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight);
            light = new Vector2((int)canvas.ActualWidth / 2, (int)canvas.ActualHeight / 2);
            Draw();
        }

        
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
