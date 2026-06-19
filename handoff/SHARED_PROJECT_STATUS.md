# 共有プロジェクト状況

このファイルは、家側チャットと学校側Codexの両方が編集する全体状況メモです。

## 運用ルール

- 家側と学校側の両方が、このファイルを更新してよい。
- 現在のプロジェクト状態、決定事項、未解決事項、次にやることを書く。
- 内容が食い違う場合は家側の内容を優先する。
- 細かい作業履歴は `SCHOOL_WORK_LOG.md` や `FROM_HOME_CHAT.md` に書き、このファイルには全体状況を短くまとめる。

## 現在の全体状況

- プロジェクト名: `DungeonMerchant`
- ジャンル: 商人ローグライクダンジョンRPG
- Unity: `2022.3.62f3`
- メインシーン: `Assets/Proiject/Scenes/SampleScene.unity`
- 主な作業フォルダ: `Assets/Proiject`

## 実装済み

- 商人の所持金管理
- 傭兵データのScriptableObject
- 傭兵候補のランダム生成
- 量産型傭兵は戦士・弓兵・魔法使いの3職から生成。候補3人以上なら各職最低1人を保証
- 個別SO傭兵の自動検出表示
- 傭兵の雇用
- 最大3人のパーティー編成
- 戦闘参加傭兵の経験値獲得、レベルアップ、職業別能力成長
- 再生中は傭兵ごとの現在HPを保持し、戦闘後にHPを持ち越し
- `HEAL` タブでゴールドを払って傭兵を全回復
- 日付進行時に雇用済み傭兵が自然回復
- 戦闘不能者は通常より高額な復帰治療が必要で、自然回復では復帰しない
- 味方パーティー vs 敵グループの複数対複数自動戦闘
- `DUNGEON` タブから複数回の連続戦闘を行うダンジョンラン
- ダンジョン戦闘間のランダムイベントと3択（回復、報酬、危険行動、撤退）
- `DungeonDataSO` でダンジョンごとの遭遇設定、踏破ゴールド、踏破アイテムを設定可能
- ダンジョンは低級、下級、中級、上級、最上級の5等級
- 低級から開始し、踏破ごとに次の等級を段階開放
- ダンジョンの最高開放等級を `PlayerPrefs` へ保存し、ゲーム再起動後も維持
- JSONセーブで所持金、日数、在庫、雇用傭兵、HP、パーティー、ダンジョン進行を保存・復元
- モンスターは1等級が最強、10等級が最弱の10段階
- ダンジョンごとに通常敵候補と最終戦ボスを設定
- 最終遭遇では通常敵編成にボス1体が追加される
- 通常敵素材9種、ボス固有遺物5種を追加し、既存素材と合わせてアイテム15種
- 装備データに武器・防具・装飾品スロット、職業制限、ランク、能力補正を追加
- 戦士・弓兵・魔術師それぞれに初期武器と一段上の武器を追加
- 全モンスターへ正式なドロップテーブルを設定
- バトルログは1つのスクロール表示で全件保持し、最新行へ自動スクロール。味方攻撃は青、敵攻撃は赤、報酬は緑。
- 勝利時のゴールド報酬
- アイテム基本データ `ItemDataSO`
- 商人在庫 `MerchantInventory`
- 戦闘勝利時のアイテム戦利品追加
- `INVENTORY` タブで在庫確認と売却
- 日数管理 `DayManager`
- 日ごとの市場価格変動 `MarketPriceManager`
- `INVENTORY` タブの `NEXT DAY` で日付を進め、売却価格を更新
- 日ごとの仕入れ商品生成 `MarketStockManager`
- `MARKET` タブではモンスター素材ではなく、各職の武器を仕入れて商人在庫へ追加
- `鍛冶` タブでモンスター素材とゴールドを消費し、市場では買えないランク3武器を制作
- `COMPANY` タブは雇用人数が増えてもスクロール表示
- `COMPANY` の詳細ボタンから傭兵の全能力・経験値・契約・状態を確認可能
- ランタイム生成の簡易UGUI
- 傭兵名以外のプレイヤー向けUI、戦闘ログ、ダンジョン表示を日本語化
- シーン上の管理GameObjectが消えていても、再生時に最低限の管理オブジェクトとUIを自動生成するブートストラップ

