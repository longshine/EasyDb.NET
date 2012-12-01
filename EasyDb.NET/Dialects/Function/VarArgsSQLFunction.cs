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
    class VarArgsSQLFunction : ISQLFunction
    {
        private String begin;
        private String sep;
        private String end;
        private DbType registeredType;

        public VarArgsSQLFunction(DbType registeredType, String begin, String sep, String end)
        {
            this.registeredType = registeredType;
            this.begin = begin;
            this.sep = sep;
            this.end = end;
        }

        public DbType GetReturnType(DbType firstArgumentType)
        {
            return registeredType == DbType.Empty ? firstArgumentType : registeredType;
        }

        public string Render(IList<Object> args, IConnectionFactory factory)
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

        protected String TransformArgument(String argument)
        {
            return argument;
        }
    }
}
