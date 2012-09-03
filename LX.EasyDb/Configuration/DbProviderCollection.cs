//
// LX.EasyDb.Configuration.DbProviderCollection.cs
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
using System.Configuration;

namespace LX.EasyDb.Configuration
{
    /// <summary>
    /// Collection of db providers.
    /// </summary>
    public class DbProviderCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Adds a element.
        /// </summary>
        /// <param name="element">the <see cref="LX.EasyDb.Configuration.DbProviderElement"/> to add</param>
        public void Add(DbProviderElement element)
        {
            BaseAdd(element);
        }
        
        /// <summary>
        /// Adds a element.
        /// </summary>
        /// <param name="name">the name property of the element</param>
        /// <param name="provider">the provider property of the element</param>
        /// <param name="connectionString">the connectionString property of the element</param>
        public void Add(String name, String provider, String connectionString)
        {
            Add(new DbProviderElement(name, provider, connectionString));
        }

        /// <summary>
        /// Creates a new element.
        /// </summary>
        protected override ConfigurationElement CreateNewElement()
        {
            return new DbProviderElement();
        }

        /// <summary>
        /// Gets the key of the given element.
        /// </summary>
        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((DbProviderElement)element).Name;
        }

        /// <summary>
        /// Gets an element by its name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public new DbProviderElement this[String name]
        {
            get { return (DbProviderElement)base.BaseGet(name); }
        }

        /// <summary>
        /// Gets an element by its index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public DbProviderElement this[Int32 index]
        {
            get { return (DbProviderElement)base.BaseGet(index); }
        }

        internal new void Reset(ConfigurationElement parentElement)
        {
            base.Reset(parentElement);
        }

        internal new Boolean IsModified()
        {
            return base.IsModified();
        }
    }
}
