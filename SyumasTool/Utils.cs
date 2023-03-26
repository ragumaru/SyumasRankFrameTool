using SkiaSharp;
using System.CodeDom;
using System.IO;

namespace SyumasTool;

internal static class Utils
{
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
}
