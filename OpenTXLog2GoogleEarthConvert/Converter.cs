using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Globalization;

namespace OpenTXLog2GoogleEarthConvert
{
    public class Converter
    {
        private readonly ConverterOptions _options;

        private int _gpsIndex = 0;
        private int _altitudeIndex = 0;
        private int _gpsSpeedIndex = 0;
        private int _timeIndex = 0;
        private int _dateIndex = 0;
        private decimal _maxSpeed = 0;

        private Dictionary<int, ColorRGB> _speedColors = new Dictionary<int, ColorRGB>();
        private List<LegendData> _speedLegend = new List<LegendData>();

        public Converter(ConverterOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        private class LegendData
        {
            public decimal FromSpeed { get; set; }
            public decimal ToSpeed { get; set; }
            public ColorRGB FromColor { get; set; }
            public ColorRGB ToColor { get; set; }
        }

        private void ParseHeader()
        {
            _speedColors.Clear();
            _speedLegend.Clear();
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
            var makeSpeedHue = new Func<decimal, decimal, double>((spd, maxSpd) => {
                decimal speedPercent = 0.34m;
                if (spd > 0)
                    speedPercent = ((spd / maxSpd) * -0.34m) + 0.34m;
                return (double)speedPercent;
            });

            _gpsIndex = getIndexOf(_options.GPSHeader);
            _altitudeIndex = getIndexOf(_options.AltitudeHeader);
            _gpsSpeedIndex = getIndexOf(_options.GPSSpeedHeader);
            _timeIndex = getIndexOf(_options.TimeHeader);
            _dateIndex = getIndexOf(_options.DateHeader);

            var currentCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            var speeds = _options.LogLines.Skip(1).Select(line => decimal.Parse(line.Split(',')[_gpsSpeedIndex]) * _options.GPSSpeedFactor).ToArray();
            CultureInfo.CurrentCulture = currentCulture;
            var minSpeed = speeds.Min();
            var maxSpeed = _maxSpeed = speeds.Max();
            var rangeSpeed = maxSpeed - minSpeed;
            var legendStepCount = 5;
            var legendSteps = rangeSpeed / legendStepCount;

            for (int i = 0; i < legendStepCount; i++)
            {
                var fromSpeed = i * legendSteps;
                var toSpeed = (i + 1) * legendSteps;

                _speedLegend.Add(new LegendData()
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
                decimal speedPercent = 0.34m;
                if (speed > 0)
                    speedPercent = ((speed / maxSpeed) * -0.34m) + 0.34m;
                _speedColors[abs] = new ColorHLS(180, makeSpeedHue(speed,maxSpeed), 0.5, 1).HlsToRgb(); 
            }
        }

        public void Convert()
        {
            // Invariant culture for parsing / converting operations
            var currentCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            ParseHeader();
            using (StreamWriter sw = new StreamWriter(_options.OutputStream))
            {
                StringBuilder colorSection = new StringBuilder();
                if (_speedColors != null && _speedColors.Count > 0)
                {
                    foreach (var speedColor in _speedColors)
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
                foreach (var legend in _speedLegend)
                {
                    sw.WriteLine($"<Placemark>");
                    sw.WriteLine($"<name><![CDATA[<span style=\"color:{legend.FromColor.WebColor};\"><b>{legend.FromSpeed:0.00}</b></span> - <span style=\"color:{legend.ToColor.WebColor};\"><b>{legend.ToSpeed:0.00}</b></span>]]></name>");
                    sw.WriteLine($"<visibility>1</visibility><styleUrl>#speed_legend</styleUrl></Placemark>");
                }
                sw.WriteLine($"</Folder>");

                // write speedlayer
                decimal? last_lat = null;
                decimal? last_lon = null;
                float? last_alt = null;
                sw.WriteLine(@"<Folder><name>SpeedLayer</name><visibility>1</visibility><open>0</open>");
                foreach (var data in StreamLogData())
                {

                    if (last_lat == null || last_lon == null || last_alt == null)
                    {
                        last_lat = data.Lat;
                        last_lon = data.Lon;
                        last_alt = data.Alt;
                        continue;
                    }

                    var color = _speedColors[(int)data.Speed];
                    sw.WriteLine($"<Placemark>");
                    sw.WriteLine($"<name><![CDATA[<span style=\"color:{color.WebColor}\">{data.Speed:0.00} {_options.GPSSpeedLabel}</span>]]></name>");
                    sw.WriteLine($"<styleUrl>#colorSpeedTrack{(int)data.Speed}</styleUrl>");
                    sw.WriteLine($"<LineString>");
                    sw.WriteLine($"<tessellate>1</tessellate>");
                    sw.WriteLine($"<altitudeMode>{_options.AltitudeMode}</altitudeMode>");
                    sw.WriteLine($"<coordinates>{last_lon:0.00000000},{last_lat:0.00000000},{last_alt:0.00} {data.Lon:0.00000000},{data.Lat:0.00000000},{data.Alt:0.00}</coordinates>");
                    sw.WriteLine($"</LineString>");
                    sw.WriteLine($"</Placemark>");

                    last_lat = data.Lat;
                    last_lon = data.Lon;
                    last_alt = data.Alt;
                }
                sw.WriteLine(@"</Folder>");

                // write playback track (google earth extension)
                sw.WriteLine($"<Folder><name>Flight max. {_maxSpeed:0.00}{_options.GPSSpeedLabel}</name><visibility>1</visibility><open>0</open>");
                sw.WriteLine($"<Placemark><name>{_options.Name}</name><styleUrl>#multiTrack</styleUrl><gx:Track><altitudeMode>{_options.AltitudeMode}</altitudeMode>");
                foreach (var data in StreamLogData())
                {
                    sw.WriteLine($"<when>{data.TimeStamp:yyyy-MM-ddTHH:mm:ss.FFFZ}</when>");
                    sw.WriteLine($"<gx:coord>{data.Lon:0.00000000},{data.Lat:0.00000000},{data.Alt:0.00}</gx:coord>");
                }
                sw.WriteLine(@"</gx:Track></Placemark></Folder>");

                // write footer
                sw.WriteLine(Constants.KML_FOOTER);
            }

            CultureInfo.CurrentCulture = currentCulture;
        }

        private class LogData
        {
            public float Alt { get; set; }
            public decimal Lat { get; set; }
            public decimal Lon { get; set; }
            public decimal Speed { get; set; }
            public DateTime TimeStamp { get; set; }
        }

        private IEnumerable<LogData> StreamLogData()
        {
            foreach (var line in _options.LogLines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var data = line.Split(',');

                if (string.IsNullOrWhiteSpace(data[_dateIndex]) ||
                    string.IsNullOrWhiteSpace(data[_timeIndex]) ||
                    string.IsNullOrWhiteSpace(data[_gpsIndex]) ||
                    string.IsNullOrWhiteSpace(data[_gpsSpeedIndex]) ||
                    string.IsNullOrWhiteSpace(data[_altitudeIndex]))
                    continue;
                var timeStamp =
                    DateTime.Parse(data[_dateIndex])
                        .AddTicks(DateTime.Parse(data[_timeIndex]).Ticks);

                float alt = float.Parse(data[_altitudeIndex]) + _options.AltitudeOffset;
                decimal spd = decimal.Parse(data[_gpsSpeedIndex]) * _options.GPSSpeedFactor;
                var gps = data[_gpsIndex].Split(' ');
                decimal lat = decimal.Parse(gps[0]);
                decimal lon = decimal.Parse(gps[1]);

                yield return new LogData() { Alt = alt, Speed = spd, Lat = lat, Lon = lon, TimeStamp = timeStamp };
            }
        }
    }
}
