# KeyOverlayFPS

FPSゲーム配信用のキーボード・マウス入力可視化オーバーレイアプリケーション

## 🎯 概要

KeyOverlayFPSは、FPSゲームプレイ中のキーボードとマウス入力をリアルタイムで可視化するWindows用アプリケーションです。透明なオーバーレイウィンドウとして動作し、配信や録画時に視聴者がプレイヤーの操作を直感的に理解できます。

## ✨ 主な機能

### 🎮 キーボード入力可視化
- **リアルタイム検出**: 全キーの押下状態をリアルタイムで表示
- **Shift表示**: Shift押下時の記号表示に自動切り替え（65%レイアウト）
- **65%キーボード**: フルキーボードレイアウトをサポート
- **FPSモード**: FPSに特化したコンパクトレイアウト（WASD中心）

### 🖱️ マウス入力可視化
- **クリック表示**: 左クリック、右クリック、中ボタン、サイドボタン対応
- **スクロール表示**: 上下スクロールの可視化
- **移動方向表示**: 16方向に分割されたマウス移動可視化
- **プレミアムデザイン**: グラデーションとシャドウ効果

### 🎨 カスタマイズ機能
- **カラーテーマ**: 背景色、文字色、ハイライト色の変更
- **表示オプション**: 最前面固定、マウス表示/非表示
- **スケール調整**: 80%、100%、120%、150%のサイズ変更
- **プロファイル切替**: 65%キーボード ⇔ FPSキーボード

### 🔧 レイアウト管理
- **カスタムレイアウト**: YAML形式でのレイアウトカスタマイズ
- **動的読み込み**: アプリケーション実行中のレイアウト切替
- **プリセット**: 65%キーボードとFPSキーボードのプリセット提供
- **設定オーバーライド**: settings.yamlの設定がレイアウトYAMLの値を自動的にオーバーライド

## 🛠️ 技術仕様

### システム要件
- **OS**: Windows 10/11 (64-bit)
- **フレームワーク**: .NET 8.0
- **追加要件**: なし（スタンドアロン実行可能）

### 技術スタック
- **言語**: C# 12.0
- **UI フレームワーク**: WPF (Windows Presentation Foundation)
- **入力検出**: Win32 API (グローバルフック: `SetWindowsHookEx`)
- **マウス追跡**: Win32 API (フックベース検出)
- **設定管理**: YamlDotNet
- **テスト**: NUnit 3.14

### アーキテクチャ
```
src/
├── MainWindow.xaml.cs          # メインウィンドウ
├── Input/                      # 入力システム
│   ├── BaseHook.cs            # フック基底クラス（IDisposable対応）
│   ├── KeyboardHook.cs         # Win32 APIキーボードフック
│   ├── MouseHook.cs            # Win32 APIマウスフック
│   └── InputStateManager.cs    # 統合入力状態管理
├── Layout/                     # レイアウト管理システム
│   ├── LayoutManager.cs        # レイアウトの保存・読み込み
│   ├── LayoutConfig.cs         # レイアウト設定データ構造
│   ├── UIGenerator.cs          # 動的UI生成
│   ├── KeyboardElementGenerator.cs # キーボード要素生成
│   ├── MouseElementGenerator.cs    # マウス要素生成
│   └── UIElementLocator.cs     # UI要素検索・キャッシュ管理
├── MouseVisualization/         # マウス入力可視化
│   ├── DirectionCalculator.cs  # 16方向計算ロジック
│   ├── DirectionArcGenerator.cs # 円弧描画生成
│   ├── MouseTracker.cs         # マウス移動追跡
│   └── MouseDirectionVisualizer.cs # マウス方向可視化
├── UI/                         # UI管理
│   ├── MainWindowSettings.cs   # 設定関連機能
│   ├── MainWindowMenu.cs       # メニュー関連機能
│   ├── MainWindowInput.cs      # 入力処理
│   ├── CanvasRebuilder.cs      # Canvas再構築処理
│   ├── MouseElementManager.cs  # マウス要素管理
│   ├── ProfileManager.cs       # プロファイル管理
│   ├── ProfileSwitcher.cs      # プロファイル切替
│   ├── BrushFactory.cs         # ブラシ生成ファクトリー
│   └── UIElementFactory.cs     # UI要素生成ファクトリー
├── Settings/                   # 設定管理
│   ├── SettingsManager.cs      # 設定の保存・読み込み
│   └── AppSettings.cs          # アプリケーション設定モデル
├── Utils/                      # ユーティリティ
│   ├── YamlSerializerFactory.cs # YAML設定統一化
│   ├── CanvasElementHelper.cs  # Canvas要素操作ヘルパー
│   ├── DisposableBase.cs       # IDisposable基底クラス
│   └── Logger.cs               # ロギングシステム
├── Initialization/             # 初期化システム
│   └── WindowInitializer.cs    # ウィンドウ初期化
├── Constants/                  # 定数定義
│   └── ApplicationConstants.cs # アプリケーション定数
└── tests/                      # 単体テスト
```

