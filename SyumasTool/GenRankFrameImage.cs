using ExcelDataReader;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;
using static SyumasTool.GenRankFrameInfo;

namespace SyumasTool;

internal class GenRankFrameImage
{
    //class RowItem
    //{
    //    internal int Col { get; }
    //    internal string Value { get; set; }

    //    public RowItem(int col)
    //    {
    //        Col = col;
    //        Value = "";
    //    }

    //    internal void SetValue(DataRow row)
    //    {
    //        Value = row[Col].ToString() ?? "";
    //    }
    //}

    class RankingRowData
    {
        /// <summary>順位</summary>
        internal string Rank { get; }
        /// <summary>順位差識別</summary>
        internal string RankDiffMark { get; }
        /// <summary>変動</summary>
        internal string RankDiff { get; }
        /// <summary>動画ID</summary>
        internal string VideoID { get; }
        /// <summary>タイトル</summary>
        internal string VideoTitle { get; }
        /// <summary>Pts</summary>
        internal int Pts { get; }
        /// <summary>登録</summary>
        internal int Mylist { get; }
        /// <summary>再生</summary>
        internal int Play { get; }
        /// <summary>作者</summary>
        internal string Author { get; }
        /// <summary>投稿日</summary>
        internal DateTime PostDate { get; }
        /// <summary>補足</summary>
        internal string LongInfo { get; }

        public RankingRowData(DataRow row)
        {
            Rank = row[0].ToString() ?? "";
            RankDiffMark = row[1].ToString() ?? "";
            RankDiff = row[3].ToString() ?? "";
            VideoID = row[6].ToString() ?? "";
            VideoTitle = row[8].ToString() ?? "";
            Pts = int.Parse(row[9].ToString() ?? "0");
            Mylist = int.Parse(row[10].ToString() ?? "0");
            Play = int.Parse(row[11].ToString() ?? "0");
            Author = row[12].ToString() ?? "";
            PostDate = DateTime.Parse(row[13].ToString() ?? DateTime.MaxValue.ToString());
            LongInfo = row[14].ToString() ?? "";
        }
    }

    ///// <summary>
    ///// ランキングシート列番号
    ///// </summary>
    //struct RankingColInfo
    //{
    //    /// <summary>順位</summary>
    //    internal const int Rank = 0;
    //    /// <summary>順位差識別</summary>
    //    internal const int RankDiffMark = 1;
    //    /// <summary>変動</summary>
    //    internal const int RankDiff = 3;
    //    /// <summary>動画ID</summary>
    //    internal const int VideoID = 6;
    //    /// <summary>タイトル</summary>
    //    internal const int VideoTitle = 8;
    //    /// <summary>Pts</summary>
    //    internal const int Pts = 9;
    //    /// <summary>登録</summary>
    //    internal const int Mylist = 10;
    //    /// <summary>再生</summary>
    //    internal const int Play = 11;
    //    /// <summary>作者</summary>
    //    internal const int Author = 12;
    //    /// <summary>投稿日</summary>
    //    internal const int PostDate = 13;
    //    /// <summary>補足</summary>
    //    internal const int LongInfo = 14;
    //}

    //static readonly Dictionary<string, (string frameFile, SKColor color)> FrameKinds = new()
    //{
    //    { "U", ("frameUpW"  , SKColors.Red) },      // ランクアップ
    //    { "D", ("frameDownW", SKColors.Blue) },     // ランクダウン
    //    { "N", ("frameNewW" , SKColors.Green) },    // 新登場
    //    { "H", ("frameUpW"  , SKColors.Red) },      // 初登場
    //    { "K", ("frameKeepW", SKColors.Red) },      // キープ
    //    { "S", ("frameUpW"  , SKColors.Red) },      // 再登場
    //    { "L", ("frameLongW", SKColors.Orange) },   // 殿堂入り
    //};

    string Errmsg { get; set; } = "";
    RankingSettings Settings { get; init; } = GenRankFrameInfo.ReadInfo();

