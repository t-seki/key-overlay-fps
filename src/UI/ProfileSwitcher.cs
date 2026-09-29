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
        private readonly CanvasRebuilder _canvasRebuilder;
        private readonly MainWindow _mainWindow;
        private readonly Action _updateMousePositionsAction;
        private readonly Action _updateMenuStateAction;
        private readonly Action _applyDisplaySettingsAction;

        public ProfileSwitcher(
            ProfileManager profileManager,
            CanvasRebuilder canvasRebuilder,
            MainWindow mainWindow,
            Action updateMousePositionsAction,
            Action updateMenuStateAction,
            Action applyDisplaySettingsAction)
        {
            _profileManager = profileManager ?? throw new ArgumentNullException(nameof(profileManager));
            _canvasRebuilder = canvasRebuilder ?? throw new ArgumentNullException(nameof(canvasRebuilder));
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
            _updateMousePositionsAction = updateMousePositionsAction ?? throw new ArgumentNullException(nameof(updateMousePositionsAction));
            _updateMenuStateAction = updateMenuStateAction ?? throw new ArgumentNullException(nameof(updateMenuStateAction));
            _applyDisplaySettingsAction = applyDisplaySettingsAction ?? throw new ArgumentNullException(nameof(applyDisplaySettingsAction));
        }

        /// <summary>
        /// プロファイルを切り替える
        /// </summary>
        public void SwitchProfile(KeyboardProfile profile)
        {
            Logger.Info($"プロファイル切り替え開始: {profile}");
            
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
            
            // 再構築に成功してから現在のプロファイルを更新・保存する（保存は 1 回だけ）
            _profileManager.SwitchProfile(profile);

            // 再構築で失われた表示設定（背景色・スケール・文字色・マウス表示）を再適用
            _applyDisplaySettingsAction();

            // 追加の更新処理を実行
            _updateMousePositionsAction();
            _updateMenuStateAction();
            
            Logger.Info($"プロファイル切り替え完了: {profile}");
        }
    }
}