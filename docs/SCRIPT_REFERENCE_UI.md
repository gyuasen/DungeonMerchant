# スクリプト解説（UI層）

Assets/Proiject/Scripts/UI/ 配下。全72ファイル。  
（ゲームロジック層は [SCRIPT_REFERENCE_CORE.md](SCRIPT_REFERENCE_CORE.md) を参照）

作成日: 2026-08-13

---

## この層の構造

`SimpleMercenaryHireUI` がシーン上の各Managerを解決し、画面・オーバーレイ・Controller・Presenterを組み立てるcomposition rootである。  
Controllerは雇用、経済、戦闘、移動などの操作をManagerへ伝え、表示に必要な文言や並び順も作成する。  
Presenterは非MonoBehaviourとして、依存するView参照とドメイン依存を受け取り、ページ構築と再描画を担う。  
View / PageUIは行やボタンなどのUnity UIを生成し、渡されたコールバックと表示データを使って画面を更新する。  
オーバーレイViewは個別のモーダルを構築・表示・非表示にし、図鑑ではPresenterがデータを作ってViewがBookPageUIへ渡す。  
基盤クラスはページの表示切替、共通テーマ、UI生成、ツールチップ、表示文言・画像の解決を提供する。

---

## composition root（5ファイル）

シーンの参照を解決し、各機能のController、Presenter、Viewを接続するpartialクラス群である。画面固有の処理はPresenterへ委譲しつつ、MonoBehaviourのライフサイクルとコルーチンを必要とする処理を保持する。

### SimpleMercenaryHireUI.BattleDungeon.cs（313行）

戦闘、ダンジョン、街道戦闘のイベントを受け取り、ログ更新、画面遷移、効果音、戦闘演出コントローラとの接続を行う。戦闘・ダンジョン完了時には演出の完了を待ってから結果処理を進め、進行中の結果を保留する。`IEnumerator`と`StartCoroutine`、`MonoBehaviour`の有効状態に依存するため、これらの待機・演出完了処理はPresenterへ移さず本体側に置かれている。

### SimpleMercenaryHireUI.cs（2,033行）

UIシーンのcomposition rootとして、各種Manager、UIプレハブ、候補傭兵を参照し、起動時にUI全体と機能別のController／Presenterを構築する。ページ、オーバーレイ、タブ、共通ステータス表示を保持し、Managerのイベントを各画面の再描画や結果表示へ接続する。装備詳細Viewのインターフェース実装はCharacterEquipmentOverlayPresenterへ委譲し、雇用・経済・地図・戦闘なども各Presenterへ中継する。ストーリー表示、日次結果、戦闘結果の順序制御にはコルーチンと保留フラグを用いる。

### SimpleMercenaryHireUI.Map.cs（52行）

グローバル／ワールド／街マップの構築、表示、再描画をMapPresenterへ委譲するpartialファイルである。街道戦闘後に移動を継続するコルーチンと、ダンジョン解放・隠し島解放の同期処理を持つ。コルーチンはMonoBehaviourのライフサイクルで実行する必要があるため、意図的にcomposition root本体側に残されている。

### SimpleMercenaryHireUI.ScrollHelpers.cs（70行）

スクロール領域のContent、見出し、ラベル、ボタンを生成する共通ヘルパーを定義する。各要素のアンカー、サイズ、色、フォントを設定して、オーバーレイ内の縦長コンテンツを組み立てる。生成をpartialファイルに集約し、個別画面から同じレイアウト処理を利用する。

### SimpleMercenaryHireUI.UIFactory.cs（523行）

グローバルメニュー、Canvas、ページ、オーバーレイ、共通ボタンを生成し、ページ切替とdirty pageの再描画を管理する。UIPageRouterを使って現在ページを切り替え、非表示中に更新要求があったページは表示時に更新する。プレハブを使える場合はそこからボタンを生成し、なければコードでUI要素を作る。

## Controller（13ファイル）

Controllerはユーザー操作やManagerのイベントを受け、ドメイン操作と表示用データの作成を行う。MonoBehaviourではないControllerはcomposition rootから必要な依存を受け取る。

### BattleVisualController.cs（1,111行）

BattleManagerのプレゼンテーションイベントを購読し、味方・敵のスロット、HP、状態異常、ダメージ表示、勝敗表示をUnity UI上で演出するMonoBehaviourである。イベントキューをコルーチンで順に再生し、速度変更、ポーズ、即時完了では待機時間と未再生イベントの適用を切り替える。OnEnable／OnDestroyで購読を管理し、演出完了を通知する。

