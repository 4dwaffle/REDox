using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using REDox.Json;

namespace REDox.Tests;

public sealed class JsonEdgeCaseTest
{
    private static readonly SerializerSettings s_settings = SerializerSettings.Default;

    private static readonly JsonDocumentOptions s_ndjsonOptions = new()
    {
        UseNewlineDelimitedFormat = true
    };

    private readonly ITestOutputHelper _output;

    public JsonEdgeCaseTest(ITestOutputHelper output)
    {
        _output = output;
    }

    private static MemoryStream ToStream(string json)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(json));
    }

    private async Task<List<string>> CollectAsync(string json, JsonDocumentOptions options = default)
    {
        using var stream = ToStream(json);

        var results = new List<string>();

        await foreach (var element in JsonSequence.ParseAsync(stream, s_settings, options,
                           TestContext.Current.CancellationToken))
        {
            results.Add(element.ToJsonString());
        }

        return results;
    }

    // ---- JsonSequence: root is neither an array nor NDJSON ----

    [Fact]
    public async Task Sequence_RootObject()
    {
        var results = await CollectAsync("{\"a\":1}");

        _output.WriteLine(string.Join(" | ", results));

        Assert.Equal(["{\"a\":1}"], results);
    }

    [Fact]
    public async Task Sequence_RootScalar()
    {
        var results = await CollectAsync("123");

        _output.WriteLine(string.Join(" | ", results));

        Assert.Equal(["123"], results);
    }

    [Fact]
    public async Task Sequence_RootObjectWithNestedArray()
    {
        var results = await CollectAsync("{\"items\":[1,2,3]}");

        _output.WriteLine(string.Join(" | ", results));

        Assert.Equal(["{\"items\":[1,2,3]}"], results);
    }

    [Fact]
    public async Task Sequence_ConcatenatedValues_WithoutNDJson()
    {
        var error = await Record.ExceptionAsync(() => CollectAsync("{\"a\":1} {\"a\":2}"));

        _output.WriteLine(error?.ToString() ?? "(no exception)");

        Assert.NotNull(error);
    }

    [Fact]
    public async Task Sequence_UnterminatedArray()
    {
        var error = await Record.ExceptionAsync(() => CollectAsync("[1,2"));

        _output.WriteLine(error?.ToString() ?? "(no exception)");

        Assert.NotNull(error);
    }

    [Fact]
    public async Task Sequence_Garbage()
    {
        // 既知の問題: EnableValueValidation が無効な場合、数値トークンとして解釈され例外にならない
        var results = await CollectAsync("abc");

        _output.WriteLine(string.Join(" | ", results));

        Assert.Single(results);
    }

    [Fact]
    public async Task Sequence_Garbage_WithValidation()
    {
        var error = await Record.ExceptionAsync(() => CollectAsync("abc", new JsonDocumentOptions
        {
            EnableValueValidation = true
        }));

        _output.WriteLine(error?.ToString() ?? "(no exception)");

        Assert.NotNull(error);
    }

    [Fact]
    public async Task Sequence_TrailingGarbageAfterArray()
    {
        // 既知の問題: ルート配列が閉じた時点で列挙を終えるため、後続の不正な文字列は検証されない
        var results = await CollectAsync("[1,2] trailing");

        _output.WriteLine(string.Join(" | ", results));

        Assert.Equal(["1", "2"], results);
    }

    [Fact]
    public async Task Sequence_NDJson_MultilineRecord()
    {
        var error = await Record.ExceptionAsync(() => CollectAsync("{\n\"a\":1}\n", s_ndjsonOptions));

        _output.WriteLine(error?.ToString() ?? "(no exception)");

        Assert.NotNull(error);
    }

    // ---- JsonDocument: NDJSON output combined with WriteIndented ----

    [Fact]
    public void EncodeNDJson_WriteIndented()
    {
        using var doc = JsonDocument.Parse("{\"a\":1}\n{\"a\":2}", s_settings, s_ndjsonOptions);

        var json = JsonDocument.EncodeToString(doc.RootElement, new JsonWriteOptions
        {
            UseNewlineDelimitedFormat = true,
            WriteIndented = true,
            NewLine = "\n"
        });

        _output.WriteLine(json.Replace("\n", "\\n"));

        // NDJSON 出力時は WriteIndented を内部で無視し、1 レコード 1 行で出力する
        Assert.Equal("{\"a\":1}\n{\"a\":2}", json);
    }

    [Fact]
    public void EncodeNDJson_WriteIndented_RoundTrip()
    {
        using var doc = JsonDocument.Parse("{\"a\":1}\n{\"a\":2}", s_settings, s_ndjsonOptions);

        var json = JsonDocument.EncodeToString(doc.RootElement, new JsonWriteOptions
        {
            UseNewlineDelimitedFormat = true,
            WriteIndented = true,
            NewLine = "\n"
        });

        var error = Record.Exception(() =>
        {
            using var reparsed = JsonDocument.Parse(json, s_settings, s_ndjsonOptions);
        });

        _output.WriteLine(error?.ToString() ?? "(no exception)");

        // NDJSON 出力は WriteIndented 指定があっても NDJSON として再解析できる
        Assert.Null(error);
    }
}