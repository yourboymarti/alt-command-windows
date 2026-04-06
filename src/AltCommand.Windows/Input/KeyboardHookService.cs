using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AltCommand.Windows.Input;

internal sealed class KeyboardHookService : IDisposable
{
    private readonly Func<KeyboardEvent, bool> _handler;
    private readonly NativeMethods.LowLevelKeyboardProc _hookCallback;
    private IntPtr _hookHandle;

    public KeyboardHookService(Func<KeyboardEvent, bool> handler)
    {
        _handler = handler;
        _hookCallback = HookCallback;
    }

    public void Start()
    {
        if (_hookHandle != IntPtr.Zero)
        {
            return;
        }

        _hookHandle = NativeMethods.SetWindowsHookEx(
            NativeMethods.WH_KEYBOARD_LL,
            _hookCallback,
            NativeMethods.GetModuleHandle(null),
            0);

        if (_hookHandle == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to install the global keyboard hook.");
        }
    }

    public void Dispose()
    {
        if (_hookHandle == IntPtr.Zero)
        {
            return;
        }

        _ = NativeMethods.UnhookWindowsHookEx(_hookHandle);
        _hookHandle = IntPtr.Zero;
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode < 0)
        {
            return NativeMethods.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        var message = (uint)wParam.ToInt64();
        var isKeyDown = message is NativeMethods.WM_KEYDOWN or NativeMethods.WM_SYSKEYDOWN;
        var isKeyUp = message is NativeMethods.WM_KEYUP or NativeMethods.WM_SYSKEYUP;

        if (!isKeyDown && !isKeyUp)
        {
            return NativeMethods.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        var data = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
        var keyboardEvent = new KeyboardEvent(
            (Keys)data.vkCode,
            isKeyDown,
            isKeyUp,
            (data.flags & NativeMethods.LLKHF_INJECTED) != 0);

        try
        {
            if (_handler(keyboardEvent))
            {
                return (IntPtr)1;
            }
        }
        catch
        {
        }

        return NativeMethods.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }
}
