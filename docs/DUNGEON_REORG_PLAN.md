# ダンジョン再編 設計正本

Terra 段階実装の参照用。Sol 監査およびユーザー承認済みの確定設計を記録する。ここにない実装判断は行わず、第7章の確認を先に完了すること。

## 0. 前提・確定事項

- 対象は全15ダンジョン。アステラ「星環深層」（`dungeon.hidden.astral_depths`）は対象外であり、SO・敵・報酬を変更しない。
- ヴェルム黒鉄坑（`dungeon.VelmBlackIronMine`）は Upper（grade 3）から Highest（grade 4）へ昇格する。
- 新設は4件: ノルン Middle「翠樹族の集落跡」、グラード Middle「竜鱗峡谷」、ヴェルム Upper「熔炉防衛区」、アビス Upper「奈落境門」。
- 敵は既存アセットの再配置を原則とし、新規作成はアビス悪魔2体だけとする。道路レア4体はダンジョンの `normalEnemies` には入れず、道路遭遇のまま維持する。
- 通常敵等級の目安は Low=9--10、Lower=7--8、Middle=5--6、Upper=3--4、Highest=1--2。例外は既存アセットの再等級（第4章）で解消する。
- 装備ランク規則は「市場=町ランク、鍛冶=町ランク+1、ダンジョン限定装備=町ランク+2」（上限Rank 9）。同一町の複数ダンジョンは限定装備ランクを同一にする。黒鉄坑はHighest化後も Rank 8 を維持し、Rank 9 はアビス専用。
- 既存SOの `grade` 数値は Low=0 / Lower=1 / Middle=2 / Upper=3 / Highest=4 とする。

## 1. 全15ダンジョン確定構成表

|町|ダンジョン|段階|主軸種族|既存/新規|PersistentId|
|---|---|---:|---|---|---|
|セイル|はじまりの洞窟|Low|スライム+小型獣|既存|`dungeon.DungeonData`|
|リーフ|樹海の獣道|Low|獣|既存|`dungeon.LeafForestTrail`|
|エルド|地下水路|Low|人型ゴブリン|既存|`dungeon.EldUndergroundWaterway`|
|リーフ|封じられた廃坑|Lower|獣+不死|既存|`dungeon.LowerMine`|
|エルド|旧採石場|Lower|不死|既存|`dungeon.EldOldQuarry`|
|エルド|霧の古代遺跡|Middle|構造体+人型|既存|`dungeon.MiddleRuins`|
|ノルン|翠樹族の集落跡|Middle|人型オーク|新規|`dungeon.norn_verdant_settlement`|
|グラード|竜鱗峡谷|Middle|竜リザードマン|新規|`dungeon.glaad_dragon_scale_canyon`|
|ノルン|樹冠迷宮|Upper|構造体+人型|既存|`dungeon.NornCanopyLabyrinth`|
|グラード|天嶺砦|Upper|竜飛竜|既存|`dungeon.GlaadSkyFortress`|
|ヴェルム|熔炉防衛区|Upper|構造体+人型|新規|`dungeon.velm_furnace_defense_zone`|
|アビス|奈落境門|Upper|悪魔+竜|新規|`dungeon.abyss_gateway_threshold`|
|ヴェルム|黒鉄坑|Highest|構造体+竜+人型|既存・昇格|`dungeon.VelmBlackIronMine`|
|アビス|黒土深淵|Highest|悪魔+竜|既存|`dungeon.FinalBlackSoilAbyss`|
|アステラ|星環深層|Highest|星環|既存・対象外|`dungeon.hidden.astral_depths`|

## 2. 各ダンジョンの確定 `normalEnemies`

表記は実在する EnemyData アセットのファイル名（拡張子 `.asset` 省略）。新規2体以外は既存アセットを移管する。括弧内は件数。各アセットはここか「対象外/道路遭遇」に一度だけ置く。

