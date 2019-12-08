using System.Collections.Generic;
using System.IO;

namespace OpenTXLog2GoogleEarthConvert
{
    public class ColorTrackWriter
    {
        private readonly StreamWriter _streamWriter;
        private readonly ConverterOptions _options;
        private readonly IDictionary<int, ColorRGB> _trackColorData;
        private readonly IEnumerable<LogData> _logData;

        public ColorTrackWriter(StreamWriter streamWriter, ConverterOptions options, IDictionary<int, ColorRGB> trackColorData, IEnumerable<LogData> logData)
        {
            _streamWriter = streamWriter;
            _options = options;
            _trackColorData = trackColorData;
            _logData = logData;
        }

        public void Write()
        {
            double? last_lat = null;
            double? last_lon = null;
            float? last_alt = null;
            using (var folder = new FolderWriter(_streamWriter, "SpeedLayer", true, false))
            {
                folder.Open();
                foreach (var data in _logData)
                {
                    if (last_lat == null || last_lon == null || last_alt == null)
                    {
                        last_lat = data.Latitude;
                        last_lon = data.Longitude;
                        last_alt = data.Alt;
                        continue;
                    }

                    var color = _trackColorData[(int)data.Speed];
                    _streamWriter.WriteLine($"<Placemark>");
                    _streamWriter.WriteLine($"<name><![CDATA[<span style=\"color:{color.WebColor}\">{data.Speed:0.00} {_options.GPSSpeedLabel}</span>]]></name>");
                    _streamWriter.WriteLine($"<styleUrl>#colorSpeedTrack{(int)data.Speed}</styleUrl>");
                    _streamWriter.WriteLine($"<LineString>");
                    _streamWriter.WriteLine($"<tessellate>1</tessellate>");
                    _streamWriter.WriteLine($"<altitudeMode>{_options.AltitudeMode}</altitudeMode>");
                    _streamWriter.WriteLine($"<coordinates>{last_lon:0.00000000},{last_lat:0.00000000},{last_alt:0.00} {data.Longitude:0.00000000},{data.Latitude:0.00000000},{data.Alt:0.00}</coordinates>");
                    _streamWriter.WriteLine($"</LineString>");
                    _streamWriter.WriteLine($"</Placemark>");

                    last_lat = data.Latitude;
                    last_lon = data.Longitude;
                    last_alt = data.Alt;
                }
            }
        }
    }
}
