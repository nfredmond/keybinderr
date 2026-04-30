using System.Text.Json;
using Keybinderr.Core.Models;

namespace Keybinderr.Core.Services;

public sealed class JsonProfileRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public JsonProfileRepository(string filePath)
    {
        FilePath = filePath;
    }

    public string FilePath { get; }

    public ProfileDocument Load()
    {
        if (!File.Exists(FilePath))
        {
            return DefaultProfiles.CreateDocument();
        }

        ProfileDocument document;
        try
        {
            using var stream = File.OpenRead(FilePath);
            document = JsonSerializer.Deserialize<ProfileDocument>(stream, SerializerOptions)
                ?? DefaultProfiles.CreateDocument();
        }
        catch (JsonException)
        {
            document = DefaultProfiles.CreateDocument();
        }

        DefaultProfiles.EnsureDefaults(document);
        return document;
    }

    public void Save(ProfileDocument document)
    {
        DefaultProfiles.EnsureDefaults(document);

        var directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = $"{FilePath}.tmp";
        using (var stream = File.Create(tempPath))
        {
            JsonSerializer.Serialize(stream, document, SerializerOptions);
        }

        File.Move(tempPath, FilePath, overwrite: true);
    }
}
