# DungeonMerchant 画像アセット一覧

更新日: **2026-07-21**

## サマリー

実ファイル（`.png`/`.jpg`本体）を `Assets` 配下で全走査し、必要アセットと照合した。`.meta` は枚数に含めていない。

| セクション | 必要 | 配置済み | 未配置 |
|---|---:|---:|---:|
| 敵スプライト（EnemyDataSO 99体） | 99 | 93 | 6 |
| 戦闘背景 | 12 | 12 | 0 |
| 戦闘イベント | 13 | 13 | 0 |
| タイトル | 1 | 1 | 0 |
| 施設職員 | 7 | 7 | 0 |
| 図鑑・装備 | 89 | 89 | 0 |
| 掲示板 | 1 | 1 | 0 |
| ポートレート | 0 | 0 | 0 |
| **合計（必要画像）** | **222** | **216** | **6** |

ポートレートシート2枚（`MercenaryPortraitSheet.png`、`MercenarySpecialPortraitSheet.png`）は既存配置済みだが、シート内切り出しを使用するため必要画像数から除外した。`ParchmentPanel.png`、`Resources/UI/Equipment/` の2枚、マップ画像11枚、`Assets/Proiject/UI/` の1枚も同様に参照・基盤アセットとして別管理し、上表の必要枚数には含めていない。

## 戦闘敵スプライト（EnemyDataSO 99体）

基準は `Assets/Proiject/Resources/**/EnemyDataSO` の `battleVisualKey`。配置済み93体は `Resources/Battle/Enemies/*.png` の実ファイル93枚と一致した。拡張敵については、実装で定義されたキーと既存ファイル名の対応（`GradeNN_<job>` → `enemy_job_<job>`、`GradeNN_slime_<variant>` → `enemy_slime_slime_<variant>`）を適用して照合した。

