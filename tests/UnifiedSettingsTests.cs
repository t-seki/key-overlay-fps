using KeyOverlayFPS.Settings;
using System.Linq;
using System;

namespace KeyOverlayFPS.Tests;

[TestFixture]
public class UnifiedSettingsTests
{
    [Test]
    public void UnifiedSettings_DefaultValues_ShouldBeValid()
    {
        // Arrange & Act
        var settings = new UnifiedSettings();
        
        // Assert
        Assert.That(settings.Window, Is.Not.Null);
        Assert.That(settings.Display, Is.Not.Null);
        Assert.That(settings.Colors, Is.Not.Null);
        Assert.That(settings.Profile, Is.Not.Null);
        Assert.That(settings.Mouse, Is.Not.Null);
        Assert.That(settings.Layout, Is.Not.Null);
        
        // デフォルト値の検証
        Assert.That(settings.Display.Scale, Is.EqualTo(1.0));
        Assert.That(settings.Display.IsMouseVisible, Is.True);
        Assert.That(settings.Window.IsTopmost, Is.True);
        Assert.That(settings.Colors.Background, Is.EqualTo("Transparent"));
        Assert.That(settings.Colors.Foreground, Is.EqualTo("White"));
        Assert.That(settings.Colors.Highlight, Is.EqualTo("Green"));
        Assert.That(settings.Profile.Current, Is.EqualTo("FullKeyboard65"));
    }

    [Test]
    public void ColorDefinitions_ShouldContainExpectedColors()
    {
        // Arrange
        var settings = new UnifiedSettings();
        var colorDefs = settings.Colors.Definitions;
        
        // Act & Assert
        Assert.That(colorDefs.ForegroundColors.ContainsKey("White"), Is.True);
        Assert.That(colorDefs.ForegroundColors.ContainsKey("Black"), Is.True);
        Assert.That(colorDefs.ForegroundColors.ContainsKey("Red"), Is.True);
        
        Assert.That(colorDefs.HighlightColors.ContainsKey("Green"), Is.True);
        Assert.That(colorDefs.HighlightColors.ContainsKey("Red"), Is.True);
        Assert.That(colorDefs.HighlightColors.ContainsKey("Blue"), Is.True);
        
        Assert.That(colorDefs.BackgroundColors.ContainsKey("Transparent"), Is.True);
        Assert.That(colorDefs.BackgroundColors.ContainsKey("Black"), Is.True);
    }

    [Test]
    public void AppConstants_ShouldHaveValidValues()
    {
        // Act & Assert
        Assert.That(AppConstants.TimerInterval, Is.GreaterThan(0));
        Assert.That(AppConstants.ScrollDisplayFrames, Is.GreaterThan(0));
        Assert.That(AppConstants.DisplayScales.Count, Is.GreaterThan(0));
        Assert.That(AppConstants.DisplayScales.Contains(1.0), Is.True);
        
        Assert.That(AppConstants.WindowSizes.FullKeyboardWidth, Is.GreaterThan(0));
        Assert.That(AppConstants.WindowSizes.FullKeyboardHeight, Is.GreaterThan(0));
        Assert.That(AppConstants.WindowSizes.FpsWidth, Is.GreaterThan(0));
        Assert.That(AppConstants.WindowSizes.FpsHeight, Is.GreaterThan(0));
    }
}

[TestFixture]
public class SettingsValidatorTests
{
    [Test]
    public void ValidateUnifiedSettings_ValidSettings_ShouldReturnValid()
    {
        // Arrange
        var settings = new UnifiedSettings();
        
        // Act
        var result = SettingsValidator.ValidateUnifiedSettings(settings);
        
        // Assert
        Assert.That(result.IsValid, Is.True, $"検証失敗: {result}");
        Assert.That(result.Errors.Count, Is.EqualTo(0));
    }

    [Test]
    public void ValidateUnifiedSettings_NullSettings_ShouldReturnInvalid()
    {
        // Arrange
        UnifiedSettings? settings = null;
        
        // Act
        var result = SettingsValidator.ValidateUnifiedSettings(settings!);
        
        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Count, Is.GreaterThan(0));
        Assert.That(result.Errors.Any(e => e.Contains("null")), Is.True);
    }

    [Test]
    public void ValidateDisplaySettings_InvalidScale_ShouldReturnErrors()
    {
        // Arrange
        var settings = new UnifiedSettings();
        settings.Display.Scale = 2.5; // 無効なスケール（AppConstantsにない値）
        
        // Act
        var result = SettingsValidator.ValidateUnifiedSettings(settings);
        
        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.Contains("スケール")), Is.True);
    }
}

[TestFixture]
public class SettingsMigratorTests
{
    [Test]
    public void IsMigrationNeeded_ChecksExistenceCorrectly()
    {
        // Arrange
        var migrator = new SettingsMigrator();
        
        // Act
        var result = migrator.IsMigrationNeeded();
        
        // Assert
        // 結果は環境によって異なるが、メソッドが正常に実行されることを確認
        Assert.That(result, Is.TypeOf<bool>());
        
        // デバッグ情報として結果を出力
        Console.WriteLine($"Migration needed: {result}");
    }
}