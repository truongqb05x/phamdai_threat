using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ThreadsManagerUI
{
    public partial class SelectAccountWindow : Window
    {
        public List<Models.AccountModel> SelectedAccounts { get; private set; }

        public SelectAccountWindow(ObservableCollection<Models.AccountModel> allAccounts)
        {
            InitializeComponent();
            SelectedAccounts = new List<Models.AccountModel>();
            
            // Lọc các tài khoản có trạng thái Live
            var liveAccounts = allAccounts.Where(a => a.Status == "Live").ToList();
            DgAccounts.ItemsSource = liveAccounts;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ChkSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            DgAccounts.SelectAll();
        }

        private void ChkSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            DgAccounts.UnselectAll();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            SelectedAccounts = DgAccounts.SelectedItems.Cast<Models.AccountModel>().ToList();
            this.DialogResult = true;
            this.Close();
        }
    }
}