| 状態 | 対象（battleVisualKey／実ファイル名） | 配置先 |
|---|---|---|
| ✅配置済み（93） | `Boss01AbyssLord`, `Boss01CelestialJudge`, `Boss02BlackIronGeneral`, `Boss02GlaadStormCastellan`, `Boss04RuinGuardian`, `Boss06MineTyrant`, `Boss07CaveOgre`, `EnemyData`, `Grade01AbyssDragon`, `Grade01AstralOracle`, `Grade01AstralReaver`, `Grade01AstralSentinel`, `Grade02DemonKnight`, `Grade03GlaadFrostDrake`, `Grade03GlaadSkyWarden`, `Grade04DarkMage`, `Grade04GlaadGaleHarpy`, `Grade05IronGolem`, `Grade05OgreMage`, `Grade05StoneGolem`, `Grade06Hobgoblin`, `Grade06Lizardman`, `Grade06MarshLizard`, `Grade06Orc`, `Grade06Troll`, `Grade07ArmoredSkeleton`, `Grade07BoneHound`, `Grade07Skeleton`, `Grade07Wraith`, `Grade07Zombie`, `Grade08CaveBat`, `Grade08CaveSpider`, `Grade08GiantRat`, `Grade08RockBeetle`, `Grade08VenomMoth`, `Grade09Goblin`, `Grade09GoblinScout`, `Grade09GoblinSpearman`, `Grade09Kobold`, `Grade09WildDog`, `Grade10BlueSlime`, `Grade10GreenSlime`, `Grade10HornRabbit`, `Grade10MossSlime`, `MythicalGrade01AstralDragon`, `MythicalGrade03FlamewingGryphon`, `MythicalGrade05ThunderhornKirin`, `MythicalGrade07MistfangWolf`, `VelmBlackIronDelver`, `VelmDeepforgeHexer`, `VelmDeepforgeOverlord`, `VelmEmberforgedAutomaton`, `VelmMagmaDrake`, `Grade03_wyvern`/`Grade03Wyvern`, `Grade03_wyvern_captain`/`enemy_job_wyvern_captain`, `Grade03_wyvern_hexer`/`enemy_job_wyvern_hexer`, `Grade03_wyvern_ironwing`/`enemy_job_wyvern_ironwing`, `Grade03_wyvern_ravager`/`enemy_job_wyvern_ravager`, `Grade03_wyvern_skyrider`/`enemy_job_wyvern_skyrider`, `Grade05_orc_berserker`/`enemy_job_orc_berserker`, `Grade05_orc_bulwark`/`enemy_job_orc_bulwark`, `Grade05_orc_rider`/`enemy_job_orc_rider`, `Grade05_orc_shaman`/`enemy_job_orc_shaman`, `Grade05_orc_veteran`/`enemy_job_orc_veteran`, `Grade07_skeleton_archer`/`enemy_job_skeleton_archer`, `Grade07_skeleton_captain`/`enemy_job_skeleton_captain`, `Grade07_skeleton_guard`/`enemy_job_skeleton_guard`, `Grade07_skeleton_hexer`/`enemy_job_skeleton_hexer`, `Grade07_skeleton_reaper`/`enemy_job_skeleton_reaper`, `Grade09_goblin_assassin`/`enemy_job_goblin_assassin`, `Grade09_goblin_knight`/`enemy_job_goblin_knight`, `Grade09_goblin_magician`/`enemy_job_goblin_magician`, `Grade09_goblin_raider`/`enemy_job_goblin_raider`, `Grade09_goblin_veteran`/`enemy_job_goblin_veteran`, `Grade09_kobold_bulwark`/`enemy_job_kobold_bulwark`, `Grade09_kobold_hexer`/`enemy_job_kobold_hexer`, `Grade09_kobold_packleader`/`enemy_job_kobold_packleader`, `Grade09_kobold_prowler`/`enemy_job_kobold_prowler`, `Grade09_kobold_ravager`/`enemy_job_kobold_ravager`, `Grade01_slime_astral`/`enemy_slime_slime_astral`, `Grade02_slime_magma`/`enemy_slime_slime_magma`, `Grade03_slime_frost_crystal`/`enemy_slime_slime_frost_crystal`, `Grade04_slime_thunder`/`enemy_slime_slime_thunder`, `Grade05_slime_verdant`/`enemy_slime_slime_verdant`, `Grade06_slime_quicksilver`/`enemy_slime_slime_quicksilver`, `Grade07_slime_stone`/`enemy_slime_slime_stone`, `Grade08_slime_venom`/`enemy_slime_slime_venom`, `Grade09_slime_acid`/`enemy_slime_slime_acid` | `Resources/Battle/Enemies/` |
| ⬜未配置（6） | `Grade04_abyss_spawn` → `Grade04_abyss_spawn.png`; `Grade04_norn_verdant_orc_high_chieftain` → `Grade04_norn_verdant_orc_high_chieftain.png`; `Grade04_dragonscale_king` → `Grade04_dragonscale_king.png`; `Grade03_grand_furnace_colossus` → `Grade03_grand_furnace_colossus.png`; `Grade03_abyss_gatekeeper` → `Grade03_abyss_gatekeeper.png`; `Grade06_eld_quarry_gravelord` → `Grade06_eld_quarry_gravelord.png` | `Resources/Battle/Enemies/` |

## 戦闘背景

| 状態 | ファイル名 | 配置先 |
|---|---|---|
| ✅配置済み（12） | `Default`, `Dungeon_AstralDepths`, `Dungeon_MistRuins`, `Dungeon_OriginCave`, `Dungeon_SealedMine`, `Road_0_1`, `Road_0_3`, `Road_1_2`, `Road_3_4`, `Road_4_5`, `Road_5_6`, `Road_6_7`（各 `.png`） | `Resources/Battle/Backgrounds/` |

## 戦闘イベント

| 状態 | ファイル名 | 配置先 |
|---|---|---|
| ✅配置済み（13） | `AbandonedCamp_QuickRest`, `AbandonedCamp_Rest`, `CollapsedPassage_Detour`, `CollapsedPassage_Force`, `HerbGrove_Careful`, `HerbGrove_Quick`, `MineralVein_Careful`, `MineralVein_Quick`, `QualityGrove_Careful`, `QualityGrove_Quick`, `Retreat`, `TreasureCache_Careful`, `TreasureCache_Force`（各 `.png`） | `Resources/Battle/Events/` |

## タイトル・施設職員・掲示板

| セクション | 状態 | ファイル名 | 配置先 |
|---|---|---|---|
| タイトル | ✅配置済み（1） | `TitleBackground.png` | `Resources/UI/` |
| 職員 | ✅配置済み（7） | `Blacksmith.png`, `Clinic.png`, `Guild.png`, `Market.png`, `Tavern.png`, `Temple.png`, `Warehouse.png` | `Resources/UI/Staff/` |
| 掲示板 | ✅配置済み（1） | `QuestBoard.png` | `Resources/UI/` |

## 図鑑・装備