- はじまりの洞窟 (7): `EnemyData`, `Grade10GreenSlime`, `Grade10BlueSlime`, `Grade10MossSlime`, `Grade10HornRabbit`, `enemy_slime_slime_acid`, `enemy_slime_slime_venom`
- 樹海の獣道 (7): `Grade09Kobold`, `Grade09WildDog`, `enemy_job_kobold_hexer`, `enemy_job_kobold_prowler`, `enemy_job_kobold_bulwark`, `enemy_job_kobold_ravager`, `enemy_job_kobold_packleader`
- 地下水路 (9): `Grade09Goblin`, `Grade09GoblinScout`, `Grade09GoblinSpearman`, `enemy_job_goblin_assassin`, `enemy_job_goblin_magician`, `enemy_job_goblin_knight`, `enemy_job_goblin_raider`, `enemy_job_goblin_veteran`, `enemy_slime_slime_acid` は重複禁止のため **酸液スライムの既存アセットを地下水路に残す**。したがって、はじまりの洞窟の酸液スライムは `enemy_slime_slime_stone` に置換する。

  実装時の最終Low内訳は次のとおりとする: はじまりの洞窟=`EnemyData`, `Grade10GreenSlime`, `Grade10BlueSlime`, `Grade10MossSlime`, `Grade10HornRabbit`, `enemy_slime_slime_stone`, `enemy_slime_slime_venom`; 樹海の獣道=上記7件; 地下水路=上記9件。これは「地下水路にゴブリン系8+酸液スライムを残す」の確定指示を優先した記述である。

- 封じられた廃坑 (13): `Grade07Zombie`, `Grade07BoneHound`, `Grade08CaveBat`, `Grade08GiantRat`, `Grade08CaveSpider`, `Grade08VenomMoth`, `Grade08RockBeetle`, `enemy_slime_slime_venom`, `Grade08CaveBat` 以外の獣系移管、および `enemy_slime_slime_stone`。

  **実装用の重複なし確定リスト**: `Grade07Zombie`, `Grade07BoneHound`, `Grade08CaveBat`, `Grade08GiantRat`, `Grade08CaveSpider`, `Grade08VenomMoth`, `Grade08RockBeetle`, `enemy_slime_slime_venom`。なお膨張整理文にある「ゾンビ/骨猟犬/洞窟コウモリ/大ネズミ/洞窟グモ/毒蛾/岩甲虫」は7件であり、猛毒スライムを加え8件。重複表現を避け、この8件を正とする。

- 旧採石場 (9): `Grade07Skeleton`, `Grade07Wraith`, `Grade07ArmoredSkeleton`, `enemy_job_skeleton_archer`, `enemy_job_skeleton_hexer`, `enemy_job_skeleton_guard`, `enemy_job_skeleton_reaper`, `enemy_job_skeleton_captain`, `enemy_slime_slime_stone`
- 霧の古代遺跡 (6): `Grade05StoneGolem`, `Grade05IronGolem`, `Grade05OgreMage`, `Grade06Hobgoblin`, `Grade06Troll`, `Grade06MarshLizard`
- 翠樹族の集落跡 (7): `Grade06Orc`, `enemy_job_orc_shaman`, `enemy_job_orc_rider`, `enemy_job_orc_bulwark`, `enemy_job_orc_berserker`, `enemy_job_orc_veteran`, `enemy_slime_slime_verdant`
- 竜鱗峡谷 (7): `Grade06Lizardman`, `enemy_job_lizardman_shaman`, `enemy_job_lizardman_stalker`, `enemy_job_lizardman_scaleguard`, `enemy_job_lizardman_ravager`, `enemy_job_lizardman_captain`, `enemy_slime_slime_quicksilver`
- 樹冠迷宮 (6): `Grade04DarkMage`, `Grade03GlaadSkyWarden`, `Grade05IronGolem`, `Grade05StoneGolem`, `VelmEmberforgedAutomaton`, `enemy_slime_slime_thunder`
- 天嶺砦 (10): `Grade03Wyvern`, `Grade03GlaadFrostDrake`, `Grade04GlaadGaleHarpy`, `enemy_job_wyvern_hexer`, `enemy_job_wyvern_skyrider`, `enemy_job_wyvern_ironwing`, `enemy_job_wyvern_ravager`, `enemy_job_wyvern_captain`, `enemy_slime_slime_frost_crystal`, `Grade03GlaadSkyWarden` は樹冠迷宮に移すため除外する。

  **実装用の重複なし確定リスト**: `Grade03Wyvern`, `Grade03GlaadFrostDrake`, `Grade04GlaadGaleHarpy`, `enemy_job_wyvern_hexer`, `enemy_job_wyvern_skyrider`, `enemy_job_wyvern_ironwing`, `enemy_job_wyvern_ravager`, `enemy_job_wyvern_captain`, `enemy_slime_slime_frost_crystal`。

