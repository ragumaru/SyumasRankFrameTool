using SkiaSharp;
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

    /// <summary>
    /// フレームの画像ファイルのフォルダを返します。
    /// </summary>
    public static string FrameImagePath => Path.Combine(Utils.BaseDir, @"_image");
}
