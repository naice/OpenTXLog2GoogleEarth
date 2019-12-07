using Newtonsoft.Json;
using OpenTXLog2GoogleEarthConvert;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Threading;

namespace OpenTXLog2GoogleEarth
{
   class Program
    {
        static void Main(string[] args)
        {
            var exe = Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, "exe"); // force switch to exe on .net core
            var exeDir = Path.GetDirectoryName(exe);
            Directory.SetCurrentDirectory(exeDir);

            CreateContextMenu(exe);
            if (args.Length == 0 || string.IsNullOrEmpty(args[0]) || !File.Exists(args[0]))
                return;
            string logPath = args[0];
            string outputPath = Path.Combine(exeDir, Guid.NewGuid().ToString() + Constants.KML_EXTENSION);
            var configPath = Path.Combine(exeDir, "config.json");

            if (!File.Exists(configPath))
                return;

            var config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(configPath));

            var options = new ConverterOptions()
            {
                AltitudeOffset = config.AltitudeOffset,
                AltitudeMode = config.AltitudeMode,
                LogLines = File.ReadAllLines(logPath),
                OutputStream = File.Create(outputPath),
                FullName = Path.GetFileNameWithoutExtension(logPath),
                Name = Path.GetFileNameWithoutExtension(logPath).Split("-")[0],
                AltitudeHeader = config.AltitudeHeader,
                DateHeader = config.DateHeader,
                GPSHeader = config.GPSHeader,
                GPSSpeedHeader = config.GPSSpeedHeader,
                TimeHeader = config.TimeHeader,
                PathColor = config.PathColor,
                GPSSpeedFactor = config.GPSSpeedFactor,
                GPSSpeedLabel = config.GPSSpeedLabel,
            };
            var converter = new Converter(options);
            converter.Convert();


            if (!string.IsNullOrEmpty(config.GoogleEarthExe) && File.Exists(config.GoogleEarthExe))
            {
                Process.Start(config.GoogleEarthExe, outputPath);
            }
        }

        public static bool IsElevated
        {
            get
            {
                return new WindowsPrincipal
                    (WindowsIdentity.GetCurrent()).IsInRole
                    (WindowsBuiltInRole.Administrator);
            }
        }

        static void CreateContextMenu(string executable)
        {
            if (IsElevated)
            {
                ShellContextMenu scMenu = new ShellContextMenu();
                scMenu.Items.Add(
                    new ShellContextMenuItem()
                    {
                        FileKey = ".csv",
                        Name = "openTXLog2GoogleEarth",
                        Caption = "Open in Google Earth",
                        Icon = executable,
                        ExePath = $"\"{executable}\" \"%V\"",
                    });
                scMenu.Save();
            }
        }
    }
}
