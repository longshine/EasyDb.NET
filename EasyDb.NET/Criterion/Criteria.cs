using System;
using System.Collections.Generic;
using System.Text;
using LX.EasyDb.Dialects;

namespace LX.EasyDb.Criterion
{
    class Criteria<T> : ICriteria<T>, ICriteriaRender
    {
        private IConnectionSupport _connection;
        private IConnectionFactorySupport _factory;
        private List<IExpression> _conditions = new List<IExpression>();
        private List<Order> _orders = new List<Order>();
        private Dictionary<String, Object> _params = new Dictionary<String, Object>();
        private Mapping.Table _table;

        public Criteria(IConnectionSupport connection, IConnectionFactorySupport factory)
        {
            _table = factory.Mapping.FindTable(typeof(T));
            _connection = connection;
            _factory = factory;
            Parameterized = true;
        }

        public Boolean Parameterized { get; set; }

        private String RegisterParam(String name, Object value)
        {
            _params[name] = value;
            return _factory.Dialect.ParamPrefix + name;
        }

        private String RegisterParam(Object value)
        {
            return RegisterParam("p_" + _params.Count, value);
        }

        public ICriteria<T> Add(IExpression condition)
        {
            _conditions.Add(condition);
            return this;
        }

        public ICriteria<T> AddOrder(Order order)
        {
            _orders.Add(order);
            return this;
        }

        public IList<Order> Orders
        {
            get { return _orders; }
        }

        public IList<IExpression> Conditions
        {
            get { return _conditions; }
        }

        public IDictionary<String, Object> Parameters
        {
            get { return _params; }
        }

        public IEnumerable<T> List()
        {
            return Enumerable.ToList(_connection.List(this));
        }

        public T SingleOrDefault()
        {
            return Enumerable.SingleOrDefault(_connection.List(this));
        }

        public String ToSqlString()
        {
            StringBuilder sbSql = StringHelper.CreateBuilder()
                 .Append("select * from ")
                  .Append(_table.GetQualifiedName(_factory.Dialect, _factory.Mapping.Catalog, _factory.Mapping.Schema));
            GenerateFragment(sbSql, "WHERE", _conditions, " AND ");
            return sbSql.ToString();
        }

        private void GenerateFragment(StringBuilder sb, String prefix, IList<IExpression> exps, String sep)
        {
            if (exps.Count > 0)
            {
                sb.Append(" ").Append(prefix).Append(" ");
                StringHelper.AppendItemsWithSeperator(exps, sep, delegate(IExpression exp)
                {
                    sb.Append(exp.ToSqlString(this));
                }, sb);
            }
        }

        public string ToSqlString(BetweenExpression between)
        {
            return StringHelper.CreateBuilder()
                .Append(between.Expression)
                .Append(" between ")
                .Append(between.Lower.ToSqlString(this))
                .Append(" and ")
                .Append(between.Upper.ToSqlString(this))
                .ToString();
        }

        public string ToSqlString(LikeExpression like)
        {
            StringBuilder sb = StringHelper.CreateBuilder();

            if (like.IgnoreCase)
                sb.Append(_factory.Dialect.LowercaseFunction)
                    .Append('(').Append(like.Expression.ToSqlString(this)).Append(')');
            else
                sb.Append(like.Expression.ToSqlString(this));

            sb.Append(" like ")
                .Append(like.MatchMode.ToMatchString(like.Value.ToSqlString(this)));

            if (like.EscapeChar != null)
                sb.Append(" escape \'").Append(like.EscapeChar).Append("\'");

            return sb.ToString();
        }

        public string ToSqlString(IlikeExpression ilike)
        {
            StringBuilder sb = StringHelper.CreateBuilder();

            if (_factory.Dialect is PostgreSQLDialect)
                sb.Append(ilike.Expression.ToSqlString(this))
                    .Append(" ilike ");
            else
                sb.Append(_factory.Dialect.LowercaseFunction)
                    .Append('(').Append(ilike.Expression.ToSqlString(this)).Append(')')
                    .Append(" like ");

            return sb.Append(ilike.MatchMode.ToMatchString(ilike.Value.ToSqlString(this))).ToString();
        }

