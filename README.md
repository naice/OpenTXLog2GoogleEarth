# OpenTXLog2GoogleEarth
Converts OpenTX log files (with gps data) to GoogleEarth (kml file)

# Usage
Install Google Earth Pro from here: https://www.google.de/earth/download/gep/agree.html

You need to configure the application first in order to use it correctly. Configuration is done via config.json

```JSON
{
  "PathColor": "991081f4",
  "AltitudeMode": "relativeToGround",
  "AltitudeOffset": 0.0,
  "GoogleEarthExe": "C:\\Program Files\\Google\\Google Earth Pro\\client\\googleearth.exe",
  "GPSHeader": "GPS",
  "GPSSpeedHeader": "GSpd(kmh)",
  "AltitudeHeader": "Alt(m)",
  "TimeHeader": "Time",
  "DateHeader": "Date"
}
```

 * **PathColor** currently unused
 * **AltitudeMode** sets the mode of altitude for your GPS Path, absolut / relativeToGround
 * **AltitudeOffset** will add the given amount to the alititude readings of your log
 * **GoogleEarthExe** path to your google earth executable, will autolaunch google earth after conversion when set
 * **Headers** The exact representation of your CSV-File Headers for the given data, in case this changes in future
 
 If your configuration is done, run the application once as administrator, it will automatically bind the *.csv extension to it so you just can rightclick (or doubleclick) and select "Open in Google Earth". 
 
 The application stores the converted files in its application path.

# Nerds
The first input parameter is the CSV File to convert. I haven't had time to make a 2nd parameter for the output file, but you are welcome to make a pull request. 

# Roadmap
 * Move to XML-Serialization 
 * Extend command line
 * Error handling
 
# Discord
For questions, bugs and help: https://discord.gg/Qb99yZ
