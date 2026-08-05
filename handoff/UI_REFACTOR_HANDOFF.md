# UI層リファクタリング 引き継ぎ

最終更新: 2026-07-31 / 対象ブランチ: `main`

このファイルは、UI層の構造改善作業を**別環境で再開する**ための引き継ぎメモ。
全体状況は [SHARED_PROJECT_STATUS.md](SHARED_PROJECT_STATUS.md)、詳細な作業履歴は [CLAUDE_WORK_LOG.md](CLAUDE_WORK_LOG.md) を参照。

---

## 1. 現在の到達点（実測値）

| 指標 | 作業開始時 | 現在 |
|---|---|---|
| EditModeテスト | 713 | **794**（失敗0） |
| PlayModeテスト | 8（未検証） | **8**（失敗0・実証済み） |
| `SimpleMercenaryHireUI` partial数 | 19 | **5** |
| 同クラス合計行数 | 10,406 | **2,985**（初回8,627行から65%減） |
| 抽出View/Presenter | 0 | **19クラス** |
| 主要Presenterのctor引数 | — | **3〜5**（最大53から整理） |
| 共有フィールド | 317 → 162 | **153** |
| `FindObjectOfType` | 134 | **101** |
| 抽出View/Presenterクラス | 0 | **10クラス / 1,561行** |

### 構造評価の推移

| 領域 | 初回 | 現在 |
|---|---|---|
| テスト戦略 | A | A |
| ドメイン分離 | B+ | B+ |
| セーブ設計 | B+ | B+ |
| パフォーマンス対応 | B | B+ |
| 依存関係管理 | C | **B−** |
| **UI層の構造** | **D** | **A−** |

> UI層は第三者監査（Codex Sol）による5回の独立判定を経て **D → C+ → B → B+ → A−** に到達。
> 総合は「純粋な構造品質A− / ポートフォリオとして強いA−」。**このリファクタリング系列は完了と判断された。**

評価根拠の全文は [../docs/ARCHITECTURE_ASSESSMENT.md](../docs/ARCHITECTURE_ASSESSMENT.md)。

---

## 2. 完了した作業（このセッション）

### partial → 独立クラス変換（9本）

| 変換元partial | 抽出先クラス |
|---|---|
| `.Tutorial.cs` | `TutorialOverlayView` |
| `.ContractDetails.cs` | `ContractDetailsOverlayView` |
| `.FacilityGreeting.cs` | `FacilityGreetingOverlayView` |
| `.MonsterCodex.cs` | `MonsterCodexOverlayView` + `MonsterCodexPresenter` |
| `.Onboarding.cs` | `OnboardingGuideBannerView` |
| `.TrainingGround.cs` | `TrainingGroundPagePresenter` |
| `.RemoteSale.cs` | `RemoteSaleOverlayView` |
| `.Story.cs` | `StoryOverlayView`（表示のみ） |
| `.DailyResult.cs` | `DailyResultOverlayView`（表示のみ） |
| `.Expedition.cs` | `ExpeditionOverlayPresenter` |
| `.MerchantQuest.cs` | `MerchantQuestOverlayPresenter` |
| `.CharacterEquipment.cs` | `CharacterEquipmentOverlayPresenter` |
| `.Economy.cs` | `EconomyPresenter`（3段階） |
| `.HireParty.cs` | `HirePartyPresenter`（3段階） |
| `.BattleDungeon.cs` | `BattleDungeonPresenter`（3段階・313行残置） |
| `.Map.cs` | `MapPresenter`（3段階・52行残置） |

**500行を超えるpartialは3段階に分けること。** 1,031行の一括変換を試みて失敗した実績がある。各段階でコンパイルと全テスト通過を確認してからコミットする。

### その他

- **アセンブリ分割の最小実証**: `DungeonMerchant.Domain`（`noEngineReferences: true`）に `HealingCostService` / `TrainingCostService` を分離。UnityEngine参照が CS0103 で拒否されることを実証済み
- **テスト汚染の修正**: 破棄済み `SaveManager` のゾンビ購読による `MissingReferenceException` を解消（回帰テスト追加）
- **テスト追加**: `CharacterEquipmentControllerTests`(28) / `TownTravelControllerTests`(30) / `EconomyControllerTests`(+22)

