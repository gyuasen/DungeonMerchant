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

## 2026-06-17

### 家側作業内容を確認

- ユーザーから「家での作業を共有してください」と指定あり。
- `handoff/FROM_HOME_CHAT.md` を確認。
- 家側で `.instructions.md`、`handoff/README.md`、`handoff/FROM_HOME_CHAT.md` の共有ルール追記が行われていた。
- 家側で `DayManager`、`MarketPriceManager`、`MarketStockManager`、`MarketStockEntry` が追加されていた。
- 家側で `MerchantInventory`、`SimpleMercenaryHireUI`、`DungeonMerchantBootstrap` が日数、市場価格、仕入れUIに対応していた。
- `handoff/SHARED_PROJECT_STATUS.md` には、日数管理、市場価格変動、`MARKET` タブ、仕入れ購入が実装済みとして反映済みだった。
- 対象ファイルが学校側環境にも存在することを確認。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

### 傭兵HPの戦闘後持ち越しを実装

- ユーザーから「HP変更を作成してください」と指定あり。
- `MercenaryInstance` に `SetCurrentHP`、`TakeDamage`、`Heal`、`RestoreFullHP`、`IsIncapacitated` を追加。
- `BattleUnit` のコンストラクタに開始時の `currentHP` を渡せるように変更。
- `BattleManager` が戦闘参加した `MercenaryInstance` と `BattleUnit` の対応を保持するようにした。
- 戦闘開始時、傭兵の `CurrentHP` を `BattleUnit` にコピーするようにした。
- HPが0の傭兵は戦闘参加ユニットに入れないようにした。
- 戦闘終了時、各 `BattleUnit.CurrentHP` を元の `MercenaryInstance` に書き戻すようにした。
- 戦闘ログに `HP carried over` を出し、持ち越し後HPを確認できるようにした。
- `SimpleMercenaryHireUI` は戦闘完了時に `COMPANY` / `PARTY` リストを再構築し、HP表示を更新するようにした。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

### 連続戦闘ダンジョンの最小実装を追加

- ユーザーから「連続戦闘のダンジョンの実装をします」と指定あり。
- `Assets/Proiject/Scripts/Dungeon/DungeonRunManager.cs` を新規追加。
- `DungeonRunManager` は3回の遭遇を順番に開始し、勝利時は次の遭遇、敗北時はラン終了にする。
- 遭遇ごとに敵数が増える設定を追加。初期設定は2体、次が3体、次が4体。
- `BattleManager` に今回だけの敵編成を渡せる `StartBattle(partyMembers, enemyEncounter)` を追加。
- `BattleManager.CreateDefaultEnemyEncounter(enemyCount)` を追加し、ダンジョン側がスライム複数体の遭遇を作れるようにした。
- `DungeonMerchantBootstrap` が `DungeonRunManager` を自動生成するようにした。
- `SimpleMercenaryHireUI` に `DUNGEON` タブを追加。
- `DUNGEON` タブの `ENTER` から連続戦闘を開始できるようにした。
- タブ数増加に合わせてナビゲーションボタン幅と位置を調整し、1列に収まるようにした。
- `Assembly-CSharp.csproj` に `DungeonRunManager.cs` を追加。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

### バトルログを最新行へ自動スクロールするように変更

- ユーザーから「バトルログを一番最新のものへ自動でスクロールできるか」と要望あり。
- `SimpleMercenaryHireUI` に `battleLogScrollRect` を保持するフィールドを追加。
- ログ追加後に `ScrollBattleLogToLatest()` を呼び、1フレーム待ってから `verticalNormalizedPosition = 0f` にするようにした。
- Unity UIのレイアウト反映前にスクロール位置を動かして失敗しないよう、コルーチンで `Canvas.ForceUpdateCanvases()` 後に最下部へ移動する。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

### バトルログ自動スクロール時の見切れを修正

- ユーザーから「途中で上に行きすぎてログが見切れていた」と報告あり。
- 原因はログContent高さを `行数 * 22` の概算で決めており、実際のText描画高さとズレていたこと。
- `SimpleMercenaryHireUI` に `UpdateBattleLogContentHeight()` を追加。
- `battleLogText.preferredHeight + 32f` を使って、実際のログ文字量に合わせてContent高さを更新するようにした。
- ログTextに上下8pxの余白を追加。
- 自動スクロール前にもContent高さを再計算するようにした。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

### 治療システムを実装

- ユーザーから「治療のシステムを作ります」と指定あり。
- `Assets/Proiject/Scripts/Mercenary/HealingManager.cs` を新規追加。
- `HealingManager` は治療費、自然回復、治療処理を管理する。
- 治療費は `healCostPerHP = 2`。失ったHPぶんだけ費用が増える。
- `TryHealFull` で商人のゴールドを支払い、対象傭兵を全回復する。
- `DayManager.DayChanged` を購読し、日付進行時に雇用済み傭兵を `naturalHealPerDay = 10` 回復する。
- `DungeonMerchantBootstrap` が `HealingManager` を自動生成するようにした。
- `SimpleMercenaryHireUI` に `HEAL` タブを追加。
- `HEAL` タブで雇用済み傭兵のHP、失ったHP、全回復費用を表示し、`HEAL` ボタンで治療できるようにした。
- 戦闘後、日付進行後、ゴールド変化後に治療一覧が更新されるようにした。
- タブ数増加に合わせてナビゲーション配置を調整した。
- `Assembly-CSharp.csproj` に `HealingManager.cs` を追加。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

## 2026-06-19

### 家側の最新作業内容を確認

- ユーザーから「家での作業内容を確認してください」と指定あり。
- `handoff/FROM_HOME_CHAT.md`、`handoff/SHARED_PROJECT_STATUS.md`、`.instructions.md` を確認。
- 家側優先ルールに従い、2026-06-19の家側変更を最新状態として扱う。
- ダンジョン戦闘間のランダムイベントと3択行動が追加されている。
- `SaveManager` と `GameSaveData` によるJSONセーブ・ロード、自動保存が追加されている。
- ダンジョン5等級、段階開放、最高開放等級の保存が追加されている。
- モンスター1〜10等級、通常敵10種、ボス5種、最終戦ボス編成が追加されている。
- アイテムが合計15種へ拡張され、全敵に正式なドロップテーブルが設定されている。
- 傭兵名を除くプレイヤー向け表示が日本語化され、`JapaneseDisplayText` が追加されている。
- 学校側環境に敵SO 15個、アイテムSO 15個、ダンジョンSO 5個が存在することを確認。
- `Assets/Proiject/Scripts/Core/SaveManager.cs`、`GameSaveData.cs`、`DungeonDataSO.cs`、`JapaneseDisplayText.cs` などの追加ファイルが存在することを確認。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

### 次の作業候補を整理

- ユーザーから次に行う作業候補の提示依頼あり。
- 優先候補として、装備システム、傭兵成長、契約・維持費、戦闘スキル、経営バランス調整、セーブ/UI検証を提示。
- 現在のアイテム収集、傭兵育成、ダンジョン攻略を接続できるため、次は装備システムを推奨。

### 傭兵経験値・レベルアップシステムを実装

