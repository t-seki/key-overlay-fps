using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using KeyOverlayFPS.Colors;
using KeyOverlayFPS.Constants;
using KeyOverlayFPS.Layout;

namespace KeyOverlayFPS.UI
{
    /// <summary>
    /// MainWindowのコンテキストメニュー管理責務を担当するクラス
    /// </summary>
    public class MainWindowMenu
    {
        private readonly Window _window;
        private readonly MainWindowSettings _settings;
        private readonly ProfileManager _profileManager;
        
        // メニューアイテムの参照を保持
        private MenuItem? _topmostMenuItem;
        private MenuItem? _mouseVisibilityMenuItem;
        private MenuItem? _fullKeyboardMenuItem;
        private MenuItem? _fpsKeyboardMenuItem;
        private MenuItem[]? _scaleMenuItems;
        private MenuItem[]? _backgroundColorMenuItems;
        private MenuItem[]? _foregroundColorMenuItems;
        private MenuItem[]? _highlightColorMenuItems;

        /// <summary>
        /// メニューアクション
        /// </summary>
        public Action<Color, bool>? SetBackgroundColorAction { get; set; }
        public Action<Color>? SetForegroundColorAction { get; set; }
        public Action<Color>? SetHighlightColorAction { get; set; }
        public Action? ToggleTopmostAction { get; set; }
        public Action? ToggleMouseVisibilityAction { get; set; }
        public Action<double>? SetDisplayScaleAction { get; set; }
        public Action<KeyboardProfile>? SwitchProfileAction { get; set; }

        public MainWindowMenu(Window window, MainWindowSettings settings, ProfileManager profileManager)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _profileManager = profileManager ?? throw new ArgumentNullException(nameof(profileManager));
        }

        /// <summary>
        /// コンテキストメニューを設定
        /// </summary>
        public void SetupContextMenu()
        {
            var contextMenu = new ContextMenu();
            
            // 背景色メニュー
            contextMenu.Items.Add(CreateBackgroundColorMenu());
            
            // 前景色メニュー
            contextMenu.Items.Add(CreateForegroundColorMenu());
            
            // ハイライト色メニュー
            contextMenu.Items.Add(CreateHighlightColorMenu());
            
            contextMenu.Items.Add(new Separator());
            
            // 表示オプションメニュー
            contextMenu.Items.Add(CreateViewOptionsMenu());
            
            contextMenu.Items.Add(new Separator());
            
            // プロファイルメニュー
            contextMenu.Items.Add(CreateProfileMenu());
            
            contextMenu.Items.Add(new Separator());
            
            // 終了メニュー
            contextMenu.Items.Add(CreateExitMenu());
            
            _window.ContextMenu = contextMenu;
        }

        /// <summary>
        /// 背景色メニューを作成
        /// </summary>
        private MenuItem CreateBackgroundColorMenu()
        {
            var parentMenuItem = new MenuItem { Header = "背景色" };
            var options = SimpleColorManager.BackgroundMenuOptions;
            _backgroundColorMenuItems = new MenuItem[options.Length];
            
            for (int i = 0; i < options.Length; i++)
            {
                var (name, color, transparent) = options[i];
                var menuItem = new MenuItem { Header = name, IsCheckable = true };
                menuItem.Click += (s, e) =>
                {
                    SetBackgroundColorAction?.Invoke(color, transparent);
                    UpdateMenuCheckedState();
                };
                _backgroundColorMenuItems[i] = menuItem;
                parentMenuItem.Items.Add(menuItem);
            }
            UpdateColorMenuCheckedState(_backgroundColorMenuItems, SimpleColorManager.FindBackgroundIndex(_settings.BackgroundColorSetting));
            
            return parentMenuItem;
        }

