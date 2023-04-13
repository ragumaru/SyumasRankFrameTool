using SkiaSharp;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace SyumasTool;

/// <summary>
/// ランキングフレームを描画する。
/// </summary>
// 当初はJSONファイルに設定値を書き込んでおき、それを読み込んで
// 各種文字列を描画する方法を取ろうとしていたが、
// 設定が複雑になること。読み込みが複雑になること。その割には自由度が無いこと。
// などから設定ファイル化を断念し、ソースコード上に固定値として設定値をもつこととした。
class GenRankFrameImage
{
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

    static readonly Dictionary<string, (string frameFile, SKColor color)> FrameKinds = new()
    {
        { "U", ("frameUpW"  , SKColors.Red) },      // ランクアップ
        { "D", ("frameDownW", SKColors.Blue) },     // ランクダウン
        { "N", ("frameNewW" , SKColors.Green) },    // 新登場
        { "H", ("frameUpW"  , SKColors.Red) },      // 初登場
        { "K", ("frameKeepW", SKColors.Red) },      // キープ
        { "S", ("frameUpW"  , SKColors.Red) },      // 再登場
        { "L", ("frameLongW", SKColors.Orange) },   // 殿堂入り
    };

    string Errmsg { get; set; } = "";
    SKTypeface AzukiFont { get; } = SKTypeface.FromFile(Utils.AzukiFontPath);
    //RankingSettings Settings { get; init; } = GenRankFrameInfo.ReadInfo();

    public bool Gen(string outputPath, string reverseOutputPath, DataTable ranking, IProgress<int> progress, int progressMax)
    {
        // よく使うあずきフォントはここで定義
        //AzukiFont = SKTypeface.FromFile(Utils.AzukiFontPath);

        // 画像生成処理
        for (var i = 1; i < ranking.Rows.Count; i++)
        {
            foreach (var isRev in new[] { false, true })
            {
                //var isRev = false;

                var data = new RankingRowData(ranking.Rows[i]);
                Errmsg = $"ranking:{i + 1}行目 タイトル:{data.VideoTitle} ";

                // "順位差識別"でフレーム画像を読み込み
                if (!FrameKinds.TryGetValue(data.RankDiffMark, out var frameInfo))
                {
                    throw new Exception($"{Errmsg} 順位差識別:{data.RankDiffMark} この順位差識別は設定ファイルに定義されていません。");
                }

                //var frameFile = frameInfo.frameFile + (isRev ? "U" : "") + ".png";
                var frameFile = frameInfo.frameFile + ".png";
                var framePath = Path.Combine(Utils.BaseDir, "_image", frameFile);

                using var bitmap = SKBitmap.Decode(framePath);
                using var surface = SKSurface.Create(new SKImageInfo(bitmap.Width, bitmap.Height + 30));
                var canvas = surface.Canvas;

                canvas.DrawBitmap(bitmap, 0, !isRev ? 30 : 0);

                // 順位
                float rankWidth;
                if (data.RankDiffMark != "L")
                {
                    var point = new SKPoint(10, !isRev ? 90 : 60);
                    rankWidth = DrawRank(canvas, frameInfo.color, point, data.Rank);
                }
                else
                {
                    var point = new SKPoint(10, !isRev ? 70 : 40);
                    rankWidth = DrawText(canvas, point, SKColors.Orange, 35, "★");
                }

                // 順位変動
                var rcPoint = new SKPoint(5, !isRev ? 28 : 102);
                DrawRankChange(canvas, frameInfo.color, rcPoint, data.RankDiff);

                // ポイント等
                var pPoint = new SKPoint(rankWidth + 20, !isRev ? 80 : 50);
                DrawText(canvas, pPoint, SKColors.Black, 18, $"{data.Pts}pts 登録:{data.Mylist} 再生:{data.Play}");

                // 投稿者等
                var aPoint = new SKPoint(bitmap.Width - 55, !isRev ? 100 : 70);
                DrawText(canvas, aPoint, SKColors.Black, 18, $"{data.Author} {data.VideoID} [{data.PostDate:d}]", SKTextAlign.Right);

                // 連続
                var cPoint = new SKPoint(bitmap.Width - 10, !isRev ? 25 : 102);
                DrawCont(canvas, cPoint, data.LongInfo);

                // 動画タイトル（長すぎて幅のスケールを変える場合、以後のcanvas操作に影響を与えるため最後に描画する）
                var tPoint = new SKPoint(rankWidth + 20, !isRev ? 58 : 28);
                DrawTitle(canvas, tPoint, bitmap.Width, data.VideoTitle);

                // PNG形式で保存します。
                canvas.Flush();

                string outputFile = Path.Combine(outputPath, isRev ? "reverse" : "", $"ranking_{i}.png");
                using (var output = File.Create(outputFile))
                {
                    surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100).SaveTo(output);
                }
            }

            progress.Report((int)i * progressMax / ranking.Rows.Count);
        }

