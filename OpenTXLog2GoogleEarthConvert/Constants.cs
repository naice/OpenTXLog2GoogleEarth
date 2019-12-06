using System;

namespace OpenTXLog2GoogleEarthConvert
{
    public class Constants
    {
        public const string KML_COLORSECTION = @"<Style id=""colorSpeedTrack{SPEED}"">
			<LineStyle>
				<color>{COLOR}</color>
				<width>6</width>
			</LineStyle>
		</Style>";

        public const string KML_HEADER = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"">
	<Document>
		<name>{FULLNAME}</name>
		<Style id=""multiTrack"">
			<IconStyle>
				<Icon>
					<href>track0.png</href>
				</Icon>
			</IconStyle>
			<LineStyle>
				<color>00FFFFFF</color>
				<width>6</width>
			</LineStyle>
		</Style>
        <Style id=""speed_legend"">
            <IconStyle>
                <Icon>
                    <href>speed.png</href>
                </Icon>
            </IconStyle>
        </Style>
        {COLORSECTION}";
        public const string KML_FOOTER = @"
	</Document>
</kml>";
        public const string KML_EXTENSION = ".kml";
    }
}
