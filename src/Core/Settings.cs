using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace WinMemoryCleaner
{
    public static class Settings
    {
        private static readonly CultureInfo _culture = new CultureInfo(Constants.Windows.Locale.Name.English);

        #region Constructors

        static Settings()
        {
            // Default values
            AlwaysOnTop = false;
            AutoOptimizationInterval = 0;
            AutoOptimizationMemoryUsage = 0;
            AutoUpdate = true;
            CloseAfterOptimization = false;
            CloseToTheNotificationArea = false;
            CompactMode = false;
            Language = Constants.Windows.Locale.Name.English;
            MemoryAreas = Enums.Memory.Areas.CombinedPageList | Enums.Memory.Areas.ModifiedPageList | Enums.Memory.Areas.ProcessesWorkingSet | Enums.Memory.Areas.StandbyList | Enums.Memory.Areas.SystemWorkingSet;
            OptimizationKey = Key.M;
            OptimizationModifiers = ModifierKeys.Control | ModifierKeys.Alt;
            ProcessExclusionList = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            RunOnPriority = Enums.Priority.Low;
            RunOnStartup = false;
            ShowOptimizationNotifications = true;
            ShowVirtualMemory = false;
            StartMinimized = false;
            TrayIcon = Enums.Icon.Tray.Image;

            // User values
            try
            {
                // Process Exclusion List
                var exlusionpath = GetFileName(Constants.App.Registry.Key.ProcessExclusionListFile);
                var text = File.ReadAllText(exlusionpath);
                var lines = text.Split('\n').Where(line => (!string.IsNullOrWhiteSpace(line) && !line[0].Equals('#'))).Select(line => line.RemoveWhitespaces().Replace(".exe", string.Empty).ToLower(_culture)).ToArray();
                foreach (var line in lines)
                {
                    ProcessExclusionList.Add(line);
                }
            }
            catch(FileNotFoundException)
            {
                // ignore
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            try
            {

                // Settings
                var inipath = GetFileName(Constants.App.Registry.Key.SettingsFile);
                var ini = SimpleIni.Load(File.ReadAllText(inipath));
                AlwaysOnTop = ini.GetBool(Constants.App.Registry.Name.AlwaysOnTop, AlwaysOnTop);
                AutoOptimizationInterval = ini.GetInt(Constants.App.Registry.Name.AutoOptimizationInterval, AutoOptimizationInterval);
                AutoOptimizationMemoryUsage = ini.GetInt(Constants.App.Registry.Name.AutoOptimizationMemoryUsage, AutoOptimizationMemoryUsage);
                AutoUpdate = ini.GetBool(Constants.App.Registry.Name.AutoUpdate, AutoUpdate);
                CloseAfterOptimization = ini.GetBool(Constants.App.Registry.Name.CloseAfterOptimization, CloseAfterOptimization);
                CloseToTheNotificationArea = ini.GetBool(Constants.App.Registry.Name.CloseToTheNotificationArea, CloseToTheNotificationArea);
                CompactMode = ini.GetBool(Constants.App.Registry.Name.CompactMode, CompactMode);
                Language = ini.GetString(Constants.App.Registry.Name.Language, Language);

                var memoryAreasStr = ini.Get(Constants.App.Registry.Name.MemoryAreas);
                if (!string.IsNullOrWhiteSpace(memoryAreasStr))
                {
                    Enums.Memory.Areas memoryAreas;
                    if (Enum.TryParse(memoryAreasStr, out memoryAreas) && memoryAreas.IsValid())
                    {
                        if ((memoryAreas & Enums.Memory.Areas.StandbyList) != 0 && (memoryAreas & Enums.Memory.Areas.StandbyListLowPriority) != 0)
                        {
                            memoryAreas &= ~Enums.Memory.Areas.StandbyListLowPriority;
                        }
                            
                        MemoryAreas = memoryAreas;
                    }
                }

                var optimizationKeyStr = ini.Get(Constants.App.Registry.Name.OptimizationKey);
                if (!string.IsNullOrWhiteSpace(optimizationKeyStr))
                {
                    Key key;
                    if (Enum.TryParse(optimizationKeyStr, out key) && key.IsValid())
                    {
                        OptimizationKey = key;
                    }
                }

                var optimizationModifiersStr = ini.Get(Constants.App.Registry.Name.OptimizationModifiers);
                if (!string.IsNullOrWhiteSpace(optimizationModifiersStr))
                {
                    ModifierKeys modifiers;
                    if (Enum.TryParse(optimizationModifiersStr, out modifiers) && modifiers.IsValid())
                    {
                        OptimizationModifiers = modifiers;
                    }
                }

                var runOnPriorityStr = ini.Get(Constants.App.Registry.Name.RunOnPriority);
                if (!string.IsNullOrWhiteSpace(runOnPriorityStr))
                {
                    Enums.Priority priority;
                    if (Enum.TryParse(runOnPriorityStr, out priority) && priority.IsValid())
                    {
                        RunOnPriority = priority;
                    }
                }

                RunOnStartup = ini.GetBool(Constants.App.Registry.Name.RunOnStartup, RunOnStartup);
                ShowOptimizationNotifications = ini.GetBool(Constants.App.Registry.Name.ShowOptimizationNotifications, ShowOptimizationNotifications);
                ShowVirtualMemory = ini.GetBool(Constants.App.Registry.Name.ShowVirtualMemory, ShowVirtualMemory);
                StartMinimized = ini.GetBool(Constants.App.Registry.Name.StartMinimized, StartMinimized);

                var trayIconStr = ini.Get(Constants.App.Registry.Name.TrayIcon);
                if (!string.IsNullOrWhiteSpace(trayIconStr))
                {
                    Enums.Icon.Tray icon;
                    if (Enum.TryParse(trayIconStr, out icon) && icon.IsValid())
                    {
                        TrayIcon = icon;
                    }
                }



               
            }
            catch (FileNotFoundException)
            {

            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            finally
            {
                Save();
            }
        }

        #endregion

        #region Properties

        public static bool AlwaysOnTop { get; set; }

        public static int AutoOptimizationInterval { get; set; }

        public static int AutoOptimizationMemoryUsage { get; set; }

        public static bool AutoUpdate { get; set; }

        public static bool CloseAfterOptimization { get; set; }

        public static bool CloseToTheNotificationArea { get; set; }

        public static bool CompactMode { get; set; }

        public static string Language { get; set; }

        public static Enums.Memory.Areas MemoryAreas { get; set; }

        public static Key OptimizationKey { get; set; }

        public static ModifierKeys OptimizationModifiers { get; set; }

        public static SortedSet<string> ProcessExclusionList { get; private set; }

        public static Enums.Priority RunOnPriority { get; set; }

        public static bool RunOnStartup { get; set; }

        public static bool ShowOptimizationNotifications { get; set; }

        public static bool ShowVirtualMemory { get; set; }

        public static bool StartMinimized { get; set; }

        public static Enums.Icon.Tray TrayIcon { get; set; }

        #endregion

        #region Methods

        public static string GetFileName(string subKey)
        {
            var exe_dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            return System.IO.Path.Combine(exe_dir, subKey);
        }

        private class SimpleIni
        {
            private Dictionary<string, string> _data = new Dictionary<string, string>();
            public void Insert(string key, string value)
            {
                _data[key] = value;
            }
            public string GetString(string key, string defaultValue)
            {
                if (_data.ContainsKey(key))
                {
                    return _data[key];
                }
                else
                {
                    return defaultValue;
                }
            }
            public string Get(string key)
            {
                if (_data.ContainsKey(key))
                {
                    return _data[key];
                }
                else
                {
                    return null;
                }
            }
            public bool GetBool(string key, bool defaultValue)
            {
                if (_data.ContainsKey(key))
                {
                    switch (_data[key])
                    {
                        case "0":
                            return false;
                        case "1":
                            return true;
                        default:
                            return defaultValue;
                    }
                }
                else
                {
                    return defaultValue;
                }
            }
            public int GetInt(string key, int defaultValue)
            {
                if (_data.ContainsKey(key))
                {
                    try
                    {
                        return Convert.ToInt32(_data[key],_culture);
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }
                else
                {
                    return defaultValue;
                }
            }
            
            public void Remove(string key)
            {
                if (_data.ContainsKey(key))
                {
                    _data.Remove(key);
                }
            }
            public static SimpleIni Load(string content)
            {
                var ini = new SimpleIni();
                var content_lines = content.Split('\n');
                content_lines = content_lines.Where(line => (!string.IsNullOrWhiteSpace(line) && !line[0].Equals('#'))).Select(line => line.Trim()).ToArray();
                foreach (var line in content_lines)
                {
                    // this simple parser only care * = *
                    var kv = line.Split('=');
                    if (kv.Length == 2)
                    {
                        ini.Insert(kv[0].Trim(), kv[1].Trim());
                    }
                }
                return ini;
            }
            public string Export()
            {
                var content = _data.ToArray().OrderBy(kv => kv.Key).Select(kv => kv.Key + " = " + kv.Value).ToArray();
                var inistring = string.Join("\n", content);
                return inistring;
            }
        }

        public static void Save()
        {
            try
            {
                // Process Exclusion List
                var exclusionpath = GetFileName(Constants.App.Registry.Key.ProcessExclusionListFile);
                if (File.Exists(exclusionpath))
                {
                    File.Delete(exclusionpath);
                }
                if (ProcessExclusionList.Any())
                {
                    
                    var writelist = ProcessExclusionList.Select(process => process.RemoveWhitespaces().Replace(".exe", string.Empty).ToLower(_culture)).ToArray();
                    var content = string.Join("\n", writelist);
                    using (var file = File.CreateText(exclusionpath))
                    {
                        file.Write(content);
                    }
                }

                // Settings
                var inipath = GetFileName(Constants.App.Registry.Key.SettingsFile);
                var inifile = new SimpleIni();
                inifile.Insert(Constants.App.Registry.Name.AlwaysOnTop, AlwaysOnTop ? "1" : "0");
                inifile.Insert(Constants.App.Registry.Name.AutoOptimizationInterval, AutoOptimizationInterval.ToString(_culture));
                inifile.Insert(Constants.App.Registry.Name.AutoOptimizationMemoryUsage, AutoOptimizationMemoryUsage.ToString(_culture));
                inifile.Insert(Constants.App.Registry.Name.AutoUpdate, AutoUpdate ? "1" : "0");
                inifile.Insert(Constants.App.Registry.Name.CloseAfterOptimization, CloseAfterOptimization ? "1" : "0");
                inifile.Insert(Constants.App.Registry.Name.CloseToTheNotificationArea, CloseToTheNotificationArea ? "1" : "0");
                inifile.Insert(Constants.App.Registry.Name.CompactMode, CompactMode ? "1" : "0");
                inifile.Insert(Constants.App.Registry.Name.Language, Language);
                inifile.Insert(Constants.App.Registry.Name.MemoryAreas, ((int)MemoryAreas).ToString(_culture));
                inifile.Insert(Constants.App.Registry.Name.OptimizationKey, ((int)OptimizationKey).ToString(_culture));
                inifile.Insert(Constants.App.Registry.Name.OptimizationModifiers, ((int)OptimizationModifiers).ToString(_culture));
                inifile.Insert(Constants.App.Registry.Name.RunOnPriority, ((int)RunOnPriority).ToString(_culture));
                inifile.Insert(Constants.App.Registry.Name.RunOnStartup, RunOnStartup ? "1" : "0");
                inifile.Insert(Constants.App.Registry.Name.ShowOptimizationNotifications, ShowOptimizationNotifications ? "1" : "0");
                inifile.Insert(Constants.App.Registry.Name.ShowVirtualMemory, ShowVirtualMemory ? "1" : "0");
                inifile.Insert(Constants.App.Registry.Name.StartMinimized, StartMinimized ? "1" : "0");
                inifile.Insert(Constants.App.Registry.Name.TrayIcon, ((int)TrayIcon).ToString(_culture));
                var inicontent = inifile.Export();

                using (var file = File.CreateText(inipath))
                {
                    file.Write(inicontent);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        #endregion
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member