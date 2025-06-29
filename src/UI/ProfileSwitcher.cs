using System;
using KeyOverlayFPS.Input;
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
        private readonly KeyboardInputHandler _keyboardHandler;
        private readonly ISettingsService _settingsService;
        private readonly Action _applyLayoutAction;
        private readonly Action _updateMousePositionsAction;
        private readonly Action _updateMenuStateAction;

        public ProfileSwitcher(
            LayoutManager layoutManager,
            KeyboardInputHandler keyboardHandler,
            ISettingsService settingsService,
            Action applyLayoutAction,
            Action updateMousePositionsAction,
            Action updateMenuStateAction)
        {
            _layoutManager = layoutManager ?? throw new ArgumentNullException(nameof(layoutManager));
            _keyboardHandler = keyboardHandler ?? throw new ArgumentNullException(nameof(keyboardHandler));
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
            _keyboardHandler.CurrentProfile = profile;
            
            // プロファイルに対応する新しいYAMLファイルを読み込み
            try
            {
                _layoutManager.LoadLayout(profile);
            }
            catch (Exception ex)
            {
                Logger.Error($"レイアウトファイル読み込みエラー: {ex.Message}", ex);
            }
            
            _applyLayoutAction();
            _updateMousePositionsAction();
            _settingsService.SetCurrentProfile(profile.ToString());
            _updateMenuStateAction();
        }
    }
}