using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace SyumasTool;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void FileBrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var fileDialog = new OpenFileDialog()
        {
            Title = "ランキングExcelファイルを指定",
            Filter = "Excelファイル(*.xls, *.xlsx)|*.xls;*.xlsx",
        };

        if (fileDialog.ShowDialog() == true)
        {
            FileTextBox.Text = fileDialog.FileName;
        }
    }

    private void ExecButton_Click(object sender, RoutedEventArgs e)
    {
        if (String.IsNullOrEmpty(FileTextBox.Text))
        {
            MessageBox.Show("ランキングExcelファイルが入力されていません。");
            return;
        }

        if (!File.Exists(FileTextBox.Text))
        {
            MessageBox.Show("ランキングExcelファイルが存在しません。");
            return;
        }

        try
        {
            GenMain.MainProc(FileTextBox.Text);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }


        MessageBox.Show("完了しました!");
    }
}
