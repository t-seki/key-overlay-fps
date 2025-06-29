namespace KeyOverlayFPS.Input
{
    /// <summary>
    /// Windows Virtual Key Codes定数定義
    /// Microsoft Learn: https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
    /// </summary>
    public static class VirtualKeyCodes
    {
        // マウスボタン
        public const int VK_LBUTTON = 0x01;     // 左マウスボタン
        public const int VK_RBUTTON = 0x02;     // 右マウスボタン
        public const int VK_CANCEL = 0x03;      // Control-break processing
        public const int VK_MBUTTON = 0x04;     // ホイールボタン (中ボタン)
        public const int VK_XBUTTON1 = 0x05;    // X1マウスボタン
        public const int VK_XBUTTON2 = 0x06;    // X2マウスボタン

        // 制御キー
        public const int VK_BACK = 0x08;        // Backspace
        public const int VK_TAB = 0x09;         // Tab
        public const int VK_CLEAR = 0x0C;       // Clear
        public const int VK_RETURN = 0x0D;      // Enter
        public const int VK_SHIFT = 0x10;       // Shift
        public const int VK_CONTROL = 0x11;     // Ctrl
        public const int VK_MENU = 0x12;        // Alt
        public const int VK_PAUSE = 0x13;       // Pause
        public const int VK_CAPITAL = 0x14;     // Caps Lock
        public const int VK_ESCAPE = 0x1B;      // Esc
        public const int VK_SPACE = 0x20;       // Space

        // ナビゲーションキー
        public const int VK_PRIOR = 0x21;       // Page Up
        public const int VK_NEXT = 0x22;        // Page Down
        public const int VK_END = 0x23;         // End
        public const int VK_HOME = 0x24;        // Home
        public const int VK_LEFT = 0x25;        // 左矢印
        public const int VK_UP = 0x26;          // 上矢印
        public const int VK_RIGHT = 0x27;       // 右矢印
        public const int VK_DOWN = 0x28;        // 下矢印
        public const int VK_SELECT = 0x29;      // Select
        public const int VK_PRINT = 0x2A;       // Print
        public const int VK_EXECUTE = 0x2B;     // Execute
        public const int VK_SNAPSHOT = 0x2C;    // Print Screen
        public const int VK_INSERT = 0x2D;      // Insert
        public const int VK_DELETE = 0x2E;      // Delete
        public const int VK_HELP = 0x2F;        // Help

        // 数字キー (0-9)
        public const int VK_0 = 0x30;
        public const int VK_1 = 0x31;
        public const int VK_2 = 0x32;
        public const int VK_3 = 0x33;
        public const int VK_4 = 0x34;
        public const int VK_5 = 0x35;
        public const int VK_6 = 0x36;
        public const int VK_7 = 0x37;
        public const int VK_8 = 0x38;
        public const int VK_9 = 0x39;

        // アルファベットキー (A-Z)
        public const int VK_A = 0x41;
        public const int VK_B = 0x42;
        public const int VK_C = 0x43;
        public const int VK_D = 0x44;
        public const int VK_E = 0x45;
        public const int VK_F = 0x46;
        public const int VK_G = 0x47;
        public const int VK_H = 0x48;
        public const int VK_I = 0x49;
        public const int VK_J = 0x4A;
        public const int VK_K = 0x4B;
        public const int VK_L = 0x4C;
        public const int VK_M = 0x4D;
        public const int VK_N = 0x4E;
        public const int VK_O = 0x4F;
        public const int VK_P = 0x50;
        public const int VK_Q = 0x51;
        public const int VK_R = 0x52;
        public const int VK_S = 0x53;
        public const int VK_T = 0x54;
        public const int VK_U = 0x55;
        public const int VK_V = 0x56;
        public const int VK_W = 0x57;
        public const int VK_X = 0x58;
        public const int VK_Y = 0x59;
        public const int VK_Z = 0x5A;

        // Windowsキー
        public const int VK_LWIN = 0x5B;        // 左Windowsキー
        public const int VK_RWIN = 0x5C;        // 右Windowsキー
        public const int VK_APPS = 0x5D;        // アプリケーションキー

        // テンキー
        public const int VK_NUMPAD0 = 0x60;
        public const int VK_NUMPAD1 = 0x61;
        public const int VK_NUMPAD2 = 0x62;
        public const int VK_NUMPAD3 = 0x63;
        public const int VK_NUMPAD4 = 0x64;
        public const int VK_NUMPAD5 = 0x65;
        public const int VK_NUMPAD6 = 0x66;
        public const int VK_NUMPAD7 = 0x67;
        public const int VK_NUMPAD8 = 0x68;
        public const int VK_NUMPAD9 = 0x69;
        public const int VK_MULTIPLY = 0x6A;    // テンキー *
        public const int VK_ADD = 0x6B;         // テンキー +
        public const int VK_SEPARATOR = 0x6C;   // テンキー Separator
        public const int VK_SUBTRACT = 0x6D;    // テンキー -
        public const int VK_DECIMAL = 0x6E;     // テンキー .
        public const int VK_DIVIDE = 0x6F;      // テンキー /

        // ファンクションキー
        public const int VK_F1 = 0x70;
        public const int VK_F2 = 0x71;
        public const int VK_F3 = 0x72;
        public const int VK_F4 = 0x73;
        public const int VK_F5 = 0x74;
        public const int VK_F6 = 0x75;
        public const int VK_F7 = 0x76;
        public const int VK_F8 = 0x77;
        public const int VK_F9 = 0x78;
        public const int VK_F10 = 0x79;
        public const int VK_F11 = 0x7A;
        public const int VK_F12 = 0x7B;
        public const int VK_F13 = 0x7C;
        public const int VK_F14 = 0x7D;
        public const int VK_F15 = 0x7E;
        public const int VK_F16 = 0x7F;
        public const int VK_F17 = 0x80;
        public const int VK_F18 = 0x81;
        public const int VK_F19 = 0x82;
        public const int VK_F20 = 0x83;
        public const int VK_F21 = 0x84;
        public const int VK_F22 = 0x85;
        public const int VK_F23 = 0x86;
        public const int VK_F24 = 0x87;

        // ロックキー
        public const int VK_NUMLOCK = 0x90;     // Num Lock
        public const int VK_SCROLL = 0x91;      // Scroll Lock

        // 修飾キー (左右区別)
        public const int VK_LSHIFT = 0xA0;      // 左Shift
        public const int VK_RSHIFT = 0xA1;      // 右Shift
        public const int VK_LCONTROL = 0xA2;    // 左Ctrl
        public const int VK_RCONTROL = 0xA3;    // 右Ctrl
        public const int VK_LMENU = 0xA4;       // 左Alt
        public const int VK_RMENU = 0xA5;       // 右Alt

        // ブラウザキー
        public const int VK_BROWSER_BACK = 0xA6;        // ブラウザ戻る
        public const int VK_BROWSER_FORWARD = 0xA7;     // ブラウザ進む
        public const int VK_BROWSER_REFRESH = 0xA8;     // ブラウザ更新
        public const int VK_BROWSER_STOP = 0xA9;        // ブラウザ停止
        public const int VK_BROWSER_SEARCH = 0xAA;      // ブラウザ検索
        public const int VK_BROWSER_FAVORITES = 0xAB;   // ブラウザお気に入り
        public const int VK_BROWSER_HOME = 0xAC;        // ブラウザホーム

        // 音量制御
        public const int VK_VOLUME_MUTE = 0xAD;         // ミュート
        public const int VK_VOLUME_DOWN = 0xAE;         // 音量下げる
        public const int VK_VOLUME_UP = 0xAF;           // 音量上げる

        // メディア制御
        public const int VK_MEDIA_NEXT_TRACK = 0xB0;    // 次のトラック
        public const int VK_MEDIA_PREV_TRACK = 0xB1;    // 前のトラック
        public const int VK_MEDIA_STOP = 0xB2;          // メディア停止
        public const int VK_MEDIA_PLAY_PAUSE = 0xB3;    // 再生/一時停止

        // OEM 特殊文字キー (US配列)
        public const int VK_OEM_1 = 0xBA;       // ; :
        public const int VK_OEM_PLUS = 0xBB;    // = +
        public const int VK_OEM_COMMA = 0xBC;   // , <
        public const int VK_OEM_MINUS = 0xBD;   // - _
        public const int VK_OEM_PERIOD = 0xBE;  // . >
        public const int VK_OEM_2 = 0xBF;       // / ?
        public const int VK_OEM_3 = 0xC0;       // ` ~
        public const int VK_OEM_4 = 0xDB;       // [ {
        public const int VK_OEM_5 = 0xDC;       // \ |
        public const int VK_OEM_6 = 0xDD;       // ] }
        public const int VK_OEM_7 = 0xDE;       // ' "

        // 処理キー
        public const int VK_PROCESSKEY = 0xE5;  // IME PROCESS key

        // その他
        public const int VK_PACKET = 0xE7;      // Unicode文字用
        public const int VK_ATTN = 0xF6;        // Attn
        public const int VK_CRSEL = 0xF7;       // CrSel
        public const int VK_EXSEL = 0xF8;       // ExSel
        public const int VK_EREOF = 0xF9;       // Erase EOF
        public const int VK_PLAY = 0xFA;        // Play
        public const int VK_ZOOM = 0xFB;        // Zoom
        public const int VK_NONAME = 0xFC;      // Reserved
        public const int VK_PA1 = 0xFD;         // PA1
        public const int VK_OEM_CLEAR = 0xFE;   // Clear
        
        // 特殊キー（検出不可能な機能キー）
        public const int VK_FN = -1;            // Fnキー（ハードウェア依存、通常検出不可）
    }
}