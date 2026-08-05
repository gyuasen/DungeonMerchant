# テスト実行基準記録（TEST_BASELINE）

- **測定日**: 2026-08-04（初回測定: 2026-07-22）
- **Unity バージョン**: 2022.3.62f3
- **実行方法**: Unity バッチモード（`-batchmode -runTests`）
- **実行環境**: Windows 11 / プロジェクトの非破壊クローン上で実行
  - Unity Editor が対象プロジェクトを開いており排他ロックがかかっていたため、`Assets` / `Library` / `Packages` / `ProjectSettings` を一時領域へ複製し、そのクローンに対してテストを実行した。ソースおよびアセットは実プロジェクトと同一。

---

## 1. サマリ

| プラットフォーム | 総数 | 成功 | 失敗 | スキップ | 結果 |
|---|---:|---:|---:|---:|---|
| EditMode | 797 | 795 | **0** | 2 | Passed |
| PlayMode | 8 | 8 | **0** | 0 | Passed |
| **合計** | **805** | **803** | **0** | **2** | **Passed** |

テストアセンブリ:

- `DungeonMerchant.EditModeTests.dll` — 797 件
- `DungeonMerchant.PlayModeTests.dll` — 8 件

**失敗しているテストは存在しない。**

推移: 初回測定（2026-07-22）は EditMode 559 件。その後の改善作業で
EconomyController（+22）・CharacterEquipmentController（+28）・
TownTravelController（+30）などUI層のテストを拡充し、797 件となった。
スキップ2件は `BalanceExpansionDefinitionTests` の既知の2件で初回から変化なし。

---

## 2. 失敗テストの分類

失敗テストは 0 件のため、本節に該当項目はない。

分類基準（今後失敗が発生した場合に使用する区分）:

| 区分 | 意味 | 対応方針 |
|---|---|---|
| 実装上の不具合 | プロダクトコードの誤り | 修正対象。提出前に対応 |
| テスト側の期待値誤り | テストの assert が誤っている | テストを修正 |
| 仕様変更による不一致 | 仕様変更にテストが追随していない | テストを更新 |
| 実行環境依存 | 特定環境でのみ失敗 | 再現条件を記録し、環境注記 |
| 未調査 | 原因未特定 | 既知の問題として明記 |

---

## 3. スキップテスト（2件）

いずれも `Explicit` 属性が付与された**意図的な手動実行専用テスト**であり、不具合ではない。Editor ツールによるアセット生成を前提とするアセット検証のため、通常のバッチ実行では自動的に除外される。

| テスト | 分類 | スキップ理由（テストが出力した reason） |
|---|---|---|
| `BalanceExpansionDefinitionTests.AllEnemyAssets_HaveAssignedRace` | 仕様上の意図的スキップ（Explicit） | `Tools/DungeonMerchant/Enemy Race/Assign Missing Races` を実行してから本アセット検証を行うこと |
| `BalanceExpansionDefinitionTests.SlimeRaceAssets_ContainBaseFourPlusNineVariants` | 仕様上の意図的スキップ（Explicit） | `DungeonMerchant/Build Balance Expansion Assets` を実行してから本アセット検証を行うこと |

実行するには Unity Editor の Test Runner から明示的に選択して実行する。

---

## 4. 過去に報告された失敗の追跡

| テスト | 過去の状態 | 現在の状態 | 備考 |
|---|---|---|---|
| `BalanceExpansionSpecialEquipmentTests.LowHpDamageBonus_ActivatesBelowButNotAtThreshold` | 失敗として報告されていた | **Passed** | 本測定時点で解消済み。再発監視対象 |

---

## 5. 新規追加機能（ドロップ装備・特殊能力）のテスト裏付け

新しく追加されたドロップ装備および装備特殊能力は、以下のテストで検証されており、すべて成功している。

| テスト | 結果 | 検証内容 |
|---|---|---|
| `BalanceExpansionSpecialEquipmentTests.LowHpDamageBonus_ActivatesBelowButNotAtThreshold` | Passed | 低HP時ダメージ上昇が閾値未満で発動し、閾値ちょうどでは発動しないこと |
| `DungeonLimitedEquipmentStage1Tests.ReorganizedDungeon_UsesItsThreeDedicatedEquipmentDrops` | Passed | ダンジョン専用装備ドロップ（`item.dungeon.norn_verdant_chieftain_hatchet` / `LowHpDamageBonus` / 0.12f 等） |
| `ExistingDungeonEquipmentStage4Tests.ExistingDungeon_DropsOnlyItsThreeSetItemsWithSpecifiedWeaponEffect` | Passed | 既存ダンジョンが指定の3種セット装備と武器効果のみをドロップすること（`AbyssFang` / `LowHpDamageBonus` / 0.2f 等） |

---

## 6. セーブデータ整合性の監査結果（2026-07-31 再確認）

