using System;
using System.Text;

namespace REDox.Tests;

public class Utf8TextWriterTest
{
    private readonly ITestOutputHelper _output;

    public Utf8TextWriterTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void EncodeToStringDecodesUtf8AcrossBufferSegments()
    {
        using var writer = new Utf8TextWriter(SerializerSettings.Default);

        var bufSize = writer.Settings.DefaultBufferSize;

        writer.WriteString(Encoding.ASCII.GetBytes(new string('a', bufSize - 1)));

        var utf8Bytes = Encoding.UTF8.GetBytes("あ");
        writer.WriteUtf8Byte(utf8Bytes[0]);
        writer.WriteString(new ReadOnlySpan<byte>(utf8Bytes, 1, utf8Bytes.Length - 1));

        var expected = new string('a', bufSize - 1) + "あ";

        _output.WriteLine(expected.Length.ToString());
        _output.WriteLine(writer.EncodeToString().Length.ToString());

        Assert.Equal(expected, writer.EncodeToString());
    }
}