using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyumasTool;

internal partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _excelFile = string.Empty;

    [ObservableProperty]
    private string _outputFolder = string.Empty;

    [ObservableProperty]
    private int _progressValue = 0;
}
