# 図鑑ブック UI

`BookPageUI` は羊皮紙の左右ページを持つ見開きです。
各ページは2件なので、1見開きは4件です。
下部の前後ボタンで見開きを切り替え、表示は `現在 / 総数` です。

装備画像の Resources キーは `UI/Codex/Equipment/{ItemDataSO のアセット名}` です。
画像がない場合は枠と `?` が表示されます。
魔物は `EnemyDataSO.battleSprite` を優先し、未設定時は `battleVisualKey` と `Battle/Enemies/{battleVisualKey}` を順に読みます。
