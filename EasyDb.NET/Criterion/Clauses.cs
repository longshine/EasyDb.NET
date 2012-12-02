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
        public static IExpression Value(Object value)
        {
            return new ValueExpression(value);
        }

        /// <summary>
        /// Wraps a plain text as an expression.
        /// </summary>
        public static IExpression Plain(String value)
        {
            return new PlainExpression(value);
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
        public static IExpression In(String fieldName, Object[] values)
        {
            IExpression[] exps = new IExpression[values.Length];
            for (int i = 0; i < exps.Length; i++)
            {
                exps[i] = Value(values[i]);
            }
            return In(Field(fieldName), exps);
        }

        /// <summary>
        /// Applies a "in" constraint to the expression.
        /// </summary>
        public static IExpression In(IExpression field, Object[] values)
        {
            IExpression[] exps = new IExpression[values.Length];
            for (int i = 0; i < exps.Length; i++)
            {
                exps[i] = Value(values[i]);
            }
            return In(field, exps);
        }

        /// <summary>
        /// Applies a "in" constraint to the expression.
        /// </summary>
        public static IExpression In(IExpression field, IExpression[] values)
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
            return new LikeExpression(Field(fieldName), Value(value));
        }

        /// <summary>
        /// Applies a "like" constraint to the field.
        /// </summary>
        public static IExpression Like(String fieldName, String value, MatchMode matchMode)
        {
            return new LikeExpression(Field(fieldName), Value(value), matchMode);
        }

        /// <summary>
        /// A case-insensitive "like", similar to Postgres <code>ilike</code>.
        /// </summary>
        public static IExpression Ilike(String fieldName, String value)
        {
            return new IlikeExpression(Field(fieldName), Value(value));
        }

        /// <summary>
        /// A case-insensitive "like", similar to Postgres <code>ilike</code>.
        /// </summary>
        public static IExpression Ilike(String fieldName, String value, MatchMode matchMode)
        {
            return new IlikeExpression(Field(fieldName), Value(value), matchMode);
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
        public static IExpression Function(String function, String fieldName)
        {
            return Function(function, new IExpression[] { Field(fieldName) });
        }

        /// <summary>
        /// Creates an expression that represents a function.
        /// </summary>
        /// <param name="function">the name of the function</param>
        /// <param name="args">the parameters</param>
        public static IExpression Function(String function, params IExpression[] args)
        {
            return new Function(function, args);
        }

        public static From From(String tableName)
        {
            return From(tableName, null);
        }

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

    class FieldExpression : IExpression
    {
        public String Filed { get; private set; }
        public String Table { get; private set; }

        public FieldExpression(String fieldName)
            : this(fieldName, null)
        { }

        public FieldExpression(String fieldName, String tableName)
        {
            Filed = fieldName;
            Table = tableName;
        }

        public override String ToString()
        {
            if (String.IsNullOrEmpty(Table))
                return Filed;
            else
                return Table + "." + Filed;
        }

        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }
    }

    class PropertyExpression : IExpression
    {
        public PropertyExpression(String propertyName, String otherPropertyName, String op)
        {
            PropertyName = new FieldExpression(propertyName);
            OtherPropertyName = new FieldExpression(otherPropertyName);
            Op = op;
        }

        public IExpression PropertyName { get; private set; }
        public IExpression OtherPropertyName { get; private set; }
        public String Op { get; private set; }

        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        public override String ToString()
        {
            return PropertyName + Op + OtherPropertyName;
        }
    }

    class ValueExpression : IExpression
    {
        public Object Value { get; private set; }

        public ValueExpression(Object value)
        {
            this.Value = value;
        }

        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        public override String ToString()
        {
            if (Value is String || Value is DateTime)
                return "'" + Value.ToString() + "'";
            else
                return Value.ToString();
        }
    }

    class StarExpression : IExpression
    {
        public static readonly String STAR = "*";

        public String Render(ICriteria criteria)
        {
            return STAR;
        }
    }

    class PlainExpression : IExpression
    {
        public String value { get; private set; }

        public PlainExpression(String value)
        {
            this.value = value;
        }

        public override String ToString()
        {
            return this.value;
        }

        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }
    }

    #endregion

    #region Conditions

    class BetweenExpression : IExpression
    {
        public BetweenExpression(IExpression expression, IExpression lower, IExpression upper)
        {
            Expression = expression;
            Lower = lower;
            Upper = upper;
        }

        public IExpression Expression { get; private set; }

        public IExpression Upper { get; private set; }

        public IExpression Lower { get; private set; }

        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

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

    class InExpression : IExpression
    {
        public IExpression Expression { get; private set; }
        public IExpression[] Values { get; private set; }

        public InExpression(IExpression expression, IExpression[] values)
        {
            Expression = expression;
            Values = values;
        }

        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        public override String ToString()
        {
            return Expression + " in (" + StringHelper.ToString(Values) + ')';
        }
    }

    class LikeExpression : IExpression
    {
        public LikeExpression(IExpression expression, IExpression value, MatchMode matchMode, String escapeChar, Boolean ignoreCase)
        {
            Expression = expression;
            Value = value;
            MatchMode = matchMode;
            EscapeChar = escapeChar;
            IgnoreCase = ignoreCase;
        }

        public LikeExpression(IExpression expression, IExpression value)
            : this(expression, value, MatchMode.Exact, null, false)
        { }

        public LikeExpression(IExpression expression, IExpression value, MatchMode matchMode)
            : this(expression, value, matchMode, null, false)
        { }

        public IExpression Expression { get; private set; }
        public IExpression Value { get; private set; }
        public String EscapeChar { get; private set; }
        public Boolean IgnoreCase { get; private set; }
        public MatchMode MatchMode { get; private set; }

        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        public override String ToString()
        {
            return Expression + " like " + MatchMode.ToMatchString(Value.ToString());
        }
    }

    class IlikeExpression : IExpression
    {
        public IlikeExpression(IExpression expression, IExpression value, MatchMode matchMode)
        {
            Expression = expression;
            Value = value;
            MatchMode = matchMode;
        }

        public IlikeExpression(IExpression expression, IExpression value)
            : this(expression, value, MatchMode.Exact)
        { }

        public IExpression Expression { get; private set; }
        public IExpression Value { get; private set; }
        public MatchMode MatchMode { get; private set; }

        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        public override String ToString()
        {
            return Expression + " ilike " + MatchMode.ToMatchString(Value.ToString());
        }
    }

    class LogicalExpression : IExpression
    {
        public IExpression Left { get; private set; }
        public IExpression Right { get; private set; }
        public String Op { get; private set; }

        public LogicalExpression(IExpression left, IExpression right, String op)
        {
            Left = left;
            Right = right;
            Op = op;
        }

        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        public override String ToString()
        {
            return "(" + Left + ' ' + Op + ' ' + Right + ")";
        }
    }

    class SimpleExpression : IExpression
    {
        public IExpression Left { get; private set; }
        public IExpression Right { get; private set; }
        public String Op { get; private set; }
        public Boolean IgnoreCase { get; private set; }

        public SimpleExpression(IExpression left, IExpression right, String op)
            : this(left, right, op, false)
        {
        }

        public SimpleExpression(IExpression left, IExpression right, String op, Boolean ignoreCase)
        {
            Left = left;
            Right = right;
            Op = op;
            IgnoreCase = ignoreCase;
        }

        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        public override String ToString()
        {
            return "(" + Left + ' ' + Op + ' ' + Right + ")";
        }
    }

    class NullExpression : IExpression
    {
        public IExpression Expression { get; private set; }

        public NullExpression(IExpression expression)
        {
            Expression = expression;
        }

        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        public override String ToString()
        {
            return Expression + " is null";
        }
    }

    class NotNullExpression : IExpression
    {
        public IExpression Expression { get; private set; }

        public NotNullExpression(IExpression expression)
        {
            Expression = expression;
        }

        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        public override String ToString()
        {
            return Expression + " is not null";
        }
    }

    class NotExpression : IExpression
    {
        public IExpression Expression { get; private set; }

        public NotExpression(IExpression expression)
        {
            Expression = expression;
        }

        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        public override String ToString()
        {
            return "not " + Expression;
        }
    }

    #endregion

    #region Fragments

    public class From : IFragment
    {
        private IFragment Source;

        public From(IFragment from)
        {
            this.Source = from;
        }

        public String Render(ICriteria criteria)
        {
            //return this.Source.ToSqlString(criteria);
            return null;
        }

        public class Table : IFragment
        {
            public String name { get; private set; }
            public String alias { get; private set; }

            public Table(String name, String alias)
            {
                this.name = name;
                this.alias = alias;
            }

            public String Render(ICriteria criteria)
            {
                return (criteria as ICriteriaRender).ToSqlString(this);
            }

            public override String ToString()
            {
                if (null == this.alias || 0 == this.alias.Length)
                    return this.name;
                else
                    return this.name + " " + this.alias;
            }
        }
    }

    #endregion

    class Function : IExpression
    {
        public String FunctionName { get; private set; }
        public IExpression[] Arguments { get; private set; }

        public Function(String name, IExpression[] args)
        {
            FunctionName = name;
            Arguments = args;
        }

        public String Render(ICriteria criteria)
        {
            return (criteria as ICriteriaRender).ToSqlString(this);
        }

        public override String ToString()
        {
            return FunctionName + "(" + StringHelper.ToString(Arguments) + ")";
        }
    }
}
