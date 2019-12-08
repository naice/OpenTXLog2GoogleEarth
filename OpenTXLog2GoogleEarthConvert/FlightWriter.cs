using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenTXLog2GoogleEarthConvert
{
    public class FolderWriter : IDisposable
    {
        private readonly StreamWriter _streamWriter;
        private readonly string _name;
        private readonly bool _visibility;
        private readonly bool _open;

        private bool? _isClosed;

        public FolderWriter(StreamWriter streamWriter, string name) : this(streamWriter, name, true)
        {

        }
        public FolderWriter(StreamWriter streamWriter, string name, bool open) : this(streamWriter, name, true, open)
        {

        }
        public FolderWriter(StreamWriter streamWriter, string name, bool visibility, bool open)
        {
            _streamWriter = streamWriter ?? throw new ArgumentNullException(nameof(streamWriter));
            _name = name;
            _visibility = visibility;
            _open = open;
        }

        public void Open()
        {
            _streamWriter.WriteLine($"<Folder><name>{_name}</name><visibility>{(_visibility ? 1 : 0)}</visibility><open>{(_open ? 1 : 0)}</open>");
            _isClosed = false;
        }
        public void Close()
        {
            _streamWriter.WriteLine($"</Folder>");
            _isClosed = true;
        }

        public void Dispose()
        {
            if (_isClosed != null && !_isClosed.Value)
                Close();
        }
    }
    public class DocumentWriter : IDisposable
    {
        private readonly StreamWriter _streamWriter;
        private readonly string _name;
        private readonly bool _visibility;
        private readonly bool _open;
        private readonly string _snippet;
        private bool? _isClosed;

        public DocumentWriter(StreamWriter streamWriter, string name, string snippet) : this(streamWriter, name, snippet, true)
        {

        }
        public DocumentWriter(StreamWriter streamWriter, string name, string snippet, bool open) : this(streamWriter, name, snippet, true, open)
        {

        }
        public DocumentWriter(StreamWriter streamWriter, string name, string snippet, bool visibility, bool open)
        {
            _streamWriter = streamWriter ?? throw new ArgumentNullException(nameof(streamWriter));
            _name = name;
            _visibility = visibility;
            _open = open;
            _snippet = snippet;
        }

        public void Open()
        {
            _streamWriter.Write($"<Document>");
            _streamWriter.Write($"<name>{_name}</name>");
            _streamWriter.Write($"<visibility>{(_visibility ? 1 : 0)}</visibility>");
            _streamWriter.Write($"<open>{(_open ? 1 : 0)}</open>");
            _streamWriter.Write($"<Snippet>{_snippet}</Snippet>");
            _streamWriter.Write($"<Style id=\"multiTrack\">");
            _streamWriter.Write($"<IconStyle>");
            _streamWriter.Write($"<Icon>");
            _streamWriter.Write($"<href>track0.png</href>");
            _streamWriter.Write($"</Icon>");
            _streamWriter.Write($"</IconStyle>");
            _streamWriter.Write($"<LineStyle>");
            _streamWriter.Write($"<color>00FFFFFF</color>");
            _streamWriter.Write($"<width>6</width>");
            _streamWriter.Write($"</LineStyle>");
            _streamWriter.Write($"</Style>");
            _streamWriter.Write($"<Style id=\"speed_legend\">");
            _streamWriter.Write($"<IconStyle>");
            _streamWriter.Write($"<Icon>");
            _streamWriter.Write($"<href>speed.png</href>");
            _streamWriter.Write($"</Icon>");
            _streamWriter.Write($"</IconStyle>");
            _streamWriter.Write($"</Style>");
            _isClosed = false;
        }
        public void Close()
        {
            _streamWriter.WriteLine($"</Document>");
            _isClosed = true;
        }

        public void Dispose()
        {
            if (_isClosed != null && !_isClosed.Value)
                Close();
        }
    }

    public class ColorTrackLegendWriter
    {
        private readonly StreamWriter _streamWriter;
        private readonly ConverterOptions _options;
        private readonly SpeedData _speedData;

        public ColorTrackLegendWriter(StreamWriter streamWriter, ConverterOptions options, SpeedData speedData)
        {
            _streamWriter = streamWriter;
            _options = options;
            _speedData = speedData;
        }

        public void Write()
        {
            using (var folder = new FolderWriter(_streamWriter, $"Legend: Speed ({_options.GPSSpeedLabel})", true, true))
            {
                folder.Open();
                foreach (var legend in _speedData.SpeedLegend)
                {
                    _streamWriter.WriteLine($"<Placemark>");
                    _streamWriter.WriteLine($"<name><![CDATA[<span style=\"color:{legend.FromColor.WebColor};\"><b>{legend.FromSpeed:0.00}</b></span> - <span style=\"color:{legend.ToColor.WebColor};\"><b>{legend.ToSpeed:0.00}</b></span>]]></name>");
                    _streamWriter.WriteLine($"<visibility>1</visibility><styleUrl>#speed_legend</styleUrl></Placemark>");
                }
            }
        }
    }

    public class ColorTrackStyleWriter
    {
        private readonly StreamWriter _streamWriter;
        private readonly SpeedData _speedData;

        public ColorTrackStyleWriter(StreamWriter streamWriter, ConverterOptions options, SpeedData speedData)
        {
            _streamWriter = streamWriter;
            _speedData = speedData;
        }

        public void Write()
        {
            if (_speedData.SpeedColors != null && _speedData.SpeedColors.Count > 0)
            {
                foreach (var speedColor in _speedData.SpeedColors)
                {
                    _streamWriter.WriteLine(Constants.KML_COLORSECTION
                        .Replace("{SPEED}", speedColor.Key.ToString())
                        .Replace("{COLOR}", speedColor.Value.ABGRString));
                }
            }
        }
    }

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

    public class FlightWriter
    {
        private readonly StreamWriter _streamWriter;
        private readonly ConverterOptions _options;
        private readonly IEnumerable<LogData> _logData;

        public FlightWriter(StreamWriter streamWriter, ConverterOptions options, IEnumerable<LogData> logData)
        {
            _streamWriter = streamWriter;
            _options = options;
            _logData = logData;
        }

        public void Write()
        {
            using (var folder = new FolderWriter(_streamWriter, "Flight", false))
            {
                _streamWriter.WriteLine($"<Placemark><name>{_options.Name}</name><styleUrl>#multiTrack</styleUrl><gx:Track><altitudeMode>{_options.AltitudeMode}</altitudeMode>");
                foreach (var data in _logData)
                {
                    _streamWriter.WriteLine($"<when>{data.TimeStamp:yyyy-MM-ddTHH:mm:ss.FFFZ}</when>");
                    _streamWriter.WriteLine($"<gx:coord>{data.Longitude:0.00000000},{data.Latitude:0.00000000},{data.Alt:0.00}</gx:coord>");
                }
                _streamWriter.WriteLine(@"</gx:Track></Placemark>");
            }
        }
    }
}
