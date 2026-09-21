using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ThreadsManagerUI;

public partial class ImportAccountWindow : Window
{
    public ObservableCollection<Models.AccountModel> PreviewAccounts { get; set; } = new ObservableCollection<Models.AccountModel>();

    public ImportAccountWindow()
    {
        InitializeComponent();
        DgPreview.ItemsSource = PreviewAccounts;
    }

    public string InputData
    {
        get => TxtInputData.Text;
        set => TxtInputData.Text = value;
    }

    private void TxtInputData_TextChanged(object sender, TextChangedEventArgs e)
    {
        PreviewAccounts.Clear();
        if (string.IsNullOrWhiteSpace(TxtInputData.Text)) return;

        var lines = TxtInputData.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var parts = line.Split('|');
            var acc = new Models.AccountModel();
            
            // Format: UID|Pass|Cookie|Token|Email|PassEmail|2FA|Proxy|UserAgent
            if (parts.Length > 0) acc.Uid = parts[0].Trim();
            if (parts.Length > 1) acc.Password = parts[1].Trim();
            if (parts.Length > 2) acc.Cookie = parts[2].Trim();
            if (parts.Length > 3) acc.Token = parts[3].Trim();
            if (parts.Length > 4) acc.Email = parts[4].Trim();
            if (parts.Length > 5) acc.PassEmail = parts[5].Trim();
            if (parts.Length > 6) acc.TwoFA = parts[6].Trim();
            if (parts.Length > 7) acc.Proxy = parts[7].Trim();
            if (parts.Length > 8) acc.UserAgent = parts[8].Trim();

            PreviewAccounts.Add(acc);
        }
    }

    private void BtnImport_Click(object sender, RoutedEventArgs e)
    {
        var mainViewModel = (this.Owner as MainWindow)?.DataContext as ViewModels.MainViewModel;
        if (mainViewModel != null && PreviewAccounts.Count > 0)
        {
            foreach (var acc in PreviewAccounts)
            {
                acc.Index = mainViewModel.Accounts.Count + 1;
                acc.Status = "Live"; // Trạng thái mặc định
                mainViewModel.Accounts.Add(acc);
            }
            mainViewModel.TotalAccounts = mainViewModel.Accounts.Count;
        }
        this.Close();
    }
}
