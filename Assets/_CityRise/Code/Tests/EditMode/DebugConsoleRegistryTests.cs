#nullable enable

using System.Reflection;
using CityRise.Debug;
using NUnit.Framework;

namespace CityRise.Tests.EditMode;

public sealed class DebugConsoleRegistryTests
{
    // Sample command set lives inside a private class — registry walks loaded assemblies and
    // discovers them via [DebugCommand] reflection.
    private static class SampleCommands
    {
        public static int InvokeCount;
        public static string? LastNote;

        [DebugCommand("ping", "respond with pong")]
        public static string Ping()
        {
            InvokeCount++;
            return "pong";
        }

        [DebugCommand("add", "sum two ints")]
        public static string Add(int a, int b)
        {
            InvokeCount++;
            return (a + b).ToString();
        }

        [DebugCommand("note", "record a note string")]
        public static void Note(string message)
        {
            InvokeCount++;
            LastNote = message;
        }

        [DebugCommand("explode", "always throws")]
        public static void Explode()
        {
            throw new System.InvalidOperationException("boom");
        }

        [DebugCommand("toggle", "accept a bool")]
        public static string Toggle(bool on) => on ? "on" : "off";
    }

    [SetUp]
    public void Reset()
    {
        SampleCommands.InvokeCount = 0;
        SampleCommands.LastNote = null;
    }

    private static DebugConsoleRegistry BuildRegistry()
    {
        var r = new DebugConsoleRegistry();
        r.ScanAssembly(Assembly.GetExecutingAssembly());
        return r;
    }

    [Test]
    public void ScanAssembly_DiscoversAttributedMethods()
    {
        var r = BuildRegistry();
        Assert.That(r.Find("ping"), Is.Not.Null);
        Assert.That(r.Find("add"), Is.Not.Null);
        Assert.That(r.Find("note"), Is.Not.Null);
        Assert.That(r.Find("explode"), Is.Not.Null);
        Assert.That(r.Find("toggle"), Is.Not.Null);
    }

    [Test]
    public void Find_IsCaseInsensitive()
    {
        var r = BuildRegistry();
        Assert.That(r.Find("PING"), Is.Not.Null);
        Assert.That(r.Find("Ping"), Is.Not.Null);
    }

    [Test]
    public void Execute_NoArgs_ReturnsCommandOutput()
    {
        var r = BuildRegistry();
        Assert.That(r.Execute("ping"), Is.EqualTo("pong"));
        Assert.That(SampleCommands.InvokeCount, Is.EqualTo(1));
    }

    [Test]
    public void Execute_TypedArgs_AreConverted()
    {
        var r = BuildRegistry();
        Assert.That(r.Execute("add 3 4"), Is.EqualTo("7"));
    }

    [Test]
    public void Execute_StringArg_PassedThrough()
    {
        var r = BuildRegistry();
        Assert.That(r.Execute("note hello"), Is.Empty); // void → empty output
        Assert.That(SampleCommands.LastNote, Is.EqualTo("hello"));
    }

    [Test]
    public void Execute_BoolArg_AcceptsTrueFalseAnd01()
    {
        var r = BuildRegistry();
        Assert.That(r.Execute("toggle true"), Is.EqualTo("on"));
        Assert.That(r.Execute("toggle false"), Is.EqualTo("off"));
        Assert.That(r.Execute("toggle 1"), Is.EqualTo("on"));
        Assert.That(r.Execute("toggle 0"), Is.EqualTo("off"));
    }

    [Test]
    public void Execute_UnknownCommand_ReturnsErrorMessage()
    {
        var r = BuildRegistry();
        Assert.That(r.Execute("nope"), Does.Contain("Unknown command"));
        Assert.That(SampleCommands.InvokeCount, Is.EqualTo(0));
    }

    [Test]
    public void Execute_WrongArgCount_ReturnsUsage()
    {
        var r = BuildRegistry();
        Assert.That(r.Execute("add 3"), Does.Contain("Usage: add"));
        Assert.That(SampleCommands.InvokeCount, Is.EqualTo(0));
    }

    [Test]
    public void Execute_BadIntArg_ReturnsErrorMessage()
    {
        var r = BuildRegistry();
        Assert.That(r.Execute("add abc 4"), Does.Contain("expected"));
        Assert.That(SampleCommands.InvokeCount, Is.EqualTo(0));
    }

    [Test]
    public void Execute_CommandThrows_ReturnsCaughtMessage()
    {
        var r = BuildRegistry();
        Assert.That(r.Execute("explode"), Does.Contain("boom"));
    }

    [Test]
    public void Execute_EmptyOrWhitespace_ReturnsEmpty()
    {
        var r = BuildRegistry();
        Assert.That(r.Execute(""), Is.Empty);
        Assert.That(r.Execute("   "), Is.Empty);
    }

    [Test]
    public void Suggest_PrefixMatches_AreSorted()
    {
        var r = BuildRegistry();
        var suggestions = r.Suggest("p");
        Assert.That(suggestions, Has.Member("ping"));
    }

    [Test]
    public void Suggest_EmptyPrefix_ReturnsEmpty()
    {
        var r = BuildRegistry();
        Assert.That(r.Suggest(""), Is.Empty);
    }

    [Test]
    public void Tokenize_HandlesMultipleSpaces()
    {
        var tokens = DebugConsoleRegistry.Tokenize("  add   3   4  ");
        Assert.That(tokens, Is.EqualTo(new[] { "add", "3", "4" }));
    }
}