### CharacterEquipmentController.cs（882行）

傭兵の装備・消耗品の装着解除、強化、ロック、売却、使用を処理し、装備詳細Viewを更新する。装備比較、品質色、セット効果、スキル情報、図鑑用の装備説明テキストも生成する。所持品内にある装備だけを対象にする確認と、EquipmentInstanceを扱う経路を分けている。

### DailyResultController.cs（1,019行）

日次結果に表示するゴールド取引、クエスト、訓練、遠征、輸送、リモート販売、傭兵・所持品の変化を記録し、結果テキストへ整形する。日ごとの傭兵スナップショット、パーティ構成、既知装備を保持して前日との差分を算出する。ゴールド台帳では返金済み取引を除外して理由別の明細を作成する。

### DungeonBattleController.cs（341行）

パーティ戦闘とダンジョン探索の開始、ダンジョン選択、イベント選択、近くのダンジョンを開く操作をManagerへ伝える。戦闘速度、ポーズ、スキップ、戦闘ログの消去・追記も制御し、ログ種別ごとにリッチテキストの色を付ける。報酬プレビューはDungeonDataSOから構築する。

### EconomyController.cs（549行）

インベントリ、マーケット、鍛冶屋で表示する品目をフィルタ・カテゴリ・並び順に基づいて返し、売却、購入、作成を実行する。登録済みの購入・作成ボタンの操作可否を更新し、一括売却対象と合計額も求める。インベントリ、マーケット、鍛冶屋のサイドバー状態をそれぞれ別のenumで保持する。

### EndingSceneController.cs（233行）

エンディングシーンのCanvas、背景、メッセージ、クレジット、タイトルへ戻るボタンを実行時に構築するMonoBehaviourである。開始時にSaveManagerから状態を確認し、終了済みのセーブを消去してタイトルシーンへ戻る操作を提供する。画面要素はコードで配置し、フェード表示にはコルーチンを用いる。

### FacilityGreetingController.cs（150行）

施設に初めて入る際の挨拶表示の可否と、挨拶済み状態の保存を管理する。施設キーごとの表示文と、入場時に実行するコールバックを扱う。PlayerPrefsのキーにより、施設ごとの既読状態を保持する。

### HireAndPartyController.cs（354行）

候補傭兵の雇用・解雇、契約変更、パーティへの加入・離脱、回復処理をMercenaryHireManagerとMercenaryPartyManagerへ伝える。候補や所属傭兵の表示に必要な詳細、契約条件、回復費用・可否を作成する。契約変更や解雇の確認対象を保持して、確認オーバーレイから実行する。

### MerchantStatusAndQuestController.cs（249行）

商会のゴールド、借金、日数、進行度と、クエスト一覧・達成報酬を表示用テキストへ変換する。クエスト表示と商人ステータス表示を別々に再構築できる。各Managerから数値を読み取る表示専用の集約点として機能する。

### RemoteSaleController.cs（140行）

リモート販売に出せる在庫、販売枠、価格、経過日数を取得し、出品・取り消し操作をRemoteSaleManagerへ伝える。販売結果イベントを受けて画面を更新できるようコールバックを公開する。表示候補はアイテムスタックの数量や出品済み状態で絞り込む。

### TitleSceneController.cs（374行）

タイトルシーンの背景、タイトル、メニュー、新規開始確認、設定ウィンドウをコードで構築するMonoBehaviourである。セーブの有無に応じて「続きから」の可否を更新し、新規ゲーム、音量変更、終了を処理する。背景は縦グラデーションのSpriteを実行時に生成する。

### TownTravelController.cs（667行）

街・ダンジョンへの移動要求、確認、街道戦闘、継続・撤退、積荷の受取と敗北時の積荷処理を管理する。移動先の地域解放条件やゲート街の攻略状態を判定し、移動結果と積荷結果を結合したメッセージを返す。街道戦闘の背景キーを出発地・到着地から作り、戦闘開始処理へ渡す。

### TutorialController.cs（140行）

チュートリアルの表示済み状態、現在ステップ、前後移動を管理し、TutorialOverlayViewの文言とボタン状態を更新する。未完了時のみ表示する入口と、完了状態をリセットする静的メソッドを持つ。進行状態はPlayerPrefsに保存する。

## Presenter（10ファイル）

