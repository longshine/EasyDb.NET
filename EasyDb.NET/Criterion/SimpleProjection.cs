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

    /// <summary>
    /// Represents an aggregation funciton projection
    /// </summary>
    public class AggregateProjection : SimpleProjection
    {
        /// <summary>
        /// Gets the name of this aggregation function.
        /// </summary>
        public String FunctionName { get; private set; }

        /// <summary>
        /// Gets the expression of this aggregation function.
        /// </summary>
        public IExpression Expression { get; private set; }

        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="functionName">the name of this aggregation function</param>
        /// <param name="expression">the expression of this aggregation function</param>
        public AggregateProjection(String functionName, IExpression expression)
        {
            FunctionName = functionName;
            Expression = expression;
        }

        /// <inheritdoc/>
        public override String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        /// <inheritdoc/>
        public override String ToString()
        {
            return FunctionName + "(" + Expression + ')';
        }

        /// <summary>
        /// Builds parameters for this function.
        /// </summary>
        public virtual IList<Object> BuildFunctionParameterList(ICriteria criteria)
        {
            List<Object> list = new List<Object>();
            list.Add(Expression.Render(criteria));
            return list;
        }
    }

    /// <summary>
    /// Represents count() function.
    /// </summary>
    public class CountProjection : AggregateProjection
    {
        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="expression">the expression to count</param>
        public CountProjection(IExpression expression)
            : base("count", expression)
        {
        }

        /// <summary>
        /// Gets or sets distinct or not.
        /// </summary>
        public Boolean Distinct { get; set; }

        /// <inheritdoc/>
        public override String ToString()
        {
            if (Distinct)
                return "distinct " + base.ToString();
            else
                return base.ToString();
        }

        /// <inheritdoc/>
        public override IList<Object> BuildFunctionParameterList(ICriteria criteria)
        {
            List<Object> list = new List<Object>();
            if (Distinct)
                list.Add("distinct");
            list.Add(Expression.Render(criteria));
            return list;
        }
    }

    /// <summary>
    /// Represents count(*).
    /// </summary>
    public class RowCountProjection : SimpleProjection
    {
        /// <summary>
        /// Arguments for row count.
        /// </summary>
        public static readonly IList<Object> Arguments = new List<Object>(new String[] { "*" });

        /// <inheritdoc/>
        public override String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        /// <inheritdoc/>
        public override String ToString()
        {
            return "count(*)";
        }
    }

    /// <summary>
    /// Represents projecting a property.
    /// </summary>
    public class PropertyProjection : SimpleProjection
    {
        private Boolean _grouped;

        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="propertyName">the name of the property to project</param>
        public PropertyProjection(String propertyName)
            : this(propertyName, false)
        { }

        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="propertyName">the name of the property to project</param>
        /// <param name="grouped">true if this property should be included in group-by.</param>
        public PropertyProjection(String propertyName, Boolean grouped)
        {
            PropertyName = propertyName;
            _grouped = grouped;
        }

        /// <summary>
        /// Gets the property name.
        /// </summary>
        public String PropertyName { get; private set; }

        /// <inheritdoc/>
        public override Boolean Grouped
        {
            get { return _grouped; }
        }

        /// <inheritdoc/>
        public override String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        /// <inheritdoc/>
        public override String ToGroupString(ICriteria criteria)
        {
            if (Grouped)
                return Clauses.Field(PropertyName).Render(criteria);
            else
                return base.ToGroupString(criteria);
        }
    }

    /// <summary>
    /// Represents a projection of an expression.
    /// </summary>
    public class ExpressionProjection : IProjection
    {
        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="expression">the expression to project</param>
        public ExpressionProjection(IExpression expression)
        {
            Expression = expression;
        }

        /// <inheritdoc/>
        public Boolean Grouped { get; set; }

        /// <inheritdoc/>
        public String Alias { get; set; }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        public IExpression Expression { get; private set; }

        /// <inheritdoc/>
        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        /// <inheritdoc/>
        public String ToGroupString(ICriteria criteria)
        {
            if (Grouped)
                return Expression.Render(criteria);
            else
                return String.Empty;
        }
    }

    /// <summary>
    /// Represents a distinct projection.
    /// </summary>
    public class Distinct : IProjection
    {
        private readonly IProjection _projection;

        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="projection">the projection to be distinct</param>
        public Distinct(IProjection projection)
        {
            _projection = projection;
        }

        /// <inheritdoc/>
        public Boolean Grouped
        {
            get { return _projection.Grouped; }
        }

        /// <inheritdoc/>
        public String Alias
        {
            get { return _projection.Alias; }
            set { _projection.Alias = value; }
        }

        /// <inheritdoc/>
        public String Render(ICriteria criteria)
        {
            return "distinct " + _projection.Render(criteria);
        }

        /// <inheritdoc/>
        public String ToGroupString(ICriteria criteria)
        {
            return _projection.ToGroupString(criteria);
        }

        /// <inheritdoc/>
        public override String ToString()
        {
            return "distinct " + _projection.ToString();
        }
    }
}
