namespace OpenTXLog2GoogleEarth
{
    public class Configuration
    {
        public string PathColor { get; set; } = "991081f4";
        public string AltitudeMode { get; set; } = "relativeToGround";
        public float AltitudeOffset { get; set; } = 0;
        public string GoogleEarthExe { get; set; } = @"C:\Program Files\Google\Google Earth Pro\client\googleearth.exe";
        public string GPSHeader { get; set; } = "GPS";
        public string GPSSpeedHeader { get; set; } = "GSpd(kmh)";
        /// <summary>
        /// The value from gps speed header will be multiplied by this factor. 
        /// KMH = 1
        /// NODE (KNOTEN) = 1.852
        /// MPH = 1.609344
        /// </summary>
        public decimal GPSSpeedFactor { get; set; } = 1m;
        /// <summary>
        /// Label for speed output.
        /// </summary>
        public string GPSSpeedLabel { get; set; } = "kmh";
        public string AltitudeHeader { get; set; } = "Alt(m)";
        public string TimeHeader { get; set; } = "Time";
        public string DateHeader { get; set; } = "Date";
    }
}
