using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace ThreadsManagerUI;

public class AppSettings
{
    public string ProfilePath { get; set; } = AppDomain.CurrentDomain.BaseDirectory;
    public int ProxyTypeIndex { get; set; } = 0;
}

public partial class SettingsWindow : Window
{
    private string _settingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

    public SettingsWindow()
    {
        InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsFile))
            {
                var json = File.ReadAllText(_settingsFile);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings != null)
                {
                    TxtProfilePath.Text = settings.ProfilePath;
                    CboProxyType.SelectedIndex = settings.ProxyTypeIndex;
                    return;
                }
            }
        }
        catch { }
        
        TxtProfilePath.Text = AppDomain.CurrentDomain.BaseDirectory;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        var settings = new AppSettings
        {
            ProfilePath = TxtProfilePath.Text,
            ProxyTypeIndex = CboProxyType.SelectedIndex
        };

        try
        {
            var json = JsonSerializer.Serialize(settings);
            File.WriteAllText(_settingsFile, json);

            // Cập nhật config.py
            string configPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\config\config.py"));
            if (File.Exists(configPath))
            {
                var lines = new System.Collections.Generic.List<string>(File.ReadAllLines(configPath));
                bool foundProfile = false;
                bool foundProxyType = false;

                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].StartsWith("PROFILE_DIR"))
                    {
                        var safePath = settings.ProfilePath.Replace("\\", "/");
                        lines[i] = $"PROFILE_DIR = \"{safePath}\"";
                        foundProfile = true;
                    }
                    else if (lines[i].StartsWith("PROXY_TYPE"))
                    {
                        lines[i] = $"PROXY_TYPE = {settings.ProxyTypeIndex}";
                        foundProxyType = true;
                    }
                }

                if (!foundProfile) lines.Add($"PROFILE_DIR = \"{settings.ProfilePath.Replace("\\", "/")}\"");
                if (!foundProxyType) lines.Add($"PROXY_TYPE = {settings.ProxyTypeIndex}");

                File.WriteAllLines(configPath, lines);
            }
            
            MessageBox.Show("Lưu cài đặt thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi khi lưu cài đặt: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
