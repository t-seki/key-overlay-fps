using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using KeyOverlayFPS.Input;
using KeyOverlayFPS.Constants;

namespace KeyOverlayFPS.Layout
{
    /// <summary>
    /// キーボードプロファイル
    /// </summary>
    public enum KeyboardProfile
    {
        FullKeyboard65,  // 現在の65%キーボード
        FPSKeyboard      // FPS用コンパクトキーボード
    }

    /// <summary>
    /// レイアウト管理クラス
    /// </summary>
    public class LayoutManager
    {
        private LayoutConfig? _currentLayout;

        /// <summary>
        /// レイアウト変更イベント
        /// </summary>
        public event EventHandler<LayoutConfig?>? LayoutChanged;

        /// <summary>
        /// 現在のレイアウト設定
        /// </summary>
        public LayoutConfig? CurrentLayout
        {
            get => _currentLayout;
            private set
            {
                if (_currentLayout != value)
                {
                    _currentLayout = value;
                    LayoutChanged?.Invoke(this, value);
                }
            }
        }

        /// <summary>
        /// プロファイルに応じたレイアウトを読み込み
        /// </summary>
        /// <param name="profile">キーボードプロファイル</param>
        public void LoadLayout(KeyboardProfile profile)
        {
            string layoutPath = GetLayoutPath(profile);
            if (File.Exists(layoutPath))
            {
                CurrentLayout = ImportLayout(layoutPath);
            }
            else
            {
                throw new FileNotFoundException($"レイアウトファイルが見つかりません: {layoutPath}");
            }
        }

        /// <summary>
        /// マウス位置を取得
        /// </summary>
        /// <returns>マウス位置（左, 上）</returns>
        /// <exception cref="InvalidOperationException">マウス位置が定義されていない場合</exception>
        public (double Left, double Top) GetMousePosition()
        {
            if (CurrentLayout?.Mouse?.Position == null)
            {
                throw new InvalidOperationException(
                    "マウス位置が定義されていません。YAMLファイルにmouse.positionセクションを追加してください。"
                );
            }
            
            return (CurrentLayout.Mouse.Position.X, CurrentLayout.Mouse.Position.Y);
        }

        /// <summary>
        /// Shift表示設定を取得
        /// </summary>
        public bool IsShiftDisplayEnabled()
        {
            return CurrentLayout?.Global?.ShiftDisplayEnabled ?? true;
        }

        /// <summary>
        /// 表示可能なキーの名前リストを取得
        /// </summary>
        /// <returns>isVisible: true のキー名リスト</returns>
        public List<string> GetVisibleKeys()
        {
            if (CurrentLayout?.Keys == null)
                return new List<string>();

            return CurrentLayout.Keys
                .Where(kvp => kvp.Value.IsVisible)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        /// <summary>
        /// ウィンドウサイズを取得
        /// </summary>
        /// <param name="includeMouseWidth">マウス表示用の幅を含めるかどうか</param>
        /// <returns>ウィンドウサイズ（幅、高さ）</returns>
        /// <exception cref="InvalidOperationException">レイアウトが読み込まれていない場合</exception>
        public (double Width, double Height) GetWindowSize(bool includeMouseWidth = false)
        {
            if (CurrentLayout?.Window == null)
            {
                throw new InvalidOperationException("レイアウトが読み込まれていません。YAMLファイルの読み込みに失敗した可能性があります。");
            }

            double width = includeMouseWidth ? CurrentLayout.Window.Width : CurrentLayout.Window.WidthWithoutMouse;
            double height = CurrentLayout.Window.Height;

            return (width, height);
        }

        /// <summary>
        /// プロファイルに応じたレイアウトファイルパスを取得
        /// </summary>
        private static string GetLayoutPath(KeyboardProfile profile)
        {
            return profile switch
            {
                KeyboardProfile.FPSKeyboard => ApplicationConstants.Paths.FpsLayout,
                _ => ApplicationConstants.Paths.Keyboard65Layout
            };
        }

        private static readonly ISerializer Serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        private static readonly IDeserializer Deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeConverter(new VirtualKeyCodeConverter())
            .Build();

        /// <summary>
        /// レイアウトファイルをエクスポート
        /// </summary>
        /// <param name="layout">レイアウト設定</param>
        /// <param name="filePath">保存先ファイルパス</param>
        public static void ExportLayout(LayoutConfig layout, string filePath)
        {
            try
            {
                // ディレクトリが存在しない場合は作成
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var yaml = Serializer.Serialize(layout);
                File.WriteAllText(filePath, yaml);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"レイアウトファイルのエクスポートに失敗しました: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// レイアウトファイルをインポート
        /// </summary>
        /// <param name="filePath">読み込み元ファイルパス</param>
        /// <returns>レイアウト設定</returns>
        public LayoutConfig ImportLayout(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"レイアウトファイルが見つかりません: {filePath}");
                }

                var yaml = File.ReadAllText(filePath);
                var layout = Deserializer.Deserialize<LayoutConfig>(yaml);
                
                // 設定値の検証
                ValidateLayout(layout);
                
                return layout;
            }
            catch (Exception ex) when (!(ex is FileNotFoundException))
            {
                throw new InvalidOperationException($"レイアウトファイルのインポートに失敗しました: {ex.Message}", ex);
            }
        }


        /// <summary>
        /// レイアウト設定の検証
        /// </summary>
        private static void ValidateLayout(LayoutConfig layout)
        {
            if (layout == null)
                throw new ArgumentNullException(nameof(layout));

            if (layout.Global == null)
                throw new InvalidOperationException("グローバル設定が存在しません");

            if (layout.Keys == null)
                throw new InvalidOperationException("キー設定が存在しません");

            if (layout.Window == null)
                throw new InvalidOperationException("ウィンドウ設定が存在しません");

            // 基本的な数値範囲チェック
            if (layout.Global.FontSize <= 0)
                throw new InvalidOperationException("フォントサイズは1以上である必要があります");

            if (layout.Global.KeySize.Width <= 0 || layout.Global.KeySize.Height <= 0)
                throw new InvalidOperationException("キーサイズは0より大きい値である必要があります");

            if (layout.Window.Width <= 0 || layout.Window.Height <= 0)
                throw new InvalidOperationException("ウィンドウサイズは0より大きい値である必要があります");

            // マウス設定の検証
            if (layout.Mouse != null)
            {
                if (layout.Mouse.Movement != null)
                {
                    if (layout.Mouse.Movement.Threshold < 0)
                        throw new InvalidOperationException("マウス移動感度は0以上である必要があります");

                    if (layout.Mouse.Movement.HighlightDuration < 0)
                        throw new InvalidOperationException("ハイライト継続時間は0以上である必要があります");
                }
            }
        }
    }
}