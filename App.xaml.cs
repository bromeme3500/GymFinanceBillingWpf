using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using GymFinanceBillingWpf.Models;
using GymFinanceBillingWpf.Services;

namespace GymFinanceBillingWpf;

public partial class App : Application
{
    public static IGymService GymService { get; private set; } = null!;

    /// <summary>The user who is currently logged in. Set after successful authentication.</summary>
    public static User? CurrentUser { get; set; }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Prevent WPF from shutting down when the connection settings dialog closes
        Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        // Read appsettings.json from app output directory
        string connectionString;
        string server, port, name, user, password;

        try
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            server   = config["Database:Server"]   ?? "localhost";
            port     = config["Database:Port"]     ?? "3306";
            name     = config["Database:Name"]     ?? "aerogym";
            user     = config["Database:User"]     ?? "root";
            password = config["Database:Password"] ?? "";

            // Read and apply theme
            string themeKey = config["Database:Theme"] ?? "Indigo";
            ApplyThemeColor(themeKey);

            connectionString = $"Server={server};Port={port};Database={name};User={user};Password={password};";
        }
        catch (Exception)
        {
            // appsettings.json missing — open settings dialog with defaults
            server = "localhost"; port = "3306"; name = "aerogym"; user = "root"; password = "";
            ApplyThemeColor("Indigo");
            connectionString = $"Server={server};Port={port};Database={name};User={user};Password={password};";
        }

        // Try to initialise DB; if it fails, show the Connection Settings dialog
        bool connected = false;
        while (!connected)
        {
            try
            {
                var context = new GymDbContext(connectionString);
                GymService = new GymService(context);
                await GymService.InitializeDatabaseAsync();
                connected = true;
            }
            catch (Exception ex)
            {
                // Find root exception
                var rootEx = ex;
                while (rootEx.InnerException != null) rootEx = rootEx.InnerException;
                string errorMsg = rootEx.Message;

                // Show the Connection Settings window so the user can fix the config
                var settingsWin = new ConnectionSettingsWindow(server, port, name, user, password, errorMsg);
                settingsWin.ShowDialog();

                if (!settingsWin.Saved)
                {
                    // User closed without saving — quit
                    Shutdown();
                    return;
                }

                // Re-read settings after user saved them
                try
                {
                    var config = new ConfigurationBuilder()
                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                        .Build();

                    server   = config["Database:Server"]   ?? "localhost";
                    port     = config["Database:Port"]     ?? "3306";
                    name     = config["Database:Name"]     ?? "aerogym";
                    user     = config["Database:User"]     ?? "root";
                    password = config["Database:Password"] ?? "";

                    connectionString = $"Server={server};Port={port};Database={name};User={user};Password={password};";
                }
                catch { /* keep existing values */ }
            }
        }

        // Restore default shutdown mode so the app closes when windows close
        Current.ShutdownMode = ShutdownMode.OnLastWindowClose;

        // Show Login Window — MainWindow is opened from inside LoginWindow
        var loginWindow = new LoginWindow();
        loginWindow.Show();
    }

    public static void ApplyThemeColor(string themeKey)
    {
        ApplyTheme(themeKey);
    }

    public static void ApplyTheme(string themeName)
    {
        // Default Midnight Dark values
        string bgMain = "#0B0F19";
        string bgSidebar = "#0E1422";
        string bgCard = "#182035";
        string bgCardHover = "#202A46";
        string borderMain = "#1D2A46";
        string borderGlow = "#6366F1";
        string primary = "#6366F1";
        string primaryLight = "#818CF8";
        string textPrimary = "#F3F4F6";
        string textSecondary = "#9CA3AF";
        string textMuted = "#6B7280";

        switch (themeName?.ToLower())
        {
            case "light":
                bgMain = "#F3F4F6";
                bgSidebar = "#FFFFFF";
                bgCard = "#FFFFFF";
                bgCardHover = "#F9FAFB";
                borderMain = "#E5E7EB";
                borderGlow = "#0D9488";
                primary = "#0D9488";
                primaryLight = "#14B8A6";
                textPrimary = "#111827";
                textSecondary = "#4B5563";
                textMuted = "#9CA3AF";
                break;

            case "cyberpunk":
                bgMain = "#05000A";
                bgSidebar = "#0F001A";
                bgCard = "#1A0033";
                bgCardHover = "#26004C";
                borderMain = "#3D007A";
                borderGlow = "#E11D48";
                primary = "#E11D48";
                primaryLight = "#F43F5E";
                textPrimary = "#00FFFF";
                textSecondary = "#FF00FF";
                textMuted = "#800080";
                break;

            case "forest":
                bgMain = "#0C120C";
                bgSidebar = "#121B12";
                bgCard = "#1C281C";
                bgCardHover = "#273827";
                borderMain = "#2D3F2D";
                borderGlow = "#059669";
                primary = "#059669";
                primaryLight = "#10B981";
                textPrimary = "#ECFDF5";
                textSecondary = "#A7F3D0";
                textMuted = "#6EE7B7";
                break;

            case "teal":
                primary = "#0D9488";
                primaryLight = "#2DD4BF";
                borderGlow = "#0D9488";
                break;

            case "emerald":
                primary = "#059669";
                primaryLight = "#34D399";
                borderGlow = "#059669";
                break;

            case "amber":
                primary = "#D97706";
                primaryLight = "#FBBF24";
                borderGlow = "#D97706";
                break;

            case "rose":
                primary = "#E11D48";
                primaryLight = "#FB7185";
                borderGlow = "#E11D48";
                break;

            default: // Indigo / Midnight
                // Defaults are already set above
                break;
        }

        UpdateBrushColor("BgMain", bgMain);
        UpdateBrushColor("BgSidebar", bgSidebar);
        UpdateBrushColor("BgCard", bgCard);
        UpdateBrushColor("BgCardHover", bgCardHover);
        UpdateBrushColor("BorderMain", borderMain);
        UpdateBrushColor("BorderGlow", borderGlow);
        UpdateBrushColor("Primary", primary);
        UpdateBrushColor("PrimaryLight", primaryLight);
        UpdateBrushColor("TextPrimary", textPrimary);
        UpdateBrushColor("TextSecondary", textSecondary);
        UpdateBrushColor("TextMuted", textMuted);
    }

    private static void UpdateBrushColor(string resourceKey, string hexColor)
    {
        if (Application.Current.Resources[resourceKey] is System.Windows.Media.SolidColorBrush brush)
        {
            var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hexColor);
            if (brush.IsFrozen)
            {
                Application.Current.Resources[resourceKey] = new System.Windows.Media.SolidColorBrush(color);
            }
            else
            {
                brush.Color = color;
            }
        }
        else
        {
            var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hexColor);
            Application.Current.Resources[resourceKey] = new System.Windows.Media.SolidColorBrush(color);
        }
    }
}
