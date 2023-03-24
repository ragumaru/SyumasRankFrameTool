﻿using ExcelDataReader;
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
            using var bitmap = SKBitmap.Decode(GetFramePath(data, isRev));

            using var surface = SKSurface.Create(new SKImageInfo(bitmap.Width, bitmap.Height + 30));
            var canvas = surface.Canvas;

            canvas.DrawBitmap(bitmap, 0, !isRev ? 30 : 0);  // 設定にかく？

            // 順位
            var isLongrun = data.RankDiffMark == "L" ? true : false;
            var rankWidth = DrawRank(canvas, isLongrun, data.Rank, isRev);

            // ポイント等
            DrawPoints(canvas, rankWidth + 20, $"{data.Pts}pts 登録:{data.Mylist} 再生:{data.Play}");

            // 投稿者等
            DrawAuthor(canvas, bitmap.Width - 55, $"{data.Author} {data.VideoID} [{data.PostDate:d}]");

            // 順位変動
            //DrawRankChange(canvas, frameInfo.color, data.RankDiff);

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
    /// 順位差識別を設定ファイルから探し、フレームのファイル名を返します。
    /// </summary>
    string GetFramePath(RankingRowData data, bool isRev)
    {
        try
        {
            var frameInfo = Settings.FrameBases.Single(n => n.Mark == data.RankDiffMark);
            var frameFile = !isRev ? frameInfo.Normalfile : frameInfo.ReverseFile;

            return Path.Combine(Utils.FrameImagePath, frameFile);
        }
        catch (InvalidOperationException ex)
        {
            throw new Exception($"{Errmsg} 順位差識別:{data.RankDiffMark} この順位差識別は設定ファイルに定義されていません。");
        }
        catch (Exception ex)
        {
            throw new Exception($"{Errmsg} 順位差識別の処理中に予期せぬエラーが発生しました。");
        }
    }

    /// <summary>
    /// 順位を描画します。
    /// </summary>
    float DrawRank(SKCanvas canvas, bool isLongrun, string text, bool isRev)
    {
        var info = Settings.RankInfos.Rank;
        var point = new SKPoint(info.PosX, !isRev ? info.PosY : info.RevPosY);

        using var paint = new SKPaint
        {
            //Color = SKColor.Parse(info.TextStyle?.TextColor ?? "Black"),
            Typeface = SKTypeface.FromFamilyName("Meiryo UI", SKFontStyle.BoldItalic),
            TextSize = info.TextStyle?.TextSize ?? 65,
            IsAntialias = true,
        };

        // 長期作品（殿堂入り）の場合、描画内容を書き換え
        if (isLongrun)
        {
            text = "★";
            point.Y = 70;
            paint.Color = SKColors.Orange;
            paint.Typeface = SKTypeface.FromFile(Utils.AzukiFontPath);
            paint.TextSize = 35;
        }

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
        paint.Color = SKColor.Parse(info.TextStyle?.TextColor ?? "Black");
        paint.IsStroke = false;
        paint.Style = SKPaintStyle.Fill;

        canvas.DrawText(text, point, paint);

        return paint.MeasureText(text);
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
            Typeface = SKTypeface.FromFile(Utils.AzukiFontPath),
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

    private static void DrawPoints(SKCanvas canvas, float x, string text)
    {
        var point = new SKPoint(x, 80);

        using var paint = new SKPaint()
        {
            Color = SKColors.Black,
            //Typeface = SKTypeface.FromFile(GenMain.AzukiFontPath),
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
            //Typeface = SKTypeface.FromFile(GenMain.AzukiFontPath),
            IsAntialias = true,
            TextSize = 18,
            TextAlign = SKTextAlign.Right,
        };

        canvas.DrawText(text, point, paint);
    }

    private static void DrawRankChange(SKCanvas canvas, SKColor color, string text)
    {
        var point = new SKPoint(5, 28);

        using var paint = new SKPaint()
        {
            Color = SKColors.White,
            //Typeface = SKTypeface.FromFile(GenMain.AzukiFontPath),
            IsAntialias = true,
            IsStroke = true,
            StrokeWidth = 3,
            TextSize = 28,
        };

        canvas.DrawText(text, point, paint);

        paint.Color = color;
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
            //Typeface = SKTypeface.FromFile(GenMain.AzukiFontPath),
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