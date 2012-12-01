//
// LX.EasyDb.Dialects.Function.StandardSQLFunction.cs
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

namespace LX.EasyDb.Dialects.Function
{
    /// <summary>
    /// Provides a standard implementation of SQL functions.
    /// </summary>
    public class StandardSQLFunction : ISQLFunction
    {
        private DbType _returnType;

        /// <summary>
        /// Creates a function with a name which has no returned result.
        /// </summary>
        /// <param name="name"></param>
        public StandardSQLFunction(String name)
            : this(name, DbType.Empty)
        {
        }

        /// <summary>
        /// Creates a function with a name and a type of the returned result.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="registeredType"></param>
        public StandardSQLFunction(String name, DbType registeredType)
        {
            Name = name;
            _returnType = registeredType;
        }

        /// <summary>
        /// Gets the name of this function.
        /// </summary>
        public String Name { get; private set; }

        /// <summary>
        /// Gets the return type of the function.
        /// May be either a concrete type, or variable depending upon the type of the first argument.
        /// </summary>
        public DbType GetReturnType(DbType firstArgumentType)
        {
            return _returnType == DbType.Empty ? firstArgumentType : _returnType;
        }

        /// <summary>
        /// Renders the function call as SQL fragment.
        /// </summary>
        public String Render(IList<Object> args, IConnectionFactory factory)
        {
            return DoRender(args, factory);
        }

        /// <summary>
        /// 
        /// </summary>
        public override String ToString()
        {
            return Name;
        }

        /// <summary>
        /// Renders the function call as SQL fragment.
        /// </summary>
        protected virtual String DoRender(IList<Object> args, IConnectionFactory factory)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Name).Append('(');
            if (args.Count > 0)
            {
                sb.Append(args[0]);
                for (Int32 i = 1; i < args.Count; i++)
                    sb.Append(", ").Append(args[i]);
            }
            return sb.Append(')').ToString();
        }
    }
}
