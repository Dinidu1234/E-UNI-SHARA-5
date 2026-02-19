using System.Windows;
using System.Windows.Controls;
using KickBlastUltraLight.Data;
using KickBlastUltraLight.Models;

namespace KickBlastUltraLight.Views;

public partial class AthletesView : UserControl
{
    public event Action<string, UIElement>? RequestOpenDrawer;
    private Athlete? _editing;

    public AthletesView()
    {
        InitializeComponent();
    }

    public void Reload()
    {
        try
        {
            var keyword = LocalSearchTextBox.Text.Trim();
            var plan = (PlanFilterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All";
            AthletesDataGrid.ItemsSource = Db.GetAthletes(keyword, plan);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to load athletes: " + ex.Message);
        }
    }

    public void ApplyQuickSearch(string search)
    {
        LocalSearchTextBox.Text = search;
    }

    public void OpenAddAthleteDrawer()
    {
        _editing = new Athlete();
        RequestOpenDrawer?.Invoke("Add Athlete", BuildAthleteForm());
    }

    private UIElement BuildAthleteForm()
    {
        var panel = new StackPanel();

        var nameBox = new TextBox { Text = _editing?.FullName ?? string.Empty };
        var ageBox = new TextBox { Text = (_editing?.Age ?? 0).ToString() };
        var planBox = new ComboBox();
        planBox.Items.Add("Beginner");
        planBox.Items.Add("Intermediate");
        planBox.Items.Add("Advanced");
        planBox.SelectedItem = _editing?.Plan ?? "Beginner";
        var weightBox = new TextBox { Text = (_editing?.WeightKg ?? 0).ToString() };
        var targetBox = new TextBox { Text = (_editing?.TargetWeightKg ?? 0).ToString() };
        var notesBox = new TextBox { Text = _editing?.Notes ?? string.Empty, Height = 80, TextWrapping = TextWrapping.Wrap, AcceptsReturn = true };

        panel.Children.Add(new TextBlock { Text = "Full Name" });
        panel.Children.Add(nameBox);
        panel.Children.Add(new TextBlock { Text = "Age" });
        panel.Children.Add(ageBox);
        panel.Children.Add(new TextBlock { Text = "Plan" });
        panel.Children.Add(planBox);
        panel.Children.Add(new TextBlock { Text = "Weight Kg" });
        panel.Children.Add(weightBox);
        panel.Children.Add(new TextBlock { Text = "Target Weight Kg" });
        panel.Children.Add(targetBox);
        panel.Children.Add(new TextBlock { Text = "Notes" });
        panel.Children.Add(notesBox);

        var saveButton = new Button { Content = "Save Athlete", Style = (Style)FindResource("PrimaryButtonStyle"), Margin = new Thickness(0, 12, 0, 0) };
        saveButton.Click += (_, _) =>
        {
            try
            {
                _editing ??= new Athlete();
                _editing.FullName = nameBox.Text.Trim();
                _editing.Age = int.TryParse(ageBox.Text, out var ageValue) ? ageValue : 0;
                _editing.Plan = planBox.SelectedItem?.ToString() ?? "Beginner";
                _editing.WeightKg = decimal.TryParse(weightBox.Text, out var weight) ? weight : 0;
                _editing.TargetWeightKg = decimal.TryParse(targetBox.Text, out var target) ? target : 0;
                _editing.Notes = notesBox.Text.Trim();
                Db.UpsertAthlete(_editing);
                Reload();
                ((MainWindow)Application.Current.MainWindow).ShowToast("Athlete saved");
                ((MainWindow)Application.Current.MainWindow).CloseDrawer();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save failed: " + ex.Message);
            }
        };

        panel.Children.Add(saveButton);
        return new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    }

    private void FilterChanged(object sender, RoutedEventArgs e)
    {
        Reload();
    }

    private void AddAthleteButton_Click(object sender, RoutedEventArgs e)
    {
        OpenAddAthleteDrawer();
    }

    private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (AthletesDataGrid.SelectedItem is not Athlete athlete) return;
            Db.DeleteAthlete(athlete.Id);
            Reload();
            ((MainWindow)Application.Current.MainWindow).ShowToast("Athlete deleted");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Delete failed: " + ex.Message);
        }
    }

    private void AthletesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AthletesDataGrid.SelectedItem is not Athlete athlete) return;
        _editing = athlete;
        RequestOpenDrawer?.Invoke("Edit Athlete", BuildAthleteForm());
    }
}