- ユーザーから「全体のキャラクターに経験値システムを制作したい」と指定あり。
- 全 `MercenaryInstance` に `currentExperience`、`CurrentExperience`、`ExperienceToNextLevel` を追加。
- レベル1の必要経験値は100で、レベルごとに50ずつ増加。レベル上限は99。
- `AddExperience` とレベルアップ処理を追加。
- 戦士はHP・防御、弓兵は攻撃・攻撃速度、魔法使いは攻撃が伸びやすい職業別成長を追加。
- レベルアップ時、生存中の傭兵は最大HP増加分だけ現在HPも増える。戦闘不能者はHP0を維持する。
- `BattleManager` が勝利時に敵等級から経験値を計算し、戦闘参加傭兵へ均等配分するようにした。
- 敵等級10は10経験値、敵等級1は100経験値を基準とし、ボスは2倍。
- 経験値獲得とレベルアップを報酬色の戦闘ログへ表示。
- `COMPANY` と `PARTY` に現在経験値/次レベル必要経験値を表示。
- `GameSaveData` のバージョンを2へ更新し、`SavedMercenary.currentExperience` を追加。
- `SaveManager` が経験値を保存・復元するようにした。旧セーブは経験値0として読込可能。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

### 弓兵・魔法使い生成と累進経験値テーブルへ変更

- ユーザーから「戦士しか生成されないため魔法使いや弓兵も生成し、敵経験値と必要経験値を累進式にしたい」と指定あり。
- 文脈上「給付塀」は弓兵として対応。
- `ArcherArchetype.asset` と `MageArchetype.asset` を追加。
- 弓兵は速度・攻撃寄り、魔法使いは攻撃寄りの基礎能力に設定。
- `SampleScene.unity` の `MercenaryGenerator.archetypes` に戦士・弓兵・魔法使いの3SOを登録。
- `MercenaryGenerator` が不足職をランタイムアーキタイプで補完するようにし、シーン参照消失時も3職を生成可能にした。
- 候補数が3人以上の場合、戦士・弓兵・魔法使いを最低1人ずつ含め、残り枠をランダム生成して表示順をシャッフルするようにした。
- 必要経験値を線形式から `100 + 40n + 10n²`（n = 現在レベル - 1）の累進式へ変更。
- 敵経験値を `(11 - 敵等級) * 10` から `10 * (11 - 敵等級)²` へ変更。
- ボス経験値2倍は維持。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

### 戦闘不能治療費・ログ初期表示・傭兵詳細UIを改善

- ユーザーから、戦闘不能治療費の高額化、戦闘開始時ログ見切れの修正、詳細ステータスUIの追加依頼あり。
- `HealingManager` に `incapacitatedCostMultiplier = 5` と `revivalBaseCost = 500` を追加。
- 戦闘不能者の治療費を `通常治療費 × 5 + 500G` に変更。
- HP0の傭兵は日付進行による自然回復の対象外とし、有料治療でのみ復帰するようにした。
- HEAL画面に戦闘不能表示と高額治療ルールを表示。
- バトルログの固定最小高さ430pxを廃止し、実際のViewport高さを最小Content高さに使用。
- 戦闘・ダンジョン開始時にログContent位置とスクロール位置を先頭へ明示的にリセットするようにした。
- `COMPANY` の各傭兵に `詳細` ボタンを追加。
- 詳細モーダルで種別、ID、職業、契約、状態、レベル、経験値、HP、攻撃、防御、攻撃速度、雇用費を表示。
- パーティー追加/解除ボタンは維持し、詳細ボタンと横並びに配置。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

### 各職の初期装備と上位装備を追加

- ユーザーから装備システム制作開始として、各職の初期装備と一段上の装備作成依頼あり。
- 装備変更UIは、上部タブを増やさず傭兵確認から直接操作できるため、既存の詳細画面へ組み込む方針を採用。
- `EquipmentSlot` enumを追加。武器、防具、装飾品を定義。
- `ItemDataSO` に装備スロット、必要職業、装備ランク、HP・攻撃・防御・攻撃速度補正を追加。
- `ItemDataSO.IsEquipment` と `CanEquip` を追加。
- 戦士用に `鉄の剣`（ランク1）と `鋼の剣`（ランク2）を追加。
- 弓兵用に `ショートボウ`（ランク1）と `複合弓`（ランク2）を追加。
- 魔術師用に `見習いの杖`（ランク1）と `秘術の杖`（ランク2）を追加。
- `JapaneseDisplayText` に装備名と装備スロットの日本語表示を追加。
- 現時点は装備SOとデータ定義まで。着脱、能力反映、セーブ、詳細画面操作は次工程。
- `Assembly-CSharp.csproj` に `EquipmentSlot.cs` を追加。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

### 市場の購入商品をモンスター素材から武器へ変更

- ユーザーから、現在購入可能なモンスター素材を武器に変更するよう指定あり。
- `MarketStockManager` の仕入れ候補を `ItemType.Equipment` かつ `EquipmentSlot.Weapon` のアイテムだけに限定。
- シーンに以前保存されていた素材候補も `RemoveInvalidItems` で自動除外するようにした。
- 市場の武器在庫数を1〜2個に変更。
- 装備アセットが見つからない場合のフォールバック商品を交易素材から鉄の剣へ変更。
- フォールバック武器の数量を1個、価格を基準価格から計算するよう修正。
- MARKET一覧に対応職、武器ランク、HP・攻撃・防御補正を表示するようにした。
- モンスター素材はダンジョン戦利品・売却品として維持し、市場購入候補からのみ除外。

### 鍛冶屋・ハクスラ要素の設計方針

- ユーザーから、モンスター素材とゴールドで市場では購入できない武器を制作する鍛冶屋と、ハクスラ要素を追加したい意向あり。
- 市場は固定能力の既製武器、鍛冶屋は素材を使う限定武器とランダム性能武器を扱う役割分担を推奨。
- `EquipmentRecipeSO` で必要素材、必要ゴールド、対応職、完成装備、必要ダンジョン等級を設定する方針。
- 初期段階は固定レシピで確実に制作し、その後に接頭辞・接尾辞、品質、ランダム補正を追加する段階実装を推奨。
- 制作装備は市場購入不可とし、鍛冶屋レシピまたはダンジョンドロップ限定にする。

### 固定レシピ式の鍛冶屋を実装

- ユーザーから鍛冶屋を提案どおり実装するよう指定あり。
- `ItemAcquisitionType` を追加し、市場品・鍛冶品・ダンジョン品を区別できるようにした。
- `CraftingMaterialRequirement` と `EquipmentRecipeSO` を追加。
- `BlacksmithManager` を追加し、レシピ自動検出、制作可否判定、ゴールド支払い、素材消費、完成武器追加を実装。
- `MerchantInventory` に所持数取得、素材所持判定、複数素材の一括消費を追加。
- `DungeonMerchantBootstrap` が `BlacksmithManager` を自動生成するようにした。
- `鍛冶` タブを追加し、完成武器の職業・ランク・能力補正、必要素材の所持数、必要ゴールドを表示。
- 制作可能な場合のみ制作ボタンを有効化。
- 戦士用ランク3 `ゴブリン狩りの剣` を追加。ゴブリンの耳5、魔物の牙3、300Gで制作。
- 弓兵用ランク3 `獣骨の弓` を追加。コウモリの翼5、オークの牙2、340Gで制作。
- 魔術師用ランク3 `呪木の杖` を追加。呪われた骨4、闇の結晶3、380Gで制作。
- 3武器は `ItemAcquisitionType.Blacksmith` のため市場購入候補から除外。
- 上部タブ9個が1列に収まるようナビゲーション幅と位置を調整。
- `Assembly-CSharp.csproj` に鍛冶関連スクリプトを追加。
- 現段階は固定性能の鍛冶限定武器。ランダム品質・接辞・個体装備化は装備着脱実装後の次段階。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。
## 2026-06-19