    public bool Gen(string outputPath, string reverseOutputPath, DataTable ranking)
    {
        // よく使うあずきフォントはここで定義
        using (var azukiFont = SKTypeface.FromFile(Utils.AzukiFontPath)) ;

        // 画像生成処理
        for (var i = 1; i < ranking.Rows.Count; i++)
        {
            var isRev = false;

            var data = new RankingRowData(ranking.Rows[i]);
            Errmsg = $"ranking:{i + 1}行目 タイトル:{data.VideoTitle} ";

            // "順位差識別"でフレーム画像を読み込み
            var frameInfo = GetFrameSetting(data);
            var frameFile = !isRev ? frameInfo.Normalfile : frameInfo.ReverseFile;
            var framePath = Path.Combine(Utils.FrameImagePath, frameFile);

            using var bitmap = SKBitmap.Decode(framePath);
            using var surface = SKSurface.Create(new SKImageInfo(bitmap.Width, bitmap.Height + 30));
            var canvas = surface.Canvas;

            canvas.DrawBitmap(bitmap, 0, !isRev ? 30 : 0);  // 設定にかく？

            // 順位
            float rankWidth;
            if (data.RankDiffMark != "L")
            {
                rankWidth = DrawRank(canvas, frameInfo, data, isRev);
            }
            else
            {
                rankWidth = DrawLongrun(canvas, isRev);
            }

            // 順位変動
            DrawRankChange(canvas, frameInfo.TextColor, data.RankDiff, isRev);

            // ポイント等
            DrawPoints(canvas, rankWidth, $"{data.Pts}pts 登録:{data.Mylist} 再生:{data.Play:#,0}", isRev);

            // 投稿者等
            DrawAuthor(canvas, bitmap.Width - 55, $"{data.Author} {data.VideoID} [{data.PostDate:d}]");

            // 連続
            DrawCont(canvas, bitmap.Width - 10, data.LongInfo);

            // 動画タイトル（長すぎて幅のスケールを変える場合、以後のcanvas操作に影響を与えるため最後に描画する）
            DrawTitle(canvas, rankWidth + 20, bitmap.Width, data.VideoTitle);

            canvas.Flush();

            // PNG形式で保存します。
            string outputFile = Path.Combine(outputPath, $"ranking_{i}.png");
            using (var output = File.Create(outputFile))
            {
                surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100).SaveTo(output);
            }
        }