- 熔炉防衛区 (3): `VelmBlackIronDelver`, `VelmDeepforgeHexer`, `VelmEmberforgedAutomaton` は樹冠迷宮へ移すため、代替として `Grade02DemonKnight` を使用しない。**実装前に第7章で3体目を確定する。**
- 奈落境門 (4): `Grade02DemonKnight`, `Grade01AbyssDragon`, `AbyssSpawnling`（新規・奈落の眷属）, `AbyssGatekeeper`（新規・奈落の門衛）
- 黒鉄坑 (5): `VelmBlackIronDelver`, `VelmEmberforgedAutomaton`, `VelmMagmaDrake`, `VelmDeepforgeHexer`, `enemy_slime_slime_magma`
- 黒土深淵 (2): `Grade02DemonKnight`, `Grade01AbyssDragon`
- 星環深層（対象外、4）: `Grade01AstralSentinel`, `Grade01AstralOracle`, `Grade01AstralReaver`, `enemy_slime_slime_astral`

### 2.1 配置の整合ルール

上記の原案には、同一アセットを2箇所へ置く記述（酸液/猛毒/石殻スライム、天嶺衛士、熾火魔導兵、悪魔騎士、深淵竜）が含まれる。`normalEnemies` はSO参照なので再利用自体は技術的には可能だが、「敵アセット名単位で移管」「全93+新規2の配置先を一意にする」という要件とは両立しない。Terra は以下を正として実装する。

- `enemy_slime_slime_acid` は地下水路、`enemy_slime_slime_venom` は封じられた廃坑、`enemy_slime_slime_stone` は旧採石場に残す。
- `Grade03GlaadSkyWarden` は天嶺砦、`VelmEmberforgedAutomaton` は黒鉄坑、`Grade02DemonKnight` と `Grade01AbyssDragon` は黒土深淵に残す。
- この結果、樹冠迷宮は `Grade04DarkMage`, `Grade05IronGolem`, `Grade05StoneGolem`, `VelmEmberforgedAutomaton`, `enemy_slime_slime_thunder` の5件となる。第7章の承認後に不足1件を熔炉防衛区とのトレードで補う。

既存ボス7体は `bossEnemy` として維持・再割当する（第7章）。道路レア4体 `MythicalGrade01AstralDragon`, `MythicalGrade03FlamewingGryphon`, `MythicalGrade05ThunderhornKirin`, `MythicalGrade07MistfangWolf` は道路遭遇専用で変更しない。これら、アステラ4体、既存ボス7体を含め既存93アセットを棚卸し対象とする。

## 3. 新規敵6体の仕様

