using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace KeyOverlayFPS.Layout
{
    /// <summary>
    /// レイアウト管理クラス
    /// </summary>
    public static class LayoutManager
    {
        private static readonly ISerializer Serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        private static readonly IDeserializer Deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
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
        public static LayoutConfig ImportLayout(string filePath)
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
        /// 現在の設定から65%キーボードレイアウトを作成
        /// </summary>
        public static LayoutConfig CreateDefault65KeyboardLayout()
        {
            var layout = new LayoutConfig();
            
            // プロファイル情報を設定
            layout.Profile = new ProfileInfo
            {
                Name = "65%キーボード",
                Description = "標準的な65%キーボードレイアウト - 全キー表示",
                Version = "1.0",
                Author = "KeyOverlayFPS",
                CreatedAt = DateTime.Now
            };

            // ウィンドウ設定
            layout.Window = new WindowSettings
            {
                Width = 580,
                Height = 160,
                BackgroundColor = "Transparent"
            };

            // マウス設定
            layout.Mouse = new MouseSettings
            {
                IsVisible = true,
                Position = new PositionConfig { X = 475, Y = 20 },
                Movement = new MouseMovementConfig
                {
                    CircleSize = 15,
                    CircleColor = "#FFFFFF",
                    HighlightColor = "#00FF00",
                    HighlightDuration = 100,
                    Threshold = 5.0
                },
                Buttons = new Dictionary<string, ButtonConfig>
                {
                    ["MouseLeft"] = new ButtonConfig
                    {
                        Offset = new PositionConfig { X = 3, Y = 3 },
                        Size = new SizeConfig { Width = 25, Height = 35 },
                        VirtualKey = 1,
                        IsVisible = true
                    },
                    ["MouseRight"] = new ButtonConfig
                    {
                        Offset = new PositionConfig { X = 32, Y = 3 },
                        Size = new SizeConfig { Width = 25, Height = 35 },
                        VirtualKey = 2,
                        IsVisible = true
                    },
                    ["MouseWheelButton"] = new ButtonConfig
                    {
                        Offset = new PositionConfig { X = 25, Y = 10 },
                        Size = new SizeConfig { Width = 10, Height = 22 },
                        VirtualKey = 3,
                        IsVisible = true
                    },
                    ["MouseButton4"] = new ButtonConfig
                    {
                        Offset = new PositionConfig { X = 0, Y = 42 },
                        Size = new SizeConfig { Width = 10, Height = 18 },
                        VirtualKey = 5,
                        IsVisible = true
                    },
                    ["MouseButton5"] = new ButtonConfig
                    {
                        Offset = new PositionConfig { X = 0, Y = 64 },
                        Size = new SizeConfig { Width = 10, Height = 18 },
                        VirtualKey = 6,
                        IsVisible = true
                    }
                }
            };
            
            // ESCキー
            layout.Keys["KeyEscape"] = new KeyDefinition 
            { 
                Position = new PositionConfig { X = 5, Y = 5 }, 
                Size = new SizeConfig { Width = 26, Height = 26 },
                Text = "Esc",
                VirtualKey = 0x1B,
                FontSize = 8,
                IsVisible = true
            };
            
            // 数字キー行 - 新しいKeyDefinition構造で定義
            AddKeyDefinition(layout, "Key1", 33, 5, 26, 26, "1", "!", 0x31);
            AddKeyDefinition(layout, "Key2", 61, 5, 26, 26, "2", "@", 0x32);
            AddKeyDefinition(layout, "Key3", 89, 5, 26, 26, "3", "#", 0x33);
            AddKeyDefinition(layout, "Key4", 117, 5, 26, 26, "4", "$", 0x34);
            AddKeyDefinition(layout, "Key5", 145, 5, 26, 26, "5", "%", 0x35);
            AddKeyDefinition(layout, "Key6", 173, 5, 26, 26, "6", "^", 0x36);
            AddKeyDefinition(layout, "Key7", 201, 5, 26, 26, "7", "&", 0x37);
            AddKeyDefinition(layout, "Key8", 229, 5, 26, 26, "8", "*", 0x38);
            AddKeyDefinition(layout, "Key9", 257, 5, 26, 26, "9", "(", 0x39);
            AddKeyDefinition(layout, "Key0", 285, 5, 26, 26, "0", ")", 0x30);
            AddKeyDefinition(layout, "KeyMinus", 313, 5, 26, 26, "-", "_", 0xBD);
            AddKeyDefinition(layout, "KeyEquals", 341, 5, 26, 26, "=", "+", 0xBB);
            AddKeyDefinition(layout, "KeyBackspace", 369, 5, 60, 26, "BS", null, 0x08, 8);
            
            // QWERTY行
            AddKeyDefinition(layout, "KeyTab", 5, 33, 36, 26, "Tab", null, 0x09, 9);
            AddKeyDefinition(layout, "KeyQ", 43, 33, 26, 26, "Q", null, 0x51);
            AddKeyDefinition(layout, "KeyW", 71, 33, 26, 26, "W", null, 0x57);
            AddKeyDefinition(layout, "KeyE", 99, 33, 26, 26, "E", null, 0x45);
            AddKeyDefinition(layout, "KeyR", 127, 33, 26, 26, "R", null, 0x52);
            AddKeyDefinition(layout, "KeyT", 155, 33, 26, 26, "T", null, 0x54);
            AddKeyDefinition(layout, "KeyY", 183, 33, 26, 26, "Y", null, 0x59);
            AddKeyDefinition(layout, "KeyU", 211, 33, 26, 26, "U", null, 0x55);
            AddKeyDefinition(layout, "KeyI", 239, 33, 26, 26, "I", null, 0x49);
            AddKeyDefinition(layout, "KeyO", 267, 33, 26, 26, "O", null, 0x4F);
            AddKeyDefinition(layout, "KeyP", 295, 33, 26, 26, "P", null, 0x50);
            AddKeyDefinition(layout, "KeyOpenBracket", 323, 33, 26, 26, "[", "{", 0xDB);
            AddKeyDefinition(layout, "KeyCloseBracket", 351, 33, 26, 26, "]", "}", 0xDD);
            AddKeyDefinition(layout, "KeyBackslash", 379, 33, 50, 26, "\\", "|", 0xDC);
            
            // ASDF行
            AddKeyDefinition(layout, "KeyCapsLock", 5, 61, 46, 26, "Caps", null, 0x14, 8);
            AddKeyDefinition(layout, "KeyA", 53, 61, 26, 26, "A", null, 0x41);
            AddKeyDefinition(layout, "KeyS", 81, 61, 26, 26, "S", null, 0x53);
            AddKeyDefinition(layout, "KeyD", 109, 61, 26, 26, "D", null, 0x44);
            AddKeyDefinition(layout, "KeyF", 137, 61, 26, 26, "F", null, 0x46);
            AddKeyDefinition(layout, "KeyG", 165, 61, 26, 26, "G", null, 0x47);
            AddKeyDefinition(layout, "KeyH", 193, 61, 26, 26, "H", null, 0x48);
            AddKeyDefinition(layout, "KeyJ", 221, 61, 26, 26, "J", null, 0x4A);
            AddKeyDefinition(layout, "KeyK", 249, 61, 26, 26, "K", null, 0x4B);
            AddKeyDefinition(layout, "KeyL", 277, 61, 26, 26, "L", null, 0x4C);
            AddKeyDefinition(layout, "KeySemicolon", 305, 61, 26, 26, ";", ":", 0xBA);
            AddKeyDefinition(layout, "KeyQuote", 333, 61, 26, 26, "'", "\"", 0xDE);
            AddKeyDefinition(layout, "KeyEnter", 361, 61, 68, 26, "Enter", null, 0x0D, 9);
            
            // ZXCV行
            AddKeyDefinition(layout, "KeyShift", 5, 89, 58, 26, "Shift", null, 0xA0, 9);
            AddKeyDefinition(layout, "KeyZ", 65, 89, 26, 26, "Z", null, 0x5A);
            AddKeyDefinition(layout, "KeyX", 93, 89, 26, 26, "X", null, 0x58);
            AddKeyDefinition(layout, "KeyC", 121, 89, 26, 26, "C", null, 0x43);
            AddKeyDefinition(layout, "KeyV", 149, 89, 26, 26, "V", null, 0x56);
            AddKeyDefinition(layout, "KeyB", 177, 89, 26, 26, "B", null, 0x42);
            AddKeyDefinition(layout, "KeyN", 205, 89, 26, 26, "N", null, 0x4E);
            AddKeyDefinition(layout, "KeyM", 233, 89, 26, 26, "M", null, 0x4D);
            AddKeyDefinition(layout, "KeyComma", 261, 89, 26, 26, ",", "<", 0xBC);
            AddKeyDefinition(layout, "KeyPeriod", 289, 89, 26, 26, ".", ">", 0xBE);
            AddKeyDefinition(layout, "KeySlash", 317, 89, 26, 26, "/", "?", 0xBF);
            AddKeyDefinition(layout, "KeyRightShift", 345, 89, 56, 26, "Shift", null, 0xA1, 8);
            AddKeyDefinition(layout, "KeyUpArrow", 403, 89, 26, 26, "↑", null, 0x26);
            
            // 最下段
            AddKeyDefinition(layout, "KeyCtrl", 5, 117, 32, 26, "Ctrl", null, 0xA2, 8);
            AddKeyDefinition(layout, "KeyWin", 39, 117, 32, 26, "Win", null, 0x5B, 8);
            AddKeyDefinition(layout, "KeyAlt", 73, 117, 32, 26, "Alt", null, 0xA4, 8);
            AddKeyDefinition(layout, "KeySpace", 107, 117, 164, 26, "Space", null, 0x20, 8);
            AddKeyDefinition(layout, "KeyRightAlt", 273, 117, 32, 26, "Alt", null, 0xA5, 8);
            AddKeyDefinition(layout, "KeyFn", 307, 117, 32, 26, "Fn", null, 0, 8);
            AddKeyDefinition(layout, "KeyRightCtrl", 341, 117, 32, 26, "Ctrl", null, 0xA3, 8);
            AddKeyDefinition(layout, "KeyLeftArrow", 375, 117, 26, 26, "←", null, 0x25);
            AddKeyDefinition(layout, "KeyDownArrow", 403, 117, 26, 26, "↓", null, 0x28);
            
            // ナビゲーションキー
            AddKeyDefinition(layout, "KeyHome", 431, 5, 26, 26, "Home", null, 0x24, 7);
            AddKeyDefinition(layout, "KeyDelete", 431, 33, 26, 26, "Del", null, 0x2E, 8);
            AddKeyDefinition(layout, "KeyPageUp", 431, 61, 26, 26, "PgUp", null, 0x21, 7);
            AddKeyDefinition(layout, "KeyPageDown", 431, 89, 26, 26, "PgDn", null, 0x22, 7);
            AddKeyDefinition(layout, "KeyRightArrow", 431, 117, 26, 26, "→", null, 0x27);
            
            return layout;
        }

        /// <summary>
        /// FPSキーボードレイアウトを作成
        /// </summary>
        public static LayoutConfig CreateDefaultFPSLayout()
        {
            var layout = CreateDefault65KeyboardLayout();
            
            // プロファイル情報を更新
            layout.Profile.Name = "FPSキーボード";
            layout.Profile.Description = "FPSゲーム用コンパクトキーボードレイアウト - 必要最小限のキー表示";
            
            // FPSで重要なキー
            var fpsKeys = new HashSet<string>
            {
                "KeyEscape", "Key1", "Key2", "Key3", "Key4", "Key5", "Key6", "Key7",
                "KeyTab", "KeyQ", "KeyW", "KeyE", "KeyR", "KeyT", "KeyY", "KeyU",
                "KeyCapsLock", "KeyA", "KeyS", "KeyD", "KeyF", "KeyG", "KeyH", "KeyJ",
                "KeyShift", "KeyZ", "KeyX", "KeyC", "KeyV", "KeyB", "KeyN", "KeyM",
                "KeyCtrl", "KeyWin", "KeyAlt", "KeySpace"
            };

            // FPS用にウィンドウサイズを調整
            layout.Window.Width = 520;
            layout.Window.Height = 160;
            layout.Global.WindowWidth = 520;
            layout.Global.WindowHeight = 160;
            
            // マウス位置をFPS用に調整
            layout.Mouse.Position.X = 290;
            layout.Mouse.Position.Y = 20;

            // FPSキー以外を非表示に
            foreach (var (keyName, keyDef) in layout.Keys)
            {
                if (!fpsKeys.Contains(keyName))
                {
                    keyDef.IsVisible = false;
                }
            }

            return layout;
        }

        /// <summary>
        /// キー定義を追加するヘルパーメソッド
        /// </summary>
        private static void AddKeyDefinition(LayoutConfig layout, string keyName, double x, double y, 
            double width, double height, string text, string? shiftText = null, int virtualKey = 0, int? fontSize = null)
        {
            layout.Keys[keyName] = new KeyDefinition
            {
                Position = new PositionConfig { X = x, Y = y },
                Size = new SizeConfig { Width = width, Height = height },
                Text = text,
                ShiftText = shiftText,
                VirtualKey = virtualKey,
                FontSize = fontSize,
                IsVisible = true
            };
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