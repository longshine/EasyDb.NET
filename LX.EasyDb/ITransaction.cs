//
// LX.EasyDb.ITransaction.cs
//
// Authors:
//	Longshine He <longshinehe@users.sourceforge.net>
//
// Copyright (c) 2012 Longshine He
//
// This code is distributed in the hope that it will be useful,
// but WITHOUT WARRANTY OF ANY KIND.
//

namespace LX.EasyDb
{
    /// <summary>
    /// Represents a transaction to be performed at a data source.
    /// </summary>
    public interface ITransaction : System.Data.IDbTransaction, IDbOperation
    {
        /// <summary>
        /// Gets the provider associated with this transaction.
        /// </summary>
        IDbProvider Provider { get; }
    }
}
