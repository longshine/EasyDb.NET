//
// LX.EasyDb.ConnectionFactoryBuilder.cs
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
using System.Reflection;

namespace LX.EasyDb
{
    /// <summary>
    /// Builder that builds connection factories.
    /// </summary>
    public class ConnectionFactoryBuilder
    {
        private System.Data.Common.DbProviderFactory _factory;
        private Dialect _dialect;
        private String _connectionString;
        private String _name;

        private ConnectionFactoryBuilder()
        { }

        /// <summary>
        /// Sets the <see cref="System.Data.Common.DbProviderFactory"/>.
        /// </summary>
        /// <param name="provider">an instance of <see cref="System.Data.Common.DbProviderFactory"/></param>
        /// <returns></returns>
        public ConnectionFactoryBuilder SetDbProviderFactory(System.Data.Common.DbProviderFactory provider)
        {
            _factory = provider;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="System.Data.Common.DbProviderFactory"/> by its name.
        /// </summary>
        /// <param name="provider">the name of the provider</param>
        /// <returns></returns>
        public ConnectionFactoryBuilder SetDbProviderFactory(String provider)
        {
            _factory = GetProvider(provider);
            return this;
        }

        /// <summary>
        /// Sets the <see cref="Dialect"/> to use.
        /// </summary>
        /// <param name="dialect">an instance of <see cref="Dialect"/></param>
        /// <returns></returns>
        public ConnectionFactoryBuilder SetDialect(Dialect dialect)
        {
            _dialect = dialect;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="Dialect"/> by its name.
        /// </summary>
        /// <param name="dialect">the name of the dialect</param>
        /// <returns></returns>
        public ConnectionFactoryBuilder SetDialect(String dialect)
        {
            _dialect = Dialect.CreateDialect(dialect);
            return this;
        }

        /// <summary>
        /// Sets the connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public ConnectionFactoryBuilder SetConnectionString(String connectionString)
        {
            _connectionString = connectionString;
            return this;
        }

        /// <summary>
        /// Sets the name of the <see cref="ConnectionFactory"/> to build.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ConnectionFactoryBuilder SetName(String name)
        {
            _name = name;
            return this;
        }

        /// <summary>
        /// Builds a provider with given properties.
        /// </summary>
        /// <returns><see cref="LX.EasyDb.IConnectionFactory"/></returns>
        public IConnectionFactory Build()
        {
            ConnectionFactory cf = new ConnectionFactory(_factory);
            cf.ConnectionString = _connectionString;
            cf.Name = _name;
            cf.Dialect = _dialect;
            return cf;
        }

        /// <summary>
        /// Gets a builder.
        /// </summary>
        public static ConnectionFactoryBuilder NewBuilder()
        {
            return new ConnectionFactoryBuilder();
        }

        /// <summary>
        /// Gets a builder.
        /// </summary>
        /// <param name="factory">the <see cref="System.Data.Common.DbProviderFactory"/> that creating instances of a provider's implementation of the data source classes</param>
        /// <param name="connectionString">the connection used to open the database</param>
        /// <param name="name">the name of the provider</param>
        /// <param name="dialect">the SQL dialect to apply</param>
        /// <returns><see cref="LX.EasyDb.ConnectionFactoryBuilder"/></returns>
        public static ConnectionFactoryBuilder NewBuilder(System.Data.Common.DbProviderFactory factory, String connectionString, String name, Dialect dialect)
        {
            ConnectionFactoryBuilder builder = new ConnectionFactoryBuilder();
            builder._factory = factory;
            builder._connectionString = connectionString;
            builder._name = name;
            builder._dialect = dialect;
            return builder;
        }

        /// <summary>
        /// Gets a builder.
        /// </summary>
        /// <param name="provider">the provider that implements the data source classes</param>
        /// <param name="connectionString">the connection used to open the database</param>
        /// <param name="name">the name of the provider</param>
        /// <param name="dialect">the SQL dialect to apply</param>
        /// <returns><see cref="LX.EasyDb.ConnectionFactoryBuilder"/></returns>
        public static ConnectionFactoryBuilder NewBuilder(String provider, String connectionString, String name, String dialect)
        {
            ConnectionFactoryBuilder builder = new ConnectionFactoryBuilder();
            builder._factory = GetProvider(provider);
            builder._connectionString = connectionString;
            builder._name = name;
            builder._dialect = Dialect.CreateDialect(dialect);
            return builder;
        }

        private static System.Data.Common.DbProviderFactory GetProvider(String provider)
        {
            Object factory = null;
            Type factoryType = null;
            String nsName, assemblyName;
            ReflectHelper.GetTypeNames(provider, out nsName, out assemblyName);
            if (String.IsNullOrEmpty(assemblyName))
            {
                Assembly asm = ReflectHelper.LoadAssembly(nsName);
                factoryType = ReflectHelper.GetSubTypeInNamespace<System.Data.Common.DbProviderFactory>(asm, null);
            }
            else
            {
                Assembly asm = ReflectHelper.LoadAssembly(assemblyName);
                factoryType = ReflectHelper.GetSubTypeInNamespace<System.Data.Common.DbProviderFactory>(asm, nsName);
            }
            if (factoryType != null)
            {
                FieldInfo instanceField = factoryType.GetField("Instance");
                if (instanceField != null && instanceField.IsStatic)
                    factory = instanceField.GetValue(null);
                else
                    factory = factoryType.GetConstructor(Type.EmptyTypes).Invoke(null);
            }
            return (System.Data.Common.DbProviderFactory)factory;
        }

        #region Shortcuts

        /// <summary>
        /// Builds a <see cref="IConnectionFactory"/> for SQL Server with integrated security.
        /// </summary>
        /// <param name="server">the server name of the SQL Server</param>
        /// <param name="database">the database to connect</param>
        public static IConnectionFactory BuildSQLServer(String server, String database)
        {
            return BuildSQLServer(server, database, true, null, null, new Dialects.SQLServerDialect());
        }

        /// <summary>
        /// Builds a <see cref="IConnectionFactory"/> for SQL Server.
        /// </summary>
        /// <param name="server">the server name of the SQL Server</param>
        /// <param name="database">the database to connect</param>
        /// <param name="user">the name of user</param>
        /// <param name="password">the password of user</param>
        public static IConnectionFactory BuildSQLServer(String server, String database, String user, String password)
        {
            return BuildSQLServer(server, database, false, user, password, new Dialects.SQLServerDialect());
        }

        /// <summary>
        /// Builds a <see cref="IConnectionFactory"/> for SQL Server 2005 with integrated security.
        /// </summary>
        /// <param name="server">the server name of the SQL Server</param>
        /// <param name="database">the database to connect</param>
        public static IConnectionFactory BuildSQLServer2005(String server, String database)
        {
            return BuildSQLServer(server, database, true, null, null, new Dialects.SQLServer2005Dialect());
        }

        /// <summary>
        /// Builds a <see cref="IConnectionFactory"/> for SQL Server 2005.
        /// </summary>
        /// <param name="server">the server name of the SQL Server</param>
        /// <param name="database">the database to connect</param>
        /// <param name="user">the name of user</param>
        /// <param name="password">the password of user</param>
        public static IConnectionFactory BuildSQLServer2005(String server, String database, String user, String password)
        {
            return BuildSQLServer(server, database, false, user, password, new Dialects.SQLServer2005Dialect());
        }

        /// <summary>
        /// Builds a <see cref="IConnectionFactory"/> for SQL Server 2008 with integrated security.
        /// </summary>
        /// <param name="server">the server name of the SQL Server</param>
        /// <param name="database">the database to connect</param>
        public static IConnectionFactory BuildSQLServer2008(String server, String database)
        {
            return BuildSQLServer(server, database, true, null, null, new Dialects.SQLServer2008Dialect());
        }

        /// <summary>
        /// Builds a <see cref="IConnectionFactory"/> for SQL Server 2008.
        /// </summary>
        /// <param name="server">the server name of the SQL Server</param>
        /// <param name="database">the database to connect</param>
        /// <param name="user">the name of user</param>
        /// <param name="password">the password of user</param>
        public static IConnectionFactory BuildSQLServer2008(String server, String database, String user, String password)
        {
            return BuildSQLServer(server, database, false, user, password, new Dialects.SQLServer2008Dialect());
        }

        /// <summary>
        /// Builds a <see cref="IConnectionFactory"/> for SQL Server 2012 with integrated security.
        /// </summary>
        /// <param name="server">the server name of the SQL Server</param>
        /// <param name="database">the database to connect</param>
        public static IConnectionFactory BuildSQLServer2012(String server, String database)
        {
            return BuildSQLServer(server, database, true, null, null, new Dialects.SQLServer2012Dialect());
        }

        /// <summary>
        /// Builds a <see cref="IConnectionFactory"/> for SQL Server 2012.
        /// </summary>
        /// <param name="server">the server name of the SQL Server</param>
        /// <param name="database">the database to connect</param>
        /// <param name="user">the name of user</param>
        /// <param name="password">the password of user</param>
        public static IConnectionFactory BuildSQLServer2012(String server, String database, String user, String password)
        {
            return BuildSQLServer(server, database, false, user, password, new Dialects.SQLServer2012Dialect());
        }

        private static IConnectionFactory BuildSQLServer(String server, String database, Boolean integratedSecurity, String user, String password, Dialect dialect)
        {
            System.Data.SqlClient.SqlConnectionStringBuilder builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            builder.DataSource = server;
            builder.InitialCatalog = database;
            builder.IntegratedSecurity = integratedSecurity;
            if (!integratedSecurity)
            {
                builder.UserID = user;
                builder.Password = password;
            }
            builder.Pooling = true;
            return NewBuilder(
                System.Data.SqlClient.SqlClientFactory.Instance,
                builder.ConnectionString,
                null,
                dialect).Build();
        }

        #endregion
    }
}
