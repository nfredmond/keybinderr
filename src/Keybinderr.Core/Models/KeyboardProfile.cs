namespace Keybinderr.Core.Models;

public sealed class KeyboardProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString("n");

    public string Name { get; set; } = "New Profile";

    public bool Enabled { get; set; } = true;

    public bool IsBuiltIn { get; set; }

    public string? ExecutablePath { get; set; }

    public string? WindowTitleMatch { get; set; }

    public List<KeyMapping> Mappings { get; set; } = [];
}

