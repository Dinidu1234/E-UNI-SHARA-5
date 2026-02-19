using System.Windows;
using KickBlastUltraLight.Data;
using KickBlastUltraLight.Views;

namespace KickBlastUltraLight;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Db.Init();

        var login = new LoginWindow();
        MainWindow = login;
        login.Show();
    }
}
