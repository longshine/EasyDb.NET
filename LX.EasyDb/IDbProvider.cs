//
// LX.EasyDb.IDbProvider.cs
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
    /// Defines methods to manipulate data.
    /// </summary>
    public interface IDbProvider : IDbOperation
    {
        /// <summary>
        /// Gets or sets the <see cref="System.Data.Common.DbProviderFactory"/> that creating instances of a provider's implementation of the data source classes.
        /// </summary>
        System.Data.Common.DbProviderFactory Factory { get; set; }
        /// <summary>
        /// Gets or sets the name of this provider.
        /// </summary>
        String Name { get; set; }
        /// <summary>
        /// Gets or sets the string used to open a database.
        /// </summary>
        String ConnectionString { get; set; }
        /// <summary>
        /// Starts a database transaction.
        /// </summary>
        /// <returns>An <see cref="LX.EasyDb.ITransaction"/> representing the new transaction.</returns>
        ITransaction BeginTransaction();
    }
}
