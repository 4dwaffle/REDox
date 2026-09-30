using System;

namespace REDox.Toml.Tests;

public sealed class TomlValueTest
{
    private static DElement ParseValue(string keyValueToml, string key = "v")
    {
        var doc = TomlDocument.Parse(keyValueToml);
        return doc.RootElement.GetProperty(key);
    }

    // ---- Strings ----

    [Theory]
    [InlineData("v = \"hello\"", "hello")]
    [InlineData("v = \"\"", "")]
    [InlineData("v = \"I'm a string.\"", "I'm a string.")]
    [InlineData("v = \"quote: \\\"x\\\"\"", "quote: \"x\"")]
    [InlineData("v = \"tab\\tend\"", "tab\tend")]
    [InlineData("v = \"newline\\nend\"", "newline\nend")]
    [InlineData("v = \"unicode \\u00E9\"", "unicode \u00E9")]
    [InlineData("v = \"unicode \\U0001F600\"", "unicode \U0001F600")]
    public void BasicString_ShouldDecode(string toml, string expected)
    {
        Assert.Equal(expected, ParseValue(toml).GetString());
    }

    [Theory]
    [InlineData("v = 'C:\\\\Users\\\\nodejs'", "C:\\\\Users\\\\nodejs")]
    [InlineData("v = 'no escape \\n here'", "no escape \\n here")]
    [InlineData("v = '<\\i\\c*\\s*>'", "<\\i\\c*\\s*>")]
    public void LiteralString_ShouldKeepVerbatim(string toml, string expected)
    {
        Assert.Equal(expected, ParseValue(toml).GetString());
    }

    [Fact]
    public void MultilineBasicString_ShouldTrimFirstNewline()
    {
        const string toml = "v = \"\"\"\nThe quick brown \\\n  fox.\"\"\"";
        Assert.Equal("The quick brown fox.", ParseValue(toml).GetString());
    }

    [Fact]
    public void MultilineLiteralString_ShouldKeepVerbatim()
    {
        const string toml = "v = '''\nline1\nline2'''";
        Assert.Equal("line1\nline2", ParseValue(toml).GetString());
    }

    // ---- Integers ----

    [Theory]
    [InlineData("v = 0", 0L)]
    [InlineData("v = +99", 99L)]
    [InlineData("v = -17", -17L)]
    [InlineData("v = 1_000", 1000L)]
    [InlineData("v = 5_349_221", 5349221L)]
    [InlineData("v = 0xDEADBEEF", 0xDEADBEEFL)]
    [InlineData("v = 0xdead_beef", 0xDEADBEEFL)]
    [InlineData("v = 0o01234567", 342391L)]
    [InlineData("v = 0o755", 493L)]
    [InlineData("v = 0b11010110", 214L)]
    public void Integer_ShouldDecode(string toml, long expected)
    {
        Assert.Equal(expected, ParseValue(toml).GetInt64());
    }

    // ---- Floats ----

    [Theory]
    [InlineData("v = 1.0", 1.0)]
    [InlineData("v = 3.1415", 3.1415)]
    [InlineData("v = -0.01", -0.01)]
    [InlineData("v = 5e+22", 5e+22)]
    [InlineData("v = 1e06", 1e06)]
    [InlineData("v = -2E-2", -2E-2)]
    [InlineData("v = 6.626e-34", 6.626e-34)]
    [InlineData("v = 9_224_617.445_991_228", 9224617.445991228)]
    public void Float_ShouldDecode(string toml, double expected)
    {
        Assert.Equal(expected, ParseValue(toml).GetDouble(), 12);
    }

    [Theory]
    [InlineData("v = inf", double.PositiveInfinity)]
    [InlineData("v = +inf", double.PositiveInfinity)]
    [InlineData("v = -inf", double.NegativeInfinity)]
    public void Float_Infinity_ShouldDecode(string toml, double expected)
    {
        Assert.Equal(expected, ParseValue(toml).GetDouble());
    }

    [Theory]
    [InlineData("v = nan")]
    [InlineData("v = +nan")]
    [InlineData("v = -nan")]
    public void Float_Nan_ShouldDecode(string toml)
    {
        Assert.True(double.IsNaN(ParseValue(toml).GetDouble()));
    }

    // ---- Booleans ----

    [Theory]
    [InlineData("v = true", true)]
    [InlineData("v = false", false)]
    public void Boolean_ShouldDecode(string toml, bool expected)
    {
        Assert.Equal(expected, ParseValue(toml).GetBoolean());
    }

    // ---- Date / Time ----

    [Fact]
    public void OffsetDateTime_ShouldDecode()
    {
        var value = ParseValue("v = 1979-05-27T07:32:00Z").GetDateTimeOffset();
        Assert.Equal(new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero), value);
    }

    [Fact]
    public void OffsetDateTime_WithOffset_ShouldDecode()
    {
        var value = ParseValue("v = 1979-05-27T00:32:00-07:00").GetDateTimeOffset();
        Assert.Equal(new DateTimeOffset(1979, 5, 27, 0, 32, 0, TimeSpan.FromHours(-7)), value);
    }

    [Fact]
    public void LocalDateTime_ShouldDecode()
    {
        var value = ParseValue("v = 1979-05-27T07:32:00").GetDateTime();
        Assert.Equal(new DateTime(1979, 5, 27, 7, 32, 0), value);
    }

    [Fact]
    public void LocalDate_ShouldDecode()
    {
        var value = ParseValue("v = 1979-05-27").GetDateTime();
        Assert.Equal(new DateTime(1979, 5, 27), value);
    }
}