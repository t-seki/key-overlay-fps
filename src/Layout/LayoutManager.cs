using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using YamlDotNet.Serialization;
using KeyOverlayFPS.Input;
using KeyOverlayFPS.Constants;
using KeyOverlayFPS.Utils;

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
        /// 現在のレイアウト設定
        /// </summary>
        public LayoutConfig? CurrentLayout
        {
            get => _currentLayout;
            private set => _currentLayout = value;
        }

        /// <summary>
        /// プロファイルに応じたレイアウトを読み込み
        /// 外部ファイル → 埋め込みリソース → デフォルトの順で試行（カスタマイズ優先）
        /// </summary>
        /// <param name="profile">キーボードプロファイル</param>
        public void LoadLayout(KeyboardProfile profile)
        {
            // 1. 外部ファイルから読み込みを試行（カスタマイズ優先）
            string layoutPath = GetLayoutPath(profile);
            if (File.Exists(layoutPath))
            {
                try
                {
                    CurrentLayout = ImportLayout(layoutPath);
                    Logger.Info($"外部ファイルからレイアウトを読み込み: {layoutPath}");
                    return;
                }
                catch (Exception ex)
                {
                    Logger.Warning($"外部ファイルからの読み込みに失敗: {ex.Message}");
                }
            }

            // 2. 埋め込みリソースから読み込みを試行（フォールバック）
            try
            {
                CurrentLayout = LoadEmbeddedLayout(profile);
                Logger.Info($"埋め込みリソースからレイアウトを読み込み: {profile}");
                return;
            }
            catch (Exception ex)
            {
                Logger.Warning($"埋め込みリソースからの読み込みに失敗: {ex.Message}");
            }

            // 3. フォールバック: デフォルトレイアウトを作成
            try
            {
                CurrentLayout = CreateDefaultLayout(profile);
                Logger.Warning($"デフォルトレイアウトを使用: {profile}");
                return;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"レイアウトの読み込みに完全に失敗しました: {ex.Message}", ex);
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
        /// 埋め込みリソースからレイアウトを読み込み
        /// </summary>
        /// <param name="profile">キーボードプロファイル</param>
        /// <returns>レイアウト設定</returns>
        private LayoutConfig LoadEmbeddedLayout(KeyboardProfile profile)
        {
            string resourceName = GetEmbeddedResourceName(profile);
            var assembly = Assembly.GetExecutingAssembly();
            
            Logger.Info($"埋め込みリソースを読み込み中: {resourceName}");
            
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                var availableResources = string.Join(", ", assembly.GetManifestResourceNames());
                throw new FileNotFoundException(
                    $"埋め込みリソースが見つかりません: {resourceName}\n" +
                    $"利用可能なリソース: {availableResources}");
            }

            using var reader = new StreamReader(stream);
            var yaml = reader.ReadToEnd();
            var layout = Deserializer.Deserialize<LayoutConfig>(yaml);
            
            // 設定値の検証
            ValidateLayout(layout);
            
            return layout;
        }

        /// <summary>
        /// デフォルトレイアウトを作成
        /// </summary>
        /// <param name="profile">キーボードプロファイル</param>
        /// <returns>デフォルトレイアウト設定</returns>
        private LayoutConfig CreateDefaultLayout(KeyboardProfile profile)
        {
            Logger.Info($"デフォルトレイアウトを作成中: {profile}");
            
            return profile switch
            {
                KeyboardProfile.FPSKeyboard => CreateDefaultFpsLayout(),
                _ => CreateDefault65Layout()
            };
        }

        /// <summary>
        /// デフォルト65%キーボードレイアウトを作成
        /// </summary>
        private LayoutConfig CreateDefault65Layout()
        {
            return new LayoutConfig
            {
                Profile = new ProfileInfo { Name = "FullKeyboard65" },
                Global = new GlobalSettings
                {
                    FontSize = 14,
                    BackgroundColor = "Transparent",
                    ForegroundColor = "White",
                    HighlightColor = "Green",
                    KeySize = new SizeConfig { Width = 50, Height = 50 },
                    ShiftDisplayEnabled = true
                },
                Window = new WindowSettings
                {
                    Width = 1050,
                    Height = 300,
                    WidthWithoutMouse = 750
                },
                Keys = CreateDefaultKeyMappings(),
                Mouse = new MouseSettings
                {
                    Position = new PositionConfig { X = 800, Y = 50 },
                    DirectionCanvas = new MouseDirectionCanvasConfig
                    {
                        Offset = new PositionConfig { X = 0, Y = 70 },
                        Size = new SizeConfig { Width = 200, Height = 150 },
                        Visualization = new DirectionVisualizationConfig
                        {
                            Threshold = 5.0,
                            HighlightDuration = 500
                        }
                    }
                }
            };
        }

        /// <summary>
        /// デフォルトFPSレイアウトを作成
        /// </summary>
        private LayoutConfig CreateDefaultFpsLayout()
        {
            var layout = CreateDefault65Layout();
            layout.Profile!.Name = "FPSKeyboard";
            layout.Window!.Width = 400;
            layout.Window.WidthWithoutMouse = 350;
            
            // FPS向けに主要キーのみを表示
            var fpsKeys = new[] { "W", "A", "S", "D", "Space", "Shift", "Ctrl", "Tab", "R", "F", "G", "C" };
            if (layout.Keys != null)
            {
                foreach (var key in layout.Keys.Keys.ToList())
                {
                    if (!fpsKeys.Contains(key))
                    {
                        layout.Keys[key].IsVisible = false;
                    }
                }
            }
            
            return layout;
        }

        /// <summary>
        /// デフォルトキーマッピングを作成
        /// </summary>
        private Dictionary<string, KeyDefinition> CreateDefaultKeyMappings()
        {
            var keys = new Dictionary<string, KeyDefinition>();
            
            // 基本的なキーマッピングを定義（簡略版）
            var basicKeys = new[]
            {
                ("W", 150, 50, 87), ("A", 100, 100, 65), ("S", 150, 100, 83), ("D", 200, 100, 68),
                ("Space", 250, 150, 32), ("Shift", 50, 150, 160), ("Ctrl", 50, 200, 162),
                ("Tab", 50, 50, 9), ("R", 250, 50, 82), ("F", 300, 100, 70),
                ("G", 350, 100, 71), ("C", 200, 150, 67)
            };

            foreach (var (key, x, y, virtualKey) in basicKeys)
            {
                keys[key] = new KeyDefinition
                {
                    Position = new PositionConfig { X = x, Y = y },
                    Text = key,
                    VirtualKey = virtualKey,
                    IsVisible = true
                };
            }

            return keys;
        }

        /// <summary>
        /// 埋め込みリソース名を取得
        /// </summary>
        private static string GetEmbeddedResourceName(KeyboardProfile profile)
        {
            return profile switch
            {
                KeyboardProfile.FPSKeyboard => "layouts/fps_keyboard.yaml",
                _ => "layouts/65_keyboard.yaml"
            };
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

        private static readonly ISerializer Serializer = YamlSerializerFactory.CreateLayoutSerializer();
        private static readonly IDeserializer Deserializer = YamlSerializerFactory.CreateLayoutDeserializer();

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
                if (layout.Mouse.DirectionCanvas?.Visualization != null)
                {
                    if (layout.Mouse.DirectionCanvas.Visualization.Threshold < 0)
                        throw new InvalidOperationException("マウス移動感度は0以上である必要があります");

                    if (layout.Mouse.DirectionCanvas.Visualization.HighlightDuration < 0)
                        throw new InvalidOperationException("ハイライト継続時間は0以上である必要があります");
                }
            }
        }
    }
}