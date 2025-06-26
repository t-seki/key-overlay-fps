using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    // 色管理クラス
    public static class ColorManager
    {
        private static readonly Dictionary<string, Color> ForegroundColors = new()
        {
            { "White", Colors.White },
            { "Black", Colors.Black },
            { "Gray", Colors.Gray },
            { "Blue", Colors.CornflowerBlue },
            { "Green", Colors.LimeGreen },
            { "Red", Colors.Crimson },
            { "Yellow", Colors.Yellow }
        };
        
        private static readonly Dictionary<string, Color> HighlightColors = new()
        {
            { "Green", Color.FromArgb(180, 0, 255, 0) },
            { "Red", Color.FromArgb(180, 255, 68, 68) },
            { "Blue", Color.FromArgb(180, 68, 136, 255) },
            { "Orange", Color.FromArgb(180, 255, 136, 68) },
            { "Purple", Color.FromArgb(180, 136, 68, 255) },
            { "Yellow", Color.FromArgb(180, 255, 255, 68) },
            { "Cyan", Color.FromArgb(180, 68, 255, 255) }
        };
        
        private static readonly Dictionary<string, Color> BackgroundColors = new()
        {
            { "Transparent", Colors.Transparent },
            { "Lime", Colors.Lime },
            { "Blue", Colors.Blue },
            { "Black", Colors.Black }
        };
        
        public static Brush GetForegroundBrush(string colorName) =>
            ForegroundColors.TryGetValue(colorName, out var color) ? new SolidColorBrush(color) : Brushes.White;
            
        public static Brush GetHighlightBrush(string colorName) =>
            HighlightColors.TryGetValue(colorName, out var color) ? new SolidColorBrush(color) : new SolidColorBrush(Color.FromArgb(180, 0, 255, 0));
            
        public static Color GetBackgroundColor(string colorName) =>
            BackgroundColors.TryGetValue(colorName, out var color) ? color : Colors.Transparent;
            
        public static string GetColorName(Brush brush)
        {
            if (brush is not SolidColorBrush solidBrush) return "White";
            
            var color = solidBrush.Color;
            
            // 前景色チェック
            foreach (var (name, c) in ForegroundColors)
                if (color == c) return name;
                
            // ハイライト色チェック
            foreach (var (name, c) in HighlightColors)
                if (color.A == c.A && color.R == c.R && color.G == c.G && color.B == c.B) return name;
                
            return "White";
        }
        
        public static string GetBackgroundColorName(Color color)
        {
            foreach (var (name, c) in BackgroundColors)
                if (color == c) return name;
            return "Transparent";
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

    // キー設定クラス
    public class KeyConfig
    {
        public string Name { get; }
        public int VirtualKey { get; }
        public string NormalText { get; }
        public string ShiftText { get; }
        public bool HasShiftVariant { get; }
        
        public KeyConfig(string name, int virtualKey, string normalText = "", string shiftText = "")
        {
            Name = name;
            VirtualKey = virtualKey;
            NormalText = normalText;
            ShiftText = shiftText;
            HasShiftVariant = !string.IsNullOrEmpty(shiftText);
        }
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

        // よく使用されるキーコードのみ保持
        private const int VK_LSHIFT = 0xA0;
        private const int VK_RSHIFT = 0xA1;
        private const int VK_LBUTTON = 0x01;
        private const int VK_RBUTTON = 0x02;
        private const int VK_MBUTTON = 0x04;
        private const int VK_XBUTTON1 = 0x05;
        private const int VK_XBUTTON2 = 0x06;

        private readonly DispatcherTimer _timer;
        private Brush _activeBrush = new SolidColorBrush(Color.FromArgb(180, 0, 255, 0));
        private readonly Brush _inactiveBrush;
        private readonly Brush _keyboardKeyDefaultBrush;
        
        // UIエレメントキャッシュ
        private readonly Dictionary<string, FrameworkElement> _elementCache = new();
        // スクロール表示色（ハイライト色と同じ）
        private bool _isDragging = false;
        private Point _dragStartPoint;
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
        
        // キー設定定義
        private readonly Dictionary<string, KeyConfig> _keyConfigurations = new()
        {
            // 数字キー
            { "KeyEscape", new KeyConfig("KeyEscape", 0x1B) },
            { "Key1", new KeyConfig("Key1", 0x31, "1", "!") },
            { "Key2", new KeyConfig("Key2", 0x32, "2", "@") },
            { "Key3", new KeyConfig("Key3", 0x33, "3", "#") },
            { "Key4", new KeyConfig("Key4", 0x34, "4", "$") },
            { "Key5", new KeyConfig("Key5", 0x35, "5", "%") },
            { "Key6", new KeyConfig("Key6", 0x36, "6", "^") },
            { "Key7", new KeyConfig("Key7", 0x37, "7", "&") },
            { "Key8", new KeyConfig("Key8", 0x38, "8", "*") },
            { "Key9", new KeyConfig("Key9", 0x39, "9", "(") },
            { "Key0", new KeyConfig("Key0", 0x30, "0", ")") },
            { "KeyMinus", new KeyConfig("KeyMinus", 0xBD, "-", "_") },
            { "KeyEquals", new KeyConfig("KeyEquals", 0xBB, "=", "+") },
            { "KeyBackspace", new KeyConfig("KeyBackspace", 0x08) },
            
            // QWERTYキー
            { "KeyTab", new KeyConfig("KeyTab", 0x09) },
            { "KeyQ", new KeyConfig("KeyQ", 0x51) },
            { "KeyW", new KeyConfig("KeyW", 0x57) },
            { "KeyE", new KeyConfig("KeyE", 0x45) },
            { "KeyR", new KeyConfig("KeyR", 0x52) },
            { "KeyT", new KeyConfig("KeyT", 0x54) },
            { "KeyY", new KeyConfig("KeyY", 0x59) },
            { "KeyU", new KeyConfig("KeyU", 0x55) },
            { "KeyI", new KeyConfig("KeyI", 0x49) },
            { "KeyO", new KeyConfig("KeyO", 0x4F) },
            { "KeyP", new KeyConfig("KeyP", 0x50) },
            { "KeyOpenBracket", new KeyConfig("KeyOpenBracket", 0xDB, "[", "{") },
            { "KeyCloseBracket", new KeyConfig("KeyCloseBracket", 0xDD, "]", "}") },
            { "KeyBackslash", new KeyConfig("KeyBackslash", 0xDC, "\\", "|") },
            
            // ASDFキー
            { "KeyCapsLock", new KeyConfig("KeyCapsLock", 0x14) },
            { "KeyA", new KeyConfig("KeyA", 0x41) },
            { "KeyS", new KeyConfig("KeyS", 0x53) },
            { "KeyD", new KeyConfig("KeyD", 0x44) },
            { "KeyF", new KeyConfig("KeyF", 0x46) },
            { "KeyG", new KeyConfig("KeyG", 0x47) },
            { "KeyH", new KeyConfig("KeyH", 0x48) },
            { "KeyJ", new KeyConfig("KeyJ", 0x4A) },
            { "KeyK", new KeyConfig("KeyK", 0x4B) },
            { "KeyL", new KeyConfig("KeyL", 0x4C) },
            { "KeySemicolon", new KeyConfig("KeySemicolon", 0xBA, ";", ":") },
            { "KeyQuote", new KeyConfig("KeyQuote", 0xDE, "'", "\"") },
            { "KeyEnter", new KeyConfig("KeyEnter", 0x0D) },
            
            // ZXCVキー
            { "KeyShift", new KeyConfig("KeyShift", VK_LSHIFT) },
            { "KeyZ", new KeyConfig("KeyZ", 0x5A) },
            { "KeyX", new KeyConfig("KeyX", 0x58) },
            { "KeyC", new KeyConfig("KeyC", 0x43) },
            { "KeyV", new KeyConfig("KeyV", 0x56) },
            { "KeyB", new KeyConfig("KeyB", 0x42) },
            { "KeyN", new KeyConfig("KeyN", 0x4E) },
            { "KeyM", new KeyConfig("KeyM", 0x4D) },
            { "KeyComma", new KeyConfig("KeyComma", 0xBC, ",", "<") },
            { "KeyPeriod", new KeyConfig("KeyPeriod", 0xBE, ".", ">") },
            { "KeySlash", new KeyConfig("KeySlash", 0xBF, "/", "?") },
            { "KeyRightShift", new KeyConfig("KeyRightShift", VK_RSHIFT) },
            { "KeyUpArrow", new KeyConfig("KeyUpArrow", 0x26) },
            
            // 最下段キー
            { "KeyCtrl", new KeyConfig("KeyCtrl", 0xA2) },
            { "KeyWin", new KeyConfig("KeyWin", 0x5B) },
            { "KeyAlt", new KeyConfig("KeyAlt", 0xA4) },
            { "KeySpace", new KeyConfig("KeySpace", 0x20) },
            { "KeyRightAlt", new KeyConfig("KeyRightAlt", 0xA5) },
            { "KeyFn", new KeyConfig("KeyFn", 0) }, // Fnキーは検出不可
            { "KeyRightCtrl", new KeyConfig("KeyRightCtrl", 0xA3) },
            { "KeyLeftArrow", new KeyConfig("KeyLeftArrow", 0x25) },
            { "KeyDownArrow", new KeyConfig("KeyDownArrow", 0x28) },
            { "KeyRightArrow", new KeyConfig("KeyRightArrow", 0x27) },
            
            // ナビゲーションキー
            { "KeyDelete", new KeyConfig("KeyDelete", 0x2E) },
            { "KeyHome", new KeyConfig("KeyHome", 0x24) },
            { "KeyPageUp", new KeyConfig("KeyPageUp", 0x21) },
            { "KeyPageDown", new KeyConfig("KeyPageDown", 0x22) }
        };
        
        // プロファイル別キー設定
        private readonly Dictionary<KeyboardProfile, HashSet<string>> _profileKeys = new()
        {
            [KeyboardProfile.FullKeyboard65] = new HashSet<string>
            {
                // 全キーを含む
                "KeyEscape", "Key1", "Key2", "Key3", "Key4", "Key5", "Key6", "Key7", "Key8", "Key9", "Key0", "KeyMinus", "KeyEquals", "KeyBackspace",
                "KeyTab", "KeyQ", "KeyW", "KeyE", "KeyR", "KeyT", "KeyY", "KeyU", "KeyI", "KeyO", "KeyP", "KeyOpenBracket", "KeyCloseBracket", "KeyBackslash",
                "KeyCapsLock", "KeyA", "KeyS", "KeyD", "KeyF", "KeyG", "KeyH", "KeyJ", "KeyK", "KeyL", "KeySemicolon", "KeyQuote", "KeyEnter",
                "KeyShift", "KeyZ", "KeyX", "KeyC", "KeyV", "KeyB", "KeyN", "KeyM", "KeyComma", "KeyPeriod", "KeySlash", "KeyRightShift", "KeyUpArrow",
                "KeyCtrl", "KeyWin", "KeyAlt", "KeySpace", "KeyRightAlt", "KeyFn", "KeyRightCtrl", "KeyLeftArrow", "KeyDownArrow", "KeyRightArrow",
                "KeyDelete", "KeyHome", "KeyPageUp", "KeyPageDown"
            },
            [KeyboardProfile.FPSKeyboard] = new HashSet<string>
            {
                // FPS用キーのみ
                "KeyEscape", "Key1", "Key2", "Key3", "Key4", "Key5", "Key6", "Key7",
                "KeyTab", "KeyQ", "KeyW", "KeyE", "KeyR", "KeyT", "KeyY", "KeyU",
                "KeyCapsLock", "KeyA", "KeyS", "KeyD", "KeyF", "KeyG", "KeyH", "KeyJ",
                "KeyShift", "KeyZ", "KeyX", "KeyC", "KeyV", "KeyB", "KeyN", "KeyM",
                "KeyCtrl", "KeyWin", "KeyAlt", "KeySpace"
            }
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
            // キーボードキーの背景ブラシを初期化（マウス本体と同じグラデーション）
            _keyboardKeyDefaultBrush = new LinearGradientBrush(
                new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(0x2A, 0x2A, 0x2A), 0),
                    new GradientStop(Color.FromRgb(0x1A, 0x1A, 0x1A), 1)
                },
                new Point(0, 0),
                new Point(1, 1)
            );
            
            _inactiveBrush = _keyboardKeyDefaultBrush;
            
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
            
            // キーボードキーの背景色を初期化
            InitializeKeyboardKeyBackgrounds();
        }
        
        private void InitializeKeyboardKeyBackgrounds()
        {
            var canvas = Content as Canvas;
            if (canvas == null) return;
            
            foreach (UIElement child in canvas.Children)
            {
                if (child is Border border && !string.IsNullOrEmpty(border.Name) && !IsMouseElement(border.Name))
                {
                    // マウス要素以外のBorderの背景をキーボードキー用背景に設定
                    border.Background = _keyboardKeyDefaultBrush;
                }
            }
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
                    UpdateMenuCheckedState(scaleMenuItem, menuItem);
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
                    UpdateMenuCheckedState(profileMenuItem, menuItem);
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
            if (transparent)
            {
                Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            }
            else
            {
                Background = new SolidColorBrush(color);
            }
            _settings.BackgroundColor = ColorManager.GetBackgroundColorName(color);
            SaveSettings();
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
        
        private static void UpdateMenuCheckedState(MenuItem parentMenu, MenuItem selectedItem)
        {
            foreach (MenuItem item in parentMenu.Items.OfType<MenuItem>())
            {
                if (item.IsCheckable)
                {
                    item.IsChecked = item == selectedItem;
                }
            }
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
        
        // マウス要素管理クラス
        private static class MouseElements
        {
            public static readonly HashSet<string> Names = new()
            {
                "MouseBody", "MouseLeft", "MouseRight", "MouseWheelButton", 
                "MouseButton4", "MouseButton5", "ScrollUp", "ScrollDown"
            };
            
            public static readonly Dictionary<string, (double Left, double Top)> Offsets = new()
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
        }
        
        private static bool IsMouseElement(string elementName) => MouseElements.Names.Contains(elementName);
        
        /// <summary>
        /// キャッシュ付きでUIエレメントを取得
        /// </summary>
        private T? GetCachedElement<T>(string name) where T : FrameworkElement
        {
            if (!_elementCache.TryGetValue(name, out var element))
            {
                element = FindName(name) as T;
                if (element != null)
                {
                    _elementCache[name] = element;
                }
            }
            return element as T;
        }
        
        private void UpdateMousePositions()
        {
            if (!_mousePositions.TryGetValue(_currentProfile, out var position))
                return;
            
            // 全マウス要素の位置を一括更新
            foreach (var (elementName, offset) in MouseElements.Offsets)
            {
                var element = GetCachedElement<FrameworkElement>(elementName);
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
                _settings.ForegroundColor = ColorManager.GetColorName(_foregroundBrush);
                _settings.HighlightColor = ColorManager.GetColorName(_activeBrush);
                // 背景色は SetBackgroundColor メソッドで既に設定済み
                
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
            _foregroundBrush = ColorManager.GetForegroundBrush(_settings.ForegroundColor);
            _activeBrush = ColorManager.GetHighlightBrush(_settings.HighlightColor);
            
            // 背景色設定を適用
            ApplyBackgroundColorFromSettings();
            
            // レイアウトとスケールを適用
            ApplyProfileLayout();
            ApplyDisplayScale();  // DisplayScaleを明示的に適用
            UpdateMousePositions();
            UpdateAllTextForeground();
        }
        
        private void ApplyBackgroundColorFromSettings()
        {
            var color = ColorManager.GetBackgroundColor(_settings.BackgroundColor ?? "Transparent");
            bool transparent = color == Colors.Transparent;
            SetBackgroundColor(color, transparent);
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
            
            // キーボードキー更新
            UpdateKeys(isShiftPressed);
            
            // マウス入力（表示時のみ更新）
            if (_isMouseVisible)
            {
                UpdateMouseKeys();
                UpdateScrollIndicators();
            }
        }
        
        private void UpdateKeys(bool isShiftPressed)
        {
            var activeKeys = _profileKeys[_currentProfile];
            bool shouldShowShiftText = isShiftPressed && _shiftDisplayEnabled.GetValueOrDefault(_currentProfile, true);
            
            foreach (var keyName in activeKeys)
            {
                if (_keyConfigurations.TryGetValue(keyName, out var config))
                {
                    if (config.HasShiftVariant)
                    {
                        UpdateKeyStateWithShift(config.Name, config.VirtualKey, config.NormalText, config.ShiftText, shouldShowShiftText);
                    }
                    else
                    {
                        UpdateKeyStateByName(config.Name, config.VirtualKey);
                    }
                }
            }
        }
        
        private void UpdateMouseKeys()
        {
            var mouseKeys = new[]
            {
                ("MouseLeft", VK_LBUTTON),
                ("MouseRight", VK_RBUTTON),
                ("MouseWheelButton", VK_MBUTTON),
                ("MouseButton4", VK_XBUTTON2),
                ("MouseButton5", VK_XBUTTON1)
            };
            
            foreach (var (keyName, virtualKey) in mouseKeys)
            {
                UpdateKeyStateByName(keyName, virtualKey);
            }
        }
        


        private void UpdateKeyStateByName(string keyName, int virtualKeyCode)
        {
            if (virtualKeyCode == 0) return; // Fnキーなど検出不可のキー
            
            var keyBorder = GetCachedElement<Border>(keyName);
            if (keyBorder != null)
            {
                bool isPressed = IsKeyPressed(virtualKeyCode);
                keyBorder.Background = isPressed ? _activeBrush : _inactiveBrush;
            }
        }

        private void UpdateKeyStateWithShift(string keyName, int virtualKeyCode, string normalText, string shiftText, bool isShiftPressed)
        {
            var keyBorder = GetCachedElement<Border>(keyName);
            var textBlock = GetCachedElement<TextBlock>(keyName + "Text");
            
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
            var scrollUpIndicator = GetCachedElement<TextBlock>("ScrollUpIndicator");
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
            var scrollDownIndicator = GetCachedElement<TextBlock>("ScrollDownIndicator");
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