### Character detail equipment controls implemented
- Investigated the report that equipment changes from the character detail screen were not working.
- Confirmed that the existing detail modal only displayed statistics and had no equip/unequip implementation.
- Added equipped weapon state to `MercenaryInstance`.
- Equipment bonuses now affect max HP, attack, defense, attack speed, and battle unit creation through the existing stat properties.
- Added inventory item removal and weapon swap behavior. The previous weapon returns to inventory.
- Added compatible owned weapon rows, equip buttons, and an unequip button to the character detail modal.
- Added equipped weapon persistence to `GameSaveData` and `SaveManager`.
- Save data now stores base stats separately, preventing equipment bonuses from being applied twice after loading.
- Preserved HP correctly when loading an HP-boosting weapon.
- `dotnet build DungeonMerchant.sln` completed with 0 warnings and 0 errors.
- Rollback: revert the 2026-06-19 equipment changes in `MercenaryInstance`, `MerchantInventory`, `GameSaveData`, `SaveManager`, and `SimpleMercenaryHireUI`.
## 2026-06-19

### Equipment comparison and detail UI readability improved
- Added stat comparison between each owned weapon and the currently equipped weapon.
- Comparison values use green for increases, red for decreases, and gray for no change.
- Comparison covers max HP, attack, defense, and attack speed.
- Enlarged the character detail modal and separated character stats from equipment controls more clearly.
- Increased equipment row height and font size so weapon information is no longer compressed.
- Added a masked scrollable equipment viewport for characters with many compatible weapons.
- Added the currently equipped weapon name to the character stat area.
- `dotnet build DungeonMerchant.sln` completed with 0 warnings and 0 errors.
- Rollback: revert the equipment comparison and layout changes in `SimpleMercenaryHireUI.cs`.
## 2026-06-19

### Equipped weapon save timing reinforced
- Confirmed that `GameSaveData` stores `equippedWeaponAssetName` for each mercenary.
- Confirmed that `SaveManager` restores the weapon SO and reapplies saved HP after equipment restoration.
- Added a `SaveManager` reference to `SimpleMercenaryHireUI`.
- Weapon equip, swap, and unequip now explicitly call `SaveGame()` after inventory and character state changes are complete.
- This removes reliance on the intermediate inventory event timing and saves the final equipment state immediately.
- `dotnet build DungeonMerchant.sln` completed with 0 warnings and 0 errors.
- Rollback: remove the `SaveManager` reference and `SaveEquipmentChanges()` calls from `SimpleMercenaryHireUI.cs`.
## 2026-06-24

### 施設画面から町マップへ戻る導線を追加
- ユーザーから、町施設をクリックした後の遷移先が全体マップだけになっていると報告あり。
- 既存の `全体マップ` ボタンの横に `町マップ` ヘッダーボタンを追加。
- 通常の施設画面では `全体マップ` と `町マップ` の両方を表示し、現在の町マップへ直接戻れるようにした。
- マップ画面では追加した `町マップ` ボタンを非表示にし、既存のマップ内ナビゲーションを維持。
- 新しいボタンと重ならないように、日数表示の位置と幅を調整。
- `dotnet build DungeonMerchant.sln` は最初、読み取り専用サンドボックスのためMSBuildが一時ファイルを作れず失敗。
- 同じビルドを承認付きで再実行し、警告0件、エラー0件で成功。
- 戻す場合は `townMapButton`、`BuildUI` での生成、`SetMapHeaderButtons`、`ShowGlobalMap` / `ShowWorldMap` / `ShowTownMap` / `HideMapPages` の関連呼び出しを戻す。
## 2026-06-24

### 戦闘スキルと戦闘用魔力を実装
- ユーザーから、スキルの実装とスキル発動に必要な魔力の実装依頼あり。
- `BattleUnit` に戦闘中だけ使う魔力を追加。
- プレイヤー側の戦闘ユニットは少量の魔力を持って戦闘を開始し、行動開始時に魔力を `20` 回復する。
- `MercenaryInstance` に最大魔力を追加。
  - 戦士: `60 + level * 2`
  - 弓兵: `75 + level * 2`
  - 魔法使い: `100 + level * 3`
- 魔力が足りている場合、職業ごとのスキルを自動発動する処理を追加。
  - 戦士: `挑発の一撃`、消費35、低威力攻撃と敵の攻撃引きつけ。
  - 弓兵: `連射`、消費45、威力を落とした2回攻撃。
  - 魔法使い: `火球`、消費50、単体へ高威力攻撃。
- 挑発中の戦士がいる場合、敵のターゲット選択で優先されるようにした。
- 戦闘ログにスキル名、ダメージ、残り魔力を表示。
- 傭兵詳細UIに最大魔力と各傭兵の戦闘スキル情報を表示。
- 現在魔力は戦闘専用で、戦闘間では保存しない方針。
- `dotnet build DungeonMerchant.sln` は警告0件、エラー0件で成功。
- 戻す場合は `BattleUnit.cs`、`BattleManager.cs`、`MercenaryInstance.cs`、`SimpleMercenaryHireUI.cs` のスキル表示追加を戻す。
## 2026-06-24

### 傭兵とモンスターに魔力と行動速度ステータスを追加
- ユーザーから、傭兵とモンスターの両方に魔力と行動順を決める速度ステータスを追加する依頼あり。
- `MercenaryDataSO`、`MercenaryArchetypeSO`、`MercenaryInstance`、`EnemyDataSO`、セーブデータに `maxMagicPower` を追加。
- 既存の `attackSpeed` を、戦闘中の行動順を決める速度ステータスとして使用。
- 戦闘順は「味方全員の後に敵全員」ではなく、生存している味方と敵を毎ラウンド速度順で並べるように変更。
- 魔力回復量は行動速度に応じて増え、速いユニットほど魔力がたまりやすくなった。
- 先に実装した戦闘スキルは、傭兵のステータス上の `MaxMagicPower` を使うように変更。
- 弓兵は生成時に速度ボーナスを得る。
- 魔法使いは生成時に最大魔力ボーナスを得て、レベルアップ時にも追加で魔力が伸びる。
- 古いセーブで魔力ステータスがない場合は、職業に応じて補完する。
- 敵SOアセットに `maxMagicPower` の値を明示的に追加。
- 傭兵アーキタイプと固有傭兵アセットにも魔力の値を明示的に追加。
- 傭兵詳細UIでは速度ステータスを `行動速度` と表示。
- セーブバージョンを13へ更新。
- `dotnet build DungeonMerchant.sln` は警告0件、エラー0件で成功。
- 戻す場合は `EnemyDateSO.cs`、`MercenaryDstsSO.cs`、`MercenaryArchetypeSO.cs`、`MercenaryInstance.cs`、`MercenaryGenerator.cs`、`BattleManager.cs`、`GameSaveData.cs`、`SaveManager.cs`、`SimpleMercenaryHireUI.cs`、編集した敵/傭兵SOアセットを戻す。
## 2026-06-24

