#nullable enable

using CityRise.Core;
using NUnit.Framework;

namespace CityRise.Tests.EditMode;

public sealed class AccessibilityServiceTests
{
    [Test]
    public void Defaults_AreNonePalette_OneScale_NoReducedMotion()
    {
        var svc = new AccessibilityService();
        Assert.That(svc.ColorblindPalette, Is.EqualTo(ColorblindPalette.None));
        Assert.That(svc.UiScale, Is.EqualTo(1f));
        Assert.That(svc.ReducedMotion, Is.False);
    }

    [Test]
    public void UiScale_Clamps_BelowMin()
    {
        var svc = new AccessibilityService { UiScale = 0.1f };
        Assert.That(svc.UiScale, Is.EqualTo(IAccessibilityService.MinUiScale));
    }

    [Test]
    public void UiScale_Clamps_AboveMax()
    {
        var svc = new AccessibilityService { UiScale = 5f };
        Assert.That(svc.UiScale, Is.EqualTo(IAccessibilityService.MaxUiScale));
    }

    [Test]
    public void Changed_FiresOnPaletteChange()
    {
        var svc = new AccessibilityService();
        var fired = 0;
        svc.Changed += () => fired++;

        svc.ColorblindPalette = ColorblindPalette.Deuteranopia;
        Assert.That(fired, Is.EqualTo(1));
    }

    [Test]
    public void Changed_DoesNotFire_WhenSettingSameValue()
    {
        var svc = new AccessibilityService();
        var fired = 0;
        svc.Changed += () => fired++;

        svc.ReducedMotion = false;        // already false
        svc.UiScale = 1f;                 // already 1
        svc.ColorblindPalette = ColorblindPalette.None;  // already None
        Assert.That(fired, Is.EqualTo(0));
    }

    [Test]
    public void Changed_FiresOnceForEachDistinctMutation()
    {
        var svc = new AccessibilityService();
        var fired = 0;
        svc.Changed += () => fired++;

        svc.UiScale = 1.2f;
        svc.ReducedMotion = true;
        svc.ColorblindPalette = ColorblindPalette.Tritanopia;
        Assert.That(fired, Is.EqualTo(3));
    }
}
