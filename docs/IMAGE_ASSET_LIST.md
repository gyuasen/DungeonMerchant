# DungeonMerchant 画像アセット一覧

調査日: 2026-07-19。`Resources.Load<Sprite>` / `Resources.Load<Texture2D>` の呼び出し、EnemyDataSO 93体、DungeonDataSO、街道生成キー、イベント定義を照合した。既存の `Resources/UI/ParchmentPanel.png`、地図5枚、`MercenaryPortraitSheet.png`、`MercenarySpecialPortraitSheet.png` は配置済みとして本表から除外する。

## 共通スタイル指針

`docs/WORLDVIEW.md` 準拠。羊皮紙の繊維感、煤けた縁、インク線と控えめな水彩塗りを基調にする。魔大陸の結界都市・竜・不死・獣・魔の気配を入れ、写実寄りのダークファンタジーだがゲームUIで判別しやすい明快なシルエットにする。文字、ロゴ、UI枠、透かしは描かない。敵は正面寄りの全身または胸像で、余白を残し、透過PNGで書き出す。

表中の ★ は必須、☆ は任意（代替表示でも進行可能）。「色替え可」は同じ元絵に色・発光・装備差分を施してよい、という意味であり、別ポーズの新規作画までは要求しない。

## 戦闘敵（EnemyDataSO 93体）

すべて `Resources/Battle/Enemies/{ファイル名}.png`。通常は 512x512、ボス・神話級は 768x768、透過あり。以下の一覧は93体を個別に列挙している。空欄だった `battleVisualKey` はデータアセット名を使用する実装である。

