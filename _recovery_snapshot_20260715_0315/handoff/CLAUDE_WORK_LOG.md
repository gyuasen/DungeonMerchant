# Claude Code 作業ログ・引き継ぎファイル

このファイルは、家側で Claude Code（Claude Sonnet 5 メインセッション + Opus/Sonnet サブエージェント）が作業する際の詳細な進捗記録・引き継ぎ用ドキュメントです。

`handoff/FROM_HOME_CHAT.md` には他環境（学校側・教室側のCodex）向けの簡潔な要約を書き、このファイルには Claude Code セッション再開時に必要な技術的詳細（計画全文、各ステップの完了状況、発見した技術的な注意点）を残します。

次回 Claude Code でこのプロジェクトの続きを行う場合は、**まずこのファイルを読んでください**。

## 運用ルール

- 家側で Claude Code が作業した内容は、作業単位ごとにこのファイルへ追記する。
- 全体状況の短い要約は `handoff/SHARED_PROJECT_STATUS.md` と `handoff/FROM_HOME_CHAT.md` にも反映する（`.instructions.md` の既存ルールに準拠）。
- 学校側・教室側（Codex）との内容の食い違いは、既存ルールどおり家側を優先する。

---

## 現在進行中の取り組み: 全体設計改善（Action 1〜4、全20ステップ）

### 経緯

2026-07-09、Claude Code のメインセッションが `Assets/Proiject` 配下の全C#スクリプト（約85ファイル）を4領域に分けてサブエージェントで並行調査し、全体設計評価を実施した。

### 発見した主な設計課題

1. **Core→UI依存の逆転**: `SaveManager`/`DungeonMerchantBootstrap`（Core層）が`SimpleMercenaryHireUI`（UI層）を直接参照し、町の進行状態（本来セーブ対象のCoreデータ）がUIクラスにしか存在しなかった。
2. **神クラス化**: `SimpleMercenaryHireUI`（partial 11ファイル、約7,200行、実質1クラス）、`BattleManager`（約1,480行）、`DungeonRunManager`（約870行）、`MarketStockManager`、`ProgressionManager`、`MerchantInventory`が複数責務を抱える。
3. **テストカバレッジの逆相関**: 純粋ロジックの小さいクラス（`WorldMapService`、`RoadTravelState`等）だけがテストされ、最も複雑でリスクの高いクラスがテストゼロ。
4. **ロジック重複**: 価格用ハッシュ関数（`MarketPriceManager`/`MarketStockManager`）、町availability判定switch（`MarketStockManager`/`BlacksmithManager`、ルール自体は別物）、装備品質倍率テーブル（`EquipmentInstance`周辺/`MerchantInventory`）、`BattlePageUIBase`/`MapPageUIBase`（実質同一）、`Company/Party/HealPageUI`のボイラープレート。

### 承認済み改善計画

計画全文は `C:\Users\yuga0\.claude\plans\vivid-puzzling-catmull.md`（Claude Code の個人環境内、Windowsユーザープロファイル配下）にあります。**このパスは家のPC・Claude Codeセッション固有のローカルパスであり、学校/教室環境や他のAIからは参照できません。** 以下は他環境からも読めるよう、計画の要点をこのファイルへ転記したものです。

前提: 開発中で実プレイヤーのセーブデータは存在しないため、セーブ形式の破壊的変更は許容する方針。`SimpleMercenaryHireUI`の分割は機能領域ごとの段階分割で進める方針。

#### Action 1 — Core→UI依存の切断（町進行状態の所有権移動）**【完了】**

| # | 内容 | 状態 |
|---|---|---|
| 1.1 | `TownProgressState`新設（`Assets/Proiject/Scripts/Core/TownProgressState.cs`）。`DayManager`/`DebtManager`と同型の小さなMonoBehaviour。町の現在地・解放町・表示中マップ番号を保持し、ドメイン計算は`WorldMapService`へ委譲。 | 完了 |
| 1.2 | `DungeonMerchantBootstrap`へ`TownProgressState`の自動生成を追加。 | 完了 |
| 1.3 | `SaveManager`の参照先を`SimpleMercenaryHireUI`から`TownProgressState`へ切り替え。`GameSaveData.CurrentVersion`を18→19へ更新（マイグレーション処理の追加は不要と確認済み）。 | 完了 |
| 1.4 | `SimpleMercenaryHireUI`本体・全partialファイルの`currentTownIndex`/`unlockedTownIndices`/`viewedWorldMapIndex`を`townProgressState`参照へ一括置換。 | 完了 |
| 1.5 | `RoadTravelCompletionServiceTests`/`RoadTravelStateTests`が無関係であることを静的に確認。 | 完了（Unity上の動作確認もユーザーにより実施済み） |

**実装中に発見・修正した問題**: `SaveManager`が`TownProgressState.RestoreTownProgress(...)`を直接呼ぶようになったことで、旧`SimpleMercenaryHireUI.RestoreTownProgress`内にあった`dungeonRunManager?.SetCurrentWorldMapIndex(...)`という副作用がロード時に呼ばれなくなる潜在バグを発見。`ApplyTownServiceSettings`/`SyncDungeonUnlocks`/`RefreshTownMapButtons`は`SimpleMercenaryHireUI.Start()`が毎回再実行するため問題なかったが、`DungeonRunManager.currentWorldMapIndex`の同期だけは他で行われていなかった。`SaveManager.ApplySaveData`の末尾（`dungeonRunManager?.RestoreProgress(...)`の直後）に`dungeonRunManager?.SetCurrentWorldMapIndex(townProgressState.CurrentWorldMapIndex);`を追加して解消。

**`TownProgressState`の公開API**（Action 3.10で`.Map.cs`/`.BattleDungeon.cs`を分割する際に再利用する）:
`CurrentTownIndex`（get）、`CurrentWorldMapIndex`（get）、`ViewedWorldMapIndex`（get/set）、`GetUnlockedTownIndices()`、`IsTownUnlocked(int)`、`RestoreTownProgress(int, IReadOnlyList<int>)`、`UnlockTown(int)`、`SetCurrentTown(int)`、`event Action TownProgressChanged`、`Initialize(int, IReadOnlyList<int>)`（テスト用シーム）。

#### Action 2 — 神クラスへのキャラクタリゼーションテスト追加 **【完了】**

対象順序: `BattleManager` → `DungeonRunManager` → `MarketStockManager`/`ProgressionManager` → `MerchantInventory`。いずれも`Initialize(...)`シームは追加せず、`RoadEncounterServiceTests.cs`のリフレクション手法（`FieldInfo`でprivate `[SerializeField]`に直接注入）と、公開ファクトリメソッド（`EquipmentInstance.CreateRestored`等）を使用。

