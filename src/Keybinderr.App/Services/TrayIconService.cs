using System.Drawing;
using System.Windows;
using Keybinderr.Core.Services;
using WinForms = System.Windows.Forms;

namespace Keybinderr.App.Services;

public sealed class TrayIconService : IDisposable
{
    private readonly ProfileRuntime _profileRuntime;
    private readonly ActiveProfileService _activeProfileService;
    private readonly KeyboardHookService _keyboardHookService;
    private readonly Action _showSettings;
    private readonly Action _shutdown;
    private readonly WinForms.NotifyIcon _notifyIcon;
    private readonly WinForms.ContextMenuStrip _menu;

    public TrayIconService(
        ProfileRuntime profileRuntime,
        ActiveProfileService activeProfileService,
        KeyboardHookService keyboardHookService,
        Action showSettings,
        Action shutdown)
    {
        _profileRuntime = profileRuntime;
        _activeProfileService = activeProfileService;
        _keyboardHookService = keyboardHookService;
        _showSettings = showSettings;
        _shutdown = shutdown;

        _menu = new WinForms.ContextMenuStrip();
        _menu.Opening += (_, _) => BuildMenu();

        _notifyIcon = new WinForms.NotifyIcon
        {
            Icon = SystemIcons.Application,
            ContextMenuStrip = _menu,
            Visible = false
        };
        _notifyIcon.DoubleClick += (_, _) => _showSettings();

        _activeProfileService.ActiveProfileChanged += (_, _) => UpdateTooltip();
        _profileRuntime.ProfilesChanged += (_, _) => UpdateTooltip();
    }

    public void Start()
    {
        BuildMenu();
        UpdateTooltip();
        _notifyIcon.Visible = true;
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _menu.Dispose();
    }

    private void BuildMenu()
    {
        _menu.Items.Clear();

        var activeProfile = _activeProfileService.ActiveProfile;
        _menu.Items.Add(new WinForms.ToolStripMenuItem($"Active profile: {activeProfile.Name}") { Enabled = false });
        _menu.Items.Add(new WinForms.ToolStripMenuItem($"Settings file: {_profileRuntime.ConfigPath}") { Enabled = false });
        _menu.Items.Add(new WinForms.ToolStripSeparator());

        var pausedItem = new WinForms.ToolStripMenuItem("Pause all remapping")
        {
            Checked = _profileRuntime.Document.Settings.RemappingPaused
        };
        pausedItem.Click += (_, _) =>
        {
            var document = _profileRuntime.Document.DeepClone();
            document.Settings.RemappingPaused = !document.Settings.RemappingPaused;
            _profileRuntime.Save(document);
        };
        _menu.Items.Add(pausedItem);

        var profilesMenu = new WinForms.ToolStripMenuItem("Profiles");
        foreach (var profile in _profileRuntime.Document.Profiles)
        {
            profilesMenu.DropDownItems.Add(new WinForms.ToolStripMenuItem(profile.Name)
            {
                Checked = profile.Id == activeProfile.Id,
                Enabled = false
            });
        }
        _menu.Items.Add(profilesMenu);

        _menu.Items.Add(new WinForms.ToolStripSeparator());

        var openSettingsItem = new WinForms.ToolStripMenuItem("Open settings…");
        openSettingsItem.Click += (_, _) => _showSettings();
        _menu.Items.Add(openSettingsItem);

        var exitItem = new WinForms.ToolStripMenuItem("Exit Keybinderr");
        exitItem.Click += (_, _) =>
        {
            _keyboardHookService.Stop();
            _shutdown();
        };
        _menu.Items.Add(exitItem);
    }

    private void UpdateTooltip()
    {
        var status = _profileRuntime.Document.Settings.RemappingPaused
            ? "Paused"
            : _activeProfileService.ActiveProfile.Name;

        var text = $"Keybinderr - {status}";
        _notifyIcon.Text = text.Length <= 63 ? text : text[..63];
    }
}

