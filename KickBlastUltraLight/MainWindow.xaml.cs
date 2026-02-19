using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using KickBlastUltraLight.Views;

namespace KickBlastUltraLight;

public partial class MainWindow : Window
{
    private readonly DashboardView _dashboardView;
    private readonly AthletesView _athletesView;
    private readonly CalculatorView _calculatorView;
    private readonly HistoryView _historyView;
    private readonly SettingsView _settingsView;
    private UserControl? _currentView;

    public MainWindow()
    {
        InitializeComponent();

        _dashboardView = new DashboardView();
        _athletesView = new AthletesView();
        _calculatorView = new CalculatorView();
        _historyView = new HistoryView();
        _settingsView = new SettingsView();

        _athletesView.RequestOpenDrawer += OpenDrawer;
        _historyView.RequestOpenDrawer += OpenDrawer;

        ShowDashboard();
    }

    public void ShowToast(string message)
    {
        ToastText.Text = message;
        ToastBorder.Visibility = Visibility.Visible;

        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            ToastBorder.Visibility = Visibility.Collapsed;
        };
        timer.Start();
    }

    public void OpenDrawer(string title, UIElement content)
    {
        DrawerTitleText.Text = title;
        DrawerContentHost.Content = content;
        DrawerPanel.Visibility = Visibility.Visible;

        var anim = new DoubleAnimation(380, 0, TimeSpan.FromMilliseconds(200));
        DrawerTransform.BeginAnimation(TranslateTransform.XProperty, anim);
    }

    public void CloseDrawer()
    {
        var anim = new DoubleAnimation(0, 380, TimeSpan.FromMilliseconds(200));
        anim.Completed += (_, _) =>
        {
            DrawerPanel.Visibility = Visibility.Collapsed;
            DrawerContentHost.Content = null;
        };
        DrawerTransform.BeginAnimation(TranslateTransform.XProperty, anim);
    }

    private void ActivateButton(Button active)
    {
        var buttons = new[] { DashboardButton, AthletesButton, CalculatorButton, HistoryButton, SettingsButton };
        foreach (var button in buttons)
        {
            button.BorderBrush = Brushes.Transparent;
            button.BorderThickness = new Thickness(0);
        }

        active.BorderBrush = (Brush)FindResource("AccentBrush");
        active.BorderThickness = new Thickness(4, 0, 0, 0);
    }

    private void SetPage(string title, UserControl view)
    {
        PageTitleText.Text = title;
        MainContentHost.Content = view;
        _currentView = view;
        CloseDrawer();
    }

    private void ShowDashboard()
    {
        _dashboardView.Reload();
        SetPage("Dashboard", _dashboardView);
        ActivateButton(DashboardButton);
    }

    private void ShowAthletes()
    {
        _athletesView.Reload();
        SetPage("Athletes", _athletesView);
        ActivateButton(AthletesButton);
    }

    private void ShowCalculator()
    {
        _calculatorView.Reload();
        SetPage("Calculator", _calculatorView);
        ActivateButton(CalculatorButton);
    }

    private void ShowHistory()
    {
        _historyView.Reload();
        SetPage("History", _historyView);
        ActivateButton(HistoryButton);
    }

    private void ShowSettings()
    {
        _settingsView.Reload();
        SetPage("Settings", _settingsView);
        ActivateButton(SettingsButton);
    }

    private void DashboardButton_Click(object sender, RoutedEventArgs e) => ShowDashboard();
    private void AthletesButton_Click(object sender, RoutedEventArgs e) => ShowAthletes();
    private void CalculatorButton_Click(object sender, RoutedEventArgs e) => ShowCalculator();
    private void HistoryButton_Click(object sender, RoutedEventArgs e) => ShowHistory();
    private void SettingsButton_Click(object sender, RoutedEventArgs e) => ShowSettings();

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        var login = new LoginWindow();
        Application.Current.MainWindow = login;
        login.Show();
        Close();
    }

    private void GlobalSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_currentView == _athletesView)
        {
            _athletesView.ApplyQuickSearch(GlobalSearchTextBox.Text.Trim());
        }
    }

    private void FabButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentView == _athletesView)
        {
            _athletesView.OpenAddAthleteDrawer();
        }
        else if (_currentView == _calculatorView)
        {
            _calculatorView.RunCalculation();
        }
        else if (_currentView == _settingsView)
        {
            _settingsView.Save();
        }
    }

    private void CloseDrawerButton_Click(object sender, RoutedEventArgs e)
    {
        CloseDrawer();
    }
}
