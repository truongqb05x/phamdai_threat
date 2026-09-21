using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ThreadsManagerUI.Models;

namespace ThreadsManagerUI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<AccountModel> Accounts { get; set; }

        private int _totalAccounts;
        public int TotalAccounts
        {
            get => _totalAccounts;
            set { _totalAccounts = value; OnPropertyChanged(); }
        }

        private int _liveAccounts;
        public int LiveAccounts
        {
            get => _liveAccounts;
            set { _liveAccounts = value; OnPropertyChanged(); }
        }

        public MainViewModel()
        {
            Accounts = new System.Collections.ObjectModel.ObservableCollection<AccountModel>();
            LoadData();
        }

        private string GetDataFilePath()
        {
            string devPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\resources\account.txt"));
            if (System.IO.File.Exists(devPath)) return devPath;
            return "account.txt"; // Fallback
        }

        public void LoadData()
        {
            Accounts.Clear();
            string path = GetDataFilePath();
            if (System.IO.File.Exists(path))
            {
                var lines = System.IO.File.ReadAllLines(path);
                int index = 1;
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split('|');
                    var acc = new AccountModel
                    {
                        Index = index++,
                        Uid = parts.Length > 0 ? parts[0] : "",
                        Password = parts.Length > 1 ? parts[1] : "",
                        Cookie = parts.Length > 2 ? parts[2] : "",
                        Token = parts.Length > 3 ? parts[3] : "",
                        Email = parts.Length > 4 ? parts[4] : "",
                        PassEmail = parts.Length > 5 ? parts[5] : "",
                        TwoFA = parts.Length > 6 ? parts[6] : "",
                        Proxy = parts.Length > 7 ? parts[7] : "",
                        UserAgent = parts.Length > 8 ? parts[8] : "",
                        Status = "All"
                    };
                    
                    // Lắng nghe sự kiện thay đổi trên từng property để tự động lưu
                    acc.PropertyChanged += (s, e) => {
                        if (e.PropertyName != nameof(AccountModel.IsSelected))
                        {
                            SaveData();
                        }
                    };
                    Accounts.Add(acc);
                }
            }
            UpdateStats();
            
            // Lắng nghe thay đổi của collection (Thêm, xóa)
            Accounts.CollectionChanged += (s, e) => {
                if (e.NewItems != null)
                {
                    foreach (AccountModel newItem in e.NewItems)
                    {
                        newItem.PropertyChanged += (sender, args) => {
                            if (args.PropertyName != nameof(AccountModel.IsSelected))
                                SaveData();
                        };
                    }
                }
                UpdateStats();
                SaveData();
            };
        }

        public void SaveData()
        {
            string path = GetDataFilePath();
            var lines = new System.Collections.Generic.List<string>();
            foreach (var acc in Accounts)
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

                // Loại bỏ các phần tử rỗng ở cuối (giống như copy)
                while (parts.Count > 0 && string.IsNullOrEmpty(parts[^1]))
                {
                    parts.RemoveAt(parts.Count - 1);
                }

                lines.Add(string.Join("|", parts));
            }
            
            try
            {
                System.IO.File.WriteAllLines(path, lines);
            }
            catch { }
        }

        public void UpdateStats()
        {
            TotalAccounts = Accounts.Count;
            int liveCount = 0;
            foreach(var acc in Accounts) {
                if (acc.Status == "Live") liveCount++;
            }
            LiveAccounts = liveCount;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
