namespace AltCommand.Windows.Input;

[Flags]
internal enum ModifierKeys
{
    None = 0,
    Alt = 1 << 0,
    Control = 1 << 1,
    Shift = 1 << 2,
    Win = 1 << 3
}
