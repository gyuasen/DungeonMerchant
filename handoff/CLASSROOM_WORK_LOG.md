# 教室側作業ログ

このファイルは、家・学校とは別の3つ目の開発環境「教室」でCodexが行った作業を記録するためのログです。

## 運用ルール

- 教室側は学校側と同じ開発ルールに従う。
- 教室側で行った作業は、作業ごとにこのファイルへ追記する。
- 各記録には変更内容、主な変更ファイル、確認結果、影響範囲、元に戻す方法を必要に応じて記載する。
- 全環境に関係する状況や連絡は `SHARED_PROJECT_STATUS.md` に短く追記する。
- 家側・学校側・教室側の内容に食い違いがある場合は、家側の内容を優先する。

## 2026-06-30

### 教室環境の共有運用を追加

- 家・学校とは別の3つ目の開発環境として「教室」の運用を開始した。
- 教室側の専用作業ログとして、この `CLASSROOM_WORK_LOG.md` を新設した。
- `.instructions.md` と `handoff/README.md` を三環境運用へ更新した。
- `handoff/SHARED_PROJECT_STATUS.md` に、教室環境でも確認・開発を行っている旨を家側・学校側向けに追記した。
- ゲーム本体のコード、アセット、シーンには変更を加えていない。
- 影響範囲は引き継ぎ文書の運用のみ。
- 元に戻す場合は、このファイルを削除し、上記3文書の教室環境に関する追記を削除する。

### 一日のリザルトUIを追加

- 日付をまたぐたびに「一日のリザルト」オーバーレイを表示するようにした。
- 商人の所持金、経験値、レベルアップ、技能ポイント、交渉・統率・鑑定・物流の変化を前日との差分で表示する。
- 傭兵の経験値、レベルアップ、最大HP・攻撃・防御・魔力・速度の上昇を前日との差分で表示する。
- 契約期限が切れた傭兵と、契約切れによって編成から外れた傭兵を明示する。
- 当日に新規雇用・編成した日雇い傭兵も契約終了表示から漏れないよう、雇用時と編成時に状態を記録する。
- ダンジョン完了時の商人経験値を日付進行前に付与し、その日のリザルトへレベルアップが反映されるよう処理順を修正した。
- 変更ファイル:
  - `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`
  - `Assets/Proiject/Scripts/Merchant/ProgressionManager.cs`
- `dotnet build DungeonMerchant.sln` は警告0件、エラー0件で成功。
- Unity再生時のレイアウトと、日雇い契約終了時の実表示は未確認。
- 戻す場合は `SimpleMercenaryHireUI.cs` の日次スナップショット・リザルトオーバーレイ処理を削除し、`ProgressionManager.cs` の経験値付与と日付進行の順番を元へ戻す。

### 一日のリザルトへ入手品と傭兵詳細を追加

- 在庫変更のたびに数量増加を記録し、その日に入手した通常アイテムの名前と合計数をリザルトへ表示するようにした。
- 個体装備はインスタンスIDで追跡し、着脱を再入手と誤認せず、品質・装備名・強化値を表示する。
- 傭兵欄を全雇用者の詳細表示へ拡張した。
- 各傭兵について、職業、レベル、当日の成長、現在HP、経験値進捗、攻撃、防御、魔力、速度、会心率、回避率、契約期限、編成状態を表示する。
- 長いリザルトを読めるよう、内容に応じてスクロール領域の高さを拡張する。
- 変更ファイル: `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`
- `dotnet build DungeonMerchant.sln` は警告0件、エラー0件で成功。
- Unity再生時の長文レイアウトは未確認。
- 戻す場合は、日次在庫追跡フィールドと処理を削除し、`ShowDailyResult` の入手品・傭兵詳細部分を以前の簡易成長表示へ戻す。

### モンスター素材の鍛冶屋装備とダンジョンドロップ装備を追加

- モンスター素材で制作する鍛冶屋装備を、未追加だった僧侶・盗賊・槍兵向けに各3種追加した。
- 追加した鍛冶屋装備:
  - 僧侶: `SanctifiedMace`、`BonePrayerVestment`、`SpiritBead`
  - 盗賊: `VenomFangDagger`、`ShadowhideArmor`、`BatEyeCharm`
  - 槍兵: `OrcboneSpear`、`GolemPlate`、`WyvernCrest`
- 上記9種に対応する鍛冶レシピを `Assets/Proiject/Resources/GameData/Blacksmith` へ追加した。
- ダンジョンドロップ装備を下級・中級・上級向けに各3種追加した。
  - 下級: `MinebreakerHammer`、`DeepMinerArmor`、`EchoStoneRing`
  - 中級: `MistRuneBlade`、`RuinweaveMantle`、`GuardianEyeCharm`
  - 上級: `BlackIronHalberd`、`GeneralPlate`、`BlackIronWarEmblem`
