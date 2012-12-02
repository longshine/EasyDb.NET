//
// LX.EasyDb.Criterion.Junction.cs
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

namespace LX.EasyDb.Criterion
{
    /// <summary>
    /// Represents a sequence of a logical expressions combined by some associative logical operator.
    /// </summary>
    public class Junction : IExpression
    {
        /// <summary>
        /// </summary>
        public Junction(String op)
        {
            Op = op;
            Expressions = new List<IExpression>();
        }

        /// <summary>
        /// Gets inner expressions.
        /// </summary>
        public IList<IExpression> Expressions { get; private set; }

        /// <summary>
        /// Adds an expression.
        /// </summary>
        public Junction Add(IExpression exp)
        {
            Expressions.Add(exp);
            return this;
        }

        /// <summary>
        /// Gets the operator of this sequence.
        /// </summary>
        public String Op { get; private set; }

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
            return '(' + StringHelper.Join(' ' + Op + ' ', Expressions.GetEnumerator()) + ')';
        }
    }

    class Conjunction : Junction
    {
        public Conjunction()
            : base("and")
        { }
    }

    class Disjunction : Junction
    {
        public Disjunction()
            : base("or")
        { }
    }
}
