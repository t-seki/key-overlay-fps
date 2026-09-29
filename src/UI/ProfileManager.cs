using System;
using KeyOverlayFPS.Layout;
using KeyOverlayFPS.Settings;

namespace KeyOverlayFPS.UI
{
    /// <summary>
    /// プロファイル管理クラス - キーボードプロファイルの統一管理
    /// </summary>
    public class ProfileManager
    {
        private readonly SettingsManager _settingsService;
        
        /// <summary>
        /// 現在のキーボードプロファイル
        /// </summary>
        public KeyboardProfile CurrentProfile
        {
            get
            {
                var profileName = _settingsService.Current.CurrentProfile;
                if (!string.IsNullOrEmpty(profileName) && Enum.TryParse<KeyboardProfile>(profileName, out var profile))
                {
                    return profile;
                }
                return KeyboardProfile.FullKeyboard65;
            }
        }
        
        public ProfileManager(SettingsManager settingsService)
        {
            _settingsService = settingsService;
        }
        
        /// <summary>
        /// プロファイルを切り替え
        /// </summary>
        /// <param name="profile">新しいプロファイル</param>
        public void SwitchProfile(KeyboardProfile profile)
        {
            _settingsService.SetCurrentProfile(profile.ToString());
        }
        
        /// <summary>
        /// 現在のプロファイルを文字列として取得（設定保存用）
        /// </summary>
        /// <returns>プロファイル名の文字列</returns>
        public string GetCurrentProfileName()
        {
            return CurrentProfile.ToString();
        }
        
        /// <summary>
        /// 指定されたプロファイルが現在選択されているかを判定
        /// </summary>
        /// <param name="profile">判定するプロファイル</param>
        /// <returns>選択されている場合はtrue</returns>
        public bool IsCurrentProfile(KeyboardProfile profile)
        {
            return CurrentProfile == profile;
        }
    }
}
