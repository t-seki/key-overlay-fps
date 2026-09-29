using System;
using System.IO;
using System.Windows.Media;
using YamlDotNet.Serialization;
using KeyOverlayFPS.Utils;
using KeyOverlayFPS.Constants;
using KeyOverlayFPS.Layout;
using KeyOverlayFPS.UI;

namespace KeyOverlayFPS.Settings
{
    /// <summary>
    /// 統一された設定管理クラス
    /// </summary>
    public class SettingsManager
    {
        private AppSettings _settings;
        private readonly string _settingsPath;
        private readonly ISerializer _serializer;
        private readonly IDeserializer _deserializer;
        private readonly Action<string, string> _moveFile;

        /// <summary>
        /// 壊れた設定ファイルの退避に失敗し、元のファイルを守るために保存を止めているかどうか
        /// </summary>
        private bool _saveSuppressed;

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
        /// 直近の <see cref="Load"/> で壊れた設定ファイルを検出し、既定値で復旧したときの結果。
        /// 復旧していなければ null
        /// </summary>
        public SettingsRecoveryInfo? Recovery { get; private set; }

        /// <summary>
        /// コンストラクタ。設定ディレクトリは %APPDATA%\KeyOverlayFPS を使う
        /// </summary>
        public SettingsManager()
            : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KeyOverlayFPS"))
        {
        }

        /// <summary>
        /// コンストラクタ。設定ディレクトリを指定する（テストでの隔離用）
        /// </summary>
        /// <param name="settingsDirectory">settings.yaml を置くディレクトリ</param>
        public SettingsManager(string settingsDirectory)
            : this(settingsDirectory, (source, destination) => File.Move(source, destination, overwrite: true))
        {
        }

        /// <summary>
        /// コンストラクタ。設定ディレクトリと、壊れた設定ファイルを退避する処理を指定する（テストでの差し替え用）
        /// </summary>
        /// <param name="settingsDirectory">settings.yaml を置くディレクトリ</param>
        /// <param name="moveFile">ファイルを移動する処理（移動元, 移動先）。移動先が既にあれば上書きすること</param>
        public SettingsManager(string settingsDirectory, Action<string, string> moveFile)
        {
            _moveFile = moveFile ?? throw new ArgumentNullException(nameof(moveFile));
            var appFolder = settingsDirectory;

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            
            _settingsPath = Path.Combine(appFolder, "settings.yaml");
            
            _serializer = YamlSerializerFactory.CreateSettingsSerializer();
            _deserializer = YamlSerializerFactory.CreateSettingsDeserializer();
            
            _settings = new AppSettings();
        }

