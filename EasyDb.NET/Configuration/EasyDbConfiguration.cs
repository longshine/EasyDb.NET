//
// LX.EasyDb.Configuration.EasyDbConfiguration.cs
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
    /// EasyDb configuration.
    /// </summary>
    public class EasyDbConfiguration : System.Configuration.ConfigurationSection
    {
        internal const String SECTION = "EasyDb";
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propMasterProviderName;
        private static readonly ConfigurationProperty _propProviders;
        private static readonly ConfigurationProperty _propSlaveEnabled;

        static EasyDbConfiguration()
        {
            _propMasterProviderName = new ConfigurationProperty("masterProviderName", typeof(String), "");
            _propProviders = new ConfigurationProperty("", typeof(DbProviderCollection), null, null, null, ConfigurationPropertyOptions.IsDefaultCollection);
            _propSlaveEnabled = new ConfigurationProperty("slaveEnabled", typeof(Boolean), false);

            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propMasterProviderName);
            _properties.Add(_propProviders);
            _properties.Add(_propSlaveEnabled);
        }

        /// <summary>
        /// Gets or sets the name of the primary provider.
        /// If this property is not set, the first provider in Providers will be taken as primary.
        /// </summary>
        public String MasterProviderName
        {
            get { return (String)base[_propMasterProviderName]; }
            set { base[_propMasterProviderName] = value; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether to enable master/slave mode or not.
        /// </summary>
        public Boolean SlaveEnabled
        {
            get { return (Boolean)base[_propSlaveEnabled]; }
            set { base[_propSlaveEnabled] = value; }
        }

        /// <summary>
        /// Gets the primary provider.
        /// </summary>
        public DbProviderElement MasterProvider
        {
            get { return String.IsNullOrEmpty(MasterProviderName) ? (Providers.Count > 0 ? Providers[0] : null) : Providers[MasterProviderName]; }
        }

        /// <summary>
        /// Gets the collection of providers.
        /// </summary>
        public DbProviderCollection Providers
        {
            get { return (DbProviderCollection)base[_propProviders]; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override Boolean IsModified()
        {
            return Providers.IsModified();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void Reset(ConfigurationElement parentSection)
        {
            EasyDbConfiguration psec = parentSection as EasyDbConfiguration;
            if (psec != null)
                Providers.Reset(psec.Providers);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }
    }
}