| # | 内容 | 状態 |
|---|---|---|
| 2.1 | `BattleManagerTests.cs`（11テスト）: `StartBattle()`系ガード節、`CreateDefaultEnemyEncounter(int)`、`GetEncounterDescription()`。**発見**: `FindEnemyData()`は`Resources`に何もなくても必ずランタイムスライムへフォールバックするため、「敵データ未設定」系の分岐は公開APIから到達不能と判明。それを前提にテストを調整済み。 | 完了 |
| 2.2 | `DungeonRunManagerTests.cs`（12テスト）: `IsDungeonUnlocked`、`TrySelectDungeon`、`StartRun()`ガード節、`ChooseEventOption(int)`ガード節、`CreateFloorProgressSaveData()`/`RestoreProgress(...)`往復。**発見**: `OnEnable()`が`PlayerPrefs`(`"DungeonMerchant.Dungeon.HighestUnlockedGrade"`)を即座に読むため、`[SetUp]`/`[TearDown]`両方で`DeleteKey`必須（他テストファイルへの汚染防止）。`PopulateDungeonDataIfNeeded()`は全`Resources`配下から`DungeonDataSO`を読み込むため、独自の`persistentId`付きテストダンジョンを注入して決定性を確保。 | 完了 |
| 2.3 | `MarketStockManagerTests.cs`（6テスト）+ `ProgressionManagerTests.cs`（4テスト）: `GetBuyMultiplier`の決定性・レンジ（Action 4.1の回帰ガード兼用）、`ProgressionManager`の`CreateSaveData()`/`Restore(...)`往復。**発見**: `ProgressionManager.TotalGoldEarned`は`merchantData`が存在すると`merchantData.LifetimeGoldEarned`を優先し、`Restore(...)`で書いた`totalGoldEarned`フィールドは無視される二重管理の癖を明示的にテストで固定（設計レビューで指摘済みの問題と一致）。 | 完了 |
| 2.4 | `MerchantInventoryTests.cs`（15テスト）: `GetSellPrice(EquipmentInstance)`の品質倍率（Poor 0.65/Normal 1.0/Fine 1.2/Rare 1.55/Legendary 2.2）と強化値+0.12/lvを厳密な期待値でピン留め（Action 4.3の回帰ガード）。`SellEquipmentInstance`のロック拒否、`AddItem`/`TryRemoveItem`のスタック挙動も網羅。 | 完了 |

**既知の未解決バグ（今回のセッションとは無関係、ユーザーが別途対応予定）**: ダンジョンタブ（`BuildDungeonPage`/`DungeonPageUI`）で、ダンジョン選択行がヘッダー付近に表示され、「探索開始」ボタンや選択中ラベルと重なって見える表示崩れを確認。Consoleにエラー・警告は無し。Action 1〜3.1のいずれの変更もこのコードパス（`SimpleMercenaryHireUI.BattleDungeon.cs`の`BuildDungeonPage`、`DungeonPageUI.cs`）に触れておらず、Prefab（`Assets/Proiject/Resources/UI/SimpleMercenaryHireUIView.prefab`）上の`Dungeon Page`のRectTransformサイズ（764×430）もコード側の想定（`CreatePage`のフォールバック計算と一致）と完全に一致することを確認済み。ユーザーの認識では、今回より前のUI責務分割作業（`handoff/FROM_HOME_CHAT.md`の学校側/教室側エントリ）由来の既存バグと推測。ユーザーが別途、実機でのRectTransform Inspector値確認等により対応予定。次にこの調査を再開する場合は、Playモード中に`SimpleMercenaryHireUIView(Clone)/Guild Panel/Dungeon Page/Dungeon Selection List`配下の個別行のRectTransform実値（Anchors/Pivot/Pos/Size）を確認するところから始めること。

**重要な制約（今後のAction 2/3/4作業で再確認すること）**: `BattleManager.StartBattle`/`DungeonRunManager.StartRun`はいずれも`StartCoroutine`で実際の戦闘進行を行うが、UnityのコルーチンはPlayモード外（EditModeテスト）では進行しない。本プロジェクトにはPlayModeテスト基盤が存在しないため、`BattleCompleted`発火・ダメージ計算・スキル解決などのテストは対象外とし、同期的に返るガード節・純粋関数のみをテスト対象とした。将来PlayModeテストを整備する場合はfast-followとして別途スコープを切ること。

**Unity Editor起動中の制約**: サブエージェントがUnityバッチモードでのテスト実行を試みたところ、既に起動中のUnity Editorと競合して失敗した（"Multiple Unity instances cannot open the same project"）。以降、テスト内容の検証はソースコードの手動照合で行い、実際のテスト実行・グリーン確認はユーザーがUnity Editor上のTest Runnerで行った。

**Unity Test Runnerでの実行結果: 104件中104件成功（0失敗）で確認済み。** 実行時に3件のコンパイル/テスト不具合が見つかり、いずれも修正済み。
1. `MerchantInventoryTests.cs`で`using System;`と`using UnityEngine;`が同居し、`Object`型（`System.Object`と`UnityEngine.Object`）が曖昧参照でCS0104エラー。`UnityEngine.Object`に明示修飾して解消。
2. 同ファイルの`Does.Not.Contain(equipment)`/`Does.Contain(equipment)`がCS1503（`EquipmentInstance`から`string`への変換不可）でコンパイル失敗。原因はNUnItの`Does.Contain`のオーバーロード解決の問題。`CollectionAssert.DoesNotContain`/`CollectionAssert.Contains`に置き換えて解消。
3. `ProgressionManagerTests.TotalGoldEarned_WithMerchantDataPresent_PrefersMerchantDataOverRestoredField`がテスト実行時に失敗（Expected 1234, But was 9999）。`ProgressionManager.TotalGoldEarned`のgetterは`ResolveReferences()`を自分で呼ばず`OnEnable()`頼みのため、同一GameObjectへ複数コンポーネントを追加した直後に読むとタイミング依存で`merchantData`が未解決のままになることがあると判明。テスト側で`CanStore()`（`ResolveReferences()`を内部で呼ぶ公開メソッド）を明示的に呼んでから検証するよう修正して解消（本番コードは変更していない）。

#### Action 3 — `SimpleMercenaryHireUI`の段階分割【完了】

低結合→高結合の順で抽出。`.Map.cs`/`.BattleDungeon.cs`（3.10）は町状態を直接触るため**Action 1完了後**の今なら着手可能。

