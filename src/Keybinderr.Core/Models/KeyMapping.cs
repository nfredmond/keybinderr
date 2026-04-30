namespace Keybinderr.Core.Models;

public sealed class KeyMapping
{
    public string SourceKey { get; set; } = string.Empty;

    public string TargetKey { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public string SourceKeyCanonical => CanonicalizeKey(SourceKey);

    public string TargetKeyCanonical => CanonicalizeKey(TargetKey);

    public static string CanonicalizeKey(string? key)
    {
        return string.IsNullOrWhiteSpace(key)
            ? string.Empty
            : key.Trim().ToUpperInvariant();
    }
}

