using System;
using System.Collections.Generic;

namespace LX.EasyDb.Criterion
{
    /// <summary>
    /// Static class for creating query expressions.
    /// </summary>
    public static class Clauses
    {
        /// <summary>
        /// Wraps a field as an expression.
        /// </summary>
        public static IExpression Field(String fieldName)
        {
            return new FieldExpression(fieldName);
        }

        /// <summary>
        /// Wraps a field as an expression.
        /// </summary>
        public static IExpression Field(String fieldName, String tableName)
        {
            return new FieldExpression(fieldName, tableName);
        }

        /// <summary>
        /// Wraps a parameter value as an expression.
        /// </summary>
        public static IExpression Value(Object val)
        {
            return new ValueExpression(val);
        }

        /// <summary>
        /// Wraps a plain text as an expression.
        /// </summary>
        public static IExpression Plain(String val)
        {
            return new PlainExpression(val);
        }

        /// <summary>
        /// Applies a "between" constraint to the field.
        /// </summary>
        public static IExpression Between(String fieldName, Object lower, Object upper)
        {
            return Between(Field(fieldName), Value(lower), Value(upper));
        }

        /// <summary>
        /// Applies a "between" constraint to the expression.
        /// </summary>
        public static IExpression Between(IExpression field, IExpression lower, IExpression upper)
        {
            return new BetweenExpression(field, lower, upper);
        }

        /// <summary>
        /// Applies a "in" constraint to the field.
        /// </summary>
        public static IExpression In(String fieldName, IEnumerable<Object> values)
        {
            return In(Field(fieldName), values);
        }

        /// <summary>
        /// Applies a "in" constraint to the expression.
        /// </summary>
        public static IExpression In(IExpression field, IEnumerable<Object> values)
        {
            List<IExpression> exps = new List<IExpression>();
            foreach (Object val in values)
            {
                exps.Add(Value(val));
            }
            return In(field, exps);
        }

        /// <summary>
        /// Applies a "in" constraint to the expression.
        /// </summary>
        public static IExpression In(IExpression field, IEnumerable<IExpression> values)
        {
            return new InExpression(field, values);
        }

        /// <summary>
        /// Applies a "is null" constraint to the field.
        /// </summary>
        public static IExpression IsNull(String fieldName)
        {
            return new NullExpression(Field(fieldName));
        }

        /// <summary>
        /// Applies a "is not null" constraint to the field.
        /// </summary>
        public static IExpression IsNotNull(String fieldName)
        {
            return new NotNullExpression(Field(fieldName));
        }

        /// <summary>
        /// Group expressions together in a single disjunction (A or B or C...).
        /// </summary>
        public static Junction Disjunction()
        {
            return new Disjunction();
        }

        /// <summary>
        /// Group expressions together in a single conjunction (A and B and C...).
        /// </summary>
        public static Junction Conjunction()
        {
            return new Conjunction();
        }

        /// <summary>
        /// Return the negation of an expression.
        /// </summary>
        public static IExpression Not(IExpression expression)
        {
            return new NotExpression(expression);
        }

        /// <summary>
        /// Return the conjuction of two expressions.
        /// </summary>
        public static IExpression And(IExpression left, IExpression right)
        {
            return new LogicalExpression(left, right, "and");
        }

        /// <summary>
        /// Return the disjuction of two expressions.
        /// </summary>
        public static IExpression Or(IExpression left, IExpression right)
        {
            return new LogicalExpression(left, right, "or");
        }

        /// <summary>
        /// Applies a "greater than" constraint to the field.
        /// </summary>
        public static IExpression Gt(String fieldName, Object value)
        {
            return new SimpleExpression(Field(fieldName), Value(value), ">");
        }

        /// <summary>
        /// Applies a "less than" constraint to the field.
        /// </summary>
        public static IExpression Lt(String fieldName, Object value)
        {
            return new SimpleExpression(Field(fieldName), Value(value), "<");
        }

