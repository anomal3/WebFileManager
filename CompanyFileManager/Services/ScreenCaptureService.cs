using Microsoft.AspNetCore.SignalR;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using CompanyFileManager.Hubs;

#pragma warning disable CA1416

namespace CompanyFileManager.Services
{
    public class ScreenCaptureService : IDisposable
    {
        private readonly IHubContext<ScreenShareHub> _hubContext;
        private readonly HashSet<string> _viewers = new();
        private readonly SemaphoreSlim _sendLock = new(1, 1);
        private Timer? _timer;
        private const int FrameMs = 66;     // ~15 FPS
        private const long JpegQuality = 55L;

        [DllImport("user32.dll")] static extern int GetSystemMetrics(int n);
        [DllImport("user32.dll")] static extern bool GetCursorPos(out POINT p);
        [DllImport("user32.dll")] static extern bool DrawIcon(IntPtr hdc, int x, int y, IntPtr hicon);
        [DllImport("user32.dll")] static extern IntPtr LoadCursor(IntPtr h, int id);

        [StructLayout(LayoutKind.Sequential)] struct POINT { public int X, Y; }

        public ScreenCaptureService(IHubContext<ScreenShareHub> hubContext) => _hubContext = hubContext;

        public void AddViewer(string id)
        {
            lock (_viewers) { _viewers.Add(id); if (_viewers.Count == 1) StartTimer(); }
        }

        public void RemoveViewer(string id)
        {
            lock (_viewers) { _viewers.Remove(id); if (_viewers.Count == 0) StopTimer(); }
        }

        private void StartTimer() => _timer = new Timer(Tick, null, 0, FrameMs);
        private void StopTimer() { _timer?.Dispose(); _timer = null; }

        private async void Tick(object? _)
        {
            if (!await _sendLock.WaitAsync(0)) return;
            string[] viewers;
            lock (_viewers) { viewers = _viewers.ToArray(); }
            if (viewers.Length == 0) { _sendLock.Release(); return; }

            try
            {
                var (b64, w, h) = Capture();
                if (b64 is null) return;
                await _hubContext.Clients.Clients(viewers).SendAsync("ReceiveFrame", b64, w, h);
            }
            catch { }
            finally { _sendLock.Release(); }
        }

        private static (string? b64, int w, int h) Capture()
        {
            try
            {
                var w = GetSystemMetrics(0);
                var h = GetSystemMetrics(1);
                if (w <= 0 || h <= 0) return (null, 0, 0);

                using var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(0, 0, 0, 0, new Size(w, h));
                    // Draw cursor
                    if (GetCursorPos(out var cp))
                    {
                        var hCursor = LoadCursor(IntPtr.Zero, 32512); // IDC_ARROW
                        DrawIcon(g.GetHdc(), cp.X, cp.Y, hCursor);
                        g.ReleaseHdc();
                    }
                }

                using var ms = new MemoryStream();
                var enc = ImageCodecInfo.GetImageEncoders().First(e => e.FormatID == ImageFormat.Jpeg.Guid);
                var ep = new EncoderParameters(1);
                ep.Param[0] = new EncoderParameter(Encoder.Quality, JpegQuality);
                bmp.Save(ms, enc, ep);
                return (Convert.ToBase64String(ms.ToArray()), w, h);
            }
            catch { return (null, 0, 0); }
        }

        public void Dispose() { StopTimer(); _sendLock.Dispose(); }
    }
}
