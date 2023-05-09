using SkiaSharp;
using System.Data;
using System.IO;

namespace SyumasTool;

class GenPlayRankImage
{
    SKTypeface AzukiFont { get; } = SKTypeface.FromFile(Utils.AzukiFontPath);
    SKTypeface AzukiPFont { get; } = SKTypeface.FromFile(Utils.AzukiPFontPath);

    const string SummaryTitle = "┗今週の集計概要┣";
    const int Scol1x = 290;
    const int Scol2x = 410;
    const int Scol3x = 510;
    const int Scol4x = 650;

    const string PlayRankTitle = "┗登録数及び再生数の順位┣";
    const int PHcol1x = 30;
    const int PHcol2x = 532;
    const int PHcol3x = 590;
    const int PHcol4x = 720;
    const int PDcol1x = 30;
    const int PDcol2x = 550;
    const int PDcol3x = 610;
    const int PDcol4x = 740;

    class SummaryRowData
    {
        DataRow Row { get; }

        /// <summary>項目</summary>
        internal string ItemName => Row[0].ToString() ?? "";

        /// <summary>今週</summary>
        internal int thisWeek => int.Parse(Row[1].ToString() ?? "0");

        /// <summary>先週比</summary>
        internal int lastWeekRatio => int.Parse(Row[2].ToString() ?? "0");

        /// <summary>新作</summary>
        internal int NewTitles => int.Parse(Row[3].ToString() ?? "0");

        internal SummaryRowData(DataRow row)
        {
            Row = row;
        }
    }

    class PlayRankRowData
    {
        DataRow Row { get; }

        /// <summary>タイトル</summary>
        internal string VideoTitle => Row[0].ToString() ?? "";

        /// <summary>順位</summary>
        internal int Rank => Utils.GetExcelInt(Row, 1) ?? 0;

        /// <summary>登録順位</summary>
        internal int MylistRank => Utils.GetExcelInt(Row, 2) ?? 0;

        /// <summary>登録数</summary>
        internal int Mylists => Utils.GetExcelInt(Row, 3) ?? 0;

        /// <summary>再生順位</summary>
        internal int PlayRank => Utils.GetExcelInt(Row, 4) ?? 0;

        /// <summary>再生数</summary>
        internal int Plays => Utils.GetExcelInt(Row, 5) ?? 0;

        internal PlayRankRowData(DataRow row) => Row = row;
    }

