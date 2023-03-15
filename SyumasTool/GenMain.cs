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

    public static readonly string AzukiFontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"_fonts\azuki.ttf");

    public static void MainProc(string excelFilePath)
    {
        // あずきフォントがあるかどうか
        if (!File.Exists(AzukiFontPath))
        {
            throw new Exception($"あずきフォントが見つかりません。\n" +
                $"読み込み先パス「{AzukiFontPath}」が存在しません。");
        }

        // ExcelDataReaderのおまじない
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        using (var stream = File.Open(excelFilePath, FileMode.Open, FileAccess.Read))
        using (var reader = ExcelReaderFactory.CreateReader(stream))
        {
            var result = reader.AsDataSet();

            // 出力先フォルダー作成
            var exePath = AppDomain.CurrentDomain.BaseDirectory;
            var outputPath = Path.Combine(exePath, $"frame{DateTime.Now.ToString("yyyyMMdd_HHmmss")}");

            outputPath = @"D:\app\tool\imas_rank_tool\frame";//dbg

            if (Directory.Exists(outputPath))
            {
                throw new Exception($"出力先フォルダー「{outputPath}」がすでに存在しました。もう一度実行してください。");
            }

            // ランキングフレーム生成
            if (result.Tables.Contains(XlShRanking))
            {
                GenRankFrameImage.Gen(outputPath, result.Tables[XlShRanking]);
            }



        }
    }
}