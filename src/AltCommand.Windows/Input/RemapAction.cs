namespace AltCommand.Windows.Input;

internal sealed class RemapAction
{
    public RemapAction(IReadOnlyList<HotkeyCombination> steps)
    {
        if (steps.Count == 0)
        {
            throw new ArgumentException("A remap action must contain at least one shortcut step.", nameof(steps));
        }

        Steps = steps;
    }

    public IReadOnlyList<HotkeyCombination> Steps { get; }
}
