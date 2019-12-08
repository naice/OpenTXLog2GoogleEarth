using System.IO;

namespace OpenTXLog2GoogleEarthConvert
{
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
}
