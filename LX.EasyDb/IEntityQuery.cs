//
// LX.EasyDb.IEntityQuery.cs
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
    /// Provides querying methods with specific entity.
    /// </summary>
    public interface IEntityQuery
    {
        /// <summary>
        /// Executes a query and returns enumerable data typed as <see cref="System.Collections.Generic.IDictionary&lt;String, Object&gt;"/>.
        /// </summary>
        /// <param name="entity">the name of returned entities</param>
        /// <param name="sql">the text command to run against the data source</param>
        /// <param name="param">the object which contains parameters</param>
        /// <param name="buffered">buffer the result or not</param>
        /// <param name="commandTimeout">the wait time before terminating the attempt to execute a command and generating an error</param>
        /// <param name="commandType">the type indicates or specifies how the command is interpreted</param>
        /// <returns>an <see cref="System.Collections.Generic.IEnumerable&lt;IDictionary&gt;"/></returns>
        IEnumerable<IDictionary<String, Object>> Query(String entity, String sql, Object param = null, Boolean buffered = true, Int32? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// Checks if the table mapped to the entity exists.
        /// </summary>
        /// <param name="entity">the name of the entity</param>
        /// <returns>true if the table mapped to the entity exists, otherwise false</returns>
        Boolean ExistTable(String entity);
        /// <summary>
        /// Creates a table mapped to the entity.
        /// </summary>
        /// <param name="entity">the name of the entity</param>
        void CreateTable(String entity);
        /// <summary>
        /// Drops a table mapped to the entity.
        /// </summary>
        /// <param name="entity">the name of the entity</param>
        void DropTable(String entity);
        /// <summary>
        /// Inserts an entity and returns generated identity value if any.
        /// </summary>
        /// <param name="entity">the name of returned entity</param>
        /// <param name="item">the entity to insert</param>
        /// <param name="commandTimeout">the wait time before terminating the attempt to execute a command and generating an error</param>
        /// <returns>the generated identity value if any</returns>
        Int32 Insert(String entity, Object item, Int32? commandTimeout = null);
        /// <summary>
        /// Finds an entity by one or more keys.
        /// </summary>
        /// <param name="entity">the name of returned entity</param>
        /// <param name="id">the value of primary key(s)</param>
        /// <param name="commandTimeout">the wait time before terminating the attempt to execute a command and generating an error</param>
        /// <returns>an instance of the type, or a default value if nothing is found</returns>
        IDictionary<String, Object> Find(String entity, Object id, Int32? commandTimeout = null);
        /// <summary>
        /// Gets an entity by one or more keys, or throws an exception if no entity is found.
        /// </summary>
        /// <param name="entity">the name of returned entity</param>
        /// <param name="id">the value of primary key(s)</param>
        /// <param name="commandTimeout">the wait time before terminating the attempt to execute a command and generating an error</param>
        /// <returns>an instance of the type</returns>
        IDictionary<String, Object> Get(String entity, Object id, Int32? commandTimeout = null);
        /// <summary>
        /// Updates an entity.
        /// </summary>
        /// <param name="entity">the name of returned entity</param>
        /// <param name="item">the entity to update</param>
        /// <param name="commandTimeout">the wait time before terminating the attempt to execute a command and generating an error</param>
        /// <returns>true if updated, false if not found or not modified (for tracked entities)</returns>
        Boolean Update(String entity, Object item, Int32? commandTimeout = null);
        /// <summary>
        /// Deletes an entity.
        /// </summary>
        /// <param name="entity">the name of returned entity</param>
        /// <param name="item">the entity to delete</param>
        /// <param name="commandTimeout">the wait time before terminating the attempt to execute a command and generating an error</param>
        /// <returns>true if deleted, false if nothing happened</returns>
        Boolean Delete(String entity, Object item, Int32? commandTimeout = null);
    }
}
