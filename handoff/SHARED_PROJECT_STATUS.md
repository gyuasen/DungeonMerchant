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
- 個別SO傭兵の自動検出表示
- 傭兵の雇用
- 最大3人のパーティー編成
- 味方パーティー vs 敵グループの複数対複数自動戦闘
- バトルログは1つのスクロール表示で全件保持。味方攻撃は青、敵攻撃は赤、報酬は緑。
- 勝利時のゴールド報酬
- アイテム基本データ `ItemDataSO`
- 商人在庫 `MerchantInventory`
- 戦闘勝利時のアイテム戦利品追加
- `INVENTORY` タブで在庫確認と売却
- 日数管理 `DayManager`
- 日ごとの市場価格変動 `MarketPriceManager`
- `INVENTORY` タブの `NEXT DAY` で日付を進め、売却価格を更新
- 日ごとの仕入れ商品生成 `MarketStockManager`
- `MARKET` タブで仕入れ商品を購入し、商人在庫へ追加
- `COMPANY` タブは雇用人数が増えてもスクロール表示
- ランタイム生成の簡易UGUI
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
