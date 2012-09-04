//
// LX.EasyDb.DbProvider.cs
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
using System.Data;

namespace LX.EasyDb
{
    /// <summary>
    /// Provides a set of methods to interact with the data source.
    /// </summary>
    public sealed class DbProvider : IDbProvider
    {
        /// <summary>
        /// Initializes with a <see cref="System.Data.Common.DbProviderFactory"/>.
        /// </summary>
        /// <param name="factory"></param>
        public DbProvider(System.Data.Common.DbProviderFactory factory)
        {
            Factory = factory;
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Data.Common.DbProviderFactory"/> that creating instances of a provider's implementation of the data source classes.
        /// </summary>
        public System.Data.Common.DbProviderFactory Factory { get; set; }

        /// <summary>
        /// Gets or sets the name of this provider.
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// Gets or sets the string used to open a database.
        /// </summary>
        public String ConnectionString { get; set; }

        /// <summary>
        /// Starts a database transaction.
        /// </summary>
        /// <returns>An <see cref="LX.EasyDb.ITransaction"/> representing the new transaction.</returns>
        public ITransaction BeginTransaction()
        {
            IDbConnection conn = GetConnection();
            conn.Open();
            return new ManagedDbTransaction(this, conn.BeginTransaction());
        }

        /// <summary>
        /// Gets a new connection.
        /// </summary>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <returns><see cref="System.Data.IDbConnection"/></returns>
        public IDbConnection GetConnection()
        {
            if (String.IsNullOrEmpty(ConnectionString))
                throw new InvalidOperationException("ConnectionString shoud not be empty.");
            //RaiseSqlExecutingEvent(DataOperationBlock.DataOperation.GetConnection, null, null, default(Boolean));
            IDbConnection conn = Factory.CreateConnection();
            conn.ConnectionString = ConnectionString;
            return conn;
        }

        /// <summary>
        /// Creates a new instance of an <see cref="System.Data.IDbDataParameter"/> object.
        /// </summary>
        /// <param name="name">the name of the parameter to map</param>
        /// <param name="value">an Object that is the value of the parameter</param>
        /// <returns><see cref="System.Data.IDbDataParameter"/></returns>
        public IDbDataParameter CreateParameter(String name, Object value)
        {
            IDbDataParameter p = Factory.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            return p;
        }

        /// <summary>
        /// Creates and returns a <see cref="System.Data.IDbCommand"/> object. 
        /// </summary>
        /// <param name="commandText">the text command to run against the data source</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <returns><see cref="System.Data.IDbCommand"/></returns>
        public IDbCommand CreateCommand(String commandText)
        {
            return CreateCommand(commandText, null, CommandType.Text);
        }

        /// <summary>
        /// Creates and returns a <see cref="System.Data.IDbCommand"/> object. 
        /// </summary>
        /// <param name="commandText">the text command to run against the data source</param>
        /// <param name="parameters">the collection of parameters</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <returns><see cref="System.Data.IDbCommand"/></returns>
        public IDbCommand CreateCommand(String commandText, IDbDataParameter[] parameters)
        {
            return CreateCommand(commandText, parameters, CommandType.Text);
        }

        /// <summary>
        /// Creates and returns a <see cref="System.Data.IDbCommand"/> object. 
        /// </summary>
        /// <param name="commandText">the text command to run against the data source</param>
        /// <param name="parameters">the collection of parameters</param>
        /// <param name="commandType">the type indicates or specifies how the command is interpreted</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <returns><see cref="System.Data.IDbCommand"/></returns>
        public IDbCommand CreateCommand(String commandText, IDbDataParameter[] parameters, CommandType commandType)
        {
            return CreateManagedCommand(GetConnection(), commandText, parameters, commandType);
        }

        /// <summary>
        /// Creates a new instance of an <see cref="System.Data.IDbDataAdapter"/> object.
        /// </summary>
        /// <returns><see cref="System.Data.IDbDataAdapter"/></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        public IDbDataAdapter CreateDataAdapter()
        {
            return Factory.CreateDataAdapter();
        }

        /// <summary>
        /// Creates a new instance of an <see cref="System.Data.IDbDataAdapter"/> object.
        /// </summary>
        /// <param name="selectCommandText">the text command to run against the data source</param>
        /// <param name="parameters">the collection of parameters</param>
        /// <param name="commandType">the type indicates or specifies how the command is interpreted</param>
        /// <returns><see cref="System.Data.IDbDataAdapter"/></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        public IDbDataAdapter CreateDataAdapter(String selectCommandText, IDbDataParameter[] parameters, CommandType commandType)
        {
            IDbDataAdapter ada = Factory.CreateDataAdapter();
            ada.SelectCommand = CreateDbCommand(GetConnection(), selectCommandText, parameters, commandType);
            return ada;
        }

        private static IDbCommand CreateManagedCommand(IDbConnection conn, String commandText, IDbDataParameter[] parameters, CommandType commandType)
        {
            return new ManagedDbCommandWrapper(CreateDbCommand(conn, commandText, parameters, commandType));
        }

        /// <summary>
        /// Creates an instance of unwrapped DbCommand.
        /// This is helpful since wrapped commands cannot be used in IDbDataAdapter and ITransaction.
        /// </summary>
        internal static IDbCommand CreateDbCommand(IDbConnection conn, String commandText, IDbDataParameter[] parameters, CommandType commandType)
        {
            if (String.IsNullOrEmpty(commandText))
                throw new ArgumentNullException("commandText");

            IDbCommand comm = conn.CreateCommand();

            comm.CommandText = commandText;
            comm.CommandType = commandType;

            if (null != parameters && parameters.Length > 0)
            {
                foreach (IDbDataParameter parameter in parameters)
                {
                    comm.Parameters.Add(parameter);
                }
            }

            return comm;
        }

        class ManagedDbCommandWrapper : DbCommandWrapper
        {
            public ManagedDbCommandWrapper(IDbCommand comm)
                : base(comm)
            { }

            protected override System.Data.Common.DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
            {
                IDataReader reader = null;

                try
                {
                    Connection.Open();
                    reader = base.ExecuteDbDataReader(behavior);
                }
                catch (Exception ex)
                {
                    Connection.Close();
                    throw ex;
                }
                finally
                {
                    //Connection.Close();
                }

                return (System.Data.Common.DbDataReader)reader;
            }

            public override Int32 ExecuteNonQuery()
            {
                Int32 rowsAffected = -1;

                try
                {
                    Connection.Open();
                    rowsAffected = base.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    Connection.Close();
                }

                return rowsAffected;
            }

            public override Object ExecuteScalar()
            {
                Object result = null;

                try
                {
                    Connection.Open();
                    result = base.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    Connection.Close();
                }

                return result;
            }
        }

        class ManagedDbTransaction : DbTransactionWrapper
        {
            public ManagedDbTransaction(DbProvider provider, IDbTransaction tran)
                : base(provider, tran)
            {
            }

            protected override IDbCommand Wrap(IDbCommand comm)
            {
                return new DbCommandWrapper(comm);
            }
        }
    }
}