| # | 抽出対象 | 新規クラス | 備考 |
|---|---|---|---|
| 3.1 | `.UIFactory.cs`（669行） | `SimpleMercenaryHireUIFactory` | **完了**。実際は純粋なUI構築ヘルパーだけでなく`RefreshUI()`・ページルーティング・メニュー・ビュー結合ロジックも混在していたため、計画を修正: 純粋な構築ヘルパー（`CreateText`/`CreateRow`/`CreateActionButton`等16メソッド）のみを新クラスへ移動し、`SimpleMercenaryHireUI.UIFactory.cs`側は同一シグネチャの薄い委譲ラッパーとして残した（他9partialファイルの呼び出し元は無変更）。Unity上でコンパイル・表示確認済み（ダンジョン画面のUI崩れは無関係な既存バグと判明、ユーザーが別途対応）。 |
| 3.2 | `.MapData.cs`（27行） | `WorldMapService`へ統合 | **完了**。調査の結果、このファイルは過去の作業で既に`WorldMapService`への薄い委譲ラッパー（`TownNames`/`WorldRegionNames`/`GetWorldMapIndexForTown`/`AreTownsAdjacent`/`GetNextTownToward`）だけになっていた。全partialファイルの呼び出し元を`WorldMapService.X`直接参照に置換し、`SimpleMercenaryHireUI.MapData.cs`と`.meta`を削除（`GetWorldMapIndexForTown`/`GetNextTownToward`のラッパーは呼び出し元ゼロの死にコードだったため単純削除）。メインセッションが直接実施。 |
| 3.3 | `.DailyResult.cs`（527行）+関連フィールド | `DailyResultController` | **完了**。日次スナップショットの全フィールド・`DailyMercenarySnapshot`・テキスト生成ロジックを新設`DailyResultController.cs`（447行、プレーンC#クラス、コンストラクタ注入: MerchantData/MercenaryHireManager/MercenaryPartyManager/MerchantInventory + `GetEquipmentDisplayName`デリゲート）へ移動。`.DailyResult.cs`は527→126行に縮小しオーバーレイ生成/表示ルーティングのみ残存。`.HireParty.cs`/`.Economy.cs`/コア`.cs`の呼び出しは`dailyResultController.X(...)`へ更新。旧`ShowDailyResult`の`currentDay <= dailySnapshotDay`早期リターンは`BuildDailyResultText`がnullを返す形で制御フロー同一を確認（メインセッションが旧版とdiff照合済み）。 |
| 3.4 | `BattlePageUIBase`/`MapPageUIBase`統合（Action4.5） | `RefreshOnlyPageUIBase` | **完了**。両者はバイト同一の重複だった。新設`RefreshOnlyPageUIBase.cs`へ統合し、6サブクラス（BattlePageUI/RoadBattlePageUI/DungeonPageUI/GlobalMapPageUI/WorldMapPageUI/TownMapPageUI）の基底宣言を差し替え。旧2ファイル+meta削除（`MapPageUI.cs`はベースクラスのみだったためファイルごと削除）。 |
| 3.5 | `Company/Party/HealPageUI`共通化（Action4.5） | `EconomyPageUI`基盤へ移行 | **完了**。`EconomyPageUI`を`ListPageUIBase`へリネーム（git mvでGUID保持、シリアライズ参照への影響なしを事前確認）。CompanyPageUI（249→203行）とHealPageUI（177→98行）は`RebuildRows`へ移行、PartyPageUIは固定スロット表示のため`Refresh()`独自実装のまま（131行）。重複していた色定数・`Configure`・`CreateEmptyMessage`を基底へ集約（色値は全クラスでfloat同一だったため上書き不要と確認）。`ListRoot`をprotected→publicへ変更（HireParty.cs:180の既存参照のため）。呼び出し側の変更ゼロ、dotnet buildエラー0件。既知の微小差分: 空リスト時のスクロール高さが430f最小値に正規化される（旧は未更新のまま）が、表示への影響なし。 |
| 3.6 | `.HireParty.cs`（670行） | `HireAndPartyController` | **完了**。雇用/編成/治療/転職のアクションロジック17メソッド＋候補追跡5フィールド（hireButtons等）を新設`HireAndPartyController.cs`（282行）へ移動。UI構築（Build*/Bind*）・ページ遷移（Show*）・イベントハンドラ（Handle*）はゴッドクラス側に残存（3.3と同じ切り方）。`RefreshUI()`内の雇用ボタン活性ループは`UpdateHireButtonInteractability()`としてコントローラーへ。死にコード`CreateMercenaryListEmptyMessage`を削除。`.HireParty.cs`は670→465行。dotnet buildエラー0・警告0。 |
| 3.7 | `.Economy.cs`（528行） | `EconomyController` | **完了**。市場/鍛冶屋/在庫のアクションロジック18メソッド＋追跡4フィールド（marketBuyButtons等）＋絞込/並替状態（inventoryFilter/equipmentSort）を新設`EconomyController.cs`（284行、コンストラクタ注入: MerchantInventory/MarketStockManager/BlacksmithManager + setStatus/refresh*Page/refreshUI/setFilterButtonLabel/setSortButtonLabelデリゲート）へ移動。UI構築（Build*）・ページ遷移（Show*）・イベントハンドラ（Handle*）・`AdvanceDay`（DayManager一行委譲のため注入回避）はゴッドクラス側に残存（3.6と同じ切り方）。`InventoryFilter`/`EquipmentSort`のenum型宣言はトップレベル（コア`.cs`末尾）のため移動なし。`RefreshUI()`内の購入/制作ボタン活性ループは`UpdateEconomyButtonInteractability()`としてコントローラーへ。`.CharacterEquipment.cs`の`SellEquipment`呼び出しも`economyController.SellEquipment`へ更新。`.Economy.cs`は528→251行。dotnet buildエラー0・警告0。Action 4.1/4.2は未実施（別途）。 |
| 3.8 | `.CharacterEquipment.cs`（1294行）+コア`.cs`内`EquipSelectedEquipment` | `CharacterEquipmentController` | **完了**。選択状態（`SelectedDetailMercenary`/`SelectedEquipmentDetail`プロパティ化）＋テキスト生成（詳細/比較/スキル表/図鑑）＋ビジネスアクション（装備/解除/強化/ロック/売却/消費アイテム使用）を新設`CharacterEquipmentController.cs`（857行、プレーンC#、コンストラクタ注入: MerchantData/MerchantInventory/MercenaryHireManager/BattleManager/EconomyController + setStatus/setCharacterDetailContent/showCharacterDetails/hideEquipmentDetails/hasEquipmentDetailOverlay/装備詳細ラベル設定系/refresh*Page/refreshUI/saveEquipmentChanges/saveGameデリゲート）へ移動。コア`.cs`の`EquipSelectedEquipment`両オーバーロード・`FormatSigned`/`FormatComparison`・privateネスト`MercenarySkillInfo`（トップレベル化）と`.BattleDungeon.cs`の`UseConsumable`も同コントローラーへ。`GetEquipmentDisplayName`/`GetEquipmentQualityColor`は純関数のため**public static**化 — `DailyResultController`コンストラクタ引数の生成順序問題は静的参照で解消（順序制約なし）。オーバーレイ構築（Build*）・表示ルーティング（Show*/Hide*/Rebuild*行生成）・`showingCharacterStatusPage`（純粋な表示状態）・`SaveEquipmentChanges`（GetComponent/FindObjectOfTypeのMonoBehaviour依存フォールバックのためデリゲート注入で残置）・`ShowMercenarySkillDetail`（4行のUI直結表示）はゴッドクラス側に残存。`ShowEquipmentDetails`は本体をコントローラーへ移し、UI反映はラベル設定デリレゲート経由（3.7と同じ切り方）。死にコード`BuildMercenarySkillSummary`（呼び出し元ゼロ）は挙動同一のためコントローラーへそのまま移動。実測行数: `CharacterEquipmentController.cs`928行、`.CharacterEquipment.cs`1294→689行、コア`.cs`907行、`.BattleDungeon.cs`786行。dotnet buildエラー0・警告0。※担当サブエージェントは検証フェーズ途中でセッション上限停止したため、最終検証（残存参照ゼロ・静的メソッド化による生成順解消・ビルド再実行）はメインセッションで実施し問題なしを確認。 |
| 3.9 | `.MerchantQuest.cs`（439行） | `MerchantStatusAndQuestController` | **完了**。テキスト生成（商人サマリー/技能行タイトル・説明/技能ボタンラベル/長期目標/依頼行タイトル・詳細・状態・ボタンラベル）＋ビジネスアクション5メソッド（`IncreaseMerchantSkill`/`RepayDebt`/`AcceptQuest`/`UpgradeStorage`/`RenewContract`）＋活性判定（`CanIncreaseSkill`/`CanRepay`/`CanAcceptQuest`/`ShouldShowRepayButtons`）を新設`MerchantStatusAndQuestController.cs`（209行、プレーンC#、コンストラクタ注入: MerchantData/ProgressionManager/DebtManager/MercenaryHireManager + setStatus/rebuildMerchantStatus/rebuildQuestList/refreshCompanyPage/refreshUIデリゲート）へ移動。オーバーレイ構築（Build*）・表示ルーティング（Show*/Hide*）・イベントハンドラ（HandleGoldChanged/HandleProgressionChanged、コア`.cs`のイベント購読先のため）・行生成ループ（Rebuild*/CreateMerchantSkillRow — UI構築はゴッドクラス、行内テキスト/活性判定のみコントローラー参照、3.8と同じ切り方）はゴッドクラス側に残存。`.Economy.cs`の`UpgradeStorage`と`.HireParty.cs`の`RenewContract`（CompanyPageUIへのデリゲート渡し）を`merchantStatusAndQuestController.X`へ更新。依頼状態文字列は旧コードでタイトルとボタンラベルの2箇所で同一ローカル変数を使用していたが、コントローラー内`GetQuestState`共有で同値を確認。`.MerchantQuest.cs`は439→300行。dotnet buildエラー0・警告0。 |
| 3.10 | `.Map.cs`（857行）+`.BattleDungeon.cs`（951行） | `TownTravelController`+`DungeonBattleController` | **完了**。町移動フロー（`roadTravelState`＋確認状態2フィールド、`TravelToTown`/`TravelToDungeon`/`RequestTownTravel`/`ConfirmTownTravel`/`StartTownTravelBattle`/`StartNextTravelEncounter`/`ContinueTownTravel`/`RetreatFromTownTravel`/旧`HandleBattleCompleted`の街道分岐→`HandleRoadBattleOutcome(bool)`/`CanEnterWorldRegion`/`IsGateTownFullyCleared`/`ApplyTownServiceSettings`）を新設`TownTravelController.cs`（408行、プレーンC#、コンストラクタ注入: TownProgressState/MercenaryPartyManager/BattleManager/RoadEncounterService/DungeonRunManager/DayManager/MercenaryGenerator/MarketStockManager/BlacksmithManager/SaveManager + setStatus/showTownMap/showWorldMap/showTravelConfirmation/hideTravelConfirmation/resetBattleLog/showRoadBattlePage/setRoadChoiceButtonsActive/setRoadBattleRouteText/continueTravelBattle/openNearbyDungeon/syncDungeonUnlocks/refreshTownMapButtonsデリゲート13本）へ移動。Action 1で確認済みの街道戦闘勝利時シーケンス（UnlockTown→SetCurrentTown→ViewedWorldMapIndex→SetCurrentWorldMapIndex→ApplyTownServiceSettings→AdvanceDay→SyncDungeonUnlocks→RefreshTownMapButtons）は文単位で同一順序を維持（コントローラー内docコメントで順序固定を明記）。ダンジョン/戦闘アクション（`battleLogLines`＋`AppendBattleMessage`/`ClearBattleLog`、`StartPartyBattle`/`StartDungeonRun`/`SelectDungeon`/`ChooseDungeonEventOption`/`CycleBattleSpeed`/`OpenNearbyDungeon`/`EnsureNearbyDungeonSelected`/`BuildDungeonRewardPreview`(public static)/`ColorizeBattleMessage`/`EscapeRichText`）は新設`DungeonBattleController.cs`（295行、注入: BattleManager/DungeonRunManager/MercenaryPartyManager/TownProgressState + setStatus/resetBattleLog/showBattlePage/showDungeonPage/setStartBattleButtonInteractable/setStartBattleButtonActive/setBattlePageTitle/setBattleEncounterText/refreshPartyStatePages/updateDungeonEventUI/setSpeedButtonLabels/refreshUIデリゲート12本）へ。**判断事項**: (1)コルーチンは移動不可のため`ContinueTownTravelBattleRoutine`は1フレーム待機→`StartNextTravelEncounter()`呼び出しの薄い殻として残置、ログ自動スクロールコルーチン（`ScrollBattleLogToLatestRoutine`）と`ResetBattleLog`のUI部も残置。(2)`SyncDungeonUnlocks`はコア`Start()`がコントローラー生成**前**（先頭付近）に呼ぶうえGetComponentフォールバックを持つためpartial残置とし、コントローラーからはデリゲート経由。(3)コントローラー間連携（移動完了後の`OpenNearbyDungeon`）はDungeonBattleControllerを先に生成し`openNearbyDungeon`デリゲートで接続（直接相互参照なし）。(4)`GetTownName`は`WorldMapService.GetTownName`とバイト同一の重複だったため削除し呼び出し元を直接参照へ（計画時の想定どおり）。(5)死にコード`GetNextUnlockableTownIndex`ラッパー/`ShowUnavailableWorldMap`（いずれも呼び出し元ゼロ）を削除。(6)`ContinueToNextDungeonFloor`/`ReturnToTownAfterDungeon`/`HideTravelConfirmation`はUIのみの薄いハンドラのためpartial残置（内部でコントローラー呼び出し）。新クラス名は計画時の仮称`TownMapController`から、実責務（町マップUIではなく移動フロー）に合わせ`TownTravelController`へ変更。実測行数: `.Map.cs`858→606行、`.BattleDungeon.cs`889→625行、コア`.cs`919→981行（コントローラー生成配線分の増加）。dotnet build（Runtime段階2回＋EditModeTests）いずれもエラー0・警告0。※完了条件のPlayモード通し確認（開始→雇用→移動→戦闘→ダンジョン→セーブ/ロード）はユーザーによる実施待ち。 |

