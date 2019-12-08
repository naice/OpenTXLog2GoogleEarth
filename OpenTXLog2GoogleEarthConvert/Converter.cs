using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Globalization;

namespace OpenTXLog2GoogleEarthConvert
{
    public class SpeedData
    {
        public double MaxSpeed { get; set; } = 0;
        public Dictionary<int, ColorRGB> SpeedColors { get; set; } = new Dictionary<int, ColorRGB>();
        public List<LegendData> SpeedLegend { get; set; } = new List<LegendData>();
    }

    public partial class Converter
    {
        private readonly ConverterOptions _options;
        private readonly Lazy<LogData[]> _logData;
        private readonly Lazy<LogHeader> _logHeader;
        private readonly Lazy<SpeedData> _speedData;

        public Converter(ConverterOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logData = new Lazy<LogData[]>(() => ParseLogData().ToArray());
            _logHeader = new Lazy<LogHeader>(() => ParseLogHeader());
            _speedData = new Lazy<SpeedData>(() => ParseSpeedData());
        }

        public void Convert()
        {
            // Invariant culture for parsing / converting operations
            var currentCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            var speedData = _speedData.Value;

            using (StreamWriter sw = new StreamWriter(_options.OutputStream))
            {
                StringBuilder colorSection = new StringBuilder();
                if (speedData.SpeedColors != null && speedData.SpeedColors.Count > 0)
                {
                    foreach (var speedColor in speedData.SpeedColors)
                    {
                        colorSection.AppendLine(Constants.KML_COLORSECTION
                            .Replace("{SPEED}", speedColor.Key.ToString())
                            .Replace("{COLOR}", speedColor.Value.ABGRString));
                    }
                }

                // Write header
                sw.WriteLine(Constants.KML_HEADER
                    .Replace("{COLORSECTION}", colorSection.ToString())
                    .Replace("{NAME}", _options.Name)
                    .Replace("{FULLNAME}", _options.FullName)
                    .Replace("{ALT_MODE}", _options.AltitudeMode)
                    .Replace("{COLOR}", _options.PathColor)
                );

                // Write legend
                sw.WriteLine($"<Folder id=\"Legend\"><name>Legend: Speed ({_options.GPSSpeedLabel})</name><visibility>1</visibility><open>1</open>");
                foreach (var legend in speedData.SpeedLegend)
                {
                    sw.WriteLine($"<Placemark>");
                    sw.WriteLine($"<name><![CDATA[<span style=\"color:{legend.FromColor.WebColor};\"><b>{legend.FromSpeed:0.00}</b></span> - <span style=\"color:{legend.ToColor.WebColor};\"><b>{legend.ToSpeed:0.00}</b></span>]]></name>");
                    sw.WriteLine($"<visibility>1</visibility><styleUrl>#speed_legend</styleUrl></Placemark>");
                }
                sw.WriteLine($"</Folder>");

                // write speedlayer
                double? last_lat = null;
                double? last_lon = null;
                float? last_alt = null;
                sw.WriteLine(@"<Folder><name>SpeedLayer</name><visibility>1</visibility><open>0</open>");
                foreach (var data in _logData.Value)
                {
                    if (last_lat == null || last_lon == null || last_alt == null)
                    {
                        last_lat = data.Latitude;
                        last_lon = data.Longitude;
                        last_alt = data.Alt;
                        continue;
                    }

                    var color = speedData.SpeedColors[(int)data.Speed];
                    sw.WriteLine($"<Placemark>");
                    sw.WriteLine($"<name><![CDATA[<span style=\"color:{color.WebColor}\">{data.Speed:0.00} {_options.GPSSpeedLabel}</span>]]></name>");
                    sw.WriteLine($"<styleUrl>#colorSpeedTrack{(int)data.Speed}</styleUrl>");
                    sw.WriteLine($"<LineString>");
                    sw.WriteLine($"<tessellate>1</tessellate>");
                    sw.WriteLine($"<altitudeMode>{_options.AltitudeMode}</altitudeMode>");
                    sw.WriteLine($"<coordinates>{last_lon:0.00000000},{last_lat:0.00000000},{last_alt:0.00} {data.Longitude:0.00000000},{data.Latitude:0.00000000},{data.Alt:0.00}</coordinates>");
                    sw.WriteLine($"</LineString>");
                    sw.WriteLine($"</Placemark>");

                    last_lat = data.Latitude;
                    last_lon = data.Longitude;
                    last_alt = data.Alt;
                }
                sw.WriteLine(@"</Folder>");

                // write playback track (google earth extension)
                sw.WriteLine($"<Folder><name>Flight max. {speedData.MaxSpeed:0.00}{_options.GPSSpeedLabel}</name><visibility>1</visibility><open>0</open>");
                sw.WriteLine($"<Placemark><name>{_options.Name}</name><styleUrl>#multiTrack</styleUrl><gx:Track><altitudeMode>{_options.AltitudeMode}</altitudeMode>");
                foreach (var data in _logData.Value)
                {
                    sw.WriteLine($"<when>{data.TimeStamp:yyyy-MM-ddTHH:mm:ss.FFFZ}</when>");
                    sw.WriteLine($"<gx:coord>{data.Longitude:0.00000000},{data.Latitude:0.00000000},{data.Alt:0.00}</gx:coord>");
                }
                sw.WriteLine(@"</gx:Track></Placemark></Folder>");

                // write footer
                sw.WriteLine(Constants.KML_FOOTER);
            }

            CultureInfo.CurrentCulture = currentCulture;
        }

