using System;
using System.IO;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using KeyOverlayFPS.Utils;
using KeyOverlayFPS.Constants;
using KeyOverlayFPS.Layout;

namespace KeyOverlayFPS.Settings
{
    /// <summary>
    /// 簡素化された設定管理クラス
    /// </summary>
    public class SettingsManager
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
    }
}