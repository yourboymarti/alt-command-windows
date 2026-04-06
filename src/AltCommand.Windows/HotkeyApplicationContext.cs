using System.Diagnostics;
using System.Drawing;
using AltCommand.Windows.Configuration;
using AltCommand.Windows.Input;

namespace AltCommand.Windows;

internal sealed class HotkeyApplicationContext : ApplicationContext
{
    private readonly ConfigStore _configStore = new();
    private readonly KeyStrokeSender _keyStrokeSender = new();
    private readonly HotkeyEngine _hotkeyEngine;
    private readonly KeyboardHookService _keyboardHookService;
    private NotifyIcon? _notifyIcon;
    private readonly ToolStripMenuItem _enabledMenuItem;
    private readonly ToolStripMenuItem _launchAtStartupMenuItem;

    private AppConfig _currentConfig = AppConfig.CreateDefault();
    private bool _isEnabled = true;

    public HotkeyApplicationContext()
    {
        _hotkeyEngine = new HotkeyEngine(_keyStrokeSender);
        _hotkeyEngine.UpdateConfiguration(AppConfig.CreateDefault());
        LoadConfiguration(showSuccessBalloon: false);

        _keyboardHookService = new KeyboardHookService(HandleKeyboardEvent);
        _keyboardHookService.Start();

        _enabledMenuItem = new ToolStripMenuItem("Enabled", null, ToggleEnabled)
        {
            Checked = _isEnabled,
            CheckOnClick = true
        };

        _launchAtStartupMenuItem = new ToolStripMenuItem("Launch at startup", null, ToggleLaunchAtStartup)
        {
            Checked = StartupManager.IsEnabled()
        };

        var menu = new ContextMenuStrip();
        menu.Items.Add(new ToolStripMenuItem("Settings", null, (_, _) => OpenSettings()));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_enabledMenuItem);
        menu.Items.Add(_launchAtStartupMenuItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem("Open config file", null, (_, _) => OpenConfigFile()));
        menu.Items.Add(new ToolStripMenuItem("Open config folder", null, (_, _) => OpenConfigFolder()));
        menu.Items.Add(new ToolStripMenuItem("Reload config", null, (_, _) => LoadConfiguration(showSuccessBalloon: true)));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem("Exit", null, (_, _) => ExitThread()));

        _notifyIcon = new NotifyIcon
        {
            Text = "Alt Command",
            Icon = SystemIcons.Application,
            Visible = true,
            ContextMenuStrip = menu
        };

        _notifyIcon.DoubleClick += (_, _) => OpenSettings();
    }

    private bool HandleKeyboardEvent(KeyboardEvent keyboardEvent)
    {
        if (!_isEnabled)
        {
            return false;
        }

        return _hotkeyEngine.Handle(keyboardEvent);
    }

    private void ToggleEnabled(object? sender, EventArgs eventArgs)
    {
        _isEnabled = _enabledMenuItem.Checked;
        ShowBalloon("Alt Command", _isEnabled ? "Shortcut remapping is enabled." : "Shortcut remapping is paused.", ToolTipIcon.Info);
    }

    private void ToggleLaunchAtStartup(object? sender, EventArgs eventArgs)
    {
        try
        {
            StartupManager.SetEnabled(_launchAtStartupMenuItem.Checked);
            ShowBalloon(
                "Alt Command",
                _launchAtStartupMenuItem.Checked ? "Launch at startup enabled." : "Launch at startup disabled.",
                ToolTipIcon.Info);
        }
        catch (Exception exception)
        {
            _launchAtStartupMenuItem.Checked = StartupManager.IsEnabled();
            ShowBalloon("Alt Command", $"Startup toggle failed: {exception.Message}", ToolTipIcon.Error);
        }
    }

    private void LoadConfiguration(bool showSuccessBalloon)
    {
        try
        {
            var config = _configStore.Load();
            _hotkeyEngine.UpdateConfiguration(config);
            _currentConfig = config.Clone();

            if (showSuccessBalloon)
            {
                ShowBalloon("Alt Command", $"Config reloaded from {_configStore.ConfigPath}", ToolTipIcon.Info);
            }
        }
        catch (Exception exception)
        {
            ShowBalloon("Alt Command", $"Config error: {exception.Message}", ToolTipIcon.Error);
        }
    }

    private void OpenSettings()
    {
        try
        {
            using var settingsForm = new SettingsForm(
                _currentConfig.Clone(),
                _isEnabled,
                StartupManager.IsEnabled(),
                _configStore.ConfigPath);

            if (settingsForm.ShowDialog() != DialogResult.OK || settingsForm.SavedConfig is null)
            {
                return;
            }

            _configStore.Save(settingsForm.SavedConfig);
            _hotkeyEngine.UpdateConfiguration(settingsForm.SavedConfig);
            _currentConfig = settingsForm.SavedConfig.Clone();

            _isEnabled = settingsForm.RemappingEnabled;
            _enabledMenuItem.Checked = _isEnabled;

            StartupManager.SetEnabled(settingsForm.LaunchAtStartupEnabled);
            _launchAtStartupMenuItem.Checked = settingsForm.LaunchAtStartupEnabled;

            ShowBalloon("Alt Command", "Settings saved successfully.", ToolTipIcon.Info);
        }
        catch (Exception exception)
        {
            ShowBalloon("Alt Command", $"Unable to save settings: {exception.Message}", ToolTipIcon.Error);
        }
    }

    private void OpenConfigFile()
    {
        try
        {
            _ = _configStore.Load();

            Process.Start(new ProcessStartInfo
            {
                FileName = _configStore.ConfigPath,
                UseShellExecute = true
            });
        }
        catch (Exception exception)
        {
            ShowBalloon("Alt Command", $"Unable to open config: {exception.Message}", ToolTipIcon.Error);
        }
    }

    private void OpenConfigFolder()
    {
        try
        {
            _ = _configStore.Load();

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{_configStore.ConfigPath}\"",
                UseShellExecute = true
            });
        }
        catch (Exception exception)
        {
            ShowBalloon("Alt Command", $"Unable to open config folder: {exception.Message}", ToolTipIcon.Error);
        }
    }

    private void ShowBalloon(string title, string text, ToolTipIcon icon)
    {
        if (_notifyIcon is null)
        {
            MessageBox.Show(text, title, MessageBoxButtons.OK, icon == ToolTipIcon.Error ? MessageBoxIcon.Error : MessageBoxIcon.Information);
            return;
        }

        _notifyIcon.BalloonTipTitle = title;
        _notifyIcon.BalloonTipText = text;
        _notifyIcon.BalloonTipIcon = icon;
        _notifyIcon.ShowBalloonTip(3000);
    }

    protected override void ExitThreadCore()
    {
        _keyboardHookService.Dispose();
        _keyStrokeSender.Dispose();
        if (_notifyIcon is not null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }

        base.ExitThreadCore();
    }
}
