# 家のチャットから学校側Codexへ

このファイルに、家のチャットで進めた内容を貼ってください。

## 家でやったこと

- 

## 変更したファイル

- 

## 学校側で次にやってほしいこと

- 

## エラーや未解決の問題

- 

## 家のチャットからのメモ

ここに家のチャットの返答、設計メモ、コード案、Unity Consoleのエラーなどを貼ってください。


## 家でやったこと 2026-06-17

- `.instructions.md` に家/学校間の共有ルールを追加。
- `handoff/README.md` に、家側Codexの作業内容も必要に応じて `FROM_HOME_CHAT.md` へ追記するルールを明記。

## 変更したファイル 2026-06-17

- `.instructions.md`
- `handoff/README.md`
- `handoff/FROM_HOME_CHAT.md`

## 家でやったこと 2026-06-17

- アイテム売買の土台として日数/市場価格要素を追加。
- `DayManager` を追加し、現在日数と日送りイベントを管理するようにした。
- `MarketPriceManager` を追加し、日付とアイテムごとに売却価格倍率が変わるようにした。
- `MerchantInventory.SellItem` が `item.basePrice` 固定ではなく、市場価格を使って売却するようにした。
- `SimpleMercenaryHireUI` の `INVENTORY` タブに `NEXT DAY` ボタンと市場日付表示を追加。
- `NEXT DAY` を押すと日数が進み、売却価格表示が更新される。
- `DungeonMerchantBootstrap` が `DayManager` と `MarketPriceManager` も自動生成するようにした。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

## 変更したファイル 2026-06-17

- `Assets/Proiject/Scripts/Merchant/DayManager.cs`
- `Assets/Proiject/Scripts/Merchant/DayManager.cs.meta`
- `Assets/Proiject/Scripts/Merchant/MarketPriceManager.cs`
- `Assets/Proiject/Scripts/Merchant/MarketPriceManager.cs.meta`
- `Assets/Proiject/Scripts/Core/DungeonMerchantBootstrap.cs`
- `Assets/Proiject/Scripts/Item/MerchantInventory.cs`
- `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`

## 家で行ったこと 2026-06-19

- 傭兵の装備枠を武器・防具・装飾品の3枠へ拡張。
- 3枠の固定装備と品質付き装備の能力補正を、HP・攻撃・防御・攻撃速度へ合算するようにした。
- キャラクター詳細画面で、防具と装飾品も比較・装備・交換・解除できるようにした。
- 装備候補は同じスロットの現在装備と比較し、品質と装備種別も表示する。
- 防具と装飾品の装備状態をJSONセーブ・ロードへ追加。旧武器セーブとの互換性は維持。
- 戦士・弓兵・魔法使い向けの防具3種、装飾品3種と鍛冶屋レシピを追加。
- 鍛冶屋は既存の登録済みレシピがある場合も、新規レシピを自動検出して追加するようにした。
- `dotnet build DungeonMerchant.sln` は警告・エラー0で成功。

## 変更した主なファイル 2026-06-19

- `Assets/Proiject/Scripts/Mercenary/MercenaryInstance.cs`
- `Assets/Proiject/Scripts/Core/GameSaveData.cs`
- `Assets/Proiject/Scripts/Core/SaveManager.cs`
- `Assets/Proiject/Scripts/Blacksmith/BlacksmithManager.cs`
- `Assets/Proiject/Scripts/UI/JapaneseDisplayText.cs`
- `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`
- `Assets/Proiject/ScriptableObjects/Items/*Armor.asset`
- `Assets/Proiject/ScriptableObjects/Items/*Leather.asset`
- `Assets/Proiject/ScriptableObjects/Items/*Robe.asset`
- `Assets/Proiject/ScriptableObjects/Items/*Emblem.asset`
- `Assets/Proiject/ScriptableObjects/Items/*Charm.asset`
- `Assets/Proiject/ScriptableObjects/Items/*Pendant.asset`
- `Assets/Proiject/ScriptableObjects/Blacksmith/*ArmorRecipe.asset`
- `Assets/Proiject/ScriptableObjects/Blacksmith/*LeatherRecipe.asset`
- `Assets/Proiject/ScriptableObjects/Blacksmith/*RobeRecipe.asset`
- `Assets/Proiject/ScriptableObjects/Blacksmith/*EmblemRecipe.asset`
- `Assets/Proiject/ScriptableObjects/Blacksmith/*CharmRecipe.asset`
- `Assets/Proiject/ScriptableObjects/Blacksmith/*PendantRecipe.asset`
- `Assembly-CSharp.csproj`

## 家でやったこと 2026-06-17

- 仕入れ商品の生成と購入UIを追加。
- `MarketStockManager` を追加し、日ごとに仕入れ可能な商品、数量、仕入れ価格を生成するようにした。
- `MarketStockEntry` を追加し、仕入れ商品のアイテム、残数、購入価格を管理するようにした。
- `SimpleMercenaryHireUI` に `MARKET` タブを追加。
- `MARKET` タブで `BUY` を押すと、商人のゴールドを支払い、商人在庫へアイテムを追加するようにした。
- 日付が進むと市場価格だけでなく、仕入れ商品も更新される。
- `DungeonMerchantBootstrap` が `MarketStockManager` も自動生成するようにした。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

## 変更したファイル 2026-06-17

- `Assets/Proiject/Scripts/Merchant/MarketStockManager.cs`
- `Assets/Proiject/Scripts/Merchant/MarketStockManager.cs.meta`
- `Assets/Proiject/Scripts/Item/MarketStockEntry.cs`
- `Assets/Proiject/Scripts/Item/MarketStockEntry.cs.meta`
- `Assets/Proiject/Scripts/Core/DungeonMerchantBootstrap.cs`
- `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`
- `Assembly-CSharp.csproj`

## 家でやったこと 2026-06-19

- ダンジョンの戦闘間にランダムイベントを追加。
- イベントは `Abandoned Camp`、`Hidden Treasure`、`Collapsed Passage` の3種類。
- 各イベントで回復、ゴールド獲得、パーティー全員へのダメージ、撤退を選択できる。
- 戦闘勝利後、最終戦でなければ次の戦闘へ直行せず、DUNGEONタブでイベント選択を待つようにした。
- イベント選択後は次の戦闘へ進み、撤退選択時はダンジョンランを終了する。
- DUNGEONタブにイベント名、説明、3つの選択ボタンを追加。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

