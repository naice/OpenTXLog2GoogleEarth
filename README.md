# OpenTXLog2GoogleEarth
Converts OpenTX log files (with gps data) to GoogleEarth (kml file)

# Usage
You need to configure the application first in order to use it correctly. Configuration is done via config.json
 * AltitudeMode sets the mode of altitude for your GPS Path, absolut / relativeToGround
 * AltitudeOffset will add the given amount to the alititude readings of your log
 * GoogleEarthExe path to your google earth executable, will autolaunch google earth after conversion when set
 
 If your configuration is done, run the application once as administrator, it will automatically bind the *.csv extension to it so you just can rightclick (or doubleclick) and select "Open in Google Earth". 
 
 The application stores the converted files in its application path.