### フロア日数、スキルAI、挑発状態の挙動を調整
- ユーザーから、ダンジョン1フロアの探索日数を1日にする依頼あり。
- 探索完了時の処理を変更し、イベント遅延日数に関係なくフロア探索は必ず `1` 日だけ進むようにした。
- ユーザーから、通常攻撃で倒せる場合や過剰攻撃になる場合など、無駄になる場面ではスキルを使わないようにする依頼あり。
- スキル発動前にランダム発動率と有用性チェックを追加。
- 現在のターゲットを通常攻撃で倒せる場合、スキルは使わないようにした。
- ダメージスキルは有効な対象を探し、大きな過剰攻撃を避けるようにした。
- 弓兵の `連射` は硬い単体相手には使えるが、通常攻撃で倒せる対象へ無駄撃ちしない。
- ユーザー指摘により、`挑発` は攻撃スキルではない扱いへ変更。
- 戦士の `挑発` をダメージ攻撃から、自分に付与する状態効果へ変更。
- `BattleUnit` に挑発ターン、防御バフターン、防御バフ量、実効防御、状態概要の戦闘状態フィールドを追加。
- 敵のターゲット選択は、挑発中のユニットを引き続き優先する。
- この時点では挑発はダメージを与えず、敵の攻撃を引きつけ、一時的な防御ボーナスを得る効果にした。
- `dotnet build DungeonMerchant.sln` は警告0件、エラー0件で成功。
- 戻す場合は `ProgressionManager.cs`、`BattleUnit.cs`、`BattleManager.cs` の変更を戻す。
## 2026-06-24

### 挑発の防御アップ削除とダンジョンイベント文言の調整
- ユーザー要望により、挑発による防御上昇を削除。
- `BattleUnit` の挑発状態は敵の狙われやすさだけに影響し、防御力は変化しない。
- `BattleManager` の戦士挑発ログから防御アップの表記を削除。
- ダンジョンイベントの説明文と選択肢に、時間消費と「フロア探索は合計1日として処理される」内容を追記。
- `dotnet build DungeonMerchant.sln` は警告0件、エラー0件で成功。
- 戻す場合は `BattleUnit.cs`、`BattleManager.cs`、`DungeonRunManager.cs` の変更を戻す。
## 2026-06-24

### 今日分の共有記録を日本語表記へ修正
- ユーザー要望により、今日行った変更の共有記録で英語のまま残っていた部分を日本語へ修正。
- `handoff/SCHOOL_WORK_LOG.md` の2026-06-24分にある、町マップ導線、戦闘スキル、魔力/行動速度、フロア日数/スキルAIの記録を日本語化。
- `handoff/SHARED_PROJECT_STATUS.md` の2026-06-24分にある学校側更新を日本語化。
- コード変更はなく、共有記録のみの修正のためビルドは未実行。
- 戻す場合は、この節の直前に行った `handoff/SCHOOL_WORK_LOG.md` と `handoff/SHARED_PROJECT_STATUS.md` の文言修正を戻す。

## 2026-06-30

### 等級10～6の通常モンスターを追加

- ユーザー要望により、等級10から6までの通常モンスターを各1種、合計5種追加。
- 10等級「グリーンスライム」、9等級「コボルト」、8等級「大ネズミ」、7等級「ゾンビ」、6等級「リザードマン」を追加。
- 各モンスターには同等級の既存敵と同程度になるよう、HP・攻撃・防御・魔力・行動速度・クリティカル率・回避率・報酬を設定。
- 新規モンスターは同等級の既存素材をドロップするため、新しい素材や在庫セーブ形式への変更はなし。
- グリーンスライムとコボルトを「はじまりの洞窟」、大ネズミとゾンビを「封じられた廃坑」、リザードマンを「霧の古代遺跡」の通常遭遇候補へ追加。
- `JapaneseDisplayText` に5種の日本語表示名を追加。
- 既存モンスターは削除せず、遭遇候補の種類を増やす変更。
- `dotnet build DungeonMerchant.sln` は警告0件、エラー0件で成功。
- 戻す場合は、追加した敵SOと`.meta`を削除し、3ダンジョンの`normalEnemies`追加分と`JapaneseDisplayText`の5件を戻す。

### 等級10～6の通常モンスターをさらに追加

- ユーザー要望により、等級10から6までの通常モンスターをさらに各1種、合計5種追加。
- 10等級「ブルースライム」、9等級「ゴブリン斥候」、8等級「洞窟グモ」、7等級「装甲スケルトン」、6等級「ホブゴブリン」を追加。
- ブルースライムとゴブリン斥候は「はじまりの洞窟」、洞窟グモと装甲スケルトンは「封じられた廃坑」、ホブゴブリンは「霧の古代遺跡」の通常遭遇候補へ追加。
- 同じ等級内でも能力傾向が変わるよう、速度・回避・防御などを調整。
- 洞窟グモには「毒牙」、装甲スケルトンとホブゴブリンには「強撃」を個別設定。
- 新規モンスターは同等級の既存素材をドロップし、UIでは日本語名を表示。
- `dotnet build DungeonMerchant.sln` は警告0件、エラー0件で成功。
- 戻す場合は、今回追加した敵SOと`.meta`を削除し、3ダンジョンの`normalEnemies`追加分と`JapaneseDisplayText`の5件を戻す。

### 上位等級の町へ低等級ダンジョンを追加

- ユーザー要望により、下級・中級ダンジョンがある町でも、それ以前の等級のダンジョンへ挑戦できるようにした。
- 下級ダンジョンがあるリーフ森林都市へ、低級「リーフ樹海の獣道」を追加。
- 中級ダンジョンがあるエルド交易都市へ、低級「エルド地下水路」と下級「エルド旧採石場」を追加。
- リーフ森林都市では低級と下級、エルド交易都市では低級・下級・中級のダンジョンを選択可能。
- 新規ダンジョンは既存の等級別通常敵・ボス・素材報酬・限定装備候補を利用し、攻略状況はアセットごとに独立して保存される。
- ビルド後も自動検出されるよう、新規ダンジョンSOは`Assets/Proiject/Resources/Dungeons`へ配置。
- 既存の町別一覧UIと等級開放処理が複数ダンジョンに対応していることを確認し、コード変更なしで追加。
- `dotnet build DungeonMerchant.sln` は警告0件、エラー0件で成功。
- 戻す場合は、`Assets/Proiject/Resources/Dungeons`へ追加した3つのダンジョンSOと各`.meta`、不要ならフォルダーとフォルダー`.meta`を削除する。

### 中級までの通常モンスターを各等級2種追加

- ユーザー要望により、低級・下級・中級ダンジョンの出現モンスターに幅を持たせるため、等級10～5へ各2種、合計12種の通常敵を追加。
- 10等級に「苔スライム」「角ウサギ」、9等級に「野犬」「ゴブリン槍兵」を追加。
- 8等級に「毒蛾」「岩甲虫」、7等級に「レイス」「骨猟犬」を追加。
- 6等級に「トロル」「沼トカゲ」、5等級に「アイアンゴーレム」「オーガメイジ」を追加。
- HP型・速度型・防御型・回避型に能力傾向を分け、強撃・毒牙・麻痺の咆哮・研ぎ澄ますなどの個別スキルも設定。
- 等級10・9の新規4種を「はじまりの洞窟」「リーフ樹海の獣道」「エルド地下水路」へ配置。
- 等級8・7の新規4種を「封じられた廃坑」「エルド旧採石場」へ配置。
- 等級6・5の新規4種を「霧の古代遺跡」へ配置。
- 新規敵は同等級の既存素材をドロップし、`JapaneseDisplayText`で日本語表示に対応。
- GUID参照数を検査し、各敵が対象となる全ダンジョンから参照されていることを確認。
- `dotnet build DungeonMerchant.sln` は警告0件、エラー0件で成功。
- 戻す場合は、今回追加した12組の敵SOと`.meta`を削除し、6ダンジョンの`normalEnemies`追加分と`JapaneseDisplayText`の12件を戻す。

