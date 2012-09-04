//
// LX.EasyDb.ObservableDbProvider.cs
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
    /// Provides a set of methods for interacting with the data source, and an event for publishing operation events.
    /// </summary>
    public class ObservableDbProvider : IDbProvider
    {
        private IDbProvider _providerToObserve;
        private Dictionary<IDbTransaction, ITransaction> _transactionMap = new Dictionary<IDbTransaction, ITransaction>();

        /// <summary>
        /// Occurs when an operation is taken.
        /// </summary>
        public event EventHandler<DbOperationEventArgs> Operating;

        /// <summary>
        /// Occurs when a transaction takes actions.
        /// </summary>
        public event EventHandler<DbTransactionEventArgs> Transacting;

        /// <summary>
        /// Initializes with a <see cref="LX.EasyDb.IDbProvider"/> to be observed.
        /// </summary>
        /// <param name="providerToObserve">the <see cref="LX.EasyDb.IDbProvider"/> to be observed</param>
        public ObservableDbProvider(IDbProvider providerToObserve)
        {
            _providerToObserve = providerToObserve;
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Data.Common.DbProviderFactory"/> that creating instances of a provider's implementation of the data source classes.
        /// </summary>
        public System.Data.Common.DbProviderFactory Factory
        {
            get { return _providerToObserve.Factory; }
            set { _providerToObserve.Factory = value; }
        }

        /// <summary>
        /// Gets or sets the name of this provider.
        /// </summary>
        public String Name
        {
            get { return _providerToObserve.Name; }
            set { _providerToObserve.Name = value; }
        }

        /// <summary>
        /// Gets or sets the string used to open a database.
        /// </summary>
        public String ConnectionString
        {
            get { return _providerToObserve.ConnectionString; }
            set { _providerToObserve.ConnectionString = value; }
        }

        /// <summary>
        /// Starts a database transaction.
        /// </summary>
        /// <returns>An <see cref="LX.EasyDb.ITransaction"/> representing the new transaction.</returns>
        public virtual ITransaction BeginTransaction()
        {
            IDbConnection conn = GetConnection();
            conn.Open();
            IDbTransaction dbTran = conn.BeginTransaction();
            ObservableDbTransaction tran = new ObservableDbTransaction(this, dbTran);
            AddTransaction(dbTran, tran);
            return tran;
        }

        /// <summary>
        /// Gets a new connection.
        /// </summary>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <returns><see cref="System.Data.IDbConnection"/></returns>
        public IDbConnection GetConnection()
        {
            return _providerToObserve.GetConnection();
        }

        /// <summary>
        /// Creates a new instance of an <see cref="System.Data.IDbDataParameter"/> object.
        /// </summary>
        /// <param name="name">the name of the parameter to map</param>
        /// <param name="value">an Object that is the value of the parameter</param>
        /// <returns><see cref="System.Data.IDbDataParameter"/></returns>
        public IDbDataParameter CreateParameter(String name, Object value)
        {
            return _providerToObserve.CreateParameter(name, value);
        }

        /// <summary>
        /// Creates and returns a <see cref="System.Data.IDbCommand"/> object. 
        /// </summary>
        /// <param name="command">the text command to run against the data source</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <returns><see cref="System.Data.IDbCommand"/></returns>
        public IDbCommand CreateCommand(String command)
        {
            return CreateCommand(command, null, CommandType.Text);
        }

        /// <summary>
        /// Creates and returns a <see cref="System.Data.IDbCommand"/> object. 
        /// </summary>
        /// <param name="command">the text command to run against the data source</param>
        /// <param name="parameters">the collection of parameters</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <returns><see cref="System.Data.IDbCommand"/></returns>
        public IDbCommand CreateCommand(String command, IDbDataParameter[] parameters)
        {
            return CreateCommand(command, parameters, CommandType.Text);
        }

        /// <summary>
        /// Creates and returns a <see cref="System.Data.IDbCommand"/> object. 
        /// </summary>
        /// <param name="command">the text command to run against the data source</param>
        /// <param name="parameters">the collection of parameters</param>
        /// <param name="commandType">the type indicates or specifies how the command is interpreted</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <returns><see cref="System.Data.IDbCommand"/></returns>
        public IDbCommand CreateCommand(String command, IDbDataParameter[] parameters, CommandType commandType)
        {
            return new ObservableDbCommandWrapper(this, _providerToObserve.CreateCommand(command, parameters, commandType));
        }

        /// <summary>
        /// Creates a new instance of an <see cref="System.Data.IDbDataAdapter"/> object.
        /// </summary>
        /// <returns><see cref="System.Data.IDbDataAdapter"/></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        public IDbDataAdapter CreateDataAdapter()
        {
            return _providerToObserve.CreateDataAdapter();
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
            return _providerToObserve.CreateDataAdapter(selectCommandText, parameters, commandType);
        }

        private void AddTransaction(IDbTransaction dbTran, ITransaction tran)
        {
            _transactionMap.Add(dbTran, tran);
        }

        private void RemoveTransaction(IDbTransaction dbTran)
        {
            _transactionMap.Remove(dbTran);
        }

        private ITransaction GetTransaction(IDbTransaction dbTran)
        {
            return _transactionMap[dbTran];
        }

        private void RaiseOperatedEvent(DbOperationType operation, IDbCommand comm)
        {
            if (Operating != null && comm.Transaction == null)
                Operating(this, new DbOperationEventArgs(new DbOperationBlock(operation, comm.CommandText, comm.Parameters, comm.CommandType)));
            else if (Transacting != null && comm.Transaction != null)
                Transacting(GetTransaction(comm.Transaction), new DbTransactionEventArgs(DbTransactionEventType.Operation, new DbOperationBlock(operation, comm.CommandText, comm.Parameters, comm.CommandType)));
        }

        private void RaiseTransactingEvent(ITransaction transaction, DbTransactionEventType type)
        { 
            if (Transacting != null)
                Transacting(transaction, new DbTransactionEventArgs(type, null));
        }

        class ObservableDbCommandWrapper : DbCommandWrapper
        {
            private ObservableDbProvider _provider;

            public ObservableDbCommandWrapper(ObservableDbProvider provider, IDbCommand comm)
                : base(comm)
            {
                _provider = provider;
            }

            public override Int32 ExecuteNonQuery()
            {
                Int32 rowsAffected = base.ExecuteNonQuery();
                RaiseOperatedEvent(DbOperationType.ExecuteNonQuery);
                return rowsAffected;
            }

            protected override System.Data.Common.DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
            {
                System.Data.Common.DbDataReader reader = base.ExecuteDbDataReader(behavior);
                RaiseOperatedEvent(DbOperationType.ExecuteReader);
                return reader;
            }

            public override Object ExecuteScalar()
            {
                Object result = base.ExecuteScalar();
                RaiseOperatedEvent(DbOperationType.ExecuteScalar);
                return result;
            }

            private void RaiseOperatedEvent(DbOperationType operation)
            {
                _provider.RaiseOperatedEvent(operation, this);
            }
        }

        class ObservableDbTransaction : DbTransactionWrapper
        {
            private ObservableDbProvider _provider;

            public ObservableDbTransaction(ObservableDbProvider provider, IDbTransaction tran)
                : base(provider, tran)
            {
                _provider = provider;
            }

            public override void Commit()
            {
                base.Commit();
                _provider.RaiseTransactingEvent(this, DbTransactionEventType.Commit);
            }

            public override void Rollback()
            {
                base.Rollback();
                _provider.RaiseTransactingEvent(this, DbTransactionEventType.Rollback);
            }

            protected override IDbCommand Wrap(IDbCommand comm)
            {
                return new ObservableDbCommandWrapper(_provider, comm);
            }
        }
    }
}
