/*
The MIT License (MIT)

Copyright (c) 2024 Pond Hawk Technologies Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using CommunityToolkit.Diagnostics;

namespace Pondhawk.Rql.Builder
{

    /// <summary>
    /// Represents the name of a field targeted by an RQL predicate. Supports case-insensitive equality.
    /// </summary>
    public class Target(string name)
    {


        /// <summary>Determines whether the target name equals the specified string (case-insensitive).</summary>
        /// <param name="target">The target to compare.</param>
        /// <param name="candidate">The string to compare against.</param>
        /// <returns><c>true</c> if the names are equal ignoring case; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Target target, string candidate)
        {
            if (target is null || string.IsNullOrWhiteSpace(target.Name) || string.IsNullOrWhiteSpace(candidate))
                return false;

            return string.Equals(target.Name, candidate, StringComparison.OrdinalIgnoreCase);

        }

        /// <summary>Determines whether the target name does not equal the specified string (case-insensitive).</summary>
        /// <param name="target">The target to compare.</param>
        /// <param name="candidate">The string to compare against.</param>
        /// <returns><c>true</c> if the names are not equal ignoring case; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Target target, string candidate)
        {

            if (target is null || string.IsNullOrWhiteSpace(target.Name) || string.IsNullOrWhiteSpace(candidate))
                return true;

            return !string.Equals(target.Name, candidate, StringComparison.OrdinalIgnoreCase);

        }


        /// <summary>The field name this target represents.</summary>
        public string Name { get; } = name;


        /// <summary>Determines whether this target equals the specified target by ordinal name comparison.</summary>
        /// <param name="other">The other target to compare.</param>
        /// <returns><c>true</c> if the names are equal; otherwise, <c>false</c>.</returns>
        protected bool Equals(Target other)
        {
            Guard.IsNotNull(other);

            return string.Equals(Name, other.Name, StringComparison.Ordinal);

        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {

            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((Target)obj);

        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Name.GetHashCode(StringComparison.Ordinal);
        }


        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }

    }

}
