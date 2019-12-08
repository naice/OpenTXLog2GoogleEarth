using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenTXLog2GoogleEarthConvert
{
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
