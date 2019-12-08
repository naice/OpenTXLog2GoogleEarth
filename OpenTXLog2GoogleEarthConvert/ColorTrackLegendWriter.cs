using System.IO;

namespace OpenTXLog2GoogleEarthConvert
{
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
}
