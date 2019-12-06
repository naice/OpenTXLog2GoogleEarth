using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTXLog2GoogleEarth
{
    public class ShellContextMenuItem
    {
        public string FileKey { get; set; }
        public string ExePath { get; set; }
        public string Caption { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
    }

    public class ShellContextMenu
    {
        public List<ShellContextMenuItem> Items { get; set; } = new List<ShellContextMenuItem>();


        public ShellContextMenu(IEnumerable<string> fileKeys = null)
        {
            if (fileKeys != null)
                refresh(fileKeys);
        }

        private void refresh(IEnumerable<string> fileKeys)
        {
            Items = new List<ShellContextMenuItem>();

            foreach (var fileKey in fileKeys)
            {
                try
                {
                    RegistryKey keyShellBack = Registry.ClassesRoot.OpenSubKey($"{fileKey}\\shell");
                    foreach (var keyName in keyShellBack.GetSubKeyNames())
                    {
                        try
                        {
                            var shellKey = keyShellBack.OpenSubKey(keyName);
                            if (shellKey.GetSubKeyNames().Contains("command"))
                            {
                                var commandKey = shellKey.OpenSubKey("command");

                                ShellContextMenuItem item = new ShellContextMenuItem();
                                item.FileKey = fileKey;
                                item.Name = keyName;
                                item.Caption = shellKey.GetValue("") as string;
                                item.Icon = shellKey.GetValue("Icon") as string;
                                item.ExePath = commandKey.GetValue("") as string;

                                Items.Add(item);
                            }
                        }
                        catch
                        {
                        }
                    }

                }
                catch
                {
                }
            }
        }

        public void Save()
        {
            writeShellItems(this.Items);
        }

        public static void writeShellItems(List<ShellContextMenuItem> items)
        {
            foreach (var fileKeyMap in items.GroupBy(A => A.FileKey))
            {
                string fileKey = fileKeyMap.Key;

                if (!string.IsNullOrEmpty(fileKey))
                {
                    RegistryKey key = null;
                    try
                    {
                        key = Registry.ClassesRoot.CreateSubKey($"{fileKey}\\shell");
                        string[] subKeyNames = key.GetSubKeyNames();

                        foreach (var item in fileKeyMap)
                        {
                            try
                            {
                                RegistryKey shellKey = key.CreateSubKey(item.Name);
                                RegistryKey commandKey = shellKey.CreateSubKey("command");

                                if (!string.IsNullOrEmpty(item.Caption))
                                    shellKey.SetValue("", item.Caption);
                                if (!string.IsNullOrEmpty(item.Icon))
                                    shellKey.SetValue("Icon", item.Icon);
                                if (item.ExePath != null)
                                    commandKey.SetValue("", item.ExePath);

                                shellKey.Close();
                                commandKey.Close();
                            }
                            catch
                            {

                            }

                        }

                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}
