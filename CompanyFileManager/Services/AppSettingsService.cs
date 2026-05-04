using CompanyFileManager.Models;
using System.Text.Json;

namespace CompanyFileManager.Services
{
    public class AppSettingsService
    {
        private static readonly string SettingsPath;
        private AppSettings _settings = new();

        static AppSettingsService()
        {
            SettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.user.json");
        }

        public AppSettings Settings => _settings;

        public void Load()
        {
            if (!File.Exists(SettingsPath)) return;
            try
            {
                var json = File.ReadAllText(SettingsPath);
                _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                _settings = new AppSettings();
            }
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }

        public void UpdateAndSave(AppSettings settings)
        {
            _settings = settings;
            Save();
        }
    }
}