| 用途 | ファイル名 | 配置パス | 推奨サイズ | 透過 | 内容の指示 |
|---|---|---|---|---|---|
|★通常敵|Grade09Goblin / Grade09GoblinScout / Grade09GoblinSpearman / enemy_job_goblin_assassin / enemy_job_goblin_magician / enemy_job_goblin_knight / enemy_job_goblin_raider / enemy_job_goblin_veteran|`Resources/Battle/Enemies/{各名}.png`|512x512|あり|痩せたゴブリンの元種を共通化し、粗末な武器・暗殺装束・魔術具・騎士鎧で差分化する。肌色と布色の変更で派生を賄える。等級9・人型。|
|★通常敵|Grade09Kobold / enemy_job_kobold_hexer / enemy_job_kobold_prowler / enemy_job_kobold_bulwark / enemy_job_kobold_ravager / enemy_job_kobold_packleader|`Resources/Battle/Enemies/{各名}.png`|512x512|あり|獣じみたコボルドを、呪術師・追跡者・盾役・猛撃役・群れ長の装備差分で描く。毛色の色替えで派生を賄える。等級9・獣。|
|★通常敵|Grade10BlueSlime / Grade10GreenSlime / Grade10MossSlime / enemy_slime_slime_acid / enemy_slime_slime_venom / enemy_slime_slime_stone / enemy_slime_slime_quicksilver / enemy_slime_slime_verdant / enemy_slime_slime_thunder / enemy_slime_slime_frost_crystal / enemy_slime_slime_magma / enemy_slime_slime_astral|`Resources/Battle/Enemies/{各名}.png`|512x512|あり|半透明の液体スライムを同じ丸い元種で描き、青・緑・苔・酸・毒・石・水銀・樹木・雷・氷晶・溶岩・星空へ色と内部エフェクトを変える。全等級の色替え・質感差分で賄える。等級10〜1・スライム。|
|★通常敵|Grade08GiantRat / Grade08CaveSpider / Grade08RockBeetle / Grade08CaveBat / Grade08VenomMoth / Grade09WildDog / Grade10HornRabbit|`Resources/Battle/Enemies/{各名}.png`|512x512|あり|地下道の鼠・蜘蛛・岩甲虫・蝙蝠・毒蛾と、野犬・角兎を不気味な自然光で描く。各種別は新規絵、同種内の色替えは可。等級10〜8・獣。|
|★通常敵|Grade07Skeleton / Grade07ArmoredSkeleton / Grade07BoneHound / Grade07Wraith / Grade07Zombie / enemy_job_skeleton_archer / enemy_job_skeleton_hexer / enemy_job_skeleton_guard / enemy_job_skeleton_reaper / enemy_job_skeleton_captain|`Resources/Battle/Enemies/{各名}.png`|512x512|あり|朽ちた骨、不死火、錆びた武具を共通モチーフに、弓・呪具・盾・大鎌・隊長装備を差分化する。骨色と霊光の色替えで派生を賄える。等級7・不死。|
|★通常敵|Grade06Orc / Grade06Hobgoblin / Grade06Troll / enemy_job_orc_shaman / enemy_job_orc_rider / enemy_job_orc_bulwark / enemy_job_orc_berserker / enemy_job_orc_veteran|`Resources/Battle/Enemies/{各名}.png`|512x512|あり|黒鉄鉱山周辺の粗暴なオーク系を、呪術・騎乗・大盾・狂戦士・古参兵の装備差分で描く。肌色と鎧色の変更で派生を賄える。等級6〜5・人型。|
|★通常敵|Grade06Lizardman / Grade06MarshLizard / enemy_job_lizardman_shaman / enemy_job_lizardman_stalker / enemy_job_lizardman_scaleguard / enemy_job_lizardman_ravager / enemy_job_lizardman_captain|`Resources/Battle/Enemies/{各名}.png`|512x512|あり|沼と古代遺跡の鱗人・湿地蜥蜴を、呪術・潜伏・鱗盾・猛撃・隊長装備で描く。鱗色の色替えで派生を賄える。等級6・竜。|
|★通常敵|Grade03Wyvern / enemy_job_wyvern_hexer / enemy_job_wyvern_skyrider / enemy_job_wyvern_ironwing / enemy_job_wyvern_ravager / enemy_job_wyvern_captain|`Resources/Battle/Enemies/{各名}.png`|512x512|あり|天空要塞の翼竜を同じ骨格で描き、呪翼・疾風翼・鉄翼・猛爪・群れ長の装備や翼膜差分を付ける。鱗色と翼膜の色替えで派生を賄える。等級3・竜。|
|★通常敵|Grade05IronGolem / Grade05StoneGolem / Grade04DarkMage / Grade05OgreMage / Grade02DemonKnight|`Resources/Battle/Enemies/{各名}.png`|512x512|あり|黒鉄・石・魔導の質感を持つ構築体と魔族を描き、鉱山の炉光と封印文様を添える。ゴーレムの鉱石色や魔術光の色替えは可。等級5〜2・構築体/人型/魔。|
|★上位・ボス|Grade01AbyssDragon / Grade01AstralOracle / Grade01AstralReaver / Grade01AstralSentinel / Grade02DemonKnight|`Resources/Battle/Enemies/{各名}.png`|768x768|あり|星環深層の竜・神託者・略奪者・星界 sentinel を神秘的な天体光と結界文様で描く。役割別の武器差分は可だが、竜と人型は別シルエットにする。等級2〜1・竜/魔/構築体。|
|★ボス|Boss01AbyssLord / Boss01CelestialJudge / Boss02BlackIronGeneral / Boss04RuinGuardian / Boss06MineTyrant / Boss07CaveOgre|`Resources/Battle/Enemies/{各名}.png`|768x768|あり|各地域の支配者として、深淵の王・天上の審判者・黒鉄将軍・遺跡守護者・鉱山暴君・洞窟鬼を威厳ある全身像で描く。ボスは色替えだけで済ませず個別の装備・背景光を持たせる。|
|★地域・街道敵|Boss02GlaadStormCastellan / Grade03GlaadFrostDrake / Grade03GlaadSkyWarden / Grade04GlaadGaleHarpy / VelmBlackIronDelver / VelmDeepforgeHexer / VelmDeepforgeOverlord / VelmEmberforgedAutomaton / VelmMagmaDrake|`Resources/Battle/Enemies/{各名}.png`|通常512x512、Boss768x768|あり|グラード天空要塞の吹雪・風翼・氷竜と、ヴェルム鉱山の黒鉄・炉心・溶岩竜を地域色で描く。竜・魔導士・構築体は個別絵、同系列の色替えは可。|
|★街道レア|MythicalGrade01AstralDragon / MythicalGrade03FlamewingGryphon / MythicalGrade05ThunderhornKirin / MythicalGrade07MistfangWolf|`Resources/Battle/Enemies/{各名}.png`|768x768|あり|街道に一瞬現れる神話級の竜・炎翼 gryphon・雷角麒麟・霧牙狼を、羊皮紙図鑑にも映える強い輪郭と希少な発光で描く。各個体は新規描き起こし。|
|★予備|EnemyData|`Resources/Battle/Enemies/EnemyData.png`|512x512|あり|戦闘データが見つからない場合の青緑スライム代替絵。既存スライム元種の色替えで賄える。|

