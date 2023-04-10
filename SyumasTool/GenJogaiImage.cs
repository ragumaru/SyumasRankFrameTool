using SkiaSharp;
using System.Data;
using System.Diagnostics;
using System.IO;

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
        internal DateTime? PostDate => Utils.GetExcelDateTime(Row[7].ToString());

        internal string FormattedPostDate => PostDate?.ToString("[yyyy/MM/dd]") ?? "(不明な日付)";

        internal JogaiRowData(DataRow row)
        {
            Row = row;
        }
    }

    struct JogaiRow
    {
        internal int Page;
        internal int Y;
        internal string Reason;
        internal string Title;
        internal string Points;
        internal string VideoID;
    }

    SKTypeface AzukiFont { get; } = SKTypeface.FromFile(Utils.AzukiFontPath);
    SKTypeface AzukiPFont { get; } = SKTypeface.FromFile(Utils.AzukiPFontPath);
    const int InitY = 60;

    internal void Gen(string outputPath, DataTable jogaiTable, IProgress<int> progress)
    {
        List<JogaiRow> jogaiList = GenList(jogaiTable);

        // データを描画
        for (var page = 1; page <= jogaiList.Max(d => d.Page); page++)
        {
            using var surface = SKSurface.Create(new SKImageInfo(Utils.ImageWidth, Utils.ImageHeight));
            var canvas = surface.Canvas;

            DrawHeader(canvas);

            foreach (var row in jogaiList.Where(d => d.Page == page))
            {
                var y = row.Y;
                if (!String.IsNullOrEmpty(row.Reason))
                {
                    DrawExclusionReson(canvas, y, row.Reason);
                    y += 22;
                }

                DrawVideoTitle(canvas, y, row);
                DrawVideoData(canvas, y + 20, row);
            }

            canvas.Flush();

            string outputFile = Path.Combine(outputPath, $"jogai{page}.png");
            using (var output = File.Create(outputFile))
            {
                surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100).SaveTo(output);
            }
        }
    }

    /// <summary>
    /// データのページ付け
    /// </summary>
    List<JogaiRow> GenList(DataTable jogaiTable)
    {
        List<JogaiRow> jogaiList = new();

        var prevExclusionReason = "";
        var page = 1;
        var y = InitY;

        for (var i = 1; i < jogaiTable.Rows.Count; i++)
        {
            var row = new JogaiRowData(jogaiTable.Rows[i]);

            // 改ページマーク"*"があったら改ページ
            if (row.ExclusionReason == "*")
            {
                if (y != InitY) page++;
                y = InitY;
                prevExclusionReason = "";
                continue;
            }

            // 新しい行の入れ物
            var newRow = new JogaiRow()
            {
                Page = page,
                Y = y,
                Title = row.VideoTitle,
                Points = $"{row.Points}pts　登録:{row.Mylist}　再生:{row.Play}　{row.Author}",
                VideoID = $"{row.VideoID}  {row.FormattedPostDate}",
            };

            // 除外理由が変わったら除外理由を入れておく
            if (prevExclusionReason != row.ExclusionReason)
            {
                prevExclusionReason = row.ExclusionReason;
                newRow.Reason = row.ExclusionReason;
                y += 22;
            }

            jogaiList.Add(newRow);

            Debug.WriteLine($"y:{y} ({row.VideoTitle})");

            y += 70;

            bool isNextReasonBreak = false;
            if (i < jogaiTable.Rows.Count - 1)
            {
                var nextRow = new JogaiRowData(jogaiTable.Rows[i + 1]);
                if (nextRow.ExclusionReason != row.ExclusionReason)
                {
                    isNextReasonBreak = true;
                }
            }

            if (y > Utils.ImageHeight - 54 - (isNextReasonBreak ? 10 : 0))
            {
                page++;
                y = InitY;
                prevExclusionReason = "";

                Debug.WriteLine($"改ページ {page}");
            }
        }

        return jogaiList;
    }

    void DrawHeader(SKCanvas canvas)
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

        var point = new SKPoint(Utils.ImageWidth / 2, 30);
        canvas.DrawText("┗圏内に入った除外カテゴリ作品┣", point, paint);
    }


    void DrawExclusionReson(SKCanvas canvas, int y, string text)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Red,
            Typeface = AzukiFont,
            TextSize = 20,
            StrokeWidth = 1.2f,
            IsStroke = true,
            Style = SKPaintStyle.StrokeAndFill,
            IsAntialias = true,
        };

        canvas.DrawText($"★{text}", 20, y, paint);
    }

    /// <summary>
    /// 動画タイトルの描画
    /// </summary>
    void DrawVideoTitle(SKCanvas canvas, int y, JogaiRow row)
    {
        using var paint = new SKPaint()
        {
            Color = SKColors.Black,
            Typeface = AzukiPFont,
            TextSize = 18,
            FakeBoldText = true,
            IsAntialias = true,
        };

        // テキストの幅を取得
        float x = 35;
        var titleWidth = paint.MeasureText(row.Title);

        if (x + titleWidth > Utils.ImageWidth - 15)
        {
            var sx = (Utils.ImageWidth - 15) / (x + titleWidth);
            paint.TextScaleX = sx;
        }

        canvas.DrawText(row.Title, x, y, paint);
    }

    void DrawVideoData(SKCanvas canvas, int y, JogaiRow row)
    {
        // 動画タイトル
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            Typeface = AzukiPFont,
            TextSize = 16,
            FakeBoldText = true,
            IsAntialias = true,
        };

        // ポイント・マイリス・再生・作者
        canvas.DrawText(row.Points, 50, y, paint);

        // 動画ID・投稿日
        canvas.DrawText(row.VideoID, 50, y + 20, paint);
    }
}