PresenterはUnityのライフサイクルを持たず、Viewの構築・更新とManagerから得るデータの橋渡しを行う。生成したボタンの操作はControllerやナビゲーション用コールバックへ接続する。

### BattleDungeonPresenter.cs（534行）

通常戦闘、街道戦闘、ダンジョンの各ページとダンジョンイベント用オーバーレイを構築・更新する。戦闘ログの高さ・スクロール位置、戦闘演出用の効果音、ダンジョン踏破結果、次階層・帰還の操作を扱う。View参照、ドメイン依存、コールバック、ナビゲーションを別の依存クラスとしてコンストラクタに渡す。

### CharacterEquipmentOverlayPresenter.cs（1,464行）

装備詳細、装備図鑑、傭兵詳細、装備・消耗品スロット選択の各オーバーレイを構築して表示を更新する。装備候補、装着済み装備、スキル、セット効果を行単位で生成し、選択・強化・売却・ロックなどをCharacterEquipmentControllerへ接続する。エディタ向けには装備発見状態を変更するデバッグボタンも構築する。

### EconomyPresenter.cs（722行）

インベントリ、マーケット、鍛冶屋のページと、アイテム詳細・売却確認・数量選択・倉庫拡張確認のオーバーレイを構築する。EconomyControllerから取得した行を表示し、在庫・価格・作成の変更イベントで必要なページとボタン状態を更新する。View依存、ドメイン依存、ナビゲーションを依存クラスに分けて受け取る。

### ExpeditionOverlayPresenter.cs（427行）

ダンジョン遠征の開始・管理オーバーレイを構築し、遠征可能な傭兵、編成、残日数、報酬見込みを表示する。遠征の開始、回収、キャンセルなどの操作をDungeonExpeditionManagerへ接続し、状態変更で表示を再構築する。ダンジョンごとの遠征有無とアクション表示可否も公開する。

### HirePartyPresenter.cs（455行）

雇用、商会、パーティ、回復、転職のページと昇格・解雇・契約変更・輸送のオーバーレイを構築する。HireAndPartyControllerを呼び出すボタンを作り、傭兵・候補・パーティ・回復状態の変更に応じて該当ページを再描画する。ページ切替と表示更新はコンストラクタで受け取るコールバックへ委譲する。

### ItemCodexPresenter.cs（66行）

全ItemDataSOを読み込み、アイテム種別と名称で並べ替えてBookPageUI用のEntry一覧に変換する。各Entryに日本語名、説明、アイコン、ItemCodexManagerの発見状態を設定する。非保存用のアセットを除外して図鑑対象を作る。

### MapPresenter.cs（557行）

グローバル、ワールド、街マップのページ、移動確認、積荷選択のUIを構築し、現在地・解放状態・移動先を表示する。TownTravelControllerを通じて街・ダンジョン移動と確認を行い、地図表示の更新やタブ状態も制御する。コンストラクタ引数は`ViewDependencies`、`DomainDependencies`、`Callbacks`、`Navigation`の依存束に整理され、UI参照・Manager・処理・画面遷移を分離して受け取る。

### MerchantQuestOverlayPresenter.cs（628行）

商人ステータスとクエストのオーバーレイを構築し、MerchantStatusAndQuestControllerが作る内容を表示する。クエストの詳細、達成報酬、受取操作、および商会の各数値表示を更新する。オーバーレイの生成、表示、非表示と、行コンテンツの再構築をこのPresenterに集約する。

### MonsterCodexPresenter.cs（74行）

EnemyDataSOを読み込み、特殊個体とDontSaveアセットを除外してモンスター等級順に並べる。各敵をBookPageUI.Entryへ変換し、日本語名、能力値、カテゴリ、最大2件のドロップ、画像、遭遇済み状態を設定する。Viewはこのデータを受け取るだけにし、図鑑のデータ整形と発見判定をPresenter側へ分離している。

### TrainingGroundPagePresenter.cs（240行）

訓練所ページを構築・表示し、傭兵ごとの訓練内容、開始可否、利用不可理由、訓練状態をTrainingGroundPageUIへ渡す。訓練開始操作をManagerへ接続し、訓練所の状態変更時にページを更新する。町の進行状態と日数を使って訓練可否を判定する。

## View / PageUI（28ファイル）

この区分はページ、モーダル、表示用コンポーネントであり、受け取った表示データとコールバックからUnity UIを構築する。ドメイン操作そのものはControllerまたはPresenterに委譲する。

