using System.Buffers;
using BenchmarkDotNet.Attributes;

namespace REDox.MessagePack.Benchmarks;

[MemoryDiagnoser]
public class MessagePackWrite
{
    private readonly ArrayBufferWriter<byte> _bufferWriter;
    private readonly double[] _doubleTbl;
    private readonly int[] _intTbl;
    private readonly MessagePackWriter _messagePackWriter;

    public MessagePackWrite()
    {
        _bufferWriter = new ArrayBufferWriter<byte>();
        _messagePackWriter = new MessagePackWriter(SerializerSettings.Default);

        _doubleTbl = new double[10000];
        _intTbl = new int[10000];
    }

    /*
    [Benchmark]
    [BenchmarkCategory(nameof(REDox))]
    public byte[] REDoxMessagePackWrite()
    {
        var writer = _messagePackWriter;
        writer.Reset(SerializerSettings.Default);

        writer.WriteStartMap(2);
        writer.WriteString("DoubleArray"u8);
        writer.WriteStartArray(_doubleTbl.Length);
        writer.WriteDoubleValues(_doubleTbl);
        writer.WriteEndArray();
        writer.WriteString("IntArray"u8);
        writer.WriteStartArray(_intTbl.Length);
        writer.WriteInt32Values(_intTbl);
        writer.WriteEndArray();
        writer.WriteEndMap();

        return writer.Encode();
    }

    [Benchmark]
    [BenchmarkCategory(nameof(REDox))]
    public byte[] REDoxMessagePackWriteToBuffer()
    {
        _bufferWriter.Clear();

        var writer = _messagePackWriter;
        writer.Reset(_bufferWriter, SerializerSettings.Default);

        writer.WriteStartMap(2);
        writer.WriteString("DoubleArray"u8);
        writer.WriteStartArray(_doubleTbl.Length);
        writer.WriteDoubleValues(_doubleTbl);
        writer.WriteEndArray();
        writer.WriteString("IntArray"u8);
        writer.WriteStartArray(_intTbl.Length);
        writer.WriteInt32Values(_intTbl);
        writer.WriteEndArray();
        writer.WriteEndMap();
        writer.Dispose();

        return _bufferWriter.WrittenSpan.ToArray();
    }
    */

    [Benchmark]
    [BenchmarkCategory(nameof(REDox))]
    public byte[] MessagePackCSharpMessagePackWrite()
    {
        _bufferWriter.Clear();

        var writer = new global::MessagePack.MessagePackWriter(_bufferWriter);

        writer.WriteMapHeader(2);
        writer.WriteString("DoubleArray"u8);
        writer.WriteArrayHeader(_doubleTbl.Length);
        foreach (var d in _doubleTbl)
        {
            writer.Write(d);
        }

        writer.WriteString("IntArray"u8);
        writer.WriteArrayHeader(_intTbl.Length);
        foreach (var d in _intTbl)
        {
            writer.Write(d);
        }

        writer.Flush();

        return _bufferWriter.WrittenSpan.ToArray();
    }
}