using System.IO;
using System.Text.Json;

namespace SyumasTool;

internal class GenRankFrameInfo
{
    public record RankingSettings
    {
        public required List<FrameBase> FrameBases { get; set; }
        public required RankInfo RankInfos { get; set; }
    };

    public record FrameBase
    {
        public required string Mark { get; set; }
        public required string TextColor { get; set; }
        public required string Normalfile { get; set; }
        public required string ReverseFile { get; set; }
    }

    public record RankInfo
    {
        public required DrawInfo Rank { get; set; }
        public required DrawInfo Points { get; set; }
    }

    public record DrawInfo
    {
        public int PosX { get; set; }
        public int OffsetX { get; set; }
        public int PosY { get; set; }
        public int RevPosY { get; set; }
        public TextStyle? TextStyle { get; set; }
    }

    public record TextStyle
    {
        public string? Font { get; set; }
        public string? TextColor { get; set; }
        public int? TextSize { get; set; }
        public double? StrokeWidth { get; set; }
    }


    internal static RankingSettings ReadInfo()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            AllowTrailingCommas = true,
        };

        var jsonFile = Path.Combine(Utils.BaseDir, "settings", "RankInfo.json");
        var jsonStr = File.ReadAllText(jsonFile);

        var rankingSettings = JsonSerializer.Deserialize<RankingSettings>(jsonStr, options);

        if (rankingSettings == null) throw new Exception("ランキング設定ファイルが読み込めませんでした。");

        return rankingSettings;
    }
}