using System;
using System.IO;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using KeyOverlayFPS.Utils;

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
                    Logger.Info($"設定ファイルが存在しない、デフォルト設定で保存: {_settingsPath}");
                    // デフォルト設定で保存
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
    }
}