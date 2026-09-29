using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using KeyOverlayFPS.Input;

namespace KeyOverlayFPS.Utils
{
    /// <summary>
    /// YAML シリアライザー・デシリアライザーの共通ファクトリークラス
    /// プロジェクト全体で統一されたYAML設定を提供
    /// </summary>
    public static class YamlSerializerFactory
    {
        /// <summary>
        /// 設定ファイル用のシリアライザーを作成
        /// AppSettings など基本的な設定ファイル用
        /// </summary>
        public static ISerializer CreateSettingsSerializer()
        {
            return new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
        }

        /// <summary>
        /// 設定ファイル用のデシリアライザーを作成
        /// AppSettings など基本的な設定ファイル用。
        /// フィールドの削除・改名後も古い設定ファイルを読めるよう、未知のキーは無視する
        /// </summary>
        public static IDeserializer CreateSettingsDeserializer()
        {
            return new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
        }

        /// <summary>
        /// レイアウトファイル用のデシリアライザーを作成
        /// LayoutConfig など仮想キーコードを含むレイアウトファイル用。
        /// フィールドの削除後も旧版のレイアウトファイルを読めるよう、未知のキーは無視する
        /// </summary>
        public static IDeserializer CreateLayoutDeserializer()
        {
            return new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .WithTypeConverter(new VirtualKeyCodeConverter())
                .Build();
        }
    }
}