|EnemyDataSO|表示名|種別|通常/ボス|等級|配置先|persistentId・battleVisualKey・役割|
|---|---|---|---|---:|---|---|
|`AbyssSpawn`|奈落の眷属|悪魔|通常|3--4|奈落境門|`HexBolt`。persistentId: `enemy.abyss_spawn`。術式役として等級3を基準に等級4帯へ混在できる耐久・魔力寄りの配分。battleVisualKey: `AbyssSpawn`。|
|`NornVerdantOrcHighChieftain`|翠樹の大族長|人型|ボス|4|翠樹族の集落跡|`BloodFrenzy`。persistentId: `enemy.boss.norn_verdant_orc_high_chieftain`。攻撃型、等級4通常敵を基準にHP・攻撃をボス補正。battleVisualKey: `NornVerdantOrcHighChieftain`。|
|`GlaadDragonScaleKing`|竜鱗王|竜|ボス|4|竜鱗峡谷|`ArmorPierce`。persistentId: `enemy.boss.glaad_dragon_scale_king`。貫通攻撃型、等級4通常敵を基準にHP・攻撃・防御をボス補正。battleVisualKey: `GlaadDragonScaleKing`。|
|`VelmGrandFurnaceColossus`|大熔炉巨像|構造体|ボス|3|熔炉防衛区|`Reconstitute`。persistentId: `enemy.boss.velm_grand_furnace_colossus`。耐久型、等級3通常敵を基準にHP・防御を強くボス補正。battleVisualKey: `VelmGrandFurnaceColossus`。|
|`AbyssGatekeeper`|奈落の門衛|悪魔|ボス|3|奈落境門|`MeteorRain`。persistentId: `enemy.boss.abyss_gatekeeper`。範囲攻撃型、等級3通常敵を基準にHP・攻撃をボス補正。battleVisualKey: `AbyssGatekeeper`。|
|`EldOldQuarryGravelord`|旧採石場の骸王|不死|ボス|6|エルド旧採石場|`SoulBurst`。persistentId: `enemy.boss.eld_old_quarry_gravelord`。`Boss06MineTyrant`を置換し、等級6通常敵を基準にHP・攻撃・防御をボス補正。battleVisualKey: `EldOldQuarryGravelord`。|

新規EnemyDataSOは上記6体。ボス補正は同等級の通常敵の役割別ステータスを土台に、攻撃型は攻撃、耐久型はHP・防御、術式・範囲型はHP・攻撃を優先して上積みする。深炉王は既存`VelmDeepforgeOverlord`を流用し、等級を1段階強化する（新規EnemyDataSOには含めない）。

## 4. 黒鉄坑Highest昇格の敵等級再割当と報酬

`VelmBlackIronMine.asset`（`dungeon.VelmBlackIronMine`）の `grade` を4へ変更する。通常敵は Highest 目安の等級1--2に合わせる。

|既存アセット|現状等級/役割|再割当|
|---|---|---|
|`VelmBlackIronDelver`|黒鉄坑通常|等級2|
|`VelmEmberforgedAutomaton`|黒鉄坑通常|等級2|
|`VelmMagmaDrake`|黒鉄坑通常|等級1|
|`VelmDeepforgeHexer`|黒鉄坑通常|等級1|
|`enemy_slime_slime_magma`|黒鉄坑通常|等級2|
|`VelmDeepforgeOverlord`|黒鉄坑ボス|等級1相当のボス|

報酬は Highest相当のゴールド・素材・強化鉱石へ引き上げる。ただし `limitedEquipmentDrops` は既存の `VelmBlackIronBreaker`, `VelmDeepforgeArmor`, `VelmEmberCore` を Rank 8 として維持する。Rank 9限定装備は `FinalBlackSoilAbyss.asset` の領分であり、黒鉄坑へ追加しない。

## 5. 新規4ダンジョンの DungeonDataSO 設定値

数値は同段階の既存SO（Middle: `MiddleRuins.asset`、Upper: `GlaadSkyFortress.asset` / `VelmBlackIronMine.asset`）を基準にした実装値。背景キーは既存SOが空文字列のため、背景アセット採用が決まるまで空文字列を入れる。

