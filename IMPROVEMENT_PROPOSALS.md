# DungeonMerchant 改善案(精査記録)

> 作成日: 2026-07-22
> 精査体制: sol(起草)/ luna(裏取り検証)/ メイン(統括監査)
> 対象: `Assets/Proiject/` 配下(Scripts / Editor / Tests)。Unity生成物・パッケージは対象外。
> ※ 実装は未着手。分析・提案のみ。

---

## 0. 検証済みの重大指摘(luna が実コードと照合済み)

| # | 指摘 | 実コード確認 |
|---|---|---|
| 1 | `ProgressionManager.CanStore()` が `inventory == null` で **`true` を返す fail-open** | `ProgressionManager.cs:89-94` |
| 2 | `MerchantInventory` が `progressionManager == null` で容量判定をスキップ | `MerchantInventory.cs:79,109,435` |
| 3 | セーブが**本番ファイルへ直接 `File.WriteAllText`**、.bak/原子的置換なし | `SaveManager.cs:97,136` |
| 4 | Bootstrap が `MercenaryHireManager` を同一式内で**二重 `FindObjectOfType`** | `DungeonMerchantBootstrap.cs:42-43` |
| 5 | `SimpleMercenaryHireUI` が **1129行**の肥大クラス | `SimpleMercenaryHireUI.cs` |

---

## 1. 高優先度

### [高] SimpleMercenaryHireUI を画面単位の Presenter/View に分割する
- 対象: `Assets/Proiject/Scripts/UI/SimpleMercenaryHireUI.cs:8,10,45,933`
- 問題点: 本体1,129行。partial 群も `CharacterEquipment` 1,049行、`BattleDungeon` 1,037行と巨大。19個以上の Manager/Service 参照、全ページの Widget 状態・画面生成・遷移・操作を一クラスが所有。partial はファイル分割のみで状態・依存は共有され、変更影響範囲を隔離できていない。
- 改善方針: 本体を Composition Root + ルーターに縮小。戦闘/装備/雇用/地図/市場をページごとの Presenter/View と入力モデルへ分離。各 Presenter は必要なサービスだけ受け取り、親の private field を直接共有しない。
- リスク/工数: 大

### [高] Manager 間の FindObjectOfType 依存解決を Composition Root に集約する
- 対象: `DungeonMerchantBootstrap.cs:41,46,75` / `SaveManager.cs:964` / `TransportManager.cs:591` / `DungeonRunManager.cs:720`
- 問題点: Bootstrap・SaveManager・TransportManager・DungeonRunManager・各UIが個別にシーン全体を探索。Bootstrap は `MercenaryHireManager` を同一式内で二度検索。操作のたびに `ResolveReferences()` を呼ぶ箇所も。複数インスタンス時の取得先が不明瞭、非アクティブは見つからず、初期化順に依存。
- 改善方針: Bootstrap で全生成・取得後に `Initialize(deps...)` で明示注入。Inspector 配置は `[SerializeField]` + `OnValidate` で不足検出。必須依存欠落時は黙って縮退せず起動時エラー。
- リスク/工数: 大

### [高] Bootstrap の AddComponent 即時ライフサイクルによる購読漏れを解消する
- 対象: `DungeonMerchantBootstrap.cs:46,69` / `MercenaryPartyManager.cs:20,22,25`
- 問題点: `AddComponent<T>()` 直後に `OnEnable()` が走るが、Bootstrap は順番に追加するため、`OnEnable` 時にまだ未追加の依存は `null`。後の `ResolveReferences()` で参照を発見してもイベント購読は再実行されない。現状は追加順で偶然成立しているだけで、新規 Manager 挿入や順序変更で壊れる。
- 改善方針: root を非アクティブにして全構築後に注入・有効化するか、購読を冪等な `Initialize/StartListening` に移す。購読済みフラグと依存差替時の旧購読解除も用意。
- リスク/工数: 中

### [高] セーブを原子的にし、破損時の復旧経路を追加する
- 対象: `SaveManager.cs:81,97,110,136`
- 問題点: 通常保存・マイグレーション後保存ともに本番ファイルへ直接 `File.WriteAllText`。書込み中の終了・容量不足・I/O例外で唯一のセーブが切り詰められうる。読込失敗はログのみでバックアップ復旧・破損隔離なし。`SaveGame()` に例外処理なし。
- 改善方針: 同一ディレクトリの一時ファイルへ書込み→再読込/検証後に置換。直前ファイルを `.bak` 保持し、主ファイル読込失敗時に復旧候補を提示。保存結果を戻り値/イベントでUIに伝達。
- リスク/工数: 中

### [高] SaveManager を集約・永続化・ファイルI/Oの責務に分割する
- 対象: `SaveManager.cs:7,12,55,964`
- 問題点: 1,002行が15種類以上の Manager を知り、参照探索・イベント購読・DTO作成・復元順制御・マイグレーション・JSON変換・ファイルI/O・自動保存を担当。機能追加のたびに SaveManager のフィールド・生成・復元・購読を同時変更する必要がある。
- 改善方針: 各領域に `ISaveParticipant` 相当の Capture/Restore を持たせ、Coordinator は参加者と復元順のみ管理。ファイル操作は `ISaveStorage`、JSON は Serializer、オートセーブは別コンポーネントへ。
- リスク/工数: 大

### [高] ストレージ制限を fail-open にしない
- 対象: `ProgressionManager.cs:89-92` / `MerchantInventory.cs:78`
- 問題点: `CanStore()` は `inventory == null` で `true` を返し、参照解決失敗時に容量制限が無効化。`MerchantInventory` も `progressionManager == null` なら制限判定を飛ばすため、構成不備がユーザー有利なデータ不整合として現れる。
- 改善方針: 必須依存を初期化時に検証し、不足時は追加操作を拒否+診断ログ。容量計算は Inventory の現在値を引数に取る純粋 Policy にし相互参照を排除。
- リスク/工数: 小