## 2026-07-02

### 商人成長・1億G借金・月次返済システムを実装

- ユーザー要望により、商人レベルの成長条件を依頼・探索の専用経験値から「ゲーム中に累計で獲得したG」へ変更。
- `MerchantData.AddGold`で全収入を累計し、累計獲得Gから現在レベルと次レベル進行を再計算するようにした。
- 商人レベル上限を20から100へ拡張。
- 次レベル必要Gは`500 + 現在Lv² × 100`の累進式とし、レベルアップごとに技能ポイントを1獲得。
- 交渉・統率・鑑定・兵站の能力上限をLv10からLv50へ大幅拡張。
- 依頼・探索から旧商人経験値を直接加算する処理を停止し、獲得した報酬Gだけが商人成長へ反映されるよう統一。
- `DebtManager`を追加し、ゲーム開始時の借金を1億Gに設定。
- 30日を1月とし、30日経過するごとに最低1万Gを所持金から自動返済。
- 所持金不足時は支払える分だけ返済し、不足額を滞納として翌月の最低返済額へ繰り越す。
- 商人詳細画面へ借金残高、現在月、次回最低返済、返済までの日数、累計獲得Gを表示。
- 商人詳細画面へ「1万G返済」「全額返済」ボタンを追加し、月次返済以外にも任意返済可能。
- 借金残高が0になると「借金完済・ゲームクリア」と表示し、依頼画面の長期目標にも反映。
- 日付表示を「日目／月目」に変更し、月次返済時に完了または滞納額を通知。
- 累計獲得G、借金残高、滞納額、処理済み月数をJSONセーブへ追加し、セーブバージョンを16へ更新。
- 旧セーブは従来の商人Lv・EXPから累計獲得Gを推定し、借金1億Gから開始する。
- `DebtManager`をBootstrapの自動生成対象と`Assembly-CSharp.csproj`のコンパイル対象へ追加。
- `dotnet build DungeonMerchant.sln`は警告0件、エラー0件で成功。
- 戻す場合は`DebtManager`と`.meta`を削除し、`MerchantData`、`ProgressionManager`、`GameSaveData`、`SaveManager`、`DungeonMerchantBootstrap`、`SimpleMercenaryHireUI`、`Assembly-CSharp.csproj`の今回分を戻す。

## 2026-07-06

### UI責務分離・転職画面

- ユーザー要望により、家側で進めたUI責務分割の続きを実施。
- 14通常画面のうち、専用ページコンポーネントが未実装だった転職画面を対象にした。
- `JobChangePageUI`を追加し、`UIPageBase`を継承。
- 転職画面のタイトル、`ScrollRect`、転職対象一覧ルートの参照を`JobChangePageUI`へ移した。
- 転職画面を表示した際の一覧再構築を、親UIの直接呼び出しから`UIPageRouter`による`Refresh()`呼び出しへ変更。
- 転職完了後の一覧更新も`JobChangePageUI.Refresh()`経由へ変更。
- `SimpleMercenaryHireUIPrefabBuilder`が転職ページへ`JobChangePageUI`を追加するようにした。
- PrefabレイアウトVersionを13から14へ更新。旧Prefabや参照不足時は実行時にコンポーネントを補う既存フォールバックを維持。
- `Assembly-CSharp.csproj`へ`JobChangePageUI.cs`を追加。
- `Assembly-CSharp.csproj`と`Assembly-CSharp-Editor.csproj`は、ともに警告0件、エラー0件でビルド成功。
- Unity Editor起動・再コンパイル後、Main UI PrefabがVersion 14へ自動更新されるか実表示確認が必要。
- 戻す場合は`JobChangePageUI.cs`と`.meta`を削除し、`SimpleMercenaryHireUI.HireParty.cs`、`SimpleMercenaryHireUIView.cs`、Prefabビルダー、`Assembly-CSharp.csproj`の今回分を戻す。

### 失敗時の返金処理を削除

- コード確認で、失敗時の返金に`MerchantData.AddGold`を使うことで累計獲得Gと商人レベルが不正に上昇する問題を確認。
- ユーザー指定により返金専用処理は追加せず、返金自体を削除。
- 傭兵の雇用判定に失敗した場合、支払った雇用費は返金されない。
- 鍛冶で支払い後に素材消費へ失敗した場合、支払った制作費は返金されない。
- 市場で支払い後に在庫数更新へ失敗した場合、支払った購入費は返金されない。
- 装備強化で支払い後に強化素材消費へ失敗した場合、支払った強化費は返金されない。
- `AddGold`の利用箇所を再確認し、戦闘・探索・売却・依頼など実際の収入だけに使用されていることを確認。
- ランタイム・Editorプロジェクトはともに警告0件、エラー0件でビルド成功。
- 戻す場合は`MercenaryHireManager.cs`、`BlacksmithManager.cs`、`MarketStockManager.cs`、`MerchantInventory.cs`の失敗分岐へ返金処理を戻す。

### 長期目標の借金情報参照を修正

- コード確認で、Bootstrapが`ProgressionManager`を`DebtManager`より先に生成するため、初期化時に借金管理を取得できない問題を確認。
- `DungeonMerchantBootstrap`の生成順を変更し、`DebtManager`を`ProgressionManager`より先に生成するよう修正。
- シーン配置や将来の構成変更で生成順が異なっても対応できるよう、`GetAchievementSummary()`の開始時に`ResolveReferences()`を呼び、借金管理を再取得するようにした。
- 依頼画面の長期目標で、借金残高または完済状態が表示される。
- ランタイム・Editorプロジェクトはともに警告0件、エラー0件でビルド成功。
- 戻す場合は`DungeonMerchantBootstrap.cs`の生成順と`ProgressionManager.GetAchievementSummary()`の参照再解決を戻す。

### UI責務分離・一覧更新の直接呼び出しを廃止

- ユーザー要望により、家側から段階的に進めているUI責務分離の続きを実施。
- `UIPageRouter`へ、指定ページの`UIPageBase.Refresh()`を呼ぶ`Refresh(RectTransform)`を追加。
- 親UIへ`RefreshPage(RectTransform)`を追加し、画面更新要求をルーター経由へ統一。
- 戦闘、装備、日次結果、依頼、雇用、治療、経済処理などから呼ばれていた`RebuildCompanyList`、`RebuildPartyList`、`RebuildHealList`、`RebuildHireList`、`RebuildInventoryList`、`RebuildMarketList`、`RebuildBlacksmithList`の直接呼び出しを廃止。
- 各更新は対応する`CompanyPageUI`、`PartyPageUI`、`HealPageUI`、`HirePageUI`、各`EconomyPageUI`の`Refresh()`を経由する。
- 転職後の更新も`JobChangePageUI`をルーター経由で更新するよう変更。
- ダンジョン結果パネルを維持したまま一覧だけ更新できるよう、`DungeonPageUI`へ`RefreshSelection()`を追加。
- Prefabが存在しないフォールバックUIでも、雇用画面と傭兵一覧画面へ専用ページコンポーネントを追加・設定するよう補完。
- `BindPageLayout`での先行一括登録を削除し、各専用ページの設定完了後に登録するよう初期化順を整理。
- 機械検索により、対象一覧更新の直接実行が残っていないことを確認。更新本体と`Configure`へ渡すコールバックのみ維持。
- ランタイム・Editorプロジェクトはともに警告0件、エラー0件でビルド成功。
- Unity再生上で、雇用・編成・治療・売買・鍛冶・装備変更・戦闘終了・ダンジョン結果後に各一覧が更新されることの実操作確認が必要。
- 戻す場合は`UIPageRouter.cs`、`BattlePageUI.cs`、`SimpleMercenaryHireUI`各partialファイルの`RefreshPage`関連変更を戻す。

