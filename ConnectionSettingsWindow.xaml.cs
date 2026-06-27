using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using MySqlConnector;

namespace GymFinanceBillingWpf;

public partial class ConnectionSettingsWindow : Window
{
    public bool Saved { get; private set; } = false;

    public ConnectionSettingsWindow(string server = "localhost", string port = "3306",
        string database = "aerogym", string user = "root", string password = "", string errorMessage = "")
    {
        InitializeComponent();
        TxtServer.Text = server;
        TxtPort.Text = port;
        TxtDatabase.Text = database;
        TxtUser.Text = user;
        TxtPassword.Password = password;

        if (!string.IsNullOrEmpty(errorMessage))
        {
            ShowError(errorMessage);
        }
    }

    private string BuildConnectionString()
    {
        return $"Server={TxtServer.Text.Trim()};" +
               $"Port={TxtPort.Text.Trim()};" +
               $"Database={TxtDatabase.Text.Trim()};" +
               $"User={TxtUser.Text.Trim()};" +
               $"Password={TxtPassword.Password};";
    }

    private async void BtnTest_Click(object sender, RoutedEventArgs e)
    {
        BtnTest.IsEnabled = false;
        BtnTest.Content = "Testing...";
        BorderError.Visibility = Visibility.Collapsed;

        try
        {
            // Test using raw MySqlConnection so we don't need a DB to exist yet
            var testConnStr = $"Server={TxtServer.Text.Trim()};" +
                              $"Port={TxtPort.Text.Trim()};" +
                              $"User={TxtUser.Text.Trim()};" +
                              $"Password={TxtPassword.Password};" +
                              $"Connection Timeout=5;";

            await using var conn = new MySqlConnection(testConnStr);
            await conn.OpenAsync();

            ShowSuccess("✅ Connection successful! MySQL server is reachable.");
        }
        catch (Exception ex)
        {
            ShowError($"Connection failed: {ex.Message}");
        }
        finally
        {
            BtnTest.IsEnabled = true;
            BtnTest.Content = "Test Connection";
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtServer.Text) ||
            string.IsNullOrWhiteSpace(TxtPort.Text) ||
            string.IsNullOrWhiteSpace(TxtDatabase.Text) ||
            string.IsNullOrWhiteSpace(TxtUser.Text))
        {
            ShowError("Please fill in all fields (Server, Port, Database, Username).");
            return;
        }

        // Save to appsettings.json
        try
        {
            var settings = new
            {
                Database = new
                {
                    Server = TxtServer.Text.Trim(),
                    Port = TxtPort.Text.Trim(),
                    Name = TxtDatabase.Text.Trim(),
                    User = TxtUser.Text.Trim(),
                    Password = TxtPassword.Password
                }
            };

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });

            // Write to the output directory where the app reads it from
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            var settingsPath = Path.Combine(appDir, "appsettings.json");
            File.WriteAllText(settingsPath, json);

            Saved = true;
            DialogResult = true;
            this.Close();
        }
        catch (Exception ex)
        {
            ShowError($"Failed to save settings: {ex.Message}");
        }
    }

    private void ShowError(string message)
    {
        TxtError.Text = message;
        TxtError.Foreground = System.Windows.Media.Brushes.OrangeRed;
        BorderError.Background = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromArgb(30, 239, 68, 68));
        BorderError.Visibility = Visibility.Visible;
    }

    private void ShowSuccess(string message)
    {
        TxtError.Text = message;
        TxtError.Foreground = System.Windows.Media.Brushes.LightGreen;
        BorderError.Background = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromArgb(30, 16, 185, 129));
        BorderError.Visibility = Visibility.Visible;
    }

    private void CloseWindow(object sender, RoutedEventArgs e)
    {
        if (!Saved) Application.Current.Shutdown();
        else this.Close();
    }
}
