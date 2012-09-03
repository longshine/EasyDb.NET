//
// LX.EasyDb.DbProviderBuilder.cs
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
using System.Reflection;

namespace LX.EasyDb
{
    /// <summary>
    /// Builder that builds providers.
    /// </summary>
    public class DbProviderBuilder
    {
        private System.Data.Common.DbProviderFactory _factory;
        private String _factoryType;
        private String _connectionString;
        private String _name;
        private List<Object[]> _slaves = new List<Object[]>();

        private DbProviderBuilder() { }

        /// <summary>
        /// Gets a builder.
        /// </summary>
        /// <param name="factory">the <see cref="System.Data.Common.DbProviderFactory"/> that creating instances of a provider's implementation of the data source classes</param>
        /// <param name="connectionString">the connection used to open the database</param>
        /// <param name="name">the name of the provider</param>
        /// <returns><see cref="LX.EasyDb.DbProviderBuilder"/></returns>
        public static DbProviderBuilder NewBuilder(System.Data.Common.DbProviderFactory factory, String connectionString, String name)
        {
            DbProviderBuilder builder = new DbProviderBuilder();
            builder._factory = factory;
            builder._connectionString = connectionString;
            builder._name = name;
            return builder;
        }

        /// <summary>
        /// Gets a builder.
        /// </summary>
        /// <param name="factory">the <see cref="System.Data.Common.DbProviderFactory"/> that creating instances of a provider's implementation of the data source classes</param>
        /// <returns><see cref="LX.EasyDb.DbProviderBuilder"/></returns>
        public static DbProviderBuilder NewBuilder(System.Data.Common.DbProviderFactory factory)
        {
            return NewBuilder(factory, null, null);
        }

        /// <summary>
        /// Gets a builder.
        /// </summary>
        /// <param name="factory">the <see cref="System.Data.Common.DbProviderFactory"/> that creating instances of a provider's implementation of the data source classes</param>
        /// <param name="connectionString">the connection used to open the database</param>
        /// <returns><see cref="LX.EasyDb.DbProviderBuilder"/></returns>
        public static DbProviderBuilder NewBuilder(System.Data.Common.DbProviderFactory factory, String connectionString)
        {
            return NewBuilder(factory, connectionString, null);
        }

        /// <summary>
        /// Gets a builder.
        /// </summary>
        /// <param name="provider">the provider that implements the data source classes</param>
        /// <param name="connectionString">the connection used to open the database</param>
        /// <param name="name">the name of the provider</param>
        /// <returns><see cref="LX.EasyDb.DbProviderBuilder"/></returns>
        public static DbProviderBuilder NewBuilder(String provider, String connectionString, String name)
        {
            DbProviderBuilder builder = new DbProviderBuilder();
            builder._factoryType = provider;
            builder._connectionString = connectionString;
            builder._name = name;
            return builder;
        }

        /// <summary>
        /// Gets a builder.
        /// </summary>
        /// <param name="provider">the provider that implements the data source classes</param>
        /// <returns><see cref="LX.EasyDb.DbProviderBuilder"/></returns>
        public static DbProviderBuilder NewBuilder(String provider)
        {
            return NewBuilder(provider, null, null);
        }

        /// <summary>
        /// Gets a builder.
        /// </summary>
        /// <param name="provider">the provider that implements the data source classes</param>
        /// <param name="connectionString">the connection used to open the database</param>
        /// <returns><see cref="LX.EasyDb.DbProviderBuilder"/></returns>
        public static DbProviderBuilder NewBuilder(String provider, String connectionString)
        {
            return NewBuilder(provider, connectionString, null);
        }

        /// <summary>
        /// Adds a slave provider.
        /// </summary>
        /// <param name="factory">the <see cref="System.Data.Common.DbProviderFactory"/> that creating instances of a provider's implementation of the data source classes</param>
        /// <param name="connectionString">the connection used to open the database</param>
        /// <param name="name">the name of the provider</param>
        /// <returns><see cref="LX.EasyDb.DbProviderBuilder"/></returns>
        public DbProviderBuilder AddSlave(System.Data.Common.DbProviderFactory factory, String connectionString, String name)
        {
            _slaves.Add(new Object[] { factory, connectionString, name });
            return this;
        }

        /// <summary>
        /// Adds a slave provider.
        /// </summary>
        /// <param name="provider">the provider that implements the data source classes</param>
        /// <param name="connectionString">the connection used to open the database</param>
        /// <param name="name">the name of the provider</param>
        /// <returns><see cref="LX.EasyDb.DbProviderBuilder"/></returns>
        public DbProviderBuilder AddSlave(String provider, String connectionString, String name)
        {
            _slaves.Add(new Object[] { provider, connectionString, name });
            return this;
        }