## 変更したファイル 2026-06-19

- `Assets/Proiject/Scripts/Dungeon/DungeonRunManager.cs`

## 家でやったこと 2026-06-19

- ゲーム全体のJSONセーブ・ロード機能を追加。
- `Application.persistentDataPath/game-save.json` に保存する `SaveManager` を追加。
- 保存対象は所持金、日数、商人在庫、雇用済み傭兵、傭兵の能力値と現在HP、パーティー編成、ダンジョン最高開放等級、選択中ダンジョン。
- 固有傭兵SOとアーキタイプSOはアセット名から復元し、生成傭兵は保存済み能力値から再構築する。
- アイテムはアセット名とアイテム名から復元する。
- 起動時はUI構築前に自動ロードする。
- 所持金、日数、在庫、雇用、編成、治療、戦闘終了、ダンジョン状態変更時に自動保存する。
- アプリ終了時とバックグラウンド移行時にも保存する。
- `SaveManager` のInspectorコンテキストメニューに手動保存、手動読込、セーブデータ削除を追加。
- 古い/不完全なJSONでリストが空でも安全に読み込めるようにした。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

## 変更したファイル 2026-06-19

- `Assets/Proiject/Scripts/Core/GameSaveData.cs`
- `Assets/Proiject/Scripts/Core/GameSaveData.cs.meta`
- `Assets/Proiject/Scripts/Core/SaveManager.cs`
- `Assets/Proiject/Scripts/Core/SaveManager.cs.meta`
- `Assets/Proiject/Scripts/Core/DungeonMerchantBootstrap.cs`
- `Assets/Proiject/Scripts/Merchant/MerchantData.cs`
- `Assets/Proiject/Scripts/Merchant/DayManager.cs`
- `Assets/Proiject/Scripts/Item/MerchantInventory.cs`
- `Assets/Proiject/Scripts/Mercenary/MercenaryInstance.cs`
- `Assets/Proiject/Scripts/Mercenary/MercenaryHireManager.cs`
- `Assets/Proiject/Scripts/Mercenary/MercenaryPartyManager.cs`
- `Assets/Proiject/Scripts/Dungeon/DungeonRunManager.cs`
- `Assembly-CSharp.csproj`

## 家でやったこと 2026-06-19

- モンスターに1〜10等級を追加。1等級が最強、10等級が最弱。
- `EnemyDataSO` に `monsterGrade` と `isBoss` を追加。
- ダンジョンSOに通常敵候補 `normalEnemies` と最終戦ボス `bossEnemy` を追加。
- 通常遭遇は設定された通常敵候補からランダム編成し、最終遭遇は通常敵にボス1体を加える。
- 低級は通常10・9等級、ボス7等級。
- 下級は通常8・7等級、ボス6等級。
- 中級は通常6・5等級、ボス4等級。
- 上級は通常4・3等級、ボス2等級。
- 最上級は通常2・1等級、ボス1等級。
- 通常敵10種とボス5種の敵SOを作成。
- DUNGEON一覧に通常敵等級とボス等級を表示。
- 戦闘中のボス名に `ボス` を付けて表示。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

## 変更したファイル 2026-06-19

- `Assets/Proiject/Scripts/Data/EnemyDateSO.cs`
- `Assets/Proiject/Scripts/Dungeon/DungeonDataSO.cs`
- `Assets/Proiject/Scripts/Dungeon/DungeonRunManager.cs`
- `Assets/Proiject/Scripts/Battle/BattleManager.cs`
- `Assets/Proiject/Scripts/UI/JapaneseDisplayText.cs`
- `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`
- `Assets/Proiject/ScriptableObjects/Enemies/*.asset`
- `Assets/Proiject/ScriptableObjects/Enemies/*.asset.meta`
- `Assets/Proiject/ScriptableObjects/Dungeons/*.asset`

## 家でやったこと 2026-06-19

- アイテムバリエーションを既存1種から合計15種へ拡張。
- 通常敵素材として、ゴブリンの耳、コウモリの翼、呪われた骨、オークの牙、ゴーレムの核、闇の結晶、ワイバーンの鱗、魔鋼、深淵竜の鱗を追加。
- ボス固有遺物として、オーガの王冠、暴君の大つるはし、守護者の魔眼、黒鉄の紋章、深淵の王冠を追加。
- 通常素材は等級が高い敵ほど価値が高く、ドロップ率が低くなるように設定。
- ボス固有遺物は各ボスから1個確定ドロップ。
- 基準価格は一般素材25Gから最上級ボス遺物1200Gまで段階設定。
- 全アイテムの日本語表示名を `JapaneseDisplayText` に追加。
- 新規アイテムは市場の商品自動検出、日ごとの価格変動、在庫セーブの対象になる。
- 全15敵にドロップテーブルが設定されていることを確認。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

## 変更したファイル 2026-06-19

- `Assets/Proiject/Scripts/UI/JapaneseDisplayText.cs`
- `Assets/Proiject/ScriptableObjects/Items/*.asset`
- `Assets/Proiject/ScriptableObjects/Items/*.asset.meta`
- `Assets/Proiject/ScriptableObjects/Enemies/*.asset`

## 家のチャットからのメモ 2026-06-19

- 家側での開発作業を一時停止する。
- 次回このチャットで行う最初の操作は、学校側の最新作業内容を読み込むこと。
- 再開時は `handoff/SCHOOL_WORK_LOG.md`、`HANDOFF_HOME.md`、`handoff/SHARED_PROJECT_STATUS.md` を確認する。
- 学校側と家側の内容に食い違いがある場合は、従来どおり家側の内容を優先する。

## 家でやったこと 2026-06-19

