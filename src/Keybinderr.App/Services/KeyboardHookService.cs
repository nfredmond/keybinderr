using System.Runtime.InteropServices;
using WinForms = System.Windows.Forms;

namespace Keybinderr.App.Services;

public sealed class KeyboardHookService : IDisposable
{
    private const int WhKeyboardLl = 13;
    private const int WmKeyDown = 0x0100;
    private const int WmKeyUp = 0x0101;
    private const int WmSysKeyDown = 0x0104;
    private const int WmSysKeyUp = 0x0105;
    private const uint LlkhfInjected = 0x00000010;
    private const uint InputKeyboard = 1;
    private const uint KeyEventFKeyUp = 0x0002;

    private readonly ActiveProfileService _activeProfileService;
    private readonly LowLevelKeyboardProc _hookCallback;
    private IntPtr _hookId = IntPtr.Zero;

    public KeyboardHookService(ActiveProfileService activeProfileService)
    {
        _activeProfileService = activeProfileService;
        _hookCallback = HookCallback;
    }

    public bool IsRunning => _hookId != IntPtr.Zero;

    public void Start()
    {
        if (_hookId != IntPtr.Zero)
        {
            return;
        }

        _hookId = SetWindowsHookEx(WhKeyboardLl, _hookCallback, GetModuleHandle(null), 0);
    }

    public void Stop()
    {
        if (_hookId == IntPtr.Zero)
        {
            return;
        }

        UnhookWindowsHookEx(_hookId);
        _hookId = IntPtr.Zero;
    }

    public void Dispose()
    {
        Stop();
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode < 0)
        {
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        var hookData = Marshal.PtrToStructure<KbdLlHookStruct>(lParam);
        if ((hookData.Flags & LlkhfInjected) == LlkhfInjected)
        {
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        var message = wParam.ToInt32();
        var isKeyDown = message is WmKeyDown or WmSysKeyDown;
        var isKeyUp = message is WmKeyUp or WmSysKeyUp;
        if (!isKeyDown && !isKeyUp)
        {
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        var sourceKey = ((WinForms.Keys)hookData.VkCode).ToString().ToUpperInvariant();
        if (!_activeProfileService.ActiveMappings.TryGetValue(sourceKey, out var targetKeyName))
        {
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        if (!Enum.TryParse<WinForms.Keys>(targetKeyName, ignoreCase: true, out var targetKey))
        {
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        SendKey(targetKey, keyUp: isKeyUp);
        return (IntPtr)1;
    }

    private static void SendKey(WinForms.Keys targetKey, bool keyUp)
    {
        var input = new Input
        {
            Type = InputKeyboard,
            Data = new InputUnion
            {
                Keyboard = new KeybdInput
                {
                    VirtualKey = (ushort)targetKey,
                    ScanCode = 0,
                    Flags = keyUp ? KeyEventFKeyUp : 0,
                    Time = 0,
                    ExtraInfo = UIntPtr.Zero
                }
            }
        };

        SendInput(1, new[] { input }, Marshal.SizeOf<Input>());
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct KbdLlHookStruct
    {
        public uint VkCode;
        public uint ScanCode;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public uint Type;
        public InputUnion Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public KeybdInput Keyboard;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeybdInput
    {
        public ushort VirtualKey;
        public ushort ScanCode;
        public uint Flags;
        public uint Time;
        public UIntPtr ExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);
}