        /// <summary>
        /// Builds a provider with given properties.
        /// </summary>
        /// <returns><see cref="LX.EasyDb.IDbProvider"/></returns>
        public IDbProvider Build()
        {
            return _factory == null ? CreateDbProvider(_factoryType, _connectionString, _name) : CreateDbProvider(_factory, _connectionString, _name);
        }

        /// <summary>
        /// Builds a observable provider.
        /// </summary>
        /// <returns><see cref="LX.EasyDb.ObservableDbProvider"/></returns>
        public ObservableDbProvider BuildObservable()
        {
            return new ObservableDbProvider(Build());
        }

        /// <summary>
        /// Builds a master/slave provider.
        /// </summary>
        /// <returns><see cref="LX.EasyDb.MasterSlaveDbProvider"/></returns>
        public MasterSlaveDbProvider BuildMasterSlave()
        {
            return new MasterSlaveDbProvider(Build(), BuildSlaves());
        }

        private IDbProvider[] BuildSlaves()
        { 
            List<IDbProvider> slaves = new List<IDbProvider>();
            foreach (Object[] objs in _slaves)
            {
                slaves.Add(Build(objs[0] as System.Data.Common.DbProviderFactory, objs[0] as String, (String)objs[1], (String)objs[2]));
            }
            return slaves.ToArray();
        }

        private static IDbProvider Build(System.Data.Common.DbProviderFactory factory, String type, String connectionString, String name)
        {
            return factory == null ? CreateDbProvider(type, connectionString, name) : CreateDbProvider(factory, connectionString, name);
        }

        /// <summary>
        /// Create a instance of <see cref="LX.EasyDb.IDbProvider"/> specified by the type.
        /// </summary>
        /// <param name="factory">the <see cref="System.Data.Common.DbProviderFactory"/> that creating instances of a provider's implementation of the data source classes</param>
        /// <param name="connectionString">the connection used to open the database</param>
        /// <param name="name">the name of the provider</param>
        /// <returns><see cref="LX.EasyDb.IDbProvider"/></returns>
        private static IDbProvider CreateDbProvider(System.Data.Common.DbProviderFactory factory, String connectionString, String name)
        {
            DbProvider provider = new DbProvider(factory);
            provider.ConnectionString = connectionString;
            provider.Name = name;
            return provider;
        }

        /// <summary>
        /// Create a instance of <see cref="LX.EasyDb.IDbProvider"/> specified by the type.
        /// </summary>
        /// <param name="provider">the provider that implements the data source classes</param>
        /// <param name="connectionString">the connection used to open the database</param>
        /// <param name="name">the name of the provider</param>
        /// <returns>a instance of<see cref="LX.EasyDb.IDbProvider"/>, or null if failed</returns>
        private static IDbProvider CreateDbProvider(String provider, String connectionString, String name)
        {
            Object factory = null;
            String nsName, assemblyName;
            GetTypeNames(provider, out nsName, out assemblyName);
            Assembly asm = LoadAssembly(assemblyName);
            Type factoryType = GetSubTypeInNamespace<System.Data.Common.DbProviderFactory>(asm, nsName);
            if (factoryType != null)
            {
                FieldInfo instanceField = factoryType.GetField("Instance");
                if (instanceField != null && instanceField.IsStatic)
                    factory = instanceField.GetValue(null);
                else
                    factory = factoryType.GetConstructor(Type.EmptyTypes).Invoke(null);
            }

            if (factory == null)
                return null;
            else
                return CreateDbProvider((System.Data.Common.DbProviderFactory)factory, connectionString, name);
        }

        private static Type GetSubTypeInNamespace<T>(Assembly asm, String ns)
        {
            return Array.Find(asm.GetTypes(), delegate(Type t) {
                return String.Equals(t.Namespace, ns) && t.IsSubclassOf(typeof(T));
            });
        }

        private static void GetTypeNames(String typeFullName, out String className, out String assemblyName)
        {
            String[] tmp = typeFullName.Split(new Char[] { ',' }, 2);
            className = tmp[0];
            assemblyName = (tmp.Length > 1) ? tmp[1].Trim() : String.Empty;
        }

        private static Assembly LoadAssembly(String assemblyName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            if (!String.IsNullOrEmpty(assemblyName) && !assemblyName.Equals(asm.FullName.Split(',')[0]))
                asm = Assembly.Load(assemblyName);
            return asm;
        }
    }
}
