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
                try
                {
                    var profileName = _settingsService.Current.CurrentProfile;
                    if (!string.IsNullOrEmpty(profileName) && Enum.TryParse<KeyboardProfile>(profileName, out var profile))
                    {
                        return profile;
                    }
                }
                catch (Exception)
                {
                    // エラーの場合はデフォルト値を返す
                }
                return KeyboardProfile.FullKeyboard65;
            }
        }
        
        /// <summary>
        /// プロファイル変更イベント
        /// </summary>
        public event EventHandler<ProfileChangedEventArgs>? ProfileChanged;
        
        public ProfileManager(SettingsManager settingsService)
        {
            _settingsService = settingsService;
        }
        
        /// <summary>
        /// プロファイルを設定から復元
        /// </summary>
        /// <param name="profileName">設定から読み込んだプロファイル名</param>
        public void RestoreFromSettings(string profileName)
        {
            if (Enum.TryParse<KeyboardProfile>(profileName, out var profile))
            {
                SwitchProfile(profile);
            }
        }
        
        /// <summary>
        /// プロファイルを切り替え
        /// </summary>
        /// <param name="profile">新しいプロファイル</param>
        public void SwitchProfile(KeyboardProfile profile)
        {
            var oldProfile = CurrentProfile;
            _settingsService.SetCurrentProfile(profile.ToString());
            
            // プロファイルが実際に変更された場合のみイベントを発火
            if (oldProfile != profile)
            {
                ProfileChanged?.Invoke(this, new ProfileChangedEventArgs(profile));
            }
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
    
    /// <summary>
    /// プロファイル変更イベント引数
    /// </summary>
    public class ProfileChangedEventArgs : EventArgs
    {
        public KeyboardProfile NewProfile { get; }
        
        public ProfileChangedEventArgs(KeyboardProfile newProfile)
        {
            NewProfile = newProfile;
        }
    }
}