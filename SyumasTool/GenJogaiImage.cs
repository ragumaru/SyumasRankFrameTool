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





        /*
        while (true)
        {
            using var surface = SKSurface.Create(new SKImageInfo(Utils.ImageWidth, Utils.ImageHeight));
            var canvas = surface.Canvas;

            DrawTitle(canvas);

            var y = InitY;

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
                    y += 22;
                }

                // 動画データを描画
                DrawVideoData(canvas, y, row);
                y += 70;

                // ページ下まで到達しそうだったら改ページ
                // TODO:次の行の除外理由がブレークするかどうかも考慮に入れる必要あり
                if (y + 70 > Utils.ImageHeight)
                {
                    prevExclusionReason = "";
                    i++;
                    break;
                }
            }

            // ここに来たら保存
            canvas.Flush();

            string outputFile = Path.Combine(outputPath, $"jogai{page}.png");
            using (var output = File.Create(outputFile))
            {
                surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100).SaveTo(output);
            }

            page++;

            // データが終わりなら抜ける
            if (i >= jogaiTable.Rows.Count) break;
        }
        */




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

            y += 70;
            if (y + 70 > Utils.ImageHeight)
            {
                page++;
                y = InitY;
                prevExclusionReason = "";
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
        var titleRightX = titleWidth + x + 15;

        if (titleRightX > Utils.ImageWidth)
        {
            var sx = (Utils.ImageWidth - x) / (titleWidth + 10);
            x = x * (1 / sx) - 5;

            Debug.WriteLine($"枠を超えました sx:{sx}");
            canvas.Scale(sx, 1);
        }

        canvas.DrawText(row.Title, x, y, paint);

        //canvas.Scale(0);
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
            //StrokeWidth = 1.1f,
            //IsStroke = true,
            //Style = SKPaintStyle.StrokeAndFill,
            IsAntialias = true,
        };

        //canvas.DrawText(row.VideoTitle, 35, y, paint);

        // ポイント・マイリス・再生・作者
        //canvas.DrawText($"{row.Points}pts　登録:{row.Mylist}　再生:{row.Play}　{row.Author}", 50, y + 20, paint);
        canvas.DrawText(row.Points, 50, y, paint);

        // 動画ID・投稿日
        //canvas.DrawText($"{row.VideoID}  {row.FormattedPostDate}", 50, y + 40, paint);
        canvas.DrawText(row.VideoID, 50, y + 20, paint);

        //Debug.WriteLine($"{row.VideoID}  Y:{y}  Ascent:{paint.FontMetrics.Ascent}  Top:{paint.FontMetrics.Top}  Bottom:{paint.FontMetrics.Bottom}  Descent:{paint.FontMetrics.Descent}");
    }
}
