using System.Globalization;
using System.Text;

namespace AltCommand.Windows.Input;

internal readonly record struct HotkeyCombination(ModifierKeys Modifiers, Keys Key)
{
    private static readonly Dictionary<string, Keys> NamedKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Tab"] = Keys.Tab,
        ["Enter"] = Keys.Enter,
        ["Return"] = Keys.Enter,
        ["Escape"] = Keys.Escape,
        ["Esc"] = Keys.Escape,
        ["Space"] = Keys.Space,
        ["Spacebar"] = Keys.Space,
        ["Backspace"] = Keys.Back,
        ["Back"] = Keys.Back,
        ["Delete"] = Keys.Delete,
        ["Del"] = Keys.Delete,
        ["Insert"] = Keys.Insert,
        ["Ins"] = Keys.Insert,
        ["Home"] = Keys.Home,
        ["End"] = Keys.End,
        ["PageUp"] = Keys.PageUp,
        ["PgUp"] = Keys.PageUp,
        ["PageDown"] = Keys.PageDown,
        ["PgDn"] = Keys.PageDown,
        ["Left"] = Keys.Left,
        ["Right"] = Keys.Right,
        ["Up"] = Keys.Up,
        ["Down"] = Keys.Down,
        ["CapsLock"] = Keys.CapsLock,
        ["Apps"] = Keys.Apps,
        ["Menu"] = Keys.Apps,
        ["PrintScreen"] = Keys.PrintScreen,
        ["PrtSc"] = Keys.PrintScreen,
        ["Minus"] = Keys.OemMinus,
        ["Equal"] = Keys.Oemplus,
        ["Plus"] = Keys.Oemplus,
        ["Comma"] = Keys.Oemcomma,
        ["Period"] = Keys.OemPeriod,
        ["Dot"] = Keys.OemPeriod,
        ["Slash"] = Keys.OemQuestion,
        ["Question"] = Keys.OemQuestion,
        ["Backslash"] = Keys.OemBackslash,
        ["Semicolon"] = Keys.OemSemicolon,
        ["Quote"] = Keys.OemQuotes,
        ["Apostrophe"] = Keys.OemQuotes,
        ["LeftBracket"] = Keys.OemOpenBrackets,
        ["RightBracket"] = Keys.OemCloseBrackets,
        ["Backtick"] = Keys.Oemtilde,
        ["Grave"] = Keys.Oemtilde,
        ["Tilde"] = Keys.Oemtilde
    };

    public static bool TryParse(string? text, out HotkeyCombination combination)
    {
        combination = default;

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        ModifierKeys modifiers = ModifierKeys.None;
        Keys? key = null;

        foreach (var rawPart in text.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (TryParseModifier(rawPart, out var modifier))
            {
                modifiers |= modifier;
                continue;
            }

            if (key is not null || !TryParseKey(rawPart, out var parsedKey))
            {
                return false;
            }

            key = parsedKey;
        }

        if (key is null)
        {
            return false;
        }

        combination = new HotkeyCombination(modifiers, key.Value);
        return true;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        AppendModifier(builder, Modifiers, ModifierKeys.Control, "Ctrl");
        AppendModifier(builder, Modifiers, ModifierKeys.Alt, "Alt");
        AppendModifier(builder, Modifiers, ModifierKeys.Shift, "Shift");
        AppendModifier(builder, Modifiers, ModifierKeys.Win, "Win");

        if (builder.Length > 0)
        {
            builder.Append('+');
        }

        builder.Append(FormatKey(Key));

        return builder.ToString();
    }

    private static bool TryParseModifier(string part, out ModifierKeys modifier)
    {
        modifier = part.ToUpperInvariant() switch
        {
            "ALT" or "OPTION" or "CMD" or "COMMAND" => ModifierKeys.Alt,
            "CTRL" or "CONTROL" => ModifierKeys.Control,
            "SHIFT" => ModifierKeys.Shift,
            "WIN" or "WINDOWS" or "META" => ModifierKeys.Win,
            _ => ModifierKeys.None
        };

        return modifier != ModifierKeys.None;
    }

    private static bool TryParseKey(string part, out Keys key)
    {
        key = Keys.None;

        if (NamedKeys.TryGetValue(part, out key))
        {
            return true;
        }

        if (part.Length == 1)
        {
            var character = part[0];

            if (char.IsAsciiLetter(character))
            {
                key = Enum.Parse<Keys>(char.ToUpperInvariant(character).ToString());
                return true;
            }

            if (char.IsDigit(character))
            {
                key = Enum.Parse<Keys>($"D{character}");
                return true;
            }
        }

        if (part.StartsWith("F", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(part[1..], NumberStyles.None, CultureInfo.InvariantCulture, out var functionNumber)
            && functionNumber is >= 1 and <= 24)
        {
            key = Enum.Parse<Keys>($"F{functionNumber}");
            return true;
        }

        return Enum.TryParse(part, ignoreCase: true, out key);
    }

    private static string FormatKey(Keys key)
    {
        if (key is >= Keys.A and <= Keys.Z)
        {
            return key.ToString().ToUpperInvariant();
        }

        if (key is >= Keys.D0 and <= Keys.D9)
        {
            return ((int)key - (int)Keys.D0).ToString(CultureInfo.InvariantCulture);
        }

        return key switch
        {
            Keys.ControlKey => "Ctrl",
            Keys.Menu => "Alt",
            Keys.ShiftKey => "Shift",
            Keys.OemMinus => "Minus",
            Keys.Oemplus => "Plus",
            Keys.Oemcomma => "Comma",
            Keys.OemPeriod => "Period",
            Keys.OemQuestion => "Slash",
            Keys.OemBackslash => "Backslash",
            Keys.OemSemicolon => "Semicolon",
            Keys.OemQuotes => "Quote",
            Keys.OemOpenBrackets => "LeftBracket",
            Keys.OemCloseBrackets => "RightBracket",
            Keys.Oemtilde => "Backtick",
            _ => key.ToString()
        };
    }

    private static void AppendModifier(StringBuilder builder, ModifierKeys activeModifiers, ModifierKeys modifier, string label)
    {
        if (!activeModifiers.HasFlag(modifier))
        {
            return;
        }

        if (builder.Length > 0)
        {
            builder.Append('+');
        }

        builder.Append(label);
    }
}