## 🚀 セットアップ

### ビルド方法
```bash
# リポジトリのクローン
git clone <repository-url>
cd key-overlay-fps

# プロジェクトのビルド
dotnet build src/KeyOverlayFPS.csproj --configuration Release

# 実行
dotnet run --project src/KeyOverlayFPS.csproj
```

### テスト実行
```bash
# すべてのテストを実行
dotnet test

# 特定のテストクラスを実行
dotnet test --filter "TestClass=LayoutManagerTests"
```

## 💻 使用方法

### 基本操作
1. **アプリケーション起動**: `KeyOverlayFPS.exe` を実行
2. **位置調整**: ウィンドウをドラッグして配置を調整
3. **設定変更**: 右クリックでコンテキストメニューを表示

### プロファイル切替
- **65%キーボード**: 全キー表示（配信・録画用）
- **FPSキーボード**: WASD周辺のみ表示（ゲーム中用）

### レイアウトカスタマイズ
1. レイアウトファイル（YAML形式）を編集
2. アプリケーションのレイアウトディレクトリに配置
3. 右クリックメニューからレイアウトを選択

## 🎯 配信での活用

### OBSでの設定
1. **ソース追加**: 「ウィンドウキャプチャ」でKeyOverlayFPSを追加
2. **背景透過**: 「クロマキー緑」または「クロマキー青」に設定
3. **レイヤー調整**: ゲーム画面の上に配置

### 推奨設定
- **背景色**: クロマキー緑 (OBS用)
- **文字色**: 白または黄色 (視認性重視)
- **ハイライト色**: 緑または赤 (アクション強調)
- **スケール**: 120% (配信画質考慮)

## 🔗 設定ファイル

設定は自動的に `%APPDATA%/KeyOverlayFPS/settings.yaml` に保存されます。

```yaml
currentProfile: "FullKeyboard65"
displayScale: 1.2
isMouseVisible: true
isTopmost: true
backgroundColor: "Transparent"
foregroundColor: "White"
highlightColor: "Green"
windowLeft: 100
windowTop: 100
```

### 設定オーバーライド機能
`settings.yaml` の設定値は、レイアウトYAMLファイルの対応する設定値を自動的にオーバーライドします：
- `currentProfile` 以外のすべての設定項目がオーバーライド可能
- `isMouseVisible` が `false` の場合、レイアウトにマウス要素が定義されていても非表示
- 色設定、スケール、ウィンドウ設定など、すべてユーザー設定が優先

## 🤝 コントリビューション

### 開発環境のセットアップ
1. Visual Studio 2022 または VS Code + C# 拡張機能
2. .NET 8.0 SDK のインストール
3. Git での開発フロー

### テストガイドライン
- 新機能には対応する単体テストを追加
- `dotnet test` でテストが通ることを確認
- 複雑なロジックには統合テストを検討

### コードスタイル
- C# の標準命名規則に従う
- XMLドキュメントコメントを記述
- SOLID原則を意識した設計
- 共通処理はヘルパークラスに集約（例: CanvasElementHelper、YamlSerializerFactory）
- IDisposableパターンはDisposableBase基底クラスを使用
- ロギングはLoggerクラスを使用（Debug.WriteLineの代替）

## 📜 ライセンス

このプロジェクトはMITライセンスの下で公開されています。詳細は[LICENSE](LICENSE)ファイルを参照してください。

## ⚡ パフォーマンス

- **更新間隔**: 60FPS (16.67ms)
- **CPU使用率**: 通常時 < 5%
- **メモリ使用量**: 約 50MB
- **入力遅延**: < 1フレーム
- **スレッドセーフ**: ConcurrentDictionaryによる安全な状態管理
- **最適化**: 
  - マウス可視性制御でCanvas再構築を回避
  - UI要素検索のキャッシュ化（UIElementLocator）
  - 重複コード削減による実行効率向上

## 🔍 トラブルシューティング

### よくある問題

**Q: キー入力が検出されない**
- 管理者権限で実行を試してください
- 他のキーフック系ソフトウェアとの競合を確認

**Q: ウィンドウが最前面に表示されない**
- 「表示オプション」→「最前面固定」が有効か確認
- Windows のゲームモードが影響する場合があります

**Q: 設定が保存されない**
- `%APPDATA%/KeyOverlayFPS/` フォルダの書き込み権限を確認
- ウイルス対策ソフトの除外設定を検討

## 📞 サポート

- **Issue報告**: [GitHub Issues](https://github.com/user/key-overlay-fps/issues)
- **機能要望**: GitHub Issues に Enhancement ラベルで投稿
- **質問・議論**: [GitHub Discussions](https://github.com/user/key-overlay-fps/discussions)

---

⭐ プロジェクトが役に立った場合は、GitHubでスターをお願いします！