### BattlePageUI.cs（3行）

戦闘ページ用の型で、RefreshOnlyPageUIBaseを継承する。個別の描画ロジックは持たず、登録された再描画コールバックを利用する。

### BlacksmithPageUI.cs（200行）

鍛冶屋のレシピ一覧をスクロール行として生成し、必要素材、作成費用、作成ボタンを表示する。ConfigureBlacksmithでレシピ供給元と作成コールバックを受け取り、Refreshで一覧を作り直す。素材アイコンはItemDataSOの画像から生成する。

### BookPageUI.cs（209行）

図鑑用の見開き本UIを生成し、Entry一覧を左右ページに分けて表示する。前後ボタンで表示開始位置を変更し、各項目に名前、説明、Sprite、未発見状態を表示する。ページ本体をコードで構築するMonoBehaviourである。

### CompanyPageUI.cs（377行）

商会所属傭兵の一覧を作り、並び順と職業フィルタを切り替えられる。傭兵ごとに能力、装備、所属・移動状態を表示し、詳細表示用のコールバックを行へ接続する。ソートモードと職業フィルタをページ内の状態として保持する。

### ContractDetailsOverlayView.cs（213行）

雇用候補または傭兵の契約詳細を列形式のオーバーレイに表示する。Buildでウィンドウと列を作り、Showのオーバーロードで候補種別に応じた契約データを設定する。共通の表示処理を私有Showメソッドへ集約している。

### DailyResultOverlayView.cs（142行）

一日の結果テキストを表示するスクロール可能なオーバーレイを構築する。Show、Hide、AppendTextを提供し、閉じるボタンで指定されたコールバックを実行する。結果文の生成はDailyResultController側に分離されている。

### DungeonPageUI.cs（249行）

ダンジョン選択リストを構築し、ダンジョン名、敵等級の要約、報酬などを行に表示する。選択時コールバックと再描画コールバックを別途設定できる。選択一覧の更新を通常のページRefreshから分離している。

### EquipmentSpecialCodexPageUI.cs（196行）

特殊装備図鑑のリストを作り、装備名、発見状態、詳細表示ボタンを表示する。EquipmentSpecialPageModelから取得した行を使い、タブや詳細表示のコールバックを設定する。通常装備の図鑑とは別のPageUIとして構成する。

### FacilityGreetingOverlayView.cs（142行）

施設の初回挨拶を表示するモーダルを構築し、タイトル、本文、入場・閉じる操作を表示する。Showで施設名と文言を更新し、Hideでオーバーレイを閉じる。入場判断と既読管理はFacilityGreetingControllerに委譲する。

### GlobalMapPageUI.cs（3行）

グローバルマップページ用の型で、RefreshOnlyPageUIBaseを継承する。表示更新は外部から登録されたコールバックで行う。

### HealPageUI.cs（124行）

回復対象の傭兵を一覧表示し、HP状態、回復費用、回復操作を行ごとに配置する。Configureで対象列挙と実行コールバックを受け、Refreshで行を再生成する。リストの共通生成はListPageUIBaseを継承して使う。

### HirePageUI.cs（614行）

雇用候補の一覧、候補詳細、契約確認、契約変更確認、解雇確認のUIを構築する。候補の能力・契約内容を表示し、雇用、詳細、確認操作のコールバックを各ボタンに結び付ける。候補リストと複数の確認オーバーレイを同じPageUI内で保持する。

### InventoryPageUI.cs（317行）

所持品と装備品をカテゴリ別に表示し、アイテム詳細、使用、売却、装備操作へ進むための行を作る。フィルタ・ソートのラベルとサイドバー選択を反映してRefreshする。ItemDataSOのアイコンと数量を行へ表示する。

### ItemCodexOverlayView.cs（93行）

アイテム図鑑のオーバーレイを作り、BookPageUIを内部に配置する。Show時にItemCodexPresenterからEntry一覧を取得して本UIへ渡す。表示構築と表示切替だけを担当し、一覧データの作成はPresenterに任せる。

### JobChangePageUI.cs（224行）

傭兵の転職候補と昇格条件を一覧表示し、転職・プレビュー操作を行に接続する。職業ごとの説明、現在の状態、利用可否を表示する。Configureでデータ取得と各操作コールバックを受け取る。

### MarketPageUI.cs（171行）

マーケット在庫を行として表示し、商品名、価格、在庫、購入操作を配置する。マーケット情報テキストとカテゴリ表示を更新し、購入後はRefreshで一覧を再構築する。表示対象の選別と購入処理はEconomyControllerに委譲する。

