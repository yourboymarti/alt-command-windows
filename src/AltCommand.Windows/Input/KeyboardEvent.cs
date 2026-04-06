namespace AltCommand.Windows.Input;

internal readonly record struct KeyboardEvent(Keys Key, bool IsKeyDown, bool IsKeyUp, bool IsInjected);
