using System;
using System.IO;
using System.Linq;
using System.Text;
using REDox.Json;

namespace REDox.Csv.Tests;

public class CsvDocumentRegressionTest
{
    [Fact]
    public void ParseRowsWithoutHeaderProducesRootArray()
    {
        using var doc = CsvDocument.Parse(
            "name,age\ndef,66\n",
            SerializerSettings.Default);

        Assert.Equal(
            """[["name","age"],["def","66"]]""",
            doc.RootElement.ToJsonString());
    }

    [Fact]
    public void ParseRowsWithHeaderProducesObjectRows()
    {
        using var doc = CsvDocument.Parse(
            "name,age\ndef,66\n",
            SerializerSettings.Default,
            new CsvDocumentOptions { HasHeaderRecord = true });

        Assert.Equal(
            """[{"name":"def","age":"66"}]""",
            doc.RootElement.ToJsonString());
    }

    [Fact]
    public void ParseReadOnlySpanCopiesInput()
    {
        var utf8 = Encoding.UTF8.GetBytes("name,age\ndef,66\n");

        using var doc = CsvDocument.Parse(
            utf8.AsSpan(),
            SerializerSettings.Default,
            new CsvDocumentOptions { HasHeaderRecord = true });

        utf8[0] = (byte)'x';

        Assert.Equal(
            """[{"name":"def","age":"66"}]""",
            doc.RootElement.ToJsonString());
    }

    [Fact]
    public void ParseStreamProducesDocument()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("name,age\ndef,66\n"));

        using var doc = CsvDocument.Parse(
            stream,
            SerializerSettings.Default,
            new CsvDocumentOptions { HasHeaderRecord = true });

        Assert.Equal(
            """[{"name":"def","age":"66"}]""",
            doc.RootElement.ToJsonString());
    }

    [Fact]
    public void TryParseReadOnlySpanReturnsDocument()
    {
        var utf8 = "name,age\ndef,66\n"u8;

        var parsed = CsvDocument.TryParse(
            utf8,
            out var doc,
            SerializerSettings.Default,
            new CsvDocumentOptions { HasHeaderRecord = true });

        using (doc)
        {
            Assert.True(parsed);
            Assert.NotNull(doc);
            Assert.Equal(
                """[{"name":"def","age":"66"}]""",
                doc.RootElement.ToJsonString());
        }
    }

    [Fact]
    public void ParseUnterminatedQuotedFieldThrowsDocumentParseException()
    {
        var ex = Assert.ThrowsAny<DocumentParseException>(() =>
            CsvDocument.Parse(
                "id,value\n1,\"unterminated",
                SerializerSettings.Default,
                new CsvDocumentOptions { HasHeaderRecord = true }));

        Assert.Equal(11, ex.BytePosition);
    }

    [Fact]
    public void TryParseUnterminatedQuotedFieldReturnsFalse()
    {
        var parsed = CsvDocument.TryParse(
            "id,value\n1,\"unterminated"u8,
            out var doc,
            SerializerSettings.Default,
            new CsvDocumentOptions { HasHeaderRecord = true });

        Assert.False(parsed);
        Assert.Null(doc);
    }

    [Fact]
    public void ParseHeaderRecordWithEscapedQuoteInLaterColumn()
    {
        using var doc = CsvDocument.Parse(
            "id,value\n1,\"a\"\"b\"\n",
            SerializerSettings.Default,
            new CsvDocumentOptions { HasHeaderRecord = true });

        Assert.Equal(
            """[{"id":"1","value":"a\"b"}]""",
            doc.RootElement.ToJsonString());
    }

    [Fact]
    public void EncodeWithCustomSeparatorQuotesValuesContainingThatSeparator()
    {
        var rows = DValue.From(new[]
        {
            new[] { "alpha;beta", "tail" }
        });

        var csv = CsvDocument.EncodeToString(
            rows,
            new CsvWriteOptions { SeparatorChar = ';', NewLine = "\n" });

        Assert.Equal("\"alpha;beta\";tail\n", csv);
    }

    [Fact]
    public void EncodeInlineFloatValues()
    {
        var rows = DValue.From(new[]
        {
            new[] { 1.5, 2.25 }
        });

        var csv = CsvDocument.EncodeToString(
            rows,
            new CsvWriteOptions { NewLine = "\n" });

        Assert.Equal("1.5,2.25\n", csv);
    }

    [Fact]
    public void ParseHeaderRecordWithMoreThanThirtyTwoColumns()
    {
        var headers = Enumerable.Range(0, 33).Select(i => $"h{i}").ToArray();
        var values = Enumerable.Range(0, 33).Select(i => $"v{i}").ToArray();
        var csv = string.Join(',', headers) + Environment.NewLine + string.Join(',', values);

        using var doc = CsvDocument.Parse(
            csv,
            SerializerSettings.Default,
            new CsvDocumentOptions { HasHeaderRecord = true });

        var row = doc.RootElement.AsArray()[0].AsElement();

        Assert.Equal(33, row.GetPropertyCount());
        Assert.Equal("v0", row.GetProperty("h0").GetString());
        Assert.Equal("v32", row.GetProperty("h32").GetString());
    }
}