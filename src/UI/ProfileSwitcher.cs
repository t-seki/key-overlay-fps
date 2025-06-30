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
        private readonly LayoutManager _layoutManager;
        private readonly ProfileManager _profileManager;
        private readonly SettingsManager _settingsService;
        private readonly Action _applyLayoutAction;
        private readonly Action _updateMousePositionsAction;
        private readonly Action _updateMenuStateAction;

        public ProfileSwitcher(
            LayoutManager layoutManager,
            ProfileManager profileManager,
            SettingsManager settingsService,
            Action applyLayoutAction,
            Action updateMousePositionsAction,
            Action updateMenuStateAction)
        {
            _layoutManager = layoutManager ?? throw new ArgumentNullException(nameof(layoutManager));
            _profileManager = profileManager ?? throw new ArgumentNullException(nameof(profileManager));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _applyLayoutAction = applyLayoutAction ?? throw new ArgumentNullException(nameof(applyLayoutAction));
            _updateMousePositionsAction = updateMousePositionsAction ?? throw new ArgumentNullException(nameof(updateMousePositionsAction));
            _updateMenuStateAction = updateMenuStateAction ?? throw new ArgumentNullException(nameof(updateMenuStateAction));
        }

        /// <summary>
        /// プロファイルを切り替える
        /// </summary>
        public void SwitchProfile(KeyboardProfile profile)
        {
            _profileManager.SwitchProfile(profile);
            
            // プロファイルに対応する新しいYAMLファイルを読み込み
            try
            {
                _layoutManager.LoadLayout(profile);
            }
            catch (Exception ex)
            {
                Logger.Error($"レイアウトファイル読み込みエラー: {ex.Message}", ex);
            }
            
            _settingsService.SetCurrentProfile(_profileManager.GetCurrentProfileName());
            
            _applyLayoutAction();
            _updateMousePositionsAction();
            _updateMenuStateAction();
        }
    }
}