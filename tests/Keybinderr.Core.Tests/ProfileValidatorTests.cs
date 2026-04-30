using Keybinderr.Core.Services;

namespace Keybinderr.Core.Tests;

public sealed class ProfileValidatorTests
{
    [Fact]
    public void Validate_ReportsDuplicateEnabledSourceKeys()
    {
        var document = DefaultProfiles.CreateDocument();
        var profile = document.Profiles.First(profile => profile.Id == DefaultProfiles.EsdfProfileId);
        profile.Mappings.Add(profile.Mappings[0]);

        var issues = ProfileValidator.Validate(document);

        Assert.Contains(issues, issue => issue.Message.Contains("Duplicate enabled mapping", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_AllowsDisabledDuplicateSourceKeys()
    {
        var document = DefaultProfiles.CreateDocument();
        var profile = document.Profiles.First(profile => profile.Id == DefaultProfiles.EsdfProfileId);
        var duplicate = profile.Mappings[0];
        profile.Mappings.Add(new()
        {
            SourceKey = duplicate.SourceKey,
            TargetKey = duplicate.TargetKey,
            Enabled = false
        });

        var issues = ProfileValidator.Validate(document);

        Assert.Empty(issues);
    }
}

