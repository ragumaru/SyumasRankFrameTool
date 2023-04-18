using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyumasTool;
internal class GenMain
{
    static readonly string XlShRanking = "ranking";
    static readonly string XlShJogai = "jogai";
    static readonly string XlShSummary = "summary";
    static readonly string XlShPlayRank = "play_rank";

    public static async Task<bool> MainProc(string excelFilePath, string outputFolder, IProgress<int> progress)
    {
        // あずきフォントがあるかどうか
        if (!File.Exists(Utils.AzukiFontPath))
        {
            throw new Exception($"あずきフォントが見つかりません。\n" +
                $"読み込み先パス \"{Utils.AzukiFontPath}\" が存在しません。");
        }

        // ExcelDataReaderのおまじない（xls形式対応）
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        await Task.Run(() =>
        {
            try
            {
                using var stream = File.Open(excelFilePath, FileMode.Open, FileAccess.Read);
                using var reader = ExcelReaderFactory.CreateReader(stream);

                // 出力先フォルダー作成
                var outputPath = Path.Combine(outputFolder, $"frame{DateTime.Now.ToString("yyyyMMdd_HHmmss")}");

                if (Directory.Exists(outputPath))
                {
                    throw new Exception($"出力先フォルダー「{outputPath}」がすでに存在しました。もう一度実行してください。");
                }

                Directory.CreateDirectory(outputPath);

                // ランキングExcelファイルの内容をDataSetとして取得
                var result = reader.AsDataSet();

                // ランキングフレーム生成
                if (result.Tables.Contains(XlShRanking))
                {
                    var rankingOutputPath = Path.Combine(outputPath, XlShRanking);
                    Directory.CreateDirectory(rankingOutputPath);

                    var rankingReversePath = Path.Combine(rankingOutputPath, "reverse");
                    Directory.CreateDirectory(rankingReversePath);

                    var rankGen = new GenRankFrameImage();
                    rankGen.Gen(rankingOutputPath, rankingReversePath, result.Tables[XlShRanking]!, progress, 95);
                }

                // 除外画像生成
                if (result.Tables.Contains(XlShJogai))
                {
                    var jogaiOutputPath = Path.Combine(outputPath, XlShJogai);
                    Directory.CreateDirectory(jogaiOutputPath);

                    var genJogai = new GenJogaiImage();
                    genJogai.Gen(jogaiOutputPath, result.Tables[XlShJogai]!);
                    progress.Report(96);
                }

                // 集計概要・再生数順位
                if (result.Tables.Contains(XlShSummary) && result.Tables.Contains(XlShPlayRank))
                {
                    var playRankOutputPath = Path.Combine(outputPath, "playRank");
                    Directory.CreateDirectory(playRankOutputPath);

                    var genPlayRank = new GenPlayRankImage();
                    genPlayRank.Gen(playRankOutputPath, result.Tables[XlShSummary]!, result.Tables[XlShPlayRank]!);
                    progress.Report(97);
                }

            }
            catch (IOException)
            {
                // 分かりやすいエラーにして返す
                throw new Exception("指定したExcelファイルが既に開かれている可能性があります。\n閉じてから再度実行して下さい。");
            }
        });

        progress.Report(100);
        return true;
    }
}