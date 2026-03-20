using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Kalendarz.Helpers
{
    public enum Theme
    {
        Light,
        Dark
    }

    public class ThemeManager : INotifyPropertyChanged
    {
        private static ThemeManager? _instance;
        public static ThemeManager Instance => _instance ??= new ThemeManager();

        private readonly string ThemeFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Kalendarz", "theme.json");

        private Theme _currentTheme = Theme.Light;
        public Theme CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsDarkTheme));
                    SaveTheme();
                }
            }
        }

        public bool IsDarkTheme => CurrentTheme == Theme.Dark;

        private ThemeManager()
        {
            LoadTheme();
        }

        public void ToggleTheme()
        {
            CurrentTheme = CurrentTheme == Theme.Light ? Theme.Dark : Theme.Light;
        }

        private void SaveTheme()
        {
            try
            {
                var dir = Path.GetDirectoryName(ThemeFilePath);
                if (!Directory.Exists(dir!)) Directory.CreateDirectory(dir!);
                var json = JsonSerializer.Serialize(new { Theme = CurrentTheme.ToString() }, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ThemeFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveTheme failed: {ex.Message}");
            }
        }

        private void LoadTheme()
        {
            try
            {
                if (File.Exists(ThemeFilePath))
                {
                    var json = File.ReadAllText(ThemeFilePath);
                    var data = JsonSerializer.Deserialize<ThemeData>(json);
                    if (data != null && Enum.TryParse<Theme>(data.Theme, out var theme))
                    {
                        _currentTheme = theme;
                        OnPropertyChanged(nameof(CurrentTheme));
                        OnPropertyChanged(nameof(IsDarkTheme));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadTheme failed: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private class ThemeData
        {
            public string Theme { get; set; } = "Light";
        }
    }
}
