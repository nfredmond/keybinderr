using Keybinderr.Core.Models;
using Keybinderr.Core.Services;

namespace Keybinderr.Core.Tests;

public sealed class ProfileMatcherTests
{
    [Fact]
    public void ResolveActiveProfile_ReturnsNormalWhenPaused()
    {
        var document = DefaultProfiles.CreateDocument();
        document.Settings.RemappingPaused = true;
        document.Profiles.First(profile => profile.Id == DefaultProfiles.EsdfProfileId).ExecutablePath = "Game.exe";

        var active = ProfileMatcher.ResolveActiveProfile(document, new ForegroundWindowSnapshot
        {
            ProcessName = "Game",
            ProcessPath = @"C:\Games\Game.exe",
            WindowTitle = "Game"
        });

        Assert.Equal(DefaultProfiles.NormalProfileId, active.Id);
    }

    [Fact]
    public void ResolveActiveProfile_MatchesByExecutableFileName()
    {
        var document = DefaultProfiles.CreateDocument();
        var esdfProfile = document.Profiles.First(profile => profile.Id == DefaultProfiles.EsdfProfileId);
        esdfProfile.ExecutablePath = "ActionRpg.exe";

        var active = ProfileMatcher.ResolveActiveProfile(document, new ForegroundWindowSnapshot
        {
            ProcessName = "ActionRpg",
            ProcessPath = @"D:\Steam\ActionRpg.exe",
            WindowTitle = "Action RPG"
        });

        Assert.Equal(DefaultProfiles.EsdfProfileId, active.Id);
    }

    [Fact]
    public void ResolveActiveProfile_RequiresWindowTitleWhenConfigured()
    {
        var document = DefaultProfiles.CreateDocument();
        var esdfProfile = document.Profiles.First(profile => profile.Id == DefaultProfiles.EsdfProfileId);
        esdfProfile.ExecutablePath = "Launcher.exe";
        esdfProfile.WindowTitleMatch = "In Game";

        var active = ProfileMatcher.ResolveActiveProfile(document, new ForegroundWindowSnapshot
        {
            ProcessName = "Launcher",
            ProcessPath = @"D:\Games\Launcher.exe",
            WindowTitle = "Patch Notes"
        });

        Assert.Equal(DefaultProfiles.NormalProfileId, active.Id);
    }

    [Fact]
    public void BuildMappingDictionary_IgnoresDisabledMappings()
    {
        var profile = DefaultProfiles.CreateEsdfProfile();
        profile.Mappings[0].Enabled = false;

        var mappings = ProfileMatcher.BuildMappingDictionary(profile);

        Assert.False(mappings.ContainsKey("E"));
        Assert.Equal("A", mappings["S"]);
    }
}

