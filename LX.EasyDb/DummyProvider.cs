//
// LX.EasyDb.DummyProvider.cs
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
    sealed class DummyProvider : IDbProvider
    {
        public System.Data.Common.DbProviderFactory Factory
        {
            get { throw UninitializedException(); }
            set { throw UninitializedException(); }
        }

        public String Name
        {
            get { return "Uninitialized"; }
            set { throw UninitializedException(); }
        }

        public String ConnectionString
        {
            get { throw UninitializedException(); }
            set { throw UninitializedException(); }
        }

        public String ParamPrefix
        {
            get { throw UninitializedException(); }
        }

        public String CustomParamPrefix
        {
            get { throw UninitializedException(); }
            set { throw UninitializedException(); }
        }

        public IDbCommand CreateCommand(String command)
        {
            throw UninitializedException();
        }

        public IDbCommand CreateCommand(String command, IDbDataParameter[] parameters)
        {
            throw UninitializedException();
        }

        public IDbConnection GetConnection()
        {
            throw UninitializedException();
        }

        public IDbDataParameter CreateParameter(String name, Object value)
        {
            throw UninitializedException();
        }

        public IDbCommand CreateCommand(String command, IDbDataParameter[] parameters, CommandType commandType)
        {
            throw UninitializedException();
        }

        public IDbDataAdapter CreateDataAdapter()
        {
            throw UninitializedException();
        }

        public IDbDataAdapter CreateDataAdapter(String command, IDbDataParameter[] parameters, CommandType commandType)
        {
            throw UninitializedException();
        }
        
        public ITransaction BeginTransaction()
        {
            throw UninitializedException();
        }

        private Exception UninitializedException()
        {
            return new InvalidOperationException("Uninitialized provider");
        }
    }
}
