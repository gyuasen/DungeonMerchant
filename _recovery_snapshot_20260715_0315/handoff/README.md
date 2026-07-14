# Handoff Folder

このフォルダは、家・学校・教室の3つの開発環境間で開発状況を受け渡すための場所です。

## 共有状況ファイル

家側・学校側・教室側のすべてが編集する全体状況は `SHARED_PROJECT_STATUS.md` に記載してください。

## 使い方

### 学校から家へ

学校側で作ったまとめは、プロジェクト直下の `HANDOFF_HOME.md` を家のチャットに貼ってください。

学校側でCodexが作業した内容は、作業ごとに必ず `SCHOOL_WORK_LOG.md` に追記してください。

学校側と家側の内容に食い違いがある場合は、家側の内容を優先してください。

### 教室での作業

教室側は学校側と同じ開発ルールに従ってください。

教室側でCodexが作業した内容は、作業ごとに必ず `CLASSROOM_WORK_LOG.md` に追記してください。

教室側から家側・学校側へ知らせる全体状況は、`SHARED_PROJECT_STATUS.md` に短く追記してください。

教室側と家側の内容に食い違いがある場合は、家側の内容を優先してください。

### 家から学校へ

家のチャットで作業した内容や次に学校側で読ませたい内容は、`FROM_HOME_CHAT.md` に貼ってください。

次に学校でCodexへ依頼するときは、こう言えばOKです。

```text
handoff/FROM_HOME_CHAT.md を読んで、家での作業内容を反映してください
```

### 家でのClaude Code作業

家側でClaude Codeが作業する場合は、`FROM_HOME_CHAT.md`への要約に加えて、詳細な進捗・計画・技術的な発見事項を `CLAUDE_WORK_LOG.md` に記録してください。

次にClaude Codeで作業を再開するときは、こう言えばOKです。

```text
handoff/CLAUDE_WORK_LOG.md と handoff/SHARED_PROJECT_STATUS.md を読んで、続きに着手してください
```

## 追記ルール

- 学校側でCodexが作業した内容は、作業ごとに `SCHOOL_WORK_LOG.md` へ追記してください。
- 教室側でCodexが作業した内容は、作業ごとに `CLASSROOM_WORK_LOG.md` へ追記してください。
- 家側でCodexが作業した内容や学校側へ共有したい内容は、作業ごとに `FROM_HOME_CHAT.md` へ追記してください。
- 家側でClaude Codeが作業した内容は、`FROM_HOME_CHAT.md`への要約に加えて `CLAUDE_WORK_LOG.md` へ詳細を追記してください。
- 家側・学校側・教室側の内容に食い違いがある場合は、家側の内容を優先してください。
- 全体状況や次にやることは `SHARED_PROJECT_STATUS.md` に短くまとめてください。