- `LowerMine` と `EldOldQuarry` に下級装備、`MiddleRuins` に中級装備、`UpperFortress` に上級装備を限定ドロップとして接続した。
- `JapaneseDisplayText` に追加装備18種の日本語表示名を追加した。
- GUID重複は0件。今回追加した装備・レシピへの参照切れは検出されていない。
- `dotnet build DungeonMerchant.sln` は警告0件、エラー0件で成功。
- Unity上での実ドロップ、鍛冶屋一覧表示、装備性能バランスは未確認。
- `NornCanopyLabyrinth` と `VelmBlackIronMine` は限定装備ドロップが空のまま。必要なら次作業で町・ダンジョンごとの固有ドロップを追加する。

## 2026-07-07

### コード構成整理・町/地域ルールをサービスへ分離

- 全体のコード構成整理作業として、UI partialに残っていた町名、地域名、町の進行順、隣接判定を `WorldMapService` へ移した。
- `SimpleMercenaryHireUI.MapData.cs` は既存UIの参照互換を保つ薄いラッパーに変更した。
- 町移動・地域解放・マップ表示の挙動は変更していない。
- `WorldMapServiceTests` を追加し、町数、地域数、進行順の隣接判定、目的地へ向かう次の町の算出を確認できるようにした。
- 変更ファイル:
  - `Assets/Proiject/Scripts/Core/WorldMapService.cs`
  - `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.MapData.cs`
  - `Assets/Proiject/Tests/EditMode/WorldMapServiceTests.cs`
  - `DungeonMerchant.Runtime.csproj`
  - `DungeonMerchant.EditModeTests.csproj`
- `dotnet build DungeonMerchant.sln` は警告0件、エラー0件で成功。
- Unity Test Runnerでの実行は未確認。
- 戻す場合は `WorldMapService.cs` と `WorldMapServiceTests.cs` を削除し、`SimpleMercenaryHireUI.MapData.cs` に町/地域データと判定処理を戻す。

### コード構成整理・町解放/地域解放ルールを追加分離

- 町・地域ルール分離の続きとして、次に解放できる町、旧セーブ用の町解放補完、地域内に解放済み町があるか、地域解放ゲート町の判定を `WorldMapService` へ移した。
- 地域解放判定は、UI側から「ゲート町の最高等級ダンジョンを完全攻略済みか」を関数として渡す形にし、町/地域ルールとダンジョン進行確認を分けた。
- `DungeonRunManager.SetUnlockedTownIndices` も `WorldMapService.IsValidTownIndex` を使うようにし、UIとダンジョン管理が同じ町定義を参照するようにした。
- `WorldMapServiceTests` に旧セーブ補完、保存済み町リストの不正値除外、地域ゲート町、地域進入判定のテストを追加した。
- 変更ファイル:
  - `Assets/Proiject/Scripts/Core/WorldMapService.cs`
  - `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`
  - `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.Map.cs`
  - `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.MapData.cs`
  - `Assets/Proiject/Scripts/Dungeon/DungeonRunManager.cs`
  - `Assets/Proiject/Tests/EditMode/WorldMapServiceTests.cs`
- `dotnet build DungeonMerchant.sln` は警告0件、エラー0件で成功。
- Unity Test Runnerでの実行は未確認。
- 画面表示、町移動、地域解放条件の仕様変更はなし。

### コード構成整理・経済系ページの一覧生成をPageUI側へ移動

- 優先度の高い責務整理として、在庫・市場・鍛冶屋ページの一覧再構築ループを `InventoryPageUI`、`MarketPageUI`、`BlacksmithPageUI` へ移した。
- `EconomyPageUI` に共通の行再構築処理を追加し、空表示、行ループ、スクロール内容高さの更新をページ側で担当するようにした。
- `SimpleMercenaryHireUI.Economy.cs` はデータ供給、フィルタ判定、各行の描画コールバック、購入/売却/制作などの操作処理を残す構成にした。
- 旧 `RebuildInventoryList`、`RebuildMarketList`、`RebuildBlacksmithList` は削除した。
- 変更ファイル:
  - `Assets/Proiject/Scripts/UI/EconomyPageUI.cs`
  - `Assets/Proiject/Scripts/UI/InventoryPageUI.cs`
  - `Assets/Proiject/Scripts/UI/MarketPageUI.cs`
  - `Assets/Proiject/Scripts/UI/BlacksmithPageUI.cs`
  - `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.Economy.cs`
- `dotnet build DungeonMerchant.sln` は警告0件、エラー0件で成功。
- Unity上での在庫・市場・鍛冶屋の実表示とスクロール動作は未確認。
- 次段階では、各行の描画処理や表示用テキスト生成もページ/専用Presenterへ寄せる余地がある。

### コード構成整理・傭兵管理系ページの一覧生成をPageUI側へ移動