---

## 3. 確立した変換パターン（再開時はこれに従う）

### 抽出クラスの規約

```
- グローバル名前空間、public sealed class、MonoBehaviourにしない
- ctorで factory / 参照グループ / 親Transform / 必要な依存 / コールバック のみ注入
- SimpleMercenaryHireUI への参照・キャスト・GetComponent・FindObjectOfType を書かない
- 色は private static readonly で UITheme の別名を持つ
- 公開APIは Build() / Show() / Hide() + setter群
```

**参考にすべき既存実装**: `Assets/Proiject/Scripts/UI/TutorialOverlayView.cs`（最も素直な例）、
`StoryOverlayView.cs`（コルーチン・副作用を本体に残した例）、
`MonsterCodexOverlayView.cs` + `MonsterCodexPresenter.cs`（View/Presenter分離の例）。

### 守るべき制約

- **facadeは新partialに分けず、本体 `SimpleMercenaryHireUI.cs` に直接置く**（partial数削減が目的の一つ。過去に分けてしまい統合し直した経緯あり）
- `StartCoroutine` の呼び出しと MonoBehaviour ライフサイクル（`OnEnable` 等）は**必ず本体に残す**。非MonoBehaviourからは呼べない
- 挙動不変：レイアウト数値・GameObject名・階層・色・文言・イベント順を1つも変えない
- `RuntimeUIPlayModeTests` がリフレクションで参照する private フィールドは**変更・移動しない**：
  `globalMapPage` `worldMapPage` `townMapPage` `hirePage` `companyPage` `partyPage` `healPage` `battlePage` `roadBattlePage` `dungeonPage` `marketPage` `blacksmithPage` `inventoryPage` `jobChangePage` `dungeonView` `battleView` `roadBattle` `firstDungeonEventButton`
- 1 partial = 1コミット。他partialの整理を混ぜない

---

## 4. 現在の構成と、残作業の扱い

### partial 5本の内訳（すべて意図した状態）

| partial | 行数 | 性質 |
|---|---|---|
| `.cs`（本体） | 2,033 | composition root（依存解決・Presenter生成・BuildUI・イベント購読・facade） |
| `.UIFactory.cs` | 523 | UI生成ヘルパー |
| `.BattleDungeon.cs` | **313** | **意図した境界**（コルーチン4本・pending状態・イベント調停） |
| `.ScrollHelpers.cs` | 70 | 補助ヘルパー |
| `.Map.cs` | **52** | **意図した境界**（コルーチン・Unlock同期・facade） |

**`BattleDungeon.cs` と `Map.cs` の365行は「未完の抽出」ではない。** 残るのは非MonoBehaviourのPresenterへ移せないもの（`StartCoroutine`、Unity擬似nullを扱う探索、`Debug.LogWarning(..., this)`）で、監査でも「MonoBehaviourと非MonoBehaviourの意図した境界」「薄いAdapter/Facadeとして成立」と評価されている。**これ以上削るべきではない。**

### 変換完了した機能（全13本）

Tutorial / ContractDetails / FacilityGreeting / MonsterCodex / Onboarding / TrainingGround / RemoteSale / Story / DailyResult / Expedition / MerchantQuest / CharacterEquipment / Economy / HireParty / BattleDungeon / Map

### 依存束パターン（後続作業で従うこと）

Presenterの依存が増えすぎた際は、責務別の依存束へ再編する。`MapPresenter` が最も素直な実装例。

```
XxxViewDependencies    … factory / 各page / 参照コンテナ / Func<T>プロバイダ
XxxDomainDependencies  … Manager・Controller類
XxxCallbacks           … 生成物を本体へ戻すsetter
XxxNavigation          … ページ遷移・refresh・その他Action
```

主要4Presenterのctorは **3〜5引数**（整理前は最大53）。**束にPresenter自身の公開メソッドを入れないこと**（循環参照になる）。

### 監査の結論

> 「このリファクタリング系列は**完了と判断してよい**。これ以上、評価を上げる目的だけで本体を分割する費用対効果は低い。次に優先すべきは追加リファクタリングではなく、ポートフォリオで改善過程を説明すること」

