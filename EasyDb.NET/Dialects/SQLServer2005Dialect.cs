//
// LX.EasyDb.Dialects.SQLServer2005Dialect.cs
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

namespace LX.EasyDb.Dialects
{
    /// <summary>
    /// Dialect for Microsoft SQL Server 2005.
    /// </summary>
    public class SQLServer2005Dialect : SQLServerDialect
    {
        /// <inheritdoc/>
        public override String GetPaging(String sql, String order, Int32 limit, Int32 offset)
        {
            if (offset > 0)
            {
                if (String.IsNullOrEmpty(order))
                    throw new ArgumentException("An order should be specified for paging query.", "order");
                Int32 fromIndex = GetBeforeFromInsertPoint(sql);
                sql = new StringBuilder(sql.Length + order.Length + 32)
                    .Append(sql)
                    .Insert(fromIndex, ",ROW_NUMBER() OVER (" + order + ") AS RowNum")
                    .ToString();
                return new StringBuilder(sql.Substring(0, fromIndex))
                    .Append(" FROM (")
                    .Append(sql)
                    .Append(") t1 WHERE t1.RowNum BETWEEN ")
                    .Append(offset + 1)
                    .Append(" AND ")
                    .Append(limit + offset)
                    .ToString();
            }
            else
            {
                return base.GetPaging(sql, order, limit, offset);
            }
        }

        static Int32 GetBeforeFromInsertPoint(String sql)
        {
            Int32 fromIndex = sql.IndexOf(" from ", StringComparison.OrdinalIgnoreCase);
            return fromIndex;
        }
    }
}