## 注意点

- `Assets/Proiject/Scripts/Merchant/NewBehaviourScript.cs` は現在 `MerchantData.cs` にリネーム済み。
- IDEの古いタブに `NewBehaviourScript.cs` が残っている場合がある。
- `Assets/Proiject` はスペルが `Project` ではなく `Proiject`。
- 学校環境ではGitコマンドと `.git` フォルダを確認できなかった。
- 家/学校間でシーン上のGameObject配置が消えることがあったため、`DungeonMerchantBootstrap` でランタイム自動生成する方針にした。
- 個別SO傭兵は `SimpleMercenaryHireUI` が `Resources` とEditor中の `AssetDatabase` から自動収集する。
- スライム敵データは `BattleManager` が再生時に自動検出し、見つからない場合はランタイム仮スライムを作る。
- 敵グループ未設定時は、スライムを複数体出すフォールバック戦闘になる。
- HPが0の傭兵は戦闘中は行動不能扱い。
- 経験値は勝利時に参加傭兵へ均等配分。敵経験値は強さ段階の二乗で累進し、ボスは2倍。
- レベル上限は99。必要経験値は `100 + 40n + 10n²` の累進式。
- 経験値とレベルアップ後能力値はJSONセーブ対象。
- 治療費は現状、失ったHP 1あたり2G。日付進行時の自然回復は10HP。
- 戦闘不能治療は通常治療費の5倍に復帰処置500Gを加算。
- ダンジョンランは敵数が増える3連戦で、戦闘間に休憩所、宝箱、危険な通路イベントが発生する。
- 撤退時の報酬没収はなし。探索中に得た戦闘・イベント報酬は保持し、踏破時のみダンジョンSOの追加報酬を付与する。
- 内部のenumやクラス名は英語のまま維持し、`JapaneseDisplayText` で表示だけ日本語へ変換する。
- 全体セーブは `Application.persistentDataPath/game-save.json` を使用。主要なランタイム進行を自動保存する。
- 等級配分は低級10・9/ボス7、下級8・7/ボス6、中級6・5/ボス4、上級4・3/ボス2、最上級2・1/ボス1。
- 通常素材は高等級ほど高価で低確率、ボス固有遺物は確定ドロップ。
- 装備着脱処理とセーブは未実装。詳細ステータス画面へ装備欄を追加する方針。
- モンスター素材は戦利品・売却品として維持し、市場の購入候補からは除外。
- 鍛冶限定武器は戦士・弓兵・魔術師に1種ずつ。現状は固定性能で、ランダム品質・接辞は次段階。

## 未確認・問題

- Unity再生時にブートストラップ経由でUIが表示されるか、実機確認がまだ必要。
- UIが出ない場合はUnity Consoleの赤エラー全文を確認する。

## 次にやること

- UI表示確認
- `MerchantDate` などの名前タイポ修正
- `EnemyDateSO.cs` / `MercenaryDstsSO.cs` のファイル名タイポ修正
- アイテムの種類追加
- 敵ごとの正式なドロップテーブル設定
- 仕入れ/販売価格変動システムの強化
- 仕入れ商品の種類と価格バランス調整
## 2026-06-19 School Update

- Character detail now supports weapon equip, swap, and unequip.
- Only weapons compatible with the mercenary class are shown.
- Equipping consumes one inventory item; swapping or unequipping returns the previous weapon.
- Weapon bonuses affect displayed stats and battle stats.
- Equipped weapons and base stats are saved and restored without duplicate bonuses.
- Build verification succeeded with 0 warnings and 0 errors.
## 2026-06-19 School Update

- Character detail equipment rows now compare candidate weapons against the equipped weapon.
- HP, attack, defense, and attack-speed differences are color-coded.
- The equipment area uses larger text, taller rows, and scrolling for improved readability.
- The left-side character details now show the equipped weapon name.
- Build verification succeeded with 0 warnings and 0 errors.
## 2026-06-19 School Update

- Equipped weapons are stored per mercenary using the weapon asset name.
- Equipped weapons are restored when loading the JSON save.
- Equip, swap, and unequip operations now trigger an explicit save after the full transaction completes.
- Build verification succeeded with 0 warnings and 0 errors.
