using System;
using System.Collections.Generic;
using System.Text;

namespace LX.EasyDb.Criterion
{
    /// <summary>
    /// Static class for creating query expressions.
    /// </summary>
    public static class Clauses
    {
        public static IExpression Field(String fieldName)
        {
            return new FieldExpression(fieldName);
        }

        public static IExpression Field(String fieldName, String tableName)
        {
            return new FieldExpression(fieldName, tableName);
        }

        public static IExpression Value(Object value)
        {
            return new ValueExpression(value);
        }

        public static IExpression Plain(String value)
        {
            return new PlainExpression(value);
        }

        public static IExpression Between(String fieldName, Object lower, Object upper)
        {
            return Between(Field(fieldName), Value(lower), Value(upper));
        }

        public static IExpression Between(IExpression field, IExpression lower, IExpression upper)
        {
            return new BetweenExpression(field, lower, upper);
        }

        public static IExpression In(String fieldName, Object[] values)
        {
            IExpression[] exps = new IExpression[values.Length];
            for (int i = 0; i < exps.Length; i++)
            {
                exps[i] = Value(values[i]);
            }
            return In(Field(fieldName), exps);
        }

        public static IExpression In(IExpression field, Object[] values)
        {
            IExpression[] exps = new IExpression[values.Length];
            for (int i = 0; i < exps.Length; i++)
            {
                exps[i] = Value(values[i]);
            }
            return In(field, exps);
        }

        public static IExpression In(IExpression field, IExpression[] values)
        {
            return new InExpression(field, values);
        }

        public static IExpression IsNull(String fieldName)
        {
            return new NullExpression(Field(fieldName));
        }

        public static IExpression IsNotNull(String fieldName)
        {
            return new NotNullExpression(Field(fieldName));
        }

        public static Junction Disjunction()
        {
            return new Disjunction();
        }

        public static Junction Conjunction()
        {
            return new Conjunction();
        }

        public static IExpression Not(IExpression expression)
        {
            return new NotExpression(expression);
        }

        public static IExpression And(IExpression left, IExpression right)
        {
            return new LogicalExpression(left, right, "and");
        }

        public static IExpression Or(IExpression left, IExpression right)
        {
            return new LogicalExpression(left, right, "or");
        }

        public static IExpression Gt(String fieldName, Object value)
        {
            return new SimpleExpression(Field(fieldName), Value(value), ">");
        }

        public static IExpression Lt(String fieldName, Object value)
        {
            return new SimpleExpression(Field(fieldName), Value(value), "<");
        }

        public static IExpression Ge(String fieldName, Object value)
        {
            return new SimpleExpression(Field(fieldName), Value(value), ">=");
        }

        public static IExpression Le(String fieldName, Object value)
        {
            return new SimpleExpression(Field(fieldName), Value(value), "<=");
        }

        public static IExpression Eq(String fieldName, Object value)
        {
            return Eq(Field(fieldName), Value(value));
        }

        public static IExpression Eq(IExpression field, IExpression value)
        {
            return new SimpleExpression(field, value, "=");
        }

        public static IExpression Ne(String fieldName, Object value)
        {
            return new SimpleExpression(Field(fieldName), Value(value), "<>");
        }

        public static IExpression AllEq(IDictionary<String, Object> fieldNameValues)
        {
            Junction conj = Conjunction();
            foreach (KeyValuePair<String, Object> pair in fieldNameValues)
            {
                conj.Add(Eq(pair.Key, pair.Value));
            }
            return conj;
        }

        public static IExpression Like(String fieldName, String value)
        {
            return new LikeExpression(Field(fieldName), Value(value));
        }

        public static IExpression Like(String fieldName, String value, MatchMode matchMode)
        {
            return new LikeExpression(Field(fieldName), Value(value), matchMode);
        }

        public static IExpression Ilike(String fieldName, String value)
        {
            return new IlikeExpression(Field(fieldName), Value(value));
        }

