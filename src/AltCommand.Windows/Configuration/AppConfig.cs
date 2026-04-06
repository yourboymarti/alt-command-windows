namespace AltCommand.Windows.Configuration;

internal sealed class AppConfig
{
    public List<string> DisabledProcesses { get; set; } = [];

    public List<RemapEntry> Remaps { get; set; } = [];

    public static AppConfig CreateDefault()
    {
        return new AppConfig
        {
            Remaps =
            [
                new RemapEntry("Alt+C", "Ctrl+C", "Copy"),
                new RemapEntry("Alt+V", "Ctrl+V", "Paste"),
                new RemapEntry("Alt+X", "Ctrl+X", "Cut"),
                new RemapEntry("Alt+A", "Ctrl+A", "Select all"),
                new RemapEntry("Alt+S", "Ctrl+S", "Save"),
                new RemapEntry("Alt+Z", "Ctrl+Z", "Undo"),
                new RemapEntry("Alt+Y", "Ctrl+Y", "Redo"),
                new RemapEntry("Alt+F", "Ctrl+F", "Find"),
                new RemapEntry("Alt+N", "Ctrl+N", "New"),
                new RemapEntry("Alt+O", "Ctrl+O", "Open"),
                new RemapEntry("Alt+P", "Ctrl+P", "Print"),
                new RemapEntry("Alt+W", "Ctrl+W", "Close tab"),
                new RemapEntry("Alt+T", "Ctrl+T", "New tab"),
                new RemapEntry("Alt+R", "Ctrl+R", "Refresh"),
                new RemapEntry("Alt+L", "Ctrl+L", "Focus address bar"),
                new RemapEntry("Alt+Q", "Alt+F4", "Quit active window"),
                new RemapEntry("Alt+Left", "Home", "Move to line start"),
                new RemapEntry("Alt+Right", "End", "Move to line end"),
                new RemapEntry("Alt+Shift+Left", "Shift+Home", "Select to line start"),
                new RemapEntry("Alt+Shift+Right", "Shift+End", "Select to line end"),
                new RemapEntry("Alt+Up", "Ctrl+Home", "Move to document start"),
                new RemapEntry("Alt+Down", "Ctrl+End", "Move to document end"),
                new RemapEntry("Alt+Shift+Up", "Ctrl+Shift+Home", "Select to document start"),
                new RemapEntry("Alt+Shift+Down", "Ctrl+Shift+End", "Select to document end"),
                new RemapEntry("Alt+Backspace", ["Shift+Home", "Backspace"], "Delete to line start"),
                new RemapEntry("Alt+Delete", ["Shift+End", "Delete"], "Delete to line end"),
                new RemapEntry("Alt+Shift+T", "Ctrl+Shift+T", "Reopen closed tab"),
                new RemapEntry("Alt+1", "Ctrl+1", "Jump to tab 1"),
                new RemapEntry("Alt+2", "Ctrl+2", "Jump to tab 2"),
                new RemapEntry("Alt+3", "Ctrl+3", "Jump to tab 3"),
                new RemapEntry("Alt+4", "Ctrl+4", "Jump to tab 4"),
                new RemapEntry("Alt+5", "Ctrl+5", "Jump to tab 5"),
                new RemapEntry("Alt+6", "Ctrl+6", "Jump to tab 6"),
                new RemapEntry("Alt+7", "Ctrl+7", "Jump to tab 7"),
                new RemapEntry("Alt+8", "Ctrl+8", "Jump to tab 8"),
                new RemapEntry("Alt+9", "Ctrl+9", "Jump to last tab")
            ]
        };
    }
}
