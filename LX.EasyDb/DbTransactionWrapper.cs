//
// LX.EasyDb.DbTransactionWrapper.cs
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
    abstract class DbTransactionWrapper : ITransaction
    {
        protected IDbTransaction _tran;
        private IDbConnection _conn;
        private IDbProvider _provider;

        public DbTransactionWrapper(IDbProvider provider, IDbTransaction transaction)
        {
            _tran = transaction;
            _conn = transaction.Connection;
            _provider = provider;
        }

        public virtual void Commit()
        {
            // do not commit again
            if (_tran.Connection != null)
            {
                _tran.Commit();
                _conn.Close();
            }
        }

        public virtual void Rollback()
        {
            // do not rollback again
            if (_tran.Connection != null)
            {
                _tran.Rollback();
                _conn.Close();
            }
        }

        public IDbProvider Provider { get { return _provider; } }

        public IDbConnection Connection
        {
            get { return _conn; }
        }

        public IsolationLevel IsolationLevel
        {
            get { return _tran.IsolationLevel; }
        }

        public void Dispose()
        {
            _tran.Dispose();
            _tran = null;
        }

        public IDbConnection GetConnection()
        {
            return _conn;
        }

        public IDbCommand CreateCommand(String commandText)
        {
            return CreateCommand(commandText, null, CommandType.Text);
        }

        public IDbCommand CreateCommand(String commandText, IDbDataParameter[] parameters)
        {
            return CreateCommand(commandText, parameters, CommandType.Text);
        }

        public IDbCommand CreateCommand(String commandText, IDbDataParameter[] parameters, CommandType commandType)
        {
            IDbCommand comm = DbProvider.CreateDbCommand(Connection, commandText, parameters, commandType);
            comm.Transaction = _tran;
            return Wrap(comm);
        }

        public IDbDataParameter CreateParameter(String name, Object value)
        {
            return _provider.CreateParameter(name, value);
        }

        public IDbDataAdapter CreateDataAdapter()
        {
            return _provider.CreateDataAdapter();
        }

        public IDbDataAdapter CreateDataAdapter(String selectCommandText, IDbDataParameter[] parameters, CommandType commandType)
        {
            IDbCommand selectCommand = DbProvider.CreateDbCommand(Connection, selectCommandText, parameters, commandType);
            selectCommand.Transaction = _tran;

            IDbDataAdapter ada = CreateDataAdapter();
            ada.SelectCommand = selectCommand;

            return ada;
        }

        protected virtual IDbCommand Wrap(IDbCommand comm)
        {
            return comm;
        }
    }
}
