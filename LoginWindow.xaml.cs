using System.Windows;
using System.Windows.Input;

namespace GymFinanceBillingWpf;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        TxtUsername.Focus();
    }

    private async void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        await AttemptLoginAsync();
    }

    private async void InputKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await AttemptLoginAsync();
    }

    private async Task AttemptLoginAsync()
    {
        var username = TxtUsername.Text.Trim();
        var password = TxtPassword.Password;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowError("Please enter both username and password.");
            return;
        }

        // Disable button while authenticating
        BtnLogin.IsEnabled = false;
        BtnLogin.Content = "Signing in...";
        BorderError.Visibility = Visibility.Collapsed;

        try
        {
            var user = await App.GymService.AuthenticateAsync(username, password);

            if (user == null)
            {
                ShowError("Invalid username or password. Please try again.");
                BtnLogin.IsEnabled = true;
                BtnLogin.Content = "Sign In to AeroGym";
                TxtPassword.Clear();
                TxtPassword.Focus();
                return;
            }

            // ✅ Auth successful — store current user
            App.CurrentUser = user;

            // Open the main window
            var mainWindow = new MainWindow();
            mainWindow.Show();

            // Close the login window
            this.Close();
        }
        catch (Exception ex)
        {
            ShowError($"An error occurred: {ex.Message}");
            BtnLogin.IsEnabled = true;
            BtnLogin.Content = "Sign In to AeroGym";
        }
    }

    private void ShowError(string message)
    {
        TxtError.Text = message;
        BorderError.Visibility = Visibility.Visible;
    }

    private void DragWindow(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }

    private void CloseApp(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}
