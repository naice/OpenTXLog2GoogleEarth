
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenTXLog2GoogleEarthConvert
{
    public class ConverterOptions
    {
        /// <summary>
        /// Have to begin with the CSV header.
        /// </summary>
        public IEnumerable<string> LogLines { get; set; }
        /// <summary>
        /// The output <see cref="Stream"/> for the KML-Data. 
        /// </summary>
        public Stream OutputStream { get; set; }
        /// <summary>
        /// The parsed Altitude will be offset by this value.
        /// </summary>
        public float AltitudeOffset { get; set; }
        
        /// <summary>
        /// Name of the craft
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Log file name
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// relativeToGround, absolute or relativeToSeaFloor
        /// </summary>
        public string AltitudeMode { get; set; }
        /// <summary>
        /// CSV File header for GPS
        /// </summary>
        public string GPSHeader { get; set; } = "GPS";
        /// <summary>
        /// CSV File header for GPS Speed
        /// </summary>
        public string GPSSpeedHeader { get; set; } = "GSpd(kmh)";
        /// <summary>
        /// CSV File header for altitude
        /// </summary>
        public string AltitudeHeader { get; set; } = "Alt(m)";
        /// <summary>
        /// CSV File header for Time
        /// </summary>
        public string TimeHeader { get; set; } = "Time";
        /// <summary>
        /// CSV File header for Date
        /// </summary>
        public string DateHeader { get; set; } = "Date";
        /// <summary>
        /// The color of the path in google earth.
        /// </summary>
        public string PathColor { get; set; } = "991081f4";
    }
}
