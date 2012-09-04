//
// LX.EasyDb.DbTransactionEventArgs.cs
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

namespace LX.EasyDb
{
    /// <summary>
    /// Provides data for the transaction event.
    /// </summary>
    public class DbTransactionEventArgs : EventArgs
    {
        /// <summary>
        /// Gets an object of <see cref="LX.EasyDb.IDbOperationBlock"/> that contains details of current operation.
        /// </summary>
        public IDbOperationBlock DataOperationBlock { get; private set; }

        /// <summary>
        /// Gets the type of this event.
        /// </summary>
        public DbTransactionEventType Type { get; private set; }

        /// <summary>
        /// Initializes.
        /// </summary>
        /// <param name="type">one of the <see cref="LX.EasyDb.DbTransactionEventType"/></param>
        /// <param name="dataOperationBlock">an object of <see cref="LX.EasyDb.IDbOperationBlock"/> that contains details of current operation</param>
        public DbTransactionEventArgs(DbTransactionEventType type, IDbOperationBlock dataOperationBlock)
        {
            Type = type;
            DataOperationBlock = dataOperationBlock;
        }
    }

    /// <summary>
    /// Defines types of transaction events.
    /// </summary>
    public enum DbTransactionEventType
    {
        /// <summary>
        /// Commit.
        /// </summary>
        Commit,
        /// <summary>
        /// Rollback.
        /// </summary>
        Rollback,
        /// <summary>
        /// Operate.
        /// </summary>
        Operation
    }
}
