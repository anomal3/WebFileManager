using System.Diagnostics;

namespace CompanyFileManager.Services
{
    public class RestartService
    {
        private readonly IHostApplicationLifetime _lifetime;

        public RestartService(IHostApplicationLifetime lifetime) => _lifetime = lifetime;

        public bool RestartApp()
        {
            try
            {
                var exePath = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
                    return false;

                var psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true,
                    WorkingDirectory = AppContext.BaseDirectory
                };

                Process.Start(psi);
            }
            catch
            {
                return false;
            }

            // Даём время браузеру получить ответ, затем останавливаем приложение
            _ = Task.Run(async () =>
            {
                await Task.Delay(800);
                _lifetime.StopApplication();
                await Task.Delay(1500);
                Environment.Exit(0);
            });

            return true;
        }
    }
}
