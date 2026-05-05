#nullable enable

using System.Collections.Generic;
using CityRise.Core;
using NUnit.Framework;

namespace CityRise.Tests.EditMode;

public sealed class I18nTests
{
    private sealed class DictProvider : ILocalizationProvider
    {
        private readonly Dictionary<string, string> _map;
        public DictProvider(Dictionary<string, string> map) { _map = map; }
        public bool TryGet(string key, out string value) => _map.TryGetValue(key, out value!);
    }

    [TearDown]
    public void TearDown() => I18n.ResetProvider();

    [Test]
    public void Get_KnownKey_ReturnsValue()
    {
        I18n.SetProvider(new DictProvider(new() { ["greeting"] = "hello" }));
        Assert.That(I18n.Get("greeting"), Is.EqualTo("hello"));
    }

    [Test]
    public void Get_MissingKey_ReturnsBracketedMarker()
    {
        I18n.SetProvider(new DictProvider(new()));
        Assert.That(I18n.Get("does.not.exist"), Is.EqualTo("[KEY:does.not.exist]"));
    }

    [Test]
    public void Get_DefaultProvider_ReturnsBracketedMarker()
    {
        // No provider set; default NullProvider always misses.
        Assert.That(I18n.Get("foo"), Is.EqualTo("[KEY:foo]"));
    }

    [Test]
    public void Get_EmptyKey_ReturnsEmptyMarker()
    {
        Assert.That(I18n.Get(""), Is.EqualTo("[KEY:]"));
    }

    [Test]
    public void Get_FormatsArgs()
    {
        I18n.SetProvider(new DictProvider(new() { ["budget.balance"] = "Balance: ${0}" }));
        Assert.That(I18n.Get("budget.balance", 1234), Is.EqualTo("Balance: $1234"));
    }

    [Test]
    public void Get_BadFormat_FallsBackToTemplate()
    {
        I18n.SetProvider(new DictProvider(new() { ["bad"] = "Hello {0:Z}" }));
        // {0:Z} is invalid for an int — Format throws; we return the template instead.
        var result = I18n.Get("bad", 42);
        Assert.That(result, Is.EqualTo("Hello {0:Z}"));
    }

    [Test]
    public void Get_NoArgs_ReturnsRawTemplate()
    {
        I18n.SetProvider(new DictProvider(new() { ["raw"] = "{0} and {1}" }));
        Assert.That(I18n.Get("raw"), Is.EqualTo("{0} and {1}"));
    }
}