### コード構成上の問題点を確認

- ユーザー要望により、現在のコード構成について依存関係、初期化、保存、データ読込、UI責務、自動テストの観点から確認。
- 最重要問題として、Editorと製品ビルドでゲームデータの検出範囲が異なることを確認。
- Editorでは`AssetDatabase`で`Assets/Proiject/ScriptableObjects`も検索できるが、製品ビルドでは`Resources.LoadAll`のみとなる。
- 現在、`ScriptableObjects/Items`に65件、`Resources/Items`に18件が分散しており、市場、鍛冶、図鑑、依頼、セーブ復元で製品版だけデータが見つからない可能性がある。
- `SimpleMercenaryHireUI`はpartial化後も合計約8,600行あり、全partialが同じprivateフィールドへアクセスできるため、ファイル分割に対して責務境界はまだ弱い。
- 各`PageUI`は更新経路を所有するようになったが、一覧生成と表示用データ作成は親UIに残っている。
- `FindObjectOfType`が全体で約60か所あり、Bootstrapによる動的`AddComponent`と`OnEnable`の実行順に依存する初期化問題が再発しやすい。
- セーブ復元が`asset.name`に依存しており、装備、傭兵、ダンジョンなどのアセット名変更で旧セーブを復元できなくなる可能性がある。
- セーブ移行条件が`SaveManager`内の`version >= 9/11/12/16`として分散しており、今後のバージョン追加で複雑化する。
- 町名、地域、進行順、隣接判定などのゲームルールが`SimpleMercenaryHireUI.MapData.cs`にあり、UIとゲーム進行ルールが分離されていない。
- `BattleManager`は約1,500行、`DungeonRunManager`は約1,070行あり、戦闘計算、進行、報酬、状態異常、ログなど複数責務が同居している。
- 自動テストが存在せず、借金返済、商人レベル、セーブ移行、装備復元、ダンジョン進行、街道敵数、転職条件に回帰リスクがある。
- 推奨対応順は、1. データ読込方式の統一、2. 永続IDとセーブ移行処理の分離、3. UIから町・地域ルールを分離、4. 戦闘・ダンジョン責務の分割、5. 自動テスト追加。
- 今回は調査と共有記録のみで、コード変更およびビルドは未実施。

### 最優先対応・ゲームデータ読込方式を統一

- ユーザー要望により、記録したコード構成問題を優先度順に解決開始。
- 最優先だった「Editorと製品ビルドで検出されるゲームデータが異なる問題」へ対応。
- 移動前に`ScriptableObjects`配下128アセットと既存`Resources`配下28アセットを監査し、同名アセット衝突がないことを確認。
- `GameAssetRepository`を追加し、全環境で`Resources.LoadAll`を使う共通読込経路を実装。
- 市場商品、鍛冶レシピ、ダンジョン、特殊依頼、依頼素材、強化素材、装備図鑑、固有傭兵、戦闘フォールバック、セーブ復元を共通リポジトリ経由へ変更。
- ランタイムコードから`AssetDatabase.FindAssets`と`Assets/Proiject/ScriptableObjects`固定パスへの依存を削除。
- `Assets/Proiject/ScriptableObjects`全体を`Assets/Proiject/Resources/GameData`へ移動。個別`.meta`も一緒に移動し、GUIDを維持。
- 移動後、Resources配下のゲームアセットは合計156件。
- 旧`Assets/Proiject/ScriptableObjects`ディレクトリが残っていないことを確認。
- 全`.meta`を検査し、GUID重複0件を確認。
- アセット、Prefab、SceneのYAML参照を検査。今回移動したプロジェクトアセットのGUID欠落はなし。
- ランタイムコードに旧パス参照および`AssetDatabase`参照が残っていないことを確認。
- ランタイム・Editorプロジェクトはともに警告0件、エラー0件でビルド成功。
- Unity Editorでの再インポート後、市場、鍛冶、ダンジョン一覧、装備図鑑、既存セーブ読込の実表示確認が必要。
- 次の優先課題は、アセット名依存のセーブ識別を永続IDへ置き換え、セーブ移行処理を専用クラスへ分離すること。
- 戻す場合は`Resources/GameData`を元の`ScriptableObjects`へGUIDを維持して戻し、`GameAssetRepository`と各読込箇所の変更を戻す。

## 2026-07-07

### 町・地域進行ルールの責務分離を開始

- 家側の最新作業を確認し、永続ID・セーブ移行・自動テスト基盤が反映済みであることを確認した。
- 残っている優先作業として、町名、地域名、町の進行順、隣接判定、次に解放できる町、地域入場条件がUI側に残っている問題へ対応を開始した。
- `TownMapService` を追加し、町・地域の進行ルールを `SimpleMercenaryHireUI` から分離した。
- `SimpleMercenaryHireUI.MapData.cs` は既存コード互換の薄い呼び出しに変更し、実際の判定は `TownMapService` が担当するようにした。
- 地域入場条件、解放済み地域判定、次解放町判定も `TownMapService` 経由へ移した。
- `RestoreTownProgress` の旧進行順配列参照も `TownMapService` 経由へ変更した。
- `TownMapServiceTests` を追加し、町の進行順、隣接判定、地域入場条件をEditModeテストで確認できるようにした。
- `DungeonMerchant.Runtime.csproj`、`Assembly-CSharp-Editor.csproj`、`DungeonMerchant.EditModeTests.csproj` は警告0件、エラー0件でビルド成功。
- 並列ビルド時に一度だけDLLファイルロックが発生したが、単独再実行で成功した。
- IDE補助用の旧 `Assembly-CSharp.csproj` に、家側のUI分割ファイルと今回追加した `TownMapService` の参照を補完した。
- 旧 `Assembly-CSharp.csproj` もエラー0件でビルド成功。ただし未使用削除済みのVisual Scripting参照が残っているため、補助プロジェクト側のみ参照解決警告9件が出る。

### 町移動中の状態管理をUIから分離

- 次の作業として、町移動中の目的地、解放移動かどうか、ダンジョンを開く予定、接敵数、現在の接敵番号、レア接敵有無、継続選択待ち状態を `RoadTravelState` へ集約した。
- `SimpleMercenaryHireUI` に散らばっていた `pendingTravel...` 系フィールドを削除し、街道移動の開始・継続・撤退・戦闘完了処理は `RoadTravelState` を参照するように変更した。
- 街道戦闘の画面表示文言やボタン挙動は維持し、内部状態のリセット漏れを起こしにくい構造へ整理した。
- `RoadTravelStateTests` を追加し、初期化、継続時の接敵番号進行、クリア処理をEditModeテストで確認できるようにした。
- 古い `pendingRoadRareEncounter`、`pendingTravel...`、`isAwaitingRoadTravelChoice` 参照が残っていないことを検索で確認した。
- `DungeonMerchant.Runtime.csproj`、`Assembly-CSharp-Editor.csproj`、`DungeonMerchant.EditModeTests.csproj` は警告0件、エラー0件でビルド成功。
- 旧 `Assembly-CSharp.csproj` もエラー0件でビルド成功。Visual Scripting削除後の古い参照警告9件のみ残る。