    internal void Gen(string outputPath, DataTable summaryTable, DataTable playRankTable)
    {
        using var surface = SKSurface.Create(new SKImageInfo(Utils.ImageWidth, Utils.ImageHeight));
        var canvas = surface.Canvas;

        // 画像生成処理
        DrawSummary(summaryTable, canvas);
        DrawPlayRank(playRankTable, canvas);

        canvas.Flush();

        string outputFile = Path.Combine(outputPath, "playRank.png");
        using (var output = File.Create(outputFile))
        {
            surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100).SaveTo(output);
        }
    }

    /// <summary>
    /// 「今週の集計概要」の描画
    /// </summary>
    void DrawSummary(DataTable table, SKCanvas canvas)
    {
        // タイトル
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            Typeface = AzukiFont,
            TextSize = 22,
            TextAlign = SKTextAlign.Center,
            FakeBoldText = true,
            IsAntialias = true,
        };

        var point = new SKPoint(Utils.ImageWidth / 2, 30);
        canvas.DrawText(SummaryTitle, point, paint);

        // データ
        int y = 55;

        paint.TextSize = 20;
        //paint.FakeBoldText = false;

        for (var i = 1; i < table.Rows.Count; i++)
        {
            var row = new SummaryRowData(table.Rows[i]);

            paint.TextAlign = SKTextAlign.Right;
            canvas.DrawText(row.ItemName, Scol1x, y, paint);

            // 今週
            canvas.DrawText($"{row.thisWeek:N0}", Scol2x, y, paint);

            // 先週比（項目名）
            canvas.DrawText("（先週比:", Scol3x, y, paint);

            // 先週比
            switch (row.lastWeekRatio)
            {
                case > 0:
                    paint.Color = SKColors.Red;
                    break;

                case < 0:
                    paint.Color = SKColors.Blue;
                    break;

                default:
                    paint.Color = SKColors.Green;
                    break;
            }

            canvas.DrawText($"{row.lastWeekRatio:▲#,##0;▼#,##0;±0}", Scol4x, y, paint);

            // 新作（作品数行のみ）
            paint.Color = SKColors.Black;
            paint.TextAlign = SKTextAlign.Left;

            if (i == 1)
            {
                canvas.DrawText($"、新作: {row.NewTitles:N0}）", Scol4x + 3, y, paint);
            }
            else
            {
                canvas.DrawText("）", Scol4x + 3, y, paint);
            }

            y += 23;
        }
    }

    /// <summary>
    /// 「登録数及び再生数の順位」の描画
    /// </summary>
    void DrawPlayRank(DataTable table, SKCanvas canvas)
    {
        // タイトル
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            Typeface = AzukiFont,
            TextSize = 22,
            TextAlign = SKTextAlign.Center,
            FakeBoldText = true,
            IsAntialias = true,
        };

        var point = new SKPoint(Utils.ImageWidth / 2, 150);
        canvas.DrawText(PlayRankTitle, point, paint);

        // 項目名
        int y = 175;

        paint.TextSize = 18;
        paint.TextAlign = SKTextAlign.Left;

        canvas.DrawText("タイトル", PHcol1x, y, paint);
        canvas.DrawText("順位", PHcol2x, y, paint);
        canvas.DrawText("登録順位", PHcol3x, y, paint);
        canvas.DrawText("再生順位", PHcol4x, y, paint);

        // データ
        y += 25;

        //paint.FakeBoldText = false;

        int v = (Utils.ImageHeight - y) / (table.Rows.Count - 1);

        for (var i = 1; i < table.Rows.Count; i++)
        {
            var row = new PlayRankRowData(table.Rows[i]);

            // タイトル
            DrawVideoTitle(canvas, y, row.VideoTitle);

            // 順位
            paint.Color = GetRankColor(row.Rank);
            paint.TextAlign = SKTextAlign.Center;

            canvas.DrawText($"{row.Rank}", PDcol2x, y, paint);

            // 登録順位
            paint.Color = GetRankColor(row.MylistRank);
            paint.TextAlign = SKTextAlign.Right;

            canvas.DrawText($"{row.MylistRank}", PDcol3x, y, paint);

            paint.TextAlign = SKTextAlign.Left;

            canvas.DrawText($"{row.Mylists:(#,0);;#}", PDcol3x + 3, y, paint);

            // 再生順位
            paint.Color = GetRankColor(row.PlayRank);
            paint.TextAlign = SKTextAlign.Right;

            canvas.DrawText($"{row.PlayRank}", PDcol4x, y, paint);

            paint.TextAlign = SKTextAlign.Left;

            canvas.DrawText($"{row.Plays:(#,0);;#}", PDcol4x + 3, y, paint);

            y += v;
        }
    }

    /// <summary>
    /// 動画タイトルの描画
    /// </summary>
    void DrawVideoTitle(SKCanvas canvas, int y, string videoTitle)
    {
        using var paint = new SKPaint()
        {
            Color = SKColors.Black,
            Typeface = AzukiPFont,
            TextSize = 18,
            FakeBoldText = true,
            IsAntialias = true,
        };

        // テキスト幅が規定値を超えていたら縮めて描画
        var titleWidth = paint.MeasureText(videoTitle);

        if (PDcol1x + titleWidth > PHcol2x - PDcol1x)
        {
            var sx = (PHcol2x - PDcol1x) / (PDcol1x + titleWidth);
            paint.TextScaleX = sx;
        }

        canvas.DrawText(videoTitle, PDcol1x, y, paint);
    }

    /// <summary>
    /// 順位ごとの色を返します。
    /// </summary>
    SKColor GetRankColor(int rank)
    {
        switch (rank)
        {
            case 1:
                return SKColors.Red;

            case 2:
                return SKColors.Blue;

            case 3:
                return SKColors.Green;

            default:
                return SKColors.Black;
        }
    }
}