# 学校側作業ログ

このファイルは、学校側Codexで行った作業を家のチャットへ共有するためのログです。

## 運用ルール

- 学校側Codexで作業した内容は、作業ごとに必ずここへ追記する。
- 家側へ共有したい注意点、未確認事項、次にやることもここへ書く。
- 家のチャットでは、このファイルと `HANDOFF_HOME.md` を読む。
- 学校側と家側の内容に食い違いがある場合は、家側の内容を優先する。

## 2026-06-16

### 共有運用ルールを追加

- ユーザーから「こちらで行った作業は必ず家と共有するファイルに作業するごとに記載すること」と指定あり。
- `handoff/SCHOOL_WORK_LOG.md` を新規作成。
- `handoff/README.md` に、学校側作業は作業ごとに `SCHOOL_WORK_LOG.md` へ追記する運用を追加。

### 共有時の優先ルールを追加

- ユーザーから「家と学校で共有するとき食い違いがある場合は家のほうを優先する」と指定あり。
- `handoff/README.md` と `handoff/SCHOOL_WORK_LOG.md` に、競合時は家側を優先するルールを追記。

### 共有プロジェクト状況ファイルを追加

- ユーザーから「家と学校両方が編集する全体状況を記載するファイル」を作るよう指定あり。
- `handoff/SHARED_PROJECT_STATUS.md` を新規作成。
- 家側と学校側の両方が編集する全体状況ファイルとして運用する。
- `handoff/README.md` に共有状況ファイルの説明を追記。

### 現在の実装状況を確認

- ユーザーから「ここからゲーム制作の作業をしていきます。現在の実装状況はどうなっていますか？」と質問あり。
- `handoff/SHARED_PROJECT_STATUS.md`、`handoff/SCHOOL_WORK_LOG.md`、主要ファイル一覧を確認。
- `dotnet build DungeonMerchant.sln` を実行し、警告0・エラー0で成功。
- 現状は、商人所持金、傭兵雇用、最大3人パーティー、敵1体との簡易自動戦闘、勝利時ゴールド報酬、ランタイム生成UGUIまで実装済み。
- 未実装の中心は、アイテム、商人インベントリ、仕入れ/売却、ダンジョン探索、複数敵、スキル、戦闘後HP持ち越し。

### シーン上のGameObject消滅に備えたUI自動生成を追加

- ユーザーから「家の環境で設置したGameObjectが消滅しているのでUIが表示されません」と報告あり。
- `Assets/Proiject/Scripts/Core/DungeonMerchantBootstrap.cs` を新規追加。
- シーンに `SimpleMercenaryHireUI` が無い場合、再生時に `DungeonMerchant Runtime` GameObjectを作成し、`MerchantData`、`MercenaryHireManager`、`MercenaryPartyManager`、`MercenaryGenerator`、`BattleManager`、`SimpleMercenaryHireUI` を自動追加するようにした。
- `MercenaryHireManager` と `MercenaryPartyManager` に参照自動解決を追加。
- `SimpleMercenaryHireUI` が同じGameObject上の `MerchantData` も探すように修正。
- `MercenaryGenerator` に、名前リストやアーキタイプ参照が無い場合でも開発用の仮傭兵候補を生成するフォールバックを追加。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

### 個別SO傭兵が表示されない問題を修正

- ユーザーから「量産型傭兵の表示はうまくいっているが、固有の名前を持つ個別SOの傭兵が表示できていない」と報告あり。
- 原因は、ランタイム自動生成された `SimpleMercenaryHireUI` にはシーン上で設定していた `candidates` 参照が入らないため、個別SO候補が空になること。
- `SimpleMercenaryHireUI` に `PopulateUniqueCandidatesIfNeeded` を追加。
- `candidates` が空の場合、`Resources.LoadAll<MercenaryDataSO>` と、Editor中は `AssetDatabase.FindAssets("t:MercenaryDataSO", "Assets/Proiject/ScriptableObjects/Mercenaries")` で個別SOを自動収集するようにした。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