### 古いVisual Scripting参照警告を整理

- ユーザー要望により、旧 `Assembly-CSharp.csproj` に残っていたVisual Scripting関連参照を整理した。
- 削除済みの `com.unity.visualscripting` を参照していた9件の `<Reference>` ブロックを削除した。
- `Assembly-CSharp.csproj` 内に `VisualScripting` 参照が残っていないことを検索で確認した。
- `Assembly-CSharp.csproj`、`DungeonMerchant.Runtime.csproj`、`Assembly-CSharp-Editor.csproj`、`DungeonMerchant.EditModeTests.csproj` はすべて警告0件、エラー0件でビルド成功。
- 注意点として、`Assembly-CSharp.csproj` はUnityが再生成する補助ファイルのため、Unity側でプロジェクトファイル再生成が走ると今回の手動整理が上書きされる可能性がある。

## 2026-07-08

### マージ競合後の町/地域ルール分離を統合

- ユーザー報告により、家側・教室側の作業内容を確認し、マージ後の問題を調査した。
- コンフリクトマーカーはコード・プロジェクトファイル内には残っていなかった。
- 実際の問題は、教室側の `WorldMapService` と学校側の `TownMapService` が同じ町/地域ルール責務を重複して持っていたことだった。
- 家側・教室側の内容を優先し、より広い責務を持つ `WorldMapService` へ学校側の `ValidateTravelRequest` と `TravelValidationResult` を統合した。
- `SimpleMercenaryHireUI.Map.cs` の街道移動開始判定は `WorldMapService.ValidateTravelRequest` を使うように変更した。
- 重複していた `TownMapService` と `TownMapServiceTests` を削除した。
- `WorldMapServiceTests` へ、非隣接移動拒否、傭兵未編成拒否、次解放町への移動許可テストを統合した。
- 以前の学校側作業で導入した `RoadTravelState` と、家側/教室側の街道戦闘ガードが混ざった結果、継続戦闘側に古い `pendingRoadRareEncounter` 参照が1件残っていたため、`roadTravelState.SetRareEncounter` へ修正した。
- 家側で削除済みの旧雇用UI `MercenaryHireListUI` / `MercenaryHireListItemUI` 参照が `Assembly-CSharp.csproj` に残っていたため削除した。
- 家側/教室側で追加済みの `DungeonRewardService` と `WorldMapService` が `Assembly-CSharp.csproj` に不足していたため追加した。
- `DungeonMerchant.Runtime.csproj`、`Assembly-CSharp-Editor.csproj`、`DungeonMerchant.EditModeTests.csproj`、`Assembly-CSharp.csproj` はすべて警告0件、エラー0件でビルド成功。
- `git` コマンドはこの環境では利用できないため、`rg` とビルドで確認した。

### 街道移動の開始可否判定をサービスへ移管

- 次の分離作業として、街道移動を開始できるかどうかの判定を `TownMapService.ValidateTravelRequest` へ移した。
- 隣接していない町への直接移動、未解放地域、解放順序違反、傭兵未編成の判定と失敗理由文を `TownMapService` 側で返すようにした。
- `SimpleMercenaryHireUI.RequestTownTravel` は判定結果を受け取り、失敗時は理由表示、成功時は確認ダイアログ表示だけを担当する形へ縮小した。
- `TownMapServiceTests` に、非隣接移動の拒否、傭兵未編成の拒否、次解放町への移動許可のテストを追加した。
- `DungeonMerchant.Runtime.csproj`、`Assembly-CSharp-Editor.csproj`、`DungeonMerchant.EditModeTests.csproj` は警告0件、エラー0件でビルド成功。
- 旧 `Assembly-CSharp.csproj` もエラー0件でビルド成功。Visual Scripting削除後の古い参照警告9件のみ残る。
## 2026-07-08 学校側・Visual Scripting残骸の再生成対策

- `Packages/manifest.json` と `Packages/packages-lock.json` に `com.unity.visualscripting` が残っていないことを確認した。
- `ProjectSettings/VisualScriptingSettings.asset` が古いVisual Scripting参照の設定ファイルとして残っていたため削除した。
- `ProjectSettings`、`Packages`、`Assets/Proiject/Scripts`、`Assets/Proiject/Tests`、各csproj内に `VisualScripting` / `visualscripting` 参照が残っていないことを検索で確認した。
- `Assembly-CSharp.csproj` は警告0件・エラー0件でビルド成功。
- Unityがcsprojを再生成しても、Visual Scriptingパッケージと設定ファイルの両方が消えているため、古い参照警告が復活しにくい状態になった。

## 2026-07-08 学校側・街道移動完了処理の分離

- 街道戦闘勝利/敗北後の町到着、町解放、日数経過、保存要否、表示メッセージの判定を `RoadTravelCompletionService` に分離した。
- `SimpleMercenaryHireUI.BattleDungeon.cs` は、街道戦闘完了時にサービスの結果を受け取り、画面遷移・保存・UI更新だけを行う形に整理した。
- `RoadTravelCompletionServiceTests` を追加し、勝利時の町解放、勝利後ダンジョン表示、敗北時の現在地維持、無効状態の結果をEditModeテストで確認できるようにした。
- 古い `VisualScripting`、`TownMapService`、`pendingRoadRareEncounter` 参照が残っていないことを検索で確認した。
- `DungeonMerchant.Runtime.csproj`、`Assembly-CSharp-Editor.csproj`、`DungeonMerchant.EditModeTests.csproj`、`Assembly-CSharp.csproj` はすべて警告0件・エラー0件でビルド成功。
- 注意: 最初にビルドを並列実行した際、EditModeTestsだけ一時DLLのファイルロックで失敗したが、順番に再実行して警告0件・エラー0件で成功した。コード上のコンパイルエラーではない。
## 2026-07-08 学校側・ダンジョン選択UIの責務分離

- UIページ分離の続きとして、親UIに残っていたダンジョン選択リスト生成を `DungeonPageUI` 側へ移した。
- `DungeonPageUI` が、現在の町、選択可能ダンジョン、攻略済みフロア、開放状態、選択中状態を受け取り、行生成・空表示・選択ボタン表示を担当する形にした。
- `SimpleMercenaryHireUI.BattleDungeon.cs` は、ダンジョン選択時の処理とデータ提供だけを担当する形に縮小した。
- 親UI側でしか使われていなかった `dungeonSelectButtons` と `displayedDungeons` を削除した。
- `RebuildDungeonSelectionList`、古いダンジョン選択リスト用フィールド、古いVisual Scripting参照、古い `TownMapService` 参照が残っていないことを検索で確認した。
- `DungeonMerchant.Runtime.csproj`、`Assembly-CSharp-Editor.csproj`、`DungeonMerchant.EditModeTests.csproj`、`Assembly-CSharp.csproj` はすべて警告0件・エラー0件でビルド成功。
## 2026-07-08 学校側・街道戦闘の敵数バランス調整

