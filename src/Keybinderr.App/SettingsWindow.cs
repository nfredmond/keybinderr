using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Keybinderr.App.Services;
using Keybinderr.Core.Models;
using Keybinderr.Core.Services;
using Microsoft.Win32;
using Binding = System.Windows.Data.Binding;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using Control = System.Windows.Controls.Control;
using DataGrid = System.Windows.Controls.DataGrid;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Label = System.Windows.Controls.Label;
using ListBox = System.Windows.Controls.ListBox;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Orientation = System.Windows.Controls.Orientation;
using SelectionMode = System.Windows.Controls.SelectionMode;
using SystemColors = System.Windows.SystemColors;
using TextBox = System.Windows.Controls.TextBox;

namespace Keybinderr.App;

public sealed class SettingsWindow : Window
{
    private static readonly string[] KeyChoices =
    [
        "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
        "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
        "D0", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9",
        "Space", "Tab", "Escape", "Enter", "Back", "Left", "Right", "Up", "Down",
        "LShiftKey", "RShiftKey", "LControlKey", "RControlKey", "LMenu", "RMenu",
        "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12"
    ];

    private readonly ProfileRuntime _profileRuntime;
    private readonly ListBox _profileList = new();
    private readonly TextBox _nameText = new();
    private readonly TextBox _executableText = new();
    private readonly TextBox _titleText = new();
    private readonly CheckBox _enabledCheckBox = new() { Content = "Profile enabled" };
    private readonly CheckBox _startWithWindowsCheckBox = new() { Content = "Launch Keybinderr when Windows starts" };
    private readonly CheckBox _pauseCheckBox = new() { Content = "Pause all remapping" };
    private readonly DataGrid _mappingGrid = new();
    private readonly Button _deleteButton = new() { Content = "_Delete profile", MinWidth = 104 };
    private readonly Button _applyEsdfButton = new() { Content = "Use _ESDF preset", MinWidth = 120 };
    private readonly Button _addMappingButton = new() { Content = "_Add mapping", MinWidth = 102 };
    private readonly Button _removeMappingButton = new() { Content = "_Remove selected", MinWidth = 124 };
    private readonly TextBlock _editorNoticeText = new();
    private readonly TextBlock _mappingEmptyText = new();
    private ObservableCollection<KeyMapping> _mappings = [];
    private ProfileDocument _workingDocument;
    private KeyboardProfile? _selectedProfile;
    private bool _isLoadingProfile;

    public SettingsWindow(ProfileRuntime profileRuntime)
    {
        _profileRuntime = profileRuntime;
        _workingDocument = profileRuntime.Document.DeepClone();

        Title = "Keybinderr — Profiles & Mappings";
        Width = 980;
        Height = 680;
        MinWidth = 840;
        MinHeight = 560;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        Content = BuildContent();
        LoadProfiles();
    }

