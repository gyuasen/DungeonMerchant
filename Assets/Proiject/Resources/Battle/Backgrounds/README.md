# 戦闘背景

背景画像をこのフォルダへ追加し、Texture Typeを`Sprite (2D and UI)`に設定します。画像は縦横比を維持したまま画面全体を覆うように中央トリミングされます。

- 共通背景: `Default.png`
- ダンジョン背景: DungeonDataSOのアセット名と同名のPNG、または`Battle Background Key`で指定した名前
- 街道背景: `Road_町番号_町番号.png`。番号は小さい方から並べます（例: `Road_2_3.png`）

現在使用する街道背景名は`Road_1_2`、`Road_0_1`、`Road_0_3`、`Road_3_4`、`Road_4_5`、`Road_5_6`です。

現在使用するダンジョン背景名は`DungeonData`、`LeafForestTrail`、`EldUndergroundWaterway`、`LowerMine`、`EldOldQuarry`、`MiddleRuins`、`NornCanopyLabyrinth`、`UpperFortress`、`VelmBlackIronMine`、`HighestAbyss`、`AstralDepths`です。

推奨サイズは1600x900または1920x1080です。敵が重なる中央部分は情報量を抑え、上下左右に重要な絵柄を置きすぎないようにしてください。`Battle Background`へSpriteを直接割り当てることもできます。
