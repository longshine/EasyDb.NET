﻿//
// LX.EasyDb.Criterion.Projections.cs
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

namespace LX.EasyDb.Criterion
{
    /// <summary>
    /// Factory class for creating projection queries.
    /// </summary>
    public static class Projections
    {
        /// <summary>
        /// Creates a new projection list.
        /// </summary>
        public static ProjectionList List()
        {
            return new ProjectionList();
        }

        /// <summary>
        /// The query row count, ie. <code>count(*)</code>
        /// </summary>
        public static IProjection RowCount()
        {
            return new RowCountProjection();
        }

        /// <summary>
        /// A property value count.
        /// </summary>
        public static IProjection Count(String fieldName)
        {
            return new CountProjection(Clauses.Field(fieldName));
        }

        /// <summary>
        /// A distinct property value count.
        /// </summary>
        public static IProjection CountDistinct(String fieldName)
        {
            return new CountProjection(Clauses.Field(fieldName)) { Distinct = true };
        }

        /// <summary>
        /// A property average value.
        /// </summary>
        public static IProjection Avg(String fieldName)
        {
            return new AggregateProjection("avg", Clauses.Field(fieldName));
        }

        /// <summary>
        /// A property maximum value.
        /// </summary>
        public static IProjection Max(String fieldName)
        {
            return new AggregateProjection("max", Clauses.Field(fieldName));
        }

        /// <summary>
        /// A property minimum value.
        /// </summary>
        public static IProjection Min(String fieldName)
        {
            return new AggregateProjection("min", Clauses.Field(fieldName));
        }

        /// <summary>
        /// A property value sum.
        /// </summary>
        public static IProjection Sum(String fieldName)
        {
            return new AggregateProjection("sum", Clauses.Field(fieldName));
        }

        /// <summary>
        /// A grouping property value.
        /// </summary>
        public static IProjection GroupProperty(String propertyName)
        {
            return new PropertyProjection(propertyName, true);
        }

        /// <summary>
        /// A projected property value.
        /// </summary>
        public static IProjection Property(String propertyName)
        {
            return new PropertyProjection(propertyName);
        }

        /// <summary>
        /// A projected expression.
        /// </summary>
        public static IProjection Expression(IExpression expression)
        {
            return new ExpressionProjection(expression);
        }
    }
}
