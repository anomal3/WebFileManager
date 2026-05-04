using CompanyFileManager.Services;
using Microsoft.AspNetCore.SignalR;

namespace CompanyFileManager.Hubs
{
    public class ScreenShareHub : Hub
    {
        private readonly ScreenCaptureService _capture;
        private readonly InputSimulatorService _input;
        private readonly AppSettingsService _settings;

        public ScreenShareHub(ScreenCaptureService capture, InputSimulatorService input, AppSettingsService settings)
        {
            _capture = capture;
            _input = input;
            _settings = settings;
        }

        public async Task StartSession()
        {
            if (!_settings.Settings.AllowRemoteControl)
            {
                await Clients.Caller.SendAsync("Error", "Удалённое управление отключено");
                return;
            }
            _capture.AddViewer(Context.ConnectionId);
        }

        public void StopSession() => _capture.RemoveViewer(Context.ConnectionId);

        public override Task OnDisconnectedAsync(Exception? ex)
        {
            _capture.RemoveViewer(Context.ConnectionId);
            return base.OnDisconnectedAsync(ex);
        }

        public void MoveMouse(int x, int y) => _input.MoveMouse(x, y);
        public void MouseDown(int x, int y, int btn) => _input.MouseDown(x, y, btn);
        public void MouseUp(int x, int y, int btn) => _input.MouseUp(x, y, btn);
        public void Scroll(int delta) => _input.Scroll(delta);
        public void KeyEvent(string key, string code, bool ctrl, bool alt, bool shift, bool keyUp)
            => _input.KeyEvent(key, code, ctrl, alt, shift, keyUp);
    }
}