        /// <summary>
        /// 前景色メニューを作成
        /// </summary>
        private MenuItem CreateForegroundColorMenu()
        {
            var parentMenuItem = new MenuItem { Header = "文字色" };
            var options = SimpleColorManager.ForegroundMenuOptions;
            _foregroundColorMenuItems = new MenuItem[options.Length];
            
            for (int i = 0; i < options.Length; i++)
            {
                var (name, color) = options[i];
                var menuItem = new MenuItem { Header = name, IsCheckable = true };
                menuItem.Click += (s, e) =>
                {
                    SetForegroundColorAction?.Invoke(color);
                    UpdateMenuCheckedState();
                };
                _foregroundColorMenuItems[i] = menuItem;
                parentMenuItem.Items.Add(menuItem);
            }
            UpdateColorMenuCheckedState(_foregroundColorMenuItems, SimpleColorManager.FindForegroundIndex(_settings.ForegroundColorSetting));
            
            return parentMenuItem;
        }

        /// <summary>
        /// ハイライト色メニューを作成
        /// </summary>
        private MenuItem CreateHighlightColorMenu()
        {
            var parentMenuItem = new MenuItem { Header = "ハイライト色" };
            var options = SimpleColorManager.HighlightMenuOptions;
            _highlightColorMenuItems = new MenuItem[options.Length];
            
            for (int i = 0; i < options.Length; i++)
            {
                var (name, color) = options[i];
                var menuItem = new MenuItem { Header = name, IsCheckable = true };
                menuItem.Click += (s, e) =>
                {
                    SetHighlightColorAction?.Invoke(color);
                    UpdateMenuCheckedState();
                };
                _highlightColorMenuItems[i] = menuItem;
                parentMenuItem.Items.Add(menuItem);
            }
            UpdateColorMenuCheckedState(_highlightColorMenuItems, SimpleColorManager.FindHighlightIndex(_settings.HighlightColorSetting));
            
            return parentMenuItem;
        }

        /// <summary>
        /// 色メニューのチェックを、一致した位置（-1 なら全て外す）だけに付ける
        /// </summary>
        private static void UpdateColorMenuCheckedState(MenuItem[]? items, int checkedIndex)
        {
            if (items == null) return;
            for (int i = 0; i < items.Length; i++)
            {
                items[i].IsChecked = i == checkedIndex;
            }
        }

        /// <summary>
        /// 表示オプションメニューを作成
        /// </summary>
        private MenuItem CreateViewOptionsMenu()
        {
            var viewMenuItem = new MenuItem { Header = "表示オプション" };
            
            _topmostMenuItem = new MenuItem { Header = "最前面固定", IsCheckable = true, IsChecked = _window.Topmost };
            _topmostMenuItem.Click += (s, e) => ToggleTopmostAction?.Invoke();
            
            _mouseVisibilityMenuItem = new MenuItem { Header = "マウス表示", IsCheckable = true, IsChecked = _settings.IsMouseVisible };
            _mouseVisibilityMenuItem.Click += (s, e) => ToggleMouseVisibilityAction?.Invoke();
            
            var scaleMenuItem = CreateDisplayScaleMenu();
            
            viewMenuItem.Items.Add(_topmostMenuItem);
            viewMenuItem.Items.Add(_mouseVisibilityMenuItem);
            viewMenuItem.Items.Add(scaleMenuItem);
            
            return viewMenuItem;
        }

        /// <summary>
        /// 表示スケールメニューを作成
        /// </summary>
        private MenuItem CreateDisplayScaleMenu()
        {
            var scaleMenuItem = new MenuItem { Header = "表示スケール" };
            
            // 参照保持用配列の初期化
            _scaleMenuItems = new MenuItem[ApplicationConstants.ScaleOptions.Values.Length];
            
            for (int i = 0; i < ApplicationConstants.ScaleOptions.Values.Length; i++)
            {
                var scale = ApplicationConstants.ScaleOptions.Values[i];
                var label = ApplicationConstants.ScaleOptions.Labels[i];
                
                var menuItem = new MenuItem 
                { 
                    Header = label, 
                    IsCheckable = true, 
                    IsChecked = Math.Abs(_settings.DisplayScale - scale) < 0.01 
                };
                menuItem.Click += (s, e) => 
                {
                    SetDisplayScaleAction?.Invoke(scale);
                    UpdateMenuCheckedState();
                };
                
                // 参照を保持
                _scaleMenuItems[i] = menuItem;
                scaleMenuItem.Items.Add(menuItem);
            }
            
            return scaleMenuItem;
        }