## 戦闘背景（ダンジョン・街道）

すべて `Resources/Battle/Backgrounds/{ファイル名}.png`、1920x1080、透過なし。ダンジョンの `battleBackgroundKey` は現在空欄のため、各ファイル名を設定する前提である。

| 用途 | ファイル名 | 配置パス | 推奨サイズ | 透過 | 内容の指示 |
|---|---|---|---|---|---|
|★|Dungeon_OriginCave|`Resources/Battle/Backgrounds/Dungeon_OriginCave.png`|1920x1080|なし|結界都市近郊の始まりの洞窟、湿った岩、松明、羊皮紙色の霧を描く。|
|★|Dungeon_SealedMine|`Resources/Battle/Backgrounds/Dungeon_SealedMine.png`|1920x1080|なし|封じられた廃坑、崩れた坑道と黒鉄の鉱脈、古い封印杭を描く。|
|★|Dungeon_MistRuins|`Resources/Battle/Backgrounds/Dungeon_MistRuins.png`|1920x1080|なし|霧の古代遺跡、苔むした石柱と不死の青白い火、遠い結界光を描く。|
|★|Dungeon_AstralDepths|`Resources/Battle/Backgrounds/Dungeon_AstralDepths.png`|1920x1080|なし|星環深層、宙に浮く遺跡と星雲、深淵へ落ちる魔大陸の裂け目を描く。|
|★|Road_0_1 / Road_1_2 / Road_0_3 / Road_3_4 / Road_4_5 / Road_5_6 / Road_6_7|`Resources/Battle/Backgrounds/Road_{下位町}_{上位町}.png`|1920x1080|なし|町を結ぶ街道の地域背景。東方平原・森林・沼・天空要塞・黒鉄鉱山・深淵都市・星環島を各地域の植生、街道標、結界の光で描き分ける。|
|★共通|Default|`Resources/Battle/Backgrounds/Default.png`|1920x1080|なし|羊皮紙色の暗い戦場、遠景の魔大陸と薄い結界光を置いた汎用背景。|

## 戦闘イベント

| 用途 | ファイル名 | 配置パス | 推奨サイズ | 透過 | 内容の指示 |
|---|---|---|---|---|---|
|☆選択肢|AbandonedCamp_Rest / AbandonedCamp_QuickRest / TreasureCache_Careful / TreasureCache_Force / CollapsedPassage_Detour / CollapsedPassage_Force / MineralVein_Careful / MineralVein_Quick / HerbGrove_Careful / HerbGrove_Quick / QualityGrove_Careful / QualityGrove_Quick / Retreat|`Resources/Battle/Events/{各名}.png`|512x512|あり|野営、隠し宝箱、崩落坑道、鉱脈、薬草、良質な木材、撤退の選択肢を、インク線と小さな魔法の光で一目で分かるアイコン風絵にする。各ペアは慎重策と強行策の差が伝わる構図にする。|

## タイトル

| 用途 | ファイル名 | 配置パス | 推奨サイズ | 透過 | 内容の指示 |
|---|---|---|---|---|---|
|★|TitleBackground|`Resources/UI/TitleBackground.png`|1920x1080|なし|羊皮紙の地図を下地に、結界都市の城壁、遠い竜影、魔大陸の裂け目と商人の荷車を描く。タイトル文字を置く中央は暗く静かな余白にする。|

## 施設職員

| 用途 | ファイル名 | 配置パス | 推奨サイズ | 透過 | 内容の指示 |
|---|---|---|---|---|---|
|★|Tavern / Guild / Market / Blacksmith / Warehouse / Clinic / Temple|`Resources/UI/Staff/{各名}.png`|512x512|あり|酒場の女将、商会組合受付、市場の元締め、鍛冶師、倉庫番、治療師、神官を胸像で描く。職能道具と羊皮紙・結界都市の意匠を持たせ、同じ画風で個性を分ける。|