各ステップ共通の完了条件: コンパイル成功＋該当画面の手動Playモード確認（自動UIテストが存在しないため）。3.10完了時はゲームのメインループ全体（開始→雇用→移動→戦闘→ダンジョン→セーブ/ロード）を通しで確認する。

#### Action 4 — 重複ロジック統合【完了】

4.4/4.5はAction 3（3.4/3.5）で実施済み。

- 4.1 価格ハッシュ関数統合。**【完了】** 新規`Assets/Proiject/Scripts/Merchant/MarketHashUtility.cs`（静的クラス）に統合。`MarketPriceManager.CalculateStableHash`（seed17→day→type→rarity→名前）と`MarketStockManager.GetStableHash`（seed→day→town→salt→type→rarity→名前）は混合材料が異なるため、共通尾部`MixItem`を共有する2つの`ComputeItemHash`オーバーロードとして実装（混合順序は逐語コピーでビット同一）。`GetStableIndex`のxor/乗算方式も`ComputeStableIndex`として移動（`Mathf.Abs`/`Mathf.Max`の呼び出しも同一に保持）。両マネージャーのprivateメソッドは薄い委譲に置換。メインセッションが直接実施。回帰ガードは`MarketStockManagerTests.cs`の決定性テスト。
- 4.2 町availability switch統合。**【完了】** `WorldMapService`に`TownEquipmentRule`行テーブル（AllowAll/MaxRank/MinRank/Classes/Slotsの論理和評価）と`IsMarketEquipmentAllowedInTown`/`IsBlacksmithEquipmentAllowedInTown`を新設し、`MarketStockManager`/`BlacksmithManager`のswitch文をテーブル参照に置換。旧switchの「case 0-5明示・それ以外はdefault」の分岐を`SelectRule`で厳密再現。"Mutant Core Charm"の`currentTownIndex == 6`特例は`BlacksmithManager`側に原文のまま残置。**パリティテスト`TownAvailabilityParityTests.cs`を追加**: 旧switch文を逐語転記したオラクル関数と新テーブルを、町{0-7,-1}×基本6職×rank{0-4}×3スロット=全810組合せ×2サービスで完全一致検証。メインセッションが直接実施。Runtime/EditModeTests両ビルドエラー0。
- 4.3 装備品質倍率テーブル統合。**【完了】** `MerchantInventory.GetSellPrice(EquipmentInstance)`内の品質switchを`EquipmentInstance.GetSellPriceQualityMultiplier()`（新設publicメソッド、値は0.65/1.0/1.2/1.55/2.2で同一）へ移動し、`MerchantInventory`側は呼び出しに置換。メインセッションが直接実施。dotnet buildエラー0。回帰ガードは`MerchantInventoryTests.cs`の品質別厳密価格テスト（次回Unity起動時にTest Runnerで緑確認を推奨）。

