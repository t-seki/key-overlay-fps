開発ワークフロー向けの repo 固有の手順をまとめる。
`/supervise`（dotfiles の共通 skill）が読む repo 固有の手順。
なし。git 管理外で worktree にコピーが要る設定ファイルは無い（`git status --ignored` で確認）。
worker の worktree は `.claude/worktrees/` に切られる（`.gitignore` 済み）。
dotnet は WSL で動かないので、worker は `dotnet build` / `dotnet test` を実行しない。
なし。CI（`.github/workflows/build.yml`、windows-latest で Debug / Release のビルドとテスト）の結果を見る。
`src/` を触る PR では、承認の依頼に「Windows で見てほしい点」を添える。
自動。ただし次のパスを触る PR は承認制
- `src/`: アプリの挙動は Windows 実機で見るしかない
- `layouts/`: 同梱レイアウトの見た目が変わる
- `.github/workflows/`: CI とリリースの経路が変わる
なし。リリースは人が `v*` タグを打って `release.yml` で行う（RELEASE.md）。
