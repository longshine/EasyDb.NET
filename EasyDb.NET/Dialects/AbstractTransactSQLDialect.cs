//
// LX.EasyDb.Dialects.AbstractTransactSQLDialect.cs
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
    /// An abstract base class for Sybase and MS SQL Server dialects.
    /// </summary>
    public abstract class AbstractTransactSQLDialect : Dialect
    {
        /// <summary>
        /// </summary>
        public AbstractTransactSQLDialect()
        {
            RegisterColumnType(DbType.Identity, "bigint");
            RegisterColumnType(DbType.Boolean, "tinyint"); // Sybase BIT type does not support null values
            RegisterColumnType(DbType.Int64, "bigint");
            RegisterColumnType(DbType.Int16, "smallint");
            RegisterColumnType(DbType.Byte, "tinyint");
            RegisterColumnType(DbType.Int32, "int");
            //RegisterColumnType(Types.CHAR, "char(1)");
            RegisterColumnType(DbType.String, "varchar($l)");
            RegisterColumnType(DbType.Single, "float");
            RegisterColumnType(DbType.Double, "double precision");
            RegisterColumnType(DbType.Decimal, "decimal");
            RegisterColumnType(DbType.Date, "datetime");
            RegisterColumnType(DbType.DateTime, "datetime");
            //RegisterColumnType(Types.TIMESTAMP, "datetime");
            RegisterColumnType(DbType.Binary, "varbinary($l)");
            RegisterColumnType(DbType.VarNumeric, "numeric($p,$s)");
            //RegisterColumnType(Types.BLOB, "image");
            //RegisterColumnType(Types.CLOB, "text");

            RegisterFunction("ascii", new StandardSQLFunction("ascii", DbType.Int32));
            //RegisterFunction("char", new StandardSQLFunction("char", Types.CHAR));
            RegisterFunction("len", new StandardSQLFunction("len", DbType.Int64));
            RegisterFunction("lower", new StandardSQLFunction("lower"));
            RegisterFunction("upper", new StandardSQLFunction("upper"));
            RegisterFunction("str", new StandardSQLFunction("str", DbType.String));
            RegisterFunction("ltrim", new StandardSQLFunction("ltrim"));
            RegisterFunction("rtrim", new StandardSQLFunction("rtrim"));
            RegisterFunction("reverse", new StandardSQLFunction("reverse"));
            RegisterFunction("space", new StandardSQLFunction("space", DbType.String));

            RegisterFunction("user", new NoArgSQLFunction("user", DbType.String));

            RegisterFunction("current_timestamp", new NoArgSQLFunction("getdate", DbType.DateTime));
            RegisterFunction("current_time", new NoArgSQLFunction("getdate", DbType.Time));
            RegisterFunction("current_date", new NoArgSQLFunction("getdate", DbType.Date));

            RegisterFunction("getdate", new NoArgSQLFunction("getdate", DbType.DateTime));
            RegisterFunction("getutcdate", new NoArgSQLFunction("getutcdate", DbType.DateTime));
            RegisterFunction("day", new StandardSQLFunction("day", DbType.Int32));
            RegisterFunction("month", new StandardSQLFunction("month", DbType.Int32));
            RegisterFunction("year", new StandardSQLFunction("year", DbType.Int32));
            RegisterFunction("datename", new StandardSQLFunction("datename", DbType.String));

            RegisterFunction("abs", new StandardSQLFunction("abs"));
            RegisterFunction("sign", new StandardSQLFunction("sign", DbType.Int32));

            RegisterFunction("acos", new StandardSQLFunction("acos", DbType.Double));
            RegisterFunction("asin", new StandardSQLFunction("asin", DbType.Double));
            RegisterFunction("atan", new StandardSQLFunction("atan", DbType.Double));
            RegisterFunction("cos", new StandardSQLFunction("cos", DbType.Double));
            RegisterFunction("cot", new StandardSQLFunction("cot", DbType.Double));
            RegisterFunction("exp", new StandardSQLFunction("exp", DbType.Double));
            RegisterFunction("log", new StandardSQLFunction("log", DbType.Double));
            RegisterFunction("log10", new StandardSQLFunction("log10", DbType.Double));
            RegisterFunction("sin", new StandardSQLFunction("sin", DbType.Double));
            RegisterFunction("sqrt", new StandardSQLFunction("sqrt", DbType.Double));
            RegisterFunction("tan", new StandardSQLFunction("tan", DbType.Double));
            RegisterFunction("pi", new NoArgSQLFunction("pi", DbType.Double));
            RegisterFunction("square", new StandardSQLFunction("square"));
            RegisterFunction("rand", new StandardSQLFunction("rand", DbType.Single));

            RegisterFunction("radians", new StandardSQLFunction("radians", DbType.Double));
            RegisterFunction("degrees", new StandardSQLFunction("degrees", DbType.Double));

            RegisterFunction("round", new StandardSQLFunction("round"));
            RegisterFunction("ceiling", new StandardSQLFunction("ceiling"));
            RegisterFunction("floor", new StandardSQLFunction("floor"));

            RegisterFunction("isnull", new StandardSQLFunction("isnull"));

            RegisterFunction("concat", new VarArgsSQLFunction(DbType.String, "(", "+", ")"));

            RegisterFunction("length", new StandardSQLFunction("len", DbType.Int32));
            RegisterFunction("trim", new SQLFunctionTemplate(DbType.String, "ltrim(rtrim(?1))"));
        }

        /// <inheritdoc/>
        public override String IdentityColumnString
        {
            get
            {
                return "identity not null"; //starts with 1, implicitly
            }
        }

        /// <inheritdoc/>
        public override String SelectIdentityString
        {
            get { return "select @@identity"; }
        }
    }
}
