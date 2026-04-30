using System.Text.Json;
using Keybinderr.Core.Models;

namespace Keybinderr.Core.Services;

public static class ProfileDocumentExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public static ProfileDocument DeepClone(this ProfileDocument document)
    {
        var json = JsonSerializer.Serialize(document, SerializerOptions);
        return JsonSerializer.Deserialize<ProfileDocument>(json, SerializerOptions)
            ?? DefaultProfiles.CreateDocument();
    }
}

