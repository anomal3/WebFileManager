using Mono.Nat;
using System.Net;

namespace CompanyFileManager.Services
{
    public class NetworkService
    {
        private string? _cachedPublicIp;
        private bool _isDiscoveryRunning;

        public async Task<string?> GetPublicIpAsync()
        {
            if (_cachedPublicIp != null) return _cachedPublicIp;

            var urls = new[]
            {
                "https://api.ipify.org",
                "https://icanhazip.com",
                "https://checkip.amazonaws.com"
            };

            foreach (var url in urls)
            {
                try
                {
                    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(6) };
                    var result = (await client.GetStringAsync(url)).Trim();
                    if (!string.IsNullOrEmpty(result) && result.Length < 50)
                    {
                        _cachedPublicIp = result;
                        return _cachedPublicIp;
                    }
                }
                catch { }
            }

            return null;
        }

        public async Task<(bool Success, string Message)> MapPortAsync(int port)
        {
            if (_isDiscoveryRunning)
                return (false, "Обнаружение уже запущено");

            try
            {
                _isDiscoveryRunning = true;
                var tcs = new TaskCompletionSource<INatDevice?>();

                EventHandler<DeviceEventArgs>? handler = null;
                handler = (_, e) =>
                {
                    NatUtility.DeviceFound -= handler;
                    tcs.TrySetResult(e.Device);
                };

                NatUtility.DeviceFound += handler;
                NatUtility.StartDiscovery();

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(12));
                cts.Token.Register(() => tcs.TrySetResult(null));

                var device = await tcs.Task;
                NatUtility.StopDiscovery();

                if (device == null)
                    return (false, "UPnP-роутер не найден. Включите UPnP в настройках роутера.");

                // Сначала пробуем с lease 24ч, при ошибке 725 — повторяем с permanent (0)
                try
                {
                    await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, port, port, 86400, "WebFileManager"));
                    return (true, $"Порт {port} проброшен через UPnP (lease 24ч)");
                }
                catch (MappingException mex) when (mex.Message.Contains("725") || mex.Message.Contains("OnlyPermanent"))
                {
                    // Роутер требует permanent lease
                    await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, port, port, 0, "WebFileManager"));
                    return (true, $"Порт {port} проброшен через UPnP (permanent)");
                }
            }
            catch (MappingException mex)
            {
                return (false, $"Роутер отклонил маппинг: {mex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка UPnP: {ex.Message}");
            }
            finally
            {
                _isDiscoveryRunning = false;
                try { NatUtility.StopDiscovery(); } catch { }
            }
        }

        public string BuildShareableLink(string ip, int port) =>
            port == 80 ? $"http://{ip}" : $"http://{ip}:{port}";

        public string? GetLocalIp()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                return host.AddressList
                    .FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    ?.ToString();
            }
            catch { return null; }
        }

        public void ClearCachedIp() => _cachedPublicIp = null;
    }
}