- 鍛冶装備へ品質とランダム能力補正を追加。
- 品質は `粗悪`、`普通`、`良質`、`希少`、`伝説` の5段階。
- 粗悪は補正1つ。70%でマイナス補正になり、残りは小さなプラス補正。
- 普通は追加補正なし。
- 良質はプラス補正1つ。
- 希少は重複しないプラス補正2つ。
- 伝説は重複しないプラス補正3つ。補正値も他品質より高い。
- 補正対象は最大HP、攻撃、防御、攻撃速度。
- 品質抽選率は粗悪15%、普通40%、良質25%、希少15%、伝説5%。
- 市場の固定武器は従来どおりSO単位で扱い、鍛冶品を品質付き個体装備として生成。
- 品質付き装備を在庫、売却、傭兵詳細、装備比較、着脱、交換へ対応。
- 旧装備を交換・解除した際も品質とランダム補正を維持して在庫へ戻す。
- 品質付き装備個体のID、基礎武器、品質、補正、装備状態をJSONセーブへ追加。旧セーブの固定武器も読込可能。
- 品質による売値倍率を追加。粗悪65%、普通100%、良質120%、希少155%、伝説220%。
- 鍛冶直後の状態表示に完成品の品質を表示。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

## 変更したファイル 2026-06-19

- `Assets/Proiject/Scripts/Item/EquipmentQuality.cs`
- `Assets/Proiject/Scripts/Item/EquipmentQuality.cs.meta`
- `Assets/Proiject/Scripts/Item/EquipmentModifierType.cs`
- `Assets/Proiject/Scripts/Item/EquipmentModifierType.cs.meta`
- `Assets/Proiject/Scripts/Item/EquipmentModifier.cs`
- `Assets/Proiject/Scripts/Item/EquipmentModifier.cs.meta`
- `Assets/Proiject/Scripts/Item/EquipmentInstance.cs`
- `Assets/Proiject/Scripts/Item/EquipmentInstance.cs.meta`
- `Assets/Proiject/Scripts/Item/MerchantInventory.cs`
- `Assets/Proiject/Scripts/Blacksmith/BlacksmithManager.cs`
- `Assets/Proiject/Scripts/Mercenary/MercenaryInstance.cs`
- `Assets/Proiject/Scripts/Core/GameSaveData.cs`
- `Assets/Proiject/Scripts/Core/SaveManager.cs`
- `Assets/Proiject/Scripts/UI/JapaneseDisplayText.cs`
- `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`
- `Assembly-CSharp.csproj`
- `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`

## 家でやったこと 2026-06-19

- 傭兵名を除くプレイヤー向け表示を日本語化。
- タブ、ボタン、説明文、状態表示、治療、売買、戦闘ログ、ダンジョンイベントを日本語へ変更。
- 傭兵クラス、契約種別、アイテム種別、レアリティを日本語表示する `JapaneseDisplayText` を追加。
- 既存の `Slime` と `Monster Fang` は表示時に `スライム`、`魔物の牙` へ変換。
- enum名やクラス名などの内部識別子は、参照破損を避けるため英語のまま維持。
- 日本語を表示できるように `Yu Gothic UI`、`Yu Gothic`、`Meiryo`、`MS Gothic` の順でOSフォントを読み込むようにした。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

## 変更したファイル 2026-06-19

- `Assets/Proiject/Scripts/UI/JapaneseDisplayText.cs`
- `Assets/Proiject/Scripts/UI/JapaneseDisplayText.cs.meta`
- `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`
- `Assets/Proiject/Scripts/Battle/BattleManager.cs`
- `Assets/Proiject/Scripts/Dungeon/DungeonRunManager.cs`
- `Assets/Proiject/Scripts/Merchant/MarketPriceManager.cs`
- `Assembly-CSharp.csproj`

## 家でやったこと 2026-06-19

- 撤退時に報酬を失わない仕様を確定。戦闘報酬とイベント報酬は即時獲得のため、撤退後もそのまま保持される。
- ダンジョンごとの設定用に `DungeonDataSO` を追加。
- `DungeonDataSO` でダンジョン名、説明、遭遇回数、初回敵数、遭遇ごとの敵増加数を設定可能。
- 踏破時だけ追加されるゴールド報酬と複数のアイテム報酬を設定可能。
- `DungeonRunManager` が `Assets/Proiject/ScriptableObjects/Dungeons` からダンジョンSOを自動検出するようにした。
- 既存の `DungeonData.asset` をサンプルの「はじまりの洞窟」として使用。遭遇3回、踏破100G、魔物の牙2個に設定。
- DUNGEON画面にダンジョン名、遭遇回数、踏破ゴールド報酬を表示。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

## 変更したファイル 2026-06-19

- `Assets/Proiject/Scripts/Dungeon/DungeonDataSO.cs`
- `Assets/Proiject/Scripts/Dungeon/DungeonDataSO.cs.meta`
- `Assets/Proiject/Scripts/Dungeon/DungeonRunManager.cs`
- `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`
- `Assets/Proiject/ScriptableObjects/Dungeons/DungeonData.asset`
- `Assembly-CSharp.csproj`

## 家でやったこと 2026-06-19

- ダンジョンを5等級（低級、下級、中級、上級、最上級）に分類。
- 初期状態は低級のみ開放し、現在の最高開放等級を踏破すると次の等級を開放するようにした。
- 撤退や敗北では次の等級は開放されない。
- DUNGEON画面に5ダンジョンの一覧、等級、遭遇回数、踏破ゴールド、選択/未開放表示を追加。
- 探索中はダンジョン選択を変更できない。
- 既存の `DungeonData.asset` を低級 `はじまりの洞窟` として使用し、下級 `閉じられた廃坑`、中級 `霧の古代遺跡`、上級 `黒鉄の山岳要塞`、最上級 `深淵の王座` を追加。
- 等級が上がるごとに遭遇回数、敵数、踏破報酬が増えるように設定。
- 開放状態は現状、ゲーム再生中のみ保持。永続化は今後のセーブ機能で対応予定。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

## 変更したファイル 2026-06-19

- `Assets/Proiject/Scripts/Dungeon/DungeonDataSO.cs`
- `Assets/Proiject/Scripts/Dungeon/DungeonRunManager.cs`
- `Assets/Proiject/Scripts/UI/JapaneseDisplayText.cs`
- `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`
- `Assets/Proiject/ScriptableObjects/Dungeons/DungeonData.asset`
- `Assets/Proiject/ScriptableObjects/Dungeons/LowerMine.asset`
- `Assets/Proiject/ScriptableObjects/Dungeons/LowerMine.asset.meta`
- `Assets/Proiject/ScriptableObjects/Dungeons/MiddleRuins.asset`
- `Assets/Proiject/ScriptableObjects/Dungeons/MiddleRuins.asset.meta`
- `Assets/Proiject/ScriptableObjects/Dungeons/UpperFortress.asset`
- `Assets/Proiject/ScriptableObjects/Dungeons/UpperFortress.asset.meta`
- `Assets/Proiject/ScriptableObjects/Dungeons/HighestAbyss.asset`
- `Assets/Proiject/ScriptableObjects/Dungeons/HighestAbyss.asset.meta`

