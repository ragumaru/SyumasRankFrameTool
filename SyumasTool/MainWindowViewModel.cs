using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyumasTool;

internal partial class MainWindowViewModel : ObservableObject
{
    /// <summary>
    /// ランキングExcelファイル名
    /// </summary>
    [ObservableProperty]
    private string _excelFile = string.Empty;

    /// <summary>
    /// 出力先フォルダー
    /// </summary>
    [ObservableProperty]
    private string _outputFolder = string.Empty;

    /// <summary>
    /// プログレスバーの進捗度
    /// </summary>
    [ObservableProperty]
    private int _progressValue = 0;

    [RelayCommand]
    private void ExcelFileBrowseButtonClick()
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

    [RelayCommand]
    private void OutputFolderBrowseButtonClick()
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
}
