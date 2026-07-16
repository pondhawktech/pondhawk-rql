// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Rql.Criteria
{

    /// <summary>
    /// Marker interface for criteria objects that can carry raw RQL strings and be introspected by the filter builder.
    /// </summary>
    public interface ICriteria
    {
        /// <summary>Optional raw RQL criteria strings.</summary>
        public string[]? Rql { get; }

    }


}
