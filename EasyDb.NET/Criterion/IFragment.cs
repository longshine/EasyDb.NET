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
    /// Represents a query fragment that can be rendered into a query.
    /// </summary>
    public interface IFragment
    {
        /// <summary>
        /// Renders this fragment.
        /// </summary>
        String Render(ICriteria criteria);
    }
}
