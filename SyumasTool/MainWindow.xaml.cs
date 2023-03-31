using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System.Configuration;
using System.IO;
using System.Runtime.Intrinsics.X86;
using System.Windows;
using System.Windows.Input;

namespace SyumasTool;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    MainWindowViewModel vm;

    public MainWindow()
    {
        InitializeComponent();

        vm = (MainWindowViewModel)DataContext;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        vm.ExcelFile = Properties.Settings.Default["ExcelFilePath"].ToString() ?? "";
        vm.OutputFolder = Properties.Settings.Default["OutputFolder"].ToString() ?? "";
    }

    private async void ExecButton_Click(object sender, RoutedEventArgs e)
    {
        if (String.IsNullOrEmpty(vm.ExcelFile))
        {
            MessageBox.Show("ランキングExcelファイルが入力されていません。");
            return;
        }

        if (!File.Exists(vm.ExcelFile))
        {
            MessageBox.Show("ランキングExcelファイルが存在しません。");
            return;
        }

        if (!Directory.Exists(vm.OutputFolder))
        {
            MessageBox.Show("画像出力フォルダが存在しません。");
            return;
        }

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            IsEnabled = false;
            Progress.Value = 0;
            Progress.Visibility = Visibility.Visible;

            Properties.Settings.Default["ExcelFilePath"] = vm.ExcelFile;
            Properties.Settings.Default["OutputFolder"] = vm.OutputFolder;
            Properties.Settings.Default.Save();

            var p = new Progress<int>(e => vm.ProgressValue = e);

            var res = await GenMain.MainProc(vm.ExcelFile, vm.OutputFolder, p);

            MessageBox.Show($"完了しました!\n");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        finally
        {
            Mouse.OverrideCursor = null;
            this.IsEnabled = true;
            Progress.Visibility = Visibility.Hidden;
        }
    }
}