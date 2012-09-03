//
// LX.EasyDb.EasyDbHelper.cs
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
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using LX.EasyDb.Configuration;

namespace LX.EasyDb
{
    /// <summary>
    /// Data Helper
    /// </summary>
    public static class EasyDbHelper
    {
        private static IDbProvider _dummy = new DummyProvider();
        private static IDbProvider _provider = _dummy;

        /// <summary>
        /// Gets the provider of EasyDbHelper.
        /// </summary>
        public static IDbProvider Provider
        {
            get { return _provider; }
            internal set
            {
                _provider = value == null ? _dummy : value;
            }
        }

        static EasyDbHelper()
        {
            LoadConfiguration();
        }

        /// <summary>
        /// Initializes EasyDbHelper.
        /// </summary>
        /// <param name="provider">the provider that implements the data source classes</param>
        /// <param name="connectionString">the string used to open a database</param>
        public static void Initialize(String provider, String connectionString)
        {
            Provider = DbProviderBuilder.NewBuilder(provider, connectionString, "EasyDbHelper").Build();
        }

        /// <summary>
        /// Initializes EasyDbHelper.
        /// </summary>
        /// <param name="config">the <see cref="LX.EasyDb.Configuration.EasyDbConfiguration"/></param>
        public static void Initialize(EasyDbConfiguration config)
        {
            if (config != null && config.Providers.Count > 0)
            {
                IDbProvider masterProvider = null;
                List<IDbProvider> slaveProviders = new List<IDbProvider>();

                DbProviderElement primaryProviderElement = config.MasterProvider;
                if (null == primaryProviderElement)
                    throw new ConfigurationErrorsException("Master provider is not found.");
                masterProvider = CreateDbProvider(primaryProviderElement);
                if (masterProvider == null)
                    throw new ConfigurationErrorsException("Unable to load " + primaryProviderElement.Name);

                if (config.SlaveEnabled)
                {
                    foreach (DbProviderElement element in config.Providers)
                    {
                        if (element.Equals(primaryProviderElement))
                            continue;
                        IDbProvider slave = CreateDbProvider(element);
                        if (slave == null)
                            throw new ConfigurationErrorsException("Unable to load " + element.Name);
                        slaveProviders.Add(slave);
                    }
                    Provider = new MasterSlaveDbProvider(masterProvider, slaveProviders);
                }
                else
                {
                    Provider = masterProvider;
                }
            }
        }