### 検証方法

- 自動テスト: Unity Editor → Test Runner → EditModeタブ、または `Unity.exe -batchmode -projectPath . -runTests -testPlatform EditMode -testResults results.xml -quit`。既存アセンブリ定義 `Assets/Proiject/Tests/EditMode/DungeonMerchant.EditModeTests.asmdef` 配下に追加。
- 手動確認: UI層（Action 3）は自動テストが無いため、各ステップ後にPlayモードで該当画面を実際に操作して確認する。
- 回帰ガードの原則: Action 4の各項目は対応するAction 2のテストが存在する状態でのみ実施する（テストが先、統合は後）。

### サブエージェント運用方針

- 実装はOpus/Sonnetクラスのサブエージェントに切り出し、Claude Codeメインセッションは設計・監査・レビューに専念する（ユーザー指示）。
- 各ステップ完了後、メインセッションが差分をレビューしてから次のステップへ進む。
- 実装難易度が特に高い、または見落としリスクが高い箇所（例: Action 1.4の副作用調査）はメインセッションが直接調査・指示を作成する。

### 完了確認（2026-07-12）

**全20ステップ（Action 1〜4）が完了し、ユーザーによる最終確認も済んだ。**

- Unity Test Runner（EditModeタブ）: **106件中106件成功（0失敗）** をユーザーが確認（Action 2の104件+`TownAvailabilityParityTests`2件）。
- コンパイルエラーなし・Unity上の動作確認済み。
- 未コミットの作業がある場合はコミットを推奨（Action 3.2以降の変更）。

---

## 再評価（2026-07-12、第1次改善計画完了後）

第1次計画完了後に、UI層+新コントローラー群の再調査（サブエージェント）と定量指標の採取を行い、設計を再評価した。

### 定量比較（前回7/9評価 → 現在）

| 指標 | 前回 | 現在 |
|---|---|---|
| Core→UI依存 | SaveManager/BootstrapがUIを直接参照 | Bootstrapのみ（コンポジションルートとして正当） |
| `SimpleMercenaryHireUI`実ロジック | 約7,200行に混在 | partial残5,278行（UI構築/配線のみ）+独立クラス8個3,291行 |
| EditModeテスト | 8ファイル・単純クラスのみ | 14ファイル1,869行・106件全緑 |
| 重複ロジック | 5系統 | 5系統すべて一本化（パリティテスト付き） |
| `FindObjectOfType` | 約60箇所 | 61箇所（意図的に未着手） |

### 再評価で判明した新規/残存の課題

