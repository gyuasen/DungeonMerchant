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
