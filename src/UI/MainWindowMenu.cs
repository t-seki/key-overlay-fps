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
            
            var topmostMenuItem = new MenuItem { Header = "最前面固定", IsCheckable = true, IsChecked = _window.Topmost };
            topmostMenuItem.Click += (s, e) => ToggleTopmostAction?.Invoke();
            
            var mouseVisibilityMenuItem = new MenuItem { Header = "マウス表示", IsCheckable = true, IsChecked = _settings.IsMouseVisible };
            mouseVisibilityMenuItem.Click += (s, e) => ToggleMouseVisibilityAction?.Invoke();
            
            var scaleMenuItem = CreateDisplayScaleMenu();
            
            viewMenuItem.Items.Add(topmostMenuItem);
            viewMenuItem.Items.Add(mouseVisibilityMenuItem);
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
            var fullKeyboardMenuItem = new MenuItem 
            { 
                Header = "65%キーボード", 
                IsCheckable = true, 
                IsChecked = _keyboardHandler.CurrentProfile == KeyboardProfile.FullKeyboard65 
            };
            fullKeyboardMenuItem.Click += (s, e) => 
            {
                SwitchProfileAction?.Invoke(KeyboardProfile.FullKeyboard65);
                UpdateMenuCheckedState();
            };
            
            // FPSキーボード
            var fpsKeyboardMenuItem = new MenuItem 
            { 
                Header = "FPSキーボード", 
                IsCheckable = true, 
                IsChecked = _keyboardHandler.CurrentProfile == KeyboardProfile.FPSKeyboard 
            };
            fpsKeyboardMenuItem.Click += (s, e) => 
            {
                SwitchProfileAction?.Invoke(KeyboardProfile.FPSKeyboard);
                UpdateMenuCheckedState();
            };
            
            profileMenuItem.Items.Add(fullKeyboardMenuItem);
            profileMenuItem.Items.Add(fpsKeyboardMenuItem);
            
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
            if (_window.ContextMenu == null) return;
            
            foreach (MenuItem mainItem in _window.ContextMenu.Items)
            {
                if (mainItem.Header?.ToString() == "表示オプション")
                {
                    UpdateViewOptionsCheckedState(mainItem);
                }
                else if (mainItem.Header?.ToString() == "プロファイル")
                {
                    UpdateProfileCheckedState(mainItem);
                }
            }
        }

        /// <summary>
        /// 表示オプションのチェック状態を更新
        /// </summary>
        private void UpdateViewOptionsCheckedState(MenuItem viewMenuItem)
        {
            foreach (MenuItem item in viewMenuItem.Items)
            {
                switch (item.Header?.ToString())
                {
                    case "最前面固定":
                        item.IsChecked = _window.Topmost;
                        break;
                    case "マウス表示":
                        item.IsChecked = _settings.IsMouseVisible;
                        break;
                    case "表示スケール":
                        UpdateScaleCheckedState(item);
                        break;
                }
            }
        }

        /// <summary>
        /// スケールのチェック状態を更新
        /// </summary>
        private void UpdateScaleCheckedState(MenuItem scaleMenuItem)
        {
            int index = 0;
            foreach (MenuItem item in scaleMenuItem.Items)
            {
                if (index < ApplicationConstants.ScaleOptions.Values.Length)
                {
                    var scale = ApplicationConstants.ScaleOptions.Values[index];
                    item.IsChecked = Math.Abs(_settings.DisplayScale - scale) < 0.01;
                }
                index++;
            }
        }

        /// <summary>
        /// プロファイルのチェック状態を更新
        /// </summary>
        private void UpdateProfileCheckedState(MenuItem profileMenuItem)
        {
            foreach (MenuItem item in profileMenuItem.Items)
            {
                switch (item.Header?.ToString())
                {
                    case "65%キーボード":
                        item.IsChecked = _keyboardHandler.CurrentProfile == KeyboardProfile.FullKeyboard65;
                        break;
                    case "FPSキーボード":
                        item.IsChecked = _keyboardHandler.CurrentProfile == KeyboardProfile.FPSKeyboard;
                        break;
                }
            }
        }
    }
}