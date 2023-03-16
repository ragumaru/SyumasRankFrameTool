using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyumasTool;

internal static class Utils
{
    /// <summary>
    /// このアプリが走っているパスを返します。
    /// </summary>
    public static string BaseDir => AppDomain.CurrentDomain.BaseDirectory;
}