        public static IExpression Ilike(String fieldName, String value, MatchMode matchMode)
        {
            return new IlikeExpression(Field(fieldName), Value(value), matchMode);
        }

        public static IExpression Add(String fieldName, Object value)
        {
            return new SimpleExpression(Field(fieldName), Value(value), "+");
        }

        public static IExpression Minus(IExpression left, IExpression right)
        {
            return new SimpleExpression(left, right, "-");
        }

        public static IExpression Mod(String fieldName, Object value)
        {
            return Function("mod", Field(fieldName), Value(value));
        }

        public static IExpression Max(String fieldName)
        {
            return new AggregateProjection("max", fieldName);
        }

        public static IExpression Min(String fieldName)
        {
            return new AggregateProjection("min", fieldName);
        }

        public static IExpression Count(String fieldName)
        {
            return new AggregateProjection("count", fieldName);
        }

        public static IExpression Sum(String fieldName)
        {
            return new AggregateProjection("sum", fieldName);
        }

        public static IExpression Avg(String fieldName)
        {
            return new AggregateProjection("avg", fieldName);
        }

        public static IExpression Function(String function, String fieldName)
        {
            return Function(function, new IExpression[] { Field(fieldName) });
        }

        public static IExpression Function(String function, params IExpression[] args)
        {
            return new Function(function, args);
        }

        public static Select Select(IExpression expression, String alias, Boolean distinct)
        {
            return new Select(expression, alias, distinct);
        }

        public static Select Select(IExpression expression)
        {
            return new Select(expression, null, false);
        }

        public static Select Select(String fieldName)
        {
            return Select(Field(fieldName), null, false);
        }

        public static Select Select(String fieldName, String alias)
        {
            return Select(Field(fieldName), alias, false);
        }

        public static Select Select(String fieldName, String alias, Boolean distinct)
        {
            return Select(Field(fieldName), alias, distinct);
        }

        public static From From(String tableName)
        {
            return From(tableName, null);
        }

        public static From From(String tableName, String alias)
        {
            return new From(new From.Table(tableName, alias));
        }

        public static Order Asc(String fieldName)
        {
            return new Order(Clauses.Field(fieldName), true);
        }

        public static Order Asc(IExpression expression)
        {
            return new Order(expression, true);
        }

        public static Order Desc(String fieldName)
        {
            return new Order(Clauses.Field(fieldName), false);
        }

        public static IExpression EqProperty(String propertyName, String otherPropertyName)
        {
            return new PropertyExpression(propertyName, otherPropertyName, "=");
        }

        public static IExpression NeProperty(String propertyName, String otherPropertyName)
        {
            return new PropertyExpression(propertyName, otherPropertyName, "<>");
        }

        public static IExpression LtProperty(String propertyName, String otherPropertyName)
        {
            return new PropertyExpression(propertyName, otherPropertyName, "<");
        }

        public static IExpression GtProperty(String propertyName, String otherPropertyName)
        {
            return new PropertyExpression(propertyName, otherPropertyName, ">");
        }

        public static IExpression LeProperty(String propertyName, String otherPropertyName)
        {
            return new PropertyExpression(propertyName, otherPropertyName, "<=");
        }

        public static IExpression GeProperty(String propertyName, String otherPropertyName)
        {
            return new PropertyExpression(propertyName, otherPropertyName, ">=");
        }
    }

    #region Leaf nodes

    public class FieldExpression : IExpression
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

