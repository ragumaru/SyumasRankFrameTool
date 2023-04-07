using SkiaSharp;
using System.CodeDom;
using System.IO;

namespace SyumasTool;

internal static class Utils
{
    //internal const int ImageWidth = 1280; // でかすぎて余白が出まくる
    //internal const int ImageHeight = 720; // Ver3までは512*384(4:3) ワイドフレームの横640で16:9にすると640x360になり、縦が小さくなっちゃう
    internal const int ImageWidth = 864;
    internal const int ImageHeight = 486;


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

    /// <summary>
    /// Excelの日付形式のセルからDateTime形式でデータを取り出します。
    /// </summary>
    public static DateTime? GetExcelDateTime(string? s)
    {
        // Nullならそのまま返す
        if (s == null) return null;

        // おとなしくDateTime型に変換できれば変換して返す
        if (DateTime.TryParse(s, out var d)) return d;

        // シリアル値っぽかったらDoubleに変換する。ダメだったらNullで返す
        if (!Double.TryParse(s, out var r)) return null;

        // シリアル値からDateTimeに変換する、Exceptionが発生したらNullで返す
        try
        {
            return DateTime.FromOADate(r);
        }
        catch
        {
            return null;
        }
    }
}