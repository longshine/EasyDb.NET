//
// LX.EasyDb.DbCommandWrapper.cs
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
using System.Data.Common;

namespace LX.EasyDb
{
    abstract class DbCommandWrapper : DbCommand
    {
        protected IDbCommand _comm;

        public DbCommandWrapper(IDbCommand comm)
        {
            _comm = comm;
        }

        public override void Cancel()
        {
            _comm.Cancel();
        }

        public override String CommandText
        {
            get { return _comm.CommandText; }
            set { _comm.CommandText = value; }
        }

        public override Int32 CommandTimeout
        {
            get { return _comm.CommandTimeout; }
            set { _comm.CommandTimeout = value; }
        }

        public override CommandType CommandType
        {
            get { return _comm.CommandType; }
            set { _comm.CommandType = value; }
        }

        protected override DbParameter CreateDbParameter()
        {
            return (DbParameter)_comm.CreateParameter();
        }

        protected override DbConnection DbConnection
        {
            get { return (DbConnection)_comm.Connection; }
            set { _comm.Connection = value; }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return (DbParameterCollection)_comm.Parameters; }
        }

        protected override DbTransaction DbTransaction
        {
            get { return (DbTransaction)_comm.Transaction; }
            set { _comm.Transaction = value; }
        }

        public override Boolean DesignTimeVisible
        {
            get { return ((DbCommand)_comm).DesignTimeVisible; }
            set { ((DbCommand)_comm).DesignTimeVisible = value; }
        }

        public override void Prepare()
        {
            _comm.Prepare();
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get { return _comm.UpdatedRowSource; }
            set { _comm.UpdatedRowSource = value; }
        }
    }
}