        private static void LoadConfiguration()
        {
            try
            {
                Initialize((EasyDbConfiguration)ConfigurationManager.GetSection(EasyDbConfiguration.SECTION));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static IDbProvider CreateDbProvider(DbProviderElement config)
        {
            String connStr = null;
            if (String.IsNullOrEmpty(config.ConnectionStringName))
            { 
                connStr = config.ConnectionString;
            }
            else
            {
                ConnectionStringSettings css = ConfigurationManager.ConnectionStrings[config.ConnectionStringName];
                if (css != null)
                    connStr = css.ConnectionString;
            }
            return DbProviderBuilder.NewBuilder(config.Provider, connStr, config.Name).Build();
        }

        /// <summary>
        /// Gets a new <see cref="System.Data.IDbConnection"/>.
        /// </summary>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <returns><see cref="System.Data.IDbConnection"/></returns>
        public static IDbConnection GetConnection()
        {
            return Provider.GetConnection();
        }

        /// <summary>
        /// Creates a new instance of an <see cref="System.Data.IDbDataParameter"/> object.
        /// </summary>
        /// <param name="name">the name of the parameter to map</param>
        /// <param name="value">an Object that is the value of the parameter</param>
        /// <returns><see cref="System.Data.IDbDataParameter"/></returns>
        public static IDbDataParameter CreateParameter(String name, Object value)
        {
            return Provider.CreateParameter(name, value);
        }

        /// <summary>
        /// Executes an command statement and returns the number of rows affected.
        /// </summary>
        /// <param name="command">the text command to run against the data source</param>
        /// <returns>the number of rows affected</returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static Int32 ExecuteNonQuery(String command)
        {
            return ExecuteNonQuery(command, null, CommandType.Text);
        }

        /// <summary>
        /// Executes an command statement and returns the number of rows affected.
        /// </summary>
        /// <param name="command">the text command to run against the data source</param>
        /// <param name="parameters">the collection of parameters</param>
        /// <returns>the number of rows affected</returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static Int32 ExecuteNonQuery(String command, IDbDataParameter[] parameters)
        {
            return ExecuteNonQuery(command, parameters, CommandType.Text);
        }

        /// <summary>
        /// Executes an command statement and returns the number of rows affected.
        /// </summary>
        /// <param name="command">the text command to run against the data source</param>
        /// <param name="parameters">the collection of parameters</param>
        /// <param name="commandType">the type indicates or specifies how the command is interpreted</param>
        /// <returns>the number of rows affected</returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static Int32 ExecuteNonQuery(String command, IDbDataParameter[] parameters, CommandType commandType)
        {
            return Provider.CreateCommand(command, parameters, commandType).ExecuteNonQuery();
        }

        /// <summary>
        /// Executes an command statement and builds an <see cref="System.Data.IDataReader"/> with CloseConnection behavior.
        /// </summary>
        /// <param name="command">the text command to run against the data source</param>
        /// <returns><see cref="System.Data.IDataReader"/></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static IDataReader ExecuteReader(String command)
        {
            return ExecuteReader(command, null, CommandType.Text);
        }

        /// <summary>
        /// Executes an command statement and builds an <see cref="System.Data.IDataReader"/> with CloseConnection behavior.
        /// </summary>
        /// <param name="command">the text command to run against the data source</param>
        /// <param name="parameters">the collection of parameters</param>
        /// <returns><see cref="System.Data.IDataReader"/></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static IDataReader ExecuteReader(String command, IDbDataParameter[] parameters)
        {
            return ExecuteReader(command, parameters, CommandType.Text);
        }

        /// <summary>
        /// Executes an command statement and builds an <see cref="System.Data.IDataReader"/> with CloseConnection behavior.
        /// </summary>
        /// <param name="command">the text command to run against the data source</param>
        /// <param name="parameters">the collection of parameters</param>
        /// <param name="commandType">the type indicates or specifies how the command is interpreted</param>
        /// <returns><see cref="System.Data.IDataReader"/></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static IDataReader ExecuteReader(String command, IDbDataParameter[] parameters, CommandType commandType)
        {
            return ExecuteReader(command, parameters, CommandType.Text, CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// Executes an command statement and builds an <see cref="System.Data.IDataReader"/>.
        /// </summary>
        /// <param name="command">the text command to run against the data source</param>
        /// <param name="behavior">one of the <see cref="System.Data.CommandBehavior"/> values. </param>
        /// <returns><see cref="System.Data.IDataReader"/></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static IDataReader ExecuteReader(String command, CommandBehavior behavior)
        {
            return ExecuteReader(command, null, CommandType.Text, behavior);
        }

        /// <summary>
        /// Executes an command statement and builds an <see cref="System.Data.IDataReader"/>.
        /// </summary>
        /// <param name="command">the text command to run against the data source</param>
        /// <param name="parameters">the collection of parameters</param>
        /// <param name="behavior">one of the <see cref="System.Data.CommandBehavior"/> values. </param>
        /// <returns><see cref="System.Data.IDataReader"/></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static IDataReader ExecuteReader(String command, IDbDataParameter[] parameters, CommandBehavior behavior)
        {
            return ExecuteReader(command, parameters, CommandType.Text, behavior);
        }

        /// <summary>
        /// Executes an command statement and builds an <see cref="System.Data.IDataReader"/>.
        /// </summary>
        /// <param name="command">the text command to run against the data source</param>
        /// <param name="parameters">the collection of parameters</param>
        /// <param name="commandType">the type indicates or specifies how the command is interpreted</param>
        /// <param name="behavior">one of the <see cref="System.Data.CommandBehavior"/> values. </param>
        /// <returns><see cref="System.Data.IDataReader"/></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static IDataReader ExecuteReader(String command, IDbDataParameter[] parameters, CommandType commandType, CommandBehavior behavior)
        {
            return Provider.CreateCommand(command, parameters, commandType).ExecuteReader(behavior);
        }

        /// <summary>
        /// Executes an command statement, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="command">the text command to run against the data source</param>
        /// <returns>the first column of the first row in the resultset</returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static Object ExecuteScalar(String command)
        {
            return ExecuteScalar(command, null, CommandType.Text);
        }

        /// <summary>
        /// Executes an command statement, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="command">the text command to run against the data source</param>
        /// <param name="parameters">the collection of parameters</param>
        /// <returns>the first column of the first row in the resultset</returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static Object ExecuteScalar(String command, IDbDataParameter[] parameters)
        {
            return ExecuteScalar(command, parameters, CommandType.Text);
        }

        /// <summary>
        /// Executes an command statement, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="command">the text command to run against the data source</param>
        /// <param name="parameters">the collection of parameters</param>
        /// <param name="commandType">the type indicates or specifies how the command is interpreted</param>
        /// <returns>the first column of the first row in the resultset</returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static Object ExecuteScalar(String command, IDbDataParameter[] parameters, CommandType commandType)
        {
            return Provider.CreateCommand(command, parameters, commandType).ExecuteScalar();
        }

        /// <summary>
        /// Creates a new instance of an <see cref="System.Data.IDbDataAdapter"/> object.
        /// </summary>
        /// <returns><see cref="System.Data.IDbDataAdapter"/></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static IDbDataAdapter CreateDataAdapter()
        {
            return Provider.CreateDataAdapter();
        }

        /// <summary>
        /// Creates a new instance of an <see cref="System.Data.IDbDataAdapter"/> object.
        /// </summary>
        /// <param name="command">the text command to run against the data source</param>
        /// <param name="parameters">the collection of parameters</param>
        /// <param name="commandType">the type indicates or specifies how the command is interpreted</param>
        /// <returns><see cref="System.Data.IDbDataAdapter"/></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static IDbDataAdapter CreateDataAdapter(String command, IDbDataParameter[] parameters, CommandType commandType)
        {
            return Provider.CreateDataAdapter(command, parameters, commandType);
        }

        /// <summary>
        /// Starts a database transaction.
        /// </summary>
        /// <returns>An <see cref="LX.EasyDb.ITransaction"/> representing the new transaction.</returns>
        public static ITransaction BeginTransaction()
        {
            return Provider.BeginTransaction();
        }
    }
}
