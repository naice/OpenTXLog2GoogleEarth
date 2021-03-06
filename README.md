# OpenTXLog2GoogleEarth
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=KXGFDMWVS8E66)
Converts OpenTX log files (with gps data) to GoogleEarth (kml file)

![Preview](https://github.com/naice/OpenTXLog2GoogleEarth/blob/master/Media/Google%20Earth%20Pro.png)

# Google Earth
Install Google Earth Pro from here: https://www.google.de/earth/download/gep/agree.html

# Usage
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
 
# Configure OpenTX
In order to get your OpenTX trasmitter to actually log your flight, you need to tell it to do so. 
On page **SPECIAL FUNCTIONS** on your transmitter you would need to setup a sd log. My craft arms with switch SC so I created a **trigger** on switch **SC** that starts logging to **SD Log**.

![Special Functions Sample](https://github.com/naice/OpenTXLog2GoogleEarth/blob/master/Media/special-functions-log.jpg)

First entry is the trigger **SC** switch second entry is the action **SD Log** and the thrid entry is the delay of logging **0.2s**.

# Nerds
The first input parameter is the CSV File to convert. I haven't had time to make a 2nd parameter for the output file, but you are welcome to make a pull request. 

# Roadmap
 * Move to XML-Serialization 
 * Extend command line
 * Error handling
 
# Discord
For questions, bugs and help: https://discord.gg/Qb99yZ