        return true;
    }

    /// <summary>
    /// 順位を描画します。
    /// </summary>
    float DrawRank(SKCanvas canvas, SKColor color, SKPoint point, string text)
    {
        // 影の描画
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            Typeface = SKTypeface.FromFamilyName("Meiryo UI", SKFontStyle.BoldItalic),
            TextSize = 65,
            IsAntialias = true,
            ImageFilter = SKImageFilter.CreateBlur(3, 3),
        };

        canvas.DrawText(text, point.X, point.Y + 8, paint);

        // 縁取り
        paint.Color = SKColors.White;
        paint.StrokeWidth = 3;    // 縁取りの太さ
        paint.IsStroke = true;
        paint.ImageFilter = null;

        canvas.DrawText(text, point, paint);

        // 本体
        paint.Color = color;
        paint.IsStroke = false;
        paint.Style = SKPaintStyle.Fill;

        canvas.DrawText(text, point, paint);

        return paint.MeasureText(text);
    }

    /// <summary>
    /// 順位変動を描画します。
    /// </summary>
    void DrawRankChange(SKCanvas canvas, SKColor color, SKPoint point, string text)
    {
        // 縁取り
        using var paint = new SKPaint()
        {
            Color = SKColors.White,
            Typeface = AzukiFont,
            IsAntialias = true,
            IsStroke = true,
            StrokeWidth = 3,
            TextSize = 28,
        };

        canvas.DrawText(text, point, paint);

        // 本体
        paint.Color = color;
        paint.IsStroke = false;
        paint.Style = SKPaintStyle.Fill;

        canvas.DrawText(text, point, paint);
    }

    /// <summary>
    /// 動画タイトルの描画
    /// </summary>
    void DrawTitle(SKCanvas canvas, SKPoint point, int width, string text)
    {
        using var paint = new SKPaint()
        {
            Color = SKColors.Black,
            Typeface = SKTypeface.FromFile(Utils.AzukiPFontPath),
            IsAntialias = true,
            TextSize = 22,
            StrokeWidth = 1.4f,
            IsStroke = true,
            Style = SKPaintStyle.StrokeAndFill,
        };

        // テキストの幅を取得
        var titleWidth = paint.MeasureText(text);
        var titleRightX = point.X + titleWidth + 5;

        Debug.WriteLine($"{text} width:{titleWidth}  rightX:{titleRightX}  waku:{width}");

        if (titleRightX > width)
        {
            var sx = (width - point.X) / (titleWidth + 10);
            point.X = point.X * (1 / sx) - 5;

            Debug.WriteLine($"枠を超えました sx:{sx}");
            canvas.Scale(sx, 1);
        }

        canvas.DrawText(text, point, paint);
    }

    /// <summary>
    /// x週連続x回目
    /// </summary>
    void DrawCont(SKCanvas canvas, SKPoint point, string text)
    {
        using var paint = new SKPaint()
        {
            Color = SKColors.White,
            Typeface = AzukiFont,
            IsAntialias = true,
            StrokeWidth = 7,
            IsStroke = true,
            TextSize = 22,
            ImageFilter = SKImageFilter.CreateBlur(3, 3),
            TextAlign = SKTextAlign.Right,
        };

        canvas.DrawText(text, point, paint);

        paint.Color = SKColors.Black;
        paint.StrokeWidth = 1.1f;
        paint.Style = SKPaintStyle.StrokeAndFill;
        paint.ImageFilter = null;

        canvas.DrawText(text, point, paint);
    }

    /// <summary>
    /// 共通文字描画処理
    /// </summary>
    float DrawText(
        SKCanvas canvas,
        SKPoint point,
        SKColor color,
        float textSize,
        string text,
        SKTextAlign textAlign = SKTextAlign.Left)
    {
        using var paint = new SKPaint
        {
            Color = color,
            Typeface = AzukiFont,
            TextSize = textSize,
            TextAlign = textAlign,
            IsAntialias = true,
        };

        canvas.DrawText(text, point, paint);

        return paint.MeasureText(text);
    }
}