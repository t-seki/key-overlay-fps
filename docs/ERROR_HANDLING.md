# エラーハンドリングガイドライン

このドキュメントは、KeyOverlayFPSプロジェクトにおけるエラーハンドリングの統一的なアプローチを定義します。

## 基本原則

1. **ログ出力の統一**
   - すべてのエラーログは `Logger.Error()` を使用
   - 情報ログは `Logger.Info()` を使用
   - 警告ログは `Logger.Warning()` を使用
   - `System.Diagnostics.Debug.WriteLine` は使用しない（Logger.cs内部を除く）
   - `Console.WriteLine` は使用しない

2. **エラーメッセージの書き方**
   - 日本語で具体的に記述
   - 何が失敗したかを明確に示す
   - 可能な場合は対処方法を含める

## エラーハンドリングパターン

### パターン1: 重大なエラー（アプリケーション継続不可）

```csharp
try
{
    // 重要な初期化処理など
}
catch (Exception ex)
{
    Logger.Error("設定ファイルの読み込みに失敗しました", ex);
    throw; // 再スロー
}
```

使用場面：
- アプリケーション初期化
- 重要な設定ファイルの読み込み
- UI要素の初期化

### パターン2: 回復可能なエラー（デフォルト値で継続）

```csharp
try
{
    // オプショナルな処理
}
catch (Exception ex)
{
    Logger.Error("カスタム設定の読み込みに失敗、デフォルト値を使用します", ex);
    return defaultValue; // デフォルト値を返す
}
```

使用場面：
- オプショナルな設定の読み込み
- プラグインの初期化
- 非必須機能の実行

### パターン3: 無視可能なエラー（処理継続）

```csharp
try
{
    // 副次的な処理
}
catch (Exception ex)
{
    Logger.Warning("統計情報の保存に失敗しましたが、処理を継続します", ex);
    // 処理を継続
}
```

使用場面：
- ログファイルへの書き込み
- 統計情報の更新
- キャッシュの更新

## 特定の例外タイプの処理

### FileNotFoundException

```csharp
catch (FileNotFoundException ex)
{
    Logger.Error($"ファイルが見つかりません: {ex.FileName}", ex);
    // 適切な処理
}
```

### ArgumentException

```csharp
catch (ArgumentException ex)
{
    Logger.Error($"無効な引数が指定されました: {ex.ParamName}", ex);
    // 適切な処理
}
```

## Loggerクラスの使用方法

### エラーレベル

- **Error**: アプリケーションの動作に影響する重大なエラー
- **Warning**: 処理は継続可能だが注意が必要な状況
- **Info**: 通常の動作情報（初期化完了、設定保存など）

### ログ出力例

```csharp
// エラー with 例外
Logger.Error("データベース接続に失敗しました", ex);

// エラー without 例外
Logger.Error("無効な設定値が検出されました");

// 警告 with 例外
Logger.Warning("設定保存でエラーが発生、処理は継続", ex);

// 警告 without 例外
Logger.Warning("古い形式の設定ファイルを検出、自動変換します");

// 情報
Logger.Info("アプリケーションを起動しました");
```

## 禁止事項

1. **catch節を空にしない**
   ```csharp
   // 悪い例
   catch (Exception ex)
   {
       // 何もしない
   }
   ```

2. **汎用的なメッセージを使用しない**
   ```csharp
   // 悪い例
   Logger.Error("エラーが発生しました", ex);
   
   // 良い例
   Logger.Error("キーボードレイアウトファイルの解析に失敗しました", ex);
   ```

3. **例外を握りつぶして別の例外を投げない**
   ```csharp
   // 悪い例
   catch (IOException ex)
   {
       throw new Exception("エラー");
   }
   
   // 良い例
   catch (IOException ex)
   {
       throw new LayoutLoadException("レイアウトファイルの読み込みに失敗", ex);
   }
   ```

## テストでのエラーハンドリング

テストコードでは、期待される例外を明示的に検証します：

```csharp
[Test]
public void InvalidInput_ThrowsArgumentException()
{
    Assert.Throws<ArgumentException>(() => 
    {
        // テスト対象のコード
    });
}
```