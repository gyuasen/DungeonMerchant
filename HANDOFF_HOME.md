# DungeonMerchant 引き継ぎメモ

このファイルは、学校側の開発環境を家のチャット/別PCで読み込ませるための要約です。

## 家のチャットに最初に貼る文

以下のUnityプロジェクトの続き開発を手伝ってください。

プロジェクト名は `DungeonMerchant`。商人ローグライクダンジョンRPGです。商人が傭兵を雇い、パーティーを組んでダンジョン/戦闘に送り、報酬や利益を得るゲームを目指しています。

現在のUnityバージョンは `2022.3.62f3` です。主な作業フォルダは `Assets/Proiject` です。フォルダ名は `Project` ではなく `Proiject` になっています。

現在できている機能:
- 商人の所持金管理
- 傭兵データのScriptableObject
- 傭兵候補のランダム生成
- 傭兵の雇用
- 雇用済み傭兵から最大3人のパーティー編成
- シンプルな自動戦闘
- 勝利時のゴールド報酬
- ランタイム生成の簡易UGUI

重要な注意:
- 以前 `Assets/Proiject/Scripts/Merchant/NewBehaviourScript.cs` だったファイルは、現在 `Assets/Proiject/Scripts/Merchant/MerchantData.cs` にリネーム済みです。
- IDEのタブに `NewBehaviourScript.cs` が残っていても古い参照です。今後は `MerchantData.cs` を見ること。
- Gitは学校環境では読めませんでした。`git` コマンドがPATHになく、プロジェクト直下に `.git` フォルダもありませんでした。
- `dotnet build DungeonMerchant.sln` は成功しています。警告0、エラー0。

次にやりたいこと:
- Unity再生時にUIが確実に表示されるか確認
- Consoleに赤エラーが出ていたら、その全文をもとに修正
- その後、商人ローグライクらしい `ItemDataSO`、商人インベントリ、戦利品、売却/仕入れを追加したい

## 環境

- OS: Windows
- Shell: PowerShell
- Unity: `2022.3.62f3`
- Solution: `DungeonMerchant.sln`
- C# project: `Assembly-CSharp.csproj`
- Main scene: `Assets/Proiject/Scenes/SampleScene.unity`
- Main package examples:
  - `com.unity.ugui`
  - `com.unity.textmeshpro`
  - `com.unity.feature.2d`
  - `com.unity.test-framework`

## 主要ファイル

### Merchant

- `Assets/Proiject/Scripts/Merchant/MerchantData.cs`

役割:
- 所持金 `gold` を持つ
- `CanPay(int amount)`
- `TryPayGold(int amount)`
- `PayGold(int amount)`
- `AddGold(int amount)`
- `GoldChanged` イベント

最近の変更:
- `NewBehaviourScript.cs` から `MerchantData.cs` にリネーム
- 負数ガード追加
- 所持金変更イベント追加
- 支払い成功/失敗を `bool` で返せる `TryPayGold` 追加

### Mercenary

- `Assets/Proiject/Scripts/Mercenary/MercenaryHireManager.cs`
- `Assets/Proiject/Scripts/Mercenary/MercenaryPartyManager.cs`
- `Assets/Proiject/Scripts/Mercenary/MercenaryGenerator.cs`
- `Assets/Proiject/Scripts/Mercenary/MercenaryInstance.cs`

役割:
- `MercenaryHireManager`: 商人の所持金を使って傭兵を雇う
- `MercenaryPartyManager`: 雇用済み傭兵から最大3人のパーティーを作る
- `MercenaryGenerator`: 名前リストとアーキタイプから候補者を生成
- `MercenaryInstance`: 実体化した傭兵データ

### Battle

- `Assets/Proiject/Scripts/Battle/BattleManager.cs`
- `Assets/Proiject/Scripts/Battle/BattleUnit.cs`

現状の戦闘:
- パーティーメンバーを `BattleUnit` に変換
- 敵1体と戦う
- 味方が順番に敵を攻撃
- 敵は生きている先頭の味方を攻撃
- 勝利すると `enemyData.goldReward` を商人に加算
- `BattleMessage` と `BattleCompleted` イベントあり