- 優先度2の一部として、雇用・商会・編成・治療ページの一覧再構築ループを各 `PageUI` 側へ移した。
- `HirePageUI` は固定候補と町別生成候補の2系統を受け取り、候補リストの行ループと高さ更新を担当する。
- `CompanyPageUI`、`PartyPageUI`、`HealPageUI` は、それぞれ雇用済み傭兵、編成スロット、治療対象の行ループと空表示を担当する。
- `SimpleMercenaryHireUI.HireParty.cs` はデータ供給、フィルタ判定、行描画コールバック、雇用/編成/治療操作を残す構成にした。
- 旧 `RebuildHireList`、`RebuildCompanyList`、`RebuildPartyList`、`RebuildHealList` は削除した。
- 変更ファイル:
  - `Assets/Proiject/Scripts/UI/UIPageBase.cs`
  - `Assets/Proiject/Scripts/UI/HirePageUI.cs`
  - `Assets/Proiject/Scripts/UI/CompanyPageUI.cs`
  - `Assets/Proiject/Scripts/UI/PartyPageUI.cs`
  - `Assets/Proiject/Scripts/UI/HealPageUI.cs`
  - `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.HireParty.cs`
- `dotnet build DungeonMerchant.sln` は警告0件、エラー0件で成功。
- Unity上での雇用・商会・編成・治療ページの実表示とスクロール動作は未確認。
- 転職一覧 `RebuildJobChangeList` はまだ親UI側に残っているため、次段階で分離候補。

## 2026-07-21

### 戦闘演出/図鑑UI/特殊個体表示の微調整

- 戦闘勝利後、モンスターの撃破演出が終わる前にダンジョンイベントへ移る問題へ対応した。
- `BattleVisualController` の `BattleCompleted` 受信時に視覚キューを止めて最終状態へ即時スナップしていた処理を削除し、撃破イベントを含むキューを順番に再生してから勝利表示と完了通知を行うようにした。
- 装備図鑑の見開きが片側で見切れる問題へ対応するため、装備図鑑ウィンドウを拡大し、`BookPageUI` の片ページ幅を少し縮めた。
- 特殊モンスターは通常モンスターと同じ画像を使う方針に合わせ、特殊個体の画像解決時に元敵の画像へフォールバックする処理を追加した。
- 特殊個体へ紫系のオーラ枠、発光枠、`SPECIAL` ラベルを追加し、通常画像の上から特殊個体であることを判別できるようにした。
- 変更ファイル:
  - `Assets/Proiject/Scripts/UI/BattleVisualController.cs`
  - `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.CharacterEquipment.cs`
  - `Assets/Proiject/Scripts/UI/BookPageUI.cs`
  - `handoff/SHARED_PROJECT_STATUS.md`
- 確認:
  - `dotnet build DungeonMerchant.Runtime.csproj` は警告0件、エラー0件で成功。
  - `dotnet build DungeonMerchant.EditModeTests.csproj` は警告0件、エラー0件で成功。
  - `dotnet build DungeonMerchant.PlayModeTests.csproj` は警告0件、エラー0件で成功。
  - `dotnet build Assembly-CSharp-Editor.csproj` は警告0件、エラー0件で成功。
- 注意:
  - `dotnet build DungeonMerchant.sln --no-restore` は、この環境ではユーザーTemp配下のMSBuild一時ファイルアクセス拒否で失敗した。
  - Tempをワークスペースへ向けたソリューション通常ビルドも詳細なしで失敗したため、個別プロジェクトビルドで代替確認した。
- Unity上で確認する項目:
  - 撃破フェード完了後にイベントカードが表示されること。
  - 装備図鑑の左右ページが見切れないこと。
  - 特殊個体が通常画像+紫オーラ/ラベルで表示されること。

## 2026-07-21

### 一日のリザルト表示タイミング修正

- 戦闘演出中に日付変更が発生した場合、一日のリザルトを即時表示せず、戦闘演出完了まで保留するように修正した。
- `SimpleMercenaryHireUI.DailyResult.cs` に保留表示の判定と `ShowPendingDailyResultIfReady` を追加した。
- `SimpleMercenaryHireUI.BattleDungeon.cs` の戦闘演出完了処理とダンジョン完了待機処理から、保留中の一日リザルトを表示するように接続した。
- これにより、モンスター撃破演出やダンジョン完了表示より前に一日のリザルトへ遷移してしまうタイミングずれを抑制する。
- 変更ファイル:
  - `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`
  - `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.DailyResult.cs`
  - `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.BattleDungeon.cs`
- 確認:
  - `dotnet build DungeonMerchant.Runtime.csproj --no-restore` は警告0件、エラー0件で成功。
- 注意:
  - この環境では `git` コマンドが見つからないため、`git diff` / `git status` による差分確認は未実施。
  - Unity上では、日またぎ直前の戦闘で撃破演出が終わってから一日のリザルトが表示されることを確認する。
