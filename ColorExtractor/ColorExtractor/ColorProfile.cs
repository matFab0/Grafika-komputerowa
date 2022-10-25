using System.Collections.Generic;
using System.Windows;

namespace ColorExtractor
{
    class ColorProfile
    {
        public List<Point> colorPoints { get; set; }
        public Point whitePoint { get; set; }
        public double gamma { get; set; }
        public string name { get; set; }
        public string whitePointName { get; set; }
    }
}
