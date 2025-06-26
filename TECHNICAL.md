# KeyOverlayFPS - 技術仕様書

## アーキテクチャ概要

### 基本構成
```
┌─────────────────┐
│   MainWindow    │ ← UI表示・イベント処理
├─────────────────┤
│ DispatcherTimer │ ← 60FPS更新制御
├─────────────────┤
│ GetAsyncKeyState│ ← Windows API キー検出
├─────────────────┤
│   Canvas Layout │ ← 87キー絶対位置配置
└─────────────────┘
```

## キー検出システム

### Windows API 使用
```csharp
[DllImport("user32.dll")]
private static extern short GetAsyncKeyState(int vKey);
```

### 仮想キーコード一覧
```csharp
// 制御キー
private const int VK_ESCAPE = 0x1B;
private const int VK_TAB = 0x09;
private const int VK_CAPS_LOCK = 0x14;
private const int VK_SHIFT = 0x10;
private const int VK_CONTROL = 0x11;
private const int VK_ALT = 0x12;
private const int VK_SPACE = 0x20;
private const int VK_ENTER = 0x0D;
private const int VK_BACKSPACE = 0x08;

// 数字キー (0x30-0x39)
private const int VK_0 = 0x30;
private const int VK_1 = 0x31;
// ... VK_9 = 0x39

// アルファベット (0x41-0x5A)
private const int VK_A = 0x41;
private const int VK_B = 0x42;
// ... VK_Z = 0x5A

// 記号キー
private const int VK_MINUS = 0xBD;        // -
private const int VK_EQUALS = 0xBB;       // =
private const int VK_OPEN_BRACKET = 0xDB; // [
private const int VK_CLOSE_BRACKET = 0xDD;// ]
private const int VK_BACKSLASH = 0xDC;    // \
private const int VK_SEMICOLON = 0xBA;    // ;
private const int VK_QUOTE = 0xDE;        // '
private const int VK_COMMA = 0xBC;        // ,
private const int VK_PERIOD = 0xBE;       // .
private const int VK_SLASH = 0xBF;        // /

// 矢印キー
private const int VK_LEFT = 0x25;
private const int VK_UP = 0x26;
private const int VK_RIGHT = 0x27;
private const int VK_DOWN = 0x28;

// システムキー
private const int VK_WIN = 0x5B;
```

### キー状態検出ロジック
```csharp
// キー押下状態チェック
bool isPressed = (GetAsyncKeyState(virtualKeyCode) & 0x8000) != 0;

// Shift修飾キー検出
bool isShiftPressed = (GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0;
```

## UI レイアウトシステム

### Canvas 座標系
- **基準点**: 左上 (0,0)
- **単位**: WPF Device Independent Units
- **レイアウト**: 絶対位置指定

### キー配置座標

#### 第1行 (数字行)
```
ESC: (5, 5)     [26x26]
1:   (33, 5)    [26x26]
2:   (61, 5)    [26x26]
...
Backspace: (369, 5) [40x26]
```

#### 第2行 (QWERTY行)
```
Tab: (5, 33)    [36x26]
Q:   (43, 33)   [26x26]
W:   (71, 33)   [26x26]
...
\:   (379, 33)  [26x26]
```

#### 第3行 (ASDF行)
```
Caps: (5, 61)   [42x26]
A:    (49, 61)  [26x26]
...
Enter: (357, 61) [48x26]
```

#### 第4行 (ZXCV行)
```
Shift: (5, 89)  [58x26]
Z:     (65, 89) [26x26]
...
↑:     (379, 89) [26x26]
```

#### 第5行 (最下段)
```
Ctrl: (5, 117)  [32x24]
...
→:    (399, 117) [26x24]
```

## 更新ループシステム

### タイマー設定
```csharp
_timer = new DispatcherTimer
{
    Interval = TimeSpan.FromMilliseconds(16.67) // 60FPS
};
```

### 更新処理フロー
```csharp
private void Timer_Tick(object? sender, EventArgs e)
{
    // 1. Shift状態検出
    bool isShiftPressed = (GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0;
    
    // 2. 各キーの状態更新
    UpdateKeyStateWithShift("Key1", VK_1, "1", "!", isShiftPressed);
    // ...
    
    // 3. UI反映は自動 (WPFバインディング)
}
```

## Shift修飾キーシステム