1. **`CharacterEquipmentController`のデリゲート過多**: コンストラクタ引数22個。ボタン単位のマイクロセッター6個は小さなビューインターフェースに束ねるべき。
2. **チュートリアル機能が抽出規約に違反**: 別作業系統で追加された`SimpleMercenaryHireUI.Tutorial.cs`（191行）が、状態・コンテンツ・PlayerPrefsをMonoBehaviour直持ちする旧パターン。神クラス再成長の芽。
3. **色定数の重複8箇所**: RowColor/WoodButtonColor/FrameColor系が6ファイル+ボタン遷移ColorBlockが2箇所。共有パレット未整備。
4. **ページUIの`ListPageUIBase`準拠度が3段階**: 完全準拠（Company/Heal）/フィールドのみ継承（Inventory/Market/Blacksmith）/独立（Hire/JobChange）。
5. **ビュー側に残る軽いビジネスロジック**: `ShowWorldMap(int)`のゲート判定メッセージ構築、`RebuildCharacterEquipmentList`の装備可否フィルタ。
6. **死にコード3件**: `StyleUnavailableWorldMapButton`（Map.cs）、`BuildMercenarySkillSummary`（CharacterEquipmentController）、`DailyResultController.dailyInventoryNames`（書き込みのみ）。
7. **未着手のまま（既知）**: `BattleManager`（約1,480行）/`DungeonRunManager`（約870行）の神クラス、`ProgressionManager`の複数責務と`totalGoldEarned`二重管理、`FindObjectOfType`依存61箇所、ファイル名タイポ、`MercenaryContractType.cs`文字化け、ダンジョンタブUI崩れ。

---

## 第2次改善計画（ロードマップ、2026-07-12策定・未着手）

**このセクションが次の作業計画の正本。** どのエージェント（Claude Code / 学校側Codex / 教室側Codex）が着手する場合も、フェーズ順に、1ステップ=1コミット可能な粒度で進め、完了したらこの表の状態を更新すること。第1次計画で確立した規約（下記「作業規約」）に従う。

### 作業規約（第1次計画から継承。新規参加エージェントは必読）

- **コントローラー抽出パターン**: プレーンC#クラス、マネージャーはコンストラクタ注入、UI副作用（statusText・ページ更新・ボタンラベル）は`Action`/`Func`デリゲート注入。UI構築（Build*）・ページ遷移（Show*）・イベント購読・コルーチンはMonoBehaviour側に残す。既存の8クラス（特に`EconomyController`）が手本。
- **テスト規約**: EditMode NUnit、`GameObject`+`AddComponent`+`Object.DestroyImmediate`、private注入はリフレクション（`RoadEncounterServiceTests`参照）。`DungeonRunManager`を触るテストは`PlayerPrefs`キー`"DungeonMerchant.Dungeon.HighestUnlockedGrade"`をSetUp/TearDown両方でDeleteKey。
- **リファクタの原則**: 挙動同一（ロジックは逐語コピー）、テストが先・統合は後、各ステップでdotnet build（`DungeonMerchant.Runtime.csproj`/`DungeonMerchant.EditModeTests.csproj`）エラー0確認。新規.csファイルは両csprojへの`<Compile Include>`追加が必要（Unityが後で再生成するが、それまでのビルド確認用）。
- **してはいけないこと**: PowerShell正規表現での一括書き換え（日本語リテラル破損の前科あり。Edit/Writeツールを使う）、`using System;`と`using UnityEngine;`同居時の裸の`Object`参照（CS0104）、`Mathf.Abs`等のAPI呼び出しを手書き演算に置き換えること（ビット同一性が壊れる）。

### フェーズA — 衛生・小規模（各項目独立、順不同で着手可。1項目=サブエージェント1回分）

| # | 内容 | 対象ファイル | 完了条件 | 状態 |
|---|---|---|---|---|
| A-1 | `TutorialController`抽出。`SimpleMercenaryHireUI.Tutorial.cs`の状態（stepIndex）・コンテンツ（静的文字列配列）・PlayerPrefs処理をコントローラーへ移し、partialはオーバーレイ構築/表示のみに縮小 | `SimpleMercenaryHireUI.Tutorial.cs`、新規`TutorialController.cs`、コア`.cs`（生成追加） | 既存8コントローラーと同型になる。build 0エラー。Playモードでチュートリアル表示/進行/完了フラグ動作確認 | 完了（2026-07-12）: `TutorialController`新設（デリゲート9本注入、ロジック逐語コピー）、partialは構築+Show/Hideのみ97行に縮小、Runtime csproj追加、build 0エラー。Playモード確認は未実施（ユーザー確認待ち） |
| A-2 | 死にコード削除3件: `StyleUnavailableWorldMapButton`、`BuildMercenarySkillSummary`、`DailyResultController.dailyInventoryNames` | `.Map.cs`、`CharacterEquipmentController.cs`、`DailyResultController.cs` | grep で呼び出しゼロ再確認後に削除。build 0エラー | 完了（2026-07-12）: 3件ともgrepで参照ゼロ確認後に削除（`dailyInventoryNames`はフィールド+Clear+書き込み2箇所、読み取りゼロ確認済み）。両csproj build 0エラー |
| A-3 | `totalGoldEarned`二重管理解消: `ProgressionManager`のフィールド・`HandleGoldChanged`差分加算・`ProgressionSaveData.totalGoldEarned`を除去し、`merchantData.LifetimeGoldEarned`単独参照へ。セーブVersion 19→20（マイグレーション不要、フィールド削除のみ） | `ProgressionManager.cs`、`GameSaveData.cs` | `ProgressionManagerTests`の該当テスト（precedence quirkテストは仕様変更に伴い書き換え）更新。Test Runner全緑 | 完了（2026-07-12）: シャドウフィールド・`HandleGoldChanged`・`GoldChanged`購読・`lastObservedGold`（差分加算専用と確認）・`ProgressionSaveData.totalGoldEarned`を除去、getterは`merchantData.LifetimeGoldEarned`単独（null時0）。Version 19→20、`SaveDataMigrator`は参照なし確認。テスト4本書き換え（precedenceテストは`TotalGoldEarned_ReadsMerchantDataLifetimeEarnings`へ改名・簡素化）。両csproj build 0エラー。Unity Test Runner実行は未実施（Editor起動要、ユーザー確認待ち） |
| A-4 | ファイル名タイポ修正: `EnemyDateSO.cs`→`EnemyDataSO.cs`、`MercenaryDstsSO.cs`→`MercenaryDataSO.cs`。**必ず`git mv`で.metaも同時リネームしGUID保持**（アセット参照が壊れるため） | 上記2ファイル+meta | Unity再起動後にMissing Script警告ゼロ、既存アセット参照が生きていること | 未着手 |
| A-5 | `MercenaryContractType.cs`の文字化けコメント修正（UTF-8で保存し直し） | 同ファイル | 文字化け解消、コンパイル無変化 | 未着手 |
| A-6 | ダンジョンタブUI崩れの修正（ユーザー対応予定だったもの。エージェントが着手する場合は上記Action 2セクション末尾の調査メモから再開: Playモード中の`Dungeon Selection List`配下行のRectTransform実値確認が起点） | `DungeonPageUI.cs`または`.BattleDungeon.cs`のレイアウト値 | Playモードでダンジョンタブの行がヘッダーと重ならない | 未着手 |

