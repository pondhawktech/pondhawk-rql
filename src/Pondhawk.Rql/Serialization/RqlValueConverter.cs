// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using CommunityToolkit.Diagnostics;

namespace Pondhawk.Rql.Serialization;

/// <summary>
/// The built-in conversion of an operand to the type of the property it is compared with: what
/// <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> converts, and also nullable types, enums,
/// <see cref="DateTimeOffset"/>, <see cref="DateOnly"/>, <see cref="TimeOnly"/> and <see cref="Guid"/>.
/// </summary>
public static class RqlValueConverter
{
    /// <summary>Converts an operand to a property's type.</summary>
    /// <param name="value">The operand.</param>
    /// <param name="targetType">The property's type.</param>
    /// <returns>The operand as <paramref name="targetType"/> (as its underlying type when nullable).</returns>
    /// <remarks>
    /// A <see cref="DateTime"/> whose kind is unspecified is taken as UTC when it becomes a <see cref="DateTimeOffset"/>,
    /// as the RQL text parser reads <c>@…Z</c> values. Strings are parsed with the invariant culture.
    /// </remarks>
    /// <exception cref="InvalidCastException">The operand cannot be converted, as <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> reports it.</exception>
    /// <exception cref="FormatException">A string operand is not in the type's format.</exception>
    public static object Convert(object value, Type targetType)
    {
        Guard.IsNotNull(value);
        Guard.IsNotNull(targetType);

        var type = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (type.IsInstanceOfType(value))
            return value;

        return value switch
        {
            _ when type.IsEnum && value is string name => Enum.Parse(type, name, ignoreCase: true),
            _ when type.IsEnum => Enum.ToObject(type, value),
            DateTime time when type == typeof(DateTimeOffset) => time.Kind == DateTimeKind.Unspecified
                ? new DateTimeOffset(DateTime.SpecifyKind(time, DateTimeKind.Utc))
                : new DateTimeOffset(time),
            DateTime time when type == typeof(DateOnly) => DateOnly.FromDateTime(time),
            DateTime time when type == typeof(TimeOnly) => TimeOnly.FromDateTime(time),
            DateTimeOffset time when type == typeof(DateTime) => time.UtcDateTime,
            DateTimeOffset time when type == typeof(DateOnly) => DateOnly.FromDateTime(time.UtcDateTime),
            string text when type == typeof(DateTimeOffset) => DateTimeOffset.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
            string text when type == typeof(DateOnly) => DateOnly.Parse(text, CultureInfo.InvariantCulture),
            string text when type == typeof(TimeOnly) => TimeOnly.Parse(text, CultureInfo.InvariantCulture),
            string text when type == typeof(Guid) => Guid.Parse(text),
            _ => System.Convert.ChangeType(value, type, CultureInfo.InvariantCulture),
        };
    }
}
