using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using YamlDotNet.Serialization;

namespace KeyOverlayFPS
{
    // 定数定義
    public static class Constants
    {
        public const double TimerInterval = 16.67; // ms
        public const int ScrollDisplayFrames = 10;
        
        public static class WindowSizes
        {
            public const double FullKeyboardWidth = 580;
            public const double FullKeyboardHeight = 160;
            public const double FpsKeyboardWidth = 450;
            public const double FpsKeyboardWidthWithMouse = 520;
            public const double FpsKeyboardHeight = 160;
        }
        
        public static class ScaleOptions
        {
            public const double Scale80 = 0.8;
            public const double Scale100 = 1.0;
            public const double Scale120 = 1.2;
            public const double Scale150 = 1.5;
        }
    }

    // 設定データクラス
    public class AppSettings
    {
        public string CurrentProfile { get; set; } = "FullKeyboard65";
        public double DisplayScale { get; set; } = 1.0;
        public bool IsMouseVisible { get; set; } = true;
        public bool IsTopmost { get; set; } = true;
        public string BackgroundColor { get; set; } = "Transparent";
        public string ForegroundColor { get; set; } = "White";
        public string HighlightColor { get; set; } = "Green";
        public double WindowLeft { get; set; } = 100;
        public double WindowTop { get; set; } = 100;
    }

    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
        
        // キー状態検出用定数
        private const short KEY_PRESSED_MASK = unchecked((short)0x8000);
        
        /// <summary>
        /// 指定された仮想キーが現在押されているかを判定
        /// </summary>
        /// <param name="virtualKeyCode">仮想キーコード</param>
        /// <returns>キーが押されている場合はtrue</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsKeyPressed(int virtualKeyCode)
        {
            return (GetAsyncKeyState(virtualKeyCode) & KEY_PRESSED_MASK) != 0;
        }

        // ESCキー
        private const int VK_ESCAPE = 0x1B;
        
        // 数字キー
        private const int VK_1 = 0x31;
        private const int VK_2 = 0x32;
        private const int VK_3 = 0x33;
        private const int VK_4 = 0x34;
        private const int VK_5 = 0x35;
        private const int VK_6 = 0x36;
        private const int VK_7 = 0x37;
        private const int VK_8 = 0x38;
        private const int VK_9 = 0x39;
        private const int VK_0 = 0x30;
        private const int VK_MINUS = 0xBD;
        private const int VK_EQUALS = 0xBB;
        private const int VK_BACKSPACE = 0x08;
        
        // アルファベットキー
        private const int VK_TAB = 0x09;
        private const int VK_Q = 0x51;
        private const int VK_W = 0x57;
        private const int VK_E = 0x45;
        private const int VK_R = 0x52;
        private const int VK_T = 0x54;
        private const int VK_Y = 0x59;
        private const int VK_U = 0x55;
        private const int VK_I = 0x49;
        private const int VK_O = 0x4F;
        private const int VK_P = 0x50;
        private const int VK_OPEN_BRACKET = 0xDB;
        private const int VK_CLOSE_BRACKET = 0xDD;
        private const int VK_BACKSLASH = 0xDC;
        
        private const int VK_CAPS_LOCK = 0x14;
        private const int VK_A = 0x41;
        private const int VK_S = 0x53;
        private const int VK_D = 0x44;
        private const int VK_F = 0x46;
        private const int VK_G = 0x47;
        private const int VK_H = 0x48;
        private const int VK_J = 0x4A;
        private const int VK_K = 0x4B;
        private const int VK_L = 0x4C;
        private const int VK_SEMICOLON = 0xBA;
        private const int VK_QUOTE = 0xDE;
        private const int VK_ENTER = 0x0D;
        
        private const int VK_SHIFT = 0x10;
        private const int VK_Z = 0x5A;
        private const int VK_X = 0x58;
        private const int VK_C = 0x43;
        private const int VK_V = 0x56;
        private const int VK_B = 0x42;
        private const int VK_N = 0x4E;
        private const int VK_M = 0x4D;
        private const int VK_COMMA = 0xBC;
        private const int VK_PERIOD = 0xBE;
        private const int VK_SLASH = 0xBF;
        private const int VK_UP = 0x26;
        
        private const int VK_CONTROL = 0x11;
        private const int VK_WIN = 0x5B;
        private const int VK_ALT = 0x12;
        private const int VK_SPACE = 0x20;
        
        // 左右個別修飾キー
        private const int VK_LSHIFT = 0xA0;
        private const int VK_RSHIFT = 0xA1;
        private const int VK_LCONTROL = 0xA2;
        private const int VK_RCONTROL = 0xA3;
        private const int VK_LMENU = 0xA4;    // Left Alt
        private const int VK_RMENU = 0xA5;    // Right Alt
        private const int VK_LEFT = 0x25;
        private const int VK_DOWN = 0x28;
        private const int VK_RIGHT = 0x27;
        
