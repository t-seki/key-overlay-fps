using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace KeyOverlayFPS.Input
{
    /// <summary>
    /// キー設定クラス
    /// </summary>
    public class KeyConfig
    {
        public string Name { get; }
        public int VirtualKey { get; }
        public string NormalText { get; }
        public string ShiftText { get; }
        public bool HasShiftVariant { get; }
        
        public KeyConfig(string name, int virtualKey, string normalText = "", string shiftText = "")
        {
            Name = name;
            VirtualKey = virtualKey;
            NormalText = normalText;
            ShiftText = shiftText;
            HasShiftVariant = !string.IsNullOrEmpty(shiftText);
        }
    }

    /// <summary>
    /// キーボードプロファイル
    /// </summary>
    public enum KeyboardProfile
    {
        FullKeyboard65,  // 現在の65%キーボード
        FPSKeyboard      // FPS用コンパクトキーボード
    }

    /// <summary>
    /// キーボード入力処理クラス - キー状態検出、プロファイル管理、設定管理
    /// </summary>
    public class KeyboardInputHandler
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
        
        // キー状態検出用定数
        private const short KEY_PRESSED_MASK = unchecked((short)0x8000);
        
        // よく使用されるキーコード
        public const int VK_LSHIFT = 0xA0;
        public const int VK_RSHIFT = 0xA1;
        public const int VK_LBUTTON = 0x01;
        public const int VK_RBUTTON = 0x02;
        public const int VK_MBUTTON = 0x04;
        public const int VK_XBUTTON1 = 0x05;
        public const int VK_XBUTTON2 = 0x06;

        private readonly Dictionary<string, KeyConfig> _keyConfigurations;
        private readonly Dictionary<KeyboardProfile, (double Left, double Top)> _mousePositions;
        private readonly Dictionary<KeyboardProfile, bool> _shiftDisplayEnabled;
        
        public KeyboardProfile CurrentProfile { get; set; } = KeyboardProfile.FullKeyboard65;

        public KeyboardInputHandler()
        {
            _keyConfigurations = CreateKeyConfigurations();
            _mousePositions = new Dictionary<KeyboardProfile, (double Left, double Top)>
            {
                { KeyboardProfile.FullKeyboard65, (475, 20) },  // 元の位置
                { KeyboardProfile.FPSKeyboard, (290, 20) }      // FPS用位置
            };
            _shiftDisplayEnabled = new Dictionary<KeyboardProfile, bool>
            {
                { KeyboardProfile.FullKeyboard65, true },   // 65%キーボードはShift表示変更有効
                { KeyboardProfile.FPSKeyboard, false }      // FPSキーボードはShift表示変更無効
            };
        }

        /// <summary>
        /// 指定された仮想キーが現在押されているかを判定
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsKeyPressed(int virtualKeyCode)
        {
            return (GetAsyncKeyState(virtualKeyCode) & KEY_PRESSED_MASK) != 0;
        }

        /// <summary>
        /// キー設定を取得
        /// </summary>
        public KeyConfig? GetKeyConfig(string keyName)
        {
            return _keyConfigurations.TryGetValue(keyName, out var config) ? config : null;
        }

        /// <summary>
        /// 全キー設定を取得
        /// </summary>
        public IReadOnlyDictionary<string, KeyConfig> GetAllKeyConfigurations()
        {
            return _keyConfigurations;
        }

        /// <summary>
        /// プロファイル別マウス位置を取得
        /// </summary>
        public (double Left, double Top) GetMousePosition(KeyboardProfile profile)
        {
            return _mousePositions.TryGetValue(profile, out var position) ? position : (475, 20);
        }

        /// <summary>
        /// プロファイル別Shift表示設定を取得
        /// </summary>
        public bool IsShiftDisplayEnabled(KeyboardProfile profile)
        {
            return _shiftDisplayEnabled.TryGetValue(profile, out var enabled) && enabled;
        }

        /// <summary>
        /// Shiftキーが押されているかを判定
        /// </summary>
        public bool IsShiftPressed()
        {
            return IsKeyPressed(VK_LSHIFT) || IsKeyPressed(VK_RSHIFT);
        }

        /// <summary>
        /// マウスボタンが押されているかを判定
        /// </summary>
        public bool IsMouseButtonPressed(int button)
        {
            return button switch
            {
                1 => IsKeyPressed(VK_LBUTTON),
                2 => IsKeyPressed(VK_RBUTTON),
                3 => IsKeyPressed(VK_MBUTTON),
                4 => IsKeyPressed(VK_XBUTTON1),
                5 => IsKeyPressed(VK_XBUTTON2),
                _ => false
            };
        }

        /// <summary>
        /// キー設定定義を作成
        /// </summary>
        private static Dictionary<string, KeyConfig> CreateKeyConfigurations()
        {
            return new Dictionary<string, KeyConfig>
            {
                // 数字キー
                { "KeyEscape", new KeyConfig("KeyEscape", 0x1B) },
                { "Key1", new KeyConfig("Key1", 0x31, "1", "!") },
                { "Key2", new KeyConfig("Key2", 0x32, "2", "@") },
                { "Key3", new KeyConfig("Key3", 0x33, "3", "#") },
                { "Key4", new KeyConfig("Key4", 0x34, "4", "$") },
                { "Key5", new KeyConfig("Key5", 0x35, "5", "%") },
                { "Key6", new KeyConfig("Key6", 0x36, "6", "^") },
                { "Key7", new KeyConfig("Key7", 0x37, "7", "&") },
                { "Key8", new KeyConfig("Key8", 0x38, "8", "*") },
                { "Key9", new KeyConfig("Key9", 0x39, "9", "(") },
                { "Key0", new KeyConfig("Key0", 0x30, "0", ")") },
                { "KeyMinus", new KeyConfig("KeyMinus", 0xBD, "-", "_") },
                { "KeyEquals", new KeyConfig("KeyEquals", 0xBB, "=", "+") },
                { "KeyBackspace", new KeyConfig("KeyBackspace", 0x08) },
                
                // QWERTYキー
                { "KeyTab", new KeyConfig("KeyTab", 0x09) },
                { "KeyQ", new KeyConfig("KeyQ", 0x51) },
                { "KeyW", new KeyConfig("KeyW", 0x57) },
                { "KeyE", new KeyConfig("KeyE", 0x45) },
                { "KeyR", new KeyConfig("KeyR", 0x52) },
                { "KeyT", new KeyConfig("KeyT", 0x54) },
                { "KeyY", new KeyConfig("KeyY", 0x59) },
                { "KeyU", new KeyConfig("KeyU", 0x55) },
                { "KeyI", new KeyConfig("KeyI", 0x49) },
                { "KeyO", new KeyConfig("KeyO", 0x4F) },
                { "KeyP", new KeyConfig("KeyP", 0x50) },
                { "KeyOpenBracket", new KeyConfig("KeyOpenBracket", 0xDB, "[", "{") },
                { "KeyCloseBracket", new KeyConfig("KeyCloseBracket", 0xDD, "]", "}") },
                { "KeyBackslash", new KeyConfig("KeyBackslash", 0xDC, "\\", "|") },
                
                // ASDFキー
                { "KeyCapsLock", new KeyConfig("KeyCapsLock", 0x14) },
                { "KeyA", new KeyConfig("KeyA", 0x41) },
                { "KeyS", new KeyConfig("KeyS", 0x53) },
                { "KeyD", new KeyConfig("KeyD", 0x44) },
                { "KeyF", new KeyConfig("KeyF", 0x46) },
                { "KeyG", new KeyConfig("KeyG", 0x47) },
                { "KeyH", new KeyConfig("KeyH", 0x48) },
                { "KeyJ", new KeyConfig("KeyJ", 0x4A) },
                { "KeyK", new KeyConfig("KeyK", 0x4B) },
                { "KeyL", new KeyConfig("KeyL", 0x4C) },
                { "KeySemicolon", new KeyConfig("KeySemicolon", 0xBA, ";", ":") },
                { "KeyQuote", new KeyConfig("KeyQuote", 0xDE, "'", "\"") },
                { "KeyEnter", new KeyConfig("KeyEnter", 0x0D) },
                
                // ZXCVキー
                { "KeyShift", new KeyConfig("KeyShift", VK_LSHIFT) },
                { "KeyZ", new KeyConfig("KeyZ", 0x5A) },
                { "KeyX", new KeyConfig("KeyX", 0x58) },
                { "KeyC", new KeyConfig("KeyC", 0x43) },
                { "KeyV", new KeyConfig("KeyV", 0x56) },
                { "KeyB", new KeyConfig("KeyB", 0x42) },
                { "KeyN", new KeyConfig("KeyN", 0x4E) },
                { "KeyM", new KeyConfig("KeyM", 0x4D) },
                { "KeyComma", new KeyConfig("KeyComma", 0xBC, ",", "<") },
                { "KeyPeriod", new KeyConfig("KeyPeriod", 0xBE, ".", ">") },
                { "KeySlash", new KeyConfig("KeySlash", 0xBF, "/", "?") },
                { "KeyRightShift", new KeyConfig("KeyRightShift", VK_RSHIFT) },
                
                // 制御キー
                { "KeyCtrl", new KeyConfig("KeyCtrl", 0xA2) },
                { "KeyWin", new KeyConfig("KeyWin", 0x5B) },
                { "KeyAlt", new KeyConfig("KeyAlt", 0xA4) },
                { "KeySpace", new KeyConfig("KeySpace", 0x20) }
            };
        }

        /// <summary>
        /// プロファイル別のキー要素名リストを取得
        /// </summary>
        public static List<string> GetProfileKeyElements(KeyboardProfile profile)
        {
            return profile switch
            {
                KeyboardProfile.FullKeyboard65 => new List<string>
                {
                    "KeyEscape",
                    "Key1", "Key2", "Key3", "Key4", "Key5", "Key6", "Key7", "Key8", "Key9", "Key0", "KeyMinus", "KeyEquals", "KeyBackspace",
                    "KeyTab", "KeyQ", "KeyW", "KeyE", "KeyR", "KeyT", "KeyY", "KeyU", "KeyI", "KeyO", "KeyP", "KeyOpenBracket", "KeyCloseBracket", "KeyBackslash",
                    "KeyCapsLock", "KeyA", "KeyS", "KeyD", "KeyF", "KeyG", "KeyH", "KeyJ", "KeyK", "KeyL", "KeySemicolon", "KeyQuote", "KeyEnter",
                    "KeyShift", "KeyZ", "KeyX", "KeyC", "KeyV", "KeyB", "KeyN", "KeyM", "KeyComma", "KeyPeriod", "KeySlash", "KeyRightShift",
                    "KeyCtrl", "KeyWin", "KeyAlt", "KeySpace"
                },
                KeyboardProfile.FPSKeyboard => new List<string>
                {
                    "KeyEscape", "Key1", "Key2", "Key3", "Key4", "Key5", "Key6", "Key7",
                    "KeyTab", "KeyQ", "KeyW", "KeyE", "KeyR", "KeyT", "KeyY", "KeyU",
                    "KeyCapsLock", "KeyA", "KeyS", "KeyD", "KeyF", "KeyG", "KeyH", "KeyJ",
                    "KeyShift", "KeyZ", "KeyX", "KeyC", "KeyV", "KeyB", "KeyN", "KeyM",
                    "KeyCtrl", "KeyWin", "KeyAlt", "KeySpace"
                },
                _ => new List<string>()
            };
        }
    }
}