| 状態 | 枚数 | 配置先 | 照合結果 |
|---|---:|---|---|
| ✅配置済み | 89 | `Resources/UI/Codex/Equipment/{item.name}.png` | `AbyssMantle`, `AbyssSeal`, `AncientGuardianArmor`, `AncientGuardianSeal`, `ApprenticeRobe`, `ArcanePendant`, `AstralAegis`, `AstralCore`, `BatEyeCharm`, `BlackIronWarEmblem`, `BonePrayerVestment`, `ChampionEmblem`, `DeepMinerArmor`, `EchoStoneRing`, `FeatherCharm`, `GeneralPlate`, `GlaadSummitSigil`, `GlaadWardenPlate`, `GoblinFangTalisman`, `GolemPlate`, `GuardianEyeCharm`, `HawkeyeCharm`, `IronArmor`, `IronVanguardArmor`, `item_expansion_rank4_0_accessory`, `item_expansion_rank4_0_armor`, `item_expansion_rank4_1_accessory`, `item_expansion_rank4_1_armor`, `item_expansion_rank4_2_accessory`, `item_expansion_rank4_2_armor`, `item_expansion_rank5_0_accessory`, `item_expansion_rank5_0_armor`, `item_expansion_rank5_1_accessory`, `item_expansion_rank5_1_armor`, `item_expansion_rank5_2_accessory`, `item_expansion_rank5_2_armor`, `item_expansion_rank6_0_accessory`, `item_expansion_rank6_0_armor`, `item_expansion_rank6_1_accessory`, `item_expansion_rank6_1_armor`, `item_expansion_rank6_2_accessory`, `item_expansion_rank7_0_accessory`, `item_expansion_rank7_0_armor`, `item_expansion_rank7_1_accessory`, `item_expansion_rank7_1_armor`, `item_expansion_rank7_2_accessory`, `item_expansion_rank7_2_armor`, `item_expansion_undeadbane`, `LancerChainmail`, `LancerInsignia`, `LeatherArmor`, `ManaPendant`, `NormalRank01Accessory`, `NormalRank01Armor`, `NormalRank02`, `NormalRank02Accessory`, `NormalRank03`, `NormalRank03Armor`, `NormalRank04Accessory`, `NormalRank04Armor`, `NormalRank05`, `NormalRank05Accessory`, `NormalRank06`, `NormalRank06Armor`, `NormalRank07Accessory`, `NormalRank07Armor`, `NormalRank08`, `NormalRank08Accessory`, `NormalRank09`, `NormalRank09Armor`, `NormalRank10Accessory`, `NormalRank10Armor`, `NornBarkguard`, `NornVerdantCharm`, `OniHunterGarb`, `PriestRosary`, `PriestVestment`, `RogueLeatherArmor`, `RogueTalisman`, `RuinweaveMantle`, `RunewovenRobe`, `ShadowhideArmor`, `SoldierRing`, `SpiritBead`, `VelmDeepforgeArmor`, `VelmEmberCore`, `WindrunnerLeather`, `WyvernCrest`（各 `.png`） |

## 次に用意すべき優先画像

未配置の必須画像は6枚。優先度順は以下のとおり。

段階1のダンジョン限定装備図鑑画像（未配置、`Resources/UI/Codex/Equipment/{アセット名}.png`）:

- `NornVerdantChieftainHatchet`, `NornAncientBarkBattlegear`, `NornVerdantOathNecklace`
- `GlaadReversedScaleLance`, `GlaadCanyonScaleArmor`, `GlaadWingchaserDragonMark`
- `VelmGreatFurnaceSiegeHammer`, `VelmFurnaceguardHeatArmor`, `VelmEverflameTuningCore`
- `AbyssGatewaySealingBlade`, `AbyssGatewardenBlackGarb`, `AbyssChainSealRing`

段階2のダンジョン限定装備図鑑画像（未配置、`Resources/UI/Codex/Equipment/{アセット名}.png`）:

- `StartingCavePioneerMossBlade`, `StartingCaveLampTravelWear`, `StartingCaveOriginDropletCharm`
- `LeafPackripperFangSword`, `LeafHidingHuntWear`, `LeafHowlSilencingBell`
- `EldWatergateWardenHatchet`, `EldStagnantWaterCloak`, `EldBlueRustWaterwayKey`

段階3のダンジョン限定装備図鑑画像（未配置、`Resources/UI/Codex/Equipment/{アセット名}.png`）:

