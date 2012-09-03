//
// LX.EasyDb.IDbOperation.cs
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
    /// Defines methods to manipulate data.
    /// </summary>
    public interface IDbOperation
    {
        /// <summary>
        /// Gets a new <see cref="System.Data.IDbConnection"/>.
        /// </summary>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <returns><see cref="System.Data.IDbConnection"/></returns>
        IDbConnection GetConnection();
        /// <summary>
        /// Creates a new instance of an <see cref="System.Data.IDbDataParameter"/> object.
        /// </summary>
        /// <param name="name">the name of the parameter to map</param>
        /// <param name="value">an Object that is the value of the parameter</param>
        /// <returns><see cref="System.Data.IDbDataParameter"/></returns>
        IDbDataParameter CreateParameter(String name, Object value);
        /// <summary>
        /// Creates and returns a <see cref="System.Data.IDbCommand"/> object. 
        /// </summary>
        /// <param name="command">the text command to run against the data source</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <returns><see cref="System.Data.IDbCommand"/></returns>
        IDbCommand CreateCommand(String command);
        /// <summary>
        /// Creates and returns a <see cref="System.Data.IDbCommand"/> object. 
        /// </summary>
        /// <param name="command">the text command to run against the data source</param>
        /// <param name="parameters">the collection of parameters</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <returns><see cref="System.Data.IDbCommand"/></returns>
        IDbCommand CreateCommand(String command, IDbDataParameter[] parameters);
        /// <summary>
        /// Creates and returns a <see cref="System.Data.IDbCommand"/> object. 
        /// </summary>
        /// <param name="command">the text command to run against the data source</param>
        /// <param name="parameters">the collection of parameters</param>
        /// <param name="commandType">the type indicates or specifies how the command is interpreted</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <returns><see cref="System.Data.IDbCommand"/></returns>
        IDbCommand CreateCommand(String command, IDbDataParameter[] parameters, CommandType commandType);
        /// <summary>
        /// Creates a new instance of an <see cref="System.Data.IDbDataAdapter"/> object.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <returns><see cref="System.Data.IDbDataAdapter"/></returns>
        IDbDataAdapter CreateDataAdapter();
        /// <summary>
        /// Creates a new instance of an <see cref="System.Data.IDbDataAdapter"/> object.
        /// </summary>
        /// <param name="selectCommandText">the text command to run against the data source</param>
        /// <param name="parameters">the collection of parameters</param>
        /// <param name="commandType">the type indicates or specifies how the command is interpreted</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <returns><see cref="System.Data.IDbDataAdapter"/></returns>
        IDbDataAdapter CreateDataAdapter(String selectCommandText, IDbDataParameter[] parameters, CommandType commandType);
    }

    /// <summary>
    /// Defines types of data manipulations.
    /// </summary>
    public enum DbOperationType
    {
        /// <summary>
        /// Executes an command statement and returns the number of rows affected.
        /// </summary>
        ExecuteNonQuery,
        /// <summary>
        /// Executes an command statement and builds an <see cref="System.Data.IDataReader"/>.
        /// </summary>
        ExecuteReader,
        /// <summary>
        /// Executes an command statement, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        ExecuteScalar,
        /// <summary>
        /// Gets a new <see cref="System.Data.IDbConnection"/>.
        /// </summary>
        CreateConnection,
        /// <summary>
        /// Creates a new instance of an <see cref="System.Data.IDbDataParameter"/> object.
        /// </summary>
        CreateParameter,
        /// <summary>
        /// Creates a new instance of an <see cref="System.Data.IDbDataAdapter"/> object.
        /// </summary>
        CreateDataAdapter
    }
}
