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
            A = a; H = KeepInRange(h); L = KeepInRange(l); S = KeepInRange(s);
        }

        private static double KeepInRange(double a)
            => a < 0 ? 0 : a > 1 ? 1 : a;

        public string ARGBString
            => this.HlsToRgb().ARGBString;
    }
}
