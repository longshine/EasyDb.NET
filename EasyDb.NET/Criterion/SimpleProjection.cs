//
// LX.EasyDb.Criterion.SimpleProjection.cs
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
    /// Represents a single-column projection.
    /// </summary>
    public abstract class SimpleProjection : IProjection
    {
        /// <summary>
        /// Gets or sets the alias of this projection.
        /// </summary>
        public String Alias { get; set; }

        /// <summary>
        /// Checks if this projection is grouped.
        /// </summary>
        public virtual Boolean Grouped
        {
            get { return false; }
        }

        /// <summary>
        /// Renders this projection.
        /// </summary>
        public abstract String Render(ICriteria criteria);

        /// <summary>
        /// Gets the grouping string of this projection if grouped.
        /// </summary>
        public virtual String ToGroupString(ICriteria criteria)
        {
            throw new InvalidOperationException("not a grouping projection");
        }
    }

    class AggregateProjection : SimpleProjection
    {
        public String FunctionName { get; private set; }
        public IExpression Expression { get; private set; }

        public AggregateProjection(String functionName, IExpression expression)
        {
            FunctionName = functionName;
            Expression = expression;
        }

        public override String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        public override String ToString()
        {
            return FunctionName + "(" + Expression + ')';
        }

        public virtual IList<Object> BuildFunctionParameterList(ICriteria criteria)
        {
            List<Object> list = new List<Object>();
            list.Add(Expression.Render(criteria));
            return list;
        }
    }

    class CountProjection : AggregateProjection
    {
        public CountProjection(IExpression expression)
            : base("count", expression)
        {
        }

        public Boolean Distinct { get; set; }

        public override String ToString()
        {
            if (Distinct)
                return "distinct " + base.ToString();
            else
                return base.ToString();
        }

        public override IList<Object> BuildFunctionParameterList(ICriteria criteria)
        {
            List<Object> list = new List<Object>();
            if (Distinct)
                list.Add("distinct");
            list.Add(Expression.Render(criteria));
            return list;
        }
    }

    class RowCountProjection : SimpleProjection
    {
        public static readonly IList<Object> Arguments = new List<Object>(new String[] { "*" });

        public override String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        public override String ToString()
        {
            return "count(*)";
        }
    }

    class PropertyProjection : SimpleProjection
    {
        private Boolean _grouped;

        public PropertyProjection(String propertyName)
            : this(propertyName, false)
        { }

        public PropertyProjection(String propertyName, Boolean grouped)
        {
            PropertyName = propertyName;
            _grouped = grouped;
        }

        public String PropertyName { get; private set; }

        public override Boolean Grouped
        {
            get { return _grouped; }
        }

        public override String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }
    }

    class ExpressionProjection : SimpleProjection
    {
        public ExpressionProjection(IExpression expression)
        {
            Expression = expression;
        }

        public IExpression Expression { get; private set; }

        public override String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }
    }

    class Distinct : IProjection
    {
        private readonly IProjection _projection;

        public Distinct(IProjection projection)
        {
            _projection = projection;
        }

        public Boolean Grouped
        {
            get { return _projection.Grouped; }
        }

        public String Alias
        {
            get { return _projection.Alias; }
            set { _projection.Alias = value; }
        }

        public String Render(ICriteria criteria)
        {
            return "distinct " + _projection.Render(criteria);
        }

        public String ToGroupString(ICriteria criteria)
        {
            return _projection.ToGroupString(criteria);
        }

        public override String ToString()
        {
            return "distinct " + _projection.ToString();
        }
    }
}
