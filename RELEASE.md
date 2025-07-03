# KeyOverlayFPS リリースガイド

このドキュメントでは、KeyOverlayFPSのリリース手順について説明します。

## リリース手順

### 1. 事前準備

- [ ] すべてのテストが通ることを確認
- [ ] ドキュメントの更新
- [ ] バージョン番号の決定（セマンティックバージョニング）

### 2. ローカルテスト

```powershell
# Windows PowerShellで実行
.\scripts\build-release.ps1 -Version "1.0.0"
```

```bash
# Linux/macOSで実行
./scripts/build-release.sh "1.0.0"
```

### 3. タグの作成とプッシュ

```bash
# タグを作成
git tag v1.0.0

# タグをプッシュ（これによりGitHub Actionsが自動実行される）
git push origin v1.0.0
```

### 4. GitHub Releasesの確認

1. GitHub Actionsが正常に完了することを確認
2. Releaseページで生成されたアセットを確認
3. 必要に応じてリリースノートを編集

## 配布アセット

各リリースでは以下のアセットが自動生成されます：

### 単一実行ファイル版（推奨）
- `KeyOverlayFPS-win-x64.exe` - 64bit版単一実行ファイル
- `KeyOverlayFPS-win-x86.exe` - 32bit版単一実行ファイル

**特徴:**
- .NET Runtimeのインストール不要
- ダウンロード後すぐに実行可能
- ファイルサイズ: 約80-100MB

### Zip版（カスタマイズ可能）
- `KeyOverlayFPS-win-x64.zip` - 64bit版Zipファイル
- `KeyOverlayFPS-win-x86.zip` - 32bit版Zipファイル

**特徴:**
- レイアウトファイル（layouts/*.yaml）が同梱
- 設定ファイルのカスタマイズが容易
- ファイルサイズ: 約60-80MB（圧縮済み）

## バージョン管理

### セマンティックバージョニング

- **MAJOR** (1.x.x): 互換性のない変更
- **MINOR** (x.1.x): 新機能の追加（後方互換性あり）
- **PATCH** (x.x.1): バグフィックス（後方互換性あり）

### 例
- `v1.0.0`: 初回リリース
- `v1.0.1`: バグフィックス
- `v1.1.0`: 新機能追加
- `v2.0.0`: 破壊的変更

## トラブルシューティング

### ビルドエラー

**"Could not execute because the specified command or file was not found"**
- .NET 8 SDKがインストールされていることを確認

**"The current .NET SDK does not support targeting .NET 8.0"**
- .NET 8 SDKをインストール：https://dotnet.microsoft.com/download

### GitHub Actions エラー

**"Resource not accessible by integration"**
- リポジトリの設定でActionsの権限を確認
- Settings > Actions > General > Workflow permissions

**"Tag already exists"**
- 既存のタグを削除してから再実行
```bash
git tag -d v1.0.0
git push origin :refs/tags/v1.0.0
```

## 自動化された機能

### ビルド検証
- プルリクエスト時の自動ビルド・テスト
- 複数設定（Debug/Release）でのビルド検証

### リリース自動化
- タグプッシュでの自動リリース作成
- 複数アーキテクチャのビルド
- アセットの自動アップロード
- リリースノートの自動生成

### 品質保証
- 自動テスト実行
- コードカバレッジレポート（今後追加予定）
- 静的解析（今後追加予定）

## 今後の改善予定

- [ ] コードサイニング証明書の適用
- [ ] Chocolatey/wingetでの配布
- [ ] 自動更新機能の実装
- [ ] インストーラー版（MSI/MSIX）の提供