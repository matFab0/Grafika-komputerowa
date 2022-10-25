using System;
using System.Drawing;
using System.Windows;

namespace ColorExtractor
{
    class ColorConvert
    {
        
        public Macierz ConvertxyYToXYZ(System.Windows.Point p)
        {
            Macierz pom = new Macierz(1, 3);
            double z = 1 - p.X - p.Y;
            pom.tab[0, 1] = 1; //Y
            pom.tab[0, 0] = p.X / p.Y; //X
            pom.tab[0, 2] = z / p.Y; //Z

            return pom;
        }

        public Macierz ConvertRGBToXYZ(ColorProfile colorProfile,System.Windows.Point whitePoint,double gamma, Color color)
        {
            Macierz[] transforms = MakeTransformMatrixes(colorProfile, whitePoint);
            Macierz RGB = new Macierz(1, 3);
            RGB.tab[0,0] = (double)color.R / 255;
            RGB.tab[0,1] = (double)color.G / 255;
            RGB.tab[0,2] = (double)color.B / 255;

            RGB.Power(gamma);

            Macierz XYZ = RGB.Multiply1x3(RGB, transforms[0]);

            return XYZ;

        }

        public Color ConvertXYZToRGB(ColorProfile colorProfile, System.Windows.Point whitePoint, double gamma, Macierz XYZ)
        {
            Macierz[] transforms = MakeTransformMatrixes(colorProfile, whitePoint);
            Macierz RGB = XYZ.Multiply1x3(XYZ, transforms[1]);
            RGB.Power(1.0 / gamma);

            int r = (int)(RGB.tab[0, 0] * 255);
            int g = (int)(RGB.tab[0, 1] * 255);
            int b = (int)(RGB.tab[0, 2] * 255);

            LimitTo0_255(ref r, ref g, ref b);

            return Color.FromArgb(r, g, b);
        }

        public Macierz[] MakeTransformMatrixes(ColorProfile colorProfile, System.Windows.Point whitePoint)
        {
            Macierz colorProfileMatrix = new Macierz(3, 3);
            for(int i=0;i<3;i++)
            {
                colorProfileMatrix.tab[0, i] = colorProfile.colorPoints[i].X;
                colorProfileMatrix.tab[1, i] = colorProfile.colorPoints[i].Y;
                colorProfileMatrix.tab[2, i] = 1 - colorProfile.colorPoints[i].X - colorProfile.colorPoints[i].Y;
            }
            
            Macierz XwYwZw = ConvertxyYToXYZ(whitePoint);
            Macierz colorProfileMatrixInverse = new Macierz();
            try
            {
                colorProfileMatrixInverse = colorProfileMatrix.Inverse3x3();
            }catch(IrreversibleMatrixException)
            {
                MessageBox.Show("Matrix cannot be inverted","Error!");
            }
            
            

            Macierz SrSgSb = XwYwZw.Multiply1x3(XwYwZw,colorProfileMatrixInverse);

            Macierz RGBToXYZ = new Macierz(3, 3);
            for(int i=0;i<3;i++)
            {
                for(int j=0;j<3;j++)
                    RGBToXYZ.tab[j, i] = colorProfileMatrix.tab[j, i] * SrSgSb.tab[0,i];
            }

            Macierz XYZToRGB = RGBToXYZ.Inverse3x3();

            Macierz[] pom = new Macierz[2];
            pom[0] = RGBToXYZ;
            pom[1] = XYZToRGB;

            return pom;
        }