### アイテム系の最小システムを追加

- ユーザーから「アイテム系のシステム制作に入ります」と指定あり。
- `Assets/Proiject/Scripts/Item/ItemType.cs` を追加。
- `Assets/Proiject/Scripts/Item/ItemRarity.cs` を追加。
- `Assets/Proiject/Scripts/Item/ItemDataSO.cs` を追加。
- `Assets/Proiject/Scripts/Item/InventoryItemStack.cs` を追加。
- `Assets/Proiject/Scripts/Item/MerchantInventory.cs` を追加。
- `EnemyDataSO` に `ItemDropEntry[] itemDrops` を追加。
- `BattleManager` に `MerchantInventory` 参照、勝利時のアイテム報酬処理、アイテムSO自動検出、仮ドロップ生成を追加。
- `DungeonMerchantBootstrap` が `MerchantInventory` も自動追加するようにした。
- `SimpleMercenaryHireUI` に `INVENTORY` タブ、在庫一覧、売却ボタンを追加。
- サンプルアイテム `Assets/Proiject/ScriptableObjects/Items/MonsterFang.asset` を追加。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

### スライム敵データの再生時自動設定を追加

- ユーザーから「スライムのデータも再生時に自動で入れるようにしてください」と指定あり。
- `BattleManager.ResolveReferences` で `enemyData` が未設定の場合、敵SOを自動検出するようにした。
- `Resources.LoadAll<EnemyDataSO>` と、Editor中は `AssetDatabase.FindAssets("t:EnemyDataSO", "Assets/Proiject/ScriptableObjects/Enemies")` から敵データを探す。
- 敵SOが見つからない場合は、ランタイムで `Slime` の `EnemyDataSO` を生成する。
- 既存の `Assets/Proiject/ScriptableObjects/Enemies/EnemyData.asset` に `itemDrops: []` を追記し、現在の `EnemyDataSO` 定義と整合させた。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

### ItemDropEntry のUnityインポートエラーを修正

- ユーザーから `'ItemDropEntry' is missing the class attribute 'ExtensionOfNativeClass'` と `The class named 'ItemDropEntry' is not derived from MonoBehaviour or ScriptableObject` のUnity Consoleエラー報告あり。
- 原因は `EnemyDateSO.cs` の先頭に `[System.Serializable] public class ItemDropEntry` を置いたため、Unityがこの `.cs` ファイルの代表クラスを `EnemyDataSO` ではなく `ItemDropEntry` と誤認したこと。
- `EnemyDateSO.cs` の先頭を `EnemyDataSO : ScriptableObject` に戻し、`ItemDropEntry` はファイル末尾へ移動。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

### 戦闘報酬のゴールドとアイテムが反映されない問題を修正

- ユーザーから「ゴールドの数値が変化しない、アイテムがドロップしない」と報告あり。
- 原因は、UIが `BattleManager.StartBattle(partyManager.Members)` を呼んでいる一方、この overload 側で `ResolveReferences()` を実行していなかったこと。
- そのため、`merchantData` や `merchantInventory` が未解決のまま戦闘完了し、勝利報酬が反映されない可能性があった。
- `StartBattle(IReadOnlyList<MercenaryInstance> partyMembers)` の冒頭で `ResolveReferences()` を呼ぶよう修正。
- `CompleteBattle` と `GrantItemRewards` でも念のため `ResolveReferences()` を呼ぶよう修正。
- `merchantInventory` が見つからない場合は戦闘ログに `No merchant inventory is assigned.` を出すようにした。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

### COMPANY表示切れと固有傭兵のHIRE残留を修正

