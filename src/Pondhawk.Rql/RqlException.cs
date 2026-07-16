// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Rql;

/// <summary>
/// Exception thrown for RQL parsing, serialization, or validation errors.
/// </summary>
public class RqlException : Exception
{

    /// <summary>
    /// Initializes a new instance of the <see cref="RqlException"/> class with the specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RqlException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RqlException"/> class with the specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public RqlException(string message, Exception inner) : base(message, inner)
    {
    }

}
