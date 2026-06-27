using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Configuration;

namespace GymFinanceBillingWpf.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        LoadCurrentThemeSelection();
    }

    private void LoadCurrentThemeSelection()
    {
        try
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            var settingsPath = Path.Combine(appDir, "appsettings.json");

            if (File.Exists(settingsPath))
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(appDir)
                    .AddJsonFile("appsettings.json", optional: true)
                    .Build();

                string theme = config["Database:Theme"] ?? "Indigo";
                TxtStatus.Text = $"Current active theme: {theme}";
            }
        }
        catch
        {
            TxtStatus.Text = "Select a color above to apply the theme immediately.";
        }
    }

    private void BtnTheme_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string themeKey)
        {
            // Apply the theme in real time
            App.ApplyThemeColor(themeKey);
            TxtStatus.Text = $"Theme updated to {themeKey}! Saving settings...";
            TxtStatus.Foreground = System.Windows.Media.Brushes.LightGreen;

            // Save the theme setting back to appsettings.json
            try
            {
                var appDir = AppDomain.CurrentDomain.BaseDirectory;
                var settingsPath = Path.Combine(appDir, "appsettings.json");

                string server = "localhost", port = "3306", dbName = "aerogym", user = "root", pwd = "";

                // Read existing database settings so we don't overwrite them
                if (File.Exists(settingsPath))
                {
                    try
                    {
                        var config = new ConfigurationBuilder()
                            .SetBasePath(appDir)
                            .AddJsonFile("appsettings.json", optional: true)
                            .Build();

                        server = config["Database:Server"] ?? "localhost";
                        port = config["Database:Port"] ?? "3306";
                        dbName = config["Database:Name"] ?? "aerogym";
                        user = config["Database:User"] ?? "root";
                        pwd = config["Database:Password"] ?? "";
                    }
                    catch { /* Fallback to defaults */ }
                }

                // Construct configuration object with the new theme
                var settings = new
                {
                    Database = new
                    {
                        Server = server,
                        Port = port,
                        Name = dbName,
                        User = user,
                        Password = pwd,
                        Theme = themeKey
                    }
                };

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(settingsPath, json);

                TxtStatus.Text = $"Theme changed to {themeKey} and saved successfully!";
            }
            catch (Exception ex)
            {
                TxtStatus.Text = $"Failed to save preference: {ex.Message}";
                TxtStatus.Foreground = System.Windows.Media.Brushes.OrangeRed;
            }
        }
    }
}