### MonsterCodexOverlayView.cs（92行）

モンスター図鑑のモーダル、羊皮紙ウィンドウ、BookPageUI、閉じるボタンを構築する。Show時にはMonsterCodexPresenterが作成したEntry一覧をBookPageUIに設定して、オーバーレイを前面に表示する。Viewはレイアウトと可視状態のみを担い、敵アセットの走査・詳細文・遭遇判定はPresenterへ分離している。

### OnboardingGuideBannerView.cs（169行）

オンボーディングの現在ステップに応じた案内バナーを構築し、文言と表示位置を更新する。次に誘導する画面を開くボタンと、バナーを閉じる操作を扱う。表示対象のステップはOnboardingGuideControllerから受け取る。

### PartyPageUI.cs（131行）

現在のパーティメンバーを一覧化し、編成解除や傭兵詳細を開く操作を行に付ける。各行には能力・状態とパーティ内の情報を表示する。ListPageUIBaseのスクロールコンテンツ生成を利用する。

### RemoteSaleOverlayView.cs（208行）

リモート販売の在庫一覧、出品済み一覧、販売枠、価格を表示するオーバーレイを構築する。出品、取り消し、閉じるためのコールバックを受け、Refreshで販売状態を反映する。商品選別と販売実行はRemoteSaleController側で行う。

### RoadBattlePageUI.cs（3行）

街道戦闘ページ用の型で、RefreshOnlyPageUIBaseを継承する。固有の描画は持たず、外部から設定された更新処理を実行する。

### SimpleMercenaryHireUIView.cs（642行）

UI全体のページ・オーバーレイ参照を、ページスロットとオーバーレイスロットのenumで管理するMonoBehaviourである。戦闘、ダンジョン、装備、クエスト、図鑑など機能別の参照グループをネストしたReferences型として保持する。GetPage／GetOverlayと初期化処理により、composition rootが生成済みUIを一元的に取得できる。

### SimpleUIPage.cs（3行）

UIPageBaseの既定実装として使う空のページ型である。専用のPageUIコンポーネントがないページをUIPageRouterへ登録するために利用される。

### StoryOverlayView.cs（104行）

ストーリー表示用のオーバーレイを構築し、StoryPresentationの題名、本文、閉じるボタンを表示する。Showでプレゼンテーションを設定し、Hideで閉じる。閉じるボタンの文言はStoryPresentationの状態に合わせて設定する。

### TownMapPageUI.cs（3行）

街マップページ用の型で、RefreshOnlyPageUIBaseを継承する。固有の描画ロジックを持たず、登録された更新コールバックを使用する。

### TrainingGroundPageUI.cs（116行）

訓練対象の傭兵を一覧表示し、訓練内容、状態、開始ボタンを行に生成する。説明文とデータ供給・実行コールバックはConfigureTrainingGroundで設定する。訓練可否の判断と文言作成はTrainingGroundPagePresenterが担う。

### TutorialOverlayView.cs（177行）

チュートリアルのタイトル、本文、進行表示、戻る・次へボタンを持つオーバーレイを構築する。Controllerから呼ばれるテキスト設定、戻るボタンの操作可否、次へボタン文言の更新を提供する。ステップの進行状態は保持しない。

### WorldMapPageUI.cs（3行）

ワールドマップページ用の型で、RefreshOnlyPageUIBaseを継承する。画面内容の更新は外部から渡されるコールバックに委ねる。

## 基盤・ヘルパー（16ファイル）

この区分は各画面で共有するUI生成、ページ遷移、テーマ、表示文言、画像解決、ツールチップを提供する。個別機能の画面状態を持たないか、再利用可能な表示モデルを扱う。

### EnemySpriteResolver.cs（70行）

EnemyDataSOに対応する敵Spriteを解決する静的ヘルパーである。敵の種類や特殊個体の情報からResources上の候補を調べ、見つからない場合の代替Spriteも扱う。図鑑と戦闘表示で同じ画像解決規則を使えるようにする。

### EquipmentSpecialPageModel.cs（121行）

特殊装備図鑑に表示する装備データを集め、発見状態を含む表示行へ変換するモデルである。対象アセットを読み込み、品質や名前に基づく並び順と詳細テキストを作る。UI生成を持たず、EquipmentSpecialCodexPageUIへ表示データを渡す。

