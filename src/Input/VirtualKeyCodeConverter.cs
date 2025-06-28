using System;
using System.Globalization;
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace KeyOverlayFPS.Input
{
    /// <summary>
    /// YAMLでVirtual Key Code定数名をサポートするコンバーター
    /// </summary>
    public class VirtualKeyCodeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            // int型でかつVirtualKeyプロパティでのみ適用
            // 注意: この判定は限定的で、完全ではない
            return type == typeof(int);
        }

        public object ReadYaml(IParser parser, Type type)
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

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            if (value is int intValue)
            {
                // Virtual Key Codeかどうかの判定
                // VirtualKeyCodesクラスに定義されている値の場合は定数名で出力
                var constantName = GetVirtualKeyConstantName(intValue);
                if (!string.IsNullOrEmpty(constantName))
                {
                    emitter.Emit(new Scalar(constantName));
                }
                else
                {
                    // 標準の整数出力
                    emitter.Emit(new Scalar(intValue.ToString()));
                }
            }
            else
            {
                emitter.Emit(new Scalar("0"));
            }
        }

        /// <summary>
        /// Virtual Key Code値から定数名を取得
        /// </summary>
        private static string? GetVirtualKeyConstantName(int value)
        {
            var fields = typeof(VirtualKeyCodes).GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(int))
                {
                    var fieldValue = field.GetValue(null);
                    if (fieldValue != null && (int)fieldValue == value)
                    {
                        return field.Name;
                    }
                }
            }
            return null;
        }
    }
}