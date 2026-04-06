using System.Diagnostics;
using System.Drawing;
using AltCommand.Windows.Configuration;
using AltCommand.Windows.Input;

namespace AltCommand.Windows;

internal sealed class SettingsForm : Form
{
    private readonly CheckBox _enabledCheckBox;
    private readonly CheckBox _launchAtStartupCheckBox;
    private readonly DataGridView _remapsGrid;
    private readonly TextBox _excludedProcessesTextBox;
    private readonly LinkLabel _configPathLinkLabel;

    public SettingsForm(AppConfig config, bool remappingEnabled, bool launchAtStartupEnabled, string configPath)
    {
        Text = "Alt Command Settings";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(820, 560);
        Size = new Size(940, 680);

        _enabledCheckBox = new CheckBox
        {
            AutoSize = true,
            Checked = remappingEnabled,
            Text = "Remapping enabled"
        };

        _launchAtStartupCheckBox = new CheckBox
        {
            AutoSize = true,
            Checked = launchAtStartupEnabled,
            Text = "Launch at startup"
        };

        var openJsonButton = new Button
        {
            AutoSize = true,
            Text = "Open JSON config"
        };
        openJsonButton.Click += (_, _) => OpenPath(configPath);

        _configPathLinkLabel = new LinkLabel
        {
            AutoSize = true,
            Text = configPath
        };
        _configPathLinkLabel.LinkClicked += (_, _) => OpenPath(configPath);

        var headerLayout = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 0, 0, 8),
            RowCount = 3
        };

        headerLayout.Controls.Add(
            new Label
            {
                AutoSize = true,
                MaximumSize = new Size(880, 0),
                Text = "Edit shortcuts here instead of touching JSON by hand. Use '|' inside the Action column to chain several shortcut steps."
            },
            0,
            0);

        var toggleFlow = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            WrapContents = true
        };
        toggleFlow.Controls.Add(_enabledCheckBox);
        toggleFlow.Controls.Add(_launchAtStartupCheckBox);
        toggleFlow.Controls.Add(openJsonButton);
        headerLayout.Controls.Add(toggleFlow, 0, 1);

        var pathFlow = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            WrapContents = true
        };
        pathFlow.Controls.Add(new Label
        {
            AutoSize = true,
            Text = "Config file:"
        });
        pathFlow.Controls.Add(_configPathLinkLabel);
        headerLayout.Controls.Add(pathFlow, 0, 2);

        _remapsGrid = CreateRemapsGrid();

        var addShortcutButton = new Button
        {
            AutoSize = true,
            Text = "Add shortcut"
        };
        addShortcutButton.Click += (_, _) => _remapsGrid.Rows.Add(string.Empty, string.Empty, string.Empty);

        var removeShortcutButton = new Button
        {
            AutoSize = true,
            Text = "Remove selected"
        };
        removeShortcutButton.Click += (_, _) => RemoveSelectedShortcut();

        var shortcutsTabLayout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            RowCount = 3
        };
        shortcutsTabLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        shortcutsTabLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shortcutsTabLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        shortcutsTabLayout.Controls.Add(
            new Label
            {
                AutoSize = true,
                MaximumSize = new Size(860, 0),
                Padding = new Padding(0, 0, 0, 8),
                Text = "Shortcuts: From is what you press on the keyboard. Action is the Windows shortcut to send. Examples: Ctrl+C, Alt+F4, Shift+Home | Backspace."
            },
            0,
            0);
        shortcutsTabLayout.Controls.Add(_remapsGrid, 0, 1);

        var shortcutsButtonFlow = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill
        };
        shortcutsButtonFlow.Controls.Add(addShortcutButton);
        shortcutsButtonFlow.Controls.Add(removeShortcutButton);
        shortcutsTabLayout.Controls.Add(shortcutsButtonFlow, 0, 2);

        _excludedProcessesTextBox = new TextBox
        {
            AcceptsReturn = true,
            AcceptsTab = false,
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical
        };

        var excludedAppsTabLayout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            RowCount = 2
        };
        excludedAppsTabLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        excludedAppsTabLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        excludedAppsTabLayout.Controls.Add(
            new Label
            {
                AutoSize = true,
                MaximumSize = new Size(860, 0),
                Padding = new Padding(0, 0, 0, 8),
                Text = "Excluded apps: one process per line. Examples: Code, WindowsTerminal, chrome.exe."
            },
            0,
            0);
        excludedAppsTabLayout.Controls.Add(_excludedProcessesTextBox, 0, 1);

        var tabs = new TabControl
        {
            Dock = DockStyle.Fill
        };

        var shortcutsTab = new TabPage("Shortcuts");
        shortcutsTab.Controls.Add(shortcutsTabLayout);

        var excludedAppsTab = new TabPage("Excluded Apps");
        excludedAppsTab.Controls.Add(excludedAppsTabLayout);

        tabs.TabPages.Add(shortcutsTab);
        tabs.TabPages.Add(excludedAppsTab);

        var restoreDefaultsButton = new Button
        {
            AutoSize = true,
            Text = "Restore defaults"
        };
        restoreDefaultsButton.Click += (_, _) => LoadConfigIntoControls(AppConfig.CreateDefault());

        var cancelButton = new Button
        {
            AutoSize = true,
            DialogResult = DialogResult.Cancel,
            Text = "Cancel"
        };

        var saveButton = new Button
        {
            AutoSize = true,
            Text = "Save"
        };
        saveButton.Click += SaveAndClose;

        AcceptButton = saveButton;
        CancelButton = cancelButton;

        var bottomButtons = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft
        };
        bottomButtons.Controls.Add(saveButton);
        bottomButtons.Controls.Add(cancelButton);
        bottomButtons.Controls.Add(restoreDefaultsButton);

        var root = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            RowCount = 3
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(headerLayout, 0, 0);
        root.Controls.Add(tabs, 0, 1);
        root.Controls.Add(bottomButtons, 0, 2);

        Controls.Add(root);

        LoadConfigIntoControls(config);
    }

    public AppConfig? SavedConfig { get; private set; }

    public bool RemappingEnabled => _enabledCheckBox.Checked;

    public bool LaunchAtStartupEnabled => _launchAtStartupCheckBox.Checked;

    private DataGridView CreateRemapsGrid()
    {
        var grid = new DataGridView
        {
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = SystemColors.Window,
            Dock = DockStyle.Fill,
            MultiSelect = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            FillWeight = 28,
            HeaderText = "From",
            Name = "From"
        });

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            FillWeight = 42,
            HeaderText = "Action",
            Name = "Action"
        });

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            FillWeight = 30,
            HeaderText = "Description",
            Name = "Description"
        });

        return grid;
    }

    private void LoadConfigIntoControls(AppConfig config)
    {
        _remapsGrid.Rows.Clear();

        foreach (var remap in config.Remaps ?? new List<RemapEntry>())
        {
            _remapsGrid.Rows.Add(
                remap.From,
                FormatTarget(remap),
                remap.Description ?? string.Empty);
        }

        _excludedProcessesTextBox.Text = string.Join(Environment.NewLine, config.DisabledProcesses ?? new List<string>());
    }

    private void RemoveSelectedShortcut()
    {
        if (_remapsGrid.CurrentRow is null)
        {
            return;
        }

        if (!_remapsGrid.CurrentRow.IsNewRow)
        {
            _remapsGrid.Rows.Remove(_remapsGrid.CurrentRow);
        }
    }

    private void SaveAndClose(object? sender, EventArgs eventArgs)
    {
        _remapsGrid.EndEdit();

        if (!TryBuildConfig(out var config, out var errorMessage))
        {
            MessageBox.Show(this, errorMessage, "Invalid Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SavedConfig = config;
        DialogResult = DialogResult.OK;
        Close();
    }

    private bool TryBuildConfig(out AppConfig config, out string errorMessage)
    {
        config = new AppConfig();
        errorMessage = string.Empty;

        var seenShortcuts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (DataGridViewRow row in _remapsGrid.Rows)
        {
            var from = GetCellText(row, "From");
            var action = GetCellText(row, "Action");
            var description = GetCellText(row, "Description");

            if (string.IsNullOrWhiteSpace(from) && string.IsNullOrWhiteSpace(action) && string.IsNullOrWhiteSpace(description))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(action))
            {
                errorMessage = "Each shortcut row needs both From and Action.";
                return false;
            }

            if (!HotkeyCombination.TryParse(from, out _))
            {
                errorMessage = $"Invalid source shortcut: {from}";
                return false;
            }

            if (!seenShortcuts.Add(from))
            {
                errorMessage = $"Duplicate source shortcut: {from}";
                return false;
            }

            var actionSteps = action
                .Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            if (actionSteps.Count == 0)
            {
                errorMessage = $"Action is empty for shortcut: {from}";
                return false;
            }

            foreach (var step in actionSteps)
            {
                if (!HotkeyCombination.TryParse(step, out _))
                {
                    errorMessage = $"Invalid action shortcut '{step}' for source '{from}'.";
                    return false;
                }
            }

            config.Remaps.Add(
                actionSteps.Count == 1
                    ? new RemapEntry(from, actionSteps[0], NormalizeDescription(description))
                    : new RemapEntry(from, actionSteps, NormalizeDescription(description)));
        }

        config.DisabledProcesses = _excludedProcessesTextBox
            .Lines
            .Select(value => value.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return true;
    }

    private static string GetCellText(DataGridViewRow row, string columnName)
    {
        return row.Cells[columnName].Value?.ToString()?.Trim() ?? string.Empty;
    }

    private static string FormatTarget(RemapEntry remap)
    {
        return remap.ToSequence is { Count: > 0 }
            ? string.Join(" | ", remap.ToSequence)
            : remap.To;
    }

    private static string? NormalizeDescription(string description)
    {
        return string.IsNullOrWhiteSpace(description) ? null : description;
    }

    private static void OpenPath(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }
}
