//
// LX.EasyDb.Dialects.SQLServerDialect.cs
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
using System.Text;
using LX.EasyDb.Dialects.Function;

namespace LX.EasyDb.Dialects
{
    /// <summary>
    /// Dialect for Microsoft SQL Server 2000 and 2005.
    /// </summary>
    public class SQLServerDialect : AbstractTransactSQLDialect
    {
        /// <summary>
        /// </summary>
        public SQLServerDialect()
        {
            RegisterColumnType(DbType.Binary, "image");
            RegisterColumnType(DbType.Binary, 8000, "varbinary($l)");
            //RegisterColumnType(Types.LONGVARBINARY, "image");
            //RegisterColumnType(Types.LONGVARCHAR, "text");

            RegisterFunction("second", new SQLFunctionTemplate(DbType.Int32, "datepart(second, ?1)"));
            RegisterFunction("minute", new SQLFunctionTemplate(DbType.Int32, "datepart(minute, ?1)"));
            RegisterFunction("hour", new SQLFunctionTemplate(DbType.Int32, "datepart(hour, ?1)"));
            RegisterFunction("locate", new StandardSQLFunction("charindex", DbType.Int32));

            RegisterFunction("extract", new SQLFunctionTemplate(DbType.Int32, "datepart(?1, ?3)"));
            RegisterFunction("mod", new SQLFunctionTemplate(DbType.Int32, "?1 % ?2"));
            RegisterFunction("bit_length", new SQLFunctionTemplate(DbType.Int32, "datalength(?1) * 8"));

            RegisterFunction("trim", new AnsiTrimEmulationFunction());
        }

        public override Char OpenQuote { get { return '['; } }

        public override Char CloseQuote { get { return ']'; } }

        public override String GetPaging(String sql, String order, Int32 limit, Int32 offset)
        {
            if (offset > 0)
            {
                if (String.IsNullOrEmpty(order))
                    throw new ArgumentException("An order should be specified for paging query.", "order");
                sql = new StringBuilder(sql.Length + order.Length + 9)
                    .Append(sql)
                    .Append(" ")
                    .Append(order)
                    .Insert(GetAfterSelectInsertPoint(sql), " TOP " + (limit + offset))
                    .ToString();
                String anotherOrderby = order.ToUpperInvariant();
                if (anotherOrderby.Contains(" DESC"))
                    anotherOrderby = anotherOrderby.Replace(" DESC", " ASC");
                else if (anotherOrderby.Contains(" ASC"))
                    anotherOrderby = anotherOrderby.Replace(" ASC", " DESC");
                else
                    anotherOrderby += " DESC";
                // NOTE This may not work properly when the total count of records < (limit + offset)
                return new StringBuilder("SELECT * FROM (SELECT top ")
                    .Append(limit)
                    .Append(" * FROM (")
                    .Append(sql)
                    .Append(") t1 ")
                    .Append(anotherOrderby)
                    .Append(") t2 ")
                    .Append(order)
                    .ToString();
            }
            else
            {
                return new StringBuilder(sql.Length + (order == null ? 0 : order.Length) + 9)
                    .Append(sql)
                    .Append(" ")
                    .Append(order)
                    .Insert(GetAfterSelectInsertPoint(sql), " TOP " + limit)
                    .ToString();
            }
        }

        static Int32 GetAfterSelectInsertPoint(String sql)
        {
            Int32 selectIndex = sql.IndexOf("select", StringComparison.OrdinalIgnoreCase);
            Int32 selectDistinctIndex = sql.IndexOf("select distinct", StringComparison.OrdinalIgnoreCase);
            return selectIndex + (selectDistinctIndex == selectIndex ? 15 : 6);
        }
    }
}