### フェーズB — UI層の仕上げ（A完了後推奨、相互独立）

| # | 内容 | 完了条件 | 状態 |
|---|---|---|---|
| B-1 | 共有色パレット導入: `UITheme`（静的クラス）を新設し、8箇所に重複する色定数（RowColor等6ファイル+ColorBlock2箇所）を一本化。値は現状と完全同一 | 重複宣言の削除、目視で全画面の見た目不変 | 完了（2026-07-12: `UITheme.cs`新設・12色+`ApplyButtonTransitions`。6ファイルの重複宣言削除、ColorBlock2箇所は`UITheme.ApplyButtonTransitions`へ委譲。値は全て完全同一コピー。注: ページUI群の`rowTextColor=Color.white`/`mutedTextColor=Color.gray`はSimpleMercenaryHireUIの値と異なるためローカルのまま維持。Editor側`SimpleMercenaryHireUIPrefabBuilder.cs`にも同値リテラルが残存するがB-1対象外。Runtime/EditModeTests両build 0エラー） |
| B-2 | `CharacterEquipmentController`のデリゲート整理: 装備詳細オーバーレイ向けマイクロセッター6個を小さなビュー抽象（例: `IEquipmentDetailView`を`SimpleMercenaryHireUI`が実装）に束ね、コンストラクタ引数を22→15前後へ | build 0エラー、装備詳細画面の全操作（強化/売却/ロック）動作確認 | 完了（2026-07-13: `IEquipmentDetailView.cs`新設（HasOverlay/SetTitle/SetDetailText/SetEnhanceButton/SetSellButton/SetLockButtonLabel/ShowOverlay/HideOverlayの8メンバー）。装備詳細オーバーレイ系デリゲート8本（マイクロセッター6+hasEquipmentDetailOverlay+hideEquipmentDetails）を束ね、コンストラクタ引数22→15。実装は`SimpleMercenaryHireUI.CharacterEquipment.cs`末尾に明示的実装（旧ラムダ本体を逐語移動、`HideOverlay`は既存`HideEquipmentDetails`へ委譲）、コア`.cs`のnewは`this`を渡す形に短縮。第2クラスター（setCharacterDetailContent+showCharacterDetails）は2本のみで束ねる価値なしと判断し据え置き。Runtime csprojに`<Compile Include>`追加、両csproj build 0エラー0警告。Playモードでの強化/売却/ロック動作確認は未実施（ユーザー確認待ち）） |
| B-3 | ページUI準拠度の統一 | 全リスト画面の表示・スクロール・ボタン動作が不変 | 完了（2026-07-13、Sonnetが引き継ぎ実施）。**発見**: `MarketPageUI`/`BlacksmithPageUI`は着手前から既に`RebuildRows`+共有`CreateEmptyMessage`を完全使用済みだった（過去調査時点の「フィールドのみ継承」という記述は当時の状態を反映しておらず、対応不要と判明）。`JobChangePageUI`を`ListPageUIBase`へ完全移行（`Configure`は基底の`titleFontSize`任意引数=17を利用、`Refresh`は`RebuildRows`使用）。`HirePageUI`を`ListPageUIBase`へ移行したが、固定候補+生成候補の2コレクションを1本の`rowTop`で連続配置する構造のため`RebuildRows`（1コレクション前提でClearChildrenする）には合わず、装飾（色/フォント/`Configure`/`Initialize`）のみ基底委譲し`Refresh()`は独自実装のまま理由コメント付きで維持（`PartyPageUI`と同型の正当な例外）。`InventoryPageUI`も通常アイテム+装備の2コレクション構造で同じ理由により独自`Refresh()`を維持し、理由コメントを追加。**回帰修正**: `JobChangePageUI`移行時、呼び出し元`.HireParty.cs`の`pageUI.Configure(...)`が新設`titleFontSize`引数を渡しておらず、タイトル文字サイズが17→15へ縮小する回帰を作業中に発見・修正（`titleFontSize: 17`を明示指定）。Runtime/EditModeTests両build 0エラー0警告。Playモードでの雇用/転職/在庫画面の見た目確認は未実施（ユーザー確認待ち）。 |

### フェーズC — Battle/Dungeon層の分割（最大の残負債。B完了を待つ必要はないがC-1が先行必須）

| # | 内容 | 完了条件 | 状態 |
|---|---|---|---|
| C-1 | **PlayModeテスト基盤の整備**（C-2/C-3の前提）: PlayModeテスト用asmdef新設、`BattleManager.StartBattle`→`BattleCompleted`の最小ハッピーパス（1HP敵即殺で`BattleCompleted(true)`）を`[UnityTest]`で1本通す | PlayModeテストがTest Runnerで緑。学校/教室環境でも実行手順をこのファイルに追記 | 完了（2026-07-13: `Assets/Proiject/Tests/PlayMode/`にasmdefと`BattleManagerPlayModeTests`を追加。Test RunnerのPlayModeタブで`StartBattle_OneHpEnemy_CompletesWithVictory`が1/1成功。EditModeも106/106成功を確認） |
| C-2 | `BattleManager`分割（約1,480行）: 段階抽出。(1)`BattleRewardService`（報酬/ドロップ/経験値、`DungeonRewardService`が手本）→(2)`BattleLogFormatter`（ログ文字列生成、静的クラス化候補）→(3)`BattleSkillResolver`（14敵スキル+6職スキルの解決、最難関）→(4)`BattleStatusEffectService`。各段階でEditModeガード節テスト+C-1のPlayModeテストが緑を維持 | 各段階1コミット。`BattleManager`残存はターン進行コルーチンと参照解決のみ | 実装完了・Unity再確認待ち（2026-07-13: 4サービスを抽出し、`BattleManager`を1480行から602行へ縮小。各EditModeテスト追加、`dotnet build DungeonMerchant.sln`警告0・エラー0） |
| C-3 | `DungeonRunManager`分割（約870行）: (1)フロア進行保存/復元（`PlayerPrefs`+永続ID解決）を`DungeonProgressStore`へ→(2)イベント提示状態（EventTitle等のUI向けプロパティ群）の整理。`DungeonRunManagerTests`の往復テストが回帰ガード | 各段階1コミット、Test Runner全緑 | 実装完了・Unity再確認待ち（2026-07-13: `DungeonProgressStore`と`DungeonEventState`を追加。公開API・永続ID/旧名互換・通知順を維持。対応EditModeテスト追加、全sln build警告0・エラー0） |

