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
using System.Collections.ObjectModel;
using System.Linq;
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
        var selectedItem = DgAccounts.SelectedItem as Models.AccountModel;
        if (selectedItem == null || string.IsNullOrEmpty(selectedItem.Uid)) return;

        try
        {
            LoadingOverlay.Visibility = System.Windows.Visibility.Visible;
            TxtLoadingStatus.Text = $"Đang mở Chrome cho {selectedItem.Uid}...";

            string pythonScriptDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.."));
            
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"main.py open_chrome \"{selectedItem.Uid}\"",
                WorkingDirectory = pythonScriptDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8
            };
            
            var process = new System.Diagnostics.Process();
            process.StartInfo = processInfo;
            process.EnableRaisingEvents = true;

            process.OutputDataReceived += (s, args) =>
            {
                if (args.Data != null)
                {
                    if (args.Data.Contains("[CHROME_READY]") || args.Data.Contains("[CHROME_ERROR]"))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            LoadingOverlay.Visibility = System.Windows.Visibility.Collapsed;
                            if (args.Data.Contains("[CHROME_ERROR]"))
                            {
                                System.Windows.MessageBox.Show("Có lỗi khi mở Chrome. Vui lòng kiểm tra lại cấu hình.", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                            }
                        });
                    }
                }
            };
            
            process.Exited += (s, args) =>
            {
                Dispatcher.Invoke(() =>
                {
                    LoadingOverlay.Visibility = System.Windows.Visibility.Collapsed;
                });
            };

            process.Start();
            process.BeginOutputReadLine();
        }
        catch (System.Exception ex)
        {
            LoadingOverlay.Visibility = System.Windows.Visibility.Collapsed;
            System.Windows.MessageBox.Show("Lỗi khi mở Chrome: " + ex.Message, "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private void BtnTabKetNoi_Click(object sender, RoutedEventArgs e)
    {
        GridKetNoiThreads.Visibility = Visibility.Visible;
        GridDangBai.Visibility = Visibility.Collapsed;
        GridConnectThreads.Visibility = Visibility.Collapsed;
        
        BtnTabKetNoi.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1877F2"));
        BtnTabKetNoi.Foreground = Brushes.White;
        BtnTabKetNoi.FontWeight = FontWeights.Bold;

        BtnTabDangBai.Background = Brushes.Transparent;
        BtnTabDangBai.Foreground = Brushes.Black;
        BtnTabDangBai.FontWeight = FontWeights.Normal;

        BtnTabConnectThreads.Background = Brushes.Transparent;
        BtnTabConnectThreads.Foreground = Brushes.Black;
        BtnTabConnectThreads.FontWeight = FontWeights.Normal;
    }

    private void BtnTabDangBai_Click(object sender, RoutedEventArgs e)
    {
        GridKetNoiThreads.Visibility = Visibility.Collapsed;
        GridDangBai.Visibility = Visibility.Visible;
        GridConnectThreads.Visibility = Visibility.Collapsed;

        BtnTabDangBai.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1877F2"));
        BtnTabDangBai.Foreground = Brushes.White;
        BtnTabDangBai.FontWeight = FontWeights.Bold;

        BtnTabKetNoi.Background = Brushes.Transparent;
        BtnTabKetNoi.Foreground = Brushes.Black;
        BtnTabKetNoi.FontWeight = FontWeights.Normal;

        BtnTabConnectThreads.Background = Brushes.Transparent;
        BtnTabConnectThreads.Foreground = Brushes.Black;
        BtnTabConnectThreads.FontWeight = FontWeights.Normal;
    }

    private void RbSourceExcel_Checked(object sender, RoutedEventArgs e)
    {
        if (PanelExcel != null && PanelGemini != null)
        {
            PanelExcel.Visibility = Visibility.Visible;
            PanelGemini.Visibility = Visibility.Collapsed;
        }
    }

    private void RbSourceGemini_Checked(object sender, RoutedEventArgs e)
    {
        if (PanelExcel != null && PanelGemini != null)
        {
            PanelExcel.Visibility = Visibility.Collapsed;
            PanelGemini.Visibility = Visibility.Visible;
        }
    }

    private void BtnSelectExcel_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Chọn file Excel",
            Filter = "Excel Files|*.xls;*.xlsx;*.xlsm;*.csv|All Files|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            TxtExcelPath.Text = dialog.FileName;
        }
    }

    private void BtnTabConnectThreads_Click(object sender, RoutedEventArgs e)
    {
        GridKetNoiThreads.Visibility = Visibility.Collapsed;
        GridDangBai.Visibility = Visibility.Collapsed;
        GridConnectThreads.Visibility = Visibility.Visible;

        BtnTabConnectThreads.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1877F2"));
        BtnTabConnectThreads.Foreground = Brushes.White;
        BtnTabConnectThreads.FontWeight = FontWeights.Bold;

        BtnTabKetNoi.Background = Brushes.Transparent;
        BtnTabKetNoi.Foreground = Brushes.Black;
        BtnTabKetNoi.FontWeight = FontWeights.Normal;

        BtnTabDangBai.Background = Brushes.Transparent;
        BtnTabDangBai.Foreground = Brushes.Black;
        BtnTabDangBai.FontWeight = FontWeights.Normal;
    }

    private void BtnSelectAccountsForPost_Click(object sender, RoutedEventArgs e)
    {
        var vm = this.DataContext as MainViewModel;
        if (vm == null) return;
        
        var selectWindow = new SelectAccountWindow(vm.Accounts);
        selectWindow.Owner = this;
        if (selectWindow.ShowDialog() == true)
        {
            var selected = selectWindow.SelectedAccounts;
            foreach (var acc in selected)
            {
                if (!vm.SelectedAccountsForPost.Any(a => a.Uid == acc.Uid))
                {
                    vm.SelectedAccountsForPost.Add(acc);
                }
            }
            
            // Cập nhật lại số thứ tự
            for (int i = 0; i < vm.SelectedAccountsForPost.Count; i++)
            {
                vm.SelectedAccountsForPost[i].Index = i + 1;
            }
        }
    }

    private void BtnRemovePostAccount_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            var item = button.DataContext as Models.AccountModel;
            if (item != null)
            {
                var vm = this.DataContext as MainViewModel;
                if (vm != null)
                {
                    vm.SelectedAccountsForPost.Remove(item);
                    for (int i = 0; i < vm.SelectedAccountsForPost.Count; i++)
                    {
                        vm.SelectedAccountsForPost[i].Index = i + 1;
                    }
                }
            }
        }
    }

    private void BtnSelectMedia_Click(object sender, RoutedEventArgs e)
    {
        // Simple folder browser dialog simulation using WinForms or OpenFileDialog
        var dialog = new Microsoft.Win32.OpenFileDialog();
        dialog.Title = "Chọn file hình ảnh/video";
        dialog.Multiselect = true;
        dialog.Filter = "Image/Video Files|*.jpg;*.jpeg;*.png;*.mp4|All files (*.*)|*.*";
        if (dialog.ShowDialog() == true)
        {
            TxtMediaPath.Text = string.Join("\n", dialog.FileNames);
        }
    }

    private System.Threading.CancellationTokenSource _postCts;

    private async void BtnStartPost_Click(object sender, RoutedEventArgs e)
    {
        var vm = this.DataContext as MainViewModel;
        if (vm == null || vm.SelectedAccountsForPost.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn ít nhất một tài khoản để đăng bài.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        int threadCount = 1;
        if (!int.TryParse(TxtThreadCount.Text, out threadCount) || threadCount < 1)
        {
            threadCount = 1;
        }

        int delayMin = 10;
        int delayMax = 20;
        int.TryParse(TxtDelayMin.Text, out delayMin);
        int.TryParse(TxtDelayMax.Text, out delayMax);

        string contentSource = RbSourceGemini.IsChecked == true ? "2" : "1";
        string apiPrompt = TxtGeminiPrompt.Text.Trim();
        string hashtagText = ChkUseHashtag.IsChecked == true ? TxtHashtags.Text.Trim() : "";
        string mediaPathsStr = "";
        if (ChkUseMedia.IsChecked == true)
        {
            mediaPathsStr = string.Join("|", TxtMediaPath.Text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries));
        }

        TxtLogs.Text = $"[{DateTime.Now.ToString("HH:mm:ss")}] [System]: Bắt đầu tiến trình đăng bài với {threadCount} luồng...\n";
        TxtLogs.ScrollToEnd();

        _postCts = new System.Threading.CancellationTokenSource();
        var token = _postCts.Token;

        var semaphore = new System.Threading.SemaphoreSlim(threadCount);
        var tasks = new System.Collections.Generic.List<System.Threading.Tasks.Task>();
        var random = new Random();

        foreach (var acc in vm.SelectedAccountsForPost)
        {
            acc.PostProcessStatus = "Chờ";
        }

        foreach (var acc in vm.SelectedAccountsForPost)
        {
            await semaphore.WaitAsync();

            if (token.IsCancellationRequested)
            {
                semaphore.Release();
                break;
            }

            tasks.Add(System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() => { acc.PostProcessStatus = "Đang chạy"; });
                    
                    string safeApiPrompt = "\"" + apiPrompt.Replace("\"", "\\\"") + "\"";
                    string safeHashtagText = "\"" + hashtagText.Replace("\"", "\\\"") + "\"";
                    string safeMediaPathsStr = "\"" + mediaPathsStr.Replace("\"", "\\\"") + "\"";

                    var startInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "python",
                        Arguments = $"main.py post_thread {acc.Uid} \"{contentSource}\" {safeApiPrompt} {safeHashtagText} {safeMediaPathsStr}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = System.Text.Encoding.UTF8,
                        StandardErrorEncoding = System.Text.Encoding.UTF8,
                        WorkingDirectory = @"d:\starup\pham_dai"
                    };

                    using (var process = new System.Diagnostics.Process { StartInfo = startInfo })
                    {
                        process.OutputDataReceived += (s, ev) =>
                        {
                            if (!string.IsNullOrEmpty(ev.Data))
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    TxtLogs.AppendText($"[{DateTime.Now.ToString("HH:mm:ss")}] [{acc.Uid}]: {ev.Data}\n");
                                    TxtLogs.ScrollToEnd();
                                });
                            }
                        };

                        process.Start();
                        process.BeginOutputReadLine();

                        while (!process.HasExited)
                        {
                            if (token.IsCancellationRequested)
                            {
                                try { process.Kill(); } catch { }
                                break;
                            }
                            await System.Threading.Tasks.Task.Delay(500);
                        }
                    }

                    if (token.IsCancellationRequested)
                    {
                        Application.Current.Dispatcher.Invoke(() => { acc.PostProcessStatus = "Đã dừng"; });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() => { acc.PostProcessStatus = "Xong"; });
                    }
                    
                    if (!token.IsCancellationRequested)
                    {
                        int delaySec = random.Next(delayMin, delayMax + 1);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            TxtLogs.AppendText($"[{DateTime.Now.ToString("HH:mm:ss")}] [System]: Đợi {delaySec}s...\n");
                            TxtLogs.ScrollToEnd();
                        });
                        await System.Threading.Tasks.Task.Delay(delaySec * 1000, token);
                    }
                }
                catch (System.OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        TxtLogs.AppendText($"[{DateTime.Now.ToString("HH:mm:ss")}] [{acc.Uid}] Lỗi: {ex.Message}\n");
                        TxtLogs.ScrollToEnd();
                    });
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }

        await System.Threading.Tasks.Task.WhenAll(tasks);

        if (token.IsCancellationRequested)
        {
            TxtLogs.AppendText($"[{DateTime.Now.ToString("HH:mm:ss")}] [System]: Tiến trình bị hủy bởi người dùng.\n");
        }
        else
        {
            TxtLogs.AppendText($"[{DateTime.Now.ToString("HH:mm:ss")}] [System]: Hoàn thành tất cả tác vụ đăng bài.\n");
        }
        TxtLogs.ScrollToEnd();
    }

    private void BtnStopPost_Click(object sender, RoutedEventArgs e)
    {
        if (_postCts != null && !_postCts.IsCancellationRequested)
        {
            _postCts.Cancel();
            TxtLogs.AppendText($"[{DateTime.Now.ToString("HH:mm:ss")}] [System]: Đang dừng tiến trình đăng bài...\n");
            TxtLogs.ScrollToEnd();
        }
    }

    private void BtnSelectAccountsForConnect_Click(object sender, RoutedEventArgs e)
    {
        var vm = this.DataContext as MainViewModel;
        if (vm == null) return;
        
        var selectWindow = new SelectAccountWindow(vm.Accounts);
        selectWindow.Owner = this;
        if (selectWindow.ShowDialog() == true)
        {
            var selected = selectWindow.SelectedAccounts;
            foreach (var acc in selected)
            {
                if (!vm.SelectedAccountsForConnect.Any(a => a.Uid == acc.Uid))
                {
                    vm.SelectedAccountsForConnect.Add(acc);
                }
            }
            
            for (int i = 0; i < vm.SelectedAccountsForConnect.Count; i++)
            {
                vm.SelectedAccountsForConnect[i].Index = i + 1;
            }
        }
    }

    private void BtnRemoveConnectAccount_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            var item = button.DataContext as Models.AccountModel;
            if (item != null)
            {
                var vm = this.DataContext as MainViewModel;
                if (vm != null)
                {
                    vm.SelectedAccountsForConnect.Remove(item);
                    for (int i = 0; i < vm.SelectedAccountsForConnect.Count; i++)
                    {
                        vm.SelectedAccountsForConnect[i].Index = i + 1;
                    }
                }
            }
        }
    }

    private System.Threading.CancellationTokenSource _connectCts;

    private async void BtnStartConnect_Click(object sender, RoutedEventArgs e)
    {
        var vm = this.DataContext as MainViewModel;
        if (vm == null || vm.SelectedAccountsForConnect.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn ít nhất một tài khoản để kết nối.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        int threadCount = 1;
        if (!int.TryParse(TxtConnectThreadCount.Text, out threadCount) || threadCount < 1)
        {
            threadCount = 1;
        }

        int delayMin = 10;
        int delayMax = 20;
        int.TryParse(TxtConnectDelayMin.Text, out delayMin);
        int.TryParse(TxtConnectDelayMax.Text, out delayMax);

        TxtLogsConnect.Text = $"[{DateTime.Now.ToString("HH:mm:ss")}] [System]: Bắt đầu tiến trình kết nối threads với {threadCount} luồng...\n";
        TxtLogsConnect.ScrollToEnd();

        _connectCts = new System.Threading.CancellationTokenSource();
        var token = _connectCts.Token;

        var semaphore = new System.Threading.SemaphoreSlim(threadCount);
        var tasks = new System.Collections.Generic.List<System.Threading.Tasks.Task>();
        var random = new Random();

        foreach (var acc in vm.SelectedAccountsForConnect)
        {
            acc.PostProcessStatus = "Chờ";
        }

        foreach (var acc in vm.SelectedAccountsForConnect)
        {
            await semaphore.WaitAsync();

            if (token.IsCancellationRequested)
            {
                semaphore.Release();
                break;
            }

            tasks.Add(System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() => { acc.PostProcessStatus = "Đang chạy"; });
                    
                    var startInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "python",
                        Arguments = $"main.py connect_thread {acc.Uid}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = System.Text.Encoding.UTF8,
                        StandardErrorEncoding = System.Text.Encoding.UTF8,
                        WorkingDirectory = @"d:\starup\pham_dai"
                    };

                    using (var process = new System.Diagnostics.Process { StartInfo = startInfo })
                    {
                        process.OutputDataReceived += (s, ev) =>
                        {
                            if (!string.IsNullOrEmpty(ev.Data))
                            {
                                if (ev.Data.Contains("[ACCOUNT_DIE]"))
                                {
                                    Application.Current.Dispatcher.Invoke(() => { 
                                        acc.Status = "Die"; 
                                        acc.PostProcessStatus = "Die";
                                    });
                                    return;
                                }
                                
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    TxtLogsConnect.AppendText($"[{DateTime.Now.ToString("HH:mm:ss")}] [{acc.Uid}]: {ev.Data}\n");
                                    TxtLogsConnect.ScrollToEnd();
                                });
                            }
                        };

                        process.Start();
                        process.BeginOutputReadLine();

                        // Wait for process to exit or cancellation
                        while (!process.HasExited)
                        {
                            if (token.IsCancellationRequested)
                            {
                                try { process.Kill(); } catch { }
                                break;
                            }
                            await System.Threading.Tasks.Task.Delay(500);
                        }
                    }

                    if (token.IsCancellationRequested)
                    {
                        Application.Current.Dispatcher.Invoke(() => { acc.PostProcessStatus = "Đã dừng"; });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() => { acc.PostProcessStatus = "Xong"; });
                    }
                    
                    // Delay before next
                    if (!token.IsCancellationRequested)
                    {
                        int delaySec = random.Next(delayMin, delayMax + 1);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            TxtLogsConnect.AppendText($"[{DateTime.Now.ToString("HH:mm:ss")}] [System]: Đợi {delaySec}s...\n");
                            TxtLogsConnect.ScrollToEnd();
                        });
                        await System.Threading.Tasks.Task.Delay(delaySec * 1000, token);
                    }
                }
                catch (System.OperationCanceledException)
                {
                    // Ignore
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        TxtLogsConnect.AppendText($"[{DateTime.Now.ToString("HH:mm:ss")}] [{acc.Uid}] Lỗi: {ex.Message}\n");
                        TxtLogsConnect.ScrollToEnd();
                    });
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }

        await System.Threading.Tasks.Task.WhenAll(tasks);

        if (token.IsCancellationRequested)
        {
            TxtLogsConnect.AppendText($"[{DateTime.Now.ToString("HH:mm:ss")}] [System]: Tiến trình bị hủy bởi người dùng.\n");
        }
        else
        {
            TxtLogsConnect.AppendText($"[{DateTime.Now.ToString("HH:mm:ss")}] [System]: Hoàn thành tất cả tác vụ kết nối.\n");
        }
        TxtLogsConnect.ScrollToEnd();
    }

    private void BtnStopConnect_Click(object sender, RoutedEventArgs e)
    {
        if (_connectCts != null && !_connectCts.IsCancellationRequested)
        {
            _connectCts.Cancel();
            TxtLogsConnect.AppendText($"[{DateTime.Now.ToString("HH:mm:ss")}] [System]: Đang dừng tiến trình kết nối threads...\n");
            TxtLogsConnect.ScrollToEnd();
        }
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