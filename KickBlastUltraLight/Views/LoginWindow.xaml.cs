using System.Windows;
using KickBlastUltraLight.Data;

namespace KickBlastUltraLight.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var ok = Db.ValidateLogin(UsernameTextBox.Text.Trim(), PasswordBox.Password);
            if (!ok)
            {
                ErrorText.Text = "Invalid username or password.";
                return;
            }

            var main = new MainWindow();
            Application.Current.MainWindow = main;
            main.Show();
            Close();
        }
        catch (Exception ex)
        {
            ErrorText.Text = "Login failed: " + ex.Message;
        }
    }
}
