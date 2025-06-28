using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using KeyOverlayFPS.Layout;

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
        

        private readonly Layout.LayoutManager _layoutManager;
        
        public KeyboardProfile CurrentProfile { get; set; } = KeyboardProfile.FullKeyboard65;

        public KeyboardInputHandler(Layout.LayoutManager layoutManager)
        {
            _layoutManager = layoutManager ?? throw new ArgumentNullException(nameof(layoutManager));
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
            if (_layoutManager.CurrentLayout?.Keys?.TryGetValue(keyName, out var keyDefinition) == true)
            {
                return ConvertToKeyConfig(keyName, keyDefinition);
            }
            return null;
        }

        /// <summary>
        /// 全キー設定を取得
        /// </summary>
        public IReadOnlyDictionary<string, KeyConfig> GetAllKeyConfigurations()
        {
            var result = new Dictionary<string, KeyConfig>();
            if (_layoutManager.CurrentLayout?.Keys != null)
            {
                foreach (var kvp in _layoutManager.CurrentLayout.Keys)
                {
                    result[kvp.Key] = ConvertToKeyConfig(kvp.Key, kvp.Value);
                }
            }
            return result;
        }

        /// <summary>
        /// KeyDefinitionをKeyConfigに変換
        /// </summary>
        private static KeyConfig ConvertToKeyConfig(string keyName, KeyDefinition keyDefinition)
        {
            return new KeyConfig(
                keyName,
                keyDefinition.VirtualKey,
                keyDefinition.Text ?? "",
                keyDefinition.ShiftText ?? ""
            );
        }


    }
}