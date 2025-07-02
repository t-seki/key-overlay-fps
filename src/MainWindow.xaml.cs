using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using KeyOverlayFPS.MouseVisualization;
using KeyOverlayFPS.Settings;
using KeyOverlayFPS.Colors;
using KeyOverlayFPS.Layout;
using KeyOverlayFPS.Constants;
using KeyOverlayFPS.UI;
using KeyOverlayFPS.Utils;
using KeyOverlayFPS.Initialization;

namespace KeyOverlayFPS
{


    public partial class MainWindow : Window
    {
        
        // 設定管理
        public MainWindowSettings Settings { get; }
        
        // メニュー管理
        public MainWindowMenu Menu { get; }
        
        // 入力処理管理
        public MainWindowInput Input { get; }
        
        // UI管理クラス
        private WindowDragHandler? _dragHandler;
        private MouseElementManager? _mouseElementManager;
        private ProfileSwitcher? _profileSwitcher;
        
        // 設定ハンドラークラス
        private ColorSettingsHandler? _colorHandler;
        private ScaleSettingsHandler? _scaleHandler;
        private WindowSettingsHandler? _windowHandler;

        // プロファイル管理
        public ProfileManager ProfileManager { get; }
        
        // 設定管理
        private readonly SettingsManager _settingsManager;
        
        // 動的レイアウトシステム
        public LayoutManager LayoutManager { get; }
        public UIElementLocator ElementLocator { get; }
        public MouseDirectionVisualizer MouseVisualizer { get; }

        // others
        public MouseTracker MouseTracker { get; }   

        public MainWindow()
        {
            InitializeComponent();
            
            LayoutManager = new LayoutManager();
            ElementLocator = new UIElementLocator();
            MouseVisualizer = new MouseDirectionVisualizer(ElementLocator);
            _settingsManager = new SettingsManager();
            ProfileManager = new ProfileManager(_settingsManager);
            Settings = new MainWindowSettings(this, _settingsManager);
            Menu = new MainWindowMenu(this, Settings, ProfileManager);
            MouseTracker = new MouseTracker();
            var keyboardKeyBackgroundBrush = BrushFactory.CreateKeyboardKeyBackground();
            Input = new MainWindowInput(this, Settings, MouseTracker, ElementLocator, LayoutManager, keyboardKeyBackgroundBrush);

            // 設定ハンドラークラスを初期化
            _colorHandler = new ColorSettingsHandler(Settings, this);
            _scaleHandler = new ScaleSettingsHandler(Settings, LayoutManager, this);
            _windowHandler = new WindowSettingsHandler(Settings, ProfileManager);

            try
            {
                var initializer = new WindowInitializer();
                initializer.Initialize(this);

                // ウィンドウ終了時のリソース解放設定
                this.Closed += MainWindow_Closed;
            }
            catch (Exception ex)
            {
                Logger.Error("MainWindow 初期化でエラーが発生", ex);
                throw;
            }
        }
        
        /// <summary>
        /// UI管理クラスを初期化
        /// </summary>
        internal void InitializeUIManagers()
        {
            _dragHandler = new WindowDragHandler(this);
            _mouseElementManager = new MouseElementManager(LayoutManager, ElementLocator!);
            
            // CanvasRebuilderを作成
            var canvasRebuilder = new CanvasRebuilder();
            
            _profileSwitcher = new ProfileSwitcher(
                ProfileManager,
                _settingsManager,
                canvasRebuilder,
                this,
                UpdateMousePositions,
                () => Menu.UpdateMenuCheckedState()
            );
            
            // WindowHandlerにProfileSwitcherを設定
            _windowHandler!.ProfileSwitcher = _profileSwitcher;
        }
        
        
                
        private void SetBackgroundColor(Color color, bool transparent)
        {
            _colorHandler?.SetBackgroundColor(color, transparent);
        }
        
        private void SetForegroundColor(Color color)
        {
            _colorHandler?.SetForegroundColor(color);
        }
        
        private void SetHighlightColor(Color color)
        {
            _colorHandler?.SetHighlightColor(color);
        }
        
        private void ToggleTopmost()
        {
            _windowHandler?.ToggleTopmost();
        }
        
        private void ToggleMouseVisibility()
        {
            _windowHandler?.ToggleMouseVisibility();
        }
        
        private void SetDisplayScale(double scale)
        {
            _scaleHandler?.SetDisplayScale(scale);
        }
        
        internal void ApplyDisplayScale()
        {
            _scaleHandler?.ApplyDisplayScale();
        }
        
        private void SwitchProfile(KeyboardProfile profile)
        {
            _windowHandler?.SwitchProfile(profile);
        }
        
        
        
        internal void UpdateMousePositions()
        {
            _mouseElementManager?.UpdateMousePositions();
        }

        internal void UpdateAllTextForeground()
        {
            _colorHandler?.UpdateAllTextForeground();
        }

        internal void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragHandler?.StartDrag(e);
        }

        internal void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            _dragHandler?.HandleMouseMove(e);
        }

        internal void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _dragHandler?.EndDrag();
        }

        



        internal void MainWindow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ContextMenu != null)
            {
                ContextMenu.IsOpen = true;
            }
        }


        
        
        /// <summary>
        /// メニューアクションを初期化
        /// </summary>
        internal void InitializeMenuActions(MainWindowMenu menu)
        {
            menu.SetBackgroundColorAction = SetBackgroundColor;
            menu.SetForegroundColorAction = SetForegroundColor;
            menu.SetHighlightColorAction = SetHighlightColor;
            menu.ToggleTopmostAction = ToggleTopmost;
            menu.ToggleMouseVisibilityAction = ToggleMouseVisibility;
            menu.SetDisplayScaleAction = SetDisplayScale;
            menu.SwitchProfileAction = SwitchProfile;
        }
        
        
        
        /// <summary>
        /// 設定変更時のイベントハンドラー
        /// </summary>
        internal void OnSettingsChanged(object? sender, EventArgs e)
        {
            // メニューのチェック状態を更新
            Menu.UpdateMenuCheckedState();
        }
        
        /// <summary>
        /// ウィンドウ終了時のリソース解放処理
        /// </summary>
        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            try
            {
                // 入力処理のリソースを解放
                Input?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error("MainWindow 終了時のリソース解放でエラーが発生", ex);
            }
        }
    }
}