        /// <summary>
        /// Applies a "greater than or equal" constraint to the field.
        /// </summary>
        public static IExpression Ge(String fieldName, Object value)
        {
            return new SimpleExpression(Field(fieldName), Value(value), ">=");
        }

        /// <summary>
        /// Applies a "less than or equal" constraint to the field.
        /// </summary>
        public static IExpression Le(String fieldName, Object value)
        {
            return new SimpleExpression(Field(fieldName), Value(value), "<=");
        }

        /// <summary>
        /// Applies an "equal" constraint to the field.
        /// </summary>
        public static IExpression Eq(String fieldName, Object value)
        {
            return Eq(Field(fieldName), Value(value));
        }

        /// <summary>
        /// Applies an "equal" constraint to the expression.
        /// </summary>
        public static IExpression Eq(IExpression field, IExpression value)
        {
            return new SimpleExpression(field, value, "=");
        }

        /// <summary>
        /// Applies a "not equal" constraint to the field.
        /// </summary>
        public static IExpression Ne(String fieldName, Object value)
        {
            return new SimpleExpression(Field(fieldName), Value(value), "<>");
        }

        /// <summary>
        /// Applies an "equals" constraint to each property in the key set of a key-value set.
        /// </summary>
        public static IExpression AllEq(IDictionary<String, Object> fieldNameValues)
        {
            Junction conj = Conjunction();
            foreach (KeyValuePair<String, Object> pair in fieldNameValues)
            {
                conj.Add(Eq(pair.Key, pair.Value));
            }
            return conj;
        }

        /// <summary>
        /// Applies a "like" constraint to the field.
        /// </summary>
        public static IExpression Like(String fieldName, String value)
        {
            return new LikeExpression(Field(fieldName), value);
        }

        /// <summary>
        /// Applies a "like" constraint to the field.
        /// </summary>
        public static IExpression Like(String fieldName, String value, MatchMode matchMode)
        {
            return new LikeExpression(Field(fieldName), value, matchMode);
        }

        /// <summary>
        /// A case-insensitive "like", similar to Postgres <code>ilike</code>.
        /// </summary>
        public static IExpression Ilike(String fieldName, String value)
        {
            return new IlikeExpression(Field(fieldName), value);
        }

        /// <summary>
        /// A case-insensitive "like", similar to Postgres <code>ilike</code>.
        /// </summary>
        public static IExpression Ilike(String fieldName, String value, MatchMode matchMode)
        {
            return new IlikeExpression(Field(fieldName), value, matchMode);
        }

        /// <summary>
        /// Minuses the field with a value.
        /// </summary>
        public static IExpression Add(String fieldName, Object value)
        {
            return new SimpleExpression(Field(fieldName), Value(value), "+");
        }

        /// <summary>
        /// Adds the field with a value.
        /// </summary>
        public static IExpression Minus(IExpression left, IExpression right)
        {
            return new SimpleExpression(left, right, "-");
        }

        /// <summary>
        /// Applies a mod operation on the field.
        /// </summary>
        public static IExpression Mod(String fieldName, Object value)
        {
            return Function("mod", Field(fieldName), Value(value));
        }

        /// <summary>
        /// Creates an expression that represents a function on the given field.
        /// </summary>
        public static IExpression Function(String func, String fieldName)
        {
            return Function(func, new IExpression[] { Field(fieldName) });
        }

        /// <summary>
        /// Creates an expression that represents a function.
        /// </summary>
        /// <param name="func">the name of the function</param>
        /// <param name="args">the parameters</param>
        public static IExpression Function(String func, params IExpression[] args)
        {
            return new Function(func, args);
        }

        /// <summary>
        /// Creates a from fragment.
        /// </summary>
        /// <param name="tableName">the target table name</param>
        [Obsolete]
        public static From From(String tableName)
        {
            return From(tableName, null);
        }

