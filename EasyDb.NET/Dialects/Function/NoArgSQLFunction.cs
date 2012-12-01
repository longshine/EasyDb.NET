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

        /// <summary>
        /// Creates a function with a name and a type of the returned result.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="returnType"></param>
        public NoArgSQLFunction(String name, DbType returnType)
            : this(name, returnType, true)
        {
        }

        /// <summary>
        /// Creates a function with a name and a type of the returned result.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="returnType"></param>
        /// <param name="hasParenthesesIfNoArguments">a boolean value indicating whether a pair of parentheses/"()" should be appended after the function</param>
        public NoArgSQLFunction(String name, DbType returnType, Boolean hasParenthesesIfNoArguments)
        {
            _returnType = returnType;
            HasParenthesesIfNoArguments = hasParenthesesIfNoArguments;
            Name = name;
        }

        /// <summary>
        /// Gets the name of this function.
        /// </summary>
        public String Name { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a pair of parentheses/"()" should be appended after the function.
        /// </summary>
        public Boolean HasParenthesesIfNoArguments { get; private set; }

        /// <summary>
        /// Gets the return type of the function.
        /// May be either a concrete type, or variable depending upon the type of the first argument.
        /// </summary>
        public DbType GetReturnType(DbType firstArgumentType)
        {
            return _returnType;
        }

        /// <summary>
        /// Renders the function call as SQL fragment.
        /// </summary>
        public String Render(IList<Object> args, IConnectionFactory factory)
        {
            return HasParenthesesIfNoArguments ? (Name + "()") : Name;
        }
    }
}
