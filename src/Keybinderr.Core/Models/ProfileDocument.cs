namespace Keybinderr.Core.Models;

public sealed class ProfileDocument
{
    public int Version { get; set; } = 1;

    public AppSettings Settings { get; set; } = new();

    public List<KeyboardProfile> Profiles { get; set; } = [];
}

