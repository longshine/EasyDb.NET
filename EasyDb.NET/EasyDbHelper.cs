//
// LX.EasyDb.EasyDbHelper.cs
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
using System.Configuration;
using System.Data;
using LX.EasyDb.Configuration;

namespace LX.EasyDb
{
    /// <summary>
    /// Easy Data Helper
    /// </summary>
    public static class EasyDbHelper
    {
        private static IConnectionFactory _factory = DummyConnectionFactory.Instance;

        /// <summary>
        /// Gets the connection factory of EasyDbHelper.
        /// </summary>
        public static IConnectionFactory ConnectionFactory
        {
            get { return _factory; }
            internal set { _factory = value == null ? DummyConnectionFactory.Instance : value; }
        }

        static EasyDbHelper()
        {
            LoadConfiguration();
        }

        private static void LoadConfiguration()
        {
            try
            {
                Initialize((EasyDbConfiguration)ConfigurationManager.GetSection(EasyDbConfiguration.SECTION));
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Initializes EasyDbHelper.
        /// </summary>
        /// <param name="provider">the provider that implements the data source classes</param>
        /// <param name="connectionString">the string used to open a database</param>
        /// <param name="dialect">the SQL dialect to apply</param>
        public static void Initialize(String provider, String connectionString, String dialect)
        {
            ConnectionFactory = ConnectionFactoryBuilder.NewBuilder(provider, connectionString, "EasyDbHelper", dialect).Build();
        }

        /// <summary>
        /// Initializes EasyDbHelper.
        /// </summary>
        /// <param name="config">the <see cref="LX.EasyDb.Configuration.EasyDbConfiguration"/></param>
        public static void Initialize(EasyDbConfiguration config)
        {
            if (config != null && config.Providers.Count > 0)
            {
                IConnectionFactory masterProvider = null;
                List<IConnectionFactory> slaveProviders = new List<IConnectionFactory>();

                DbProviderElement primaryProviderElement = config.MasterProvider;
                if (null == primaryProviderElement)
                    throw new ConfigurationErrorsException("Master provider is not found.");
                masterProvider = CreateConnectionFactory(primaryProviderElement);
                if (masterProvider == null)
                    throw new ConfigurationErrorsException("Unable to load " + primaryProviderElement.Name);

                // TODO slave mode
                ConnectionFactory = masterProvider;
            }
        }

        private static IConnectionFactory CreateConnectionFactory(DbProviderElement element)
        {
            return ConnectionFactoryBuilder.NewBuilder(element.Provider, GetConnectionString(element), element.Name, element.Dialect).Build();
        }

        private static string GetConnectionString(DbProviderElement element)
        {
            String connStr = null;
            if (String.IsNullOrEmpty(element.ConnectionStringName))
                connStr = element.ConnectionString;
            else
            {
                ConnectionStringSettings css = ConfigurationManager.ConnectionStrings[element.ConnectionStringName];
                if (css != null)
                    connStr = css.ConnectionString;
            }
            return connStr;
        }

        /// <summary>
        /// Gets a opened <see cref="LX.EasyDb.IConnection"/>.
        /// </summary>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <returns><see cref="System.Data.IDbConnection"/></returns>
        public static IConnection OpenConnection()
        {
            return ConnectionFactory.OpenConnection();
        }

        /// <summary>
        /// Executes an command statement and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">the text command to run against the data source</param>
        /// <param name="param">the object which contains parameters</param>
        /// <param name="commandTimeout">the wait time before terminating the attempt to execute a command and generating an error</param>
        /// <param name="commandType">the type indicates or specifies how the command is interpreted</param>
        /// <returns>the number of rows affected</returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static Int32 ExecuteNonQuery(String sql, Object param = null, Int32? commandTimeout = null, CommandType? commandType = null)
        {
            IConnection conn = OpenConnection();
            try
            {
                return conn.ExecuteNonQuery(sql, param, commandTimeout, commandType);
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// Executes an command statement and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="sql">the text command to run against the data source</param>
        /// <param name="param">the object which contains parameters</param>
        /// <param name="commandTimeout">the wait time before terminating the attempt to execute a command and generating an error</param>
        /// <param name="commandType">the type indicates or specifies how the command is interpreted</param>
        /// <returns>the first column of the first row in the resultset</returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static Object ExecuteScalar(String sql, Object param, Int32? commandTimeout = null, CommandType? commandType = null)
        {
            IConnection conn = OpenConnection();
            try
            {
                return conn.ExecuteScalar(sql, param, commandTimeout, commandType);
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// Executes an command statement and builds an <see cref="System.Data.IDataReader"/>.
        /// </summary>
        /// <param name="sql">the text command to run against the data source</param>
        /// <param name="param">the object which contains parameters</param>
        /// <param name="commandTimeout">the wait time before terminating the attempt to execute a command and generating an error</param>
        /// <param name="commandType">the type indicates or specifies how the command is interpreted</param>
        /// <param name="behavior">one of the <see cref="System.Data.CommandBehavior"/> values</param>
        /// <returns><see cref="System.Data.IDataReader"/></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static IDataReader ExecuteReader(String sql, Object param, Int32? commandTimeout = null, CommandType? commandType = null, CommandBehavior? behavior = null)
        {
            IConnection conn = OpenConnection();
            try
            {
                return conn.ExecuteReader(sql, param, commandTimeout, commandType);
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// Executes a query and returns enumerable data typed as <see cref="System.Collections.Generic.IDictionary&lt;String, Object&gt;"/>.
        /// </summary>
        /// <param name="sql">the text command to run against the data source</param>
        /// <param name="param">the object which contains parameters</param>
        /// <param name="buffered">buffer the result or not</param>
        /// <param name="commandTimeout">the wait time before terminating the attempt to execute a command and generating an error</param>
        /// <param name="commandType">the type indicates or specifies how the command is interpreted</param>
        /// <returns>an <see cref="System.Collections.Generic.IEnumerable&lt;IDictionary&gt;"/></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static IEnumerable<IDictionary<String, Object>> QueryDirect(String sql, Object param = null, Boolean buffered = true, Int32? commandTimeout = null, CommandType? commandType = null)
        {
            IConnection conn = OpenConnection();
            try
            {
                return conn.QueryDirect(sql, param, buffered, commandTimeout, commandType);
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
