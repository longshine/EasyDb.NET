//
// LX.EasyDb.DialectFactory.cs
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
    /// Provides methods to create instance of dialects.
    /// </summary>
    public class DialectFactory
    {
        /// <summary>
        /// Creates a dialect.
        /// </summary>
        /// <param name="dialectType">the type of the dialect</param>
        /// <returns>a instance of <see cref="LX.EasyDb.Dialect"/></returns>
        public static Dialect CreateDialect(String dialectType)
        {
            return String.IsNullOrEmpty(dialectType) ? new Dialect() : ReflectHelper.CreateInstance<Dialect>(dialectType);
        }
    }
}
