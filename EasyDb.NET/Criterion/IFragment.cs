//
// LX.EasyDb.IFragment.cs
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
    /// Interface of query fragments.
    /// </summary>
    public interface IFragment
    {
        /// <summary>
        /// Renders the SQL fragment.
        /// </summary>
        String ToSqlString(ICriteria criteria);
    }
}
