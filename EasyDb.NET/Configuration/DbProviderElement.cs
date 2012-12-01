//
// LX.EasyDb.Configuration.DbProviderElement.cs
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
using System.Configuration;

namespace LX.EasyDb.Configuration
{
    /// <summary>
    /// Configuration of a db provider.
    /// </summary>
    public class DbProviderElement : ConfigurationElement
    {
        /// <summary>
        /// Initializes.
        /// </summary>
        internal DbProviderElement() { }

        /// <summary>
        /// Initializes.
        /// </summary>
        public DbProviderElement(String name, String provider, String connectionString)
        {
            Name = name;
            Provider = provider;
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Gets the name of this provider.
        /// </summary>
        [ConfigurationProperty("name", IsKey = true)]
        public String Name
        {
            get { return (String)base["name"]; }
            private set { base["name"] = value; }
        }

        /// <summary>
        /// Gets the type of <see cref="System.Data.Common.DbProviderFactory"/>
        /// for creating instances of a provider's implementation of the data source classes.
        /// </summary>
        [ConfigurationProperty("provider", IsRequired = true)]
        public String Provider
        {
            get { return (String)base["provider"]; }
            private set { base["provider"] = value; }
        }

        /// <summary>
        /// Gets the SQL dialect to apply.
        /// </summary>
        [ConfigurationProperty("dialect")]
        public String Dialect
        {
            get { return (String)base["dialect"]; }
            private set { base["dialect"] = value; }
        }

        /// <summary>
        /// Gets the string used to open a database.
        /// </summary>
        [ConfigurationProperty("connectionString")]
        public String ConnectionString
        {
            get { return (String)base["connectionString"]; }
            private set { base["connectionString"] = value; }
        }

        /// <summary>
        /// Gets the name of the connection string in &lt;connectionStrings&gt; section.
        /// </summary>
        [ConfigurationProperty("connectionStringName")]
        public String ConnectionStringName
        {
            get { return (String)base["connectionStringName"]; }
        }
    }
}
