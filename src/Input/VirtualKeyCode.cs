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

}