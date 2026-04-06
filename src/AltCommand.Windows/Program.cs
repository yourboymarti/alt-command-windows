namespace AltCommand.Windows;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var context = new HotkeyApplicationContext();
        Application.Run(context);
    }
}
