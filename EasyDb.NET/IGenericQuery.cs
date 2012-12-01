//
// LX.EasyDb.IGenericQuery.cs
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
using System.Data;

namespace LX.EasyDb
{
    /// <summary>
    /// Provides generic querying methods.
    /// </summary>
    public interface IGenericQuery
    {
        /// <summary>
        /// Executes a query and returns enumerable data typed as T.
        /// </summary>
        /// <typeparam name="T">the type of returned entities</typeparam>
        /// <param name="sql">the text command to run against the data source</param>
        /// <param name="param">the object which contains parameters</param>
        /// <param name="buffered">buffer the result or not</param>
        /// <param name="commandTimeout">the wait time before terminating the attempt to execute a command and generating an error</param>
        /// <param name="commandType">the type indicates or specifies how the command is interpreted</param>
        /// <returns>an <see cref="System.Collections.Generic.IEnumerable&lt;T&gt;"/></returns>
        IEnumerable<T> Query<T>(String sql, Object param = null, Boolean buffered = true, Int32? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// Checks if the table mapped to the entity exists.
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <returns>true if the table mapped to the entity exists, otherwise false</returns>
        Boolean ExistTable<T>();
        /// <summary>
        /// Creates a table mapped to the entity.
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        void CreateTable<T>();
        /// <summary>
        /// Drops a table mapped to the entity.
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        void DropTable<T>();
        /// <summary>
        /// Inserts an entity and returns generated identity value if any.
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="item">the entity to insert</param>
        /// <param name="commandTimeout">the wait time before terminating the attempt to execute a command and generating an error</param>
        /// <returns>the generated identity value if any</returns>
        Int32 Insert<T>(T item, Int32? commandTimeout = null);
        /// <summary>
        /// Finds an entity by one or more keys.
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="id">the value of primary key(s)</param>
        /// <param name="commandTimeout">the wait time before terminating the attempt to execute a command and generating an error</param>
        /// <returns>an instance of T, or default(T) if nothing is found</returns>
        T Find<T>(Object id, Int32? commandTimeout = null);
        /// <summary>
        /// Gets an entity by one or more keys, or throws an exception if no entity is found.
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="id">the value of primary key(s)</param>
        /// <param name="commandTimeout">the wait time before terminating the attempt to execute a command and generating an error</param>
        /// <returns>an instance of T</returns>
        T Get<T>(Object id, Int32? commandTimeout = null);
        /// <summary>
        /// Updates an entity.
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="item">the entity to update</param>
        /// <param name="commandTimeout">the wait time before terminating the attempt to execute a command and generating an error</param>
        /// <returns>true if updated, false if not found or not modified (for tracked entities)</returns>
        Boolean Update<T>(T item, Int32? commandTimeout = null);
        /// <summary>
        /// Deletes an entity.
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="item">the entity to delete</param>
        /// <param name="commandTimeout">the wait time before terminating the attempt to execute a command and generating an error</param>
        /// <returns>true if deleted, false if nothing happened</returns>
        Boolean Delete<T>(T item, Int32? commandTimeout = null);
    }
}
