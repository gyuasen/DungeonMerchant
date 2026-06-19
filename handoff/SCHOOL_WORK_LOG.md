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