        public String ToSqlString(ICriteriaRender criteria)
        {
            return criteria.ToSqlString(this);
        }
    }

    public class PropertyExpression : IExpression
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

        public string ToSqlString(ICriteriaRender criteria)
        {
            return criteria.ToSqlString(this);
        }

        public override String ToString()
        {
            return PropertyName + Op + OtherPropertyName;
        }
    }

    public class ValueExpression : IExpression
    {
        public Object Value { get; private set; }

        public ValueExpression(Object value)
        {
            this.Value = value;
        }

        public String ToSqlString(ICriteriaRender criteria)
        {
            return criteria.ToSqlString(this);
        }

        public override String ToString()
        {
            if (Value is String || Value is DateTime)
                return "'" + Value.ToString() + "'";
            else
                return Value.ToString();
        }
    }

    public class StarExpression : IExpression
    {
        public static readonly String STAR = "*";

        public String ToSqlString(ICriteriaRender criteria)
        {
            return STAR;
        }
    }

    public class PlainExpression : IExpression
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

        public String ToSqlString(ICriteriaRender criteria)
        {
            return criteria.ToSqlString(this);
        }
    }

    #endregion

    #region Conditions

    public class BetweenExpression : IExpression
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

        public String ToSqlString(ICriteriaRender criteria)
        {
            return criteria.ToSqlString(this);
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

    public class InExpression : IExpression
    {
        public IExpression Expression { get; private set; }
        public IExpression[] Values { get; private set; }

        public InExpression(IExpression expression, IExpression[] values)
        {
            Expression = expression;
            Values = values;
        }

        public String ToSqlString(ICriteriaRender criteria)
        {
            return criteria.ToSqlString(this);
        }

        public override String ToString()
        {
            return Expression + " in (" + StringHelper.ToString(Values) + ')';
        }
    }

    public class LikeExpression : IExpression
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

        public String ToSqlString(ICriteriaRender criteria)
        {
            return criteria.ToSqlString(this);
        }

        public override String ToString()
        {
            return Expression + " like " + MatchMode.ToMatchString(Value.ToString());
        }
    }

    public class IlikeExpression : IExpression
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

        public string ToSqlString(ICriteriaRender criteria)
        {
            return criteria.ToSqlString(this);
        }

        public override String ToString()
        {
            return Expression + " ilike " + MatchMode.ToMatchString(Value.ToString());
        }
    }

    public class LogicalExpression : IExpression
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

        public String ToSqlString(ICriteriaRender criteria)
        {
            return criteria.ToSqlString(this);
        }

        public override String ToString()
        {
            return "(" + Left + ' ' + Op + ' ' + Right + ")";
        }
    }

    public class SimpleExpression : IExpression
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

        public String ToSqlString(ICriteriaRender criteria)
        {
            return criteria.ToSqlString(this);
        }

        public override String ToString()
        {
            return "(" + Left + ' ' + Op + ' ' + Right + ")";
        }
    }

    public class NullExpression : IExpression
    {
        public IExpression Expression { get; private set; }

        public NullExpression(IExpression expression)
        {
            Expression = expression;
        }

        public String ToSqlString(ICriteriaRender criteria)
        {
            return criteria.ToSqlString(this);
        }

        public override String ToString()
        {
            return Expression + " is null";
        }
    }

    public class NotNullExpression : IExpression
    {
        public IExpression Expression { get; private set; }

        public NotNullExpression(IExpression expression)
        {
            Expression = expression;
        }

        public String ToSqlString(ICriteriaRender criteria)
        {
            return criteria.ToSqlString(this);
        }

        public override String ToString()
        {
            return Expression + " is not null";
        }
    }

    public class NotExpression : IExpression
    {
        public IExpression Expression { get; private set; }

        public NotExpression(IExpression expression)
        {
            Expression = expression;
        }

        public String ToSqlString(ICriteriaRender criteria)
        {
            return criteria.ToSqlString(this);
        }

        public override String ToString()
        {
            return "not " + Expression;
        }
    }

    #endregion

    #region Fragments

    public class Order : IFragment
    {
        public IExpression Expression { get; private set; }
        public Boolean Ascending { get; private set; }

        public Order(IExpression expression, Boolean ascending)
        {
            Expression = expression;
            Ascending = ascending;
        }

        public String ToSqlString(ICriteriaRender criteria)
        {
            return criteria.ToSqlString(this);
        }

        public override String ToString()
        {
            return Expression + (Ascending ? " asc" : " desc");
        }
    }

    public class From : IFragment
    {
        private IFragment Source;

        public From(IFragment from)
        {
            this.Source = from;
        }

        public String ToSqlString(ICriteriaRender criteria)
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

            public String ToSqlString(ICriteriaRender criteria)
            {
                return criteria.ToSqlString(this);
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

    public class Select : IFragment
    {
        public IExpression Expression { get; private set; }
        public String Alias { get; private set; }
        public Boolean Distinct { get; private set; }

        public Select(IExpression expression, String alias, Boolean distinct)
        {
            this.Expression = expression;
            this.Alias = alias;
            this.Distinct = distinct;
        }

        public String ToSqlString(ICriteriaRender criteria)
        {
            return criteria.ToSqlString(this);
        }

        public override String ToString()
        {
            StringBuilder sb = StringHelper.CreateBuilder();
            if (this.Distinct)
                sb.Append("DISTINCT ");
            sb.Append(this.Expression);
            if (null != this.Alias && this.Alias.Length > 0)
            {
                sb.Append(" AS ");
                sb.Append(this.Alias);
            }
            return sb.ToString();
        }
    }

    #endregion

    public class AggregateProjection : IExpression
    {
        public String FunctionName { get; private set; }
        public String FiledName { get; private set; }

        public AggregateProjection(String functionName, String fieldName)
        {
            FunctionName = functionName;
            FiledName = fieldName;
        }

        public String ToSqlString(ICriteriaRender criteria)
        {
            return criteria.ToSqlString(this);
        }

        public override String ToString()
        {
            return FunctionName + "(" + FiledName + ')';
        }
    }

    public class Function : IExpression
    {
        public String FunctionName { get; private set; }
        public IExpression[] Arguments { get; private set; }

        public Function(String name, IExpression[] args)
        {
            FunctionName = name;
            Arguments = args;
        }

        public String ToSqlString(ICriteriaRender criteria)
        {
            return criteria.ToSqlString(this);
        }

        public override String ToString()
        {
            return FunctionName + "(" + StringHelper.ToString(Arguments) + ")";
        }
    }

    /// <summary>
    /// Represents an strategy for matching Strings using "like".
    /// </summary>
    public abstract class MatchMode
    {
        /// <summary>
        /// Match the entire String to the pattern.
        /// </summary>
        public static readonly MatchMode Exact = new MatchExactMode();
        /// <summary>
        /// Match the start of the String to the pattern.
        /// </summary>
        public static readonly MatchMode Start = new MatchStartMode();
        /// <summary>
        /// Match the end of the String to the pattern.
        /// </summary>
        public static readonly MatchMode End = new MatchEndMode();
        /// <summary>
        /// Match the pattern anywhere in the String.
        /// </summary>
        public static readonly MatchMode Anywhere = new MatchAnywhereMode();

        private static Dictionary<String, MatchMode> Instances = new Dictionary<String, MatchMode>();
        private String _name;

        static MatchMode()
        {
            Instances.Add(Exact._name, Exact);
            Instances.Add(Start._name, Start);
            Instances.Add(End._name, End);
            Instances.Add(Anywhere._name, Anywhere);
        }

        protected internal MatchMode(String name)
        {
            _name = name;
        }

        /// <summary>
        /// Converts the pattern, by appending/prepending "%".
        /// </summary>
        public abstract String ToMatchString(String pattern);

        public override String ToString()
        {
            return _name;
        }

        class MatchExactMode : MatchMode
        {
            public MatchExactMode() : base("EXACT") { }

            public override String ToMatchString(String pattern)
            {
                return pattern;
            }
        }

        class MatchStartMode : MatchMode
        {
            public MatchStartMode() : base("START") { }

            public override String ToMatchString(String pattern)
            {
                return pattern + "%";
            }
        }

        class MatchEndMode : MatchMode
        {
            public MatchEndMode() : base("END") { }

            public override String ToMatchString(String pattern)
            {
                return "%" + pattern;
            }
        }

        class MatchAnywhereMode : MatchMode
        {
            public MatchAnywhereMode() : base("ANYWHERE") { }

            public override String ToMatchString(String pattern)
            {
                return "%" + pattern + "%";
            }
        }
    }
}
