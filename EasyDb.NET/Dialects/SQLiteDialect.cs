//
// LX.EasyDb.Dialects.SQLiteDialect.cs
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
using LX.EasyDb.Dialects.Function;

namespace LX.EasyDb.Dialects
{
    /// <summary>
    /// Dialect for SQLite.
    /// </summary>
    public class SQLiteDialect : Dialect
    {
        public SQLiteDialect()
        {
            RegisterColumnType(DbType.Identity, "integer");
            RegisterColumnType(DbType.Boolean, "integer");
            //RegisterColumnType(DbType.TINYINT, "tinyint");
            RegisterColumnType(DbType.Int16, "smallint");
            RegisterColumnType(DbType.Int32, "integer");
            RegisterColumnType(DbType.Int64, "bigint");
            RegisterColumnType(DbType.UInt64, "bigint");
            RegisterColumnType(DbType.Single, "float");
            //RegisterColumnType(DbType.REAL, "real");
            RegisterColumnType(DbType.Double, "double");
            RegisterColumnType(DbType.VarNumeric, "numeric");
            RegisterColumnType(DbType.Decimal, "decimal");
            RegisterColumnType(DbType.String, "text");
            //RegisterColumnType(DbType.CHAR, "char");
            //RegisterColumnType(DbType.VARCHAR, "varchar");
            //RegisterColumnType(DbType.LONGVARCHAR, "longvarchar");
            RegisterColumnType(DbType.Date, "date");
            RegisterColumnType(DbType.Time, "time");
            RegisterColumnType(DbType.DateTime, "timestamp");
            //RegisterColumnType(DbType.BINARY, "blob");
            //RegisterColumnType(DbType.VARBINARY, "blob");
            //RegisterColumnType(DbType.LONGVARBINARY, "blob");
            // RegisterColumnType(Types.NULL, "null");
            //RegisterColumnType(DbType.BLOB, "blob");
            //RegisterColumnType(DbType.CLOB, "clob");

            RegisterFunction("concat", new VarArgsSQLFunction(DbType.String, "", "||", ""));
            RegisterFunction("mod", new SQLFunctionTemplate(DbType.Int32, "?1 % ?2"));
            RegisterFunction("substr", new StandardSQLFunction("substr", DbType.String));
            RegisterFunction("substring", new StandardSQLFunction("substr", DbType.String));
        }

        public override Boolean SupportsIfExistsBeforeTableName
        {
            get { return true; }
        }
    }
}
