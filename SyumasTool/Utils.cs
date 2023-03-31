using SkiaSharp;
using System.CodeDom;
using System.IO;

namespace SyumasTool;

internal static class Utils
{
    internal const int ImageHeight = 360;
    internal const int ImageWidth = 640;


    /// <summary>
    /// このアプリが走っているパスを返します。
    /// </summary>
    public static string BaseDir => AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>
    /// あずきフォントのパスを返します。
    /// </summary>
    public static string AzukiFontPath => Path.Combine(Utils.BaseDir, @"_fonts\azuki.ttf");
    public static string AzukiPFontPath => Path.Combine(Utils.BaseDir, @"_fonts\azukiP.ttf");

    /// <summary>
    /// フレームの画像ファイルのフォルダを返します。
    /// </summary>
    public static string FrameImagePath => Path.Combine(Utils.BaseDir, @"_image");

    public static SKColor SKColorFromString(string? colorText)
    {
        if (colorText == null) return SKColors.Black;
        var type = typeof(SKColors);
        var field = type.GetField(colorText);
        if (field == null) return SKColors.Black;
        var v = field.GetValue(null) ?? SKColors.Black;
        return (SKColor)v;
    }

    /// <summary>
    /// 日付形式の文字列を"[yyyy/mm/dd]"形式に変換して返します。
    /// 変換できないときは空文字となります。
    /// </summary>
    public static string StringDateFormat(string stringDate)
    {
        if (!DateTime.TryParse(stringDate, out var date))
        {
            return string.Empty;
        }

        return date.ToString("[yyyy/MM/dd]");
    }
}
