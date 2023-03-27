using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Input;

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

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        //FileTextBox.Text = ConfigurationManager.AppSettings["ExcelFilePath"];
        //OutputFolderTextBox.Text = ConfigurationManager.AppSettings["OutputFolder"];
        FileTextBox.Text = Properties.Settings.Default["ExcelFilePath"].ToString();
        OutputFolderTextBox.Text = Properties.Settings.Default["OutputFolder"].ToString();

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

    private void FolderBrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var folderDialog = new VistaFolderBrowserDialog();
        if (folderDialog.ShowDialog() == true)
        {
            OutputFolderTextBox.Text = folderDialog.SelectedPath;
        }
    }

    private async void ExecButton_Click(object sender, RoutedEventArgs e)
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

        if (!Directory.Exists(OutputFolderTextBox.Text))
        {
            MessageBox.Show("画像出力フォルダが存在しません。");
            return;
        }

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            IsEnabled = false;

            //Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //config.AppSettings.Settings["ExcelFilePath"].Value = FileTextBox.Text;
            //config.AppSettings.Settings["OutputFolder"].Value = OutputFolderTextBox.Text;
            //config.Save();
            Properties.Settings.Default["ExcelFilePath"] = FileTextBox.Text;
            Properties.Settings.Default["OutputFolder"] = OutputFolderTextBox.Text;
            Properties.Settings.Default.Save();

            var res = await GenMain.MainProc(FileTextBox.Text, OutputFolderTextBox.Text);

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
        }
    }
}