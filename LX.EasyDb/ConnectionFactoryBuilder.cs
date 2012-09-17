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
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace LX.EasyDb
{
    /// <summary>
    /// Builder that builds connection factories.
    /// </summary>
    public class ConnectionFactoryBuilder
    {
        private System.Data.Common.DbProviderFactory _factory;
        private String _factoryType;
        private String _connectionString;
        private String _name;

        /// <summary>
        /// Gets a builder.
        /// </summary>
        /// <param name="factory">the <see cref="System.Data.Common.DbProviderFactory"/> that creating instances of a provider's implementation of the data source classes</param>
        /// <param name="connectionString">the connection used to open the database</param>
        /// <param name="name">the name of the provider</param>
        /// <returns><see cref="LX.EasyDb.ConnectionFactoryBuilder"/></returns>
        public static ConnectionFactoryBuilder NewBuilder(System.Data.Common.DbProviderFactory factory, String connectionString, String name)
        {
            ConnectionFactoryBuilder builder = new ConnectionFactoryBuilder();
            builder._factory = factory;
            builder._connectionString = connectionString;
            builder._name = name;
            return builder;
        }

        /// <summary>
        /// Gets a builder.
        /// </summary>
        /// <param name="provider">the provider that implements the data source classes</param>
        /// <param name="connectionString">the connection used to open the database</param>
        /// <param name="name">the name of the provider</param>
        /// <returns><see cref="LX.EasyDb.DbProviderBuilder"/></returns>
        public static ConnectionFactoryBuilder NewBuilder(String provider, String connectionString, String name)
        {
            ConnectionFactoryBuilder builder = new ConnectionFactoryBuilder();
            builder._factoryType = provider;
            builder._connectionString = connectionString;
            builder._name = name;
            return builder;
        }

        /// <summary>
        /// Builds a provider with given properties.
        /// </summary>
        /// <returns><see cref="LX.EasyDb.IDbProvider"/></returns>
        public IConnectionFactory Build()
        {
            return _factory == null ? CreateConnectionFactory(_factoryType, _connectionString, _name) : CreateConnectionFactory(_factory, _connectionString, _name);
        }

        private static IConnectionFactory CreateConnectionFactory(System.Data.Common.DbProviderFactory factory, String connectionString, String name)
        {
            ConnectionFactory cf = new ConnectionFactory(factory);
            cf.ConnectionString = connectionString;
            cf.Name = name;
            return cf;
        }

        private static IConnectionFactory CreateConnectionFactory(String provider, String connectionString, String name)
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
                return CreateConnectionFactory((System.Data.Common.DbProviderFactory)factory, connectionString, name);
        }

        private static Type GetSubTypeInNamespace<T>(Assembly asm, String ns)
        {
            return Array.Find(asm.GetTypes(), delegate(Type t)
            {
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