        /// <summary>
        /// Creates a from fragment.
        /// </summary>
        /// <param name="tableName">the target table name</param>
        /// <param name="alias">the alias</param>
        [Obsolete]
        public static From From(String tableName, String alias)
        {
            return new From(new From.Table(tableName, alias));
        }

        /// <summary>
        /// Applies an "equal" constraint to two properties.
        /// </summary>
        public static IExpression EqProperty(String propertyName, String otherPropertyName)
        {
            return new PropertyExpression(propertyName, otherPropertyName, "=");
        }

        /// <summary>
        /// Applies an "not equal" constraint to two properties.
        /// </summary>
        public static IExpression NeProperty(String propertyName, String otherPropertyName)
        {
            return new PropertyExpression(propertyName, otherPropertyName, "<>");
        }

        /// <summary>
        /// Applies an "less than" constraint to two properties.
        /// </summary>
        public static IExpression LtProperty(String propertyName, String otherPropertyName)
        {
            return new PropertyExpression(propertyName, otherPropertyName, "<");
        }

        /// <summary>
        /// Applies an "greater than" constraint to two properties.
        /// </summary>
        public static IExpression GtProperty(String propertyName, String otherPropertyName)
        {
            return new PropertyExpression(propertyName, otherPropertyName, ">");
        }

        /// <summary>
        /// Applies an "less than or equal" constraint to two properties.
        /// </summary>
        public static IExpression LeProperty(String propertyName, String otherPropertyName)
        {
            return new PropertyExpression(propertyName, otherPropertyName, "<=");
        }

        /// <summary>
        /// Applies an "greater than or equal" constraint to two properties.
        /// </summary>
        public static IExpression GeProperty(String propertyName, String otherPropertyName)
        {
            return new PropertyExpression(propertyName, otherPropertyName, ">=");
        }
    }

    #region Leaf nodes

    /// <summary>
    /// Represents a single field.
    /// </summary>
    public class FieldExpression : IExpression
    {
        /// <summary>
        /// Gets the name of this field.
        /// </summary>
        public String Filed { get; private set; }

        /// <summary>
        /// Gets the name of the table this field belongs to.
        /// </summary>
        public String Table { get; private set; }

        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="fieldName">the name of the field</param>
        public FieldExpression(String fieldName)
            : this(fieldName, null)
        { }

        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="fieldName">the name of the field</param>
        /// <param name="tableName">the name of the owner table</param>
        public FieldExpression(String fieldName, String tableName)
        {
            Filed = fieldName;
            Table = tableName;
        }

        /// <inheritdoc/>
        public override String ToString()
        {
            if (String.IsNullOrEmpty(Table))
                return Filed;
            else
                return Table + "." + Filed;
        }

