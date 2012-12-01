//
// LX.EasyDb.Dialects.Function.VarArgsSQLFunction.cs
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
    /// Support for slightly more general templating than <see cref="StandardSQLFunction"/>, with an unlimited number of arguments.
    /// </summary>
    public class VarArgsSQLFunction : ISQLFunction
    {
        private String begin;
        private String sep;
        private String end;
        private DbType registeredType;

        /// <summary>
        /// Constructs a VarArgsSQLFunction instance with a 'static' return type.  An example of a 'static'
        /// return type would be something like an <code>UPPER</code> function which is always returning
        /// a SQL VARCHAR and thus a string type.
        /// </summary>
        /// <param name="registeredType">the return type</param>
        /// <param name="begin">the beginning of the function templating</param>
        /// <param name="sep">the separator for each individual function argument</param>
        /// <param name="end">the end of the function templating</param>
        public VarArgsSQLFunction(DbType registeredType, String begin, String sep, String end)
        {
            this.registeredType = registeredType;
            this.begin = begin;
            this.sep = sep;
            this.end = end;
        }

        /// <summary>
        /// Gets the return type of the function.
        /// May be either a concrete type, or variable depending upon the type of the first argument.
        /// </summary>
        public DbType GetReturnType(DbType firstArgumentType)
        {
            return registeredType == DbType.Empty ? firstArgumentType : registeredType;
        }

        /// <summary>
        /// Renders the function call as SQL fragment.
        /// </summary>
        public String Render(IList<Object> args, IConnectionFactory factory)
        {
            StringBuilder buf = new StringBuilder().Append(begin);
            for (int i = 0; i < args.Count; i++)
            {
                buf.Append(TransformArgument((String)args[i]));
                if (i < args.Count - 1)
                {
                    buf.Append(sep);
                }
            }
            return buf.Append(end).ToString();
        }

        /// <summary>
        /// Called from Render to allow applying a change or transformation to each individual argument.
        /// </summary>
        /// <param name="argument">the argument being processed</param>
        /// <returns>the transformed argument; may be the same, though should never be null.</returns>
        protected virtual String TransformArgument(String argument)
        {
            return argument;
        }
    }
}
