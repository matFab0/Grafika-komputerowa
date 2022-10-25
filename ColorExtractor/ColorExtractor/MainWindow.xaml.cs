using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;

namespace ColorExtractor
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Drawing.Bitmap mainBitmap;
        System.Drawing.Bitmap firstBitmap;
        System.Drawing.Bitmap secondBitmap;
        System.Drawing.Bitmap thirdBitmap;

        System.Drawing.Bitmap prevBitmap;
        Stack<System.Drawing.Bitmap> prevBitmaps = new Stack<System.Drawing.Bitmap>();
        Stack<ColorProfile> oldColorProfiles = new Stack<ColorProfile>();
        string fileName;

        // domyślne ustawienia
        ColorProfile colorProfile = new ColorProfile()
        {
            colorPoints = new List<Point>()
            {
                new Point(0.64, 0.33),
                new Point(0.3, 0.6),
                new Point(0.15,0.06)
            },
            whitePoint = new Point(0.31273, 0.32902),
            gamma = 2.2,
            name = "sRGB",
            whitePointName = "D65"
        };
        ColorProfile myColorProfile = new ColorProfile()
        {
            colorPoints = new List<Point>(),
            whitePoint = new Point(),
            gamma = 2.2,
            name = "Własny",
            whitePointName = "Własna"
        };
        ColorProfile oldColorProfile = new ColorProfile();
        bool imageLoaded = false;
        ColorModel colorModel = ColorModel.YCbCr; 
        public MainWindow()
        {
            InitializeComponent();
        }
        
        //--------------------------------Buttons-----------------------------------------------------------
        private void loadButton_Click(object sender, RoutedEventArgs e)
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
                    
                    fileName = Path.GetFileNameWithoutExtension(filePath);
                    
                    System.Drawing.Bitmap orig = new System.Drawing.Bitmap(filePath);
                    System.Drawing.Bitmap clone = new System.Drawing.Bitmap(orig.Width, orig.Height,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(clone))
                    {
                        gr.DrawImage(orig, new System.Drawing.Rectangle(0, 0, clone.Width, clone.Height));
                    }
                    mainBitmap = zmienRozmiarObrazu(clone,(int)mainCanvas.ActualWidth,(int)mainCanvas.ActualHeight);
                    
                    mainImage.Source = BmpImageFromBmp(mainBitmap);
                    imageLoaded = true;
                }

            }
        } 
        private void separateChannelsButton_Click(object sender, RoutedEventArgs e)
        {
            if (imageLoaded)
            {
                int imageWidth = (int)mainCanvas.ActualWidth;
                int imageHeight = (int)mainCanvas.ActualHeight;

                BmpPixelSnoop snoop = new BmpPixelSnoop(mainBitmap);
                BmpPixelSnoop snoop1 = new BmpPixelSnoop(firstBitmap);
                BmpPixelSnoop snoop2 = new BmpPixelSnoop(secondBitmap);
                BmpPixelSnoop snoop3 = new BmpPixelSnoop(thirdBitmap);

                ColorConvert convert = new ColorConvert();
                System.Drawing.Color[] colors = new System.Drawing.Color[3];
                for (int i = 0; i < imageWidth; i++)
                    for (int j = 0; j < imageHeight; j++)
                    {
                        switch (colorModel)
                        {
                            case ColorModel.YCbCr:
                                colors = convert.ConvertRGBToYCrCb(snoop.GetPixel(i, j));
                                break;
                            case ColorModel.HSV:
                                colors = convert.ConvertRGBToHSV(snoop.GetPixel(i, j));
                                break;
                            case ColorModel.Lab:
                                colors = convert.ConvertRGBToLab(snoop.GetPixel(i, j), colorProfile, colorProfile.whitePoint, colorProfile.gamma);
                                break;
                        }

                        snoop1.SetPixel(i, j, colors[0]);
                        snoop2.SetPixel(i, j, colors[1]);
                        snoop3.SetPixel(i, j, colors[2]);
                    }
                snoop.Dispose();
                snoop1.Dispose();
                snoop2.Dispose();
                snoop3.Dispose();
                firstImage.Source = BmpImageFromBmp(firstBitmap);
                secondImage.Source = BmpImageFromBmp(secondBitmap);
                thirdImage.Source = BmpImageFromBmp(thirdBitmap);
            }
        }
        private void applyButton_Click(object sender, RoutedEventArgs e)
        {
            oldColorProfile = colorProfile;
            oldColorProfiles.Push(oldColorProfile);
            colorProfile = myColorProfile;
            if(imageLoaded)
                RefreshImage();
        }
        private void previousColorProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (prevBitmaps.Count > 0 && oldColorProfiles.Count > 0)
            {
                PreviousImage();
                colorProfile = oldColorProfiles.Pop();

                FillTextboxes();

                colorProfileCombobox.Text = colorProfile.name;
                illuminatCombobox.Text = colorProfile.whitePointName;
            }
        }
        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath;
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                filePath = dialog.SelectedPath;
            }
            string firstFileName = filePath +"\\" + fileName +"_" + firstLabel.Content+".png";
            string secondFileName = filePath + "\\" + fileName + "_" + secondLabel.Content + ".png";
            string thirdFileName = filePath + "\\" + fileName + "_" + thirdLabel.Content + ".png";
            
            Save(BmpImageFromBmp(firstBitmap), firstFileName);
            Save(BmpImageFromBmp(secondBitmap), secondFileName);
            Save(BmpImageFromBmp(thirdBitmap), thirdFileName);

            MessageBox.Show("Output has been saved", "Info");
        }
        //--------------------------------------------------------------------------------------------

        //------------------------------------------Events--------------------------------------------
        private void colorModeComboBox_DropDownClosed(object sender, EventArgs e)
        {
            string model = colorModelComboBox.Text;
            if (model == "YCbCr")
                colorModel = ColorModel.YCbCr;
            else if (model == "HSV")
                colorModel = ColorModel.HSV;
            else if (model == "Lab")
                colorModel = ColorModel.Lab;
            else
                colorModel = ColorModel.none;
            ChangeLabels(colorModel);
        }
        private void colorProfileCombobox_DropDownClosed(object sender, EventArgs e)
        {
            string profil = colorProfileCombobox.Text;
            if(profil!="Własny")
            {
                if(profil == "sRGB")
                {
                    oldColorProfile = colorProfile;
                    colorProfile = new ColorProfile()
                    {
                        colorPoints = new List<Point>()
                        {
                            new Point(0.64, 0.33),
                            new Point(0.3, 0.6),
                            new Point(0.15,0.06)
                        },

                        whitePoint = new Point(0.31273, 0.32902),
                        gamma = 2.2,
                        name = "sRGB",
                        whitePointName = "D65"
                    };
                    illuminatCombobox.SelectedItem = D65;
                }
                else if (profil == "AdobeRGB")
                {
                    oldColorProfile = colorProfile;
                    colorProfile = new ColorProfile()
                    {
                        colorPoints = new List<Point>()
                        {
                            new Point(0.64, 0.33),
                            new Point(0.21, 0.71),
                            new Point(0.15,0.06)
                        },

                        whitePoint = new Point(0.31273, 0.32902),
                        gamma = 2.2,
                        name = "AdobeRGB",
                        whitePointName = "D65"
                    };
                    illuminatCombobox.SelectedItem = D65;
                }
                else if (profil == "AppleRGB")
                {
                    oldColorProfile = colorProfile;
                    colorProfile = new ColorProfile()
                    {
                        colorPoints = new List<Point>()
                        {
                            new Point(0.625, 0.34),
                            new Point(0.28, 0.595),
                            new Point(0.155,0.07)
                        },

                        whitePoint = new Point(0.31273, 0.32902),
                        gamma = 1.8,
                        name = "AppleRGB",
                        whitePointName = "D65"
                    };
                    illuminatCombobox.SelectedItem = D65;
                }
                else if (profil == "CIERGB")
                {
                    oldColorProfile = colorProfile;
                    colorProfile = new ColorProfile()
                    {
                        colorPoints = new List<Point>()
                        {
                            new Point(0.735, 0.265),
                            new Point(0.274, 0.007),
                            new Point(1.0/3.0,1.0/3.0)
                        },

                        whitePoint = new Point(1.0/3.0, 1.0/3.0),
                        gamma = 2.2,
                        name = "CIERGB",
                        whitePointName = "E"
                    };
                    illuminatCombobox.SelectedItem = E;
                }
                else if(profil=="WideGamut")
                {
                    oldColorProfile = colorProfile;
                    colorProfile = new ColorProfile()
                    {
                        colorPoints = new List<Point>()
                        {
                            new Point(0.7347, 0.2653),
                            new Point(0.1152, 0.8264),
                            new Point(0.1566,0.0177)
                        },
                        whitePoint = new Point(0.345670,0.3585),
                        gamma = 1.2,
                        name = "WideGamut",
                        whitePointName = "D50"
                    };
                    illuminatCombobox.SelectedItem = D50;
                }
                if (xRedTextBox != null && yRedTextBox != null && xGreenTextBox != null && yGreenTextBox != null && xBlueTextBox != null && yBlueTextBox != null && gammaTextBox!=null && illuminatCombobox!=null && applyButton!=null)
                {
                    LockColorTextBoxes();

                    illuminatCombobox.IsEnabled = false;

                    applyButton.IsEnabled = false;

                    FillTextboxes();
                }
                if(imageLoaded)
                {
                    oldColorProfiles.Push(oldColorProfile);
                    RefreshImage();  
                }
            }
            else
            {
                if (xRedTextBox != null && yRedTextBox != null && xGreenTextBox != null && yGreenTextBox != null && xBlueTextBox != null && yBlueTextBox != null && gammaTextBox != null && illuminatCombobox != null && applyButton!=null)
                {
                    UnlockColorTextBoxes();

                    illuminatCombobox.IsEnabled = true;

                    applyButton.IsEnabled = true;

                    myColorProfile.whitePoint = new Point(ConvetStringToDouble(xWhiteTextBox.Text), ConvetStringToDouble(yWhiteTextBox.Text));
                    myColorProfile.colorPoints = new List<Point>()
                    {
                        new Point(ConvetStringToDouble(xRedTextBox.Text), ConvetStringToDouble(yRedTextBox.Text)),
                        new Point(ConvetStringToDouble(xGreenTextBox.Text), ConvetStringToDouble(yGreenTextBox.Text)),
                        new Point(ConvetStringToDouble(xBlueTextBox.Text), ConvetStringToDouble(yBlueTextBox.Text))
                    };
                }
            }
            
        }
        private void IlluminatCombobox_DropDownClosed(object sender, EventArgs e)
        {
            string iluminat = illuminatCombobox.Text;
            if(iluminat!="Własna")
            {
                if (xWhiteTextBox != null && yWhiteTextBox != null)
                {
                    LockWhiteTextBoxes();
                }
                
                if (iluminat == "A")
                    colorProfile.whitePoint = new Point(0.44757, 0.40744);
                else if (iluminat == "B")
                    colorProfile.whitePoint = new Point(0.3484, 0.35160);
                else if (iluminat == "C")
                    colorProfile.whitePoint = new Point(0.31006, 0.31615);
                else if (iluminat == "D50")
                    colorProfile.whitePoint = new Point(0.34567, 0.35850);
                else if (iluminat == "D55")
                    colorProfile.whitePoint = new Point(0.33242, 0.34743);
                else if (iluminat == "D65")
                    colorProfile.whitePoint = new Point(0.31273, 0.32902);
                else if (iluminat == "D75")
                    colorProfile.whitePoint = new Point(0.29902, 0.31485);
                else if (iluminat == "9300K")
                    colorProfile.whitePoint = new Point(0.2848, 0.2932);
                else if (iluminat == "E")
                    colorProfile.whitePoint = new Point(0.33333, 0.33333);
                else if (iluminat == "F2")
                    colorProfile.whitePoint = new Point(0.37207, 0.37512);
                else if (iluminat == "F7")
                    colorProfile.whitePoint = new Point(0.31285, 0.32918);
                else if (iluminat == "F11")
                    colorProfile.whitePoint = new Point(0.38054, 0.37691);
                if (xWhiteTextBox != null && yWhiteTextBox != null)
                {
                    xWhiteTextBox.Text = colorProfile.whitePoint.X.ToString();
                    yWhiteTextBox.Text = colorProfile.whitePoint.Y.ToString();
                }
            }
            else
            {
                if(xWhiteTextBox!=null && yWhiteTextBox!=null)
                {
                    UnlockWhiteTextBoxes();
                    myColorProfile.whitePoint = new Point(ConvetStringToDouble(xWhiteTextBox.Text), ConvetStringToDouble(yWhiteTextBox.Text));
                }
            }
        }
        private void xWhiteTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            myColorProfile.whitePoint = new Point(ConvetStringToDouble(xWhiteTextBox.Text), myColorProfile.whitePoint.Y);
        }
        private void yWhiteTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            myColorProfile.whitePoint = new Point(myColorProfile.whitePoint.X,ConvetStringToDouble(yWhiteTextBox.Text));
        }
        private void xRedTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            myColorProfile.colorPoints[0] = new Point(ConvetStringToDouble(xRedTextBox.Text), myColorProfile.colorPoints[0].Y);
        }

        private void yRedTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            myColorProfile.colorPoints[0] = new Point(myColorProfile.colorPoints[0].X, ConvetStringToDouble(yRedTextBox.Text));
        }

        private void xGreenTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            myColorProfile.colorPoints[1] = new Point(ConvetStringToDouble(xGreenTextBox.Text), myColorProfile.colorPoints[1].Y);
        }

        private void yGreenTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            myColorProfile.colorPoints[1] = new Point(myColorProfile.colorPoints[1].X, ConvetStringToDouble(yGreenTextBox.Text));
        }

        private void xBlueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            myColorProfile.colorPoints[2] = new Point(ConvetStringToDouble(xBlueTextBox.Text), myColorProfile.colorPoints[2].Y);
        }

        private void yBlueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            myColorProfile.colorPoints[2] = new Point(myColorProfile.colorPoints[2].X, ConvetStringToDouble(yBlueTextBox.Text));
        }

        private void gammaTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            myColorProfile.gamma = ConvetStringToDouble(gammaTextBox.Text);
        }
        private void mainCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            int imageWidth = (int)mainCanvas.ActualWidth;
            int imageHeight = (int)mainCanvas.ActualHeight;
            mainBitmap = new System.Drawing.Bitmap(imageWidth, imageHeight);
            prevBitmap = new System.Drawing.Bitmap(imageWidth, imageHeight);
            firstBitmap = new System.Drawing.Bitmap(imageWidth, imageHeight);
            secondBitmap = new System.Drawing.Bitmap(imageWidth, imageHeight);
            thirdBitmap = new System.Drawing.Bitmap(imageWidth, imageHeight);
        }
        //------------------------------------------------------------------------------

        //-----------------------------Functions---------------------------------------
        void ChangeLabels(ColorModel cm)
        {
            switch(cm)
            {
                case ColorModel.YCbCr:
                    firstLabel.Content = "Y";
                    secondLabel.Content = "Cb";
                    thirdLabel.Content = "Cr";
                    break;
                case ColorModel.HSV:
                    firstLabel.Content = "H";
                    secondLabel.Content = "S";
                    thirdLabel.Content = "V";
                    break;
                case ColorModel.Lab:
                    firstLabel.Content = "L";
                    secondLabel.Content = "a";
                    thirdLabel.Content = "b";
                    break;
            }
        }
        void FillTextboxes()
        {
            xRedTextBox.Text = colorProfile.colorPoints[0].X.ToString();
            yRedTextBox.Text = colorProfile.colorPoints[0].Y.ToString();

            xGreenTextBox.Text = colorProfile.colorPoints[1].X.ToString();
            yGreenTextBox.Text = colorProfile.colorPoints[1].Y.ToString();

            xBlueTextBox.Text = colorProfile.colorPoints[2].X.ToString();
            yBlueTextBox.Text = colorProfile.colorPoints[2].Y.ToString();

            xWhiteTextBox.Text = colorProfile.whitePoint.X.ToString();
            yWhiteTextBox.Text = colorProfile.whitePoint.Y.ToString();

            gammaTextBox.Text = colorProfile.gamma.ToString();
        }
        void LockColorTextBoxes()
        {
            xRedTextBox.IsEnabled = false;
            yRedTextBox.IsEnabled = false;

            xGreenTextBox.IsEnabled = false;
            yGreenTextBox.IsEnabled = false;

            xBlueTextBox.IsEnabled = false;
            yBlueTextBox.IsEnabled = false;

            gammaTextBox.IsEnabled = false;
        }
        void UnlockColorTextBoxes()
        {
            xRedTextBox.IsEnabled = true;
            yRedTextBox.IsEnabled = true;

            xGreenTextBox.IsEnabled = true;
            yGreenTextBox.IsEnabled = true;

            xBlueTextBox.IsEnabled = true;
            yBlueTextBox.IsEnabled = true;

            gammaTextBox.IsEnabled = true;
        }
        void LockWhiteTextBoxes()
        {
            xWhiteTextBox.IsEnabled = false;
            yWhiteTextBox.IsEnabled = false;
        }
        void UnlockWhiteTextBoxes()
        {
            xWhiteTextBox.IsEnabled = true;
            yWhiteTextBox.IsEnabled = true;
        }
        void RefreshImage()
        {
            int imageWidth = (int)mainCanvas.ActualWidth;
            int imageHeight = (int)mainCanvas.ActualHeight;
            prevBitmap = new System.Drawing.Bitmap(imageWidth, imageHeight);
            BmpPixelSnoop snoop = new BmpPixelSnoop(mainBitmap);
            BmpPixelSnoop snoop2 = new BmpPixelSnoop(prevBitmap);

            ColorConvert convert = new ColorConvert();

            for (int i = 0; i < imageWidth; i++)
                for (int j = 0; j < imageHeight; j++)
                {
                    ColorConvert colorConvert = new ColorConvert();
                    System.Drawing.Color color = snoop.GetPixel(i, j);
                    snoop2.SetPixel(i, j, color);
                    Macierz XYZ = colorConvert.ConvertRGBToXYZ(oldColorProfile, oldColorProfile.whitePoint, oldColorProfile.gamma, color);
                    color = colorConvert.ConvertXYZToRGB(colorProfile, colorProfile.whitePoint, colorProfile.gamma, XYZ);
                    snoop.SetPixel(i, j, color);
                }
            snoop.Dispose();
            snoop2.Dispose();
            prevBitmaps.Push(prevBitmap);
            mainImage.Source = BmpImageFromBmp(mainBitmap);
        }
        void PreviousImage()
        {
            int imageWidth = (int)mainCanvas.ActualWidth;
            int imageHeight = (int)mainCanvas.ActualHeight;

            System.Drawing.Bitmap _prevBitmap = prevBitmaps.Pop();

            BmpPixelSnoop snoop = new BmpPixelSnoop(mainBitmap);
            BmpPixelSnoop snoop2 = new BmpPixelSnoop(_prevBitmap);

            ColorConvert convert = new ColorConvert();

            for (int i = 0; i < imageWidth; i++)
                for (int j = 0; j < imageHeight; j++)
                {
                    System.Drawing.Color color = snoop2.GetPixel(i, j);
                    snoop.SetPixel(i, j, color);
                }
            snoop.Dispose();
            snoop2.Dispose();
            mainImage.Source = BmpImageFromBmp(mainBitmap);
        }
        double ConvetStringToDouble(string text)
        {
            double pom = 0;
            try
            {
                pom = Convert.ToDouble(text);
                applyButton.IsEnabled = true;
            }
            catch (FormatException)
            {
                MessageBox.Show($"Unable to convert '{text}' to a Double.\n Correct format: 0,xxxx","Error!");
                applyButton.IsEnabled = false;
            }
            catch (OverflowException)
            {
                MessageBox.Show($"'{text}' is outside the range of a Double.","Error!");
                applyButton.IsEnabled = false;
            }
            return pom;
        }
        private BitmapImage BmpImageFromBmp(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Jpeg);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
        public System.Drawing.Bitmap zmienRozmiarObrazu(System.Drawing.Bitmap bitmapa, int nowaSzerokosc, int nowaWysokosc)
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

        void Save(BitmapImage image, string filePath)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }
        //----------------------------------------------------------------------------------------
    }
}
