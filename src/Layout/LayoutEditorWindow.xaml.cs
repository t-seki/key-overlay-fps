using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace KeyOverlayFPS.Layout
{
    /// <summary>
    /// レイアウト編集ウィンドウ
    /// </summary>
    public partial class LayoutEditorWindow : Window
    {
        private LayoutConfig _currentLayout;
        private string? _currentFilePath;
        private bool _isModified = false;
        private string? _selectedElementKey;

        public LayoutConfig? Result { get; private set; }

        public LayoutEditorWindow()
        {
            InitializeComponent();
            _currentLayout = LayoutManager.CreateDefault65KeyboardLayout();
            LoadLayoutToUI();
            RefreshPreview();
        }

        public LayoutEditorWindow(LayoutConfig layout) : this()
        {
            _currentLayout = layout;
            LoadLayoutToUI();
            RefreshPreview();
        }

        #region Menu Events

        private void NewLayout_Click(object sender, RoutedEventArgs e)
        {
            if (CheckUnsavedChanges())
            {
                _currentLayout = LayoutManager.CreateDefault65KeyboardLayout();
                _currentFilePath = null;
                _isModified = false;
                LoadLayoutToUI();
                RefreshPreview();
                UpdateTitle();
            }
        }

        private void OpenLayout_Click(object sender, RoutedEventArgs e)
        {
            if (CheckUnsavedChanges())
            {
                var openDialog = new OpenFileDialog
                {
                    Filter = "YAML files (*.yaml)|*.yaml|All files (*.*)|*.*",
                    DefaultExt = "yaml"
                };

                if (openDialog.ShowDialog() == true)
                {
                    try
                    {
                        _currentLayout = LayoutManager.ImportLayout(openDialog.FileName);
                        _currentFilePath = openDialog.FileName;
                        _isModified = false;
                        LoadLayoutToUI();
                        RefreshPreview();
                        UpdateTitle();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"ファイルの読み込みに失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void SaveLayout_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                SaveAsLayout_Click(sender, e);
            }
            else
            {
                SaveCurrentLayout();
            }
        }

        private void SaveAsLayout_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "YAML files (*.yaml)|*.yaml|All files (*.*)|*.*",
                DefaultExt = "yaml"
            };

            if (saveDialog.ShowDialog() == true)
            {
                _currentFilePath = saveDialog.FileName;
                SaveCurrentLayout();
            }
        }

        private void LoadDefault65_Click(object sender, RoutedEventArgs e)
        {
            if (CheckUnsavedChanges())
            {
                _currentLayout = LayoutManager.CreateDefault65KeyboardLayout();
                _isModified = true;
                LoadLayoutToUI();
                RefreshPreview();
                UpdateTitle();
            }
        }

        private void LoadDefaultFPS_Click(object sender, RoutedEventArgs e)
        {
            if (CheckUnsavedChanges())
            {
                _currentLayout = LayoutManager.CreateDefaultFPSLayout();
                _isModified = true;
                LoadLayoutToUI();
                RefreshPreview();
                UpdateTitle();
            }
        }

        private void RefreshPreview_Click(object sender, RoutedEventArgs e)
        {
            RefreshPreview();
        }

        #endregion

        #region Setting Change Events

        private void GlobalSetting_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;

            UpdateLayoutFromUI();
            RefreshPreview();
            SetModified();
        }

        private void ElementsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ElementsListBox.SelectedItem is string selectedKey)
            {
                _selectedElementKey = selectedKey;
                LoadSelectedElementToUI();
                ElementSettingsGroup.IsEnabled = true;
            }
            else
            {
                _selectedElementKey = null;
                ElementSettingsGroup.IsEnabled = false;
            }
        }

        private void ElementSetting_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded || string.IsNullOrEmpty(_selectedElementKey)) return;

            UpdateSelectedElementFromUI();
            RefreshPreview();
            SetModified();
        }

        #endregion

        #region Element Management (Removed - Keyboard Keys Only)

        // エレメント管理機能は削除済み（キーボードキーの編集のみ対応）

        #endregion

        #region Dialog Events

        private void ApplyLayout_Click(object sender, RoutedEventArgs e)
        {
            UpdateLayoutFromUI();
            Result = _currentLayout;
        }

        private void CancelLayout_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OKLayout_Click(object sender, RoutedEventArgs e)
        {
            UpdateLayoutFromUI();
            Result = _currentLayout;
            DialogResult = true;
            Close();
        }

        #endregion

        #region UI Update Methods

        private void LoadLayoutToUI()
        {
            // キーボードレイアウト設定をUIに反映
            FontSizeTextBox.Text = _currentLayout.Global.FontSize.ToString();
            FontFamilyTextBox.Text = _currentLayout.Global.FontFamily ?? "";
            KeyWidthTextBox.Text = _currentLayout.Global.KeySize.Width.ToString();
            KeyHeightTextBox.Text = _currentLayout.Global.KeySize.Height.ToString();
            BackgroundColorTextBox.Text = _currentLayout.Global.BackgroundColor ?? "";
            HighlightColorTextBox.Text = _currentLayout.Global.HighlightColor ?? "";

            // キーボードキーリストを更新
            RefreshElementsList();
        }

        private void RefreshElementsList()
        {
            ElementsListBox.Items.Clear();
            
            // キーボードキーのみをフィルタリング（マウス関連を除外）
            var keyboardKeys = _currentLayout.Elements.Keys
                .Where(key => !IsMouseRelatedElement(key))
                .OrderBy(key => key);
                
            foreach (var key in keyboardKeys)
            {
                ElementsListBox.Items.Add(key);
            }
        }
        
        /// <summary>
        /// マウス関連要素かどうかを判定
        /// </summary>
        private static bool IsMouseRelatedElement(string elementName)
        {
            var mouseElementPrefixes = new[] { "Mouse", "Scroll" };
            return mouseElementPrefixes.Any(prefix => elementName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        }

        private void LoadSelectedElementToUI()
        {
            if (string.IsNullOrEmpty(_selectedElementKey) || !_currentLayout.Elements.TryGetValue(_selectedElementKey, out var element))
                return;

            ElementNameTextBox.Text = _selectedElementKey;
            ElementXTextBox.Text = element.X.ToString();
            ElementYTextBox.Text = element.Y.ToString();
            ElementTextTextBox.Text = element.Text ?? "";
            ElementWidthTextBox.Text = element.Size?.Width.ToString() ?? "";
            ElementHeightTextBox.Text = element.Size?.Height.ToString() ?? "";
            ElementFontSizeTextBox.Text = element.FontSize?.ToString() ?? "";
            ElementVisibleCheckBox.IsChecked = element.IsVisible;
        }

        private void UpdateLayoutFromUI()
        {
            // キーボードレイアウト設定をレイアウトに反映
            if (double.TryParse(FontSizeTextBox.Text, out var fontSize))
                _currentLayout.Global.FontSize = (int)fontSize;
            if (double.TryParse(KeyWidthTextBox.Text, out var keyWidth))
                _currentLayout.Global.KeySize.Width = (int)keyWidth;
            if (double.TryParse(KeyHeightTextBox.Text, out var keyHeight))
                _currentLayout.Global.KeySize.Height = (int)keyHeight;

            _currentLayout.Global.FontFamily = FontFamilyTextBox.Text;
            _currentLayout.Global.BackgroundColor = BackgroundColorTextBox.Text;
            _currentLayout.Global.HighlightColor = HighlightColorTextBox.Text;
        }

        private void UpdateSelectedElementFromUI()
        {
            if (string.IsNullOrEmpty(_selectedElementKey) || !_currentLayout.Elements.TryGetValue(_selectedElementKey, out var element))
                return;

            if (double.TryParse(ElementXTextBox.Text, out var x))
                element.X = (int)x;
            if (double.TryParse(ElementYTextBox.Text, out var y))
                element.Y = (int)y;

            element.Text = ElementTextTextBox.Text;
            element.IsVisible = ElementVisibleCheckBox.IsChecked ?? true;

            // サイズ設定
            if (double.TryParse(ElementWidthTextBox.Text, out var width) && double.TryParse(ElementHeightTextBox.Text, out var height))
            {
                element.Size = new SizeConfig { Width = (int)width, Height = (int)height };
            }
            else if (string.IsNullOrWhiteSpace(ElementWidthTextBox.Text) && string.IsNullOrWhiteSpace(ElementHeightTextBox.Text))
            {
                element.Size = null;
            }

            // フォントサイズ設定
            if (double.TryParse(ElementFontSizeTextBox.Text, out var elementFontSize))
            {
                element.FontSize = (int)elementFontSize;
            }
            else if (string.IsNullOrWhiteSpace(ElementFontSizeTextBox.Text))
            {
                element.FontSize = null;
            }

            // エレメント名の変更処理
            var newName = ElementNameTextBox.Text;
            if (!string.IsNullOrEmpty(newName) && newName != _selectedElementKey && !_currentLayout.Elements.ContainsKey(newName))
            {
                _currentLayout.Elements.Remove(_selectedElementKey);
                _currentLayout.Elements[newName] = element;
                _selectedElementKey = newName;
                RefreshElementsList();
                ElementsListBox.SelectedItem = newName;
            }
        }

        #endregion

        #region Preview

        private void RefreshPreview()
        {
            PreviewCanvas.Children.Clear();
            PreviewCanvas.Width = _currentLayout.Global.WindowWidth;
            PreviewCanvas.Height = _currentLayout.Global.WindowHeight;

            // キーボードキーのみをプレビューに表示
            foreach (var (key, element) in _currentLayout.Elements)
            {
                if (!element.IsVisible || IsMouseRelatedElement(key)) continue;

                var border = CreatePreviewElement(key, element);
                PreviewCanvas.Children.Add(border);
            }
        }

        private Border CreatePreviewElement(string key, ElementConfig element)
        {
            var width = element.Size?.Width ?? _currentLayout.Global.KeySize.Width;
            var height = element.Size?.Height ?? _currentLayout.Global.KeySize.Height;
            var fontSize = element.FontSize ?? _currentLayout.Global.FontSize;

            var border = new Border
            {
                Width = width,
                Height = height,
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(1),
                Background = Brushes.DarkGray,
                CornerRadius = new CornerRadius(2)
            };

            var textBlock = new TextBlock
            {
                Text = element.Text ?? key,
                FontSize = fontSize,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            border.Child = textBlock;
            Canvas.SetLeft(border, element.X);
            Canvas.SetTop(border, element.Y);

            // 選択中のエレメントをハイライト
            if (key == _selectedElementKey)
            {
                border.BorderBrush = Brushes.Yellow;
                border.BorderThickness = new Thickness(2);
            }

            return border;
        }

        #endregion

        #region Helper Methods

        private bool CheckUnsavedChanges()
        {
            if (_isModified)
            {
                var result = MessageBox.Show("変更が保存されていません。続行しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
                return result == MessageBoxResult.Yes;
            }
            return true;
        }

        private void SaveCurrentLayout()
        {
            try
            {
                UpdateLayoutFromUI();
                LayoutManager.ExportLayout(_currentLayout, _currentFilePath!);
                _isModified = false;
                UpdateTitle();
                MessageBox.Show("保存しました。", "保存完了", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存に失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetModified()
        {
            _isModified = true;
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            var fileName = string.IsNullOrEmpty(_currentFilePath) ? "新規レイアウト" : System.IO.Path.GetFileName(_currentFilePath);
            Title = $"レイアウト管理 - {fileName}{(_isModified ? " *" : "")}";
        }

        #endregion
    }
}