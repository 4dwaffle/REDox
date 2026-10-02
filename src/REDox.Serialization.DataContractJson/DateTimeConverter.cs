// SPDX-FileCopyrightText: 2026 CAPCOM CO., LTD.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.Serialization;

namespace REDox.Serialization.DataContractJson;

sealed class DateTimeConverter(DateTimeFormat format) : DataConverter<DateTime>
{
    public override DateTime Read(in DataReader reader, uint tokenId, DateTime existingValue)
    {
        return DateTime.ParseExact(reader.ReadString(tokenId), format.FormatString,
            format.FormatProvider, format.DateTimeStyles);
    }

    public override void Write(DataWriter writer, DateTime value)
    {
        writer.WriteString(value.ToString(format.FormatString, format.FormatProvider));
    }
}
