// SPDX-FileCopyrightText: 2026 CAPCOM CO., LTD.
// SPDX-License-Identifier: Apache-2.0

namespace REDox.Csv;

public readonly record struct CsvDocumentOptions
{
    public bool HasHeaderRecord { get; init; }

    public char SeparatorChar { get; init; }
}