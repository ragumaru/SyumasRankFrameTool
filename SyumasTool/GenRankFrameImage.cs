using ExcelDataReader;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;

namespace SyumasTool;

internal class GenRankFrameImage
{
    public static bool Gen(string outputPath, DataTable ranking)
    {
        var frameImagePath = Path.Combine(Utils.BaseDir, @"_image\frameDownW.png");

        // フレーム読み込み
        using var bitmap = SKBitmap.Decode(frameImagePath);

        using var surface = SKSurface.Create(new SKImageInfo(bitmap.Width, bitmap.Height + 30));
        var canvas = surface.Canvas;
        canvas.DrawBitmap(bitmap, 0, 30);

        // 順位
        var rankWidth = DrawRank(canvas, "10");

        // 動画名
        DrawTitle(canvas, rankWidth + 20, "とどけ！あるある");

        // ポイント等
        DrawPoints(canvas, rankWidth + 20, "863pts 登録:106 再生:15,184");

        // 投稿者等
        DrawAuthor(canvas, bitmap.Width - 55, "佐伯氏 sm41747951 [2023/02/04]");

        // 順位変動
        DrawRankChange(canvas, "2(▼1)");

        // 連続
        DrawCont(canvas, bitmap.Width - 10, "2週連続2回目");



        // 確認用枠線
        using var paint = new SKPaint()
        {
            Color = SKColors.Red,
            StrokeWidth = 1,
            Style = SKPaintStyle.Stroke,
        };
        //canvas.DrawRect(0, 0, canvas.LocalClipBounds.Width - 3, canvas.LocalClipBounds.Height - 3, paint);

        canvas.Flush();

        // PNG形式で保存します。
        string outputFile = Path.Combine(outputPath, "test.png");
        using (var output = File.Create(outputFile))
        {
            surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100).SaveTo(output);
        }

        return true;
    }

    private static float DrawRank(SKCanvas canvas, string text)
    {
        var point = new SKPoint(10, 90);

        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            Typeface = SKTypeface.FromFamilyName("Meiryo UI", SKFontStyle.BoldItalic),
            TextSize = 65,
            IsAntialias = true,
        };

        // 影
        paint.ImageFilter = SKImageFilter.CreateBlur(3, 3);

        canvas.DrawText(text, point.X, point.Y + 8, paint);

        // 縁取り
        paint.Color = SKColors.White;
        paint.StrokeWidth = 3;    // 縁取りの太さ
        paint.IsStroke = true;
        paint.ImageFilter = null;

        canvas.DrawText(text, point, paint);

        // 本体
        paint.Color = SKColors.Blue;
        paint.IsStroke = false;
        paint.Style = SKPaintStyle.Fill;

        canvas.DrawText(text, point, paint);

        return paint.MeasureText(text);
    }

    private static void DrawTitle(SKCanvas canvas, float x, string text)
    {
        var point = new SKPoint(x, 58);

        using var paint = new SKPaint()
        {
            Color = SKColors.Black,
            Typeface = SKTypeface.FromFile(GenMain.AzukiFontPath),
            IsAntialias = true,
            TextSize = 22,
            //StrokeWidth = 2,
            IsStroke = true,
        };

        canvas.DrawText(text, point, paint);
    }

    private static void DrawPoints(SKCanvas canvas, float x, string text)
    {
        var point = new SKPoint(x, 80);

        using var paint = new SKPaint()
        {
            Color = SKColors.Black,
            Typeface = SKTypeface.FromFile(GenMain.AzukiFontPath),
            IsAntialias = true,
            TextSize = 18,
            StrokeWidth = 1.2f,
        };

        canvas.DrawText(text, point, paint);
    }

    private static void DrawAuthor(SKCanvas canvas, float x, string text)
    {
        var point = new SKPoint(x, 100);

        using var paint = new SKPaint()
        {
            Color = SKColors.Black,
            Typeface = SKTypeface.FromFile(GenMain.AzukiFontPath),
            IsAntialias = true,
            TextSize = 18,
            TextAlign = SKTextAlign.Right,
        };

        canvas.DrawText(text, point, paint);
    }

    private static void DrawRankChange(SKCanvas canvas, string text)
    {
        var point = new SKPoint(5, 28);

        using var paint = new SKPaint()
        {
            Color = SKColors.White,
            Typeface = SKTypeface.FromFile(GenMain.AzukiFontPath),
            IsAntialias = true,
            IsStroke = true,
            StrokeWidth = 3,
            TextSize = 28,
        };

        canvas.DrawText(text, point, paint);

        paint.Color = SKColors.Blue;
        paint.IsStroke = false;
        paint.Style = SKPaintStyle.Fill;

        canvas.DrawText(text, point, paint);
    }

    /// <summary>
    /// x週連続x回目
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="fontPath"></param>
    /// <param name="x"></param>
    /// <param name="text"></param>
    private static void DrawCont(SKCanvas canvas, float x, string text)
    {
        var point = new SKPoint(x, 25);

        using var paint = new SKPaint()
        {
            Color = SKColors.White,
            Typeface = SKTypeface.FromFile(GenMain.AzukiFontPath),
            IsAntialias = true,
            StrokeWidth = 7,
            IsStroke = true,
            TextSize = 22,
            ImageFilter = SKImageFilter.CreateBlur(3, 3),
            TextAlign = SKTextAlign.Right,
        };

        canvas.DrawText(text, point, paint);

        paint.Color = SKColors.Black;
        paint.IsStroke = false;
        paint.StrokeWidth = 1.2f;
        paint.ImageFilter = null;

        canvas.DrawText(text, point, paint);
    }

}
