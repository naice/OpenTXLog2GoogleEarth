using System.Diagnostics;

namespace OpenTXLog2GoogleEarthConvert
{
    [DebuggerDisplay("{ARGBString}")]
    public class ColorRGB
    {
        public int A { get; set; }
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public string WebColor
            => $"#{R:X2}{G:X2}{B:X2}";
        public string ARGBString
            => $"{A:X2}{R:X2}{G:X2}{B:X2}";
        public string ABGRString
            => $"{A:X2}{B:X2}{G:X2}{R:X2}";
    }
}
