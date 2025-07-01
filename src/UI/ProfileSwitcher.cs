using System;
using KeyOverlayFPS.Layout;
using KeyOverlayFPS.Settings;
using KeyOverlayFPS.Utils;

namespace KeyOverlayFPS.UI
{
    /// <summary>
    /// プロファイル切り替えロジックを管理するクラス
    /// </summary>
    public class ProfileSwitcher
    {
        private readonly ProfileManager _profileManager;
        private readonly SettingsManager _settingsService;
        private readonly CanvasRebuilder _canvasRebuilder;
        private readonly MainWindow _mainWindow;
        private readonly Action _updateMousePositionsAction;
        private readonly Action _updateMenuStateAction;

        public ProfileSwitcher(
            ProfileManager profileManager,
            SettingsManager settingsService,
            CanvasRebuilder canvasRebuilder,
            MainWindow mainWindow,
            Action updateMousePositionsAction,
            Action updateMenuStateAction)
        {
            _profileManager = profileManager ?? throw new ArgumentNullException(nameof(profileManager));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _canvasRebuilder = canvasRebuilder ?? throw new ArgumentNullException(nameof(canvasRebuilder));
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
            _updateMousePositionsAction = updateMousePositionsAction ?? throw new ArgumentNullException(nameof(updateMousePositionsAction));
            _updateMenuStateAction = updateMenuStateAction ?? throw new ArgumentNullException(nameof(updateMenuStateAction));
        }

        /// <summary>
        /// プロファイルを切り替える
        /// </summary>
        public void SwitchProfile(KeyboardProfile profile)
        {
            Logger.Info($"プロファイル切り替え開始: {profile}");
            
            _profileManager.SwitchProfile(profile);
            
            // CanvasRebuilderを使用してキャンバスを完全に再構築（スケール適用を含む）
            try
            {
                _canvasRebuilder.RebuildCanvas(_mainWindow, profile);
            }
            catch (Exception ex)
            {
                Logger.Error($"キャンバス再構築エラー: {ex.Message}", ex);
                throw;
            }
            
            // 設定を保存
            _settingsService.SetCurrentProfile(_profileManager.GetCurrentProfileName());
            
            // 追加の更新処理を実行
            _updateMousePositionsAction();
            _updateMenuStateAction();
            
            Logger.Info($"プロファイル切り替え完了: {profile}");
        }
    }
}