---

## 2. 中優先度

### [中] Resources.LoadAll("") の全走査と線形検索をキャッシュ化する
- 対象: `GameAssetRepository.cs:11,25,43,54` / `SaveManager.cs:935`
- 問題点: 型別検索のたびに Resources 全体をロード・配列化して線形走査。ID未発見時は `FindObjectsOfTypeAll`、旧名検索で再度 `LoadAll`。セーブ復元でアイテム・傭兵ごとに呼ぶと資産数×保存要素数の走査・割当。
- 改善方針: 型別に一度だけロードし、Persistent ID/名前の辞書を構築。重複IDは Editor 検証でエラー。長期的には単一カタログSOまたは Addressables ラベルへ移行。
- リスク/工数: 中

### [中] ツールチップの毎フレーム文字列生成をイベント駆動にする
- 対象: `UIHoverTooltipTrigger.cs:59,63,122,136`
- 問題点: 表示中は毎フレーム `BuildContent()` を呼び、補間文字列・連結で新規 string を生成し `Text.text` も毎回設定。値が変わらないフレームにもGC負荷とUI再構築が発生。
- 改善方針: HP/MP/状態異常などの変更イベント(または BattleVisualController の更新時)で dirty を立て、変化時のみ再生成。表示開始時の即時更新は維持。
- リスク/工数: 小

### [中] 乱数を注入可能にし、再現可能なゲーム進行にする
- 対象: `DungeonRunManager.cs:132,408,429,462` / `ProgressionManager.cs:350,357`
- 問題点: 遭遇数・敵・特殊個体・特殊ボス・クエスト対象/個数/期限がグローバルな `UnityEngine.Random` に直結。テスト順や他機能の乱数消費に左右され、失敗セーブやプレイヤー報告の再現が困難。
- 改善方針: `IRandomSource` を注入し、ラン単位/日単位の seed を保存。確率判定・候補選択を純粋ロジックへ分離し、0%/100%/空候補/境界値を決定論的にテスト。
- リスク/工数: 中

### [中] 主要なテスト穴を「失敗系・ライフサイクル・UI更新」に絞って補う
- 対象: `RuntimeUIPlayModeTests.cs:11,80` / `UIHoverTooltipTrigger.cs:32` / `SaveManager.cs:81` / `DungeonMerchantBootstrap.cs:46`
- 問題点: PlayMode の Runtime UI テストは「主要ページ生成」「アクティブページ一つ」中心。ツールチップ内容/更新/破棄、ページ操作→Manager 伝播、Bootstrap 追加順/購読成立を直接検証するテストが無い。SaveManager 既存テストも書込み中断・破損JSON・バックアップ復旧・マイグレーション書込み失敗を未カバー。
- 改善方針: まずセーブ破損/置換、Bootstrap 購読、容量制限の依存欠落、ツールチップ dirty 更新を追加。巨大UIは分割後に Presenter の EditMode テストを増やし、PlayMode は配線スモークに保つ。
- リスク/工数: 中

### [中] バランス値と表示値を型付き設定へ集約する
- 対象: `ProgressionManager.cs:7,40,357` / `UIHoverTooltipTrigger.cs:8,41,92`
- 問題点: 倉庫容量・費用・必要レベル・維持費・クエスト個数/期限/報酬・ツールチップ寸法/色がコード内の配列や数値に分散。倉庫の3配列は添字対応が暗黙で、要素数ずれで実行時例外・誤設定。
- 改善方針: 倉庫Tierは `StorageTierDefinition` リスト、クエスト生成値は Quest settings SO、UI寸法/色は `UITheme` に集約。Editor 検証で Tier 順序・要素数・確率範囲を確認。
- リスク/工数: 中

---

## 3. 低優先度

### [低] 命名・文字列・フォールバック規約を統一する
- 対象: `ProgressionManager.cs:356,364` / `SimpleMercenaryHireUI.cs:755` / `TitleSceneController.cs:26`
- 問題点: 表示名は日本語変換を使う箇所と `"Monster Fang"`, `"Slime"` 等の英語キー直保持が混在。フォント等の Resources パスも複数クラスに直書き。アセット名変更がセーブ互換・表示に波及しやすい。
- 改善方針: 保存/ロジックは安定ID、表示はローカライズキー。Resources パスはカタログ/テーマSOへ集約し、文字列リテラルを互換IDとして使わない。
- リスク/工数: 中

---

## 4. 推奨着手順(メイン統括監査による並べ替え)

sol の影響度順を「影響度 × 工数の費用対効果」で調整:

1. **ストレージ fail-open 修正**(工数小/確実な不具合)— 数行で潰せる。容量無制限バグは実プレイに直結。
2. **セーブ原子化 + 破損復旧**(工数中)— データ喪失は最も重大。
3. **Bootstrap 初期化・購読順の固定**(工数中)— 再現性の低い購読漏れ不具合を解消。DI移行の足場。
4. **SimpleMercenaryHireUI の段階分割**(工数大)— まず1ページを Presenter 化してテンプレート確立。
5. **FindObjectOfType の Composition Root 集約**(工数大)— 隠れ結合/初期化順/テスト容易性に共通で効く。段階導入。

---

## 補足

- 本来メイン実装担当の **`gpt-5.6-tera` はこのCodex/ChatGPTアカウントで非対応**(400エラー)。実装フェーズに入る前に config 上の tera 割り当てモデルの見直しが必要。
- 本記録は精査段階の成果物。各項目の着手時は個別に設計レビューを行うこと。
