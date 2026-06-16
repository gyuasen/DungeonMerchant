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
- 傭兵の雇用
- 最大3人のパーティー編成
- 敵1体との簡易自動戦闘
- 勝利時のゴールド報酬
- ランタイム生成の簡易UGUI
- シーン上の管理GameObjectが消えていても、再生時に最低限の管理オブジェクトとUIを自動生成するブートストラップ

## 注意点

- `Assets/Proiject/Scripts/Merchant/NewBehaviourScript.cs` は現在 `MerchantData.cs` にリネーム済み。
- IDEの古いタブに `NewBehaviourScript.cs` が残っている場合がある。
- `Assets/Proiject` はスペルが `Project` ではなく `Proiject`。
- 学校環境ではGitコマンドと `.git` フォルダを確認できなかった。
- 家/学校間でシーン上のGameObject配置が消えることがあったため、`DungeonMerchantBootstrap` でランタイム自動生成する方針にした。

## 未確認・問題

- Unity再生時にブートストラップ経由でUIが表示されるか、実機確認がまだ必要。
- UIが出ない場合はUnity Consoleの赤エラー全文を確認する。

## 次にやること

- UI表示確認
- `MerchantDate` などの名前タイポ修正
- `EnemyDateSO.cs` / `MercenaryDstsSO.cs` のファイル名タイポ修正
- `ItemDataSO` と商人インベントリの実装
- 戦闘報酬としてアイテムドロップを追加