        /// <inheritdoc/>
        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }
    }

    /// <summary>
    /// Represents an operation between two properties.
    /// </summary>
    public class PropertyExpression : IExpression
    {
        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="propertyName">the name of the first property</param>
        /// <param name="otherPropertyName">the name of the second property</param>
        /// <param name="op">the operator</param>
        public PropertyExpression(String propertyName, String otherPropertyName, String op)
        {
            PropertyName = new FieldExpression(propertyName);
            OtherPropertyName = new FieldExpression(otherPropertyName);
            Op = op;
        }

        /// <summary>
        /// Gets the first property.
        /// </summary>
        public IExpression PropertyName { get; private set; }
        
        /// <summary>
        /// Gets the second property.
        /// </summary>
        public IExpression OtherPropertyName { get; private set; }

        /// <summary>
        /// Gets the operator.
        /// </summary>
        public String Op { get; private set; }

        /// <inheritdoc/>
        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        /// <inheritdoc/>
        public override String ToString()
        {
            return PropertyName + Op + OtherPropertyName;
        }
    }

    /// <summary>
    /// Represents a single value.
    /// </summary>
    public class ValueExpression : IExpression
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        public Object Value { get; private set; }

        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="value">the value</param>
        public ValueExpression(Object value)
        {
            this.Value = value;
        }

        /// <inheritdoc/>
        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        /// <inheritdoc/>
        public override String ToString()
        {
            if (Value is String || Value is DateTime)
                return "'" + Value.ToString() + "'";
            else
                return Value.ToString();
        }
    }

    /// <summary>
    /// Represents a plain SQL fragment.
    /// </summary>
    public class PlainExpression : IExpression
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        public String Value { get; private set; }

        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="value">the value</param>
        public PlainExpression(String value)
        {
            this.Value = value;
        }

        /// <inheritdoc/>
        public override String ToString()
        {
            return this.Value;
        }

        /// <inheritdoc/>
        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }
    }

    #endregion

    #region Conditions

    /// <summary>
    /// Represents a BETWEEN clause.
    /// </summary>
    public class BetweenExpression : IExpression
    {
        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="expression">the expression to constrain</param>
        /// <param name="lower">expression of the lower bound</param>
        /// <param name="upper">expression of the upper bound</param>
        public BetweenExpression(IExpression expression, IExpression lower, IExpression upper)
        {
            Expression = expression;
            Lower = lower;
            Upper = upper;
        }

        /// <summary>
        /// Gets the expression to be constrained.
        /// </summary>
        public IExpression Expression { get; private set; }

        /// <summary>
        /// Gets the upper-bound expression.
        /// </summary>
        public IExpression Upper { get; private set; }

        /// <summary>
        /// Gets the lower-bound expression.
        /// </summary>
        public IExpression Lower { get; private set; }

        /// <inheritdoc/>
        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        /// <inheritdoc/>
        public override String ToString()
        {
            return StringHelper.CreateBuilder()
                    .Append(this.Expression)
                    .Append(" between ")
                    .Append(this.Lower)
                    .Append(" and ")
                    .Append(this.Upper)
                    .ToString();
        }
    }

    /// <summary>
    /// Represents an IN clause.
    /// </summary>
    public class InExpression : IExpression
    {
        /// <summary>
        /// Gets the expression to be constrained.
        /// </summary>
        public IExpression Expression { get; private set; }

        /// <summary>
        /// Gets the values.
        /// </summary>
        public IEnumerable<IExpression> Values { get; private set; }

        /// <summary>
        /// Instantiates.
        /// </summary>
        public InExpression(IExpression expression, IEnumerable<IExpression> values)
        {
            Expression = expression;
            Values = values;
        }

        /// <inheritdoc/>
        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        /// <inheritdoc/>
        public override String ToString()
        {
            return Expression + " in (" + StringHelper.ToString(Enumerable.Cast<Object>(Values)) + ')';
        }
    }

    /// <summary>
    /// Represents a LIKE clause.
    /// </summary>
    public class LikeExpression : IExpression
    {
        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="expression">the expression to be constrained</param>
        /// <param name="value">the value</param>
        /// <param name="matchMode">the <see cref="MatchMode"/></param>
        /// <param name="escapeChar">the escape char</param>
        /// <param name="ignoreCase">ignore case or not</param>
        public LikeExpression(IExpression expression, String value, MatchMode matchMode, String escapeChar, Boolean ignoreCase)
        {
            Expression = expression;
            Value = value;
            MatchMode = matchMode;
            EscapeChar = escapeChar;
            IgnoreCase = ignoreCase;
        }

        /// <summary>
        /// Instantiates with exact <see cref="MatchMode"/>, null escape char and case-sensitive.
        /// </summary>
        /// <param name="expression">the expression to be constrained</param>
        /// <param name="value">the value</param>
        public LikeExpression(IExpression expression, String value)
            : this(expression, value, MatchMode.Exact, null, false)
        { }

        /// <summary>
        /// Instantiates with null escape char and case-sensitive.
        /// </summary>
        /// <param name="expression">the expression to be constrained</param>
        /// <param name="value">the value</param>
        /// <param name="matchMode">the <see cref="MatchMode"/></param>
        public LikeExpression(IExpression expression, String value, MatchMode matchMode)
            : this(expression, value, matchMode, null, false)
        { }

        /// <summary>
        /// Gets the expression to be constrained.
        /// </summary>
        public IExpression Expression { get; private set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public String Value { get; private set; }

        /// <summary>
        /// Gets the escape char.
        /// </summary>
        public String EscapeChar { get; private set; }

        /// <summary>
        /// Checks if ignore case or not.
        /// </summary>
        public Boolean IgnoreCase { get; private set; }

        /// <summary>
        /// Gets the <see cref="MatchMode"/>.
        /// </summary>
        public MatchMode MatchMode { get; private set; }

        /// <inheritdoc/>
        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        /// <inheritdoc/>
        public override String ToString()
        {
            return Expression + " like " + MatchMode.ToMatchString(Value);
        }
    }

    /// <summary>
    /// Represents an ILIKE clause.
    /// </summary>
    public class IlikeExpression : IExpression
    {
        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="expression">the expression to be constrained</param>
        /// <param name="value">the value</param>
        /// <param name="matchMode">the <see cref="MatchMode"/></param>
        public IlikeExpression(IExpression expression, String value, MatchMode matchMode)
        {
            Expression = expression;
            Value = value;
            MatchMode = matchMode;
        }

        /// <summary>
        /// Instantiates with exact <see cref="MatchMode"/>.
        /// </summary>
        /// <param name="expression">the expression to be constrained</param>
        /// <param name="value">the value</param>
        public IlikeExpression(IExpression expression, String value)
            : this(expression, value, MatchMode.Exact)
        { }

        /// <summary>
        /// Gets the expression to be constrained.
        /// </summary>
        public IExpression Expression { get; private set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public String Value { get; private set; }

        /// <summary>
        /// Gets the <see cref="MatchMode"/>.
        /// </summary>
        public MatchMode MatchMode { get; private set; }

        /// <inheritdoc/>
        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        /// <inheritdoc/>
        public override String ToString()
        {
            return Expression + " ilike " + MatchMode.ToMatchString(Value);
        }
    }

    /// <summary>
    /// Represents a logical clause.
    /// </summary>
    public class LogicalExpression : IExpression
    {
        /// <summary>
        /// Gets the left part of this logical clause.
        /// </summary>
        public IExpression Left { get; private set; }

        /// <summary>
        /// Gets the right part of this logical clause.
        /// </summary>
        public IExpression Right { get; private set; }

        /// <summary>
        /// Gets the operator of this logical clause.
        /// </summary>
        public String Op { get; private set; }

        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="left">the lefty</param>
        /// <param name="right">the righty</param>
        /// <param name="op">the operator</param>
        public LogicalExpression(IExpression left, IExpression right, String op)
        {
            Left = left;
            Right = right;
            Op = op;
        }

        /// <inheritdoc/>
        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        /// <inheritdoc/>
        public override String ToString()
        {
            return "(" + Left + ' ' + Op + ' ' + Right + ")";
        }
    }

    /// <summary>
    /// Represents a simple expression.
    /// <remarks>
    /// TODO same as LogicalExpression?
    /// </remarks>
    /// </summary>
    public class SimpleExpression : IExpression
    {
        /// <summary>
        /// Gets the left part of this clause.
        /// </summary>
        public IExpression Left { get; private set; }

        /// <summary>
        /// Gets the right part of this clause.
        /// </summary>
        public IExpression Right { get; private set; }

        /// <summary>
        /// Gets the operator of this clause.
        /// </summary>
        public String Op { get; private set; }

        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="left">the lefty</param>
        /// <param name="right">the righty</param>
        /// <param name="op">the operator</param>
        public SimpleExpression(IExpression left, IExpression right, String op)
        {
            Left = left;
            Right = right;
            Op = op;
        }

        /// <inheritdoc/>
        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        /// <inheritdoc/>
        public override String ToString()
        {
            return "(" + Left + ' ' + Op + ' ' + Right + ")";
        }
    }

    /// <summary>
    /// Represents an IS NULL clause.
    /// </summary>
    public class NullExpression : IExpression
    {
        /// <summary>
        /// Gets the expression to be constrained.
        /// </summary>
        public IExpression Expression { get; private set; }

        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="expression"></param>
        public NullExpression(IExpression expression)
        {
            Expression = expression;
        }

        /// <inheritdoc/>
        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        /// <inheritdoc/>
        public override String ToString()
        {
            return Expression + " is null";
        }
    }

    /// <summary>
    /// Represents an IS NOT NULL clause.
    /// </summary>
    public class NotNullExpression : IExpression
    {
        /// <summary>
        /// Gets the expression to be constrained.
        /// </summary>
        public IExpression Expression { get; private set; }

        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="expression"></param>
        public NotNullExpression(IExpression expression)
        {
            Expression = expression;
        }

        /// <inheritdoc/>
        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        /// <inheritdoc/>
        public override String ToString()
        {
            return Expression + " is not null";
        }
    }

    /// <summary>
    /// Represents a NOT clause.
    /// </summary>
    public class NotExpression : IExpression
    {
        /// <summary>
        /// Gets the expression to be constrained.
        /// </summary>
        public IExpression Expression { get; private set; }

        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="expression"></param>
        public NotExpression(IExpression expression)
        {
            Expression = expression;
        }

        /// <inheritdoc/>
        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        /// <inheritdoc/>
        public override String ToString()
        {
            return "not " + Expression;
        }
    }

    #endregion

    #region Fragments

    /// <summary>
    /// Represents a FROM fragment.
    /// </summary>
    [Obsolete]
    public class From : IFragment
    {
        private IFragment Source;

        /// <summary>
        /// Instantiates.
        /// </summary>
        public From(IFragment from)
        {
            this.Source = from;
        }

        /// <inheritdoc/>
        public String Render(ICriteria criteria)
        {
            //return this.Source.ToSqlString(criteria);
            return null;
        }

        /// <summary>
        /// Represents a table.
        /// </summary>
        [Obsolete]
        public class Table : IFragment
        {
            /// <summary>
            /// Gets the name of this table.
            /// </summary>
            public String Name { get; private set; }

            /// <summary>
            /// Gets the alias of this table.
            /// </summary>
            public String Alias { get; private set; }

            /// <summary>
            /// Instantiates.
            /// </summary>
            public Table(String name, String alias)
            {
                this.Name = name;
                this.Alias = alias;
            }

            /// <inheritdoc/>
            public String Render(ICriteria criteria)
            {
                return (criteria as ICriteriaRender).ToSqlString(this);
            }

            /// <inheritdoc/>
            public override String ToString()
            {
                if (null == this.Alias || 0 == this.Alias.Length)
                    return this.Name;
                else
                    return this.Name + " " + this.Alias;
            }
        }
    }

    #endregion

    /// <summary>
    /// Represents a function.
    /// </summary>
    public class Function : IExpression
    {
        /// <summary>
        /// Gets the name of this function.
        /// </summary>
        public String FunctionName { get; private set; }

        /// <summary>
        /// Gets the arguments of this function.
        /// </summary>
        public IExpression[] Arguments { get; private set; }

        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="name">the name of the function</param>
        /// <param name="args">the arguments of the function</param>
        public Function(String name, IExpression[] args)
        {
            FunctionName = name;
            Arguments = args;
        }

        /// <inheritdoc/>
        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        /// <inheritdoc/>
        public override String ToString()
        {
            return FunctionName + "(" + StringHelper.ToString(Arguments) + ")";
        }
    }
}
