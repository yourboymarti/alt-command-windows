namespace AltCommand.Windows.Configuration;

internal sealed class RemapEntry
{
    public RemapEntry()
    {
    }

    public RemapEntry(string from, string to, string? description = null)
    {
        From = from;
        To = to;
        Description = description;
    }

    public string From { get; set; } = string.Empty;

    public string To { get; set; } = string.Empty;

    public string? Description { get; set; }
}