## 家でやったこと 2026-06-19

- ダンジョンの最高開放等級を `PlayerPrefs` へ保存するようにした。
- ダンジョン踏破で次等級が開放された直後に保存する。
- ゲーム起動時に保存済みの最高開放等級を読み込む。
- 不正な保存値は低級から最上級の範囲へ補正する。
- 保存上ロック中のダンジョンが選択されていた場合、最初の開放済みダンジョンへ戻す。
- `DungeonRunManager` のInspectorコンテキストメニューに `ダンジョン開放状態を初期化` を追加。
- 初期化すると保存データを削除し、低級のみ開放された状態へ戻る。
- `dotnet build DungeonMerchant.sln` は警告0・エラー0で成功。

## 変更したファイル 2026-06-19

- `Assets/Proiject/Scripts/Dungeon/DungeonRunManager.cs`

## 家で行ったこと 2026-06-19

- INVENTORY画面に縦スクロールを追加。
- 表示範囲外の在庫行をMaskで隠し、マウスホイールやドラッグで確認できるようにした。
- 通常アイテムと品質付き装備の合計行数に応じて、スクロール内容の高さを自動調整するようにした。
- `dotnet build DungeonMerchant.sln` は警告・エラー0で成功。

## 変更したファイル 2026-06-19

- `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`

## 家で行ったこと 2026-06-19

- MARKET画面に縦スクロールと表示範囲のMaskを追加。
- 市場候補を武器限定から、武器・防具・装飾品の全装備へ拡張。
- 既存の市場候補が登録済みでも、新しい市場装備を自動検出するようにした。
- 市場用ランク1防具として、鉄の鎧・革の鎧・見習いのローブを追加。
- 市場用ランク1装飾品として、兵士の指輪・羽根のお守り・魔力の首飾りを追加。
- 鍛冶屋の防具・装飾品は上位の品質付き装備として、鍛冶屋限定のまま維持。
- `dotnet build DungeonMerchant.sln` は警告・エラー0で成功。

## 変更した主なファイル 2026-06-19

- `Assets/Proiject/Scripts/Merchant/MarketStockManager.cs`
- `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`
- `Assets/Proiject/Scripts/UI/JapaneseDisplayText.cs`
- `Assets/Proiject/ScriptableObjects/Items/IronArmor.asset`
- `Assets/Proiject/ScriptableObjects/Items/LeatherArmor.asset`
- `Assets/Proiject/ScriptableObjects/Items/ApprenticeRobe.asset`
- `Assets/Proiject/ScriptableObjects/Items/SoldierRing.asset`
- `Assets/Proiject/ScriptableObjects/Items/FeatherCharm.asset`
- `Assets/Proiject/ScriptableObjects/Items/ManaPendant.asset`

## 家で行ったこと 2026-06-19

- 装備セットIDとセット効果を実装。
- 限定セット「古代守護者」の武器・防具・装飾品を追加。全職業が装備可能。
- 2部位で最大HP+30・防御+8、3部位で攻撃+12・攻撃速度+0.08が発動。
- 傭兵詳細画面にセット装備数と発動状態を表示。
- 品質ごとに装備名を色分け。粗悪は灰、普通は白、良質は緑、希少は青、伝説は橙。
- 装備詳細画面で品質、強化値、性能、追加効果、セット効果、売値を確認可能。
- 個体装備を最大+10まで強化可能。1段階ごとに基礎性能が10%増加。
- 装備名の横に強化値を `+数値` で表示し、強化費用と強化済み売値も実装。
- 市場購入装備も個体化し、詳細表示と強化の対象にした。
- 強化値をJSONセーブへ追加し、バージョン6へ更新。旧セーブは強化値0として読み込み可能。
- 宝箱・物資探索イベントと最終ボスから、古代守護者セットが低確率でドロップ。
- イベント確率は低級2%から最上級5%、ボス確率は低級5%から最上級12%。
- 限定ドロップにも品質とランダム追加効果が付く。
- `dotnet build DungeonMerchant.sln` は警告・エラー0で成功。

## 変更した主なファイル 2026-06-19

- `Assets/Proiject/Scripts/Item/ItemDataSO.cs`
- `Assets/Proiject/Scripts/Item/EquipmentInstance.cs`
- `Assets/Proiject/Scripts/Item/MerchantInventory.cs`
- `Assets/Proiject/Scripts/Mercenary/MercenaryInstance.cs`
- `Assets/Proiject/Scripts/Merchant/MarketStockManager.cs`
- `Assets/Proiject/Scripts/Core/GameSaveData.cs`
- `Assets/Proiject/Scripts/Core/SaveManager.cs`
- `Assets/Proiject/Scripts/Dungeon/DungeonDataSO.cs`
- `Assets/Proiject/Scripts/Dungeon/DungeonRunManager.cs`
- `Assets/Proiject/Scripts/UI/JapaneseDisplayText.cs`
- `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`
- `Assets/Proiject/ScriptableObjects/Items/AncientGuardianBlade.asset`
- `Assets/Proiject/ScriptableObjects/Items/AncientGuardianArmor.asset`
- `Assets/Proiject/ScriptableObjects/Items/AncientGuardianSeal.asset`
- `Assets/Proiject/ScriptableObjects/Dungeons/*.asset`

## 家で行ったこと 2026-06-19