新装備・特殊能力の追加に伴うセーブデータ破損リスクを別途監査した結果（数値は 2026-07-31 に再確認した実値。初回監査時の 2026-07-22 の値は同期前の古いスナップショットで、`ItemDataSO` 226件・`CurrentVersion` 28 と記録していたが下表へ更新）:

| 監査項目 | 結果 |
|---|---|
| `ItemDataSO` の `persistentId` 未設定 | **0 件**（217件すべて設定済み） |
| `ItemDataSO` の `persistentId` 重複 | **なし** |
| 特殊能力の永続化方式 | `ItemDataSO.equipmentEffects`（マスタ側）に存在。`EquipmentInstance` へ複製保存されないため、セーブDTOの変更は不要 |
| `GameSaveData.CurrentVersion` | 37 |
| `SaveDataMigrator` 最新ステップ | version 37 対象。`CurrentVersion` と**整合** |
| セーブ書き込み方式 | 一時ファイル（`.tmp`）へ書込み→`File.Replace`（既存を `.bak` 退避）または `File.Move` による**原子的置換**（`SaveManager.cs:118-141`）。初回監査時点の「直接上書き」は解消済み |
| **セーブ→再起動→ロードで新装備・特殊能力が消失するリスク** | **なし（NO）** |

`SavedEquipmentInstance` が保存する項目: `townIndex` / `instanceId` / `baseItemAssetName` / `baseItemPersistentId` / `quality` / `enhancementLevel` / `isLocked` / `modifiers`（`type`, `value`）

### 検出された潜在リスク（提出は妨げない）

- **`EnemyDataSO` 53件で `persistentId` が未設定**（総数99件、2026-07-31 実測）。現在は実行時にアセット名へフォールバックするため動作に影響はなく、実効IDの重複もない。ただし将来アセット名を変更すると参照復元が壊れる可能性がある。
  - 暫定対応: **ID付与が完了するまで対象アセットの名前を変更しない**
  - 恒久対応: 各アセットへ不変の一意ID（例 `enemy.grade10.blue_slime`）を付与し、「対象アセットの `persistentId` が非空かつ一意」を検証する EditMode テストを追加する

---

## 7. Windows ビルド検証（未完了）

**結果: 検証できず（プロジェクトの不具合ではない）**

`BuildPipeline.BuildPlayer` による Windows (StandaloneWindows64) ビルドをクローン上で試行したが、以下の理由で完了しなかった。

- 失敗内容: `DirectoryNotFoundException`（`Extracting referenced dlls failed` → `Error building Player: 2 errors`）
- 対象ファイル例:
  - `Library/PackageCache/com.unity.collections@1.2.4/Unity.Collections.LowLevel.ILSupport/Unity.Collections.LowLevel.ILSupport.dll`
  - `Library/PackageCache/com.unity.collab-proxy@2.12.4/Lib/Editor/TextMateSharp/TextMateSharpPlastic.Grammars.dll`
- **原因: 検証用クローンの配置先パスが長く、上記ファイルのフルパスが Windows の MAX_PATH（260文字）を超過したため。** クローン上での該当パスは 283〜352 文字。
- **本体プロジェクトでは同ファイルのフルパスは 183 文字であり、この問題は発生しない。** 実際、Library/PackageCache 内の当該ファイルは本体側に正常に存在することを確認済み。
- コンパイルエラー（`error CS****`）は 1 件も発生していない。スクリプトのビルド自体は通っている。

### ビルド検証の実施方法（要実行）

Unity Editor でプロジェクトを開いている状態ではバッチモードのビルドは排他ロックで失敗する。以下のいずれかで実施すること。

1. **Unity Editor から実行（推奨）**: `File > Build Settings` → Platform を `Windows, Mac, Linux` / Target Platform を `Windows` / Architecture を `x86_64` に設定し、`Build` を実行する
2. **バッチモードで実行**: Unity Editor を閉じたうえで、本体プロジェクトに対して `-executeMethod` でビルドメソッドを呼び出す

ビルド後、`MANUAL_TEST_CHECKLIST.md` のセクション3（起動・終了・再起動）で起動確認を行う。

---

## 8. 本測定でカバーしていない範囲

自動テストの対象外であり、`MANUAL_TEST_CHECKLIST.md` による手動確認で担保する。

- Windows ビルドの生成と起動確認（**未実施** — 上記7参照）
- 実機での主要ゲームサイクル一周
- 新装備の入手→装備→特殊能力発動→保存→再起動→復元の一連確認
- Unity Console の例外・警告の目視確認
- 旧バージョン（version 37 未満）セーブデータからの実マイグレーション確認

---

## 9. 再実行手順

Unity Editor で対象プロジェクトを開いている場合は排他ロックにより失敗する。Editor を閉じるか、クローン上で実行すること。

```powershell
& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe" `
  -batchmode -nographics `
  -projectPath "<プロジェクトパス>" `
  -runTests -testPlatform EditMode `
  -testResults "editmode-results.xml" `
  -logFile "editmode-log.txt"
```

PlayMode の場合は `-testPlatform PlayMode` を指定し、`-nographics` を外す。
