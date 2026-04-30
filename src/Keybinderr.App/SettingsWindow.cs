using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Keybinderr.App.Services;
using Keybinderr.Core.Models;
using Keybinderr.Core.Services;
using Microsoft.Win32;

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
    private readonly Button _deleteButton = new() { Content = "Delete" };
    private readonly Button _applyEsdfButton = new() { Content = "Use ESDF preset" };
    private ObservableCollection<KeyMapping> _mappings = [];
    private ProfileDocument _workingDocument;
    private KeyboardProfile? _selectedProfile;
    private bool _isLoadingProfile;

    public SettingsWindow(ProfileRuntime profileRuntime)
    {
        _profileRuntime = profileRuntime;
        _workingDocument = profileRuntime.Document.DeepClone();

        Title = "Keybinderr Settings";
        Width = 900;
        Height = 620;
        MinWidth = 760;
        MinHeight = 520;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        Content = BuildContent();
        LoadProfiles();
    }

    private UIElement BuildContent()
    {
        var root = new DockPanel { Margin = new Thickness(12) };

        var footer = BuildFooter();
        DockPanel.SetDock(footer, Dock.Bottom);
        root.Children.Add(footer);

        var mainGrid = new Grid();
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(230) });
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

    private UIElement BuildProfilePanel()
    {
        var panel = new DockPanel { Margin = new Thickness(0, 0, 12, 0) };
        var heading = new TextBlock
        {
            Text = "Profiles",
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 8)
        };
        DockPanel.SetDock(heading, Dock.Top);
        panel.Children.Add(heading);

        var buttons = new UniformGrid
        {
            Columns = 2,
            Margin = new Thickness(0, 8, 0, 0)
        };
        DockPanel.SetDock(buttons, Dock.Bottom);

        var newButton = new Button { Content = "New", Margin = new Thickness(0, 0, 4, 0) };
        newButton.Click += (_, _) => AddProfile();
        buttons.Children.Add(newButton);

        _deleteButton.Margin = new Thickness(4, 0, 0, 0);
        _deleteButton.Click += (_, _) => DeleteProfile();
        buttons.Children.Add(_deleteButton);

        panel.Children.Add(buttons);

        _profileList.DisplayMemberPath = nameof(KeyboardProfile.Name);
        _profileList.SelectionChanged += (_, _) => SelectProfile(_profileList.SelectedItem as KeyboardProfile);
        panel.Children.Add(_profileList);

        return panel;
    }

    private UIElement BuildEditorPanel()
    {
        var panel = new DockPanel();

        var form = new Grid { Margin = new Thickness(0, 0, 0, 8) };
        form.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
        form.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        form.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        form.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        form.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        form.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        form.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        AddLabel(form, "Name", row: 0);
        AddControl(form, _nameText, row: 0);

        AddLabel(form, "Game .exe", row: 1);
        AddControl(form, _executableText, row: 1);
        var browseButton = new Button { Content = "Browse", Margin = new Thickness(8, 4, 0, 4), MinWidth = 76 };
        browseButton.Click += (_, _) => BrowseForExecutable();
        Grid.SetRow(browseButton, 1);
        Grid.SetColumn(browseButton, 2);
        form.Children.Add(browseButton);

        AddLabel(form, "Window title", row: 2);
        AddControl(form, _titleText, row: 2);

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
        _applyEsdfButton.Click += (_, _) => ApplyEsdfPreset();
        mappingButtons.Children.Add(_applyEsdfButton);

        var addMappingButton = new Button { Content = "Add", Margin = new Thickness(0, 0, 8, 0), MinWidth = 64 };
        addMappingButton.Click += (_, _) => _mappings.Add(new KeyMapping { SourceKey = "E", TargetKey = "W" });
        mappingButtons.Children.Add(addMappingButton);

        var removeMappingButton = new Button { Content = "Remove", MinWidth = 74 };
        removeMappingButton.Click += (_, _) =>
        {
            if (_mappingGrid.SelectedItem is KeyMapping mapping)
            {
                _mappings.Remove(mapping);
            }
        };
        mappingButtons.Children.Add(removeMappingButton);

        mappingHeader.Children.Add(mappingButtons);
        panel.Children.Add(mappingHeader);

        _mappingGrid.AutoGenerateColumns = false;
        _mappingGrid.CanUserAddRows = false;
        _mappingGrid.Columns.Add(new DataGridComboBoxColumn
        {
            Header = "Pressed key",
            ItemsSource = KeyChoices,
            SelectedItemBinding = new System.Windows.Data.Binding(nameof(KeyMapping.SourceKey))
        });
        _mappingGrid.Columns.Add(new DataGridComboBoxColumn
        {
            Header = "Send key",
            ItemsSource = KeyChoices,
            SelectedItemBinding = new System.Windows.Data.Binding(nameof(KeyMapping.TargetKey))
        });
        _mappingGrid.Columns.Add(new DataGridCheckBoxColumn
        {
            Header = "Enabled",
            Binding = new System.Windows.Data.Binding(nameof(KeyMapping.Enabled))
        });
        panel.Children.Add(_mappingGrid);

        return panel;
    }

    private UIElement BuildFooter()
    {
        var footer = new DockPanel { Margin = new Thickness(0, 12, 0, 0) };

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

        var saveButton = new Button { Content = "Save", MinWidth = 86, Margin = new Thickness(8, 0, 0, 0) };
        saveButton.Click += (_, _) => SaveAndClose();
        actions.Children.Add(saveButton);

        var cancelButton = new Button { Content = "Cancel", MinWidth = 86, Margin = new Thickness(8, 0, 0, 0) };
        cancelButton.Click += (_, _) => Close();
        actions.Children.Add(cancelButton);

        footer.Children.Add(actions);
        return footer;
    }

    private static void AddLabel(Grid grid, string text, int row)
    {
        var label = new TextBlock
        {
            Text = text,
            VerticalAlignment = VerticalAlignment.Center,
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

        _mappings = new ObservableCollection<KeyMapping>(profile?.Mappings.Select(CloneMapping) ?? []);
        _mappingGrid.ItemsSource = _mappings;

        var canEditMappings = profile is not null && profile.Id != DefaultProfiles.NormalProfileId;
        _nameText.IsEnabled = profile is not null && profile.Id != DefaultProfiles.NormalProfileId;
        _executableText.IsEnabled = canEditMappings;
        _titleText.IsEnabled = canEditMappings;
        _enabledCheckBox.IsEnabled = canEditMappings;
        _mappingGrid.IsEnabled = canEditMappings;
        _applyEsdfButton.IsEnabled = canEditMappings;
        _deleteButton.IsEnabled = profile is not null && !profile.IsBuiltIn;

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
    }

    private void AddProfile()
    {
        WriteEditorToSelectedProfile();
        var profile = DefaultProfiles.CreateGameProfile();
        _workingDocument.Profiles.Add(profile);
        _profileList.Items.Refresh();
        _profileList.SelectedItem = profile;
    }

    private void DeleteProfile()
    {
        if (_selectedProfile is null || _selectedProfile.IsBuiltIn)
        {
            return;
        }

        _workingDocument.Profiles.Remove(_selectedProfile);
        _profileList.Items.Refresh();
        _profileList.SelectedItem = _workingDocument.Profiles.FirstOrDefault();
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

            StartupService.SetRunAtLogin(_workingDocument.Settings.StartWithWindows);
            _profileRuntime.Save(_workingDocument.DeepClone());
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Unable to save settings", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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