        /// <summary>
        /// 設定を読み込む
        /// </summary>
        public void Load()
        {
            Recovery = null;

            try
            {
                if (File.Exists(_settingsPath))
                {
                    Logger.Info($"設定ファイルが存在、読み込み中: {_settingsPath}");
                    var yaml = File.ReadAllText(_settingsPath);

                    AppSettings? loaded;
                    try
                    {
                        loaded = _deserializer.Deserialize<AppSettings>(yaml);
                    }
                    catch (Exception ex)
                    {
                        // 構文エラーや型の不一致など。退避して既定値で起動する
                        Logger.Error("設定ファイルのデシリアライズに失敗、退避して既定値で復旧する", ex);
                        RecoverFromCorruptedFile();
                        return;
                    }

                    _settings = loaded ?? new AppSettings();
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
        /// 壊れた設定ファイルを settings.yaml.bak に退避し、既定値で設定を作り直す。
        /// 退避に失敗したときは元のファイルを上書きしないよう、既定値をメモリ上だけで使い、以後の保存を止める
        /// </summary>
        private void RecoverFromCorruptedFile()
        {
            var backupPath = _settingsPath + ".bak";

            try
            {
                _moveFile(_settingsPath, backupPath);
            }
            catch (Exception ex)
            {
                Logger.Error($"壊れた設定ファイルの退避に失敗、既定値をメモリ上だけで使い保存しない: {backupPath}", ex);
                _saveSuppressed = true;
                _settings = CreateSettingsFromLayout();
                Recovery = new SettingsRecoveryInfo(_settingsPath, backupPath, backupSucceeded: false);
                return;
            }

            Logger.Info($"壊れた設定ファイルを退避: {backupPath}");
            Recovery = new SettingsRecoveryInfo(_settingsPath, backupPath, backupSucceeded: true);

            // ファイルが無いときの初回起動と同じ経路
            _settings = CreateSettingsFromLayout();
            Save();
        }

        /// <summary>
        /// 設定を保存する。壊れた設定ファイルの退避に失敗したセッションでは何もしない
        /// </summary>
        public void Save()
        {
            if (_saveSuppressed)
            {
                Logger.Info("壊れた設定ファイルを退避できなかったため、設定を保存しない");
                return;
            }

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
        /// 設定を更新して保存・通知する共通メソッド
        /// </summary>
        /// <param name="updateAction">設定を更新する処理</param>
        private void UpdateSettingsAndNotify(Action updateAction)
        {
            updateAction();
            Save();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 65%キーボードYAMLから初期設定を作成
        /// </summary>
        private AppSettings CreateSettingsFromLayout()
        {
            try
            {
                var layoutManager = new LayoutManager();
                layoutManager.LoadLayout(KeyboardProfile.FullKeyboard65);
                var layout = layoutManager.CurrentLayout;

                if (layout == null)
                {
                    Logger.Warning("レイアウトがnullのため、デフォルト設定を使用");
                    return new AppSettings();
                }

                var settings = AppSettings.CreateFromLayout(layout);
                
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
            UpdateSettingsAndNotify(() =>
            {
                _settings.WindowLeft = left;
                _settings.WindowTop = top;
            });
        }

        /// <summary>
        /// 最前面表示を切り替え
        /// </summary>
        public void ToggleTopmost()
        {
            UpdateSettingsAndNotify(() => _settings.IsTopmost = !_settings.IsTopmost);
        }

        /// <summary>
        /// 背景色を設定
        /// </summary>
        public void SetBackgroundColor(Color color, bool transparent)
        {
            UpdateSettingsAndNotify(() => 
                _settings.BackgroundColor = transparent ? "Transparent" : GetColorNameFromColor(color));
        }

        /// <summary>
        /// 前景色を設定
        /// </summary>
        public void SetForegroundColor(Color color)
        {
            UpdateSettingsAndNotify(() => _settings.ForegroundColor = GetColorNameFromColor(color));
        }

        /// <summary>
        /// ハイライト色を設定
        /// </summary>
        public void SetHighlightColor(Color color)
        {
            UpdateSettingsAndNotify(() => _settings.HighlightColor = GetColorNameFromColor(color));
        }

        /// <summary>
        /// 表示スケールを設定
        /// </summary>
        public void SetDisplayScale(double scale)
        {
            UpdateSettingsAndNotify(() => _settings.DisplayScale = scale);
        }

        /// <summary>
        /// マウス可視性を切り替え
        /// </summary>
        public void ToggleMouseVisibility()
        {
            UpdateSettingsAndNotify(() => _settings.IsMouseVisible = !_settings.IsMouseVisible);
        }

        /// <summary>
        /// プロファイルを設定
        /// </summary>
        public void SetCurrentProfile(string profile)
        {
            UpdateSettingsAndNotify(() => _settings.CurrentProfile = profile);
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

    /// <summary>
    /// 壊れた設定ファイルを検出して既定値で復旧したときの結果
    /// </summary>
    public sealed class SettingsRecoveryInfo
    {
        /// <summary>
        /// 設定ファイルのパス
        /// </summary>
        public string SettingsPath { get; }

        /// <summary>
        /// 壊れた設定ファイルの退避先のパス
        /// </summary>
        public string BackupPath { get; }

        /// <summary>
        /// 退避に成功したかどうか。失敗したときは元のファイルがそのまま残り、このセッションでは設定を保存しない
        /// </summary>
        public bool BackupSucceeded { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="settingsPath">設定ファイルのパス</param>
        /// <param name="backupPath">退避先のパス</param>
        /// <param name="backupSucceeded">退避に成功したかどうか</param>
        public SettingsRecoveryInfo(string settingsPath, string backupPath, bool backupSucceeded)
        {
            SettingsPath = settingsPath;
            BackupPath = backupPath;
            BackupSucceeded = backupSucceeded;
        }
    }
}
