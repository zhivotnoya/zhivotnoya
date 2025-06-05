using System;
using System.IO;
using Rage;
using Rage.Attributes;
using System.Drawing;

[assembly: Plugin("CarHUD", Author = "zhivotnoya", Description = "Simple vehicle HUD for LSPDFR")] 

namespace CarHUD
{
    public static class EntryPoint
    {
        private static bool _colorChange;
        private static bool _showFuel;
        private static bool _useMetric;
        private static HUDPosition _position;

        private static int _speedYellow;
        private static int _speedRed;
        private static int _healthYellow;
        private static int _healthRed;

        private static string _fuelPlaceholder = "--";

        private enum HUDPosition
        {
            BottomCenter,
            BottomRight,
            AboveMinimap,
            RightOfMinimap
        }

        private const string ConfigPath = "Plugins/CarHUD.ini";

        public static void Main()
        {
            Game.DisplayNotification("CarHUD loaded.");
            EnsureConfig();
            LoadConfig();
            GameFiber.StartNew(HudLoop);
        }

        private static void EnsureConfig()
        {
            if (File.Exists(ConfigPath)) return;

            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath) ?? ".");
            File.WriteAllLines(ConfigPath, new[]
            {
                "[Display]",
                "ColorChange=true",
                "UseMetric=false",
                "ShowFuel=false",
                "Position=BottomCenter",
                "SpeedYellow=70",
                "SpeedRed=100",
                "HealthYellow=80",
                "HealthRed=30"
            });
        }

        private static void LoadConfig()
        {
            var ini = new SimpleIni(ConfigPath);
            _colorChange = ini.ReadBool("Display", "ColorChange", true);
            _showFuel = ini.ReadBool("Display", "ShowFuel", false);
            _useMetric = ini.ReadBool("Display", "UseMetric", false);
            string pos = ini.ReadString("Display", "Position", "BottomCenter");
            Enum.TryParse(pos, true, out _position);
            _speedYellow = ini.ReadInt("Display", "SpeedYellow", 70);
            _speedRed = ini.ReadInt("Display", "SpeedRed", 100);
            _healthYellow = ini.ReadInt("Display", "HealthYellow", 80);
            _healthRed = ini.ReadInt("Display", "HealthRed", 30);
        }

        private static void HudLoop()
        {
            while (true)
            {
                GameFiber.Yield();
                var player = Game.LocalPlayer.Character;
                if (!player.Exists() || !player.IsInAnyVehicle(false))
                {
                    continue;
                }
                var veh = player.CurrentVehicle;
                if (!veh || !veh.Exists()) continue;

                DrawHUD(veh);
            }
        }

        private static void DrawHUD(Vehicle veh)
        {
            float speedMps = veh.Speed;
            float speed = _useMetric ? speedMps * 3.6f : speedMps * 2.23694f;
            float hpPct = (float)veh.Health / veh.MaxHealth * 100f;
            string fuel = _showFuel ? GetFuelLevel(veh) : _fuelPlaceholder;

            Color speedColor = GetThresholdColor(speed, _speedYellow, _speedRed);
            Color hpColor = GetThresholdColor(hpPct, _healthYellow, _healthRed, true);

            var pos = CalculatePosition();
            string unit = _useMetric ? "km/h" : "MPH";
            var speedText = $"SPD: {speed:0} {unit}";
            var hpText = $"HP: {hpPct:0}%";
            var fuelText = $"FUEL: {fuel}";

            Graphics.DrawText(speedText, pos.Speed, 0.5f, speedColor);
            Graphics.DrawText(hpText, pos.Health, 0.5f, hpColor);
            if (_showFuel)
                Graphics.DrawText(fuelText, pos.Fuel, 0.5f, Color.White);
        }

        private static (PointF Speed, PointF Health, PointF Fuel) CalculatePosition()
        {
            float x = 0f, y = 0f;
            switch (_position)
            {
                case HUDPosition.BottomCenter:
                    x = 0.5f; y = 0.9f; break;
                case HUDPosition.BottomRight:
                    x = 0.85f; y = 0.9f; break;
                case HUDPosition.AboveMinimap:
                    x = 0.21f; y = 0.77f; break;
                case HUDPosition.RightOfMinimap:
                    x = 0.34f; y = 0.9f; break;
            }
            PointF speed = new PointF(x, y);
            PointF health = new PointF(x, y + 0.03f);
            PointF fuel = new PointF(x, y + 0.06f);
            return (speed, health, fuel);
        }

        private static Color GetThresholdColor(float val, int yellow, int red, bool invert = false)
        {
            if (!_colorChange) return Color.White;

            if (invert)
            {
                if (val <= red) return Color.Red;
                if (val <= yellow) return Color.Yellow;
            }
            else
            {
                if (val >= red) return Color.Red;
                if (val >= yellow) return Color.Yellow;
            }

            return Color.White;
        }

        private static string GetFuelLevel(Vehicle veh)
        {
            // Placeholder: replace with actual fuel API if available
            return "??";
        }
    }

    internal class SimpleIni
    {
        private readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>> _data = new();

        public SimpleIni(string path)
        {
            if (!System.IO.File.Exists(path)) return;
            string currentSection = string.Empty;
            foreach (var line in System.IO.File.ReadAllLines(path))
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith(";") || trimmed == string.Empty) continue;
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    currentSection = trimmed.Substring(1, trimmed.Length - 2);
                    if (!_data.ContainsKey(currentSection))
                        _data[currentSection] = new System.Collections.Generic.Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
                }
                else if (trimmed.Contains("="))
                {
                    var idx = trimmed.IndexOf('=');
                    var key = trimmed.Substring(0, idx).Trim();
                    var val = trimmed.Substring(idx + 1).Trim();
                    if (!_data.ContainsKey(currentSection))
                        _data[currentSection] = new System.Collections.Generic.Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
                    _data[currentSection][key] = val;
                }
            }
        }

        public string ReadString(string section, string key, string def)
        {
            if (_data.TryGetValue(section, out var sec) && sec.TryGetValue(key, out var val))
                return val;
            return def;
        }

        public bool ReadBool(string section, string key, bool def)
        {
            string val = ReadString(section, key, def.ToString());
            if (bool.TryParse(val, out bool result)) return result;
            return def;
        }

        public int ReadInt(string section, string key, int def)
        {
            string val = ReadString(section, key, def.ToString());
            if (int.TryParse(val, out int result)) return result;
            return def;
        }
    }
}
