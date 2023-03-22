using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyumasTool;
internal class GenMain
{
    static readonly string XlShRanking = "ranking";

    public static readonly string AzukiFontPath = Path.Combine(Utils.BaseDir, @"_fonts\azuki.ttf");

    public static async Task<bool> MainProc(string excelFilePath, string outputFolder)
    {
        // あずきフォントがあるかどうか
        if (!File.Exists(AzukiFontPath))
        {
            throw new Exception($"あずきフォントが見つかりません。\n" +
                $"読み込み先パス \"{AzukiFontPath}\" が存在しません。");
        }

        // ExcelDataReaderのおまじない（xls形式対応）
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        await Task.Run(() =>
        {

            using (var stream = File.Open(excelFilePath, FileMode.Open, FileAccess.Read))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
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

                    GenRankFrameImage.Gen(rankingOutputPath, rankingReversePath, result.Tables[XlShRanking]!);
                }
            }
        });

        return true;
    }
}