        // ナビゲーションキー
        private const int VK_DELETE = 0x2E;
        private const int VK_HOME = 0x24;
        private const int VK_PAGE_UP = 0x21;
        private const int VK_PAGE_DOWN = 0x22;
        
        // マウスボタン
        private const int VK_LBUTTON = 0x01;
        private const int VK_RBUTTON = 0x02;
        private const int VK_MBUTTON = 0x04; // ホイールクリック
        private const int VK_XBUTTON1 = 0x05; // マウスボタン4
        private const int VK_XBUTTON2 = 0x06; // マウスボタン5

        private readonly DispatcherTimer _timer;
        private Brush _activeBrush = new SolidColorBrush(Color.FromArgb(180, 0, 255, 0));
        private readonly Brush _inactiveBrush = Brushes.Transparent;
        // スクロール表示色（ハイライト色と同じ）
        private bool _isDragging = false;
        private Point _dragStartPoint;
        private bool _transparentMode = true;
        private int _scrollUpTimer = 0;
        private int _scrollDownTimer = 0;
        
        // フォアグラウンド色管理
        private Brush _foregroundBrush = Brushes.White;
        
        // マウス表示切り替え
        private bool _isMouseVisible = true;
        
        // 表示スケール（0.8, 1.0, 1.2, 1.5）
        private double _displayScale = 1.0;
        
        // プロファイル管理
        public enum KeyboardProfile
        {
            FullKeyboard65,  // 現在の65%キーボード
            FPSKeyboard      // FPS用コンパクトキーボード
        }
        
        private KeyboardProfile _currentProfile = KeyboardProfile.FullKeyboard65;
        
        // プロファイル別マウス表示位置
        private readonly Dictionary<KeyboardProfile, (double Left, double Top)> _mousePositions = new()
        {
            { KeyboardProfile.FullKeyboard65, (475, 20) },  // 元の位置
            { KeyboardProfile.FPSKeyboard, (290, 20) }      // FPS用位置
        };
        
        // プロファイル別Shift表示変更設定
        private readonly Dictionary<KeyboardProfile, bool> _shiftDisplayEnabled = new()
        {
            { KeyboardProfile.FullKeyboard65, true },   // 65%キーボードはShift表示変更有効
            { KeyboardProfile.FPSKeyboard, false }      // FPSキーボードはShift表示変更無効
        };
        
