//
// LX.EasyDb.Criterion.IProjection.cs
//
// Authors:
//	Longshine He <longshinehe@users.sourceforge.net>
//
// Copyright (c) 2012 Longshine He
//
// This code is distributed in the hope that it will be useful,
// but WITHOUT WARRANTY OF ANY KIND.
//

using System;

namespace LX.EasyDb.Criterion
{
    /// <summary>
    /// An object-oriented representation of a query result set projection in a <see cref="LX.EasyDb.ICriteria"/> query.
    /// Built-in projection types are provided by the <see cref="Projections"/> factory class.
    /// </summary>
    public interface IProjection : IFragment
    {
        /// <summary>
        /// Gets or sets the alias of this projection.
        /// </summary>
        String Alias { get; set; }
        /// <summary>
        /// Checks if this projection is grouped.
        /// </summary>
        Boolean Grouped { get; }
        /// <summary>
        /// Gets the grouping string of this projection if grouped.
        /// </summary>
        String ToGroupString(ICriteria criteria);
    }
}