        public string ToSqlString(InExpression inexp)
        {
            StringBuilder sb = StringHelper.CreateBuilder()
                .Append(inexp.Expression.ToSqlString(this))
                .Append(" in (");

            StringHelper.AppendItemsWithComma(inexp.Values, delegate(IExpression exp)
            {
                sb.Append(exp.ToSqlString(this));
            }, sb);

            return sb.Append(")").ToString();
        }

        public string ToSqlString(Junction junction)
        {
            if (0 == junction.Expressions.Count)
                return "1=1";

            StringBuilder sb = StringHelper.CreateBuilder().Append("(");

            StringHelper.AppendItemsWithSeperator(junction.Expressions, ' ' + junction.Op + ' ',
                delegate(IExpression exp)
                {
                    sb.Append(exp.ToSqlString(this));
                }, sb);

            return sb.Append(')').ToString();
        }

        public string ToSqlString(LogicalExpression logical)
        {
            return StringHelper.CreateBuilder()
                .Append('(')
                .Append(logical.Left.ToSqlString(this))
                .Append(' ')
                .Append(logical.Op)
                .Append(' ')
                .Append(logical.Right.ToSqlString(this))
                .Append(')')
                .ToString();
        }

        public string ToSqlString(NotExpression not)
        {
            if (_factory.Dialect is MySQLDialect)
                return "not (" + not.Expression.ToSqlString(this) + ')';
            else
                return "not " + not.Expression.ToSqlString(this);
        }

        public string ToSqlString(NotNullExpression notNull)
        {
            return notNull.Expression.ToSqlString(this) + " is not null";
        }

        public string ToSqlString(NullExpression nullexp)
        {
            return nullexp.Expression.ToSqlString(this) + " is null";
        }

        public String ToSqlString(PlainExpression plain)
        {
            return plain.ToString();
        }

        public String ToSqlString(ValueExpression value)
        {
            if (Parameterized)
                return RegisterParam(value.Value);
            else
                return value.ToString();
        }

        public String ToSqlString(FieldExpression field)
        {
            Mapping.Column column = _table.FindColumn(field.Filed);
            if (column == null)
                return field.ToString();
            else
                return column.GetQuotedName(_factory.Dialect);
        }

        public String ToSqlString(Order order)
        {
            return StringHelper.CreateBuilder()
                .Append(order.Expression.ToSqlString(this))
                .Append((order.Ascending ? " asc" : " desc"))
                .ToString();
        }

        public string ToSqlString(From.Table table)
        {
            throw new NotImplementedException();
        }

        public string ToSqlString(Function function)
        {
            throw new NotImplementedException();
        }

        public string ToSqlString(Select select)
        {
            throw new NotImplementedException();
        }

        public string ToSqlString(SimpleExpression simple)
        {
            return StringHelper.CreateBuilder()
                .Append('(')
                .Append(simple.Left.ToSqlString(this))
                .Append(' ')
                .Append(simple.Op)
                .Append(' ')
                .Append(simple.Right.ToSqlString(this))
                .Append(')')
                .ToString();
        }

        public string ToSqlString(PropertyExpression property)
        {
            return StringHelper.CreateBuilder()
                .Append('(')
                .Append(property.PropertyName.ToSqlString(this))
                .Append(' ')
                .Append(property.Op)
                .Append(' ')
                .Append(property.OtherPropertyName.ToSqlString(this))
                .Append(')')
                .ToString();
        }
    }

    public interface ICriteriaRender
    {
        String ToSqlString(BetweenExpression between);
        String ToSqlString(LikeExpression like);
        String ToSqlString(IlikeExpression ilike);
        String ToSqlString(InExpression inexp);
        String ToSqlString(Junction junction);
        String ToSqlString(LogicalExpression logicalExpression);
        String ToSqlString(NotExpression notExpression);
        String ToSqlString(NotNullExpression notNullExpression);
        String ToSqlString(NullExpression nullExpression);
        String ToSqlString(PlainExpression plainExpression);
        String ToSqlString(ValueExpression valueExpression);
        String ToSqlString(FieldExpression fieldExpression);
        String ToSqlString(Order order);
        String ToSqlString(From.Table table);
        String ToSqlString(Function function);
        String ToSqlString(Select select);
        String ToSqlString(SimpleExpression simpleExpression);
        String ToSqlString(PropertyExpression propertyExpression);
    }
}