        public Color[] ConvertRGBToLab(Color color, ColorProfile colorProfile, System.Windows.Point whitePoint, double gamma)
        {
            Macierz XYZ = ConvertRGBToXYZ(colorProfile, whitePoint, gamma, color);
            Macierz XwYwZw = ConvertxyYToXYZ(whitePoint);

            double dx = XYZ.tab[0, 0] / XwYwZw.tab[0,0];
            double dy = XYZ.tab[0, 1] / XwYwZw.tab[0, 1];
            double dz = XYZ.tab[0, 2] / XwYwZw.tab[0, 2];
            double L;
            if (dy > 0.008856)
                L = 116 * Math.Pow(dy, 1.0 / 3.0) - 16;
            else
                L = 903.3 * dy;
            double a = 500 * (Math.Pow(dx,1.0 / 3.0) - Math.Pow(dy,1.0 / 3.0));
            double b = 200 * (Math.Pow(dy, 1.0 / 3.0) - Math.Pow(dz, 1.0 / 3.0));

            int l = (int)(L / 100 * 255);
            int aR = (int)(a + 128);
            int aG = (int)Math.Abs(a - 128);
            int bR = (int)(b + 128);
            int bB = (int)Math.Abs(b - 128);
            LimitTo0_255(ref aR, ref aG, ref bR);
            if (bB > 255)
                bB = 255;
            if (bB < 0)
                bB = 0;
            Color[] colors = new Color[3];
            colors[0] = Color.FromArgb(l, l, l);
            colors[1] = Color.FromArgb(aR,aG,127);
            colors[2] = Color.FromArgb(bR, 127, bB);

            return colors;
        }

        public Color[] ConvertRGBToYCrCb(Color color)
        {
            double r = (double)color.R/255;
            double g = (double)color.G/255;
            double b = (double)color.B/255;

            double Y = 0.299 * r + 0.587 * g + 0.114 * b;
            double Cb = (b - Y) / 1.772 + 0.5;
            double Cr = (r - Y) / 1.402 + 0.5;

            LimitTo0_1(ref Y, ref Cb, ref Cr);

            Color[] colors = new Color[3];
            colors[0] = Color.FromArgb((int)(Y * 255), (int)(Y * 255), (int)(Y * 255));
            colors[1] = Color.FromArgb(127, (int)((1 - Cb) * 255), (int)(Cb * 255));
            colors[2] = Color.FromArgb((int)(Cr * 255), (int)((1 - Cr) * 255), 127);

            return colors;
        }

        public Color[] ConvertRGBToHSV(Color rgb) //https://www.programmingalgorithms.com/algorithm/rgb-to-hsv/
        {
            double delta, min;
            double h = 0, s, v;

            min = Math.Min(Math.Min(rgb.R, rgb.G), rgb.B);
            v = Math.Max(Math.Max(rgb.R, rgb.G), rgb.B);
            delta = v - min;

            if (v == 0.0)
                s = 0;
            else
                s = delta / v;
            
            if (s == 0)
                h = 0.0;
            else
            {
                if (rgb.R == v)
                    h = (rgb.G - rgb.B) / delta;
                else if (rgb.G == v)
                    h = 2 + (rgb.B - rgb.R) / delta;
                else if (rgb.B == v)
                    h = 4 + (rgb.R - rgb.G) / delta;

                h *= 60;

                if (h < 0.0)
                    h = h + 360;
            }
            int H = (int)(h / 360 * 255);
            int S = (int)(s * 255);
            int V = (int)v;
            
            Color[] colors = new Color[3];
            colors[0] = Color.FromArgb(H, H, H);
            colors[1] = Color.FromArgb(S, S, S);
            colors[2] = Color.FromArgb(V, V, V);
            
            return colors;
        }

        void LimitTo0_1(ref double r,ref double g, ref double b)
        {
            if (r < 0)
                r = 0;
            if (r < 0)
                r = 0;
            if (g > 1)
                g = 1;
            if (g > 1)
                g = 1;
            if (b > 1)
                b = 1;
            if (b > 1)
                b = 1;
        }

        void LimitTo0_255(ref int r,ref int g,ref int b)
        {
            if (r < 0)
                r = 0;
            if (r > 255)
                r = 255;
            if (g < 0)
                g = 0;
            if (g > 255)
                g = 255;
            if (b < 0)
                b = 0;
            if (b > 255)
                b = 255;
        }
    }
}
