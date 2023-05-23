using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace SyumasTool;

internal class GenCutin
{
    SKTypeface AzukiFont { get; } = SKTypeface.FromFile(Utils.AzukiFontPath);
    SKTypeface AzukiPFont { get; } = SKTypeface.FromFile(Utils.AzukiPFontPath);

    class CutinRowData
    {
        internal CutinRowData(DataRow row) => Row = row;

        internal DataRow Row { get; }

        /// <summary>識別子</summary>
        internal string Kind => Row[0].ToString() ?? string.Empty;

        /// <summary>週</summary>
        internal string Week => Row[1].ToString() ?? string.Empty;

        /// <summary>順位</summary>
        internal string Rank => Row[2].ToString() ?? string.Empty;

        /// <summary>Pts</summary>
        internal string Pts => Row[3].ToString() ?? string.Empty;

        /// <summary>差</summary>
        internal string Diff => Row[4].ToString() ?? string.Empty;

        /// <summary>補足</summary>
        internal string Remarks => Row[5].ToString() ?? string.Empty;
    }

    internal void Gen(string outputPath, DataTable cutinTable)
    {
        for (int i = 1; i < cutinTable.Rows.Count; i++)
        {
            using var surface = SKSurface.Create(new SKImageInfo(Utils.ImageWidth, Utils.ImageHeight));
            var canvas = surface.Canvas;

            var row = new CutinRowData(cutinTable.Rows[i]);

            switch (row.Kind)
            {
                case "A":
                    DrawPageA(canvas, row);
                    break;

                case "B":
                    DrawPageB(canvas, row);
                    break;

                default:
                    throw new Exception($"Cutinで識別子がA,B以外のものがありました。行：{i + 1}");
            }

            string outputFile = Path.Combine(outputPath, $"cutin{i}.png");
            using (var output = File.Create(outputFile))
            {
                surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100).SaveTo(output);
            }
        }
    }

    /// <summary>
    /// 識別子Aの画像を描画する
    /// </summary>
    void DrawPageA(SKCanvas canvas, CutinRowData row)
    {
        var p = new SKPoint(Utils.ImageWidth / 2, Utils.ImageHeight / 2 - 40);
        DrawOutlineString(canvas, 70, p, row.Rank);

        p.Offset(0, 120);
        DrawOutlineString(canvas, 45, p, row.Week);

        // 備考
        if (string.IsNullOrEmpty(row.Remarks)) return;

        DrawTextWithNewlines(canvas, new SKPoint(120, 120), row.Remarks);
    }

    /// <summary>
    /// 識別子Bの画像を描画する
    /// </summary>
    void DrawPageB(SKCanvas canvas, CutinRowData row)
    {
        var p = new SKPoint(Utils.ImageWidth / 2, 180);
        DrawOutlineString(canvas, 40, p, row.Week);

        p.Offset(0, 90);
        DrawOutlineString(canvas, 70, p, row.Rank);

        p.Offset(0, 60);
        DrawOutlineString(canvas, 30, p, $"{row.Pts}pts");

        p.Offset(0, 30);
        DrawOutlineString(canvas, 25, p, row.Diff);
    }

    /// <summary>
    /// 縁取り文字列を描画する
    /// </summary>
    void DrawOutlineString(SKCanvas canvas, float size, SKPoint point, string text)
    {
        // 縁取り
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            Typeface = AzukiPFont,
            TextSize = size,
            StrokeWidth = 2,    // 縁取りの太さ
            IsStroke = true,
            TextAlign = SKTextAlign.Center,
            IsAntialias = true,
        };

        canvas.DrawText(text, point, paint);

        // 本体
        paint.Color = SKColors.White;
        paint.IsStroke = false;
        paint.Style = SKPaintStyle.Fill;

        canvas.DrawText(text, point, paint);
    }

    /// <summary>
    /// 改行入りの文字列を描画する
    /// </summary>
    void DrawTextWithNewlines(SKCanvas canvas, SKPoint point, string text)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            Typeface = AzukiFont,
            TextSize = 20,
            FakeBoldText = true,
            IsAntialias = true,
        };

        SKRect rect = new();
        paint.MeasureText("A", ref rect);

        var lines = text.Split('\n');

        point.Offset(0, -((lines.Length - 1) * rect.Height));

        foreach (var line in lines)
        {
            canvas.DrawText(line, point, paint);
            point.Offset(0, rect.Height);
        }
    }
}