using SkiaSharp;
using System.Data;
using System.IO;

namespace SyumasTool;

internal class GenOutOfRangeImage
{
    SKTypeface AzukiFont { get; } = SKTypeface.FromFile(Utils.AzukiFontPath);
    SKTypeface AzukiPFont { get; } = SKTypeface.FromFile(Utils.AzukiPFontPath);

    const int RowsPerPage = 20;

    const int TitleY = 30;
    const int HeaderY = 65;
    const int DataInitY = 90;

    const int ColX1 = 60;   // 順位
    const int ColX2 = 90;   // 変動
    const int ColX3 = 210;  // pts
    const int ColX4 = 230;  // マーク
    const int ColX5 = 260;  // タイトル

    class OutOfRangeRowData
    {
        internal OutOfRangeRowData(DataRow row) => Row = row;

        DataRow Row { get; }

        /// <summary>順位</summary>
        internal int Rank => Utils.GetExcelInt(Row, 0) ?? 0;

        /// <summary>変動</summary>
        internal string RankDiff => Row[1].ToString() ?? string.Empty;

        /// <summary>点数</summary>
        internal int Pts => Utils.GetExcelInt(Row, 2) ?? 0;

        /// <summary>除外</summary>
        internal string ExclusionMark => Row[3].ToString() ?? string.Empty;

        /// <summary>タイトル</summary>
        internal string VideoTitle => Row[4].ToString() ?? string.Empty;
    }

    internal void Gen(string outputPath, DataTable outOfRangeTable)
    {
        // ページタイトル取得
        var row = new OutOfRangeRowData(outOfRangeTable.Rows[1]);
        var pageTitle = row.VideoTitle;

        // データの描画
        int totalPage = (int)Math.Ceiling((double)(outOfRangeTable.Rows.Count - 2) / RowsPerPage);

        for (var page = 1; page <= totalPage; page++)
        {
            using var surface = SKSurface.Create(new SKImageInfo(Utils.ImageWidth, Utils.ImageHeight));
            var canvas = surface.Canvas;

            DrawPageTitle(canvas, pageTitle);
            DrawDataHeader(canvas);

            for (var i = 0; i < RowsPerPage; i++)
            {
                var r = (page - 1) * RowsPerPage + i + 2;
                if (r > outOfRangeTable.Rows.Count - 1) break;

                row = new OutOfRangeRowData(outOfRangeTable.Rows[r]);

                DrawData(canvas, i * 20 + DataInitY, row);
            }

            canvas.Flush();

            string outputFile = Path.Combine(outputPath, $"outOfRange{page}.png");
            using (var output = File.Create(outputFile))
            {
                surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100).SaveTo(output);
            }
        }
    }

    /// <summary>
    /// ページタイトル描画
    /// </summary>
    void DrawPageTitle(SKCanvas canvas, string title)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            Typeface = AzukiFont,
            TextSize = 22,
            TextAlign = SKTextAlign.Center,
            StrokeWidth = 1.4f,
            IsStroke = true,
            Style = SKPaintStyle.StrokeAndFill,
            IsAntialias = true,
        };

        canvas.DrawText($"┗{title}┣", Utils.ImageWidth / 2, TitleY, paint);
    }

    /// <summary>
    /// データヘッダ描画
    /// </summary>
    void DrawDataHeader(SKCanvas canvas)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            Typeface = AzukiFont,
            TextSize = 18,
            FakeBoldText = true,
            IsAntialias = true,
        };

        paint.TextAlign = SKTextAlign.Center;
        canvas.DrawText("順位", ColX1, HeaderY, paint);

        paint.TextAlign = SKTextAlign.Left;
        canvas.DrawText("変動", ColX2 + 5, HeaderY, paint);

        paint.TextAlign = SKTextAlign.Right;
        canvas.DrawText("pts", ColX3, HeaderY, paint);

        paint.TextAlign = SKTextAlign.Left;
        canvas.DrawText("タイトル", ColX5, HeaderY, paint);
    }

    /// <summary>
    /// データ描画
    /// </summary>
    void DrawData(SKCanvas canvas, int y, OutOfRangeRowData row)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            Typeface = AzukiFont,
            TextSize = 16,
            FakeBoldText = true,
            IsAntialias = true,
        };

        paint.TextAlign = SKTextAlign.Center;
        canvas.DrawText(row.Rank.ToString(), ColX1, y, paint);

        paint.TextAlign = SKTextAlign.Left;
        canvas.DrawText(row.RankDiff, ColX2, y, paint);

        paint.TextAlign = SKTextAlign.Right;
        canvas.DrawText(row.Pts.ToString(), ColX3, y, paint);

        // 除外マークの色
        switch (row.ExclusionMark)
        {
            case "長" or "★":
                paint.Color = SKColors.Orange;
                break;

            case "シ":
                paint.Color = SKColors.Blue;
                break;

            default:
                paint.Color = SKColors.Red;
                break;
        }

        paint.TextAlign = SKTextAlign.Left;
        canvas.DrawText(row.ExclusionMark, ColX4, y, paint);

        DrawVideoTitle(canvas, y, row.VideoTitle);
    }

    /// <summary>
    /// 動画タイトルの描画
    /// </summary>
    void DrawVideoTitle(SKCanvas canvas, int y, string s)
    {
        using var paint = new SKPaint()
        {
            Color = SKColors.Black,
            Typeface = AzukiPFont,
            TextSize = 16,
            FakeBoldText = true,
            IsAntialias = true,
        };

        // テキストの幅を取得
        var titleWidth = paint.MeasureText(s);
        var maxWidth = Utils.ImageWidth - ColX5 - 15;

        if (titleWidth > maxWidth)
        {
            var sx = maxWidth / titleWidth;
            paint.TextScaleX = sx;
        }

        canvas.DrawText(s, ColX5, y, paint);
    }
}