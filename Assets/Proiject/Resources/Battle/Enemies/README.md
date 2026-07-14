# モンスター戦闘画像

透過PNGをこのフォルダへ追加し、Texture Typeを`Sprite (2D and UI)`に設定します。

使用方法は次のどちらかです。

1. `EnemyDataSO`の`Battle Sprite`へSpriteを直接割り当てる。
2. PNGのファイル名を`Battle Visual Key`へ入力する。未入力時はEnemyDataSOのアセット名と同名のSpriteを自動検索する。

通常敵は512x512、ボスは768x768または1024x1024を基準にし、足元と透明余白を統一してください。画像が未設定でも、戦闘画面では代替カードが表示されます。