### UpdateKeyStateWithShift メソッド
```csharp
private void UpdateKeyStateWithShift(string keyName, int virtualKeyCode, 
                                   string normalText, string shiftText, 
                                   bool isShiftPressed)
{
    var keyBorder = FindName(keyName) as Border;
    var textBlock = FindName(keyName + "Text") as TextBlock;
    
    if (keyBorder != null && textBlock != null)
    {
        // キー押下状態でハイライト
        bool isPressed = (GetAsyncKeyState(virtualKeyCode) & 0x8000) != 0;
        keyBorder.Background = isPressed ? _activeBrush : _inactiveBrush;
        
        // Shift状態でテキスト切り替え
        textBlock.Text = isShiftPressed ? shiftText : normalText;
    }
}
```

### Shift対応キーマッピング
```csharp
// 数字キー → 記号
"1" → "!"    "2" → "@"    "3" → "#"    "4" → "$"    "5" → "%"
"6" → "^"    "7" → "&"    "8" → "*"    "9" → "("    "0" → ")"

// 記号キー → 記号
"-" → "_"    "=" → "+"    "[" → "{"    "]" → "}"    "\" → "|"
";" → ":"    "'" → "\""   "," → "<"    "." → ">"    "/" → "?"
```

## ウィンドウ管理システム

### WPF ウィンドウ設定
```xml
<Window WindowStyle="None"           <!-- ボーダーレス -->
        AllowsTransparency="True"    <!-- 透明対応 -->
        Background="Transparent"     <!-- デフォルト透明 -->
        Topmost="True"              <!-- 最前面固定 -->
        ShowInTaskbar="True"        <!-- タスクバー表示 -->
        ResizeMode="NoResize">      <!-- サイズ固定 -->
```

### 背景色管理
```csharp
private void SetBackgroundColor(Color color, bool transparent)
{
    _transparentMode = transparent;
    if (transparent)
    {
        // ほぼ透明 (マウスイベント有効)
        Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
    }
    else
    {
        Background = new SolidColorBrush(color);
    }
}
```

### ドラッグ移動実装
```csharp
private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
{
    _isDragging = true;
    _dragStartPoint = e.GetPosition(this);
    CaptureMouse();
}

private void MainWindow_MouseMove(object sender, MouseEventArgs e)
{
    if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
    {
        var currentPosition = e.GetPosition(this);
        var screen = PointToScreen(currentPosition);
        var window = PointToScreen(_dragStartPoint);
        
        Left = screen.X - window.X + Left;
        Top = screen.Y - window.Y + Top;
    }
}
```

## パフォーマンス特性

### リソース使用量
- **CPU**: 約2-5% (アイドル時)
- **メモリ**: 約20-30MB
- **更新頻度**: 60FPS固定

### 最適化要素
- **バインディング回避**: 直接プロパティ更新
- **最小描画**: 変更時のみ背景更新
- **効率的検索**: FindName()キャッシュ可能

## セキュリティ考慮事項

### API使用権限
- **GetAsyncKeyState**: 通常ユーザー権限で動作
- **グローバルキー検出**: 管理者権限推奨（一部環境）

### 潜在的制限
- **UAC**: Windows UAC環境での制限あり
- **セキュリティソフト**: キーロガー検出の可能性
- **サンドボックス**: 仮想環境での動作制限

## トラブルシューティング

### 一般的な問題

1. **キー検出失敗**
   - 管理者権限で実行
   - セキュリティソフトの設定確認

2. **透明背景でマウス操作不可**
   - Background = Color.FromArgb(1,0,0,0) 設定確認

3. **クロマキー背景でクラッシュ**
   - AllowsTransparency静的設定確認

### デバッグ手法
```csharp
// キー状態デバッグ出力
System.Diagnostics.Debug.WriteLine($"Key {keyName}: {isPressed}");

// パフォーマンス測定
var stopwatch = System.Diagnostics.Stopwatch.StartNew();
// ... 処理 ...
Debug.WriteLine($"Update time: {stopwatch.ElapsedMilliseconds}ms");
```

## 拡張可能性

### 新しいキーレイアウト追加
1. `MainWindow.xaml`にBorderコントロール追加
2. 仮想キーコード定数追加
3. `Timer_Tick()`に更新処理追加

### カスタムスタイル対応
1. WPF StyleとTemplateを使用
2. 設定ファイル読み込み機能
3. リアルタイム設定変更

### 多言語キーボード対応
1. 言語固有の仮想キーコード
2. レイアウト定義ファイル分離
3. 動的レイアウト切り替え