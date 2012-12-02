using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using LX.EasyDb.Dialects;
using LX.EasyDb.Dialects.Function;

namespace LX.EasyDb.Criterion
{
    class Criteria : ICriteria, ICriteriaRender
    {
        protected IConnection _connection;
        private IConnectionFactorySupport _factory;
        private List<IExpression> _conditions = new List<IExpression>();
        private List<Select> _selects;
        private List<Order> _orders = new List<Order>();
        private Dictionary<String, Object> _params = new Dictionary<String, Object>();
        private Mapping.Table _table;

        public Int32 Offset { get; set; }
        public Int32 Total { get; set; }
        public Type Type { get; private set; }

        public Criteria(Type type, IConnection connection, IConnectionFactorySupport factory)
        {
            _table = factory.Mapping.FindTable(type);
            _connection = connection;
            _factory = factory;
            Parameterized = true;
            Total = -1;
            Offset = 0;
            Type = type;
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

        public ICriteria Add(IExpression condition)
        {
            _conditions.Add(condition);
            return this;
        }

        public ICriteria AddSelect(Select select)
        {
            if (_selects == null)
                _selects = new List<Select>();
            _selects.Add(select);
            return this;
        }

        public ICriteria AddOrder(Order order)
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

        public IEnumerable List()
        {
            return List(-1, 0);
        }

        public IEnumerable List(Int32 total = -1, Int32 offset = 0)
        {
            Total = total;
            Offset = offset;
            return Enumerable.ToList(_connection.Query(Type, ToSqlString(), Parameters));
        }

        public Int32 Count()
        {
            return Enumerable.Single<Int32>(_connection.Query<Int32>(ToSqlCountString(), Parameters));
        }

        public Object SingleOrDefault()
        {
            return Enumerable.SingleOrDefault(List(1));
        }

        public String ToSqlString()
        {
            String orderby = GenerateOrder();
            String sql = GenerateSelect(orderby);
            if (Total >= 0)
                sql = _factory.Dialect.GetPaging(sql, orderby, Total, Offset);
#if DEBUG
            Console.WriteLine(sql);
#endif
            return sql;
        }

        public String ToSqlCountString()
        {
            String select = GenerateSelect(null);
            StringBuilder sbSql = StringHelper.CreateBuilder()
                 .Append("SELECT COUNT(*) FROM (")
                 .Append(select)
                 .Append(") t");
            return sbSql.ToString();
        }

        private String GenerateSelect(String order)
        {
            StringBuilder sbSql = StringHelper.CreateBuilder();

            if (_selects == null)
            {
                sbSql.Append(_table.ToSqlSelect(_factory.Dialect, _factory.Mapping.Catalog, _factory.Mapping.Schema, false));
            }
            else
            {
                sbSql.Append("SELECT ");
                Boolean appendSeperator = false;
                foreach (Select select in _selects)
                {
                    if (appendSeperator)
                        sbSql.Append(",");
                    else
                        appendSeperator = true;
                    sbSql.Append(select.ToSqlString(this));
                }
                sbSql.Append(" FROM ")
                    .Append(_table.GetQualifiedName(_factory.Dialect, _factory.Mapping.Catalog, _factory.Mapping.Schema));
            }

            GenerateFragment(sbSql, "WHERE", _conditions, " AND ");
            if (order != null)
                sbSql.Append(order);

            return sbSql.ToString();
        }

        private String GenerateOrder()
        {
            if (_orders.Count > 0)
            {
                StringBuilder sb = StringHelper.CreateBuilder()
                    .Append(" ORDER BY ");
                StringHelper.AppendItemsWithSeperator(_orders, ",", delegate(Order order)
                {
                    sb.Append(order.ToSqlString(this));
                }, sb);
                return sb.ToString();
            }
            else
                return null;
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

        public String ToSqlString(BetweenExpression between)
        {
            return StringHelper.CreateBuilder()
                .Append(between.Expression.ToSqlString(this))
                .Append(" between ")
                .Append(between.Lower.ToSqlString(this))
                .Append(" and ")
                .Append(between.Upper.ToSqlString(this))
                .ToString();
        }

        public String ToSqlString(LikeExpression like)
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

        public String ToSqlString(IlikeExpression ilike)
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

        public String ToSqlString(InExpression inexp)
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

        public String ToSqlString(Junction junction)
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

        public String ToSqlString(LogicalExpression logical)
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

        public String ToSqlString(NotExpression not)
        {
            if (_factory.Dialect is MySQLDialect)
                return "not (" + not.Expression.ToSqlString(this) + ')';
            else
                return "not " + not.Expression.ToSqlString(this);
        }

        public String ToSqlString(NotNullExpression notNull)
        {
            return notNull.Expression.ToSqlString(this) + " is not null";
        }

        public String ToSqlString(NullExpression nullexp)
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
                .Append((order.Ascending ? " ASC" : " DESC"))
                .ToString();
        }

        public String ToSqlString(From.Table table)
        {
            throw new NotImplementedException();
        }

        public String ToSqlString(Function function)
        {
            ISQLFunction func = _factory.Dialect.FindFunction(function.FunctionName);
            if (func == null)
                // TODO throw an exception
                throw new Exception("Function not found");
            List<Object> list = new List<Object>();
            foreach (IExpression exp in function.Arguments)
            {
                list.Add(exp.ToSqlString(this));
            }
            return func.Render(list, _factory as IConnectionFactory);
        }

        public String ToSqlString(Select select)
        {
            StringBuilder sb = StringHelper.CreateBuilder();
            if (select.Distinct)
                sb.Append("DISTINCT ");
            sb.Append(select.Expression.ToSqlString(this));
            if (!String.IsNullOrEmpty(select.Alias))
            {
                sb.Append(" AS ");
                sb.Append(_factory.Dialect.Quote(select.Alias));
            }
            return sb.ToString();
        }

        public String ToSqlString(SimpleExpression simple)
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

        public String ToSqlString(PropertyExpression property)
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

        public String ToSqlString(AggregateProjection aggregateProjection)
        {
            ISQLFunction func = _factory.Dialect.FindFunction(aggregateProjection.FunctionName);
            if (func == null)
                // TODO throw an exception
                throw new Exception("Function not found");
            return func.Render(BuildFunctionParameterList(aggregateProjection.FiledName), _factory as IConnectionFactory);
        }

        private static IList<Object> BuildFunctionParameterList(String column)
        {
            List<Object> list = new List<Object>();
            list.Add(column);
            return list;
        }
    }

    class Criteria<T> : Criteria, ICriteria<T>, ICriteriaRender
    {
        public Criteria(IConnection connection, IConnectionFactorySupport factory)
            : base(typeof(T), connection, factory)
        {
        }

        public new ICriteria<T> Add(IExpression condition)
        {
            base.Add(condition);
            return this;
        }

        public new ICriteria<T> AddSelect(Select select)
        {
            base.AddSelect(select);
            return this;
        }

        public new ICriteria<T> AddOrder(Order order)
        {
            base.AddOrder(order);
            return this;
        }

        public new IEnumerable<T> List()
        {
            return List(-1, 0);
        }

        public new IEnumerable<T> List(Int32 total = -1, Int32 offset = 0)
        {
            Total = total;
            Offset = offset;
            return Enumerable.ToList(_connection.Query<T>(ToSqlString(), Parameters));
        }

        public new T SingleOrDefault()
        {
            return Enumerable.SingleOrDefault(List(1));
        }
    }

    interface ICriteriaRender
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
        String ToSqlString(AggregateProjection aggregateProjection);
    }
}
