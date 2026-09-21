using System;
using System.IO;
using System.Windows;

namespace ThreadsManagerUI;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        
        // Mặc định hiển thị path gốc của tool
        TxtProfilePath.Text = AppDomain.CurrentDomain.BaseDirectory;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        // Hiển thị thông báo lưu thành công
        MessageBox.Show("Lưu cài đặt thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        this.Close();
    }
}