**フェーズA/S完了確認（2026-07-12）**: A-1〜A-6（A-6=S-2）およびS-1〜S-5のエージェント作業がすべて完了し、**ユーザーがUnity上で動作確認済み**（ダンジョンタブ表示修正・チュートリアル・テスト含む）。残るユーザー作業は S-4のサンプルセーブ配置（配置後READMEの「同梱予定です」→「同梱しています」へ変更）と S-6（面接想定問答、エージェントによる下書き支援可）のみ。

**フェーズB完了確認（2026-07-13）**: B-1〜B-3すべて完了（B-3はSonnetが引き継ぎ実施、JobChangePageUIのタイトル文字サイズ回帰を作業中に発見・修正済み）。Runtime/EditModeTests両build 0エラー。**Playモードでの見た目確認（雇用/転職/在庫/装備詳細画面）は未実施 — ユーザー確認待ち**。

**フェーズC実装状況（2026-07-13）**: C-1はUnity Test RunnerでPlayMode 1/1成功済み。C-2/C-3もコード分割と単体テスト追加まで完了し、全sln buildは警告0・エラー0。最終完了判定には、Unity Test Runnerで現行EditMode全件とPlayMode 1件の再実行が必要。

### フェーズS — 提出前仕上げ（就活ポートフォリオ提出前に完了させる。A/B/Cとは独立、最優先扱い可）

背景: 本作は就活用ポートフォリオ。2026-07-12にメインセッションで提出前リスク評価を実施した。権利面の結論: マップ・羊皮紙画像はChatGPT(OpenAI)での画像生成と確認済み（権利はユーザー帰属、商用可、法的問題なし）。フォントは同梱がZen Kurenaido（SIL OFL、`Assets/Proiject/Fonts/`にOFL.txt有り）のみで、游明朝等はOSフォントの実行時読込のため再配布に非該当。残る対応は「申告の整合性」と「品質印象」。

| # | 内容 | 完了条件 | 状態 |
|---|---|---|---|
| S-1 | README更新: AI活用欄を実態（ChatGPT設計相談+画像生成 / Codex / Claude Codeによる設計レビュー・リファクタリング支援）に合わせる。画像アセットの出所（AI生成）を明記。フォントのクレジット（Zen Kurenaido, SIL OFL）を追記 | READMEの記載がリポジトリの実態（handoffログ含む）と矛盾しないこと | 完了（2026-07-12） |
| S-2 | ダンジョンタブUI崩れの修正（=A-6と同一。提出前必須に格上げ） | Playモードでダンジョンタブの行がヘッダーと重ならない | 未着手 |
| S-3 | タイポ・文字化け・体裁（=A-4/A-5+α）: ファイル名2件、`MercenaryContractType.cs`文字化け、`ProjectSettings`のcompanyName（DefaultCompany→個人名等） | Unity再起動後にMissing Script警告ゼロ | 完了（2026-07-12、別エージェント実施・本行は後追い記入）: ファイル2件リネーム（`EnemyDataSO.cs`/`MercenaryDataSO.cs`、.meta同時移動でGUID保持）+csproj更新+`MercenaryContractType.cs`文字化け修正+companyName=YugaSen。Unity再起動でのMissing Script警告ゼロ確認は未実施（ユーザー確認待ち） |
| S-4 | 審査者向け導線: READMEに「5〜10分で見るポイントと手順」を追記し、進行済み`game-save.json`のサンプルを同梱（配置場所と読込手順も記載） | 初見のレビュアーが10分で主要システム（雇用→戦闘→ダンジョン→経済）に触れられる | 完了（2026-07-12）: READMEゲームサイクル直後に「採用ご担当者様へ：10分で見るポイント」を追加（プレイルート/コードの見どころ/サンプルセーブ配置先`%USERPROFILE%\AppData\LocalLow\YugaSen\DungeonMerchant\game-save.json`）。サンプルセーブファイル自体はユーザーが後日配置（README内にTODOコメント有り） |
| S-5 | ビルド配布時のライセンス表記: ビルドフォルダに`LICENSES.txt`（Zen Kurenaido OFL全文+画像はAI生成の旨）を同梱する運用をREADMEに記載 | ビルドzipにLICENSES.txtが含まれる | 完了（2026-07-12）: リポジトリ直下に`LICENSES.txt`作成（OFL 1.1全文をOFL.txtから逐語コピー+画像出所の注記）、READMEアセットの出所欄に同梱運用を1行追記 |
| S-6 | 面接想定問答の整理（コード変更なし）: `FindObjectOfType`残存/神クラス/AI活用ワークフローについて、本ファイルの記録を基に自分の言葉で説明できる想定問答メモを作る（ユーザー自身の作業。エージェントは下書き支援可） | ユーザーが主要設計判断を説明できる状態 | 未着手 |

注意: `Assets/Proiject`フォルダ名のタイポは全アセットパスに影響するため修正しない（READMEで自己申告済みの扱いとするか、ユーザー判断）。

### フェーズD — 長期（着手判断はユーザーと相談してから）

- `FindObjectOfType`依存61箇所の段階削減（Bootstrapまたは依存解決専用クラスへ集約。全面DI化はコスト大のため非推奨、新規コードで増やさない運用を継続）
- `MercenaryClass.cs`内の`MercenaryClassProgression`（7つの並行switch）のデータ駆動化（ScriptableObject化）
- `SaveManager`の分割（`SaveDataFactory`/`SaveDataApplier`/`SaveFileStore`、2026-07-07教室側メモの候補）
- 経済チューニング値（`MerchantData`の倍率式等）の設定アセット化

### 実行時の運用（Claude Codeの場合）

- 実装はOpus/Sonnetサブエージェントへ委譲し、メインセッションは設計・監査・レビュー（ユーザー方針）。C-2(3)のスキル解決分割のみ難度が高いためメインセッション直接実施を検討。
- 各項目完了時にこの表の「状態」を更新し、`FROM_HOME_CHAT.md`に要約を追記。
