using System.Diagnostics;

namespace OpenTXLog2GoogleEarthConvert
{
    [DebuggerDisplay("{ARGBString}")]
    public class ColorHLS
    {
        public int A { get; set; }
        public double H { get; set; }
        public double L { get; set; }
        public double S { get; set; }

        public ColorHLS()
        {

        }

        public ColorHLS(int a, double h, double l, double s)
        {
            A = a; H = h; L = l; S = s;
        }

        public string ARGBString
            => this.HlsToRgb().ARGBString;
    }
}
