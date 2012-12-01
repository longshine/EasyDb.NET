//
// LX.EasyDb.Dialects.Function.ISQLFunction.cs
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
using System.Collections.Generic;

namespace LX.EasyDb.Dialects.Function
{
    /// <summary>
    /// Provides an interface for supporting various functions that are translated to SQL.
    /// The Dialect and its sub-classes use this interface to provide details required for processing of the function.
    /// </summary>
    public interface ISQLFunction
    {
        /// <summary>
        /// Gets the return type of the function.
        /// May be either a concrete type, or variable depending upon the type of the first argument.
        /// </summary>
        DbType GetReturnType(DbType firstArgumentType);
        /// <summary>
        /// Renders the function call as SQL fragment.
        /// </summary>
        String Render(IList<Object> args, IConnectionFactory factory);
    }
}
