using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ookii.Dialogs.Wpf;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace SyumasTool;

internal partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel()
    {
        ExcelFile = Properties.Settings.Default["ExcelFilePath"].ToString() ?? "";
        OutputFolder = Properties.Settings.Default["OutputFolder"].ToString() ?? "";
    }

    /// <summary>
    /// ランキングExcelファイル名
    /// </summary>
    [ObservableProperty]
    string _excelFile = string.Empty;

    /// <summary>
    /// 出力先フォルダー
    /// </summary>
    [ObservableProperty]
    string _outputFolder = string.Empty;

    /// <summary>
    /// プログレスバーの進捗度
    /// </summary>
    [ObservableProperty]
    int _progressValue = 0;

    /// <summary>
    /// プログレスバー表示状態
    /// </summary>
    [ObservableProperty]
    Visibility _progressVisibility = Visibility.Hidden;

    /// <summary>
    /// ウィンドウ活性状態
    /// </summary>
    [ObservableProperty]
    bool _windowEnabled = true;


    /// <summary>
    /// Excelファイルの参照ボタン
    /// </summary>
    [RelayCommand]
    void ExcelFileBrowseButtonClick()
    {
        var dialog = new VistaOpenFileDialog()
        {
            Title = "ランキングExcelファイルを指定",
            Filter = "Excelファイル(*.xls, *.xlsx)|*.xls;*.xlsx",
            FileName = ExcelFile,
        };

        if (dialog.ShowDialog() == true)
        {
            ExcelFile = dialog.FileName;
        }
    }

    /// <summary>
    /// 出力先フォルダーの参照ボタン
    /// </summary>
    [RelayCommand]
    void OutputFolderBrowseButtonClick()
    {
        var dialog = new VistaFolderBrowserDialog()
        {
            SelectedPath = OutputFolder,
        };

        if (dialog.ShowDialog() == true)
        {
            OutputFolder = dialog.SelectedPath;
        }
    }

    [RelayCommand]
    async void ExecButtonClick()
    {
        if (String.IsNullOrEmpty(ExcelFile))
        {
            MessageBox.Show("ランキングExcelファイルが入力されていません。");
            return;
        }

        if (!File.Exists(ExcelFile))
        {
            MessageBox.Show("ランキングExcelファイルが存在しません。");
            return;
        }

        if (!Directory.Exists(OutputFolder))
        {
            MessageBox.Show("画像出力フォルダが存在しません。");
            return;
        }

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            WindowEnabled = false;
            ProgressValue = 0;
            ProgressVisibility = Visibility.Visible;

            Properties.Settings.Default["ExcelFilePath"] = ExcelFile;
            Properties.Settings.Default["OutputFolder"] = OutputFolder;
            Properties.Settings.Default.Save();

            var p = new Progress<int>(v => ProgressValue = v);

            var res = await GenMain.MainProc(ExcelFile, OutputFolder, p);

            MessageBox.Show($"完了しました!\n", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        finally
        {
            Mouse.OverrideCursor = null;
            ProgressVisibility = Visibility.Hidden;
            WindowEnabled = true;
        }
    }
}