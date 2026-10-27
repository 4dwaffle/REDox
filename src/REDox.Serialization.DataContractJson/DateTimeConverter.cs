// SPDX-FileCopyrightText: 2026 CAPCOM CO., LTD.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Buffers;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;

namespace REDox.Serialization.DataContractJson;

sealed class DateTimeConverter : DataConverter<DateTime>
{
    private readonly DateTimeFormat _format;
    private readonly bool _useIsoTimestamp;

    public DateTimeConverter(DateTimeFormat format)
    {
        _format = format;
        _useIsoTimestamp = format.FormatString == "yyyy-MM-ddTHH:mm:ssK" &&
                           (ReferenceEquals(format.FormatProvider, CultureInfo.InvariantCulture) ||
                            ReferenceEquals(format.FormatProvider, DateTimeFormatInfo.InvariantInfo));
    }

    public override DateTime Read(in DataReader reader, uint tokenId, DateTime existingValue)
    {
        var source = reader.ReadUtf8String(tokenId);
        var styles = _format.DateTimeStyles;

        // These exact shapes and styles have the same semantics as ParseExact.
        // Offsets, other styles, and other formats use the framework parser below.
        if (_useIsoTimestamp &&
            (styles == DateTimeStyles.RoundtripKind ||
             styles == (DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal)) &&
            (source.Length == 19 || source.Length == 20 && source[19] == 'Z') &&
            Utf8Helper.TryParseDateTimeOffsetIso8601(source, out var value, out _))
        {
            return styles == DateTimeStyles.RoundtripKind
                ? value
                : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        var charCount = Encoding.UTF8.GetCharCount(source);
        char[]? rented = null;
        Span<char> chars = charCount <= 256
            ? stackalloc char[charCount]
            : (rented = ArrayPool<char>.Shared.Rent(charCount)).AsSpan(0, charCount);
        try
        {
            Encoding.UTF8.GetChars(source, chars);
            return DateTime.ParseExact(chars, _format.FormatString, _format.FormatProvider, styles);
        }
        finally
        {
            if (rented != null)
            {
                ArrayPool<char>.Shared.Return(rented);
            }
        }
    }

    public override void Write(DataWriter writer, DateTime value)
    {
        Span<char> buffer = stackalloc char[128];
        if (value.TryFormat(buffer, out var written, _format.FormatString, _format.FormatProvider))
        {
            writer.WriteString(buffer.Slice(0, written));
        }
        else
        {
            // Preserve support for unusually long custom formats.
            writer.WriteString(value.ToString(_format.FormatString, _format.FormatProvider));
        }
    }
}
