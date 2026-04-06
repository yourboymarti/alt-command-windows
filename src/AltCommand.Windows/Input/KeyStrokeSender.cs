using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AltCommand.Windows.Input;

internal sealed class KeyStrokeSender : IDisposable
{
    private bool _disposed;

    public void SendCombination(HotkeyCombination combination)
    {
        var inputs = new List<NativeMethods.INPUT>();

        foreach (var modifierKey in GetModifierKeysInOrder(combination.Modifiers))
        {
            inputs.Add(CreateKeyInput(modifierKey, keyUp: false));
        }

        inputs.Add(CreateKeyInput(combination.Key, keyUp: false));
        inputs.Add(CreateKeyInput(combination.Key, keyUp: true));

        foreach (var modifierKey in GetModifierKeysInOrder(combination.Modifiers).Reverse())
        {
            inputs.Add(CreateKeyInput(modifierKey, keyUp: true));
        }

        Send(inputs);
    }

    public void SendKeyDown(Keys key)
    {
        Send([CreateKeyInput(key, keyUp: false)]);
    }

    public void SendKeyUp(Keys key)
    {
        Send([CreateKeyInput(key, keyUp: true)]);
    }

    public void Dispose()
    {
        _disposed = true;
    }

    private void Send(List<NativeMethods.INPUT> inputs)
    {
        if (_disposed || inputs.Count == 0)
        {
            return;
        }

        var sent = NativeMethods.SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf<NativeMethods.INPUT>());

        if (sent != (uint)inputs.Count)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "SendInput failed to inject the shortcut.");
        }
    }

    private static NativeMethods.INPUT CreateKeyInput(Keys key, bool keyUp)
    {
        var flags = keyUp ? NativeMethods.KEYEVENTF_KEYUP : 0u;

        if (NeedsExtendedKeyFlag(key))
        {
            flags |= NativeMethods.KEYEVENTF_EXTENDEDKEY;
        }

        return new NativeMethods.INPUT
        {
            type = NativeMethods.INPUT_KEYBOARD,
            U = new NativeMethods.InputUnion
            {
                ki = new NativeMethods.KEYBDINPUT
                {
                    wVk = (ushort)key,
                    wScan = 0,
                    dwFlags = flags,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };
    }

    private static IEnumerable<Keys> GetModifierKeysInOrder(ModifierKeys modifiers)
    {
        if (modifiers.HasFlag(ModifierKeys.Control))
        {
            yield return Keys.ControlKey;
        }

        if (modifiers.HasFlag(ModifierKeys.Shift))
        {
            yield return Keys.ShiftKey;
        }

        if (modifiers.HasFlag(ModifierKeys.Alt))
        {
            yield return Keys.Menu;
        }

        if (modifiers.HasFlag(ModifierKeys.Win))
        {
            yield return Keys.LWin;
        }
    }

    private static bool NeedsExtendedKeyFlag(Keys key)
    {
        return key is Keys.Right
            or Keys.Left
            or Keys.Up
            or Keys.Down
            or Keys.Home
            or Keys.End
            or Keys.PageUp
            or Keys.PageDown
            or Keys.Insert
            or Keys.Delete
            or Keys.Divide
            or Keys.NumLock
            or Keys.RControlKey
            or Keys.RMenu
            or Keys.LWin
            or Keys.RWin;
    }
}
