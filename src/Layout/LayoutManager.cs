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
            
            // ESCキー
            layout.Elements["KeyEscape"] = new ElementConfig { X = 5, Y = 5, Text = "Esc" };
            
            // 数字キー行
            layout.Elements["Key1"] = new ElementConfig { X = 33, Y = 5, Text = "1" };
            layout.Elements["Key2"] = new ElementConfig { X = 61, Y = 5, Text = "2" };
            layout.Elements["Key3"] = new ElementConfig { X = 89, Y = 5, Text = "3" };
            layout.Elements["Key4"] = new ElementConfig { X = 117, Y = 5, Text = "4" };
            layout.Elements["Key5"] = new ElementConfig { X = 145, Y = 5, Text = "5" };
            layout.Elements["Key6"] = new ElementConfig { X = 173, Y = 5, Text = "6" };
            layout.Elements["Key7"] = new ElementConfig { X = 201, Y = 5, Text = "7" };
            layout.Elements["Key8"] = new ElementConfig { X = 229, Y = 5, Text = "8" };
            layout.Elements["Key9"] = new ElementConfig { X = 257, Y = 5, Text = "9" };
            layout.Elements["Key0"] = new ElementConfig { X = 285, Y = 5, Text = "0" };
            layout.Elements["KeyMinus"] = new ElementConfig { X = 313, Y = 5, Text = "-" };
            layout.Elements["KeyEquals"] = new ElementConfig { X = 341, Y = 5, Text = "=" };
            layout.Elements["KeyBackspace"] = new ElementConfig { X = 369, Y = 5, Text = "BS", Size = new SizeConfig { Width = 60, Height = 26 } };
            
            // QWERTY行
            layout.Elements["KeyTab"] = new ElementConfig { X = 5, Y = 33, Text = "Tab", Size = new SizeConfig { Width = 36, Height = 26 }, FontSize = 9 };
            layout.Elements["KeyQ"] = new ElementConfig { X = 43, Y = 33, Text = "Q" };
            layout.Elements["KeyW"] = new ElementConfig { X = 71, Y = 33, Text = "W" };
            layout.Elements["KeyE"] = new ElementConfig { X = 99, Y = 33, Text = "E" };
            layout.Elements["KeyR"] = new ElementConfig { X = 127, Y = 33, Text = "R" };
            layout.Elements["KeyT"] = new ElementConfig { X = 155, Y = 33, Text = "T" };
            layout.Elements["KeyY"] = new ElementConfig { X = 183, Y = 33, Text = "Y" };
            layout.Elements["KeyU"] = new ElementConfig { X = 211, Y = 33, Text = "U" };
            layout.Elements["KeyI"] = new ElementConfig { X = 239, Y = 33, Text = "I" };
            layout.Elements["KeyO"] = new ElementConfig { X = 267, Y = 33, Text = "O" };
            layout.Elements["KeyP"] = new ElementConfig { X = 295, Y = 33, Text = "P" };
            layout.Elements["KeyOpenBracket"] = new ElementConfig { X = 323, Y = 33, Text = "[" };
            layout.Elements["KeyCloseBracket"] = new ElementConfig { X = 351, Y = 33, Text = "]" };
            layout.Elements["KeyBackslash"] = new ElementConfig { X = 379, Y = 33, Text = "\\", Size = new SizeConfig { Width = 50, Height = 26 } };
            
            // ASDF行
            layout.Elements["KeyCapsLock"] = new ElementConfig { X = 5, Y = 61, Text = "Caps", Size = new SizeConfig { Width = 46, Height = 26 }, FontSize = 8 };
            layout.Elements["KeyA"] = new ElementConfig { X = 53, Y = 61, Text = "A" };
            layout.Elements["KeyS"] = new ElementConfig { X = 81, Y = 61, Text = "S" };
            layout.Elements["KeyD"] = new ElementConfig { X = 109, Y = 61, Text = "D" };
            layout.Elements["KeyF"] = new ElementConfig { X = 137, Y = 61, Text = "F" };
            layout.Elements["KeyG"] = new ElementConfig { X = 165, Y = 61, Text = "G" };
            layout.Elements["KeyH"] = new ElementConfig { X = 193, Y = 61, Text = "H" };
            layout.Elements["KeyJ"] = new ElementConfig { X = 221, Y = 61, Text = "J" };
            layout.Elements["KeyK"] = new ElementConfig { X = 249, Y = 61, Text = "K" };
            layout.Elements["KeyL"] = new ElementConfig { X = 277, Y = 61, Text = "L" };
            layout.Elements["KeySemicolon"] = new ElementConfig { X = 305, Y = 61, Text = ";" };
            layout.Elements["KeyQuote"] = new ElementConfig { X = 333, Y = 61, Text = "'" };
            layout.Elements["KeyEnter"] = new ElementConfig { X = 361, Y = 61, Text = "Enter", Size = new SizeConfig { Width = 68, Height = 26 }, FontSize = 9 };
            
            // ZXCV行
            layout.Elements["KeyShift"] = new ElementConfig { X = 5, Y = 89, Text = "Shift", Size = new SizeConfig { Width = 58, Height = 26 }, FontSize = 9 };
            layout.Elements["KeyZ"] = new ElementConfig { X = 65, Y = 89, Text = "Z" };
            layout.Elements["KeyX"] = new ElementConfig { X = 93, Y = 89, Text = "X" };
            layout.Elements["KeyC"] = new ElementConfig { X = 121, Y = 89, Text = "C" };
            layout.Elements["KeyV"] = new ElementConfig { X = 149, Y = 89, Text = "V" };
            layout.Elements["KeyB"] = new ElementConfig { X = 177, Y = 89, Text = "B" };
            layout.Elements["KeyN"] = new ElementConfig { X = 205, Y = 89, Text = "N" };
            layout.Elements["KeyM"] = new ElementConfig { X = 233, Y = 89, Text = "M" };
            layout.Elements["KeyComma"] = new ElementConfig { X = 261, Y = 89, Text = "," };
            layout.Elements["KeyPeriod"] = new ElementConfig { X = 289, Y = 89, Text = "." };
            layout.Elements["KeySlash"] = new ElementConfig { X = 317, Y = 89, Text = "/" };
            layout.Elements["KeyRightShift"] = new ElementConfig { X = 345, Y = 89, Text = "Shift", Size = new SizeConfig { Width = 56, Height = 26 }, FontSize = 8 };
            layout.Elements["KeyUpArrow"] = new ElementConfig { X = 403, Y = 89, Text = "↑" };
            
            // 最下段
            layout.Elements["KeyCtrl"] = new ElementConfig { X = 5, Y = 117, Text = "Ctrl", Size = new SizeConfig { Width = 32, Height = 26 }, FontSize = 8 };
            layout.Elements["KeyWin"] = new ElementConfig { X = 39, Y = 117, Text = "Win", Size = new SizeConfig { Width = 32, Height = 26 }, FontSize = 8 };
            layout.Elements["KeyAlt"] = new ElementConfig { X = 73, Y = 117, Text = "Alt", Size = new SizeConfig { Width = 32, Height = 26 }, FontSize = 8 };
            layout.Elements["KeySpace"] = new ElementConfig { X = 107, Y = 117, Text = "Space", Size = new SizeConfig { Width = 164, Height = 26 }, FontSize = 8 };
            layout.Elements["KeyRightAlt"] = new ElementConfig { X = 273, Y = 117, Text = "Alt", Size = new SizeConfig { Width = 32, Height = 26 }, FontSize = 8 };
            layout.Elements["KeyFn"] = new ElementConfig { X = 307, Y = 117, Text = "Fn", Size = new SizeConfig { Width = 32, Height = 26 }, FontSize = 8 };
            layout.Elements["KeyRightCtrl"] = new ElementConfig { X = 341, Y = 117, Text = "Ctrl", Size = new SizeConfig { Width = 32, Height = 26 }, FontSize = 8 };
            layout.Elements["KeyLeftArrow"] = new ElementConfig { X = 375, Y = 117, Text = "←" };
            layout.Elements["KeyDownArrow"] = new ElementConfig { X = 403, Y = 117, Text = "↓" };
            
            // ナビゲーションキー
            layout.Elements["KeyHome"] = new ElementConfig { X = 431, Y = 5, Text = "Home", FontSize = 7 };
            layout.Elements["KeyDelete"] = new ElementConfig { X = 431, Y = 33, Text = "Del", FontSize = 8 };
            layout.Elements["KeyPageUp"] = new ElementConfig { X = 431, Y = 61, Text = "PgUp", FontSize = 7 };
            layout.Elements["KeyPageDown"] = new ElementConfig { X = 431, Y = 89, Text = "PgDn", FontSize = 7 };
            layout.Elements["KeyRightArrow"] = new ElementConfig { X = 431, Y = 117, Text = "→" };
            
            // マウス要素
            layout.Elements["MouseBody"] = new ElementConfig { X = 475, Y = 20 };
            layout.Elements["MouseLeft"] = new ElementConfig { X = 478, Y = 23 };
            layout.Elements["MouseRight"] = new ElementConfig { X = 507, Y = 23 };
            layout.Elements["MouseWheelButton"] = new ElementConfig { X = 500, Y = 28 };
            layout.Elements["MouseButton4"] = new ElementConfig { X = 475, Y = 62 };
            layout.Elements["MouseButton5"] = new ElementConfig { X = 475, Y = 84 };
            
            return layout;
        }

        /// <summary>
        /// FPSキーボードレイアウトを作成
        /// </summary>
        public static LayoutConfig CreateDefaultFPSLayout()
        {
            var layout = CreateDefault65KeyboardLayout();
            
            // FPSで不要なキーを非表示に
            var fpsKeys = new HashSet<string>
            {
                "KeyEscape", "Key1", "Key2", "Key3", "Key4", "Key5", "Key6", "Key7",
                "KeyTab", "KeyQ", "KeyW", "KeyE", "KeyR", "KeyT", "KeyY", "KeyU",
                "KeyCapsLock", "KeyA", "KeyS", "KeyD", "KeyF", "KeyG", "KeyH", "KeyJ",
                "KeyShift", "KeyZ", "KeyX", "KeyC", "KeyV", "KeyB", "KeyN", "KeyM",
                "KeyCtrl", "KeyWin", "KeyAlt", "KeySpace",
                "MouseBody", "MouseLeft", "MouseRight", "MouseWheelButton", "MouseButton4", "MouseButton5"
            };

            // FPS用にウィンドウサイズを調整
            layout.Global.WindowWidth = 520;
            layout.Global.WindowHeight = 160;
            
            // マウス位置をFPS用に調整
            layout.Elements["MouseBody"].X = 290;
            layout.Elements["MouseLeft"].X = 293;
            layout.Elements["MouseRight"].X = 322;
            layout.Elements["MouseWheelButton"].X = 315;
            layout.Elements["MouseButton4"].X = 290;
            layout.Elements["MouseButton5"].X = 290;

            // FPSキー以外を非表示に
            foreach (var element in layout.Elements.Values)
            {
                if (!fpsKeys.Contains(layout.Elements.FirstOrDefault(x => x.Value == element).Key ?? ""))
                {
                    element.IsVisible = false;
                }
            }

            return layout;
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

            if (layout.Elements == null)
                throw new InvalidOperationException("要素設定が存在しません");

            // 基本的な数値範囲チェック
            if (layout.Global.FontSize <= 0)
                throw new InvalidOperationException("フォントサイズは1以上である必要があります");

            if (layout.Global.KeySize.Width <= 0 || layout.Global.KeySize.Height <= 0)
                throw new InvalidOperationException("キーサイズは0より大きい値である必要があります");

            if (layout.Global.MouseMoveThreshold < 0)
                throw new InvalidOperationException("マウス移動感度は0以上である必要があります");

            if (layout.Global.MouseMoveHighlightDuration < 0)
                throw new InvalidOperationException("ハイライト継続時間は0以上である必要があります");
        }
    }
}