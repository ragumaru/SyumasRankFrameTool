using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using static System.Net.Mime.MediaTypeNames;

namespace SyumasTool;

internal class GenJogaiImage
{
    class JogaiRowData
    {
        DataRow Row { get; }

        /// <summary>除外理由</summary>
        internal string ExclusionReason => Row[0].ToString() ?? "";

        /// <summary>タイトル</summary>
        internal string VideoTitle => Row[1].ToString() ?? "";

        /// <summary>点数</summary>
        internal string Points => Row[2].ToString() ?? "";

        /// <summary>登録数</summary>
        internal string Mylist => Row[3].ToString() ?? "";

        /// <summary>再生数</summary>
        internal string Play => Row[4].ToString() ?? "";

        /// <summary>作者</summary>
        internal string Author => Row[5].ToString() ?? "";

        /// <summary>動画ID</summary>
        internal string VideoID => Row[6].ToString() ?? "";

        /// <summary>投稿日</summary>
        internal DateTime PostDate => (DateTime)Row[7];


        internal JogaiRowData(DataRow row)
        {
            Row = row;
        }
    }

    SKTypeface AzukiFont { get; } = SKTypeface.FromFile(Utils.AzukiFontPath);
    const int InitY = 60;

    internal void Gen(string outputPath, DataTable jogaiTable, IProgress<int> progress)
    {
        var prevExclusionReason = "";
        var page = 1;
        var y = InitY;
        var i = 1;

        while (true)
        {
            using var surface = SKSurface.Create(new SKImageInfo(Utils.ImageWidth, Utils.ImageHeight));
            var canvas = surface.Canvas;

            DrawTitle(canvas);

            for (; i < jogaiTable.Rows.Count; i++)
            {
                var row = new JogaiRowData(jogaiTable.Rows[i]);

                // 除外理由が"*"の場合は改ページ
                if (row.ExclusionReason == "*") break;

                // 除外理由が変わった時は除外理由を描画
                if (prevExclusionReason != row.ExclusionReason)
                {
                    prevExclusionReason = row.ExclusionReason;
                    DrawExclusionReson(canvas, y, row.ExclusionReason);
                    y += 20;
                }

                // 動画データを描画
                DrawVideoData(canvas, y, row);
                y += 50;

                break;
            }

            // ここに来たら保存
            canvas.Flush();

            string outputFile = Path.Combine(outputPath, $"jogai{page}.png");
            using (var output = File.Create(outputFile))
            {
                surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100).SaveTo(output);
            }



            // 初期描画以外、除外理由がブレークまたは"*"の時、改ページ
            break;
        }





    }

    void SaveImage()
    {

    }

    //SKSurface CreateNewPage()
    //{
    //    var surface = SKSurface.Create(new SKImageInfo(bitmap.Width, bitmap.Height + 30));


    //    return surface
    //    }

    void DrawTitle(SKCanvas canvas)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            Typeface = AzukiFont,
            TextSize = 20,
            TextAlign = SKTextAlign.Center,
            StrokeWidth = 1.4f,
            IsStroke = true,
            Style = SKPaintStyle.StrokeAndFill,
            IsAntialias = true,
        };

        var point = new SKPoint(Utils.ImageWidth / 2, 30);
        canvas.DrawText("┗圏内に入った除外カテゴリ作品┣", point, paint);
    }


    void DrawExclusionReson(SKCanvas canvas, int y, string text)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Red,
            Typeface = AzukiFont,
            TextSize = 16,
            IsAntialias = true,
        };

        canvas.DrawText($"★{text}", 10, y, paint);
    }

    void DrawVideoData(SKCanvas canvas, int y, JogaiRowData row)
    {
        // タイトル
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            Typeface = AzukiFont,
            TextSize = 14,
            IsAntialias = true,
        };

        canvas.DrawText(row.VideoTitle, 20, y, paint);

        // ポイント・マイリス・再生・作者
        paint.TextSize = 12;
        canvas.DrawText($"{row.Points}pts　登録:{row.Mylist}　再生:{row.Play}　{row.Author}", 30, y + 25, paint);

        // 動画ID・投稿日
        canvas.DrawText($"{row.VideoID}  {row.PostDate:d}", 30, y + 45, paint);
    }
}
