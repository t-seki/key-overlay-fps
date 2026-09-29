using System;
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace KeyOverlayFPS.Input
{
    /// <summary>
    /// YAMLでVirtual Key Code定数名をサポートするコンバーター（読み込み専用）
    /// </summary>
    public class VirtualKeyCodeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            // int型でかつVirtualKeyプロパティでのみ適用
            // 注意: この判定は限定的で、完全ではない
            return type == typeof(int);
        }

        public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            var scalar = parser.Consume<Scalar>();
            var value = scalar.Value;

            // 定数名の場合のみ特別処理 (VK_で始まる)
            if (value.StartsWith("VK_", StringComparison.OrdinalIgnoreCase))
            {
                // VirtualKeyCodesクラスから定数値を取得
                var fieldInfo = typeof(VirtualKeyCodes).GetField(value, BindingFlags.Public | BindingFlags.Static);
                if (fieldInfo != null && fieldInfo.FieldType == typeof(int))
                {
                    return fieldInfo.GetValue(null) ?? 0;
                }
                throw new YamlException($"未知のVirtual Key Code定数: {value}");
            }

            // それ以外は標準の整数パースに委譲
            // 16進数対応
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return Convert.ToInt32(value, 16);
            }

            // 10進数
            if (int.TryParse(value, out int result))
            {
                return result;
            }

            // 標準パースに失敗した場合のみエラー
            throw new YamlException($"無効な整数値: {value}");
        }

        /// <summary>
        /// 書き出しには対応しない
        /// </summary>
        /// <remarks>
        /// レイアウトを YAML に書き出す経路は無い。<see cref="IYamlTypeConverter"/> の実装上メソッドは消せないため、呼ばれたら例外を投げる
        /// </remarks>
        /// <exception cref="NotSupportedException">常に投げる</exception>
        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            throw new NotSupportedException("VirtualKeyCodeConverter は YAML への書き出しに対応していません");
        }
    }
}