未実装/弱い点:
- 複数敵
- スキル
- 行動順への `attackSpeed` 反映
- 傭兵HPの戦闘後持ち越し
- 負傷/死亡/治療
- ダンジョン階層や探索イベント
- アイテムドロップ

### UI

- `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`

役割:
- 再生時にCanvas、Panel、Tab、Button、Textをコードで生成
- `HIRE`, `COMPANY`, `PARTY`, `BATTLE` タブを表示
- 所持金表示
- 雇用、パーティー追加/解除、戦闘開始
- 戦闘ログ表示

最近の変更:
- `MerchantData.GoldChanged` を購読して所持金変更時にUI更新
- `LegacyRuntime.ttf` が取れない場合 `Arial.ttf` にフォールバック

## シーン状態

`Assets/Proiject/Scenes/SampleScene.unity` には以下がある:

- `Main Camera`
- `MerchantDate`
  - `MerchantData` が付いている
  - 名前は `MerchantDate` だが、これはおそらく `MerchantData` のタイポ
- `MercenaryHireManager`
  - `MercenaryHireManager`
  - `SimpleMercenaryHireUI`
  - `MercenaryPartyManager`
  - `MercenaryGenerator`
- `BattleManager`
  - `BattleManager`

最近の変更:
- `SimpleMercenaryHireUI.battleManager` に `BattleManager` を直接参照設定
- `BattleManager.partyManager` に `MercenaryPartyManager` を直接参照設定
- `BattleManager.merchantData` に `MerchantData` を直接参照設定

## ScriptableObject

主なアセット:

- `Assets/Proiject/ScriptableObjects/Mercenaries/MercenaryData.asset`
- `Assets/Proiject/ScriptableObjects/Mercenaries/WarriorArchetype.asset`
- `Assets/Proiject/ScriptableObjects/Mercenaries/MercenaryNames.txt`
- `Assets/Proiject/ScriptableObjects/Enemies/EnemyData.asset`

データ定義:

- `Assets/Proiject/Scripts/Data/MercenaryDstsSO.cs`
  - クラス名は `MercenaryDataSO`
  - ファイル名が `Dsts` になっていてタイポ気味
- `Assets/Proiject/Scripts/Data/EnemyDateSO.cs`
  - クラス名は `EnemyDataSO`
  - ファイル名が `Date` になっていてタイポ気味
- `Assets/Proiject/Scripts/Data/MercenaryArchetypeSO.cs`
- `Assets/Proiject/Scripts/Data/MercenaryClass.cs`
- `Assets/Proiject/Scripts/Data/MercenaryContractType.cs`

## 現在の確認結果

実行した確認:

```powershell
dotnet build DungeonMerchant.sln
```

結果:
- Build succeeded
- Warning: 0
- Error: 0

Git確認:

```powershell
Get-Command git
Test-Path .git
```

結果:
- `git` コマンドなし
- `.git` フォルダなし

## 家で再開するときの手順

1. Unity `2022.3.62f3` でプロジェクトを開く
2. `Assets/Proiject/Scenes/SampleScene.unity` を開く
3. Unityのコンパイル完了を待つ
4. ConsoleをClear
5. Playする
6. 画面中央に `MERCENARY GUILD` UIが出るか確認
7. 出ない場合はConsoleの赤エラー全文をチャットに貼る

## 次の実装候補

優先度高:
- UIが再生時に確実に表示されることの確認
- `MerchantDate` を `MerchantData` にリネーム
- `EnemyDateSO.cs` と `MercenaryDstsSO.cs` のファイル名タイポ修正

ゲーム性追加:
- `ItemDataSO`
- `MerchantInventory`
- 戦闘勝利時のアイテムドロップ
- アイテム売却で商人の利益になる仕組み
- ダンジョン階層/探索イベント
- 傭兵HPの持ち越し、治療費、死亡/負傷

## 直近で変更したファイル

- `Assets/Proiject/Scripts/Merchant/MerchantData.cs`
- `Assets/Proiject/Scripts/Merchant/MerchantData.cs.meta`
- `Assets/Proiject/Scripts/Mercenary/MercenaryHireManager.cs`
- `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`
- `Assets/Proiject/Scenes/SampleScene.unity`
- `Assembly-CSharp.csproj`