        // 設定ファイル管理
        private readonly string _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "KeyOverlayFPS", 
            "settings.yaml"
        );
        private AppSettings _settings = new();

        public MainWindow()
        {
            InitializeComponent();
            
            // 設定を読み込み
            LoadSettings();
            
            // コンテキストメニューを設定（設定読み込み後）
            SetupContextMenu();
            
            // 設定を適用（メニュー設定後）
            ApplySettings();
            
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(Constants.TimerInterval)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            MouseMove += MainWindow_MouseMove;
            MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
            this.MouseWheel += MainWindow_MouseWheel;
            
            // アプリケーション終了時に設定を保存
            Application.Current.Exit += (s, e) => SaveSettings();
        }

        private void SetupContextMenu()
        {
            var contextMenu = new ContextMenu();
            
            contextMenu.Items.Add(CreateBackgroundColorMenu());
            contextMenu.Items.Add(CreateForegroundColorMenu());
            contextMenu.Items.Add(CreateHighlightColorMenu());
            contextMenu.Items.Add(CreateViewOptionsMenu());
            contextMenu.Items.Add(CreateProfileMenu());
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(CreateExitMenu());
            
            ContextMenu = contextMenu;
        }
        
        private MenuItem CreateBackgroundColorMenu()
        {
            var backgroundMenuItem = new MenuItem { Header = "背景色" };
            
            var backgroundOptions = new[]
            {
                ("透明", Colors.Transparent, true),
                ("クロマキー緑", Colors.Lime, false),
                ("クロマキー青", Colors.Blue, false),
                ("黒", Colors.Black, false)
            };
            
            foreach (var (name, color, transparent) in backgroundOptions)
            {
                var menuItem = new MenuItem { Header = name };
                menuItem.Click += (s, e) => SetBackgroundColor(color, transparent);
                backgroundMenuItem.Items.Add(menuItem);
            }
            
            return backgroundMenuItem;
        }
        
        private MenuItem CreateForegroundColorMenu()
        {
            var foregroundMenuItem = new MenuItem { Header = "文字色" };
            
            var foregroundColors = new[]
            {
                ("白", Colors.White),
                ("黒", Colors.Black),
                ("グレー", Colors.Gray),
                ("青", Colors.CornflowerBlue),
                ("緑", Colors.LimeGreen),
                ("赤", Colors.Crimson),
                ("黄", Colors.Yellow)
            };
            
            foreach (var (name, color) in foregroundColors)
            {
                var menuItem = new MenuItem { Header = name };
                menuItem.Click += (s, e) => SetForegroundColor(color);
                foregroundMenuItem.Items.Add(menuItem);
            }
            
            return foregroundMenuItem;
        }
        
        private MenuItem CreateHighlightColorMenu()
        {
            var highlightMenuItem = new MenuItem { Header = "ハイライト色" };
            
            var highlightColors = new[]
            {
                ("緑", Color.FromArgb(180, 0, 255, 0)),
                ("赤", Color.FromArgb(180, 255, 68, 68)),
                ("青", Color.FromArgb(180, 68, 136, 255)),
                ("オレンジ", Color.FromArgb(180, 255, 136, 68)),
                ("紫", Color.FromArgb(180, 136, 68, 255)),
                ("黄", Color.FromArgb(180, 255, 255, 68)),
                ("シアン", Color.FromArgb(180, 68, 255, 255))
            };
            
            foreach (var (name, color) in highlightColors)
            {
                var menuItem = new MenuItem { Header = name };
                menuItem.Click += (s, e) => SetHighlightColor(color);
                highlightMenuItem.Items.Add(menuItem);
            }
            
            return highlightMenuItem;
        }
        
        private MenuItem CreateViewOptionsMenu()
        {
            var viewMenuItem = new MenuItem { Header = "表示オプション" };
            
            var topmostMenuItem = new MenuItem { Header = "最前面固定", IsCheckable = true, IsChecked = _settings.IsTopmost };
            topmostMenuItem.Click += (s, e) => ToggleTopmost();
            
            var mouseVisibilityMenuItem = new MenuItem { Header = "マウス表示", IsCheckable = true, IsChecked = _settings.IsMouseVisible };
            mouseVisibilityMenuItem.Click += (s, e) => ToggleMouseVisibility();
            
            var scaleMenuItem = CreateDisplayScaleMenu();
            
            viewMenuItem.Items.Add(topmostMenuItem);
            viewMenuItem.Items.Add(mouseVisibilityMenuItem);
            viewMenuItem.Items.Add(scaleMenuItem);
            
            return viewMenuItem;
        }
        
        private MenuItem CreateDisplayScaleMenu()
        {
            var scaleMenuItem = new MenuItem { Header = "表示サイズ" };
            
            var scaleOptions = new[]
            {
                ("80%", Constants.ScaleOptions.Scale80),
                ("100%", Constants.ScaleOptions.Scale100),
                ("120%", Constants.ScaleOptions.Scale120),
                ("150%", Constants.ScaleOptions.Scale150)
            };
            
            foreach (var (name, scale) in scaleOptions)
            {
                var menuItem = new MenuItem 
                { 
                    Header = name, 
                    IsCheckable = true, 
                    IsChecked = Math.Abs(_settings.DisplayScale - scale) < 0.01 
                };
                menuItem.Click += (s, e) => 
                {
                    SetDisplayScale(scale);
                    UpdateScaleMenuChecked(scaleMenuItem, menuItem);
                };
                scaleMenuItem.Items.Add(menuItem);
            }
            
            return scaleMenuItem;
        }
        
        private MenuItem CreateProfileMenu()
        {
            var profileMenuItem = new MenuItem { Header = "プロファイル" };
            
            var profileOptions = new[]
            {
                ("65%キーボード", KeyboardProfile.FullKeyboard65, "FullKeyboard65"),
                ("FPSキーボード", KeyboardProfile.FPSKeyboard, "FPSKeyboard")
            };
            
            foreach (var (name, profile, settingName) in profileOptions)
            {
                var menuItem = new MenuItem 
                { 
                    Header = name, 
                    IsCheckable = true, 
                    IsChecked = _settings.CurrentProfile == settingName 
                };
                menuItem.Click += (s, e) => 
                {
                    SwitchProfile(profile);
                    UpdateProfileMenuChecked(profileMenuItem, menuItem);
                };
                profileMenuItem.Items.Add(menuItem);
            }
            
            return profileMenuItem;
        }
        
        private MenuItem CreateExitMenu()
        {
            var exitMenuItem = new MenuItem { Header = "終了" };
            exitMenuItem.Click += (s, e) => Application.Current.Shutdown();
            return exitMenuItem;
        }
        
        private void SetBackgroundColor(Color color, bool transparent)
        {
            _transparentMode = transparent;
            if (transparent)
            {
                Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            }
            else
            {
                Background = new SolidColorBrush(color);
            }
        }
        
        private void SetForegroundColor(Color color)
        {
            _foregroundBrush = new SolidColorBrush(color);
            UpdateAllTextForeground();
            SaveSettings();
        }
        
        private void SetHighlightColor(Color color)
        {
            _activeBrush = new SolidColorBrush(color);
            SaveSettings();
        }
        
        private void ToggleTopmost()
        {
            Topmost = !Topmost;
            SaveSettings();
        }
        
        private void ToggleMouseVisibility()
        {
            _isMouseVisible = !_isMouseVisible;
            UpdateMouseVisibility();
            SaveSettings();
        }
        
        private void UpdateMouseVisibility()
        {
            // プロファイルレイアウトを再適用してマウス表示状態を反映
            ApplyProfileLayout();
        }
        
        private void SetDisplayScale(double scale)
        {
            _displayScale = scale;
            ApplyDisplayScale();
            SaveSettings();
        }
        
        private void ApplyDisplayScale()
        {
            var canvas = Content as Canvas;
            if (canvas != null)
            {
                // Canvas全体にスケール変換を適用
                var transform = new ScaleTransform(_displayScale, _displayScale);
                canvas.RenderTransform = transform;
                
                // プロファイルに応じたウィンドウサイズ調整
                double baseWidth, baseHeight;
                if (_currentProfile == KeyboardProfile.FPSKeyboard)
                {
                    baseWidth = _isMouseVisible ? Constants.WindowSizes.FpsKeyboardWidthWithMouse : Constants.WindowSizes.FpsKeyboardWidth;
                    baseHeight = Constants.WindowSizes.FpsKeyboardHeight;
                }
                else
                {
                    baseWidth = Constants.WindowSizes.FullKeyboardWidth;
                    baseHeight = Constants.WindowSizes.FullKeyboardHeight;
                }
                
                Width = baseWidth * _displayScale;
                Height = baseHeight * _displayScale;
            }
        }
        
        private void UpdateProfileMenuChecked(MenuItem profileMenu, MenuItem selectedItem)
        {
            // 全てのプロファイルメニューアイテムのチェック状態をクリア
            foreach (MenuItem item in profileMenu.Items)
            {
                if (item.IsCheckable)
                {
                    item.IsChecked = false;
                }
            }
            // 選択されたアイテムのみチェック
            selectedItem.IsChecked = true;
        }
        
        private void UpdateScaleMenuChecked(MenuItem scaleMenu, MenuItem selectedItem)
        {
            // 全ての表示サイズメニューアイテムのチェック状態をクリア
            foreach (MenuItem item in scaleMenu.Items)
            {
                if (item.IsCheckable)
                {
                    item.IsChecked = false;
                }
            }
            // 選択されたアイテムのみチェック
            selectedItem.IsChecked = true;
        }
        
        private void SwitchProfile(KeyboardProfile profile)
        {
            _currentProfile = profile;
            ApplyProfileLayout();
            UpdateMousePositions();
            SaveSettings();
        }
        
        private void ApplyProfileLayout()
        {
            var canvas = Content as Canvas;
            if (canvas == null) return;
            
            switch (_currentProfile)
            {
                case KeyboardProfile.FullKeyboard65:
                    ShowFullKeyboardLayout();
                    // ウィンドウサイズ調整
                    Width = Constants.WindowSizes.FullKeyboardWidth * _displayScale;
                    Height = Constants.WindowSizes.FullKeyboardHeight * _displayScale;
                    break;
                    
                case KeyboardProfile.FPSKeyboard:
                    ShowFPSKeyboardLayout();
                    // ウィンドウサイズ調整（FPS用サイズ、マウス表示考慮）
                    Width = (_isMouseVisible ? Constants.WindowSizes.FpsKeyboardWidthWithMouse : Constants.WindowSizes.FpsKeyboardWidth) * _displayScale;
                    Height = Constants.WindowSizes.FpsKeyboardHeight * _displayScale;
                    break;
            }
        }
        
        private void ShowFullKeyboardLayout()
        {
            // 全キーを表示
            var canvas = Content as Canvas;
            if (canvas == null) return;
            
            foreach (UIElement child in canvas.Children)
            {
                if (child is Border border && !string.IsNullOrEmpty(border.Name))
                {
                    // マウス要素は現在の設定に従う
                    if (IsMouseElement(border.Name))
                    {
                        border.Visibility = _isMouseVisible ? Visibility.Visible : Visibility.Collapsed;
                    }
                    else
                    {
                        border.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    // マウス本体など名前なし要素の処理
                    child.Visibility = _isMouseVisible ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }
        
        private void ShowFPSKeyboardLayout()
        {
            // FPSに重要なキーのみ表示
            var fpsKeys = new HashSet<string>
            {
                // 数字行
                "KeyEscape", "Key1", "Key2", "Key3", "Key4", "Key5", "Key6", "Key7",
                // QWERTY行
                "KeyTab", "KeyQ", "KeyW", "KeyE", "KeyR", "KeyT", "KeyY", "KeyU",
                // ASDF行
                "KeyCapsLock", "KeyA", "KeyS", "KeyD", "KeyF", "KeyG", "KeyH", "KeyJ",
                // ZXCV行
                "KeyShift", "KeyZ", "KeyX", "KeyC", "KeyV", "KeyB", "KeyN", "KeyM",
                // 下段
                "KeyCtrl", "KeyWin", "KeyAlt", "KeySpace"
            };
            
            var canvas = Content as Canvas;
            if (canvas == null) return;
            
            foreach (UIElement child in canvas.Children)
            {
                if (child is Border border && !string.IsNullOrEmpty(border.Name))
                {
                    if (IsMouseElement(border.Name))
                    {
                        // マウス要素は設定に従う
                        border.Visibility = _isMouseVisible ? Visibility.Visible : Visibility.Collapsed;
                    }
                    else if (fpsKeys.Contains(border.Name))
                    {
                        // FPSキーは表示
                        border.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        // その他のキーは非表示
                        border.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    // マウス本体など名前なし要素
                    child.Visibility = _isMouseVisible ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }
        
        private bool IsMouseElement(string elementName)
        {
            var mouseElements = new HashSet<string>
            {
                "MouseBody", "MouseLeft", "MouseRight", "MouseWheelButton", 
                "MouseButton4", "MouseButton5", "ScrollUp", "ScrollDown"
            };
            return mouseElements.Contains(elementName);
        }
        
        private void UpdateMousePositions()
        {
            if (!_mousePositions.TryGetValue(_currentProfile, out var position))
                return;
            
            // マウス要素の相対位置定義
            var mouseElementOffsets = new Dictionary<string, (double Left, double Top)>
            {
                { "MouseBody", (0, 0) },           // 基準位置
                { "MouseLeft", (3, 3) },
                { "MouseRight", (32, 3) },
                { "MouseWheelButton", (25, 10) },
                { "MouseButton4", (0, 42) },
                { "MouseButton5", (0, 64) },
                { "ScrollUp", (35, 10) },
                { "ScrollDown", (35, 24) }
            };
            
            // 全マウス要素の位置を一括更新
            foreach (var (elementName, offset) in mouseElementOffsets)
            {
                var element = FindName(elementName) as FrameworkElement;
                if (element != null)
                {
                    Canvas.SetLeft(element, position.Left + offset.Left);
                    Canvas.SetTop(element, position.Top + offset.Top);
                }
            }
        }
        
        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var yaml = File.ReadAllText(_settingsPath);
                    var deserializer = new DeserializerBuilder().Build();
                    _settings = deserializer.Deserialize<AppSettings>(yaml) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                // 設定読み込みエラー時はデフォルト設定を使用
                System.Diagnostics.Debug.WriteLine($"設定読み込みエラー: {ex.Message}");
                _settings = new AppSettings();
            }
        }
        
        private void SaveSettings()
        {
            try
            {
                // 現在の設定を保存用データに反映
                _settings.CurrentProfile = _currentProfile.ToString();
                _settings.DisplayScale = _displayScale;
                _settings.IsMouseVisible = _isMouseVisible;
                _settings.IsTopmost = Topmost;
                _settings.WindowLeft = Left;
                _settings.WindowTop = Top;
                
                // 色設定の保存（簡易的に色名で保存）
                _settings.ForegroundColor = GetColorName(_foregroundBrush);
                _settings.HighlightColor = GetColorName(_activeBrush);
                
                // ディレクトリが存在しない場合は作成
                var directory = Path.GetDirectoryName(_settingsPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // YAML形式で保存
                var serializer = new SerializerBuilder().Build();
                var yaml = serializer.Serialize(_settings);
                File.WriteAllText(_settingsPath, yaml);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"設定保存エラー: {ex.Message}");
            }
        }
        
        private void ApplySettings()
        {
            // プロファイル設定
            if (Enum.TryParse<KeyboardProfile>(_settings.CurrentProfile, out var profile))
            {
                _currentProfile = profile;
            }
            
            // 表示設定
            _displayScale = _settings.DisplayScale;
            _isMouseVisible = _settings.IsMouseVisible;
            Topmost = _settings.IsTopmost;
            
            // ウィンドウ位置設定
            if (_settings.WindowLeft > 0 && _settings.WindowTop > 0)
            {
                Left = _settings.WindowLeft;
                Top = _settings.WindowTop;
            }
            
            // 色設定
            _foregroundBrush = GetBrushFromColorName(_settings.ForegroundColor);
            _activeBrush = GetBrushFromColorName(_settings.HighlightColor);
            
            // レイアウトとスケールを適用
            ApplyProfileLayout();
            ApplyDisplayScale();  // DisplayScaleを明示的に適用
            UpdateMousePositions();
            UpdateAllTextForeground();
        }
        
        private string GetColorName(Brush brush)
        {
            if (brush is SolidColorBrush solidBrush)
            {
                var color = solidBrush.Color;
                if (color == Colors.White) return "White";
                if (color == Colors.Black) return "Black";
                if (color == Colors.Gray) return "Gray";
                if (color == Colors.CornflowerBlue) return "Blue";
                if (color == Colors.LimeGreen) return "Green";
                if (color == Colors.Crimson) return "Red";
                if (color == Colors.Yellow) return "Yellow";
                
                // ハイライト色の判定
                if (color.A == 180 && color.R == 0 && color.G == 255 && color.B == 0) return "Green";
                if (color.A == 180 && color.R == 255 && color.G == 68 && color.B == 68) return "Red";
                if (color.A == 180 && color.R == 68 && color.G == 136 && color.B == 255) return "Blue";
                if (color.A == 180 && color.R == 255 && color.G == 136 && color.B == 68) return "Orange";
                if (color.A == 180 && color.R == 136 && color.G == 68 && color.B == 255) return "Purple";
                if (color.A == 180 && color.R == 255 && color.G == 255 && color.B == 68) return "Yellow";
                if (color.A == 180 && color.R == 68 && color.G == 255 && color.B == 255) return "Cyan";
            }
            return "White";
        }
        
        private Brush GetBrushFromColorName(string colorName)
        {
            return colorName switch
            {
                "White" => Brushes.White,
                "Black" => Brushes.Black,
                "Gray" => Brushes.Gray,
                "Blue" => new SolidColorBrush(Colors.CornflowerBlue),
                "Green" => new SolidColorBrush(Color.FromArgb(180, 0, 255, 0)),
                "Red" => new SolidColorBrush(Colors.Crimson),
                "Yellow" => Brushes.Yellow,
                "Orange" => new SolidColorBrush(Color.FromArgb(180, 255, 136, 68)),
                "Purple" => new SolidColorBrush(Color.FromArgb(180, 136, 68, 255)),
                "Cyan" => new SolidColorBrush(Color.FromArgb(180, 68, 255, 255)),
                _ => new SolidColorBrush(Color.FromArgb(180, 0, 255, 0))
            };
        }
        
        private void UpdateAllTextForeground()
        {
            // Canvas内のすべてのTextBlockを探してフォアグラウンド色を更新
            var canvas = Content as Canvas;
            if (canvas != null)
            {
                foreach (var child in canvas.Children)
                {
                    if (child is Border border)
                    {
                        UpdateBorderTextForeground(border);
                    }
                }
            }
        }
        
        private void UpdateBorderTextForeground(Border border)
        {
            if (border.Child is TextBlock textBlock)
            {
                textBlock.Foreground = _foregroundBrush;
            }
            else if (border.Child is StackPanel stackPanel)
            {
                foreach (var child in stackPanel.Children)
                {
                    if (child is TextBlock tb)
                    {
                        tb.Foreground = _foregroundBrush;
                    }
                }
            }
        }

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

        private void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                ReleaseMouseCapture();
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            bool isShiftPressed = IsKeyPressed(VK_LSHIFT) || IsKeyPressed(VK_RSHIFT);
            
            // プロファイルに応じてキー更新処理を分岐
            if (_currentProfile == KeyboardProfile.FPSKeyboard)
            {
                UpdateFPSKeys(isShiftPressed);
            }
            else
            {
                UpdateFullKeyboard(isShiftPressed);
            }
            
            // マウス入力（表示時のみ更新）
            if (_isMouseVisible)
            {
                UpdateKeyStateByName("MouseLeft", VK_LBUTTON);
                UpdateKeyStateByName("MouseRight", VK_RBUTTON);
                UpdateKeyStateByName("MouseWheelButton", VK_MBUTTON);
                UpdateKeyStateByName("MouseButton4", VK_XBUTTON2);
                UpdateKeyStateByName("MouseButton5", VK_XBUTTON1);
                
                // スクロール表示の更新
                UpdateScrollIndicators();
            }
        }
        
        private void UpdateFPSKeys(bool isShiftPressed)
        {
            // FPSプロファイル用キー更新
            // 数字行
            UpdateKeyStateByName("KeyEscape", VK_ESCAPE);
            UpdateKeyStateWithShift("Key1", VK_1, "1", "!", isShiftPressed);
            UpdateKeyStateWithShift("Key2", VK_2, "2", "@", isShiftPressed);
            UpdateKeyStateWithShift("Key3", VK_3, "3", "#", isShiftPressed);
            UpdateKeyStateWithShift("Key4", VK_4, "4", "$", isShiftPressed);
            UpdateKeyStateWithShift("Key5", VK_5, "5", "%", isShiftPressed);
            UpdateKeyStateWithShift("Key6", VK_6, "6", "^", isShiftPressed);
            UpdateKeyStateWithShift("Key7", VK_7, "7", "&", isShiftPressed);
            
            // QWERTY行
            UpdateKeyStateByName("KeyTab", VK_TAB);
            UpdateKeyStateByName("KeyQ", VK_Q);
            UpdateKeyState(KeyW, VK_W);
            UpdateKeyStateByName("KeyE", VK_E);
            UpdateKeyStateByName("KeyR", VK_R);
            UpdateKeyStateByName("KeyT", VK_T);
            UpdateKeyStateByName("KeyY", VK_Y);
            UpdateKeyStateByName("KeyU", VK_U);
            
            // ASDF行
            UpdateKeyStateByName("KeyCapsLock", VK_CAPS_LOCK);
            UpdateKeyState(KeyA, VK_A);
            UpdateKeyState(KeyS, VK_S);
            UpdateKeyState(KeyD, VK_D);
            UpdateKeyStateByName("KeyF", VK_F);
            UpdateKeyStateByName("KeyG", VK_G);
            UpdateKeyStateByName("KeyH", VK_H);
            UpdateKeyStateByName("KeyJ", VK_J);
            
            // ZXCV行
            UpdateKeyState(KeyShift, VK_LSHIFT);
            UpdateKeyStateByName("KeyZ", VK_Z);
            UpdateKeyStateByName("KeyX", VK_X);
            UpdateKeyStateByName("KeyC", VK_C);
            UpdateKeyStateByName("KeyV", VK_V);
            UpdateKeyStateByName("KeyB", VK_B);
            UpdateKeyStateByName("KeyN", VK_N);
            UpdateKeyStateByName("KeyM", VK_M);
            
            // 下段
            UpdateKeyState(KeyCtrl, VK_LCONTROL);
            UpdateKeyStateByName("KeyWin", VK_WIN);
            UpdateKeyStateByName("KeyAlt", VK_LMENU);
            UpdateKeyState(KeySpace, VK_SPACE);
        }
        
        private void UpdateFullKeyboard(bool isShiftPressed)
        {
            // ESCキー
            UpdateKeyStateByName("KeyEscape", VK_ESCAPE);
            
            // 数字キー（Shiftキー押下時の表示切り替え対応）
            UpdateKeyStateWithShift("Key1", VK_1, "1", "!", isShiftPressed);
            UpdateKeyStateWithShift("Key2", VK_2, "2", "@", isShiftPressed);
            UpdateKeyStateWithShift("Key3", VK_3, "3", "#", isShiftPressed);
            UpdateKeyStateWithShift("Key4", VK_4, "4", "$", isShiftPressed);
            UpdateKeyStateWithShift("Key5", VK_5, "5", "%", isShiftPressed);
            UpdateKeyStateWithShift("Key6", VK_6, "6", "^", isShiftPressed);
            UpdateKeyStateWithShift("Key7", VK_7, "7", "&", isShiftPressed);
            UpdateKeyStateWithShift("Key8", VK_8, "8", "*", isShiftPressed);
            UpdateKeyStateWithShift("Key9", VK_9, "9", "(", isShiftPressed);
            UpdateKeyStateWithShift("Key0", VK_0, "0", ")", isShiftPressed);
            UpdateKeyStateWithShift("KeyMinus", VK_MINUS, "-", "_", isShiftPressed);
            UpdateKeyStateWithShift("KeyEquals", VK_EQUALS, "=", "+", isShiftPressed);
            UpdateKeyStateByName("KeyBackspace", VK_BACKSPACE);
            
            // QWERTYキー
            UpdateKeyStateByName("KeyTab", VK_TAB);
            UpdateKeyStateByName("KeyQ", VK_Q);
            UpdateKeyState(KeyW, VK_W);
            UpdateKeyStateByName("KeyE", VK_E);
            UpdateKeyStateByName("KeyR", VK_R);
            UpdateKeyStateByName("KeyT", VK_T);
            UpdateKeyStateByName("KeyY", VK_Y);
            UpdateKeyStateByName("KeyU", VK_U);
            UpdateKeyStateByName("KeyI", VK_I);
            UpdateKeyStateByName("KeyO", VK_O);
            UpdateKeyStateByName("KeyP", VK_P);
            UpdateKeyStateWithShift("KeyOpenBracket", VK_OPEN_BRACKET, "[", "{", isShiftPressed);
            UpdateKeyStateWithShift("KeyCloseBracket", VK_CLOSE_BRACKET, "]", "}", isShiftPressed);
            UpdateKeyStateWithShift("KeyBackslash", VK_BACKSLASH, "\\", "|", isShiftPressed);
            
            // ASDFキー
            UpdateKeyStateByName("KeyCapsLock", VK_CAPS_LOCK);
            UpdateKeyState(KeyA, VK_A);
            UpdateKeyState(KeyS, VK_S);
            UpdateKeyState(KeyD, VK_D);
            UpdateKeyStateByName("KeyF", VK_F);
            UpdateKeyStateByName("KeyG", VK_G);
            UpdateKeyStateByName("KeyH", VK_H);
            UpdateKeyStateByName("KeyJ", VK_J);
            UpdateKeyStateByName("KeyK", VK_K);
            UpdateKeyStateByName("KeyL", VK_L);
            UpdateKeyStateWithShift("KeySemicolon", VK_SEMICOLON, ";", ":", isShiftPressed);
            UpdateKeyStateWithShift("KeyQuote", VK_QUOTE, "'", "\"", isShiftPressed);
            UpdateKeyStateByName("KeyEnter", VK_ENTER);
            
            // ZXCVキー
            // 左ShiftキーはVK_LSHIFTで検知
            UpdateKeyState(KeyShift, VK_LSHIFT);
            UpdateKeyStateByName("KeyZ", VK_Z);
            UpdateKeyStateByName("KeyX", VK_X);
            UpdateKeyStateByName("KeyC", VK_C);
            UpdateKeyStateByName("KeyV", VK_V);
            UpdateKeyStateByName("KeyB", VK_B);
            UpdateKeyStateByName("KeyN", VK_N);
            UpdateKeyStateByName("KeyM", VK_M);
            UpdateKeyStateWithShift("KeyComma", VK_COMMA, ",", "<", isShiftPressed);
            UpdateKeyStateWithShift("KeyPeriod", VK_PERIOD, ".", ">", isShiftPressed);
            UpdateKeyStateWithShift("KeySlash", VK_SLASH, "/", "?", isShiftPressed);
            UpdateKeyStateByName("KeyRightShift", VK_RSHIFT);
            UpdateKeyStateByName("KeyUpArrow", VK_UP);
            
            // 最下段キー（左右個別検知）
            UpdateKeyState(KeyCtrl, VK_LCONTROL);
            UpdateKeyStateByName("KeyWin", VK_WIN);
            UpdateKeyStateByName("KeyAlt", VK_LMENU);
            UpdateKeyState(KeySpace, VK_SPACE);
            UpdateKeyStateByName("KeyRightAlt", VK_RMENU);
            UpdateKeyStateByName("KeyFn", 0); // Fnキーは検出不可
            UpdateKeyStateByName("KeyRightCtrl", VK_RCONTROL);
            UpdateKeyStateByName("KeyLeftArrow", VK_LEFT);
            UpdateKeyStateByName("KeyDownArrow", VK_DOWN);
            UpdateKeyStateByName("KeyRightArrow", VK_RIGHT);
            
            // ナビゲーションキー
            UpdateKeyStateByName("KeyDelete", VK_DELETE);
            UpdateKeyStateByName("KeyHome", VK_HOME);
            UpdateKeyStateByName("KeyPageUp", VK_PAGE_UP);
            UpdateKeyStateByName("KeyPageDown", VK_PAGE_DOWN);
        }

        private void UpdateKeyState(System.Windows.Controls.Border keyBorder, int virtualKeyCode)
        {
            bool isPressed = IsKeyPressed(virtualKeyCode);
            keyBorder.Background = isPressed ? _activeBrush : _inactiveBrush;
        }

        private void UpdateKeyStateByName(string keyName, int virtualKeyCode)
        {
            if (virtualKeyCode == 0) return; // Fnキーなど検出不可のキー
            
            var keyBorder = FindName(keyName) as System.Windows.Controls.Border;
            if (keyBorder != null)
            {
                bool isPressed = IsKeyPressed(virtualKeyCode);
                keyBorder.Background = isPressed ? _activeBrush : _inactiveBrush;
            }
        }

        private void UpdateKeyStateWithShift(string keyName, int virtualKeyCode, string normalText, string shiftText, bool isShiftPressed)
        {
            var keyBorder = FindName(keyName) as System.Windows.Controls.Border;
            var textBlock = FindName(keyName + "Text") as System.Windows.Controls.TextBlock;
            
            if (keyBorder != null && textBlock != null)
            {
                bool isPressed = IsKeyPressed(virtualKeyCode);
                keyBorder.Background = isPressed ? _activeBrush : _inactiveBrush;
                
                // プロファイルのShift表示設定を確認
                bool shouldShowShiftText = isShiftPressed && _shiftDisplayEnabled.GetValueOrDefault(_currentProfile, true);
                textBlock.Text = shouldShowShiftText ? shiftText : normalText;
            }
        }


        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ContextMenu != null)
            {
                ContextMenu.IsOpen = true;
            }
        }

        private void KeyBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _dragStartPoint = e.GetPosition(this);
            CaptureMouse();
            e.Handled = true;
        }

        private void KeyBorder_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ContextMenu != null)
            {
                ContextMenu.IsOpen = true;
            }
            e.Handled = true;
        }
        
        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_isMouseVisible)
            {
                // スクロール表示（マウス表示時のみ）
                if (e.Delta > 0)
                {
                    // 上スクロール
                    _scrollUpTimer = Constants.ScrollDisplayFrames;
                }
                else if (e.Delta < 0)
                {
                    // 下スクロール
                    _scrollDownTimer = Constants.ScrollDisplayFrames;
                }
            }
        }
        
        private void UpdateScrollIndicators()
        {
            // スクロールアップ表示
            var scrollUpIndicator = FindName("ScrollUpIndicator") as TextBlock;
            if (scrollUpIndicator != null)
            {
                if (_scrollUpTimer > 0)
                {
                    scrollUpIndicator.Foreground = _activeBrush; // ハイライト色を使用
                    _scrollUpTimer--;
                }
                else
                {
                    scrollUpIndicator.Foreground = _inactiveBrush;
                }
            }
            
            // スクロールダウン表示
            var scrollDownIndicator = FindName("ScrollDownIndicator") as TextBlock;
            if (scrollDownIndicator != null)
            {
                if (_scrollDownTimer > 0)
                {
                    scrollDownIndicator.Foreground = _activeBrush; // ハイライト色を使用
                    _scrollDownTimer--;
                }
                else
                {
                    scrollDownIndicator.Foreground = _inactiveBrush;
                }
            }
        }
    }
}
