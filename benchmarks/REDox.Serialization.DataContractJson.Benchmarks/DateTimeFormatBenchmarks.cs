using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using BenchmarkDotNet.Attributes;
using REDox.Json;

namespace REDox.Serialization.DataContractJson.Benchmarks;

[MemoryDiagnoser]
public class DateTimeFormatBenchmarks
{
    public enum DateFormat
    {
        Default,
        InvariantTimestamp,
        JapaneseLongDate
    }

    [Params(DateFormat.Default, DateFormat.InvariantTimestamp, DateFormat.JapaneseLongDate)]
    public DateFormat Format { get; set; }

    [Params(1, 64)]
    public int Count { get; set; }

    private DataContractJsonSerializerSettings _settings = null!;
    private DateTime[] _values = null!;
    private string _json = null!;

    [GlobalSetup]
    public void Setup()
    {
        // The baseline ignores FormatProvider. Use the same ambient culture as the
        // Japanese provider so both revisions perform equivalent, successful work.
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ja-JP");
        var settings = new System.Runtime.Serialization.Json.DataContractJsonSerializerSettings
        {
            DateTimeFormat = Format switch
            {
                DateFormat.InvariantTimestamp => new DateTimeFormat("yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture)
                {
                    DateTimeStyles = DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal
                },
                DateFormat.JapaneseLongDate => new DateTimeFormat("D", CultureInfo.GetCultureInfo("ja-JP")),
                _ => null
            }
        };
        _settings = new DataContractJsonSerializerSettings(settings);
        _values = new DateTime[Count];
        for (var i = 0; i < Count; i++)
        {
            _values[i] = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc).AddDays(i);
        }

        // Build identical input with the framework serializer, outside measurement.
        var reference = new DataContractJsonSerializer(typeof(DateTime[]), settings);
        using var stream = new MemoryStream();
        reference.WriteObject(stream, _values);
        _json = Encoding.UTF8.GetString(stream.ToArray());
        if (Serialize() != _json)
        {
            throw new InvalidOperationException("Serialization differs from the framework serializer.");
        }

        stream.Position = 0;
        var expected = (DateTime[])reference.ReadObject(stream)!;
        var actual = Deserialize();
        for (var i = 0; i < Count; i++)
        {
            if (actual[i] != expected[i] || actual[i].Kind != expected[i].Kind)
            {
                throw new InvalidOperationException("Deserialization differs from the framework serializer.");
            }
        }
    }

    [Benchmark]
    public string Serialize() => JsonSerializer.Serialize(_values, _settings);

    [Benchmark]
    public DateTime[] Deserialize() => JsonSerializer.Deserialize<DateTime[]>(_json, _settings)!;
}
