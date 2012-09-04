//
// LX.EasyDb.MasterSlaveDbProvider.cs
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
    /// Provides capability to enable master/slave data operations.
    /// </summary>
    public class MasterSlaveDbProvider : ObservableDbProvider
    {
        delegate void OperationHandler(IDbProvider provider, IDbOperationBlock dob);
        delegate void TransactionHandler(ITransaction transaction, IDbOperationBlock dob);
        
        /// <summary>
        /// Occurs when an exception occured in slaves.
        /// </summary>
        public event EventHandler<DbExceptionEventArgs> SlaveException;

        private List<IDbProvider> _slaveProviders = new List<IDbProvider>();
        private IDictionary<ITransaction, List<ITransaction>> _transactionMap = new Dictionary<ITransaction, List<ITransaction>>();

        /// <summary>
        /// Initializes.
        /// </summary>
        /// <param name="masterProvider">a <see cref="LX.EasyDb.IDbProvider"/> as the master</param>
        /// <param name="slaveProviders">a set of <see cref="LX.EasyDb.IDbProvider"/> as slaves</param>
        public MasterSlaveDbProvider(IDbProvider masterProvider, IEnumerable<IDbProvider> slaveProviders)
            : base(masterProvider)
        {
            if (slaveProviders != null)
                _slaveProviders.AddRange(slaveProviders);
            Operating += new EventHandler<DbOperationEventArgs>(OnPrimaryProviderOperated);
            Transacting += new EventHandler<DbTransactionEventArgs>(OnPrimaryProviderTransacting);
        }

        /// <summary>
        /// Gets the slave providers.
        /// </summary>
        public List<IDbProvider> SlaveProviders { get { return _slaveProviders; } }

        /// <summary>
        /// Starts a database transaction.
        /// </summary>
        /// <returns>An <see cref="LX.EasyDb.ITransaction"/> representing the new transaction.</returns>
        public override ITransaction BeginTransaction()
        {
            ITransaction tran = base.BeginTransaction();
            List<ITransaction> slaveTrans = new List<ITransaction>();
            foreach (IDbProvider provider in _slaveProviders)
            {
                slaveTrans.Add(provider.BeginTransaction());
            }
            AddTransaction(tran, slaveTrans);
            return tran;
        }

        private void RaiseExceptionEvent(DbOperationType operation, String command,
            IDbDataParameter[] parameters, CommandType commandType, IDbProvider provider, Exception ex)
        {
            if (SlaveException != null)
                SlaveException(provider, new DbExceptionEventArgs(new DbOperationBlock(operation, command, parameters, commandType), ex));
        }

        private IDbDataParameter[] Clone(IDbDataParameter[] parameters)
        {
            IDbDataParameter[] ps = new IDbDataParameter[parameters.Length];
            parameters.CopyTo(ps, 0);
            return ps;
        }

        private void OnPrimaryProviderOperated(Object sender, DbOperationEventArgs e)
        {
            IDbOperationBlock dob = e.DataOperationBlock;
            OperationHandler handler = null;
            
            switch (dob.Operation)
            {
                case DbOperationType.ExecuteNonQuery:
                    handler = ExecuteNonQuery;
                    break;
                default:
                    break;
            }

            if (handler != null)
            {
                foreach (IDbProvider provider in _slaveProviders)
                {
                    handler.BeginInvoke(provider, dob, null, null);
                }
            }
        }

        private void ExecuteNonQuery(IDbProvider provider, IDbOperationBlock dob)
        {
            IDbDataParameter[] ps = Clone(dob.Parameters);
            try
            {
                provider.CreateCommand(dob.CommandText, ps, dob.CommandType).ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                RaiseExceptionEvent(DbOperationType.ExecuteNonQuery, dob.CommandText, ps, dob.CommandType, provider, ex);
            }
        }

        private void AddTransaction(ITransaction tran, List<ITransaction> slaveTrans)
        {
            _transactionMap.Add(tran, slaveTrans);
        }

        private void RemoveTransaction(ITransaction tran)
        {
            _transactionMap.Remove(tran);
        }

        private void OnPrimaryProviderTransacting(Object sender, DbTransactionEventArgs e)
        {
            ITransaction tran = (ITransaction)sender;
            TransactionHandler handler = null;
            Boolean remove = false;

            switch (e.Type)
            {
                case DbTransactionEventType.Commit:
                    handler = CommitTransaction;
                    remove = true;
                    break;
                case DbTransactionEventType.Rollback:
                    handler = RollbackTransaction;
                    remove = true;
                    break;
                case DbTransactionEventType.Operation:
                    handler = OperateTransaction;
                    break;
                default:
                    break;
            }

            if (handler != null)
            {
                foreach (ITransaction slaveTran in _transactionMap[tran])
                {
                    handler(slaveTran, e.DataOperationBlock);
                }
            }

            if (remove)
                RemoveTransaction(tran);
        }

        private void OperateTransaction(ITransaction transaction, IDbOperationBlock dob)
        {
            switch (dob.Operation)
            {
                case DbOperationType.ExecuteNonQuery:
                    IDbDataParameter[] ps = Clone(dob.Parameters);
                    try
                    {
                        transaction.CreateCommand(dob.CommandText, ps, dob.CommandType).ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        RaiseExceptionEvent(DbOperationType.ExecuteNonQuery, dob.CommandText, ps, dob.CommandType, transaction.Provider, ex);
                    }
                    break;
                default:
                    break;
            }
        }

        private void CommitTransaction(ITransaction transaction, IDbOperationBlock dob)
        {
            transaction.Commit();
        }

        private void RollbackTransaction(ITransaction transaction, IDbOperationBlock dob)
        {
            transaction.Rollback();
        }
    }
}