- 装備ロックを実装。ロック中は売却不可だが、装備・交換・強化は可能。
- ロック状態を装備詳細と在庫へ表示し、JSONセーブ・ロードへ追加。
- 在庫に素材・武器・防具・装飾品・セット装備・ロック装備の絞り込みを追加。
- 装備を名前・品質・強化値・セットで並び替え可能にした。
- 強化専用素材 `強化鉱石` を追加し、全ダンジョンの踏破報酬へ設定。
- 強化はゴールドと強化鉱石を消費する。必要鉱石数は強化段階に応じて増加。
- 強化成功率を実装。+0～+1は100%、以降段階的に低下し、+9では30%。
- 強化失敗時も装備破壊・強化値低下はなし。費用と素材のみ消費。
- 既存鍛冶屋装備を使い、戦士「不屈の前衛」、弓兵「風狩り」、魔法使い「秘術賢者」の3セットを追加。
- 各追加セットに2部位・3部位効果を設定。
- 装備図鑑を追加。未入手・入手済み、装備種別、セット、入手場所、収集率を表示。
- 図鑑の入手履歴は売却後も残り、JSONセーブ・ロードへ対応。
- ダンジョン選択画面に、確定報酬、限定装備候補、イベント・ボスのドロップ率を表示。
- セーブバージョンを7へ更新。旧セーブではロックなし・図鑑履歴を現在所持装備から補完。
- 自動装備は方針どおり実装していない。
- `dotnet build DungeonMerchant.sln` は警告・エラー0で成功。

## 変更した主なファイル 2026-06-19

- `Assets/Proiject/Scripts/Item/ItemDataSO.cs`
- `Assets/Proiject/Scripts/Item/EquipmentInstance.cs`
- `Assets/Proiject/Scripts/Item/MerchantInventory.cs`
- `Assets/Proiject/Scripts/Mercenary/MercenaryInstance.cs`
- `Assets/Proiject/Scripts/Core/GameSaveData.cs`
- `Assets/Proiject/Scripts/Core/SaveManager.cs`
- `Assets/Proiject/Scripts/UI/JapaneseDisplayText.cs`
- `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`
- `Assets/Proiject/ScriptableObjects/Items/EnhancementOre.asset`
- `Assets/Proiject/ScriptableObjects/Items/GoblinHunterSword.asset`
- `Assets/Proiject/ScriptableObjects/Items/IronVanguardArmor.asset`
- `Assets/Proiject/ScriptableObjects/Items/ChampionEmblem.asset`
- `Assets/Proiject/ScriptableObjects/Items/BeastboneBow.asset`
- `Assets/Proiject/ScriptableObjects/Items/WindrunnerLeather.asset`
- `Assets/Proiject/ScriptableObjects/Items/HawkeyeCharm.asset`
- `Assets/Proiject/ScriptableObjects/Items/HexwoodStaff.asset`
- `Assets/Proiject/ScriptableObjects/Items/RunewovenRobe.asset`
- `Assets/Proiject/ScriptableObjects/Items/ArcanePendant.asset`
- `Assets/Proiject/ScriptableObjects/Dungeons/*.asset`

## 家で行ったこと 2026-06-19

- 強化鉱石をダンジョン等級に合わせた5段階へ分割。
- `+1～+2` は低級強化鉱石、`+3～+4` は下級強化鉱石を使用。
- `+5～+6` は中級、`+7～+8` は上級、`+9～+10` は最上級強化鉱石を使用。
- 装備の現在強化値から、次回強化に必要な鉱石を自動選択するようにした。
- 装備詳細画面へ必要な強化鉱石名と個数を表示。
- 低級・下級・中級・上級・最上級ダンジョンの踏破報酬へ、それぞれ対応鉱石を設定。
- 既存の `EnhancementOre.asset` は低級強化鉱石として再利用し、旧セーブの所持数を維持。
- `dotnet build DungeonMerchant.sln` は警告・エラー0で成功。

## 変更した主なファイル 2026-06-19

- `Assets/Proiject/Scripts/Item/MerchantInventory.cs`
- `Assets/Proiject/Scripts/UI/JapaneseDisplayText.cs`
- `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`
- `Assets/Proiject/ScriptableObjects/Items/EnhancementOre.asset`
- `Assets/Proiject/ScriptableObjects/Items/LowerGradeEnhancementOre.asset`
- `Assets/Proiject/ScriptableObjects/Items/MiddleGradeEnhancementOre.asset`
- `Assets/Proiject/ScriptableObjects/Items/UpperGradeEnhancementOre.asset`
- `Assets/Proiject/ScriptableObjects/Items/HighestGradeEnhancementOre.asset`
- `Assets/Proiject/ScriptableObjects/Dungeons/*.asset`

## 家で行ったこと 2026-06-19

- 探索画面の報酬プレビューと旧イベント説明文が重なっていた問題を修正。
- 重複していた旧説明文を削除し、ダンジョン一覧との間に余白を確保。
- 限定装備候補は個別名の長い列挙ではなく、セット名と種類数で短く表示。
- 探索上部をダンジョン情報・確定報酬・限定候補と確率の3行へ整理。
- `dotnet build DungeonMerchant.sln` は警告・エラー0で成功。

## 変更したファイル 2026-06-19

- `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`

## 家で行ったこと 2026-06-20

- 商人レベルと商人経験値を実装。依頼・ダンジョン探索で経験値を獲得する。
- 商人レベル1で日雇い、レベル2で臨時、レベル5で専属契約を解放。
- 雇用画面で解放済み契約を切り替え可能。商人レベルに応じて雇用成功率が上昇。
- 日雇いは契約当日のみ、臨時は7日間、専属は無期限。
- 契約切れ傭兵はパーティーから外れ、商会画面から費用を払って更新可能。
- 通常依頼を常時3件生成。素材納品または指定モンスター討伐、期限、ゴールド・商人経験値報酬に対応。
- 商会画面から依頼UIを開き、受注と進行状況確認が可能。
- `SpecialQuestSO` と最初の特殊依頼「商会設立記念・ゴブリン掃討」を追加。
- 量産型傭兵へ職業別の初歩パッシブスキルを追加。レベル2で取得。
- ネームド傭兵の固有スキル名、取得レベル、能力補正を `MercenaryDataSO` で設定可能。
- ダンジョン探索終了時に確定で1日経過し、探索日数とダンジョン等級に応じた探索費用を支払う。
- 休息または通路妨害イベントで追加1日が経過し、探索費用も増加。
- 探索終了時に日数と探索費用をリザルト表示。
- 倉庫容量を30、60、100、160枠の4段階に設定。商人レベル4、8、12で段階解放可能。
- 倉庫拡張には高額ゴールドが必要。後半2段階は日数経過ごとに維持費が発生。
- 在庫画面に倉庫使用量、維持費、倉庫拡張ボタンを追加。
- 長期目標として商人Lv10、黒字探索10回、資産50000G、踏破20回を追加。
- 依頼UIで長期目標の進捗を確認可能。
- 商人レベル、契約期限、依頼、倉庫、実績をJSONセーブへ追加し、バージョン8へ更新。
- 自動装備は方針どおり実装していない。
- `dotnet build DungeonMerchant.sln` は警告・エラー0で成功。

