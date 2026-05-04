using System.Runtime.InteropServices;

namespace CompanyFileManager.Services
{
    public class InputSimulatorService
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint n, INPUT[] inputs, int size);

        [DllImport("user32.dll")] static extern bool SetCursorPos(int x, int y);

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT { public uint Type; public InputUnion U; }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT Mouse;
            [FieldOffset(0)] public KEYBDINPUT Keyboard;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT { public int dx, dy; public uint Data, Flags, Time; public IntPtr Extra; }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT { public ushort Vk, Scan; public uint Flags, Time; public IntPtr Extra; }

        const uint M = 0, K = 1;
        const uint LDOWN = 0x02, LUP = 0x04, RDOWN = 0x08, RUP = 0x10, MDOWN = 0x20, MUP = 0x40;
        const uint WHEEL = 0x0800, KUP = 0x0002;

        static int _size = Marshal.SizeOf<INPUT>();

        public void MoveMouse(int x, int y) => SetCursorPos(x, y);

        public void MouseDown(int x, int y, int btn)
        {
            SetCursorPos(x, y);
            var f = btn == 2 ? RDOWN : btn == 1 ? MDOWN : LDOWN;
            Send(Mouse(f));
        }

        public void MouseUp(int x, int y, int btn)
        {
            SetCursorPos(x, y);
            var f = btn == 2 ? RUP : btn == 1 ? MUP : LUP;
            Send(Mouse(f));
        }

        public void Scroll(int delta) => Send(new INPUT
        {
            Type = M,
            U = new InputUnion { Mouse = new MOUSEINPUT { Flags = WHEEL, Data = (uint)(-delta * 3) } }
        });

        public void KeyEvent(string key, string code, bool ctrl, bool alt, bool shift, bool keyUp)
        {
            var vk = ToVk(key, code);
            if (vk == 0) return;

            var inputs = new List<INPUT>();
            if (!keyUp)
            {
                if (ctrl) inputs.Add(Key(0x11, false));
                if (shift) inputs.Add(Key(0x10, false));
                if (alt) inputs.Add(Key(0x12, false));
                inputs.Add(Key(vk, false));
            }
            else
            {
                inputs.Add(Key(vk, true));
                if (alt) inputs.Add(Key(0x12, true));
                if (shift) inputs.Add(Key(0x10, true));
                if (ctrl) inputs.Add(Key(0x11, true));
            }

            SendInput((uint)inputs.Count, inputs.ToArray(), _size);
        }

        private static void Send(INPUT input) => SendInput(1, new[] { input }, _size);

        private static INPUT Mouse(uint flags) => new INPUT
        { Type = M, U = new InputUnion { Mouse = new MOUSEINPUT { Flags = flags } } };

        private static INPUT Key(ushort vk, bool up) => new INPUT
        { Type = K, U = new InputUnion { Keyboard = new KEYBDINPUT { Vk = vk, Flags = up ? KUP : 0 } } };

        private static ushort ToVk(string key, string code) => key switch
        {
            "Enter" => 0x0D, "Escape" => 0x1B, "Backspace" => 0x08, "Tab" => 0x09,
            "Delete" => 0x2E, "ArrowLeft" => 0x25, "ArrowUp" => 0x26,
            "ArrowRight" => 0x27, "ArrowDown" => 0x28,
            "Home" => 0x24, "End" => 0x23, "PageUp" => 0x21, "PageDown" => 0x22,
            "F1" => 0x70, "F2" => 0x71, "F3" => 0x72, "F4" => 0x73,
            "F5" => 0x74, "F6" => 0x75, "F7" => 0x76, "F8" => 0x77,
            "F9" => 0x78, "F10" => 0x79, "F11" => 0x7A, "F12" => 0x7B,
            "Control" => 0x11, "Shift" => 0x10, "Alt" => 0x12,
            " " => 0x20, "CapsLock" => 0x14, "Insert" => 0x2D,
            _ when key.Length == 1 => (ushort)char.ToUpper(key[0]),
            _ => 0
        };
    }
}