### GenericHoverTooltipTrigger.cs（98行）

任意のUI要素へ設定できるホバー用ツールチップトリガーである。ポインタの入退場時に指定テキストのツールチップを作成・表示・破棄する。EventTriggerを使わずIPointerEnterHandler／IPointerExitHandlerを実装する。

### IEquipmentDetailView.cs（32行）

装備詳細オーバーレイへタイトル、本文、ボタン状態、表示・非表示を指示するためのインターフェースである。CharacterEquipmentControllerはこの抽象に依存して詳細UIを更新する。実装側のSimpleMercenaryHireUIは呼び出しをOverlayPresenterへ委譲する。

### ItemPresentationService.cs（113行）

アイテム名、品質色、価格、所持数などの表示用情報を作るサービスである。ItemDataSOとEquipmentInstanceの差を吸収して、複数ページで使う表示文字列を提供する。アイテムの見た目用データをドメイン操作から分離する。

### ItemUsageTextBuilder.cs（381行）

消耗品の使用効果を種類ごとに読み取り、詳細表示用の日本語テキストへ組み立てる。回復、能力変化、状態変化などの効果を列挙し、数値の整形も行う。未対応または効果がない場合の表示文も扱う。

### JapaneseDisplayText.cs（458行）

敵、アイテム、職業、装備スロット、品質、状態などのゲーム内値を日本語表示名へ変換する静的ユーティリティである。等級に強さの補足を付けるメソッドや、各種enumの表示文を持つ。UI各所の表記をこのクラスに集約する。

### ListPageUIBase.cs（121行）

スクロールリストを持つページの基底クラスで、コンテンツ領域、ScrollRect、行の高さと配置を管理する。初期化、コンテンツのクリア、コンテンツ高さの設定、行生成の補助を提供する。UIPageBaseの表示・ボタン・テキスト生成ヘルパーを継承する。

### MercenaryPortraitProvider.cs（103行）

傭兵の肖像Spriteをデータから取得し、利用可能な画像がない場合の代替表示を解決するヘルパーである。傭兵データとインスタンスの両方を受け取る経路を提供する。雇用・商会・パーティなどで肖像の選択規則を共有する。

### QuestBoardLayout.cs（20行）

クエスト掲示板のレイアウトに使う定数をまとめた静的クラスである。行の高さ、余白、表示領域などを一箇所で定義する。MerchantQuestOverlayPresenterが同じ寸法を使って行を配置する。

### RefreshOnlyPageUIBase.cs（17行）

外部から渡されたUnityActionをRefreshで呼び出すためだけのUIPageBase派生クラスである。表示時のShowは基底実装に従い、固有のレイアウトや状態を持たない。戦闘・各種マップなど、Presenter側で描画するページに付与される。

### SimpleMercenaryHireUIFactory.cs（330行）

Text、行、ナビゲーションボタン、アクションボタン、パネル、Canvas、ページ、UIオブジェクトを生成するUIファクトリである。Resourcesのテンプレートが存在する場合はそれを生成し、共通の枠線・配色・テキスト・ボタン遷移を適用する。EventSystemの存在も保証する。

### UIHoverTooltipTrigger.cs（212行）

遅延表示、追従位置調整、画面端への収まりを扱う詳細なホバーツールチップMonoBehaviourである。ポインタ入退場と無効化・破棄時の処理を監視し、指定値または生成した内容でツールチップを表示する。GenericHoverTooltipTriggerより多い設定と内容生成を持つ。

### UIPageBase.cs（205行）

ページのShow、Hide、Refreshという共通ライフサイクルを定義する抽象MonoBehaviourである。Showではアクティブ化後にRefreshを呼び、子要素の削除、Text・Button・行の生成と設定を保護メソッドで提供する。行とアクションボタンはResourcesテンプレートを優先し、なければコードで生成する。

### UIPageRouter.cs（96行）

RectTransformごとにUIPageBaseを登録し、現在のページを一つだけ表示するルータである。Show時には他の登録ページを隠し、すでに表示中のページなら非アクティブ化せずRefreshのみを行う。専用ページコンポーネントがない場合はSimpleUIPageを追加して登録する。

### UITheme.cs（65行）

UIの色パレット、結果テキスト用の色コード、モーダル・ツールチップ用の色を静的に定義する。ApplyButtonTransitionsは全ボタンに共通のColorBlockとフェード時間を設定する。各ページやFactoryに散在する共通の配色・遷移設定をこのクラスから参照する。
