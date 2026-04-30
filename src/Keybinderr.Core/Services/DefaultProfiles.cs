using Keybinderr.Core.Models;

namespace Keybinderr.Core.Services;

public static class DefaultProfiles
{
    public const string NormalProfileId = "normal-qwerty";
    public const string EsdfProfileId = "esdf-rpg";

    public static ProfileDocument CreateDocument()
    {
        var document = new ProfileDocument();
        EnsureDefaults(document);
        return document;
    }

    public static KeyboardProfile CreateNormalProfile()
    {
        return new KeyboardProfile
        {
            Id = NormalProfileId,
            Name = "Normal / QWERTY",
            Enabled = true,
            IsBuiltIn = true,
            Mappings = []
        };
    }

    public static KeyboardProfile CreateEsdfProfile()
    {
        return new KeyboardProfile
        {
            Id = EsdfProfileId,
            Name = "ESDF RPG",
            Enabled = true,
            IsBuiltIn = true,
            Mappings = CreateEsdfMappings()
        };
    }

    public static KeyboardProfile CreateGameProfile(string name = "New Game Profile")
    {
        return new KeyboardProfile
        {
            Name = name,
            Enabled = true,
            Mappings = []
        };
    }

    public static List<KeyMapping> CreateEsdfMappings()
    {
        return
        [
            new KeyMapping { SourceKey = "E", TargetKey = "W" },
            new KeyMapping { SourceKey = "S", TargetKey = "A" },
            new KeyMapping { SourceKey = "D", TargetKey = "S" },
            new KeyMapping { SourceKey = "F", TargetKey = "D" }
        ];
    }

    public static void EnsureDefaults(ProfileDocument document)
    {
        if (document.Settings is null)
        {
            document.Settings = new AppSettings();
        }

        document.Profiles ??= [];

        if (document.Profiles.All(profile => profile.Id != NormalProfileId))
        {
            document.Profiles.Insert(0, CreateNormalProfile());
        }

        if (document.Profiles.All(profile => profile.Id != EsdfProfileId))
        {
            document.Profiles.Add(CreateEsdfProfile());
        }
    }
}
