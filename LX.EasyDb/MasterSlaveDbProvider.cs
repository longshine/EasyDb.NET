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
        
        /// <summary>
        /// Occurs when an exception occured in slaves.
        /// </summary>
        public event EventHandler<DbExceptionEventArgs> SlaveException;

        private static OperationHandler ExecuteNonQueryHandler;
        private List<IDbProvider> _slaveProviders = new List<IDbProvider>();

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
            Operated += new EventHandler<DbOperationEventArgs>(OnPrimaryProviderOperated);
            ExecuteNonQueryHandler = ExecuteNonQuery;
        }

        /// <summary>
        /// Gets the slave providers.
        /// </summary>
        public List<IDbProvider> SlaveProviders { get { return _slaveProviders; } }

        private void RaiseExceptionEvent(DbOperationType operation, String command,
            IDbDataParameter[] parameters, CommandType commandType, IDbProvider provider, Exception ex)
        {
            if (SlaveException != null)
                SlaveException(provider, new DbExceptionEventArgs(new DbOperationBlock(operation, command, parameters, commandType), ex));
        }

        private void OnPrimaryProviderOperated(Object sender, DbOperationEventArgs e)
        {
            IDbOperationBlock dob = e.DataOperationBlock;
            OperationHandler handler = null;
            
            switch (dob.Operation)
            {
                case DbOperationType.ExecuteNonQuery:
                    handler = ExecuteNonQueryHandler;
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
            IDbDataParameter[] ps = new IDbDataParameter[dob.Parameters.Length];
            dob.Parameters.CopyTo(ps, 0);
            try
            {
                provider.CreateCommand(dob.CommandText, ps, dob.CommandType).ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                RaiseExceptionEvent(DbOperationType.ExecuteNonQuery, dob.CommandText, ps, dob.CommandType, provider, ex);
            }
        }
    }
}
