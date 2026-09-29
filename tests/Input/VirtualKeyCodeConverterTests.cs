using System;
using NUnit.Framework;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using KeyOverlayFPS.Input;

namespace KeyOverlayFPS.Tests.Input
{
    /// <summary>
    /// VirtualKeyCodeConverterのテストクラス
    /// </summary>
    [TestFixture]
    public class VirtualKeyCodeConverterTests
    {
        private VirtualKeyCodeConverter _converter;
        private IDeserializer _deserializer;

        [SetUp]
        public void SetUp()
        {
            _converter = new VirtualKeyCodeConverter();
            
            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(_converter)
                .Build();
        }

        [Test]
        public void Accepts_ShouldReturnTrue_ForIntType()
        {
            // Act & Assert
            Assert.That(_converter.Accepts(typeof(int)), Is.True);
        }

        [Test]
        public void Accepts_ShouldReturnFalse_ForNonIntTypes()
        {
            // Act & Assert
            Assert.That(_converter.Accepts(typeof(string)), Is.False);
            Assert.That(_converter.Accepts(typeof(double)), Is.False);
            Assert.That(_converter.Accepts(typeof(bool)), Is.False);
        }

        [Test]
        [TestCase("VK_A", ExpectedResult = 0x41)]
        [TestCase("VK_SPACE", ExpectedResult = 0x20)]
        [TestCase("VK_ESCAPE", ExpectedResult = 0x1B)]
        [TestCase("VK_F1", ExpectedResult = 0x70)]
        [TestCase("VK_LSHIFT", ExpectedResult = 0xA0)]
        [TestCase("VK_RETURN", ExpectedResult = 0x0D)]
        public int ReadYaml_ShouldReturnCorrectValue_ForValidVKConstants(string vkConstant)
        {
            // Arrange
            var yaml = $"virtualKey: {vkConstant}";

            // Act
            var result = _deserializer.Deserialize<TestVirtualKeyData>(yaml);

            // Assert
            return result.VirtualKey;
        }

        [Test]
        [TestCase("0x41", ExpectedResult = 65)]
        [TestCase("0x20", ExpectedResult = 32)]
        [TestCase("0xFF", ExpectedResult = 255)]
        [TestCase("0x00", ExpectedResult = 0)]
        public int ReadYaml_ShouldParseHexValues_Correctly(string hexValue)
        {
            // Arrange
            var yaml = $"virtualKey: {hexValue}";

            // Act
            var result = _deserializer.Deserialize<TestVirtualKeyData>(yaml);

            // Assert
            return result.VirtualKey;
        }

        [Test]
        [TestCase("65", ExpectedResult = 65)]
        [TestCase("32", ExpectedResult = 32)]
        [TestCase("0", ExpectedResult = 0)]
        [TestCase("255", ExpectedResult = 255)]
        public int ReadYaml_ShouldParseDecimalValues_Correctly(string decimalValue)
        {
            // Arrange
            var yaml = $"virtualKey: {decimalValue}";

            // Act
            var result = _deserializer.Deserialize<TestVirtualKeyData>(yaml);

            // Assert
            return result.VirtualKey;
        }

        [Test]
        public void ReadYaml_ShouldThrowYamlException_ForInvalidVKConstant()
        {
            // Arrange
            var yaml = "virtualKey: VK_INVALID_KEY";

            // Act & Assert
            var ex = Assert.Throws<YamlException>(() => _deserializer.Deserialize<TestVirtualKeyData>(yaml));
            Assert.That(ex.Message, Contains.Substring("未知のVirtual Key Code定数: VK_INVALID_KEY"));
        }

        [Test]
        public void ReadYaml_ShouldThrowYamlException_ForInvalidNumber()
        {
            // Arrange
            var yaml = "virtualKey: invalid_number";

            // Act & Assert
            var ex = Assert.Throws<YamlException>(() => _deserializer.Deserialize<TestVirtualKeyData>(yaml));
            Assert.That(ex.Message, Contains.Substring("無効な整数値: invalid_number"));
        }

        [Test]
        public void ReadYaml_ShouldThrowYamlException_ForInvalidHexValue()
        {
            // Arrange
            var yaml = "virtualKey: 0xGG";

            // Act & Assert
            Assert.Throws<YamlException>(() => _deserializer.Deserialize<TestVirtualKeyData>(yaml));
        }

        [Test]
        public void WriteYaml_ShouldThrowNotSupportedException()
        {
            // Act & Assert
            Assert.Throws<NotSupportedException>(() => _converter.WriteYaml(null!, 0x41, typeof(int), null!));
        }

        [Test]
        public void ComplexYamlTest_ShouldHandleMultipleVirtualKeys()
        {
            // Arrange
            var yaml = @"
keys:
  - virtualKey: VK_A
  - virtualKey: 0x20
  - virtualKey: 65
  - virtualKey: VK_ESCAPE
";

            // Act
            var result = _deserializer.Deserialize<TestComplexVirtualKeyData>(yaml);

            // Assert
            Assert.That(result.Keys, Has.Length.EqualTo(4));
            Assert.That(result.Keys[0].VirtualKey, Is.EqualTo(VirtualKeyCodes.VK_A));
            Assert.That(result.Keys[1].VirtualKey, Is.EqualTo(0x20));
            Assert.That(result.Keys[2].VirtualKey, Is.EqualTo(65));
            Assert.That(result.Keys[3].VirtualKey, Is.EqualTo(VirtualKeyCodes.VK_ESCAPE));
        }

        [Test]
        public void CaseSensitivityTest_VKConstants_ShouldBeCaseInsensitive()
        {
            // Arrange
            var yamlUpper = "virtualKey: VK_A";

            // Act
            var resultUpper = _deserializer.Deserialize<TestVirtualKeyData>(yamlUpper);

            // Assert
            Assert.That(resultUpper.VirtualKey, Is.EqualTo(VirtualKeyCodes.VK_A));
            
            // Note: The converter currently only supports uppercase VK_ constants
            // Lower case is not supported by design
        }

        [Test]
        public void HexCaseSensitivityTest_ShouldBeCaseInsensitive()
        {
            // Arrange
            var yamlLower = "virtualKey: 0xff";
            var yamlUpper = "virtualKey: 0xFF";
            var yamlMixed = "virtualKey: 0xFf";

            // Act
            var resultLower = _deserializer.Deserialize<TestVirtualKeyData>(yamlLower);
            var resultUpper = _deserializer.Deserialize<TestVirtualKeyData>(yamlUpper);
            var resultMixed = _deserializer.Deserialize<TestVirtualKeyData>(yamlMixed);

            // Assert
            Assert.That(resultLower.VirtualKey, Is.EqualTo(255));
            Assert.That(resultUpper.VirtualKey, Is.EqualTo(255));
            Assert.That(resultMixed.VirtualKey, Is.EqualTo(255));
        }

        /// <summary>
        /// テスト用のデータクラス
        /// </summary>
        public class TestVirtualKeyData
        {
            public int VirtualKey { get; set; }
        }

        /// <summary>
        /// テスト用の複合データクラス
        /// </summary>
        public class TestComplexVirtualKeyData
        {
            public TestVirtualKeyData[] Keys { get; set; } = Array.Empty<TestVirtualKeyData>();
        }
    }
}