        /// <summary>
        /// プロファイルメニューを作成
        /// </summary>
        private MenuItem CreateProfileMenu()
        {
            var profileMenuItem = new MenuItem { Header = "プロファイル" };
            
            // 65%キーボード
            _fullKeyboardMenuItem = new MenuItem 
            { 
                Header = "65%キーボード", 
                IsCheckable = true, 
                IsChecked = _profileManager.IsCurrentProfile(KeyboardProfile.FullKeyboard65) 
            };
            _fullKeyboardMenuItem.Click += (s, e) => 
            {
                SwitchProfileAction?.Invoke(KeyboardProfile.FullKeyboard65);
            };
            
            // FPSキーボード
            _fpsKeyboardMenuItem = new MenuItem 
            { 
                Header = "FPSキーボード", 
                IsCheckable = true, 
                IsChecked = _profileManager.IsCurrentProfile(KeyboardProfile.FPSKeyboard) 
            };
            _fpsKeyboardMenuItem.Click += (s, e) => 
            {
                SwitchProfileAction?.Invoke(KeyboardProfile.FPSKeyboard);
            };
            
            profileMenuItem.Items.Add(_fullKeyboardMenuItem);
            profileMenuItem.Items.Add(_fpsKeyboardMenuItem);
            
            return profileMenuItem;
        }

        /// <summary>
        /// 終了メニューを作成
        /// </summary>
        private MenuItem CreateExitMenu()
        {
            var exitMenuItem = new MenuItem { Header = "終了" };
            exitMenuItem.Click += (s, e) => Application.Current.Shutdown();
            return exitMenuItem;
        }

        /// <summary>
        /// メニューのチェック状態を更新
        /// </summary>
        public void UpdateMenuCheckedState()
        {
            // 直接参照で更新
            if (_topmostMenuItem != null)
            {
                _topmostMenuItem.IsChecked = _settings.IsTopmost;
            }
            
            if (_mouseVisibilityMenuItem != null)
            {
                _mouseVisibilityMenuItem.IsChecked = _settings.IsMouseVisible;
            }
            
            if (_fullKeyboardMenuItem != null)
            {
                _fullKeyboardMenuItem.IsChecked = _profileManager.IsCurrentProfile(KeyboardProfile.FullKeyboard65);
            }
            
            if (_fpsKeyboardMenuItem != null)
            {
                _fpsKeyboardMenuItem.IsChecked = _profileManager.IsCurrentProfile(KeyboardProfile.FPSKeyboard);
            }
            
            // 色メニューの更新
            UpdateColorMenuCheckedState(_backgroundColorMenuItems, SimpleColorManager.FindBackgroundIndex(_settings.BackgroundColorSetting));
            UpdateColorMenuCheckedState(_foregroundColorMenuItems, SimpleColorManager.FindForegroundIndex(_settings.ForegroundColorSetting));
            UpdateColorMenuCheckedState(_highlightColorMenuItems, SimpleColorManager.FindHighlightIndex(_settings.HighlightColorSetting));

            // スケールメニューの更新
            UpdateScaleMenuCheckedState();
        }

        /// <summary>
        /// スケールメニューのチェック状態を更新
        /// </summary>
        private void UpdateScaleMenuCheckedState()
        {
            if (_scaleMenuItems == null) return;
            
            for (int i = 0; i < _scaleMenuItems.Length; i++)
            {
                if (i < ApplicationConstants.ScaleOptions.Values.Length)
                {
                    var scale = ApplicationConstants.ScaleOptions.Values[i];
                    _scaleMenuItems[i].IsChecked = Math.Abs(_settings.DisplayScale - scale) < 0.01;
                }
            }
        }
    }
}