## 図鑑装備

| 用途 | ファイル名 | 配置パス | 推奨サイズ | 透過 | 内容の指示 |
|---|---|---|---|---|---|
|★装備89種|AbyssMantle, AbyssSeal, AncientGuardianArmor, AncientGuardianSeal, ApprenticeRobe, ArcanePendant, AstralAegis, AstralCore, BatEyeCharm, BlackIronWarEmblem, BonePrayerVestment, ChampionEmblem, DeepMinerArmor, EchoStoneRing, FeatherCharm, GeneralPlate, GlaadSummitSigil, GlaadWardenPlate, GoblinFangTalisman, GolemPlate, GuardianEyeCharm, HawkeyeCharm, IronArmor, IronVanguardArmor, LancerChainmail, LancerInsignia, LeatherArmor, ManaPendant, NornBarkguard, NornVerdantCharm, OniHunterGarb, PriestRosary, PriestVestment, RogueLeatherArmor, RogueTalisman, RuinweaveMantle, RunewovenRobe, ShadowhideArmor, SoldierRing, SpiritBead, VelmDeepforgeArmor, VelmEmberCore, WindrunnerLeather, WyvernCrest, NormalRank01Accessory, NormalRank01Armor, NormalRank02, NormalRank02Accessory, NormalRank03, NormalRank03Armor, NormalRank04Accessory, NormalRank04Armor, NormalRank05, NormalRank05Accessory, NormalRank06, NormalRank06Armor, NormalRank07Accessory, NormalRank07Armor, NormalRank08, NormalRank08Accessory, NormalRank09, NormalRank09Armor, NormalRank10Accessory, NormalRank10Armor, AstralAegis, AstralCore, item_expansion_rank4_0_accessory〜item_expansion_rank7_2_accessory/armor, item_expansion_undeadbane|`Resources/UI/Codex/Equipment/{item.name}.png`|256x256|あり|羊皮紙の図鑑に貼る単品装備アイコン。正面・斜め45度で輪郭を明確にし、武器、鎧、護符、鉱石・竜鱗などの素材感と等級色を表現する。|

※ `item_expansion_rank4_0`〜`rank7_2` は各ランク・職業の武器、鎧、アクセサリーを個別ファイルにする（計27枠）。既存のゲームデータ名をファイル名にそのまま使用する。

## 掲示板

| 用途 | ファイル名 | 配置パス | 推奨サイズ | 透過 | 内容の指示 |
|---|---|---|---|---|---|
|★|QuestBoard|`Resources/UI/QuestBoard.png`|1024x1024|あり|結界都市の木製掲示板に、羊皮紙の依頼書、封蝋、簡素な魔法印を束ねた正面絵を描く。中央はクエスト文字を重ねられる余白にする。|

## 傭兵ポートレート

配置済みのため新規作成対象外。`Resources/UI/MercenaryPortraitSheet.png` と `Resources/UI/MercenarySpecialPortraitSheet.png` を `MercenaryPortraitProvider` が読み込み、シート内の矩形を切り出して使用する。将来差し替える場合は通常シート・特殊シートとも既存のスプライト割り当てを維持する。

## 集計

- 総枚数（個別ファイル単位、配置済み画像を除く）: **216枚**（敵93 + 背景12 + イベント13 + タイトル1 + 職員7 + 装備89 + 掲示板1）。装備欄の `item_expansion_rank4_0〜rank7_2` は27ファイルとして計上済み。
- 優先度別: **★必須 203枚、☆任意 13枚**（イベント13枚）。
- 敵の色替え・装備差分で賄える枚数: **約52枚**（ゴブリン5、コボルド5、スライム12、スケルトン系9、オーク系5、リザードマン系4、ワイバーン系4、その他8）。
- 新規描き起こし目安: **約42枚**（敵の基礎種別・ボス・神話級、背景、UI人物・装備・掲示板）。
- 監査上の注意: 敵93体は `GameData/Enemies` 80体と `Resources/Enemies` 13体の合計。`battleVisualKey` が空の敵も `Battle/Enemies/{アセット名}` を要求するため、一覧から省略していない。
