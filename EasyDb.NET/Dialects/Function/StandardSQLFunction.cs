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

        public StandardSQLFunction(String name)
            : this(name, DbType.Empty)
        {
        }

        public StandardSQLFunction(String name, DbType registeredType)
        {
            Name = name;
            _returnType = registeredType;
        }

        public String Name { get; private set; }

        public DbType GetReturnType(DbType firstArgumentType)
        {
            return _returnType == DbType.Empty ? firstArgumentType : _returnType;
        }

        public String Render(IList<Object> args, IConnectionFactory factory)
        {
            return DoRender(args, factory);
        }

        public override String ToString()
        {
            return Name;
        }

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