- `LowerMineTunnelbreakerHammer`, `LowerMineClosedMinerHeavyWear`, `LowerMineLampeaterMinerLamp`
- `EldSoulmasonIronHammer`, `EldTombstoneWardenBreastplate`, `EldCorpseKingSealingStoneRing`

1. ★ `Grade04_norn_verdant_orc_high_chieftain.png`
2. ★ `Grade04_dragonscale_king.png`
3. ★ `Grade03_grand_furnace_colossus.png`
4. ★ `Grade03_abyss_gatekeeper.png`
5. ★ `Grade06_eld_quarry_gravelord.png`
6. ★ `Grade04_abyss_spawn.png`

装備図鑑の特殊ページ（セット装備／Rank9以上の単独装備を一画面表示する `EquipmentSpecialCodexPageUI`）は、テキスト中心のレイアウトで画像を一切ロードしないため、この機能のための新規画像は不要（設計上も通常画像フォールバック方針で確定）。上記の段階1〜3ダンジョン限定装備図鑑画像（未配置）は従来どおり通常の装備図鑑側で使用するものであり、特殊ページの追加要件ではない。

【重要・実機で判明（2026-07-22）】通常タブの装備図鑑（`BookPageUI`）は `Resources/UI/Codex/Equipment/{item.name}.png` を発見済み装備の画像として表示するが、上記の段階1〜3ダンジョン限定装備（専用装備の武器/防具/装飾）の図鑑画像が未配置のため、発見済みでも画像枠が「?」プレースホルダになる。これは伏せ字（未発見）ではなく画像欠落。コード上の不具合ではなく、上記未配置画像を `item.name` と完全一致（大文字小文字含む）するファイル名で Sprite としてインポート配置すれば解消する。武器のアセット名（`...Hatchet` / `...Lance` / `...Hammer` / `...Blade` / `...Sword` / `...Fang` 等）も対象に含まれる。

主要ダンジョン背景12枚、タイトル背景、施設職員7枚、掲示板、イベント13枚、図鑑89枚はすべて配置済み。

## 監査対象外として確認した既存画像

必要画像の集計外だが、次の実ファイルも存在する。

- `Art/Maps/`: `ContinentMap.png`, `TownMap.png`, `TownMarker.png`, `TownMarkerChroma.png`
- `Resources/Maps/`: `BlackSoilRegionMap.png`, `EasternRegionMap.png`, `Map.png`, `NorthwestRegionMap.png`, `TownMap.png`, `TownMarker.png`, `WorldMap.png`
- `Resources/UI/`: `MercenaryPortraitSheet.png`, `MercenarySpecialPortraitSheet.png`, `ParchmentPanel.png`
- `Resources/UI/Equipment/`: `AbyssSeal.png`, `AstralAegis.png`
- `Assets/Proiject/UI/`: UUID名のPNG 1枚

全 `Assets` 配下の実画像は233枚（必要画像の配置済み216枚＋集計外17枚）として確認した。必要画像の未配置6枚は実画像枚数には含めていない。

## 2026-07-23 画像参照ずれの調査と修正

ユーザー報告「画像は保存されているのに参照が間違っているものが多々ある」を受けて全カテゴリを横断調査した。**画像は存在するのに参照名が食い違っていたものが 44 組**あり、いずれも参照側を実画像名へ合わせる形で修正済み。

### 修正済み（参照ずれ 44 組）

- **敵の `battleVisualKey` 40 件**（`Resources/GameData/Enemies/Expansion/` 配下）
  - `GradeXX_{goblin,kobold,lizardman,orc,skeleton,wyvern}_{suffix}` → `enemy_job_{同種}_{同suffix}`（各5件・計30件）
  - `GradeXX_slime_{suffix}` → `enemy_slime_slime_{同suffix}`（9件）
  - `Grade03_wyvern` → `Grade03Wyvern`（1件）
- **ダンジョン背景キー 4 件**
  - `DungeonData` → `Dungeon_OriginCave`（はじまりの洞窟）
  - `LowerMine` → `Dungeon_SealedMine`（封じられた廃坑）
  - `MiddleRuins` → `Dungeon_MistRuins`（霧の古代遺跡）
  - `AstralDepths` → `Dungeon_AstralDepths`

### 修正済み（コード側のフォールバック不足）

敵SOのうち **53 体は `battleVisualKey` が空欄**で、通常戦闘はアセット名で解決していたが**魔物図鑑には同じフォールバックが無く画像が出ていなかった**（99体中6体しか表示されていない状態）。共通の `EnemySpriteResolver` を新設し、図鑑と戦闘が同じ解決順（直接Sprite → キー → `Battle/Enemies/`+キー → アセット名 → 特殊個体は元敵へフォールバック）を使うよう統一した。データは変更していない。

