using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using KeyOverlayFPS.Input;

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
                        VirtualKey = VirtualKeyCodes.VK_LBUTTON,
                        IsVisible = true
                    },
                    ["MouseRight"] = new ButtonConfig
                    {
                        Offset = new PositionConfig { X = 32, Y = 3 },
                        Size = new SizeConfig { Width = 25, Height = 35 },
                        VirtualKey = VirtualKeyCodes.VK_RBUTTON,
                        IsVisible = true
                    },
                    ["MouseWheelButton"] = new ButtonConfig
                    {
                        Offset = new PositionConfig { X = 25, Y = 10 },
                        Size = new SizeConfig { Width = 10, Height = 22 },
                        VirtualKey = VirtualKeyCodes.VK_MBUTTON,
                        IsVisible = true
                    },
                    ["MouseButton4"] = new ButtonConfig
                    {
                        Offset = new PositionConfig { X = 0, Y = 42 },
                        Size = new SizeConfig { Width = 10, Height = 18 },
                        VirtualKey = VirtualKeyCodes.VK_XBUTTON1,
                        IsVisible = true
                    },
                    ["MouseButton5"] = new ButtonConfig
                    {
                        Offset = new PositionConfig { X = 0, Y = 64 },
                        Size = new SizeConfig { Width = 10, Height = 18 },
                        VirtualKey = VirtualKeyCodes.VK_XBUTTON2,
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
                VirtualKey = VirtualKeyCodes.VK_ESCAPE,
                FontSize = 8,
                IsVisible = true
            };
            
            // 数字キー行 - 新しいKeyDefinition構造で定義
            AddKeyDefinition(layout, "Key1", 33, 5, 26, 26, "1", "!", VirtualKeyCodes.VK_1);
            AddKeyDefinition(layout, "Key2", 61, 5, 26, 26, "2", "@", VirtualKeyCodes.VK_2);
            AddKeyDefinition(layout, "Key3", 89, 5, 26, 26, "3", "#", VirtualKeyCodes.VK_3);
            AddKeyDefinition(layout, "Key4", 117, 5, 26, 26, "4", "$", VirtualKeyCodes.VK_4);
            AddKeyDefinition(layout, "Key5", 145, 5, 26, 26, "5", "%", VirtualKeyCodes.VK_5);
            AddKeyDefinition(layout, "Key6", 173, 5, 26, 26, "6", "^", VirtualKeyCodes.VK_6);
            AddKeyDefinition(layout, "Key7", 201, 5, 26, 26, "7", "&", VirtualKeyCodes.VK_7);
            AddKeyDefinition(layout, "Key8", 229, 5, 26, 26, "8", "*", VirtualKeyCodes.VK_8);
            AddKeyDefinition(layout, "Key9", 257, 5, 26, 26, "9", "(", VirtualKeyCodes.VK_9);
            AddKeyDefinition(layout, "Key0", 285, 5, 26, 26, "0", ")", VirtualKeyCodes.VK_0);
            AddKeyDefinition(layout, "KeyMinus", 313, 5, 26, 26, "-", "_", VirtualKeyCodes.VK_OEM_MINUS);
            AddKeyDefinition(layout, "KeyEquals", 341, 5, 26, 26, "=", "+", VirtualKeyCodes.VK_OEM_PLUS);
            AddKeyDefinition(layout, "KeyBackspace", 369, 5, 60, 26, "BS", null, VirtualKeyCodes.VK_BACK, 8);
            
            // QWERTY行
            AddKeyDefinition(layout, "KeyTab", 5, 33, 36, 26, "Tab", null, VirtualKeyCodes.VK_TAB, 9);
            AddKeyDefinition(layout, "KeyQ", 43, 33, 26, 26, "Q", null, VirtualKeyCodes.VK_Q);
            AddKeyDefinition(layout, "KeyW", 71, 33, 26, 26, "W", null, VirtualKeyCodes.VK_W);
            AddKeyDefinition(layout, "KeyE", 99, 33, 26, 26, "E", null, VirtualKeyCodes.VK_E);
            AddKeyDefinition(layout, "KeyR", 127, 33, 26, 26, "R", null, VirtualKeyCodes.VK_R);
            AddKeyDefinition(layout, "KeyT", 155, 33, 26, 26, "T", null, VirtualKeyCodes.VK_T);
            AddKeyDefinition(layout, "KeyY", 183, 33, 26, 26, "Y", null, VirtualKeyCodes.VK_Y);
            AddKeyDefinition(layout, "KeyU", 211, 33, 26, 26, "U", null, VirtualKeyCodes.VK_U);
            AddKeyDefinition(layout, "KeyI", 239, 33, 26, 26, "I", null, VirtualKeyCodes.VK_I);
            AddKeyDefinition(layout, "KeyO", 267, 33, 26, 26, "O", null, VirtualKeyCodes.VK_O);
            AddKeyDefinition(layout, "KeyP", 295, 33, 26, 26, "P", null, VirtualKeyCodes.VK_P);
            AddKeyDefinition(layout, "KeyOpenBracket", 323, 33, 26, 26, "[", "{", VirtualKeyCodes.VK_OEM_4);
            AddKeyDefinition(layout, "KeyCloseBracket", 351, 33, 26, 26, "]", "}", VirtualKeyCodes.VK_OEM_6);
            AddKeyDefinition(layout, "KeyBackslash", 379, 33, 50, 26, "\\", "|", VirtualKeyCodes.VK_OEM_5);
            
            // ASDF行
            AddKeyDefinition(layout, "KeyCapsLock", 5, 61, 46, 26, "Caps", null, VirtualKeyCodes.VK_CAPITAL, 8);
            AddKeyDefinition(layout, "KeyA", 53, 61, 26, 26, "A", null, VirtualKeyCodes.VK_A);
            AddKeyDefinition(layout, "KeyS", 81, 61, 26, 26, "S", null, VirtualKeyCodes.VK_S);
            AddKeyDefinition(layout, "KeyD", 109, 61, 26, 26, "D", null, VirtualKeyCodes.VK_D);
            AddKeyDefinition(layout, "KeyF", 137, 61, 26, 26, "F", null, VirtualKeyCodes.VK_F);
            AddKeyDefinition(layout, "KeyG", 165, 61, 26, 26, "G", null, VirtualKeyCodes.VK_G);
            AddKeyDefinition(layout, "KeyH", 193, 61, 26, 26, "H", null, VirtualKeyCodes.VK_H);
            AddKeyDefinition(layout, "KeyJ", 221, 61, 26, 26, "J", null, VirtualKeyCodes.VK_J);
            AddKeyDefinition(layout, "KeyK", 249, 61, 26, 26, "K", null, VirtualKeyCodes.VK_K);
            AddKeyDefinition(layout, "KeyL", 277, 61, 26, 26, "L", null, VirtualKeyCodes.VK_L);
            AddKeyDefinition(layout, "KeySemicolon", 305, 61, 26, 26, ";", ":", VirtualKeyCodes.VK_OEM_1);
            AddKeyDefinition(layout, "KeyQuote", 333, 61, 26, 26, "'", "\"", VirtualKeyCodes.VK_OEM_7);
            AddKeyDefinition(layout, "KeyEnter", 361, 61, 68, 26, "Enter", null, VirtualKeyCodes.VK_RETURN, 9);
            
            // ZXCV行
            AddKeyDefinition(layout, "KeyShift", 5, 89, 58, 26, "Shift", null, VirtualKeyCodes.VK_LSHIFT, 9);
            AddKeyDefinition(layout, "KeyZ", 65, 89, 26, 26, "Z", null, VirtualKeyCodes.VK_Z);
            AddKeyDefinition(layout, "KeyX", 93, 89, 26, 26, "X", null, VirtualKeyCodes.VK_X);
            AddKeyDefinition(layout, "KeyC", 121, 89, 26, 26, "C", null, VirtualKeyCodes.VK_C);
            AddKeyDefinition(layout, "KeyV", 149, 89, 26, 26, "V", null, VirtualKeyCodes.VK_V);
            AddKeyDefinition(layout, "KeyB", 177, 89, 26, 26, "B", null, VirtualKeyCodes.VK_B);
            AddKeyDefinition(layout, "KeyN", 205, 89, 26, 26, "N", null, VirtualKeyCodes.VK_N);
            AddKeyDefinition(layout, "KeyM", 233, 89, 26, 26, "M", null, VirtualKeyCodes.VK_M);
            AddKeyDefinition(layout, "KeyComma", 261, 89, 26, 26, ",", "<", VirtualKeyCodes.VK_OEM_COMMA);
            AddKeyDefinition(layout, "KeyPeriod", 289, 89, 26, 26, ".", ">", VirtualKeyCodes.VK_OEM_PERIOD);
            AddKeyDefinition(layout, "KeySlash", 317, 89, 26, 26, "/", "?", VirtualKeyCodes.VK_OEM_2);
            AddKeyDefinition(layout, "KeyRightShift", 345, 89, 56, 26, "Shift", null, VirtualKeyCodes.VK_RSHIFT, 8);
            AddKeyDefinition(layout, "KeyUpArrow", 403, 89, 26, 26, "↑", null, VirtualKeyCodes.VK_UP);
            
            // 最下段
            AddKeyDefinition(layout, "KeyCtrl", 5, 117, 32, 26, "Ctrl", null, VirtualKeyCodes.VK_LCONTROL, 8);
            AddKeyDefinition(layout, "KeyWin", 39, 117, 32, 26, "Win", null, VirtualKeyCodes.VK_LWIN, 8);
            AddKeyDefinition(layout, "KeyAlt", 73, 117, 32, 26, "Alt", null, VirtualKeyCodes.VK_LMENU, 8);
            AddKeyDefinition(layout, "KeySpace", 107, 117, 164, 26, "Space", null, VirtualKeyCodes.VK_SPACE, 8);
            AddKeyDefinition(layout, "KeyRightAlt", 273, 117, 32, 26, "Alt", null, VirtualKeyCodes.VK_RMENU, 8);
            AddKeyDefinition(layout, "KeyFn", 307, 117, 32, 26, "Fn", null, 0, 8);
            AddKeyDefinition(layout, "KeyRightCtrl", 341, 117, 32, 26, "Ctrl", null, VirtualKeyCodes.VK_RCONTROL, 8);
            AddKeyDefinition(layout, "KeyLeftArrow", 375, 117, 26, 26, "←", null, VirtualKeyCodes.VK_LEFT);
            AddKeyDefinition(layout, "KeyDownArrow", 403, 117, 26, 26, "↓", null, VirtualKeyCodes.VK_DOWN);
            
            // ナビゲーションキー
            AddKeyDefinition(layout, "KeyHome", 431, 5, 26, 26, "Home", null, VirtualKeyCodes.VK_HOME, 7);
            AddKeyDefinition(layout, "KeyDelete", 431, 33, 26, 26, "Del", null, VirtualKeyCodes.VK_DELETE, 8);
            AddKeyDefinition(layout, "KeyPageUp", 431, 61, 26, 26, "PgUp", null, VirtualKeyCodes.VK_PRIOR, 7);
            AddKeyDefinition(layout, "KeyPageDown", 431, 89, 26, 26, "PgDn", null, VirtualKeyCodes.VK_NEXT, 7);
            AddKeyDefinition(layout, "KeyRightArrow", 431, 117, 26, 26, "→", null, VirtualKeyCodes.VK_RIGHT);
            
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