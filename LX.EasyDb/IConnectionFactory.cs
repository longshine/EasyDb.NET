//
// LX.EasyDb.IConnectionFactory.cs
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

namespace LX.EasyDb
{
    /// <summary>
    /// Represents a factory that builds <see cref="LX.EasyDb.IConnection"/>.
    /// </summary>
    public interface IConnectionFactory
    {
        /// <summary>
        /// Gets or sets the name of this factory.
        /// </summary>
        String Name { get; set; }
        /// <summary>
        /// Gets an opened connection.
        /// </summary>
        /// <returns>an instance of <see cref="LX.EasyDb.IConnection"/></returns>
        IConnection OpenConnection();
        /// <summary>
        /// Gets or sets the SQL dialect used in this factory.
        /// </summary>
        Dialect Dialect { get; set; }
        /// <summary>
        /// Gets or sets the string used to open a database.
        /// </summary>
        String ConnectionString { get; set; }
    }

    /// <summary>
    /// Provides supports for internal use.
    /// </summary>
    interface IConnectionFactorySupport : Dapper.SqlMapper.ITypeMapRegistry
    {
        Mapping Mapping { get; }
        Dialect Dialect { get; }
        System.Data.Common.DbProviderFactory DbProviderFactory { get; }
    }

    /// <summary>
    /// Represents a factory that builds <see cref="LX.EasyDb.IConnection"/>.
    /// </summary>
    public class ConnectionFactory : IConnectionFactory, IConnectionFactorySupport
    {
        private Mapping _mapping = new Mapping();
        private System.Data.Common.DbProviderFactory _factory;

        /// <summary>
        /// Initializes a factory with an instance of <see cref="System.Data.Common.DbProviderFactory"/>.
        /// </summary>
        /// <param name="factory"></param>
        public ConnectionFactory(System.Data.Common.DbProviderFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Gets or sets the name of this factory.
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// Gets the ORM in this factory.
        /// </summary>
        Mapping IConnectionFactorySupport.Mapping
        {
            get { return _mapping; }
        }

        /// <summary>
        /// Gets or sets the SQL dialect used in this factory.
        /// </summary>
        public Dialect Dialect { get; set; }

        /// <summary>
        /// Gets or sets the string used to open a database.
        /// </summary>
        public String ConnectionString { get; set; }

        /// <summary>
        /// Gets the <see cref="System.Data.Common.DbProviderFactory"/> that creating instances of a provider's implementation of the data source classes.
        /// </summary>
        System.Data.Common.DbProviderFactory IConnectionFactorySupport.DbProviderFactory
        {
            get { return _factory; }
        }

        /// <summary>
        /// Gets an opened connection.
        /// </summary>
        /// <returns>an instance of <see cref="LX.EasyDb.IConnection"/></returns>
        public IConnection OpenConnection()
        {
            DbConnectionWrapper conn = new DbConnectionWrapper(_factory.CreateConnection());
            conn.Factory = this;
            conn.ConnectionString = ConnectionString;
            conn.Open();
            return conn;
        }

        /// <summary>
        /// Gets type-map for the given type, or creates one if not found.
        /// </summary>
        /// <returns>the type map implementation</returns>
        public Dapper.SqlMapper.ITypeMap GetTypeMap(Type type)
        {
            return _mapping.FindTable(type);
        }

        /// <summary>
        /// Set custom mapping for type deserializers.
        /// </summary>
        /// <param name="type">the <see cref="System.Type"/> to map</param>
        /// <param name="map">the mapping rules impementation, or null to remove custom map</param>
        public void SetTypeMap(Type type, Dapper.SqlMapper.ITypeMap map)
        {
            _mapping.SetTable(type, map as Mapping.Table);
        }
    }
}