        return true;
    }

    /// <summary>
    /// 設定ファイルから順位差識別の設定を返します。
    /// </summary>
    FrameBase GetFrameSetting(RankingRowData data)
    {
        try
        {
            return Settings.FrameBases.Single(n => n.Mark == data.RankDiffMark);
        }
        catch (InvalidOperationException)
        {
            throw new Exception($"{Errmsg} 順位差識別:{data.RankDiffMark} この順位差識別は設定ファイルに定義されていません。");
        }
        catch (Exception ex)
        {
            throw new Exception($"{Errmsg} 順位差識別の処理中に予期せぬエラーが発生しました。\n{ex.Message}");
        }
    }

    /// <summary>
    /// 順位を描画します。
    /// </summary>
    float DrawRank(SKCanvas canvas, FrameBase frameInfo, RankingRowData data, bool isRev)
    {
        var info = Settings.RankInfos.Rank;
        var point = new SKPoint(info.PosX, !isRev ? info.PosY : info.RevPosY);

        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            Typeface = SKTypeface.FromFamilyName("Meiryo UI", SKFontStyle.BoldItalic),
            TextSize = info.TextStyle.TextSize,
            IsAntialias = true,
        };

        var text = data.Rank;

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
        //paint.Color = SKColor.Parse(info.TextStyle?.TextColor ?? "Black");
        paint.Color = Utils.SKColorFromString(frameInfo.TextColor);
        paint.IsStroke = false;
        paint.Style = SKPaintStyle.Fill;

        canvas.DrawText(text, point, paint);

        return paint.MeasureText(text);
    }

    /// <summary>
    /// 長期作品の"★"を描画します。
    /// </summary>
    float DrawLongrun(SKCanvas canvas, bool isRev)
    {
        var info = Settings.RankInfos.RankLongrun;
        var point = new SKPoint(info.PosX, !isRev ? info.PosY : info.RevPosY);
        var text = "★";

        using var paint = new SKPaint
        {
            Color = Utils.SKColorFromString(info.TextStyle.TextColor),
            Typeface = SKTypeface.FromFile(Utils.AzukiFontPath),
            TextSize = info.TextStyle.TextSize,
            IsAntialias = true,
        };

        canvas.DrawText(text, point, paint);

        return paint.MeasureText(text);
    }

    /// <summary>
    /// 順位変動を描画します。
    /// </summary>
    void DrawRankChange(SKCanvas canvas, string colorText, string text, bool isRev)
    {
        var info = Settings.RankInfos.RankChange;
        var point = new SKPoint(info.PosX, !isRev ? info.PosY : info.RevPosY);

        using var paint = new SKPaint()
        {
            Color = SKColors.White,
            Typeface = SKTypeface.FromFile(Utils.AzukiFontPath),
            IsAntialias = true,
            IsStroke = true,
            StrokeWidth = 3,
            TextSize = info.TextStyle.TextSize,
        };

        canvas.DrawText(text, point, paint);

        paint.Color = Utils.SKColorFromString(colorText);
        paint.IsStroke = false;
        paint.Style = SKPaintStyle.Fill;

        canvas.DrawText(text, point, paint);
    }

    void DrawPoints(SKCanvas canvas, float rankWidth, string text, bool isRev)
    {
        var info = Settings.RankInfos.Points;
        var point = new SKPoint(rankWidth + info.OffsetX, !isRev ? info.PosY : info.RevPosY);

        using var paint = new SKPaint()
        {
            Color = Utils.SKColorFromString(info.TextStyle.TextColor),
            Typeface = SKTypeface.FromFile(Utils.AzukiFontPath),
            IsAntialias = true,
            TextSize = info.TextStyle.TextSize,
            StrokeWidth = info.TextStyle.StrokeWidth,
        };

        canvas.DrawText(text, point, paint);
    }

    /// <summary>
    /// 動画タイトルの描画
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="x"></param>
    /// <param name="text"></param>
    private static void DrawTitle(SKCanvas canvas, float x, int width, string text)
    {
        var point = new SKPoint(x, 58);

        using var paint = new SKPaint()
        {
            Color = SKColors.Black,
            Typeface = SKTypeface.FromFile(Utils.AzukiPFontPath),
            IsAntialias = true,
            TextSize = 22,
            //StrokeWidth = 2,
            IsStroke = true,
        };

        // テキストの幅を取得
        var titleWidth = paint.MeasureText(text);
        var titleRightX = x + titleWidth + 5;

        Debug.WriteLine($"{text} width:{titleWidth}  rightX:{titleRightX}  waku:{width}");

        if (titleRightX > width)
        {
            var sx = (width - x) / (titleWidth + 10);
            point.X = point.X * (1 / sx) - 5;

            Debug.WriteLine($"枠を超えました sx:{sx}");
            canvas.Scale(sx, 1);
        }

        canvas.DrawText(text, point, paint);
    }

    private static void DrawAuthor(SKCanvas canvas, float x, string text)
    {
        var point = new SKPoint(x, 100);

        using var paint = new SKPaint()
        {
            Color = SKColors.Black,
            Typeface = SKTypeface.FromFile(Utils.AzukiFontPath),
            IsAntialias = true,
            TextSize = 18,
            TextAlign = SKTextAlign.Right,
        };

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
            Typeface = SKTypeface.FromFile(Utils.AzukiFontPath),
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