回帰テスト（`GameAssetRepositoryTests`）で、**全敵99体の画像解決**と**背景キーの解決**（未配置7件を除く）を検証している。

### 未配置（画像そのものが存在しない。参照ずれではない）

**ダンジョン背景 7 枚**（`Resources/Battle/Backgrounds/{キー}.png`）:
`EldOldQuarry`, `EldUndergroundWaterway`, `FinalBlackSoilAbyss`, `GlaadSkyFortress`, `LeafForestTrail`, `NornCanopyLabyrinth`, `VelmBlackIronMine`

**施設職員 1 枚**（`Resources/UI/Staff/TrainingGround.png`）:
新施設「修練場」の職員画像。他7施設（酒場/ギルド/市場/鍛冶屋/倉庫/診療所/神殿）は配置済み。

**非装備アイテム 57 点**（`Resources/UI/Items/{item.name}.png`。ディレクトリ自体が未作成）:
素材42点・遺物6点・消耗品9点。`ItemPresentationService` は `UI/Codex/Equipment/` → `UI/Items/` の順で解決するため、未配置の間はプレースホルダ表示になる。

**装備武器・装飾 15 点**（`Resources/UI/Codex/Equipment/`）:
`item_expansion_beastbane`, `item_expansion_dragonbane`, `item_expansion_rank4_0`〜`rank7_2`（12点）, `MutantCoreCharm`

### 補足

`Resources/UI/Equipment/` の `AbyssSeal.png` / `AstralAegis.png` は、同名画像が `UI/Codex/Equipment/` から先に解決されるため現行コードでは到達しない重複ファイル。動作上の害はない。

## 町ごとの個別画像（新規要望・未着手）

現在、町マップ画面は全町で共通の `Resources/Maps/TownMap.png` を使い回している。これを**町ごとに別画像**へ差し替える方針。8町ぶんの新規画像が必要。

配置先（想定）: `Resources/Maps/Towns/{英名}.png`、または既存の命名に合わせて `Resources/Maps/TownMap_{英名}.png`。
※ コード側は現在 `TownMap` 固定で解決しているため、町インデックスに応じた解決処理の追加が必要（画像配置とセットで実装する）。フォールバックとして既存 `TownMap.png` を残すのが安全。

| # | 町名 | 進行順 | 想定ファイル名 | 世界観・画のイメージ |
|---:|---|---:|---|---|
| 2 | セイル港湾都市 | 1番目（開始町） | `Sail` | 人類大陸からの船が着く魔大陸の玄関口。港、桟橋、積み荷、船。商人・傭兵・職人が行き交う雑多な活気 |
| 1 | リーフ森林都市 | 2番目 | `Leaf` | 森林に囲まれた結界都市。薬草・薬品を扱う工房、緑と木々、薬師や錬金術師の集まる静かな街並み |
| 0 | エルド交易都市 | 3番目 | `Eld` | 複数の街道が交わる交易拠点。市場・倉庫・商人組合が発達。主人公の両親の商会があった街 |
| 3 | ノルン樹冠都市 | 4番目 | `Norn` | 巨木の樹冠に築かれた都市。高所の吊り橋や木造建築 |
| 4 | グラード山塞都市 | 5番目 | `Glaad` | 山岳の要塞都市。岩壁、砦、竜が飛ぶ峡谷を望む |
| 5 | ヴェルム黒鉄都市 | 6番目 | `Velm` | 黒鉄と熔炉の工業都市。煙突、溶鉱炉、鉱山の坑口 |
| 6 | アビス辺境都市 | 7番目（最終） | `Abyss` | 黒土地域の最果て。魔力で黒く変色した大地、深淵への境門を望む重苦しい空気 |
| 7 | アステラ秘匿都市 | 隠し町 | `Astera` | 魔大陸中央の隠された島。地図に載らない神秘的な都市。魔力が集中する伝説の地 |

共通の要件:
- 結界都市であることが分かる描写（町を守る結界の描写があると世界観が伝わる）。
- 画面内に施設（市場・酒場・鍛冶屋・治療院・倉庫・転職神殿・修練場など）のボタンを重ねて配置するため、**中央〜下部は比較的シンプル**にし、UIと干渉しない構図が望ましい。
- 既存 `TownMap.png` と同じ解像度・縦横比に揃える。