## 変更した主なファイル 2026-06-20

- `Assets/Proiject/Scripts/Merchant/MerchantData.cs`
- `Assets/Proiject/Scripts/Merchant/DayManager.cs`
- `Assets/Proiject/Scripts/Merchant/ProgressionManager.cs`
- `Assets/Proiject/Scripts/Merchant/SpecialQuestSO.cs`
- `Assets/Proiject/Scripts/Mercenary/MercenaryHireManager.cs`
- `Assets/Proiject/Scripts/Mercenary/MercenaryPartyManager.cs`
- `Assets/Proiject/Scripts/Mercenary/MercenaryInstance.cs`
- `Assets/Proiject/Scripts/Data/MercenaryDstsSO.cs`
- `Assets/Proiject/Scripts/Battle/BattleManager.cs`
- `Assets/Proiject/Scripts/Dungeon/DungeonRunManager.cs`
- `Assets/Proiject/Scripts/Item/MerchantInventory.cs`
- `Assets/Proiject/Scripts/Core/GameSaveData.cs`
- `Assets/Proiject/Scripts/Core/SaveManager.cs`
- `Assets/Proiject/Scripts/Core/DungeonMerchantBootstrap.cs`
- `Assets/Proiject/Scripts/UI/JapaneseDisplayText.cs`
- `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`
- `Assets/Proiject/ScriptableObjects/Mercenaries/MercenaryData.asset`
- `Assets/Proiject/ScriptableObjects/Quests/FirstSpecialQuest.asset`
- `Assembly-CSharp.csproj`

## 家で行ったこと 2026-06-22

- 商人専用の技能ポイントと、交渉・統率・鑑定・兵站の4能力を追加。
- 商人Lv1で技能ポイント2、以後レベルアップごとに1ポイントを獲得。各能力は最大Lv10。
- 各能力Lv3・Lv7で商人技能を自動習得する。
  - 交渉: 値切り術、商談の達人
  - 統率: 人を見る目、契約管理
  - 鑑定: 目利き、慧眼
  - 兵站: 荷役整理、遠征計画
- 交渉を市場の仕入れ価格・売却価格へ反映。
- 統率を雇用成功率・傭兵契約更新費へ反映。
- 鑑定を依頼のゴールド報酬・商人経験値報酬へ反映。
- 兵站を倉庫容量・探索費用へ反映。
- 商会画面へ商人能力一覧、技能ポイント、強化ボタン、習得技能表示を追加。
- 市場価格・依頼報酬・契約更新費などのUI表示も補正後の実値へ更新。
- 商人能力と未使用技能ポイントをJSONセーブへ追加し、セーブバージョン9へ更新。
- バージョン8以前のセーブには、現在の商人レベルに応じた未使用技能ポイントを補填する。
- `dotnet build DungeonMerchant.sln` は警告・エラー0で成功。

## 変更した主なファイル 2026-06-22

- `Assets/Proiject/Scripts/Merchant/MerchantData.cs`
- `Assets/Proiject/Scripts/Merchant/MarketPriceManager.cs`
- `Assets/Proiject/Scripts/Merchant/MarketStockManager.cs`
- `Assets/Proiject/Scripts/Merchant/ProgressionManager.cs`
- `Assets/Proiject/Scripts/Mercenary/MercenaryHireManager.cs`
- `Assets/Proiject/Scripts/Core/GameSaveData.cs`
- `Assets/Proiject/Scripts/Core/SaveManager.cs`
- `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`

## 家で行ったこと 2026-06-22 商人ステータスUI変更

- 商会タブ内に表示していた商人能力・ステ振り欄を撤去。
- 右上の商人レベル・所持金表示をクリック可能なボタンへ変更。
- 右上ボタンから開く「商人ステータス」専用オーバーレイを追加。
- 詳細画面で商人レベル、経験値、所持金、未使用技能ポイント、習得技能を確認可能。
- 交渉・統率・鑑定・兵站の現在Lvと実際の補正値、Lv3・Lv7技能を表示。
- 詳細画面内の `+1` ボタンから技能ポイントを割り振れる。
- 能力値変更時は専用画面、市場価格、依頼報酬などの表示を即時更新。
- `dotnet build DungeonMerchant.sln` は警告・エラー0で成功。

## 変更したファイル 2026-06-22 商人ステータスUI変更

- `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`

## 家で行ったこと 2026-06-23 マップ機能

- 大陸全体マップと町内施設マップを追加。
- ユーザー提供画像を構図・画風の参考に、大陸背景、町拠点、町内背景を制作して使用。
- 起動時の最初の画面を大陸マップへ変更。
- 大陸上にエルド交易都市、リーフ森林都市、セイル港湾都市を配置。
- 町を選択すると町内マップへ移動し、別の町への移動時は1日経過する。
- 現在いる町をセーブし、再起動後も現在地を復元する。
- 各町の近くに低級洞窟、森林遺跡、海蝕迷宮を配置。
- 全体マップまたは町内マップから近隣ダンジョンを選択できる。
- 町内マップに酒場、商会本部、市場、鍛冶屋、倉庫、編成所、治療院、訓練場、ダンジョン門を配置。
- 各施設ボタンは既存の雇用、商会、購入、鍛冶、在庫、編成、治療、戦闘、探索画面へ接続。
- 右上付近に大陸地図へ戻るボタンを追加。
- セーブバージョンを10へ更新。
- `dotnet build DungeonMerchant.sln` は警告・エラー0で成功。

## 変更した主なファイル 2026-06-23

- `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs`
- `Assets/Proiject/Scripts/Core/GameSaveData.cs`
- `Assets/Proiject/Scripts/Core/SaveManager.cs`
- `Assets/Proiject/Resources/Maps/ContinentMap.png`
- `Assets/Proiject/Resources/Maps/TownMap.png`
- `Assets/Proiject/Resources/Maps/TownMarker.png`
## 2026-06-27 羊皮紙UIと傭兵詳細画面

