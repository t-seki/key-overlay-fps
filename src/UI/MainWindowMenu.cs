using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using KeyOverlayFPS.Colors;
using KeyOverlayFPS.Constants;
using KeyOverlayFPS.Input;

namespace KeyOverlayFPS.UI
{
    /// <summary>
    /// MainWindowのコンテキストメニュー管理責務を担当するクラス
    /// </summary>
    public class MainWindowMenu
    {
        private readonly Window _window;
        private readonly MainWindowSettings _settings;
        private readonly KeyboardInputHandler _keyboardHandler;
        
        // メニューアイテムの参照を保持
        private MenuItem? _topmostMenuItem;
        private MenuItem? _mouseVisibilityMenuItem;
        private MenuItem? _fullKeyboardMenuItem;
        private MenuItem? _fpsKeyboardMenuItem;

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

        public MainWindowMenu(Window window, MainWindowSettings settings, KeyboardInputHandler keyboardHandler)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _keyboardHandler = keyboardHandler ?? throw new ArgumentNullException(nameof(keyboardHandler));
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
            var backgroundMenuItem = new MenuItem { Header = "背景色" };
            
            foreach (var (name, color, transparent) in SimpleColorManager.BackgroundMenuOptions)
            {
                var menuItem = new MenuItem { Header = name };
                menuItem.Click += (s, e) => SetBackgroundColorAction?.Invoke(color, transparent);
                backgroundMenuItem.Items.Add(menuItem);
            }
            
            return backgroundMenuItem;
        }

        /// <summary>
        /// 前景色メニューを作成
        /// </summary>
        private MenuItem CreateForegroundColorMenu()
        {
            var foregroundMenuItem = new MenuItem { Header = "文字色" };
            
            foreach (var (name, color) in SimpleColorManager.ForegroundMenuOptions)
            {
                var menuItem = new MenuItem { Header = name };
                menuItem.Click += (s, e) => SetForegroundColorAction?.Invoke(color);
                foregroundMenuItem.Items.Add(menuItem);
            }
            
            return foregroundMenuItem;
        }

        /// <summary>
        /// ハイライト色メニューを作成
        /// </summary>
        private MenuItem CreateHighlightColorMenu()
        {
            var highlightMenuItem = new MenuItem { Header = "ハイライト色" };
            
            foreach (var (name, color) in SimpleColorManager.HighlightMenuOptions)
            {
                var menuItem = new MenuItem { Header = name };
                menuItem.Click += (s, e) => SetHighlightColorAction?.Invoke(color);
                highlightMenuItem.Items.Add(menuItem);
            }
            
            return highlightMenuItem;
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
                IsChecked = _keyboardHandler.CurrentProfile == KeyboardProfile.FullKeyboard65 
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
                IsChecked = _keyboardHandler.CurrentProfile == KeyboardProfile.FPSKeyboard 
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
                _topmostMenuItem.IsChecked = _window.Topmost;
            }
            
            if (_mouseVisibilityMenuItem != null)
            {
                _mouseVisibilityMenuItem.IsChecked = _settings.IsMouseVisible;
            }
            
            if (_fullKeyboardMenuItem != null)
            {
                _fullKeyboardMenuItem.IsChecked = _keyboardHandler.CurrentProfile == KeyboardProfile.FullKeyboard65;
            }
            
            if (_fpsKeyboardMenuItem != null)
            {
                _fpsKeyboardMenuItem.IsChecked = _keyboardHandler.CurrentProfile == KeyboardProfile.FPSKeyboard;
            }
            
            // スケールメニューは複雑なので従来の方法で更新
            UpdateScaleMenuCheckedState();
        }

        /// <summary>
        /// スケールメニューのチェック状態を更新
        /// </summary>
        private void UpdateScaleMenuCheckedState()
        {
            if (_window.ContextMenu == null) return;
            
            foreach (var item in _window.ContextMenu.Items)
            {
                if (item is not MenuItem mainItem) continue;
                
                if (mainItem.Header?.ToString() == "表示オプション")
                {
                    foreach (var subItem in mainItem.Items)
                    {
                        if (subItem is not MenuItem menuItem) continue;
                        
                        if (menuItem.Header?.ToString() == "表示スケール")
                        {
                            int index = 0;
                            foreach (var scaleSubItem in menuItem.Items)
                            {
                                if (scaleSubItem is not MenuItem scaleItem) continue;
                                
                                if (index < ApplicationConstants.ScaleOptions.Values.Length)
                                {
                                    var scale = ApplicationConstants.ScaleOptions.Values[index];
                                    scaleItem.IsChecked = Math.Abs(_settings.DisplayScale - scale) < 0.01;
                                }
                                index++;
                            }
                            break;
                        }
                    }
                    break;
                }
            }
        }
    }
}