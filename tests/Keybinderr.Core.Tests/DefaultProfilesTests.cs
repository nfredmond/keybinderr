using Keybinderr.Core.Services;

namespace Keybinderr.Core.Tests;

public sealed class DefaultProfilesTests
{
    [Fact]
    public void CreateDocument_IncludesNormalAndEsdfProfiles()
    {
        var document = DefaultProfiles.CreateDocument();

        Assert.Contains(document.Profiles, profile => profile.Id == DefaultProfiles.NormalProfileId);
        Assert.Contains(document.Profiles, profile => profile.Id == DefaultProfiles.EsdfProfileId);
    }

    [Fact]
    public void EsdfPreset_MapsEsdfMovementToWasd()
    {
        var mappings = DefaultProfiles.CreateEsdfMappings()
            .ToDictionary(mapping => mapping.SourceKey, mapping => mapping.TargetKey);

        Assert.Equal("W", mappings["E"]);
        Assert.Equal("A", mappings["S"]);
        Assert.Equal("S", mappings["D"]);
        Assert.Equal("D", mappings["F"]);
    }
}

