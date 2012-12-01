//
// LX.EasyDb.Dialects.Function.NoArgSQLFunction.cs
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

namespace LX.EasyDb.Dialects.Function
{
    /// <summary>
    /// A function which takes no arguments.
    /// </summary>
    public class NoArgSQLFunction : ISQLFunction
    {
        private DbType _returnType;

        public NoArgSQLFunction(String name, DbType returnType)
            : this(name, returnType, true)
        {
        }

        public NoArgSQLFunction(String name, DbType returnType, Boolean hasParenthesesIfNoArguments)
        {
            _returnType = returnType;
            HasParenthesesIfNoArguments = hasParenthesesIfNoArguments;
            Name = name;
        }

        public DbType GetReturnType(DbType firstArgumentType)
        {
            return _returnType;
        }

        public String Render(IList<Object> args, IConnectionFactory factory)
        {
            return HasParenthesesIfNoArguments ? (Name + "()") : Name;
        }

        public String Name { get; private set; }

        public Boolean HasParenthesesIfNoArguments { get; private set; }
    }
}
