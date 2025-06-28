using System;
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace KeyOverlayFPS.Input
{
    /// <summary>
    /// Virtual Key Codeを表すカスタム型
    /// </summary>
    public struct VirtualKeyCode : IEquatable<VirtualKeyCode>
    {
        public int Value { get; }

        public VirtualKeyCode(int value)
        {
            Value = value;
        }

        public static implicit operator int(VirtualKeyCode vkCode) => vkCode.Value;
        public static implicit operator VirtualKeyCode(int value) => new(value);

        public bool Equals(VirtualKeyCode other) => Value == other.Value;
        public override bool Equals(object? obj) => obj is VirtualKeyCode other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(VirtualKeyCode left, VirtualKeyCode right) => left.Equals(right);
        public static bool operator !=(VirtualKeyCode left, VirtualKeyCode right) => !left.Equals(right);
    }

    /// <summary>
    /// VirtualKeyCode専用のYAMLコンバーター
    /// </summary>
    public class VirtualKeyCodeYamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(VirtualKeyCode) || type == typeof(VirtualKeyCode?);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            var scalar = parser.Consume<Scalar>();
            var value = scalar.Value;

            // 定数名の場合 (VK_で始まる)
            if (value.StartsWith("VK_", StringComparison.OrdinalIgnoreCase))
            {
                // VirtualKeyCodesクラスから定数値を取得
                var fieldInfo = typeof(VirtualKeyCodes).GetField(value, BindingFlags.Public | BindingFlags.Static);
                if (fieldInfo != null && fieldInfo.FieldType == typeof(int))
                {
                    int intValue = (int)(fieldInfo.GetValue(null) ?? 0);
                    return new VirtualKeyCode(intValue);
                }
                throw new YamlException($"未知のVirtual Key Code定数: {value}");
            }

            // 数値の場合（16進数対応）
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return new VirtualKeyCode(Convert.ToInt32(value, 16));
            }

            // 10進数
            if (int.TryParse(value, out int result))
            {
                return new VirtualKeyCode(result);
            }

            throw new YamlException($"無効なVirtual Key Code値: {value}");
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            if (value is VirtualKeyCode vkCode)
            {
                // 定数名での出力を試行
                var constantName = GetVirtualKeyConstantName(vkCode.Value);
                if (!string.IsNullOrEmpty(constantName))
                {
                    emitter.Emit(new Scalar(constantName));
                }
                else
                {
                    // 定数名が見つからない場合は16進数で出力
                    emitter.Emit(new Scalar($"0x{vkCode.Value:X}"));
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