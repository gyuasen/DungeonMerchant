using System;
using System.Collections.Generic;

/// <summary>
/// Owns facility greeting definitions, deterministic daily dialogue selection,
/// and the in-memory per-day greeting skip state.
/// </summary>
public sealed class FacilityGreetingController
{
    public const string TavernKey = "Tavern";
    public const string GuildKey = "Guild";
    public const string MarketKey = "Market";
    public const string BlacksmithKey = "Blacksmith";
    public const string WarehouseKey = "Warehouse";
    public const string ClinicKey = "Clinic";
    public const string TempleKey = "Temple";
    public const string TrainingGroundKey = "TrainingGround";

    private readonly HashSet<string> enteredFacilities = new HashSet<string>();

    private static readonly Dictionary<string, FacilityGreetingDefinition> Definitions =
        CreateDefinitions();

    private static Dictionary<string, FacilityGreetingDefinition> CreateDefinitions()
    {
        Dictionary<string, FacilityGreetingDefinition> definitions =
            new Dictionary<string, FacilityGreetingDefinition>();
        AddDefinition(definitions, TavernKey, "{0}の酒場の女将",
            "あんたが例の商会の息子かい。人手は商品だよ。報酬を決めたなら、雇い手を見せてやる。",
            "親御さんの帳面は覚えてるよ。ここじゃ腕の立つ傭兵を雇える、あとはあんたの値付け次第さ。",
            "仕事を出すなら先に条件を聞こうか。酒場では傭兵の雇用を取り持ってる。");
        AddDefinition(definitions, GuildKey, "{0}の商会組合受付",
            "旧商会の紹介状なら通る。組合では輸送部隊や遠征部隊の差配も引き受けよう。",
            "商売は情けじゃ回らない。だが親御さんの取引は確かだった、組合の窓口を使うといい。",
            "再建の帳尻を合わせる気なら歓迎する。ここで商会の状況を確認できる。");
        AddDefinition(definitions, MarketKey, "{0}の市場の元締め",
            "親御さんの品はよく動いた。市場では売買ができる、今日の相場で損をしないようにな。",
            "金と品を並べれば話は早い。市場で仕入れと売却をするなら、値札をよく見な。",
            "懐かしい看板の息子か。市場の値は日ごとに変わる、商機を逃すなよ。");
        AddDefinition(definitions, BlacksmithKey, "{0}の鍛冶師",
            "あんたが例の商会の息子か。腕は落ちちゃいない、素材と金さえ持ってくればいつでも打ってやる。",
            "親御さんとは現物で話をした仲だ。ここでは武具の強化ができる、代金の用意はあるか。",
            "鉄は嘘をつかないが、勘定は先だ。素材を出せば、この鍛冶場で装備を鍛えてやる。");
        AddDefinition(definitions, WarehouseKey, "{0}の倉庫番",
            "親御さんの荷札なら見覚えがある。倉庫では品物と装備を預かる、置き場の勘定も忘れるな。",
            "荷は資本だ、濡らすな失くすな。ここで在庫と装備を整理していきな。",
            "帳面と現物が合ってこその商会だ。倉庫の出し入れは、あんたの責任で頼むよ。");
        AddDefinition(definitions, ClinicKey, "{0}の治療師",
            "親御さんの商会には何度か薬を回した。治療が要るなら診るが、薬代まで忘れるんじゃないよ。",
            "怪我人を働かせる商売は長続きしない。ここで傭兵を治療して、次の仕事へ戻しな。",
            "顔色が悪い連れがいるね。治療院は慈善所じゃないが、治せる傷なら手を貸す。");
        AddDefinition(definitions, TempleKey, "{0}の神官",
            "ご両親の商会が納めた寄進は記録にある。適性と奉納が揃えば、ここで職を改められる。",
            "神意にも手続きと対価は要る。転職を望む者がいるなら、祭壇の前へ連れてきなさい。",
            "商いの家の子よ、道を変えるなら覚悟を示せ。この神殿では傭兵の転職を執り行う。");
        AddDefinition(definitions, TrainingGroundKey, "{0}の修練場",
            "一日で一段、確かな力を積み上げましょう。",
            "新入りにも追いつく道はあります。焦らず鍛えましょう。",
            "修練中の者は、明日にはひと回り頼もしくなります。");
        return definitions;
    }

    private static void AddDefinition(
        Dictionary<string, FacilityGreetingDefinition> definitions,
        string facilityKey,
        string titleFormat,
        params string[] dialogues)
    {
        definitions.Add(facilityKey, new FacilityGreetingDefinition(titleFormat, dialogues));
    }

    public bool ShouldShowGreeting(int day, int townIndex, string facilityKey)
    {
        return !enteredFacilities.Contains(BuildVisitKey(day, townIndex, facilityKey));
    }

    public void MarkEntered(int day, int townIndex, string facilityKey)
    {
        enteredFacilities.Add(BuildVisitKey(day, townIndex, facilityKey));
    }

    public FacilityGreeting GetGreeting(int day, int townIndex, string townName, string facilityKey)
    {
        if (!Definitions.TryGetValue(facilityKey, out FacilityGreetingDefinition definition))
        {
            throw new ArgumentException("Unknown facility key.", nameof(facilityKey));
        }
        string resolvedTownName = string.IsNullOrEmpty(townName) ? "この町" : townName;
        int index = GetDialogueIndex(day, townIndex, facilityKey, definition.Dialogues.Length);
        return new FacilityGreeting(
            string.Format(definition.TitleFormat, resolvedTownName),
            definition.Dialogues[index]);
    }

    public static IReadOnlyCollection<string> FacilityKeys => Definitions.Keys;

    private static int GetDialogueIndex(int day, int townIndex, string facilityKey, int count)
    {
        if (count <= 1)
        {
            return 0;
        }

        unchecked
        {
            int hash = 17;
            hash = hash * 31 + townIndex;
            foreach (char character in facilityKey)
            {
                hash = hash * 31 + character;
            }
            int baseIndex = (hash & 0x7fffffff) % count;
            int dayOffset = ((day % count) + count) % count;
            return (baseIndex + dayOffset) % count;
        }
    }

    private static string BuildVisitKey(int day, int townIndex, string facilityKey)
    {
        return day + ":" + townIndex + ":" + facilityKey;
    }

    private sealed class FacilityGreetingDefinition
    {
        public FacilityGreetingDefinition(string titleFormat, string[] dialogues)
        {
            TitleFormat = titleFormat;
            Dialogues = dialogues;
        }

        public string TitleFormat { get; }
        public string[] Dialogues { get; }
    }
}

public readonly struct FacilityGreeting
{
    public FacilityGreeting(string title, string dialogue)
    {
        Title = title;
        Dialogue = dialogue;
    }

    public string Title { get; }
    public string Dialogue { get; }
}
