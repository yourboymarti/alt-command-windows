using System.Diagnostics;
using AltCommand.Windows.Configuration;

namespace AltCommand.Windows.Input;

internal sealed class HotkeyEngine
{
    private readonly KeyStrokeSender _keyStrokeSender;
    private Dictionary<HotkeyCombination, HotkeyCombination> _remaps = new();
    private HashSet<string> _disabledProcesses = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<Keys> _suppressedRemapKeys = [];
    private readonly HashSet<Keys> _nativePassthroughKeys = [];

    private bool _physicalLeftAltDown;
    private bool _physicalShiftDown;
    private bool _physicalControlDown;
    private bool _physicalWinDown;
    private bool _nativeAltSessionActive;
    private bool _currentChordIsExcluded;

    public HotkeyEngine(KeyStrokeSender keyStrokeSender)
    {
        _keyStrokeSender = keyStrokeSender;
    }

    public void UpdateConfiguration(AppConfig config)
    {
        var remaps = new Dictionary<HotkeyCombination, HotkeyCombination>();
        var errors = new List<string>();

        foreach (var entry in config.Remaps)
        {
            if (!HotkeyCombination.TryParse(entry.From, out var from))
            {
                errors.Add($"Invalid source shortcut '{entry.From}'.");
                continue;
            }

            if (!HotkeyCombination.TryParse(entry.To, out var to))
            {
                errors.Add($"Invalid target shortcut '{entry.To}'.");
                continue;
            }

            if (!remaps.TryAdd(from, to))
            {
                errors.Add($"Duplicate source shortcut '{entry.From}'.");
            }
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(string.Join(" ", errors));
        }

        _remaps = remaps;
        _disabledProcesses = new HashSet<string>(
            config.DisabledProcesses
                .Select(NormalizeProcessName)
                .Where(value => !string.IsNullOrWhiteSpace(value)),
            StringComparer.OrdinalIgnoreCase);
    }

    public bool Handle(KeyboardEvent keyboardEvent)
    {
        if (keyboardEvent.IsInjected)
        {
            return false;
        }

        var key = keyboardEvent.Key;

        if (HandleModifierKey(key, keyboardEvent))
        {
            return true;
        }

        if (keyboardEvent.IsKeyUp)
        {
            if (_suppressedRemapKeys.Remove(key))
            {
                return true;
            }

            if (_nativePassthroughKeys.Remove(key))
            {
                _keyStrokeSender.SendKeyUp(key);
                return true;
            }

            return false;
        }

        if (!_physicalLeftAltDown)
        {
            return false;
        }

        if (_currentChordIsExcluded)
        {
            return StartNativePassthroughFor(key);
        }

        var trigger = BuildTrigger(key);

        if (_remaps.TryGetValue(trigger, out var target))
        {
            ReleaseNativeAltSession();
            _keyStrokeSender.SendCombination(target);
            _suppressedRemapKeys.Add(key);
            return true;
        }

        return StartNativePassthroughFor(key);
    }

    private bool HandleModifierKey(Keys key, KeyboardEvent keyboardEvent)
    {
        if (key is Keys.LShiftKey or Keys.RShiftKey)
        {
            _physicalShiftDown = keyboardEvent.IsKeyDown;
            return false;
        }

        if (key is Keys.LControlKey or Keys.RControlKey)
        {
            _physicalControlDown = keyboardEvent.IsKeyDown;
            return false;
        }

        if (key is Keys.LWin or Keys.RWin)
        {
            _physicalWinDown = keyboardEvent.IsKeyDown;
            return false;
        }

        if (key != Keys.LMenu)
        {
            return false;
        }

        if (keyboardEvent.IsKeyDown)
        {
            _physicalLeftAltDown = true;
            _currentChordIsExcluded = IsForegroundProcessExcluded();
            return true;
        }

        _physicalLeftAltDown = false;
        _currentChordIsExcluded = false;
        ReleaseNativeAltSession();
        _suppressedRemapKeys.Clear();
        _nativePassthroughKeys.Clear();

        return true;
    }

    private HotkeyCombination BuildTrigger(Keys key)
    {
        var modifiers = ModifierKeys.Alt;

        if (_physicalControlDown)
        {
            modifiers |= ModifierKeys.Control;
        }

        if (_physicalShiftDown)
        {
            modifiers |= ModifierKeys.Shift;
        }

        if (_physicalWinDown)
        {
            modifiers |= ModifierKeys.Win;
        }

        return new HotkeyCombination(modifiers, key);
    }

    private bool StartNativePassthroughFor(Keys key)
    {
        EnsureNativeAltSession();
        _keyStrokeSender.SendKeyDown(key);
        _nativePassthroughKeys.Add(key);
        return true;
    }

    private void EnsureNativeAltSession()
    {
        if (_nativeAltSessionActive)
        {
            return;
        }

        _keyStrokeSender.SendKeyDown(Keys.LMenu);
        _nativeAltSessionActive = true;
    }

    private void ReleaseNativeAltSession()
    {
        if (!_nativeAltSessionActive)
        {
            return;
        }

        _keyStrokeSender.SendKeyUp(Keys.LMenu);
        _nativeAltSessionActive = false;
    }

    private bool IsForegroundProcessExcluded()
    {
        if (_disabledProcesses.Count == 0)
        {
            return false;
        }

        var windowHandle = NativeMethods.GetForegroundWindow();

        if (windowHandle == IntPtr.Zero)
        {
            return false;
        }

        _ = NativeMethods.GetWindowThreadProcessId(windowHandle, out var processId);

        if (processId == 0)
        {
            return false;
        }

        try
        {
            using var process = Process.GetProcessById((int)processId);
            return _disabledProcesses.Contains(NormalizeProcessName(process.ProcessName));
        }
        catch
        {
            return false;
        }
    }

    private static string NormalizeProcessName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim();

        if (normalized.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[..^4];
        }

        return normalized;
    }
}
