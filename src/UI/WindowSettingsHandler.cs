using System;
using KeyOverlayFPS.Layout;
using KeyOverlayFPS.UI;

namespace KeyOverlayFPS.UI
{
    /// <summary>
    /// ウィンドウ設定に関する処理を担当するクラス
    /// </summary>
    public class WindowSettingsHandler
    {
        private readonly MainWindowSettings _settings;
        private readonly ProfileManager _profileManager;

        public WindowSettingsHandler(MainWindowSettings settings, ProfileManager profileManager)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _profileManager = profileManager ?? throw new ArgumentNullException(nameof(profileManager));
        }

        /// <summary>
        /// ProfileSwitcherを設定（後から注入）
        /// </summary>
        public ProfileSwitcher? ProfileSwitcher { get; set; }

        /// <summary>
        /// 最前面表示を切り替え
        /// </summary>
        public void ToggleTopmost()
        {
            _settings.ToggleTopmost();
        }

        /// <summary>
        /// マウス表示状態を切り替え
        /// </summary>
        public void ToggleMouseVisibility()
        {
            _settings.ToggleMouseVisibility();
            UpdateMouseVisibility();
        }

        /// <summary>
        /// マウス表示状態の変更を反映
        /// </summary>
        private void UpdateMouseVisibility()
        {
            // マウス表示状態の変更をレイアウトに反映するため、現在のプロファイルでキャンバスを再構築
            ProfileSwitcher?.SwitchProfile(_profileManager.CurrentProfile);
        }

        /// <summary>
        /// プロファイルを切り替え
        /// </summary>
        public void SwitchProfile(KeyboardProfile profile)
        {
            ProfileSwitcher?.SwitchProfile(profile);
        }
    }
}