    private UIElement BuildContent()
    {
        var root = new DockPanel { Margin = new Thickness(16) };
        KeyboardNavigation.SetTabNavigation(root, KeyboardNavigationMode.Continue);

        var footer = BuildFooter();
        DockPanel.SetDock(footer, Dock.Bottom);
        root.Children.Add(footer);

        var header = BuildHeader();
        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        var mainGrid = new Grid();
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250) });
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var profilePanel = BuildProfilePanel();
        Grid.SetColumn(profilePanel, 0);
        mainGrid.Children.Add(profilePanel);

        var editorPanel = BuildEditorPanel();
        Grid.SetColumn(editorPanel, 1);
        mainGrid.Children.Add(editorPanel);

        root.Children.Add(mainGrid);
        return root;
    }

    private static UIElement BuildHeader()
    {
        var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 14) };
        panel.Children.Add(new TextBlock
        {
            Text = "Keybinderr",
            FontSize = 22,
            FontWeight = FontWeights.SemiBold
        });
        panel.Children.Add(new TextBlock
        {
            Text = "Map keys only while a matching game window is active. Keep Normal / QWERTY as the safety profile.",
            Foreground = SystemColors.GrayTextBrush,
            Margin = new Thickness(0, 3, 0, 0),
            TextWrapping = TextWrapping.Wrap
        });

        return panel;
    }

    private UIElement BuildProfilePanel()
    {
        var panel = new DockPanel { Margin = new Thickness(0, 0, 16, 0) };
        var heading = new TextBlock
        {
            Text = "Profiles",
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 6)
        };
        DockPanel.SetDock(heading, Dock.Top);
        panel.Children.Add(heading);

        var help = new TextBlock
        {
            Text = "Enabled profiles activate when their executable and optional title match the foreground window.",
            Foreground = SystemColors.GrayTextBrush,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 8)
        };
        DockPanel.SetDock(help, Dock.Top);
        panel.Children.Add(help);

        var buttons = new UniformGrid
        {
            Columns = 2,
            Margin = new Thickness(0, 8, 0, 0)
        };
        DockPanel.SetDock(buttons, Dock.Bottom);

        var newButton = new Button { Content = "_New profile", Margin = new Thickness(0, 0, 4, 0), MinWidth = 104 };
        newButton.Click += (_, _) => AddProfile();
        buttons.Children.Add(newButton);

        _deleteButton.Margin = new Thickness(4, 0, 0, 0);
        _deleteButton.Click += (_, _) => DeleteProfile();
        buttons.Children.Add(_deleteButton);

        panel.Children.Add(buttons);

        _profileList.DisplayMemberPath = nameof(KeyboardProfile.Name);
        _profileList.SelectionMode = SelectionMode.Single;
        _profileList.ToolTip = "Select a profile to edit its matching rules and mappings.";
        _profileList.SelectionChanged += (_, _) => SelectProfile(_profileList.SelectedItem as KeyboardProfile);
        panel.Children.Add(_profileList);

        return panel;
    }

    private UIElement BuildEditorPanel()
    {
        var panel = new DockPanel();

        _editorNoticeText.Foreground = SystemColors.GrayTextBrush;
        _editorNoticeText.TextWrapping = TextWrapping.Wrap;
        _editorNoticeText.Margin = new Thickness(0, 0, 0, 10);
        DockPanel.SetDock(_editorNoticeText, Dock.Top);
        panel.Children.Add(_editorNoticeText);

        var form = new Grid { Margin = new Thickness(0, 0, 0, 10) };
        form.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        form.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        form.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        for (var i = 0; i < 4; i++)
        {
            form.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        AddLabel(form, "_Name", _nameText, row: 0);
        AddControl(form, _nameText, row: 0);
        _nameText.ToolTip = "Display name shown in the tray menu and profile list.";

        AddLabel(form, "Game _.exe", _executableText, row: 1);
        AddControl(form, _executableText, row: 1);
        _executableText.ToolTip = "Use a full path for precision, or an executable name like game.exe.";
        var browseButton = new Button { Content = "_Browse…", Margin = new Thickness(8, 4, 0, 4), MinWidth = 86 };
        browseButton.Click += (_, _) => BrowseForExecutable();
        Grid.SetRow(browseButton, 1);
        Grid.SetColumn(browseButton, 2);
        form.Children.Add(browseButton);

        AddLabel(form, "Window _title contains", _titleText, row: 2);
        AddControl(form, _titleText, row: 2);
        _titleText.ToolTip = "Optional. Leave blank unless the same executable hosts multiple games or modes.";

        Grid.SetRow(_enabledCheckBox, 3);
        Grid.SetColumn(_enabledCheckBox, 1);
        _enabledCheckBox.Margin = new Thickness(0, 4, 0, 4);
        form.Children.Add(_enabledCheckBox);

        DockPanel.SetDock(form, Dock.Top);
        panel.Children.Add(form);

        var mappingHeader = new DockPanel
        {
            LastChildFill = false,
            Margin = new Thickness(0, 4, 0, 6)
        };
        DockPanel.SetDock(mappingHeader, Dock.Top);
        mappingHeader.Children.Add(new TextBlock
        {
            Text = "Mappings",
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        });

        var mappingButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        DockPanel.SetDock(mappingButtons, Dock.Right);

        _applyEsdfButton.Margin = new Thickness(0, 0, 8, 0);
        _applyEsdfButton.ToolTip = "Replace this profile's mappings with the standard ESDF movement remap.";
        _applyEsdfButton.Click += (_, _) => ApplyEsdfPreset();
        mappingButtons.Children.Add(_applyEsdfButton);

        _addMappingButton.Margin = new Thickness(0, 0, 8, 0);
        _addMappingButton.Click += (_, _) => AddMapping();
        mappingButtons.Children.Add(_addMappingButton);

        _removeMappingButton.Click += (_, _) => RemoveSelectedMapping();
        mappingButtons.Children.Add(_removeMappingButton);

        mappingHeader.Children.Add(mappingButtons);
        panel.Children.Add(mappingHeader);

        _mappingEmptyText.Foreground = SystemColors.GrayTextBrush;
        _mappingEmptyText.TextWrapping = TextWrapping.Wrap;
        _mappingEmptyText.Margin = new Thickness(0, 0, 0, 8);
        DockPanel.SetDock(_mappingEmptyText, Dock.Top);
        panel.Children.Add(_mappingEmptyText);

        _mappingGrid.AutoGenerateColumns = false;
        _mappingGrid.CanUserAddRows = false;
        _mappingGrid.HeadersVisibility = DataGridHeadersVisibility.Column;
        _mappingGrid.GridLinesVisibility = DataGridGridLinesVisibility.Horizontal;
        _mappingGrid.SelectionMode = DataGridSelectionMode.Single;
        _mappingGrid.SelectionUnit = DataGridSelectionUnit.FullRow;
        _mappingGrid.MinHeight = 210;
        _mappingGrid.SelectionChanged += (_, _) => UpdateActionStates();
        _mappingGrid.Columns.Add(new DataGridComboBoxColumn
        {
            Header = "Pressed key",
            ItemsSource = KeyChoices,
            SelectedItemBinding = new Binding(nameof(KeyMapping.SourceKey)) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });
        _mappingGrid.Columns.Add(new DataGridComboBoxColumn
        {
            Header = "Send key",
            ItemsSource = KeyChoices,
            SelectedItemBinding = new Binding(nameof(KeyMapping.TargetKey)) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });
        _mappingGrid.Columns.Add(new DataGridCheckBoxColumn
        {
            Header = "Enabled",
            Binding = new Binding(nameof(KeyMapping.Enabled)) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
            Width = DataGridLength.SizeToHeader
        });
        panel.Children.Add(_mappingGrid);

        return panel;
    }

    private UIElement BuildFooter()
    {
        var footer = new DockPanel { Margin = new Thickness(0, 14, 0, 0) };

        var options = new StackPanel { Orientation = Orientation.Vertical };
        options.Children.Add(_startWithWindowsCheckBox);
        options.Children.Add(_pauseCheckBox);
        DockPanel.SetDock(options, Dock.Left);
        footer.Children.Add(options);

        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var saveButton = new Button
        {
            Content = "_Save",
            IsDefault = true,
            MinWidth = 92,
            Margin = new Thickness(8, 0, 0, 0)
        };
        saveButton.Click += (_, _) => SaveAndClose();
        actions.Children.Add(saveButton);

        var cancelButton = new Button
        {
            Content = "_Cancel",
            MinWidth = 92,
            Margin = new Thickness(8, 0, 0, 0)
        };
        cancelButton.Click += (_, _) => Close();
        actions.Children.Add(cancelButton);

        footer.Children.Add(actions);
        return footer;
    }

    private static void AddLabel(Grid grid, string text, Control target, int row)
    {
        var label = new Label
        {
            Content = text,
            Target = target,
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Thickness(0),
            Margin = new Thickness(0, 4, 12, 4)
        };
        Grid.SetRow(label, row);
        Grid.SetColumn(label, 0);
        grid.Children.Add(label);
    }

    private static void AddControl(Grid grid, Control control, int row)
    {
        control.Margin = new Thickness(0, 4, 0, 4);
        Grid.SetRow(control, row);
        Grid.SetColumn(control, 1);
        grid.Children.Add(control);
    }

    private void LoadProfiles()
    {
        _startWithWindowsCheckBox.IsChecked = StartupService.IsRunAtLoginEnabled();
        _pauseCheckBox.IsChecked = _workingDocument.Settings.RemappingPaused;
        _profileList.ItemsSource = _workingDocument.Profiles;
        _profileList.SelectedItem = _workingDocument.Profiles.FirstOrDefault(profile => profile.Id == DefaultProfiles.EsdfProfileId)
            ?? _workingDocument.Profiles.FirstOrDefault();
    }

    private void SelectProfile(KeyboardProfile? profile)
    {
        if (_isLoadingProfile)
        {
            return;
        }

        WriteEditorToSelectedProfile();
        _selectedProfile = profile;
        LoadSelectedProfile();
    }

    private void LoadSelectedProfile()
    {
        _isLoadingProfile = true;

        var profile = _selectedProfile;
        _nameText.Text = profile?.Name ?? string.Empty;
        _executableText.Text = profile?.ExecutablePath ?? string.Empty;
        _titleText.Text = profile?.WindowTitleMatch ?? string.Empty;
        _enabledCheckBox.IsChecked = profile?.Enabled ?? false;

        ReplaceMappings(profile?.Mappings.Select(CloneMapping) ?? []);

        var canEditMappings = profile is not null && profile.Id != DefaultProfiles.NormalProfileId;
        _nameText.IsEnabled = profile is not null && profile.Id != DefaultProfiles.NormalProfileId;
        _executableText.IsEnabled = canEditMappings;
        _titleText.IsEnabled = canEditMappings;
        _enabledCheckBox.IsEnabled = canEditMappings;
        _mappingGrid.IsEnabled = canEditMappings;
        _applyEsdfButton.IsEnabled = canEditMappings;
        _addMappingButton.IsEnabled = canEditMappings;
        _deleteButton.IsEnabled = profile is not null && !profile.IsBuiltIn;

        UpdateEditorNotice();
        UpdateActionStates();
        UpdateMappingEmptyState();

        _isLoadingProfile = false;
    }

    private void WriteEditorToSelectedProfile()
    {
        if (_selectedProfile is null)
        {
            return;
        }

        _mappingGrid.CommitEdit(DataGridEditingUnit.Cell, exitEditingMode: true);
        _mappingGrid.CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true);

        if (_selectedProfile.Id != DefaultProfiles.NormalProfileId)
        {
            _selectedProfile.Name = string.IsNullOrWhiteSpace(_nameText.Text)
                ? "Unnamed Profile"
                : _nameText.Text.Trim();
            _selectedProfile.ExecutablePath = string.IsNullOrWhiteSpace(_executableText.Text) ? null : _executableText.Text.Trim();
            _selectedProfile.WindowTitleMatch = string.IsNullOrWhiteSpace(_titleText.Text) ? null : _titleText.Text.Trim();
            _selectedProfile.Enabled = _enabledCheckBox.IsChecked == true;
            _selectedProfile.Mappings = _mappings.Select(CloneMapping).ToList();
        }

        _profileList.Items.Refresh();
        UpdateEditorNotice();
    }

    private void AddProfile()
    {
        WriteEditorToSelectedProfile();
        var profile = DefaultProfiles.CreateGameProfile();
        _workingDocument.Profiles.Add(profile);
        _profileList.Items.Refresh();
        _profileList.SelectedItem = profile;
        _nameText.Focus();
        _nameText.SelectAll();
    }

    private void DeleteProfile()
    {
        if (_selectedProfile is null || _selectedProfile.IsBuiltIn)
        {
            return;
        }

        var result = MessageBox.Show(
            this,
            $"Delete '{_selectedProfile.Name}'? This removes the profile and its mappings from the working settings.",
            "Delete profile",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);
        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        var deletedIndex = _workingDocument.Profiles.IndexOf(_selectedProfile);
        _workingDocument.Profiles.Remove(_selectedProfile);
        _profileList.Items.Refresh();
        _profileList.SelectedItem = _workingDocument.Profiles.ElementAtOrDefault(Math.Max(0, deletedIndex - 1))
            ?? _workingDocument.Profiles.FirstOrDefault();
    }

    private void AddMapping()
    {
        var key = GetFirstUnusedSourceKey();
        var mapping = new KeyMapping { SourceKey = key, TargetKey = key };
        _mappings.Add(mapping);
        _mappingGrid.SelectedItem = mapping;
        _mappingGrid.ScrollIntoView(mapping);
        _mappingGrid.Focus();
    }

    private void RemoveSelectedMapping()
    {
        if (_mappingGrid.SelectedItem is not KeyMapping mapping)
        {
            return;
        }

        _mappings.Remove(mapping);
        UpdateActionStates();
    }

    private string GetFirstUnusedSourceKey()
    {
        var usedSourceKeys = _mappings
            .Select(mapping => mapping.SourceKeyCanonical)
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return KeyChoices.FirstOrDefault(key => !usedSourceKeys.Contains(key)) ?? KeyChoices[0];
    }

    private void ApplyEsdfPreset()
    {
        _mappings.Clear();
        foreach (var mapping in DefaultProfiles.CreateEsdfMappings())
        {
            _mappings.Add(mapping);
        }
    }

    private void BrowseForExecutable()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select game executable",
            Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog(this) == true)
        {
            _executableText.Text = dialog.FileName;
        }
    }

    private void SaveAndClose()
    {
        try
        {
            WriteEditorToSelectedProfile();
            _workingDocument.Settings.StartWithWindows = _startWithWindowsCheckBox.IsChecked == true;
            _workingDocument.Settings.RemappingPaused = _pauseCheckBox.IsChecked == true;

            var issues = ProfileValidator.Validate(_workingDocument)
                .Where(issue => issue.Severity == ValidationSeverity.Error)
                .ToList();
            if (issues.Count > 0)
            {
                MessageBox.Show(
                    this,
                    string.Join(Environment.NewLine, issues.Select(issue => $"{issue.ProfileName}: {issue.Message}")),
                    "Fix profiles before saving",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            StartupService.SetRunAtLogin(_workingDocument.Settings.StartWithWindows);
            _profileRuntime.Save(_workingDocument.DeepClone());
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Unable to save settings", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ReplaceMappings(IEnumerable<KeyMapping> mappings)
    {
        _mappings.CollectionChanged -= MappingsChanged;
        _mappings = new ObservableCollection<KeyMapping>(mappings);
        _mappings.CollectionChanged += MappingsChanged;
        _mappingGrid.ItemsSource = _mappings;
    }

    private void MappingsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateActionStates();
        UpdateMappingEmptyState();
    }

    private void UpdateEditorNotice()
    {
        _editorNoticeText.Text = _selectedProfile switch
        {
            null => "Select a profile, or create one for a specific game executable.",
            { Id: DefaultProfiles.NormalProfileId } => "Normal / QWERTY is the fallback safety profile. It is active when remapping is paused or no game profile matches.",
            { IsBuiltIn: true } => "Built-in profile. You can tune its mappings, but it is protected from deletion.",
            _ => "Custom profile. Match by executable first; use the window title field only when one .exe hosts multiple games or modes."
        };
    }

    private void UpdateActionStates()
    {
        var canEditMappings = _selectedProfile is not null && _selectedProfile.Id != DefaultProfiles.NormalProfileId;
        _removeMappingButton.IsEnabled = canEditMappings && _mappingGrid.SelectedItem is KeyMapping;
    }

    private void UpdateMappingEmptyState()
    {
        _mappingEmptyText.Text = _mappings.Count == 0
            ? "No mappings in this profile. Add a row to translate one pressed key into another, or leave it empty to pass keys through."
            : "Pressed key is what you physically press; send key is what Keybinderr emits while this profile is active.";
    }

    private static KeyMapping CloneMapping(KeyMapping mapping)
    {
        return new KeyMapping
        {
            SourceKey = mapping.SourceKey,
            TargetKey = mapping.TargetKey,
            Enabled = mapping.Enabled
        };
    }
}
