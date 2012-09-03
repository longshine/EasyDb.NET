//
// LX.EasyDb.DbExceptionEventArgs.cs
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

namespace LX.EasyDb
{
    /// <summary>
    /// Provides data for the Exception event.
    /// </summary>
    public class DbExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// Gets an object of <see cref="LX.EasyDb.IDbOperationBlock"/> that contains details of current operation.
        /// </summary>
        public IDbOperationBlock DataOperationBlock { get; private set; }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Initializes.
        /// </summary>
        /// <param name="dataOperationBlock">an object of <see cref="LX.EasyDb.IDbOperationBlock"/> that contains details of current operation</param>
        /// <param name="exception">the exception</param>
        public DbExceptionEventArgs(IDbOperationBlock dataOperationBlock, Exception exception)
        {
            DataOperationBlock = dataOperationBlock;
            Exception = exception;
        }
    }
}
