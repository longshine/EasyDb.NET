//
// LX.EasyDb.IDbOperationBlock.cs
//
// Authors:
//	Longshine He <longshinehe@users.sourceforge.net>
//
// Copyright (C) 2012 Longshine He
//
// This code is distributed in the hope that it will be useful,
// but WITHOUT WARRANTY OF ANY KIND.
//

using System;
using System.Data;

namespace LX.EasyDb
{
    /// <summary>
    /// Represents one execution of a text command.
    /// </summary>
    public interface IDbOperationBlock
    {
        /// <summary>
        /// Gets the type of this command.
        /// </summary>
        DbOperationType Operation { get; }
        /// <summary>
        /// Gets the text command to run.
        /// </summary>
        String CommandText { get; }
        /// <summary>
        /// Gets the collection of parameters.
        /// </summary>
        IDbDataParameter[] Parameters { get; }
        /// <summary>
        /// Gets the type indicates or specifies how the command is interpreted.
        /// </summary>
        CommandType CommandType { get; }
    }
}
