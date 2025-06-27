using System;
using System.IO;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace KeyOverlayFPS.Settings
{
    /// <summary>
    /// 既存のAppSettings形式からUnifiedSettings形式への移行を行うクラス
    /// </summary>
    public class SettingsMigrator
    {
        private readonly string _oldSettingsPath;
        private readonly string _newSettingsPath;
        private readonly IDeserializer _deserializer;

        public SettingsMigrator()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "KeyOverlayFPS");
            
            _oldSettingsPath = Path.Combine(appFolder, "settings.yaml");
            _newSettingsPath = Path.Combine(appFolder, "unified_settings.yaml");
            
            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
        }

        /// <summary>
        /// 移行が必要かどうかを判定
        /// </summary>
        public bool IsMigrationNeeded()
        {
            return File.Exists(_oldSettingsPath) && !File.Exists(_newSettingsPath);
        }

        /// <summary>
        /// 旧設定形式から新設定形式への移行を実行
        /// </summary>
        public async Task<MigrationResult> MigrateAsync()
        {
            var result = new MigrationResult();
            
            try
            {
                if (!File.Exists(_oldSettingsPath))
                {
                    result.Success = false;
                    result.ErrorMessage = "旧設定ファイルが見つかりません";
                    return result;
                }

                // 旧設定を読み込み
                var oldSettingsYaml = await File.ReadAllTextAsync(_oldSettingsPath);
                var oldSettings = _deserializer.Deserialize<LegacyAppSettings>(oldSettingsYaml);
                
                if (oldSettings == null)
                {
                    result.Success = false;
                    result.ErrorMessage = "旧設定ファイルの読み込みに失敗しました";
                    return result;
                }

                // 新設定形式に変換
                var unifiedSettings = ConvertToUnifiedSettings(oldSettings);
                
                // 変換結果を検証
                var validationResult = SettingsValidator.ValidateUnifiedSettings(unifiedSettings);
                if (!validationResult.IsValid)
                {
                    result.Success = false;
                    result.ErrorMessage = $"変換後の設定が無効です: {validationResult.GetSummary()}";
                    result.ValidationErrors = validationResult.Errors;
                    return result;
                }

                // 新設定として保存
                var settingsManager = UnifiedSettingsManager.Instance;
                await settingsManager.ImportSettingsAsync(
                    new SerializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build()
                        .Serialize(unifiedSettings)
                );

                // 旧設定ファイルをバックアップとしてリネーム
                var backupPath = _oldSettingsPath + ".backup";
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
                File.Move(_oldSettingsPath, backupPath);

                result.Success = true;
                result.BackupPath = backupPath;
                result.MigratedSettings = unifiedSettings;
                
                if (validationResult.HasWarnings)
                {
                    result.ValidationWarnings = validationResult.Warnings;
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"移行処理中にエラーが発生しました: {ex.Message}";
                result.Exception = ex;
                return result;
            }
        }

        /// <summary>
        /// 旧AppSettingsを新UnifiedSettingsに変換
        /// </summary>
        private UnifiedSettings ConvertToUnifiedSettings(LegacyAppSettings legacy)
        {
            var unified = new UnifiedSettings();

            // ウィンドウ設定の変換
            unified.Window.Left = legacy.WindowLeft;
            unified.Window.Top = legacy.WindowTop;
            unified.Window.IsTopmost = legacy.IsTopmost;
            
            // 表示設定の変換
            unified.Display.Scale = legacy.DisplayScale;
            unified.Display.IsMouseVisible = legacy.IsMouseVisible;
            
            // 色設定の変換
            unified.Colors.Background = legacy.BackgroundColor ?? "Transparent";
            unified.Colors.Foreground = legacy.ForegroundColor ?? "White";
            unified.Colors.Highlight = legacy.HighlightColor ?? "Green";
            
            // プロファイル設定の変換
            if (!string.IsNullOrEmpty(legacy.CurrentProfile))
            {
                unified.Profile.Current = legacy.CurrentProfile;
            }

            // マウス設定の変換（旧設定から推定）
            unified.Mouse.IsVisible = legacy.IsMouseVisible;
            unified.Mouse.IsTrackingEnabled = true; // デフォルト値
            
            return unified;
        }

        /// <summary>
        /// 移行のロールバック
        /// </summary>
        public Task<bool> RollbackMigrationAsync()
        {
            try
            {
                var backupPath = _oldSettingsPath + ".backup";
                
                if (!File.Exists(backupPath))
                {
                    return Task.FromResult(false);
                }

                // 新設定ファイルを削除
                if (File.Exists(_newSettingsPath))
                {
                    File.Delete(_newSettingsPath);
                }

                // バックアップファイルを元に戻す
                File.Move(backupPath, _oldSettingsPath);
                
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ロールバック失敗: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// バックアップファイルの削除
        /// </summary>
        public void CleanupBackup()
        {
            try
            {
                var backupPath = _oldSettingsPath + ".backup";
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"バックアップ削除失敗: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 旧AppSettings形式（MainWindow.xaml.csから抽出）
    /// </summary>
    public class LegacyAppSettings
    {
        public string CurrentProfile { get; set; } = "FullKeyboard65";
        public double DisplayScale { get; set; } = 1.0;
        public bool IsMouseVisible { get; set; } = true;
        public bool IsTopmost { get; set; } = true;
        public string BackgroundColor { get; set; } = "Transparent";
        public string ForegroundColor { get; set; } = "White";
        public string HighlightColor { get; set; } = "Green";
        public double WindowLeft { get; set; } = 100;
        public double WindowTop { get; set; } = 100;
    }

    /// <summary>
    /// 移行結果クラス
    /// </summary>
    public class MigrationResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
        public string BackupPath { get; set; } = string.Empty;
        public UnifiedSettings? MigratedSettings { get; set; }
        public System.Collections.Generic.List<string> ValidationErrors { get; set; } = new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<string> ValidationWarnings { get; set; } = new System.Collections.Generic.List<string>();

        public string GetSummary()
        {
            if (Success)
            {
                var summary = "移行成功";
                if (ValidationWarnings.Count > 0)
                {
                    summary += $" (警告: {ValidationWarnings.Count}件)";
                }
                return summary;
            }
            else
            {
                return $"移行失敗: {ErrorMessage}";
            }
        }

        public override string ToString()
        {
            var result = GetSummary() + Environment.NewLine;
            
            if (!string.IsNullOrEmpty(BackupPath))
            {
                result += $"バックアップ: {BackupPath}" + Environment.NewLine;
            }
            
            if (ValidationErrors.Count > 0)
            {
                result += "エラー:" + Environment.NewLine;
                foreach (var error in ValidationErrors)
                {
                    result += $"  - {error}" + Environment.NewLine;
                }
            }
            
            if (ValidationWarnings.Count > 0)
            {
                result += "警告:" + Environment.NewLine;
                foreach (var warning in ValidationWarnings)
                {
                    result += $"  - {warning}" + Environment.NewLine;
                }
            }
            
            return result;
        }
    }
}