**機能追加で自然に必要になった場合のみ**、以下を検討すればよい。

- 移動確認UIが成長したら `TravelConfirmationPresenter` を抽出
- 本体130フィールドを機能別モジュールへ分ける
- 依存束をコンストラクタ＋read-onlyプロパティ化する

### アセンブリ分割の続き（任意）

現在Domainには2ファイルのみ。候補として Unity非依存の純粋C#が30以上あるが、
`TownServicePolicy` は `WorldMapService` 経由で `ItemDataSO` に届くため閉包が閉じない。
**grepではなくコンパイルで閉包を保証できる範囲に限定する**方針を維持すること。

---

## 5. 別環境での作業手順

### テスト実行（重要）

Unity Editorがプロジェクトを開いていると排他ロックで `-runTests` が失敗する。
**プロジェクトを一時領域へ複製してテストする**のが確実：

```bash
# Assets のみ差し替えれば足りる（Library はクローン側を再利用）
SCRATCH=/path/to/scratch
rm -rf "$SCRATCH/testproj/Assets"
cp -r /path/to/DungeonMerchant/Assets "$SCRATCH/testproj/"

# EditMode
"/c/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe" -runTests -batchmode -nographics \
  -projectPath "$SCRATCH/testproj" -testPlatform EditMode \
  -testResults "$SCRATCH/r.xml" -logFile "$SCRATCH/u.log"

# PlayMode は -nographics を外す
"/c/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe" -runTests -batchmode \
  -projectPath "$SCRATCH/testproj" -testPlatform PlayMode \
  -testResults "$SCRATCH/rpm.xml" -logFile "$SCRATCH/upm.log"
```

初回は `Assets` / `Packages` / `ProjectSettings` を複製してクローンを作る。
結果XMLは `//test-run` 要素の `total` / `passed` / `failed` 属性を見る。
コンパイルエラーは `grep "error CS" u.log | sort -u`。

### 期待値

- EditMode: **total=794 passed=792 failed=0 skipped=2**
  （skip 2件は `BalanceExpansionDefinitionTests` の `Explicit` 属性付き。既知・正常）
- PlayMode: **total=8 passed=8 failed=0**

### Editor上での実行について

Editorのテストランナーで実行する場合、**同一プロセスに前回実行の残骸が残ると
順序依存の失敗が起きうる**。過去に破棄済み `SaveManager` のゾンビ購読で
`MissingReferenceException` が発生した（修正済み・回帰テストあり）。
Editorだけで落ちてバッチで通る場合は、この種のテスト間汚染をまず疑うこと。

---

## 6. 参照ドキュメント

| ファイル | 内容 |
|---|---|
| [../docs/ARCHITECTURE_ASSESSMENT.md](../docs/ARCHITECTURE_ASSESSMENT.md) | 構造評価。良い点と問題点を実測で評価。改訂履歴あり |
| [../docs/UI_IMPROVEMENT_PLAN.md](../docs/UI_IMPROVEMENT_PLAN.md) | UI改善計画と実施記録。優先順位の判断根拠 |
| [../docs/INTERVIEW_QA.md](../docs/INTERVIEW_QA.md) | 就活想定問答。設計判断の言語化 |
| [../docs/REBUILD_RETROSPECTIVE.md](../docs/REBUILD_RETROSPECTIVE.md) | ゼロから作り直す場合の改善案 |
| [../TEST_BASELINE.md](../TEST_BASELINE.md) | テスト実行基準記録 |

---

## 7. 作業で得た教訓（次に活かすこと）

- **「何箇所あるか」ではなく「どれだけの頻度で実行されるか」で優先順位を決める。**
  `FindObjectOfType` 削減で、箇所数最多の `SimpleMercenaryHireUI`(20箇所) は
  起動時1回しか走らず実害が無い一方、ガードの無い `SaveManager` が保存のたびに
  18回シーン全走査していた。計画段階でも計測が要る。
- **名前の近さではなく責務でグループを決める。**
  `equipment` 接頭辞17フィールドは3機能の混在だった。数字上の削減を優先せず分割した。
- **partialは同一クラスなので `private` が機能しない。**
  グルーピングは見通しを作るが、カプセル化には独立クラス化が要る。