        private LogHeader ParseLogHeader()
        {
            var header = _options.LogLines.First().Split(',');
            var getIndexOf = new Func<string, int>((key) => {
                var lk = key.ToLowerInvariant();
                for (int i = 0; i < header.Length; i++)
                {
                    if (header[i].ToLowerInvariant() == lk)
                        return i;
                }

                throw new Exception($"Log does not contain required CSV header '{key}'");
            });
            int gpsIndex = getIndexOf(_options.GPSHeader);
            int altitudeIndex = getIndexOf(_options.AltitudeHeader);
            int gpsSpeedIndex = getIndexOf(_options.GPSSpeedHeader);
            int timeIndex = getIndexOf(_options.TimeHeader);
            int dateIndex = getIndexOf(_options.DateHeader);

            return new LogHeader()
            {
                AltitudeIndex = altitudeIndex,
                DateIndex = dateIndex,
                GPIndex = gpsIndex,
                GPSSpeedIndex = gpsSpeedIndex,
                TimeIndex = timeIndex,
            };
        }

        private IEnumerable<LogData> ParseLogData()
        {
            var header = _logHeader.Value;
            if (header == null)
                yield break;

            LogData last = null;
            foreach (var line in _options.LogLines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var data = line.Split(',');

                if (string.IsNullOrWhiteSpace(data[header.DateIndex]) ||
                    string.IsNullOrWhiteSpace(data[header.TimeIndex]) ||
                    string.IsNullOrWhiteSpace(data[header.GPIndex]) ||
                    string.IsNullOrWhiteSpace(data[header.GPSSpeedIndex]) ||
                    string.IsNullOrWhiteSpace(data[header.AltitudeIndex]))
                    continue;
                var timeStamp =
                    DateTime.Parse(data[header.DateIndex])
                        .AddTicks(DateTime.Parse(data[header.TimeIndex]).Ticks);

                var alt = float.Parse(data[header.AltitudeIndex]) + _options.AltitudeOffset;
                var spd = ParseSpeed(data[header.GPSSpeedIndex]) * _options.GPSSpeedFactor;
                var gps = data[header.GPIndex].Split(' ');
                var lat = double.Parse(gps[0]);
                var lon = double.Parse(gps[1]);

                var logData = new LogData() { Alt = alt, Speed = spd, Latitude = lat, Longitude = lon, TimeStamp = timeStamp };

                // sanity check for GPS Data
                if (!GPSSanityCheck(last, logData))
                    continue;
                    
                yield return last = logData;
            }
        }

        private SpeedData ParseSpeedData()
        {
            SpeedData speedData = new SpeedData();
            var makeSpeedHue = new Func<double, double, double>((spd, maxSpd) => {
                double speedPercent = 0.34d;
                if (spd > 0)
                    speedPercent = ((spd / maxSpd) * -0.34d) + 0.34d;
                return speedPercent;
            });
            var speeds = _logData.Value.Select(d => d.Speed);
            var currentCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentCulture = currentCulture;
            var minSpeed = speeds.Min();
            var maxSpeed = speedData.MaxSpeed = speeds.Max();
            var rangeSpeed = maxSpeed - minSpeed;
            var legendStepCount = 5;
            var legendSteps = rangeSpeed / legendStepCount;

            for (int i = 0; i < legendStepCount; i++)
            {
                var fromSpeed = i * legendSteps;
                var toSpeed = (i + 1) * legendSteps;

                speedData.SpeedLegend.Add(new LegendData()
                {
                    FromSpeed = fromSpeed,
                    ToSpeed = toSpeed,
                    FromColor = new ColorHLS(180, makeSpeedHue(fromSpeed, maxSpeed), 0.5, 1).HlsToRgb(),
                    ToColor = new ColorHLS(180, makeSpeedHue(toSpeed, maxSpeed), 0.5, 1).HlsToRgb(),
                });
            }

            foreach (var speed in speeds)
            {
                var abs = (int)Math.Abs(speed);
                double speedPercent = 0.34d;
                if (speed > 0)
                    speedPercent = ((speed / maxSpeed) * -0.34d) + 0.34d;
                speedData.SpeedColors[abs] = new ColorHLS(180, makeSpeedHue(speed, maxSpeed), 0.5, 1).HlsToRgb();
            }

            return speedData;
        }

        private static bool GPSSanityCheck(LogData was, LogData now)
        {
            // assume start of an iteration
            if (now == null || was == null) return true;

            var seconds = (now.TimeStamp - was.TimeStamp).TotalSeconds;
            seconds = seconds <= 0 ? 0.1 : seconds;

            var altMeter = Math.Abs(was.Alt - now.Alt);
            var altMeterPerSeconds = altMeter / seconds;
            if (altMeterPerSeconds > 200)
                return false;

            var meter = was.DistanceTo(now) * 1000;
            var meterPerSecond = meter / seconds;
            if (meterPerSecond > 138)
                return false;

            return true;
        }

        private static double ParseSpeed(string input)
        {
            double.TryParse(input, out var spd);

            if (spd < 0)
                spd = 0;
            
            return spd;
        }
    }
}
