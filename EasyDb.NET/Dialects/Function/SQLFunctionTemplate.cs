//
// LX.EasyDb.Dialects.Function.SQLFunctionTemplate.cs
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
    /// Represents functions that can have different representations in different SQL dialects.
    /// E.g. in HQL we can define function concat(?1, ?2) to concatenate two strings p1 and p2.
    /// Target SQL function will be dialect-specific, e.g. (?1 || ?2) for Oracle, concat(?1, ?2)
    /// for MySql, (?1 + ?2) for MS SQL. Each dialect will define a template as a string (exactly
    /// like above) marking function parameters with '?' followed by parameter's index (first index is 1).
    /// </summary>
    class SQLFunctionTemplate : ISQLFunction
    {
        private DbType _type;
        private TemplateRenderer _renderer;

        public SQLFunctionTemplate(DbType type, String template)
        {
            _type = type;
            _renderer = new TemplateRenderer(template);
        }

        public DbType GetReturnType(DbType firstArgumentType)
        {
            return _type;
        }

        public String Render(IList<Object> args, IConnectionFactory factory)
        {
            return _renderer.Render(args, factory);
        }
    }
}