- 街道戦闘の1戦あたりの敵数を、序盤2体・中盤3体・後半4体へ調整した。
- 以前は最初の街道が5体、それ以外が4体だったため、連続戦闘時に序盤から負担が重くなりやすかった。
- 街道の連戦数そのものは変更せず、まずは1戦ごとの重さを下げる調整にした。
- `RoadEncounterServiceTests` の期待値を更新し、通常候補がある場合とフォールバック敵を使う場合の両方で敵数上限を確認できるようにした。
- 古い敵数定数、古いVisual Scripting参照、古い `TownMapService` 参照、古いダンジョン選択リスト用フィールドが残っていないことを検索で確認した。
- `DungeonMerchant.Runtime.csproj`、`Assembly-CSharp-Editor.csproj`、`DungeonMerchant.EditModeTests.csproj`、`Assembly-CSharp.csproj` はすべて警告0件・エラー0件でビルド成功。
## 2026-07-09 学校側・家側作業内容の確認

- ユーザー要望により、家側の作業内容を `handoff/FROM_HOME_CHAT.md` と `handoff/SHARED_PROJECT_STATUS.md` から確認した。
- 家側の最新主要作業は、永続IDとセーブ移行、UI責務分離、転職/雇用/経済ページ分離、街道敵数ガード、ダンジョンイベント/特殊個体処理の分離だった。
- 共有ログの一部は文字化けして保存されているが、ファイル名とコード状況から主要内容は確認できた。
- 現在のコードをビルド確認したところ、Runtime / Editor / EditModeTests は問題なかったが、IDE補助用の `Assembly-CSharp.csproj` だけ家側追加ファイルの参照漏れでエラーになっていた。
- 参照漏れしていた `DungeonEnemyVariantService.cs` と `DungeonEventService.cs` を `Assembly-CSharp.csproj` に追加した。
- 並列ビルド時に一時DLLのファイルロックが発生したため、順番に再実行して確認した。
- `DungeonMerchant.Runtime.csproj`、`Assembly-CSharp-Editor.csproj`、`DungeonMerchant.EditModeTests.csproj`、`Assembly-CSharp.csproj` はすべて警告0件・エラー0件でビルド成功。
## 2026-07-09 学校側・初回チュートリアルを追加

- チュートリアル制作の開始として、初回起動時に表示される基本操作チュートリアルを追加した。
- `SimpleMercenaryHireUI.Tutorial.cs` を追加し、目的、最初にやること、町と施設、探索と戦闘、装備と成長、日数と借金の6ページ構成にした。
- 初回表示済みフラグはセーブデータ形式を増やさず、`PlayerPrefs` の `DungeonMerchant.Tutorial.Completed` で管理する。
- 「完了」まで進めた場合だけ次回以降の自動表示を止める。閉じただけの場合は次回起動時にも再表示される。
- グローバルメニューに「チュートリアル」ボタンを追加し、完了後も見返せるようにした。
- `DungeonMerchant.Runtime.csproj` と `Assembly-CSharp.csproj` に新規チュートリアルファイル参照を追加した。
- `DungeonMerchant.Runtime.csproj`、`Assembly-CSharp-Editor.csproj`、`DungeonMerchant.EditModeTests.csproj`、`Assembly-CSharp.csproj` はすべて警告0件・エラー0件でビルド成功。
- Unity上での表示位置、文字量、初回表示/完了後非表示の実動作確認は未確認。

## 2026-07-15 学校側・ダンジョンイベントを戦闘画面へ統合

- ユーザーから、復旧後の戦闘画面確認が完了したとの報告を受けた。
- 戦闘勝利後のダンジョンイベント表示先を、ダンジョン選択画面から現在の戦闘画面へ変更した。
- 戦闘背景、敵味方表示、戦闘ログを残したまま、中央のイベントパネルへタイトル、説明、3つの選択肢を表示する。
- イベント選択後は既存の `DungeonRunManager.ChooseEventOption` を使い、効果適用後に次の遭遇を開始する。撤退、フロア結果、保存、通知順は変更していない。
- `RuntimeUIPlayModeTests` に、イベントパネルが戦闘ページ配下へ生成され、初期状態では非表示であることの回帰検査を追加した。
- 復旧後の `Assembly-CSharp.csproj` に残っていた旧ファイル名6件と現行ファイル参照漏れを、`DungeonMerchant.Runtime.csproj` と一致するよう整理した。
- Runtime、Editor、EditModeTests、PlayModeTests、Assembly-CSharpはすべて警告0件・エラー0件でビルド成功した。
- Unity上では、イベント発生時の文字量、3ボタンの収まり、選択後の次戦闘、撤退後の結果画面を確認する必要がある。

## 2026-07-15 学校側・イベント画像選択、戦闘ホバー詳細、一時停止

- ダンジョンイベントの3選択肢を、戦闘画面の敵表示領域へ並ぶ画像カード形式へ変更した。
- イベント別画像は `Resources/Battle/Events` の安定キーから読み込む。画像未配置時も既存の羊皮紙画像と選択肢別の色でカード表示を維持する。
- 各画像カードへカーソルを合わせると、HP回復、G獲得、被害、追加日数、限定装備の可能性、撤退結果を表示する。予告はラベル解析ではなく `DungeonEventService` の既存結果計算から生成する。
- 戦闘中の敵・味方へカーソルを合わせると、名前、職業または等級、Lv、HP、MP、攻撃、防御、速度、会心、回避、状態、敵スキルを表示する `UIHoverTooltipTrigger` を追加した。
- 通常戦闘と街道戦闘へ一時停止／再開ボタンを追加した。`Time.timeScale` は使わず、`BattleManager.IsPaused` で戦闘処理と演出の両方を停止する。
- 一時停止中に「結果まで」を選ぶと停止を解除して既存のスキップ処理を継続する。戦闘開始時・終了時にも停止状態を初期化する。
- イベント結果予告と画像キー、戦闘一時停止、Runtime UI構成の回帰テストを追加した。
- Runtime、Editor、EditModeTests、PlayModeTests、Assembly-CSharpは警告0件・エラー0件でビルド成功した。
- Unityが複数プロセスで起動中だったため、Test Runnerの実行は行っていない。画像カードの表示、ホバーの位置と追従、一時停止中の処理・演出停止を実画面で確認する必要がある。

## 2026-07-15 学校側・イベント文字見切れと戦闘表示同期を修正

- ユーザーの画面報告から、イベント説明と画像カードの選択肢が見切れる問題を確認した。
- イベントタイトル、説明、予告、カードラベルの表示領域を再配分し、折り返し、縦方向Overflow、フォント自動縮小、内側余白を設定した。
- 戦闘ロジック上は敵が倒れていても撃破アニメーションが残り、敵が表示されたままイベントへ進む問題を確認した。
- `BattleVisualController` に `IsPresentationBusy` と `PresentationCompleted` を追加し、撃破・勝利演出の表示キューが完了するまでイベントUIとフロア結果UIを待機させた。
- 撃破後の敵は従来の半透明表示から完全非表示へ変更し、レイキャストも無効化した。
- 結果までスキップ時も即時表示反映後に完了通知を行い、通常速度と同じ遷移条件を維持した。
- Runtime UIテストへカード文字の自動縮小、折り返し、縦Overflow設定の検査を追加した。
- Runtime、Editor、EditModeTests、PlayModeTests、Assembly-CSharpはすべて警告0件・エラー0件でビルド成功した。
- Unity上で、通常速度・2倍・4倍・結果までスキップの各条件において、敵消滅後にイベントが表示されることと文字の収まりを確認する必要がある。
