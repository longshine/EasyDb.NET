//
// LX.EasyDb.DbOperationEventArgs.cs
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
    /// Provides data for the Operating and Operated event.
    /// </summary>
    public class DbOperationEventArgs : EventArgs
    {
        /// <summary>
        /// Gets an object of <see cref="LX.EasyDb.IDbOperationBlock"/> that contains details of current operation.
        /// </summary>
        public IDbOperationBlock DataOperationBlock { get; private set; }

        /// <summary>
        /// Initializes.
        /// </summary>
        /// <param name="dataOperationBlock">an object of <see cref="LX.EasyDb.IDbOperationBlock"/> that contains details of current operation</param>
        public DbOperationEventArgs(IDbOperationBlock dataOperationBlock)
        {
            DataOperationBlock = dataOperationBlock;
        }
    }
}
