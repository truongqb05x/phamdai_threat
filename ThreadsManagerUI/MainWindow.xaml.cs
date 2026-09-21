using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ThreadsManagerUI.ViewModels;

namespace ThreadsManagerUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void BtnImportAccount_Click(object sender, RoutedEventArgs e)
    {
        var importWindow = new ImportAccountWindow
        {
            Owner = this
        };
        importWindow.ShowDialog();
    }

    private void BtnImportFromFile_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                Title = "Chọn file chứa danh sách tài khoản"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var fileContent = System.IO.File.ReadAllText(openFileDialog.FileName);
                var importWindow = new ImportAccountWindow
                {
                    Owner = this,
                    InputData = fileContent
                };
                importWindow.ShowDialog();
            }
        }
        catch (System.Exception ex)
        {
            System.Windows.MessageBox.Show("Lỗi khi mở file: " + ex.Message, "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private void BtnSettings_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow
        {
            Owner = this
        };
        settingsWindow.ShowDialog();
    }

    private void DgAccounts_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (TxtSelectedCount != null && DgAccounts != null)
        {
            TxtSelectedCount.Text = $"Đã chọn: {DgAccounts.SelectedItems.Count}";
        }
    }

    private void CboStatusFilter_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void TxtSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void BtnSearch_Click(object sender, RoutedEventArgs e)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var mainViewModel = DataContext as MainViewModel;
        if (mainViewModel == null || DgAccounts == null) return;
        
        var selectedItem = CboStatusFilter?.SelectedItem as System.Windows.Controls.ComboBoxItem;
        string statusFilter = selectedItem?.Content.ToString() ?? "All Status";
        string searchText = TxtSearch?.Text.Trim().ToLower() ?? "";

        var view = System.Windows.Data.CollectionViewSource.GetDefaultView(mainViewModel.Accounts);
        view.Filter = item =>
        {
            var acc = item as Models.AccountModel;
            if (acc == null) return false;

            bool matchesStatus = (statusFilter == "All Status") || (acc.Status == statusFilter);
            bool matchesSearch = string.IsNullOrEmpty(searchText) || (!string.IsNullOrEmpty(acc.Uid) && acc.Uid.ToLower().Contains(searchText));

            return matchesStatus && matchesSearch;
        };
    }

    private void MenuItem_Delete_Click(object sender, RoutedEventArgs e)
    {
        var mainViewModel = DataContext as MainViewModel;
        if (mainViewModel == null) return;

        var selectedItems = DgAccounts.SelectedItems.Cast<Models.AccountModel>().ToList();
        if (selectedItems.Count == 0) return;

        var result = System.Windows.MessageBox.Show($"Bạn có chắc chắn muốn xóa {selectedItems.Count} tài khoản đã chọn?", "Xác nhận xóa", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
        if (result != System.Windows.MessageBoxResult.Yes) return;

        foreach (var item in selectedItems)
        {
            mainViewModel.Accounts.Remove(item);
        }

        // Cập nhật lại cột STT (Index)
        for (int i = 0; i < mainViewModel.Accounts.Count; i++)
        {
            mainViewModel.Accounts[i].Index = i + 1;
        }
    }

    private void MenuItem_OpenChrome_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.MessageBox.Show("Chức năng Mở Chrome đang được phát triển!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }

    private void MenuItem_Copy_Click(object sender, RoutedEventArgs e)
    {
        var selectedItems = DgAccounts.SelectedItems.Cast<Models.AccountModel>().ToList();
        if (selectedItems.Count == 0) return;

        var sb = new System.Text.StringBuilder();
        foreach (var acc in selectedItems)
        {
            var parts = new System.Collections.Generic.List<string>
            {
                acc.Uid ?? "",
                acc.Password ?? "",
                acc.Cookie ?? "",
                acc.Token ?? "",
                acc.Email ?? "",
                acc.PassEmail ?? "",
                acc.TwoFA ?? "",
                acc.Proxy ?? "",
                acc.UserAgent ?? ""
            };

            // Loại bỏ các trường rỗng ở cuối để chuỗi copy gọn gàng
            while (parts.Count > 0 && string.IsNullOrEmpty(parts.Last()))
            {
                parts.RemoveAt(parts.Count - 1);
            }

            sb.AppendLine(string.Join("|", parts));
        }

        System.Windows.Clipboard.SetText(sb.ToString().TrimEnd());
        System.Windows.MessageBox.Show("Đã copy dữ liệu vào Clipboard!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }
}