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

#### Action 3 — `SimpleMercenaryHireUI`の段階分割【進行中】

低結合→高結合の順で抽出。`.Map.cs`/`.BattleDungeon.cs`（3.10）は町状態を直接触るため**Action 1完了後**の今なら着手可能。

| # | 抽出対象 | 新規クラス | 備考 |
|---|---|---|---|
| 3.1 | `.UIFactory.cs`（669行） | `SimpleMercenaryHireUIFactory` | **完了**。実際は純粋なUI構築ヘルパーだけでなく`RefreshUI()`・ページルーティング・メニュー・ビュー結合ロジックも混在していたため、計画を修正: 純粋な構築ヘルパー（`CreateText`/`CreateRow`/`CreateActionButton`等16メソッド）のみを新クラスへ移動し、`SimpleMercenaryHireUI.UIFactory.cs`側は同一シグネチャの薄い委譲ラッパーとして残した（他9partialファイルの呼び出し元は無変更）。Unity上でコンパイル・表示確認済み（ダンジョン画面のUI崩れは無関係な既存バグと判明、ユーザーが別途対応）。 |
| 3.2 | `.MapData.cs`（27行） | `WorldMapService`へ統合 | |
| 3.3 | `.DailyResult.cs`（527行）+関連フィールド | `DailyResultController` | `.HireParty.cs`/`.Economy.cs`の書き込み箇所も同時に`Record*` API呼び出しへ更新 |
| 3.4 | `BattlePageUIBase`/`MapPageUIBase`統合（Action4.5） | `RefreshOnlyPageUIBase` | 3.6の前に実施 |
| 3.5 | `Company/Party/HealPageUI`共通化（Action4.5） | `EconomyPageUI`基盤へ移行 | 3.6の前に実施。`PartyPageUI`は`Refresh()`独自実装のまま |
| 3.6 | `.HireParty.cs`（670行） | `HireAndPartyController` | Action 1完了、3.3、3.5が前提 |
| 3.7 | `.Economy.cs`（528行） | `EconomyController` | 3.6と同じ前提。Action 4.1/4.2をここで併せて実施可 |
| 3.8 | `.CharacterEquipment.cs`（1294行）+コア`.cs`内`EquipSelectedEquipment` | `CharacterEquipmentController` | 最大サイズのため他段階で経験を積んでから |
| 3.9 | `.MerchantQuest.cs`（439行） | `MerchantStatusAndQuestController` | |
| 3.10 | `.Map.cs`（857行）+`.BattleDungeon.cs`（951行） | `TownMapController`+`DungeonBattleController` | 最終段階、最大リスク |

各ステップ共通の完了条件: コンパイル成功＋該当画面の手動Playモード確認（自動UIテストが存在しないため）。3.10完了時はゲームのメインループ全体（開始→雇用→移動→戦闘→ダンジョン→セーブ/ロード）を通しで確認する。

#### Action 4 — 重複ロジック統合【未着手】

4.4/4.5はAction 3（3.4/3.5）に割り込み済み。以下はAction 2の対応テスト完了直後に実施。

- 4.1 価格ハッシュ関数統合（`MarketPriceManager`/`MarketStockManager` → 新規`MarketHashUtility.cs`）。Action 2.3直後。
- 4.2 町availability switch統合（`MarketStockManager`/`BlacksmithManager`。ルールは別物なので関数統合ではなく`WorldMapService`への静的テーブル化で構造の重複だけ解消）。全7町×クラスの組み合わせパリティテストが必須。**このリファクタリング全体で最もリスクが高い項目**。
- 4.3 装備品質倍率テーブル統合（`MerchantInventory.GetSellPrice(EquipmentInstance)`のswitchを`EquipmentInstance.GetSellPriceQualityMultiplier()`へ移動）。Action 2.4直後。

### 検証方法

- 自動テスト: Unity Editor → Test Runner → EditModeタブ、または `Unity.exe -batchmode -projectPath . -runTests -testPlatform EditMode -testResults results.xml -quit`。既存アセンブリ定義 `Assets/Proiject/Tests/EditMode/DungeonMerchant.EditModeTests.asmdef` 配下に追加。
- 手動確認: UI層（Action 3）は自動テストが無いため、各ステップ後にPlayモードで該当画面を実際に操作して確認する。
- 回帰ガードの原則: Action 4の各項目は対応するAction 2のテストが存在する状態でのみ実施する（テストが先、統合は後）。

### サブエージェント運用方針

- 実装はOpus/Sonnetクラスのサブエージェントに切り出し、Claude Codeメインセッションは設計・監査・レビューに専念する（ユーザー指示）。
- 各ステップ完了後、メインセッションが差分をレビューしてから次のステップへ進む。
- 実装難易度が特に高い、または見落としリスクが高い箇所（例: Action 1.4の副作用調査）はメインセッションが直接調査・指示を作成する。

### 次回再開時にやること

1. このファイルと `handoff/SHARED_PROJECT_STATUS.md` を読んで現状を把握する。
2. Action 3.1（`SimpleMercenaryHireUI.UIFactory.cs`の抽出）から着手する。
3. 各ステップ完了後、このファイルの表を更新し、`handoff/FROM_HOME_CHAT.md` にも要約を追記する。
4. Action 3はUnity上の自動テストが存在しないため、各ステップ後に必ずユーザーへPlayモードでの手動確認を依頼すること（Action 2で判明した通り、コード照合だけでは実行時の不具合を検出できない）。