- ユーザーから「companyはキャラクターが4人以上になると表示が見切れる。固有の傭兵は雇ってもHIREから消えない」と報告あり。
- `SimpleMercenaryHireUI` の `COMPANY` ページを `ScrollRect` 化し、4人以上でもスクロールして確認できるようにした。
- `RebuildCompanyList` で雇用済み傭兵数に応じてリスト高さを更新するようにした。
- `RebuildHireList` で `hiredCandidates` に入っている固有SO傭兵を表示しないようにした。
- 固有SO傭兵を雇用した直後に `RebuildHireList()` を呼び、HIRE一覧から即時消えるようにした。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

### 複数対複数戦闘を実装

- ユーザーから「先に複数対複数の戦闘を実装したい」と指定あり。
- `BattleManager` を敵1体前提から、複数の敵 `BattleUnit` を持つ形に変更。
- `enemyPartyData` を追加し、複数の `EnemyDataSO` を敵グループとして設定できるようにした。
- 敵グループが未設定の場合は、既存の `enemyData` を `fallbackEnemyCount` 分だけ複製して戦闘に出す。初期値は3体。
- 味方は生きている先頭の敵を攻撃し、敵側は生きている敵全員が順番に味方を攻撃する。
- 敵全滅で勝利、味方全滅で敗北。
- 勝利報酬ゴールドは出現した敵全員分の合計に変更。
- アイテムドロップは出現した敵データごとに判定するよう変更。
- `SimpleMercenaryHireUI` の戦闘説明を `BattleManager.GetEncounterDescription()` から取得するよう変更し、複数敵時に `Slime x3` のように表示できるようにした。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

### バトルログの表示限界と色分けに対応

- ユーザーから「バトルログが表示限界を超えてしまう。ログの文字を味方は青、敵は赤で表示できるか」と要望あり。
- `Assets/Proiject/Scripts/Battle/BattleLogType.cs` を追加。
- `BattleUnit` に `IsPlayerSide` を追加し、攻撃者が味方側か敵側か判定できるようにした。
- `BattleManager` に `BattleMessageTyped` イベントを追加し、ログ本文と `BattleLogType` をUIへ渡すようにした。
- 味方攻撃ログは青、敵攻撃ログは赤、報酬ログは緑で表示。
- `SimpleMercenaryHireUI` のバトルログを最大14行に制限し、古いログから削除するようにした。
- Unity UI Text の rich text を有効化。
- `Assembly-CSharp.csproj` に `BattleLogType.cs` を追加。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

### バトルログを全件見返せるように変更

- ユーザーから「ログはすべて表示で見返せる形にしてください」と要望あり。
- `SimpleMercenaryHireUI` のBATTLEページを `RECENT` と `FULL LOG` の2領域に分割。
- `RECENT` は最新6行のみ表示して、戦闘中に画面が溢れにくいようにした。
- `FULL LOG` は戦闘中の全ログを保持し、スクロールで見返せるようにした。
- 味方攻撃の青、敵攻撃の赤、報酬の緑表示は維持。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

### バトルログを単一スクロール表示へ変更

- ユーザーから「フルログは表示されていない。分ける必要はなく、通常のログをスクロールで見れる形でいい。ログの消去はなし」と要望あり。
- `SimpleMercenaryHireUI` の `RECENT` / `FULL LOG` 分割を廃止。
- BATTLEページのログを1つの `ScrollRect` に戻した。
- ログ行は削除せず、`battleLogLines` に全件保持する。
- 行数に応じてログContentの高さを伸ばし、スクロールで全件見返せるようにした。
- 味方攻撃の青、敵攻撃の赤、報酬の緑表示は維持。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

### バトルログが一切出ない問題を修正

- ユーザーから「ログが一切出なくなりました」と報告あり。
- 原因は、単一スクロール表示へ戻した際に `battleLogText` のRectTransformを上寄せアンカーにしたまま `offsetMin` / `offsetMax` を0にし、文字描画領域の高さが0になっていたこと。
- `battleLogText.rectTransform.anchorMin = Vector2.zero`、`anchorMax = Vector2.one` にして、ログContent全体へストレッチするよう修正。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。
