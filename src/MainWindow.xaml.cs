using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace KeyOverlayFPS
{
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
        private readonly Brush _scrollBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)); // 黄色
        private bool _isDragging = false;
        private Point _dragStartPoint;
        private bool _transparentMode = true;
        private int _scrollUpTimer = 0;
        private int _scrollDownTimer = 0;
        
        // フォアグラウンド色管理
        private Brush _foregroundBrush = Brushes.White;

        public MainWindow()
        {
            InitializeComponent();
            SetupContextMenu();
            
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16.67)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            MouseMove += MainWindow_MouseMove;
            MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
            this.MouseWheel += MainWindow_MouseWheel;
        }

        private void SetupContextMenu()
        {
            var contextMenu = new ContextMenu();
            
            // 背景色メニュー
            var backgroundMenuItem = new MenuItem { Header = "背景色" };
            
            var transparentMenuItem = new MenuItem { Header = "透明" };
            transparentMenuItem.Click += (s, e) => SetBackgroundColor(Colors.Transparent, true);
            
            var chromaGreenMenuItem = new MenuItem { Header = "クロマキー緑" };
            chromaGreenMenuItem.Click += (s, e) => SetBackgroundColor(Colors.Lime, false);
            
            var chromaBlueMenuItem = new MenuItem { Header = "クロマキー青" };
            chromaBlueMenuItem.Click += (s, e) => SetBackgroundColor(Colors.Blue, false);
            
            var blackMenuItem = new MenuItem { Header = "黒" };
            blackMenuItem.Click += (s, e) => SetBackgroundColor(Colors.Black, false);
            
            backgroundMenuItem.Items.Add(transparentMenuItem);
            backgroundMenuItem.Items.Add(chromaGreenMenuItem);
            backgroundMenuItem.Items.Add(chromaBlueMenuItem);
            backgroundMenuItem.Items.Add(blackMenuItem);
            
            // フォアグラウンド色メニュー
            var foregroundMenuItem = new MenuItem { Header = "文字色" };
            
            var whiteTextMenuItem = new MenuItem { Header = "白" };
            whiteTextMenuItem.Click += (s, e) => SetForegroundColor(Colors.White);
            
            var blackTextMenuItem = new MenuItem { Header = "黒" };
            blackTextMenuItem.Click += (s, e) => SetForegroundColor(Colors.Black);
            
            var grayTextMenuItem = new MenuItem { Header = "グレー" };
            grayTextMenuItem.Click += (s, e) => SetForegroundColor(Colors.Gray);
            
            var blueTextMenuItem = new MenuItem { Header = "青" };
            blueTextMenuItem.Click += (s, e) => SetForegroundColor(Colors.CornflowerBlue);
            
            var greenTextMenuItem = new MenuItem { Header = "緑" };
            greenTextMenuItem.Click += (s, e) => SetForegroundColor(Colors.LimeGreen);
            
            var redTextMenuItem = new MenuItem { Header = "赤" };
            redTextMenuItem.Click += (s, e) => SetForegroundColor(Colors.Crimson);
            
            var yellowTextMenuItem = new MenuItem { Header = "黄" };
            yellowTextMenuItem.Click += (s, e) => SetForegroundColor(Colors.Yellow);
            
            foregroundMenuItem.Items.Add(whiteTextMenuItem);
            foregroundMenuItem.Items.Add(blackTextMenuItem);
            foregroundMenuItem.Items.Add(grayTextMenuItem);
            foregroundMenuItem.Items.Add(blueTextMenuItem);
            foregroundMenuItem.Items.Add(greenTextMenuItem);
            foregroundMenuItem.Items.Add(redTextMenuItem);
            foregroundMenuItem.Items.Add(yellowTextMenuItem);
            
            // ハイライト色メニュー
            var highlightMenuItem = new MenuItem { Header = "ハイライト色" };
            
            var greenHighlightMenuItem = new MenuItem { Header = "緑" };
            greenHighlightMenuItem.Click += (s, e) => SetHighlightColor(Color.FromArgb(180, 0, 255, 0));
            
            var redHighlightMenuItem = new MenuItem { Header = "赤" };
            redHighlightMenuItem.Click += (s, e) => SetHighlightColor(Color.FromArgb(180, 255, 68, 68));
            
            var blueHighlightMenuItem = new MenuItem { Header = "青" };
            blueHighlightMenuItem.Click += (s, e) => SetHighlightColor(Color.FromArgb(180, 68, 136, 255));
            
            var orangeHighlightMenuItem = new MenuItem { Header = "オレンジ" };
            orangeHighlightMenuItem.Click += (s, e) => SetHighlightColor(Color.FromArgb(180, 255, 136, 68));
            
            var purpleHighlightMenuItem = new MenuItem { Header = "紫" };
            purpleHighlightMenuItem.Click += (s, e) => SetHighlightColor(Color.FromArgb(180, 136, 68, 255));
            
            var yellowHighlightMenuItem = new MenuItem { Header = "黄" };
            yellowHighlightMenuItem.Click += (s, e) => SetHighlightColor(Color.FromArgb(180, 255, 255, 68));
            
            var cyanHighlightMenuItem = new MenuItem { Header = "シアン" };
            cyanHighlightMenuItem.Click += (s, e) => SetHighlightColor(Color.FromArgb(180, 68, 255, 255));
            
            highlightMenuItem.Items.Add(greenHighlightMenuItem);
            highlightMenuItem.Items.Add(redHighlightMenuItem);
            highlightMenuItem.Items.Add(blueHighlightMenuItem);
            highlightMenuItem.Items.Add(orangeHighlightMenuItem);
            highlightMenuItem.Items.Add(purpleHighlightMenuItem);
            highlightMenuItem.Items.Add(yellowHighlightMenuItem);
            highlightMenuItem.Items.Add(cyanHighlightMenuItem);
            
            // 表示オプションメニュー
            var viewMenuItem = new MenuItem { Header = "表示オプション" };
            
            var topmostMenuItem = new MenuItem { Header = "最前面固定", IsCheckable = true, IsChecked = true };
            topmostMenuItem.Click += (s, e) => ToggleTopmost();
            
            viewMenuItem.Items.Add(topmostMenuItem);
            
            var exitMenuItem = new MenuItem { Header = "終了" };
            exitMenuItem.Click += (s, e) => Application.Current.Shutdown();
            
            contextMenu.Items.Add(backgroundMenuItem);
            contextMenu.Items.Add(foregroundMenuItem);
            contextMenu.Items.Add(highlightMenuItem);
            contextMenu.Items.Add(viewMenuItem);
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(exitMenuItem);
            ContextMenu = contextMenu;
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
        }
        
        private void SetHighlightColor(Color color)
        {
            _activeBrush = new SolidColorBrush(color);
        }
        
        private void ToggleTopmost()
        {
            Topmost = !Topmost;
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
            
            // マウス入力
            UpdateKeyStateByName("MouseLeft", VK_LBUTTON);
            UpdateKeyStateByName("MouseRight", VK_RBUTTON);
            UpdateKeyStateByName("MouseWheelButton", VK_MBUTTON);
            UpdateKeyStateByName("MouseButton4", VK_XBUTTON2); // 上のボタンはXBUTTON2
            UpdateKeyStateByName("MouseButton5", VK_XBUTTON1); // 下のボタンはXBUTTON1
            
            // スクロール表示の更新
            UpdateScrollIndicators();
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
                textBlock.Text = isShiftPressed ? shiftText : normalText;
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
            if (e.Delta > 0)
            {
                // 上スクロール
                _scrollUpTimer = 10; // 10フレーム表示
            }
            else if (e.Delta < 0)
            {
                // 下スクロール
                _scrollDownTimer = 10; // 10フレーム表示
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
                    scrollUpIndicator.Foreground = _scrollBrush;
                    _scrollUpTimer--;
                }
                else
                {
                    scrollUpIndicator.Foreground = Brushes.Transparent;
                }
            }
            
            // スクロールダウン表示
            var scrollDownIndicator = FindName("ScrollDownIndicator") as TextBlock;
            if (scrollDownIndicator != null)
            {
                if (_scrollDownTimer > 0)
                {
                    scrollDownIndicator.Foreground = _scrollBrush;
                    _scrollDownTimer--;
                }
                else
                {
                    scrollDownIndicator.Foreground = Brushes.Transparent;
                }
            }
        }
    }
}