- 傭兵詳細を「ステータス」と「装備」の2画面に分割し、タブで切り替え可能にした。
- ステータス画面へ習得済み・未習得スキルの一覧を追加し、詳細ボタンから効果を確認できるようにした。
- 追加された羊皮紙画像の市松模様を透明化し、Unity用素材 `Resources/UI/ParchmentPanel.png` を追加した。
- 羊皮紙の透明余白を除外して9-slice表示し、傭兵詳細だけでなくメインUIと各種詳細・確認ウィンドウにも適用した。
- 羊皮紙上の見出しと本文を濃い茶色へ変更した。
- 通常ボタンを木色、一覧カードを濃い革色、選択状態を深緑、注意操作をえんじ色へ統一した。
- ボタンとカードへ金茶色の細枠を追加し、ボタン文字を生成り色にして視認性を調整した。
- 羊皮紙へ直接表示される各画面の見出し、説明、空欄メッセージを濃い茶色へ統一した。
- 濃いカード内、戦闘ログ内、地図上の文字は白・生成り色を維持した。
- 羊皮紙上の文字が薄く見えたため、本文を黒に近い濃茶、補足を濃い焦げ茶へ再調整した。
- 背景色のない羊皮紙上の文字は、見出し・本文・補足をすべて黒へ統一した。
- 共通UIフォントの第一候補を手書き風の `UD デジタル 教科書体 N` に変更し、未導入環境向けの明朝・ゴシック体フォールバックを維持した。
- 万年筆に近い筆圧感を出すため、共通UIフォントの第一候補を `游明朝 Demibold` へ変更した。
- 羊皮紙へ直接表示される黒文字は、指定にかかわらず太字で描画するようにした。
- 追加された `ZenKurenaido-Regular.ttf` をResources配下へ配置し、通常文・説明・会話ログへ適用した。
- 見出しとボタンは游明朝Demibold、本文はZen Kurenaidoを使う二書体構成にした。
- 近隣ダンジョン一覧を現在地の町に紐づくダンジョンだけ表示するよう変更した。
- ダンジョン選択時と探索開始時にも現在地を検証し、別の町のダンジョンへ入れないようにした。
- 地域マップのダンジョン名と移動先を `nearbyTownIndex` から生成し、低級ダンジョンがエルド付近に表示されていた誤配置を修正した。
- 「はじまりの洞窟」を全5フロアへ変更し、最終フロア完全攻略報酬を1000Gへ増額した。
- はじまりの洞窟限定の入門セット「鬼狩り」を追加した。
- 鬼狩りセットは全職装備可能な「鬼狩りの鉈」「鬼狩りの装束」「小鬼牙のお守り」の3部位。
- 鬼狩りセット効果は2部位で最大HP+10・攻撃+3、3部位でさらに攻撃+5・防御+2。
- はじまりの洞窟の限定ドロップを古代守護者セットから鬼狩りセットへ差し替えた。
- 街道戦闘を出発地と目的地の近隣ダンジョンにいる通常敵の混成編成へ変更し、ボスは除外した。
- セイル―リーフ街道は両地域の通常敵4体、リーフ―エルド街道は通常敵5体が出現する。
- 町の移動を隣接町だけに制限し、セイルからエルドへ直接移動できないようにした。
- 経由が必要な町は地域マップで「要経由」と表示し、選択不可にした。
- 地域マップへセイル―リーフ、リーフ―エルドを結ぶ道路を追加した。
- 町マップから訓練場を削除した。
- 街道戦闘専用画面を追加し、移動区間・専用説明・戦闘ログを表示するようにした。
- 街道戦闘中は全体マップ・町マップボタンを隠し、戦闘終了後に目的地または地域マップへ戻る。
- ダンジョン戦闘は既存の戦闘画面とログを引き続き利用する。
- 街道移動ごとに接敵回数を3～5回からランダム抽選し、全戦勝利した場合だけ町へ到着するようにした。
- 街道戦闘専用画面へ現在の接敵数と総接敵数を表示する。
- 連続する街道戦闘は前戦終了処理との衝突を避けるため、1フレーム後に次戦を開始する。
- ダンジョンは1フロアへの挑戦開始ごとに接敵回数を3～5回からランダム抽選する。
- 街道連戦の各勝利後に「次へ進む」「撤退する」を選べるようにした。
- 撤退時は日数を進めず、出発した町へ戻る。
- 敵データへ `EnemyCategory` を追加し、特殊区分 `MythicalBeast（幻獣）` を実装した。
- 幻獣は通常の経験値計算にデータ別倍率を掛け、今回追加した4体は経験値3倍。
- 等級7「霧牙狼」、等級5「雷角麒麟」、等級3「炎翼グリフォン」、等級1「星界竜」を追加した。
- セイル―リーフ街道では等級7、リーフ―エルド街道では等級5の幻獣が各接敵8%で通常敵1体と入れ替わって出現する。
- 等級1・3の幻獣は将来の町・街道用データとして追加し、現在は出現しない。
- ビルド確認は警告0件、エラー0件で成功。

## 2026-06-29 戦闘システム拡張

- 敵スキルを実装し、全敵が共通30%の確率で使用を試みるようにした。
- 敵スキルは等級に応じて `強撃`、`毒牙`、`麻痺の咆哮`を使用する。敵SOで個別指定も可能。
- 味方・敵共通の基本クリティカル率を8%、クリティカル倍率を1.5倍にした。
- 弓兵の連射使用後、次の2行動はクリティカル率が15%上昇する。
- 傭兵へ回避率ステータスを追加。戦士3%、弓兵10%、魔法使い6%。
- 敵SOにもクリティカル率と回避率の個別補正欄を追加した。
- 状態異常 `毒` と `麻痺` を追加。毒は行動開始時に最大HPの6%ダメージ、麻痺は次の行動を阻害する。
- 戦闘終了時に残った状態異常は傭兵へ保持され、セーブデータにも保存される。セーブバージョンは14。
- 消費アイテム効果の仕組みを追加し、`解毒薬` と `麻痺治し` を実装した。
- 消費アイテムを市場抽選へ追加し、在庫画面の `使用` ボタンから該当状態の傭兵を治療できる。
- 傭兵詳細へクリティカル率、回避率、状態異常を追加した。
- ビルド確認は警告0件、エラー0件で成功。

