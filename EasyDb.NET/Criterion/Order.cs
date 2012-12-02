//
// LX.EasyDb.Criterion.Order.cs
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

namespace LX.EasyDb.Criterion
{
    /// <summary>
    /// Represents an order.
    /// </summary>
    public class Order : IFragment
    {
        /// <summary>
        /// Gets the order expression.
        /// </summary>
        public IExpression Expression { get; private set; }
        /// <summary>
        /// Checks if this is ascending or not.
        /// </summary>
        public Boolean Ascending { get; private set; }

        /// <summary>
        /// </summary>
        public Order(IExpression expression, Boolean ascending)
        {
            Expression = expression;
            Ascending = ascending;
        }

        /// <summary>
        /// Renders this fragment.
        /// </summary>
        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        /// <summary>
        /// </summary>
        public override String ToString()
        {
            return Expression + (Ascending ? " asc" : " desc");
        }

        /// <summary>
        /// Ascending order.
        /// </summary>
        public static Order Asc(String fieldName)
        {
            return new Order(Clauses.Field(fieldName), true);
        }

        /// <summary>
        /// Ascending order.
        /// </summary>
        public static Order Asc(IExpression expression)
        {
            return new Order(expression, true);
        }

        /// <summary>
        /// Descending order.
        /// </summary>
        public static Order Desc(String fieldName)
        {
            return new Order(Clauses.Field(fieldName), false);
        }

        /// <summary>
        /// Descending order.
        /// </summary>
        public static Order Desc(IExpression expression)
        {
            return new Order(expression, false);
        }
    }
}
