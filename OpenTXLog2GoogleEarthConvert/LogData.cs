using System;

namespace OpenTXLog2GoogleEarthConvert
{
    public class LogData : Coordinates
    {
        public float Alt { get; set; }
        public double Speed { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