|DungeonDataSO案|フロア|encounterCount|maxEnemyCount|clearGold|限定装備Rank|背景キー|nearbyTownIndex|worldMapIndex|persistentId案|
|---|---:|---:|---:|---:|---:|---|---:|---:|---|
|`NornVerdantSettlement.asset`|5|4|5|360|6|`NornVerdantSettlement`|3|1|`dungeon.norn_verdant_settlement`|
|`GlaadDragonScaleCanyon.asset`|5|4|5|360|7|`GlaadDragonScaleCanyon`|4|1|`dungeon.glaad_dragon_scale_canyon`|
|`VelmFurnaceDefenseZone.asset`|6|4|5|600|8|`VelmFurnaceDefenseZone`|5|2|`dungeon.velm_furnace_defense_zone`|
|`AbyssGatewayThreshold.asset`|6|4|5|800|9|`AbyssGatewayThreshold`|6|2|`dungeon.abyss_gateway_threshold`|

共通値: `firstEncounterEnemyCount=3`（Middle）/`4`（Upper）、`enemyCountIncreasePerEncounter=1`（Middle）/`2`（Upper）、`enemyCountIncreasePerFloor=1`。限定装備の具体アセットと背景採用は未確定（第7章）であり、ここで新規アセットを先行作成しない。

## 6. 段階実装順（9ステップ）とテスト影響・セーブ影響

1. 本書と第7章の判断を確定し、敵配置の一意性表を承認する。
2. 既存12 DungeonDataSO と全93 EnemyData の参照・PersistentIdを自動検査するテストを追加する。
3. 地下水路、旧採石場、樹冠迷宮の `normalEnemies` を縮小し、移管先の既存SOへ再配置する。
4. 黒鉄坑をHighestへ昇格し、第4章の等級と報酬を更新する。
5. 新規敵2 EnemyDataSO（奈落の眷属・奈落の門衛）と必要な表示/ドロップを作成する。
6. 新規4 DungeonDataSOを第5章の値、`normalEnemies`、ボスで作成する。
7. ワールドマップ、町のダンジョン一覧、解放条件、表示順を4ダンジョン増加後の15件へ更新する。
8. 限定装備・報酬・背景キーを確定して参照を設定し、ランク規則とドロップ表を検証する。
9. EditMode/PlayModeで全ダンジョンのロード、遭遇生成、進行、報酬、セーブ復元、アステラ非変更を回帰する。

テスト影響: `DungeonRunManagerTests`、`DungeonProgressStoreTests`、`DungeonExpeditionManagerTests`、`GameAssetRepositoryTests`、`WorldMapServiceTests`、`WorldMapServiceDemandTests`、`EquipmentAvailabilityTests`、`BalanceExpansionDefinitionTests`、`SaveDataMigratorTests` を更新対象とする。敵の重複・未参照、gradeと敵等級、Town/Map index、Rank 9のアビス専有をテストで固定する。

セーブ影響: 既存の全 `persistentId` は変更しない。黒鉄坑は同一IDのままgradeのみ変わるため既存進行を維持する。新規4件は新IDとして追加する。ダンジョン進行がID配列/ビット位置に依存する場合は末尾追加だけにせず、IDベース移行を `SaveDataMigrator` で検証する。星環深層の保存値は変更禁止。

## 7. 未確定でユーザー判断が要る点

実装を開始する前に次を決める。

1. 既存ボスの流用割当: 新規ノルンMiddle、グラードMiddle、ヴェルムUpper、アビスUpperへ、既存ボス7体のどれを割り当てるか。奈落境門だけは新規 `AbyssGatekeeper` を使用することが確定している。
2. 限定装備の具体: 新規4件の `limitedEquipmentDrops` の名称・部位・効果・実アセット。ランクは第5章の6/7/8/9で確定。
3. 背景: 新規4件に専用背景を作るか、既存の `battleBackgroundKey: ""` 運用を維持するか。
4. 第2章のアセット一意配置の競合解消: 確定設計に重複指定された酸液/猛毒/石殻スライム、天嶺衛士、熾火魔導兵、悪魔騎士、深淵竜をどちらへ置くか。第2.1節の暫定優先規則を承認するか。
5. 熔炉防衛区・樹冠迷宮の6枠目: 新規敵を増やさず既存アセットだけで主軸を守るため、`VelmEmberforgedAutomaton` の配置先をどちらにするか（黒鉄坑に残すなら、熔炉防衛区の構造体枠の代替も決める）。
