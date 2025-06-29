using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using KeyOverlayFPS.Utils;
using KeyOverlayFPS.Constants;
using KeyOverlayFPS.Layout;
using KeyOverlayFPS.Colors;
using KeyOverlayFPS.UI;

namespace KeyOverlayFPS.Settings
{
    /// <summary>
    /// 統一された設定管理クラス
    /// </summary>
    public class SettingsManager : ISettingsService
    {
        private static SettingsManager? _instance;
        private static readonly object _lock = new object();
        
        private AppSettings _settings;
        private readonly string _settingsPath;
        private readonly ISerializer _serializer;
        private readonly IDeserializer _deserializer;

        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static SettingsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new SettingsManager();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 設定変更時のイベント
        /// </summary>
        public event EventHandler? SettingsChanged;

        /// <summary>
        /// 現在の設定
        /// </summary>
        public AppSettings Current
        {
            get { return _settings; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private SettingsManager()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "KeyOverlayFPS");
            
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            
            _settingsPath = Path.Combine(appFolder, "settings.yaml");
            
            _serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            
            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            
            _settings = new AppSettings();
        }

        /// <summary>
        /// 設定を読み込む
        /// </summary>
        public void Load()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    Logger.Info($"設定ファイルが存在、読み込み中: {_settingsPath}");
                    var yaml = File.ReadAllText(_settingsPath);
                    _settings = _deserializer.Deserialize<AppSettings>(yaml) ?? new AppSettings();
                    Logger.Info("設定デシリアライズ完了");
                }
                else
                {
                    Logger.Info($"設定ファイルが存在しない、65%キーボードYAMLから初期設定を読み込み: {_settingsPath}");
                    // 65%キーボードYAMLから初期設定を作成
                    _settings = CreateSettingsFromLayout();
                    Save();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("設定読み込みでエラーが発生", ex);
                throw;
            }
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        public void Save()
        {
            try
            {
                var yaml = _serializer.Serialize(_settings);
                File.WriteAllText(_settingsPath, yaml);
                Logger.Info("設定保存完了");
            }
            catch (Exception ex)
            {
                Logger.Error("設定保存でエラーが発生", ex);
                throw;
            }
        }

        /// <summary>
        /// 65%キーボードYAMLから初期設定を作成
        /// </summary>
        private AppSettings CreateSettingsFromLayout()
        {
            try
            {
                var layoutManager = new LayoutManager();
                layoutManager.LoadLayout(Input.KeyboardProfile.FullKeyboard65);
                var layout = layoutManager.CurrentLayout;

                var settings = new AppSettings
                {
                    // ウィンドウ位置のみデフォルト値使用
                    WindowLeft = ApplicationConstants.UILayout.CanvasMargin * 12.5, // 100
                    WindowTop = ApplicationConstants.UILayout.CanvasMargin * 12.5,  // 100
                    IsTopmost = true,
                    
                    // レイアウトから色設定を取得
                    BackgroundColor = layout?.Global?.BackgroundColor ?? "Transparent",
                    ForegroundColor = layout?.Global?.ForegroundColor ?? "White", 
                    HighlightColor = layout?.Global?.HighlightColor ?? "Green",
                    
                    // その他はデフォルト値
                    DisplayScale = 1.0,
                    IsMouseVisible = true,
                    CurrentProfile = "FullKeyboard65",
                    IsMouseTrackingEnabled = true
                };

                Logger.Info("65%キーボードYAMLから初期設定を作成完了");
                return settings;
            }
            catch (Exception ex)
            {
                Logger.Error("65%キーボードYAMLからの設定作成でエラーが発生、デフォルト設定を使用", ex);
                return new AppSettings();
            }
        }

        /// <summary>
        /// ウィンドウ位置を更新
        /// </summary>
        public void UpdateWindowPosition(double left, double top)
        {
            _settings.WindowLeft = left;
            _settings.WindowTop = top;
            Save();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 最前面表示を切り替え
        /// </summary>
        public void ToggleTopmost()
        {
            _settings.IsTopmost = !_settings.IsTopmost;
            Save();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 背景色を設定
        /// </summary>
        public void SetBackgroundColor(Color color, bool transparent)
        {
            _settings.BackgroundColor = transparent ? "Transparent" : GetColorNameFromColor(color);
            Save();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 前景色を設定
        /// </summary>
        public void SetForegroundColor(Color color)
        {
            _settings.ForegroundColor = GetColorNameFromColor(color);
            Save();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// ハイライト色を設定
        /// </summary>
        public void SetHighlightColor(Color color)
        {
            _settings.HighlightColor = GetColorNameFromColor(color);
            Save();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 表示スケールを設定
        /// </summary>
        public void SetDisplayScale(double scale)
        {
            _settings.DisplayScale = scale;
            Save();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// マウス可視性を切り替え
        /// </summary>
        public void ToggleMouseVisibility()
        {
            _settings.IsMouseVisible = !_settings.IsMouseVisible;
            Save();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// プロファイルを設定
        /// </summary>
        public void SetCurrentProfile(string profile)
        {
            _settings.CurrentProfile = profile;
            Save();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 色名からBrushを取得
        /// </summary>
        public Brush GetBrushFromColorName(string colorName)
        {
            return BrushFactory.CreateBrushFromString(colorName, Brushes.White);
        }

        /// <summary>
        /// Brushから色名を取得
        /// </summary>
        public string GetColorNameFromBrush(Brush brush)
        {
            if (brush is SolidColorBrush solidBrush)
            {
                return GetColorNameFromColor(solidBrush.Color);
            }
            return "White";
        }

        /// <summary>
        /// Colorから色名を取得
        /// </summary>
        private string GetColorNameFromColor(Color color)
        {
            // よく使用される色の名前を返す
            if (ColorsAreEqual(color, System.Windows.Media.Colors.White)) return "White";
            if (ColorsAreEqual(color, System.Windows.Media.Colors.Red)) return "Red";
            if (ColorsAreEqual(color, System.Windows.Media.Colors.Green)) return "Green";
            if (ColorsAreEqual(color, System.Windows.Media.Colors.Blue)) return "Blue";
            if (ColorsAreEqual(color, System.Windows.Media.Colors.Yellow)) return "Yellow";
            if (ColorsAreEqual(color, System.Windows.Media.Colors.Orange)) return "Orange";
            if (ColorsAreEqual(color, System.Windows.Media.Colors.Purple)) return "Purple";
            if (ColorsAreEqual(color, System.Windows.Media.Colors.Pink)) return "Pink";
            if (ColorsAreEqual(color, ApplicationConstants.Colors.DefaultHighlight)) return "LimeGreen";
            
            // RGB形式で返す
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        /// <summary>
        /// 色の比較（アルファ値を無視）
        /// </summary>
        private static bool ColorsAreEqual(Color color1, Color color2)
        {
            return color1.R == color2.R && color1.G == color2.G && color1.B == color2.B;
        }
    }
}