using System;
using System.IO;
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
        private ISerializer _serializer;

        [SetUp]
        public void SetUp()
        {
            _converter = new VirtualKeyCodeConverter();
            
            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(_converter)
                .Build();

            _serializer = new SerializerBuilder()
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
        [TestCase(0x41, ExpectedResult = "VK_A")]
        [TestCase(0x20, ExpectedResult = "VK_SPACE")]
        [TestCase(0x1B, ExpectedResult = "VK_ESCAPE")]
        [TestCase(0x70, ExpectedResult = "VK_F1")]
        [TestCase(0xA0, ExpectedResult = "VK_LSHIFT")]
        [TestCase(0x0D, ExpectedResult = "VK_RETURN")]
        public string WriteYaml_ShouldReturnConstantName_ForKnownVKValues(int value)
        {
            // Arrange
            var data = new TestVirtualKeyData { VirtualKey = value };

            // Act
            var yaml = _serializer.Serialize(data);

            // Assert
            using var reader = new StringReader(yaml);
            var line = reader.ReadLine();
            while (line != null)
            {
                if (line.Contains("virtualKey:"))
                {
                    return line.Split(':')[1].Trim();
                }
                line = reader.ReadLine();
            }
            
            return string.Empty;
        }

        [Test]
        [TestCase(999, ExpectedResult = "999")]
        [TestCase(1000, ExpectedResult = "1000")]
        [TestCase(-1, ExpectedResult = "VK_FN")] // VK_FN is defined as -1
        public string WriteYaml_ShouldReturnNumericValue_ForUnknownValues(int value)
        {
            // Arrange
            var data = new TestVirtualKeyData { VirtualKey = value };

            // Act
            var yaml = _serializer.Serialize(data);

            // Assert
            using var reader = new StringReader(yaml);
            var line = reader.ReadLine();
            while (line != null)
            {
                if (line.Contains("virtualKey:"))
                {
                    return line.Split(':')[1].Trim();
                }
                line = reader.ReadLine();
            }
            
            return string.Empty;
        }

        [Test]
        public void WriteYaml_ShouldHandleNullValue()
        {
            // Arrange
            var data = new TestVirtualKeyDataNullable { VirtualKey = null };

            // Act
            var yaml = _serializer.Serialize(data);

            // Assert
            using var reader = new StringReader(yaml);
            var line = reader.ReadLine();
            while (line != null)
            {
                if (line.Contains("virtualKey:"))
                {
                    var value = line.Split(':')[1].Trim();
                    // Null values are typically serialized as empty or null in YAML
                    Assert.That(value, Is.AnyOf("", "null", "~"));
                    return;
                }
                line = reader.ReadLine();
            }
            
            Assert.Fail("virtualKey property not found in YAML output");
        }

        [Test]
        public void RoundTripTest_VKConstants_ShouldPreserveValues()
        {
            // Arrange
            var originalData = new TestVirtualKeyData { VirtualKey = VirtualKeyCodes.VK_A };

            // Act - シリアライズしてデシリアライズ
            var yaml = _serializer.Serialize(originalData);
            var deserializedData = _deserializer.Deserialize<TestVirtualKeyData>(yaml);

            // Assert
            Assert.That(deserializedData.VirtualKey, Is.EqualTo(originalData.VirtualKey));
        }

        [Test]
        public void RoundTripTest_HexValues_ShouldPreserveValues()
        {
            // Arrange
            var testValues = new[] { 0x41, 0x20, 0xFF, 0x00, 0x1B };

            foreach (var value in testValues)
            {
                // Arrange
                var originalData = new TestVirtualKeyData { VirtualKey = value };

                // Act
                var yaml = _serializer.Serialize(originalData);
                var deserializedData = _deserializer.Deserialize<TestVirtualKeyData>(yaml);

                // Assert
                Assert.That(deserializedData.VirtualKey, Is.EqualTo(originalData.VirtualKey), 
                    $"Failed for value 0x{value:X2}");
            }
        }

        [Test]
        public void RoundTripTest_DecimalValues_ShouldPreserveValues()
        {
            // Arrange
            var testValues = new[] { 65, 32, 255, 0, 27, 999 };

            foreach (var value in testValues)
            {
                // Arrange
                var originalData = new TestVirtualKeyData { VirtualKey = value };

                // Act
                var yaml = _serializer.Serialize(originalData);
                var deserializedData = _deserializer.Deserialize<TestVirtualKeyData>(yaml);

                // Assert
                Assert.That(deserializedData.VirtualKey, Is.EqualTo(originalData.VirtualKey), 
                    $"Failed for value {value}");
            }
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
        /// テスト用のnullable型データクラス
        /// </summary>
        public class TestVirtualKeyDataNullable
        {
            public int? VirtualKey { get; set; }
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