//
// LX.EasyDb.ICriteria.cs
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
using System.Collections;
using System.Collections.Generic;
using LX.EasyDb.Criterion;

namespace LX.EasyDb
{
    /// <summary>
    /// Provides a simplified, object-oriented API for retrieving entities
    /// by composing objects of query fragments.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICriteria<T>
    {
        /// <summary>
        /// Adds a <see cref="IExpression"/> to constrain the results to be retrieved.
        /// </summary>
        /// <param name="condition">the <see cref="IExpression"/> object representing the restriction to be applied</param>
        /// <returns>this (for method chaining)</returns>
        ICriteria<T> Add(IExpression condition);
        /// <summary>
        /// Adds an <see cref="Order"/> to the result set.
        /// </summary>
        /// <param name="order">the <see cref="Order"/> object representing an ordering to be applied to the results</param>
        /// <returns>this (for method chaining)</returns>
        ICriteria<T> AddOrder(Order order);
        /// <summary>
        /// Used to specify that the query results will be a projection.
        /// </summary>
        /// <param name="projection">the projection representing the overall "shape" of the query results</param>
        /// <returns>this (for method chaining)</returns>
        ICriteria<T> SetProjection(IProjection projection);
        /// <summary>
        /// Gets the results.
        /// </summary>
        /// <returns>matched query results</returns>
        IEnumerable<T> List();
        /// <summary>
        /// Gets paged results.
        /// </summary>
        /// <param name="limit">the limit upon the number of objects to be retrieved</param>
        /// <param name="offset">the offset where the first result starts</param>
        /// <returns>matched query results</returns>
        IEnumerable<T> List(Int32 limit, Int32 offset);
        /// <summary>
        /// Gets the total count of the results.
        /// </summary>
        /// <returns>the total count</returns>
        Int32 Count();
        /// <summary>
        /// Returns a single instance that matches the query, or null if the query returns no results.
        /// </summary>
        /// <returns>the single result or <code>null</code></returns>
        /// <exception cref="InvalidOperationException">if there is more than one matching result</exception>
        T SingleOrDefault();
    }

    /// <summary>
    /// Provides a simplified, object-oriented API for retrieving entities
    /// by composing objects of query fragments.
    /// </summary>
    public interface ICriteria
    {
        /// <summary>
        /// Gets or sets a value indicating whether use parameterized query or not.
        /// </summary>
        Boolean Parameterized { get; set; }
        /// <summary>
        /// Adds a <see cref="IExpression"/> to constrain the results to be retrieved.
        /// </summary>
        /// <param name="condition">the <see cref="IExpression"/> object representing the restriction to be applied</param>
        /// <returns>this (for method chaining)</returns>
        ICriteria Add(IExpression condition);
        /// <summary>
        /// Adds an <see cref="Order"/> to the result set.
        /// </summary>
        /// <param name="order">the <see cref="Order"/> object representing an ordering to be applied to the results</param>
        /// <returns>this (for method chaining)</returns>
        ICriteria AddOrder(Order order);
        /// <summary>
        /// Used to specify that the query results will be a projection.
        /// </summary>
        /// <param name="projection">the projection representing the overall "shape" of the query results</param>
        /// <returns>this (for method chaining)</returns>
        ICriteria SetProjection(IProjection projection);
        /// <summary>
        /// Gets the results.
        /// </summary>
        /// <returns>matched query results</returns>
        IEnumerable List();
        /// <summary>
        /// Gets paged results.
        /// </summary>
        /// <param name="limit">the limit upon the number of objects to be retrieved</param>
        /// <param name="offset">the offset where the first result starts</param>
        /// <returns>matched query results</returns>
        IEnumerable List(Int32 limit, Int32 offset);
        /// <summary>
        /// Gets the total count of the results.
        /// </summary>
        /// <returns>the total count</returns>
        Int32 Count();
        /// <summary>
        /// Returns a single instance that matches the query, or null if the query returns no results.
        /// </summary>
        /// <returns>the single result or <code>null</code></returns>
        /// <exception cref="InvalidOperationException">if there is more than one matching result</exception>
        Object SingleOrDefault();
    }
}
