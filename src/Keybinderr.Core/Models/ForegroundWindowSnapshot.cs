namespace Keybinderr.Core.Models;

public sealed class ForegroundWindowSnapshot
{
    public int ProcessId { get; init; }

    public string? ProcessName { get; init; }

    public string? ProcessPath { get; init; }

    public string? WindowTitle { get; init; }
}

