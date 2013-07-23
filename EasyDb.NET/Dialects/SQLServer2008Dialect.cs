//
// LX.EasyDb.Dialects.SQLServer2008Dialect.cs
//
// Authors:
//	Longshine He <longshinehe@users.sourceforge.net>
//
// Copyright (c) 2012 Longshine He
//
// This code is distributed in the hope that it will be useful,
// but WITHOUT WARRANTY OF ANY KIND.
//

using LX.EasyDb.Dialects.Function;

namespace LX.EasyDb.Dialects
{
    /// <summary>
    /// A dialect for Microsoft SQL Server 2008 with JDBC Driver 3.0 and above.
    /// </summary>
    public class SQLServer2008Dialect : SQLServerDialect
    {
        /// <summary>
        /// </summary>
        public SQLServer2008Dialect()
        {
            RegisterColumnType(DbType.Date, "date");
            RegisterColumnType(DbType.Time, "time");
            RegisterColumnType(DbType.DateTime2, "datetime2");

            RegisterFunction("current_timestamp", new NoArgSQLFunction("current_timestamp", DbType.DateTime2, false));
        }
    }
}