## 2026-06-29 ダンジョン特殊個体

- ダンジョン通常敵へ低確率の特殊個体抽選を追加し、1戦につき最大1体まで出現する。
- 特殊個体はHP・攻撃・防御が上昇し、ゴールドと経験値は通常個体の1.5倍。
- 取得スキルに応じて `凶暴な`、`猛毒の`、`震声の`、`鋭眼の`、`迅撃の`、`吸命の` の名前を付ける。
- 特殊個体用スキルとして `研ぎ澄ます`、`連撃`、`吸命` を追加した。
- ボス特殊個体は、そのダンジョンを一度完全攻略するまで抽選されない隠し要素とした。
- ボス特殊個体は通常特殊個体より低確率で、表示名に `異形の` が付く。
- ダンジョンごとに特殊個体スキルプールを設定した。
  - 低級: 強撃、毒牙
  - 下級: 強撃、毒牙、麻痺の咆哮
  - 中級: 強撃、麻痺の咆哮、研ぎ澄ます
  - 上級: 毒牙、研ぎ澄ます、連撃
  - 最上級: 麻痺の咆哮、研ぎ澄ます、連撃、吸命
- 通常特殊個体の出現率は低級6%から最上級10%、特殊ボスは2%から4%。
- ビルド確認は警告0件、エラー0件で成功。

## 2026-06-29 職業・転職システム

- 既存スキルの名前、消費魔力、威力、説明を `MercenarySkillDefinition` へまとめる基盤を追加した。
- 基本職に僧侶・盗賊・槍兵を追加し、量産傭兵の候補6職をランダム抽選するようにした。
- 新スキルとして僧侶の `治癒`、盗賊の `毒刃`、槍兵の `貫通突き` を戦闘へ実装した。
- Lv15で基本職から上位職へ転職できる。
- 通常上位職を各基本職2系統、合計12職追加した。
  - 戦士: 騎士／狂戦士
  - 弓兵: 狙撃手／レンジャー
  - 魔術師: 賢者／元素術師
  - 僧侶: 司祭／聖騎士
  - 盗賊: 暗殺者／忍者
  - 槍兵: 竜騎兵／守護槍兵
- 特殊職を各基本職1系統追加した。覇軍、幻獣使い、時詠み、聖者、影、竜騎士。
- ユニーク傭兵はLv15で通常上位職2択に加えて特殊職を選択できる。
- 量産傭兵は `秘伝の転職証` を所持している場合だけ特殊職が表示され、転職時に1個消費する。
- 転職施設 `転職神殿` をエルド交易都市の町施設として追加した。
- 転職後も基本職用装備を使用可能で、レベル・経験値・装備・習得情報を維持する。
- 完全攻略後に出現する特殊ボスへ秘伝の転職証を追加した。低級50%から最上級100%。
- セーブデータは転職後の職業を保存し、バージョン15へ更新した。
- ビルド確認は警告0件、エラー0件で成功。
- 次段階ではユニーク傭兵の追加、職業別スキル追加、モンスター追加スキルの拡張を予定。

## 2026-06-29 ダンジョン結果画面・戦闘倍速

- ダンジョンフロア攻略後に通常戦闘のスライム開始画面へ残る問題を修正した。
- ダンジョン戦闘画面から通常戦闘用のスライム説明と開始ボタンを削除した。
- 戦闘中はダンジョン名と現在フロアを表示する。
- フロア終了後に専用結果パネルへ必ず遷移するようにした。
- 未完全攻略時は `次のフロアへ進む` と `町へ戻る` を表示する。
- 完全攻略時や探索失敗時は `町へ戻る` を表示する。
- `次のフロアへ進む` は同じダンジョンの次フロア挑戦を開始する。
- 戦闘画面と街道戦闘画面へ速度切替ボタンを追加した。
- 戦闘速度は1倍、2倍、4倍を循環し、ログと報酬処理は省略しない。
- ビルド確認は警告0件、エラー0件で成功。

## 2026-06-30 常設メニュー

- メインUI右上へ常設の `メニュー` ボタンを追加した。
- メニューから傭兵一覧、パーティー編成、在庫確認、装備図鑑、商人情報、依頼確認、地域マップへ移動できる。
- 図鑑や情報確認など、施設の主要機能ではない画面を町施設へ戻らず利用可能にした。
- 市場購入、鍛冶、治療、転職など施設固有の主要操作は従来の施設に残した。
- 戦闘中は進行画面を維持するためメニューボタンを操作不可にした。
- メニューは羊皮紙背景と既存の木製ボタン意匠に統一した。
- ビルド確認は警告0件、エラー0件で成功。

## 2026-06-30 レベル上限・スキル習得・ユニーク傭兵

- 職段階ごとのレベル上限を追加した。
  - 基本職: Lv15
  - 通常上位職: Lv30
  - 特殊職: Lv50
- レベル上限では経験値成長を停止し、転職後に新しい上限まで再び成長できる。
- 傭兵詳細へ現在レベル／上限レベルと上限到達表示を追加した。
- 基本職はLv5・10、上位職はLv20・30、特殊職はさらにLv40・50でパッシブスキルを習得する。
- 習得パッシブは最大HP、攻撃、防御、最大魔力、速度、クリティカル率、回避率へ反映する。
- 転職前に習得した基本職スキルは転職後も維持される。
- 特殊職対応ユニーク傭兵を追加した。
  - 覇軍: 既存の戦士ユニーク `アイ`
  - 幻獣使い: 弓兵ユニーク `リン`
  - 時詠み: 魔術師ユニーク `セラ`
  - 聖者: 僧侶ユニーク `ミレイ`
  - 影: 盗賊ユニーク `クロウ`
  - 竜騎士: 槍兵ユニーク `レオン`
- ユニーク候補の読み込みを修正し、既存の候補が設定済みでもResources内の追加ユニークを読み込むようにした。
- 基本6職すべてが量産傭兵候補の抽選対象になるようにした。
- ビルド確認は警告0件、エラー0件で成功。
