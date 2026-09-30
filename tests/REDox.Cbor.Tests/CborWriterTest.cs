using PeterO.Cbor;

namespace REDox.Cbor.Tests;

public sealed class CborWriterTest
{
    [Fact]
    public void WriteInfinityLength()
    {
        byte[] cbor1 = [];
        byte[] cbor2 = [];


        {
            var writer = new System.Formats.Cbor.CborWriter();

            writer.WriteStartArray(null);
            writer.WriteBoolean(true);
            writer.WriteStartMap(null);
            writer.WriteTextString("key");
            writer.WriteInt32(123);

            writer.WriteDouble(1.23);
            writer.WriteStartArray(null);
            for (var i = 0; i < 100; i++)
            {
                writer.WriteInt32(i);
            }

            writer.WriteEndArray();

            writer.WriteEndMap();
            writer.WriteEndArray();

            cbor2 = writer.Encode();
        }

        {
            using var writer = new CborWriter(SerializerSettings.Default);

            writer.WriteStartArray(null);
            writer.WriteBoolean(true);
            writer.WriteStartMap(null);
            writer.WriteString("key");
            writer.WriteInt32(123);

            writer.WriteDouble(1.23);
            writer.WriteStartArray(null);
            for (var i = 0; i < 100; i++)
            {
                writer.WriteInt32(i);
            }

            writer.WriteEndArray();

            writer.WriteEndMap();
            writer.WriteEndArray();

            cbor1 = writer.Encode();
        }

        Assert.Equal(cbor2, cbor1);

        var json1 = CBORObject.DecodeFromBytes(cbor1).ToJSONString();
        var json2 = CBORObject.DecodeFromBytes(cbor2).ToJSONString();

        Assert.Equal(json2, json1);
    }

    [Fact]
    public void WriteUndefinedLength()
    {
        byte[] cbor1 = [];
        byte[] cbor2 = [];

        {
            using var writer = new CborWriter(SerializerSettings.Default, new CborWriteOptions
            {
                ConvertIndefiniteLengthEncodings = true
            });

            writer.WriteStartArray(null);
            writer.WriteBoolean(true);
            writer.WriteStartMap(null);
            writer.WriteString("key");
            writer.WriteInt32(123);

            writer.WriteDouble(1.23);
            writer.WriteStartArray(null);
            for (var i = 0; i < 100; i++)
            {
                writer.WriteInt32(i);
            }

            writer.WriteEndArray();


            writer.WriteEndMap();
            writer.WriteEndArray();

            cbor1 = writer.Encode();
        }

        {
            var writer = new System.Formats.Cbor.CborWriter(convertIndefiniteLengthEncodings: true);

            writer.WriteStartArray(null);
            writer.WriteBoolean(true);
            writer.WriteStartMap(null);
            writer.WriteTextString("key");
            writer.WriteInt32(123);

            writer.WriteDouble(1.23);
            writer.WriteStartArray(null);
            for (var i = 0; i < 100; i++)
            {
                writer.WriteInt32(i);
            }

            writer.WriteEndArray();


            writer.WriteEndMap();
            writer.WriteEndArray();

            cbor2 = writer.Encode();
        }

        Assert.Equal(cbor2, cbor1);

        var json1 = CBORObject.DecodeFromBytes(cbor1).ToJSONString();
        var json2 = CBORObject.DecodeFromBytes(cbor2).ToJSONString();

        Assert.Equal(json2, json1);
    }
}