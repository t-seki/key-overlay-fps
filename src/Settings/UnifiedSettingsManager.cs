using System;
using System.IO;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace KeyOverlayFPS.Settings
{
    /// <summary>
    /// 統一設定管理クラス - 設定の読み込み、保存、イベント通知を管理
    /// </summary>
    public class UnifiedSettingsManager
    {
        private static UnifiedSettingsManager? _instance;
        private static readonly object _lock = new object();
        
        private UnifiedSettings _settings;
        private readonly string _settingsPath;
        private readonly ISerializer _serializer;
        private readonly IDeserializer _deserializer;

        /// <summary>
        /// 設定変更時のイベント
        /// </summary>
        public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static UnifiedSettingsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new UnifiedSettingsManager();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 現在の設定
        /// </summary>
        public UnifiedSettings Settings
        {
            get { return _settings; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private UnifiedSettingsManager()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "KeyOverlayFPS");
            
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            
            _settingsPath = Path.Combine(appFolder, "unified_settings.yaml");
            
            _serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            
            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            
            _settings = new UnifiedSettings();
        }

        /// <summary>
        /// 設定を読み込む
        /// </summary>
        public async Task LoadSettingsAsync()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var yaml = await File.ReadAllTextAsync(_settingsPath);
                    _settings = _deserializer.Deserialize<UnifiedSettings>(yaml) ?? new UnifiedSettings();
                    
                    // 設定検証
                    ValidateSettings();
                    
                    // 設定変更通知
                    OnSettingsChanged(new SettingsChangedEventArgs
                    {
                        ChangeType = SettingsChangeType.Loaded,
                        Category = "All"
                    });
                }
                else
                {
                    // デフォルト設定で保存
                    await SaveSettingsAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"設定読み込みエラー: {ex.Message}");
                
                // エラー時はデフォルト設定を使用
                _settings = new UnifiedSettings();
                await SaveSettingsAsync();
            }
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        public async Task SaveSettingsAsync()
        {
            try
            {
                var yaml = _serializer.Serialize(_settings);
                await File.WriteAllTextAsync(_settingsPath, yaml);
                
                OnSettingsChanged(new SettingsChangedEventArgs
                {
                    ChangeType = SettingsChangeType.Saved,
                    Category = "All"
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"設定保存エラー: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 設定を同期的に読み込む
        /// </summary>
        public void LoadSettings()
        {
            LoadSettingsAsync().Wait();
        }

        /// <summary>
        /// 設定を同期的に保存する
        /// </summary>
        public void SaveSettings()
        {
            SaveSettingsAsync().Wait();
        }

        /// <summary>
        /// ウィンドウ設定を更新
        /// </summary>
        public void UpdateWindowSettings(Action<WindowSettings> updateAction)
        {
            updateAction(_settings.Window);
            OnSettingsChanged(new SettingsChangedEventArgs
            {
                ChangeType = SettingsChangeType.Updated,
                Category = "Window"
            });
        }

        /// <summary>
        /// 表示設定を更新
        /// </summary>
        public void UpdateDisplaySettings(Action<DisplaySettings> updateAction)
        {
            updateAction(_settings.Display);
            OnSettingsChanged(new SettingsChangedEventArgs
            {
                ChangeType = SettingsChangeType.Updated,
                Category = "Display"
            });
        }

        /// <summary>
        /// 色設定を更新
        /// </summary>
        public void UpdateColorSettings(Action<ColorSettings> updateAction)
        {
            updateAction(_settings.Colors);
            OnSettingsChanged(new SettingsChangedEventArgs
            {
                ChangeType = SettingsChangeType.Updated,
                Category = "Colors"
            });
        }

        /// <summary>
        /// プロファイル設定を更新
        /// </summary>
        public void UpdateProfileSettings(Action<ProfileSettings> updateAction)
        {
            updateAction(_settings.Profile);
            OnSettingsChanged(new SettingsChangedEventArgs
            {
                ChangeType = SettingsChangeType.Updated,
                Category = "Profile"
            });
        }

        /// <summary>
        /// マウス設定を更新
        /// </summary>
        public void UpdateMouseSettings(Action<MouseSettings> updateAction)
        {
            updateAction(_settings.Mouse);
            OnSettingsChanged(new SettingsChangedEventArgs
            {
                ChangeType = SettingsChangeType.Updated,
                Category = "Mouse"
            });
        }

        /// <summary>
        /// レイアウト設定を更新
        /// </summary>
        public void UpdateLayoutSettings(Action<LayoutSettings> updateAction)
        {
            updateAction(_settings.Layout);
            OnSettingsChanged(new SettingsChangedEventArgs
            {
                ChangeType = SettingsChangeType.Updated,
                Category = "Layout"
            });
        }

        /// <summary>
        /// 設定を初期化
        /// </summary>
        public async Task ResetSettingsAsync()
        {
            _settings = new UnifiedSettings();
            await SaveSettingsAsync();
            
            OnSettingsChanged(new SettingsChangedEventArgs
            {
                ChangeType = SettingsChangeType.Reset,
                Category = "All"
            });
        }

        /// <summary>
        /// 設定をエクスポート
        /// </summary>
        public Task<string> ExportSettingsAsync()
        {
            return Task.FromResult(_serializer.Serialize(_settings));
        }

        /// <summary>
        /// 設定をインポート
        /// </summary>
        public async Task ImportSettingsAsync(string yaml)
        {
            try
            {
                var importedSettings = _deserializer.Deserialize<UnifiedSettings>(yaml);
                if (importedSettings != null)
                {
                    _settings = importedSettings;
                    ValidateSettings();
                    await SaveSettingsAsync();
                    
                    OnSettingsChanged(new SettingsChangedEventArgs
                    {
                        ChangeType = SettingsChangeType.Imported,
                        Category = "All"
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"設定インポートエラー: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 設定の検証
        /// </summary>
        private void ValidateSettings()
        {
            // 表示スケールの検証
            if (!AppConstants.DisplayScales.Contains(_settings.Display.Scale))
            {
                _settings.Display.Scale = 1.0;
            }
            
            // 色設定の検証
            if (!_settings.Colors.Definitions.ForegroundColors.ContainsKey(_settings.Colors.Foreground))
            {
                _settings.Colors.Foreground = "White";
            }
            
            if (!_settings.Colors.Definitions.HighlightColors.ContainsKey(_settings.Colors.Highlight))
            {
                _settings.Colors.Highlight = "Green";
            }
            
            if (!_settings.Colors.Definitions.BackgroundColors.ContainsKey(_settings.Colors.Background))
            {
                _settings.Colors.Background = "Transparent";
            }
            
            // プロファイル設定の検証
            if (!_settings.Profile.Available.Contains(_settings.Profile.Current))
            {
                _settings.Profile.Current = "FullKeyboard65";
            }
        }

        /// <summary>
        /// 設定変更イベントを発火
        /// </summary>
        private void OnSettingsChanged(SettingsChangedEventArgs e)
        {
            SettingsChanged?.Invoke(this, e);
        }
    }

    /// <summary>
    /// 設定変更イベント引数
    /// </summary>
    public class SettingsChangedEventArgs : EventArgs
    {
        public SettingsChangeType ChangeType { get; set; }
        public string Category { get; set; } = string.Empty;
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
    }

    /// <summary>
    /// 設定変更の種類
    /// </summary>
    public enum SettingsChangeType
    {
        Loaded,
        Saved,
        Updated,
        Reset,
        Imported,
        Exported
    }
}