//
// LX.EasyDb.Dialects.Function.CastFunction.cs
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
    class CastFunction : ISQLFunction
    {
        public DbType GetReturnType(DbType firstArgumentType)
        {
            throw new NotImplementedException();
        }

        public String Render(IList<Object> args, IConnectionFactory factory)
        {
            if (args.Count != 2)
                throw new Exception("cast() requires two arguments");

            String type = (String)args[1];
            String sqlType = type;
            // TODO dialect the type

            return "cast(" + args[0] + " as " + sqlType + ')';
        }
    }
}
