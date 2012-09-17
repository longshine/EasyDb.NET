//
// LX.EasyDb.Mapping.cs
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
using System.Reflection;
using System.Text;
using Dapper;

namespace LX.EasyDb
{
    /// <summary>
    /// Provides classes and method for ORM.
    /// </summary>
    public class Mapping
    {
        private IDictionary<String, Table> _tables = new Dictionary<String, Table>();
        private INamingStrategy _namingStrategy = DefaultNamingStrategy.Instance;

        /// <summary>
        /// Gets or sets the currently bound default catalog name.
        /// </summary>
        public String Catalog { get; set; }

        /// <summary>
        /// Gets or sets the currently bound default schema name
        /// </summary>
        public String Schema { get; set; }

        /// <summary>
        /// Finds a table mapped with the specified entity.
        /// </summary>
        /// <param name="entity">the name of the entity to map</param>
        /// <returns>a mapped <see cref="LX.EasyDb.Mapping.Table"/></returns>
        public Table FindTable(String entity)
        {
            return _tables.ContainsKey(entity) ? _tables[entity] : null;
        }

        /// <summary>
        /// Finds a table mapped with the specified type.
        /// </summary>
        /// <param name="type">the <see cref="System.Type"/> to map</param>
        /// <returns>a mapped <see cref="LX.EasyDb.Mapping.Table"/></returns>
        public Table FindTable(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            Table table = FindTable(GetTypeName(type));
            if (table == null)
            {
                lock (_tables)
                {
                    table = FindTable(GetTypeName(type));
                    if (table == null)
                    {
                        table = new Table(type, _namingStrategy);
                        _tables[GetTypeName(type)] = table;
                    }
                }
            }
            return table;
        }

        /// <summary>
        /// Sets a table mapped with the specified type.
        /// </summary>
        /// <param name="type">the <see cref="System.Type"/> to map</param>
        /// <param name="table">the mapping rules impementation, or null to remove custom map</param>
        public void SetTable(Type type, Table table)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (table == null)
            {
                lock (_tables)
                {
                    _tables.Remove(GetTypeName(type));
                }
            }
            else
            {
                lock (_tables)
                {
                    _tables[GetTypeName(type)] = table;
                }
            }
        }

        private String GetTypeName(Type type)
        {
            return type.FullName;
        }

        /// <summary>
        /// Determines the physical column and table names given the information of a type.
        /// </summary>
        public interface INamingStrategy
        {
            /// <summary>
            /// Returns a column name for a property.
            /// </summary>
            /// <param name="propertyName">the property's name</param>
            /// <returns>a column name</returns>
            String GetColumnName(String propertyName);
            /// <summary>
            /// Returns a table name for an entity type.
            /// </summary>
            /// <param name="typeName">the full-qualified type name</param>
            /// <returns>a table name</returns>
            String GetTableName(String typeName);
        }

        class DefaultNamingStrategy : INamingStrategy
        {
            public static DefaultNamingStrategy Instance = new DefaultNamingStrategy();

            private DefaultNamingStrategy() { }

            public String GetColumnName(String propertyName)
            {
                return StringHelper.Unqualify(propertyName);
            }

            public String GetTableName(String typeName)
            {
                return StringHelper.Unqualify(typeName);
            }
        }

        /// <summary>
        /// Represents a relational model.
        /// </summary>
        public interface IRelationalModel
        {
            /// <summary>
            /// Generates SQL for creating this model.
            /// </summary>
            /// <param name="dialect"></param>
            /// <param name="defaultCatalog">the default catalog name</param>
            /// <param name="defaultSchema">the default schema name</param>
            /// <returns>an SQL string</returns>
            String ToSqlCreate(Dialect dialect, String defaultCatalog, String defaultSchema);
            /// <summary>
            /// Generates SQL for creating this model.
            /// </summary>
            /// <param name="dialect"></param>
            /// <param name="defaultCatalog">the default catalog name</param>
            /// <param name="defaultSchema">the default schema name</param>
            /// <returns>an SQL string</returns>
            String ToSqlDrop(Dialect dialect, String defaultCatalog, String defaultSchema);
        }

        /// <summary>
        /// A relational table.
        /// </summary>
        public class Table : IRelationalModel, SqlMapper.ITypeMap
        {
            private String _name;
            private String _schema;
            private PrimaryKey _primaryKey;
            private IDictionary<String, Column> _columns = new Dictionary<String, Column>();
            private IDictionary<String, Index> _indices = new Dictionary<String, Index>();
            private IDictionary<String, UniqueKey> _uniqueKeys = new Dictionary<String, UniqueKey>();
            private List<String> _checkConstraints = new List<String>();
            private readonly IEnumerable<FieldInfo> _fields;
            private readonly IEnumerable<PropertyInfo> _properties;

            /// <summary>
            /// Initializes a mapping table.
            /// </summary>
            /// <param name="type">the type to map</param>
            /// <param name="namingStrategy">the naming strategy to apply</param>
            public Table(Type type, INamingStrategy namingStrategy)
            {
                String typeName = type.FullName;
                TableAttribute tableAttr = ReflectHelper.GetAttribute<TableAttribute>(type);
                Object[] objs = type.GetCustomAttributes(typeof(TableAttribute), false);

                Name = (tableAttr == null) ? namingStrategy.GetTableName(typeName) : tableAttr.Name;
                Type = type;
                _fields = ReflectHelper.GetSettableFields(type);
                _properties = ReflectHelper.GetSettableProperties(type);

                foreach (PropertyInfo pi in _properties)
                {
                    Column column = CreateColumn(pi, namingStrategy);
                    if (column.Type == DbType.Empty)
                        column.Type = (DbType)SqlMapper.LookupDbType(pi.PropertyType, pi.Name);
                    column.MemberInfo = new SimpleMemberMap(column.ColumnName, pi);
                }

                foreach (FieldInfo fi in _fields)
                {
                    Column column = CreateColumn(fi, namingStrategy);
                    if (column.Type == DbType.Empty)
                        column.Type = (DbType)SqlMapper.LookupDbType(fi.FieldType, fi.Name);
                    column.MemberInfo = new SimpleMemberMap(column.ColumnName, fi);
                }

                if (PrimaryKey == null)
                {
                    Column idCol = FindColumn("id");
                    if (idCol != null)
                    {
                        PrimaryKey = new PrimaryKey();
                        PrimaryKey.AddColumn(idCol);
                    }
                }
            }

            private Column CreateColumn(MemberInfo mi, INamingStrategy namingStrategy)
            {
                ColumnAttribute colAttr = ReflectHelper.GetAttribute<ColumnAttribute>(mi);
                Column column = new Column();

                column.ColumnName = (colAttr == null || String.IsNullOrEmpty(colAttr.Name)) ? namingStrategy.GetColumnName(mi.Name) : colAttr.Name;
                if (colAttr != null && colAttr.Type != DbType.Empty)
                    column.Type = colAttr.Type;
                AddColumn(column);

                if (ReflectHelper.HasAttribute<PrimaryKeyAttribute>(mi))
                {
                    if (PrimaryKey == null)
                        PrimaryKey = new PrimaryKey();
                    PrimaryKey.AddColumn(column);
                }

                return column;
            }

            /// <summary>
            /// Gets or sets the name of this table.
            /// </summary>
            public String Name
            {
                get { return _name; }
                set { Dialect.UnQuote(value, out _name); }
            }

            /// <summary>
            /// Gets or sets the schema of this table.
            /// </summary>
            public String Schema
            {
                get { return _schema; }
                set { Dialect.UnQuote(value, out _schema); }
            }

            /// <summary>
            /// Gets or sets the catalog of this table.
            /// </summary>
            public String Catalog { get; set; }

            /// <summary>
            /// Gets or sets the comment of this table.
            /// </summary>
            public String Comment { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="System.Type"/> mapped with this table.
            /// </summary>
            public Type Type { get; private set; }

            /// <summary>
            /// Gets or sets the primary key.
            /// </summary>
            public PrimaryKey PrimaryKey
            {
                get { return _primaryKey; }
                set { _primaryKey = value; }
            }

            /// <summary>
            /// Gets if this table has a primary key.
            /// </summary>
            public Boolean HasPrimaryKey
            {
                get { return PrimaryKey != null; }
            }

            /// <summary>
            /// Gets the Identity (auto increment) column of this table if has one.
            /// </summary>
            public Column IdColumn { get; private set; }

            /// <summary>
            /// Gets columns in this table.
            /// </summary>
            public IEnumerable<Column> Columns
            {
                get { return _columns.Values; }
            }

            /// <summary>
            /// Gets the count of columns in this table.
            /// </summary>
            public Int32 ColumnSpan
            {
                get { return _columns.Count; }
            }

            /// <summary>
            /// Adds a column.
            /// </summary>
            /// <param name="column">the column to add</param>
            public void AddColumn(Column column)
            {
                _columns[column.CanonicalName] = column;
                if (column.Type == DbType.Identity)
                    IdColumn = column;
            }

            /// <summary>
            /// Finds a column by its name.
            /// </summary>
            /// <param name="columnName">the name of the column to find</param>
            public Column FindColumn(String columnName)
            {
                columnName = columnName.ToLower();
                return _columns.ContainsKey(columnName) ? _columns[columnName] : null;
            }

            /// <summary>
            /// Gets quoted name.
            /// </summary>
            public String GetQuotedName(Dialect dialect)
            {
                return dialect.OpenQuote + Name + dialect.CloseQuote;
            }

            /// <summary>
            /// Gets quoted schema.
            /// </summary>
            public String GetQuotedSchema(Dialect dialect)
            {
                return dialect.OpenQuote + Schema + dialect.CloseQuote;
            }

            /// <summary>
            /// Gets qualified name of thia table.
            /// </summary>
            /// <param name="dialect"></param>
            /// <param name="defaultCatalog">the default catalog name</param>
            /// <param name="defaultSchema">the default schema name</param>
            public String GetQualifiedName(Dialect dialect, String defaultCatalog, String defaultSchema)
            {
                String quotedName = GetQuotedName(dialect);
                String usedSchema = Schema == null ? defaultSchema : GetQuotedSchema(dialect);
                String usedCatalog = Catalog == null ? defaultCatalog : Catalog;
                return Qualify(usedCatalog, usedSchema, quotedName);
            }

            /// <summary>
            /// Qualifies a table's name.
            /// </summary>
            /// <param name="catalog">the catalog name</param>
            /// <param name="schema">the schema name</param>
            /// <param name="table">the table name</param>
            /// <returns></returns>
            public static String Qualify(String catalog, String schema, String table)
            {
                StringBuilder sb = StringHelper.CreateBuilder();
                if (!String.IsNullOrEmpty(catalog))
                    sb.Append(catalog).Append('.');
                if (!String.IsNullOrEmpty(schema))
                    sb.Append(schema).Append('.');
                return sb.Append(table).ToString();
            }

            /// <summary>
            /// Generates SQL for creating this table.
            /// </summary>
            /// <param name="dialect"></param>
            /// <param name="defaultCatalog">the default catalog name</param>
            /// <param name="defaultSchema">the default schema name</param>
            /// <returns>an SQL string</returns>
            public String ToSqlCreate(Dialect dialect, String defaultCatalog, String defaultSchema)
            {
                StringBuilder sb = StringHelper.CreateBuilder().Append(HasPrimaryKey ? dialect.CreateTableString : dialect.CreateMultisetTableString)
                .Append(" ")
                .Append(GetQualifiedName(dialect, defaultCatalog, defaultSchema))
                .Append(" (");

                StringHelper.AppendItemsWithComma(_columns.Values, delegate(Column column)
                {
                    // column info
                    sb.Append(column.GetQuotedName(dialect))
                        .Append(" ");

                    sb.Append(column.GetSqlType(dialect));

                    if (column.DefaultValue != null)
                        sb.Append(" default ").Append(column.DefaultValue);

                    if (column.Nullable)
                        sb.Append(dialect.NullColumnString);
                    else
                        sb.Append(" not null");

                    // unique constraint
                    if (column.Unique &&
                        (!column.Nullable || dialect.SupportsNotNullUnique))
                    {
                        if (dialect.SupportsUnique)
                            sb.Append(" unique");
                        else
                        {
                            UniqueKey uk = GetOrCreateUniqueKey(column.GetQuotedName(dialect) + "_");
                            uk.AddColumn(column);
                        }
                    }

                    // check constraint
                    if (column.CheckConstraint != null && dialect.SupportsColumnCheck)
                        sb.Append(" check (")
                            .Append(column.CheckConstraint)
                            .Append(")");

                    // comment
                    if (column.Comment != null)
                        sb.Append(dialect.GetColumnComment(column.Comment));
                }, sb);

                if (HasPrimaryKey)
                    sb.Append(", ")
                        .Append(PrimaryKey.ToSqlConstraintString(dialect));

                if (dialect.SupportsUniqueConstraintInCreateAlterTable)
                {
                    foreach (UniqueKey uk in _uniqueKeys.Values)
                    {
                        String constraint = uk.ToSqlConstraintString(dialect);
                        if (!String.IsNullOrEmpty(constraint))
                            sb.Append(", ").Append(constraint);
                    }
                }

                // table check
                if (dialect.SupportsTableCheck)
                {
                    foreach (String check in _checkConstraints)
                    {
                        sb.Append(", check (").Append(check).Append(")");
                    }
                }

                sb.Append(")");

                if (Comment != null)
                {
                    sb.Append(dialect.GetTableComment(Comment));
                }

                return sb.ToString();
            }

            /// <summary>
            /// Generates SQL for droping this table.
            /// </summary>
            /// <param name="dialect"></param>
            /// <param name="defaultCatalog">the default catalog name</param>
            /// <param name="defaultSchema">the default schema name</param>
            /// <returns>an SQL string</returns>
            public String ToSqlDrop(Dialect dialect, String defaultCatalog, String defaultSchema)
            {
                StringBuilder sb = StringHelper.CreateBuilder().Append("drop table ");
                if (dialect.SupportsIfExistsBeforeTableName)
                    sb.Append("if exists ");
                sb.Append(GetQualifiedName(dialect, defaultCatalog, defaultSchema))
                    .Append(dialect.CascadeConstraintsString);
                if (dialect.SupportsIfExistsAfterTableName)
                    sb.Append(" if exists");
                return sb.ToString();
            }

            /// <summary>
            /// Generates SQL for inserting records into this table.
            /// </summary>
            /// <param name="dialect"></param>
            /// <param name="defaultCatalog">the default catalog name</param>
            /// <param name="defaultSchema">the default schema name</param>
            /// <returns>an SQL string</returns>
            public String ToSqlInsert(Dialect dialect, String defaultCatalog, String defaultSchema)
            {
                StringBuilder sbSql = StringHelper.CreateBuilder();
                StringBuilder sbParameters = StringHelper.CreateBuilder();

                sbSql.Append("insert into ").Append(GetQualifiedName(dialect, defaultCatalog, defaultSchema)).Append(" (");

                StringHelper.AppendItemsWithSeperator(Columns, ", ", delegate(Column column)
                {
                    if (column.Type == DbType.Identity)
                        return false;
                    sbSql.Append(column.GetQuotedName(dialect));
                    sbParameters.Append(dialect.ParamPrefix).Append(column.FieldName);
                    return true;
                }, sbSql, sbParameters);

                sbSql.Append(") values (");
                sbSql.Append(sbParameters);
                sbSql.Append(")");

                return sbSql.ToString();
            }

            /// <summary>
            /// Generates SQL for updating records in this table.
            /// </summary>
            /// <param name="dialect"></param>
            /// <param name="defaultCatalog">the default catalog name</param>
            /// <param name="defaultSchema">the default schema name</param>
            /// <returns>an SQL string</returns>
            public String ToSqlUpdate(Dialect dialect, String defaultCatalog, String defaultSchema)
            {
                StringBuilder sbSql = StringHelper.CreateBuilder()
                    .Append("update ").Append(GetQualifiedName(dialect, defaultCatalog, defaultSchema))
                    .Append(" set ");
                StringHelper.AppendItemsWithSeperator(Columns, ", ", delegate(Column column)
                {
                    if (HasPrimaryKey && PrimaryKey.ContainsColumn(column))
                        return false;
                    sbSql.Append(column.GetQuotedName(dialect))
                        .Append(" = ").Append(dialect.ParamPrefix)
                        .Append(column.FieldName);
                    return true;
                }, sbSql);
                if (HasPrimaryKey)
                {
                    sbSql.Append(" where ");
                    StringHelper.AppendItemsWithSeperator(PrimaryKey.Columns, " and ", delegate(Column column)
                    {
                        sbSql.Append(column.GetQuotedName(dialect))
                            .Append(" = ").Append(dialect.ParamPrefix)
                            .Append(column.FieldName);
                    }, sbSql);
                }
                return sbSql.ToString();
            }

            /// <summary>
            /// Generates SQL for deleting records in this table.
            /// </summary>
            /// <param name="dialect"></param>
            /// <param name="defaultCatalog">the default catalog name</param>
            /// <param name="defaultSchema">the default schema name</param>
            /// <returns>an SQL string</returns>
            public String ToSqlDelete(Dialect dialect, String defaultCatalog, String defaultSchema)
            {
                StringBuilder sbSql = StringHelper.CreateBuilder()
                    .Append("delete from ").Append(GetQualifiedName(dialect, defaultCatalog, defaultSchema));
                if (HasPrimaryKey)
                {
                    sbSql.Append(" where ");
                    StringHelper.AppendItemsWithSeperator(PrimaryKey.Columns, " and ", delegate(Column column)
                    {
                        sbSql.Append(column.GetQuotedName(dialect))
                            .Append(" = ").Append(dialect.ParamPrefix)
                            .Append(column.FieldName);
                    }, sbSql);
                }
                return sbSql.ToString();
            }

            /// <summary>
            /// Gets a unique key by its name, or creates it if not found.
            /// </summary>
            public UniqueKey GetOrCreateUniqueKey(String keyName)
            {
                UniqueKey uk = null;
                if (_uniqueKeys.ContainsKey(keyName))
                    uk = _uniqueKeys[keyName];
                else
                {
                    uk = new UniqueKey();
                    uk.Name = keyName;
                    uk.Table = this;
                    _uniqueKeys.Add(keyName, uk);
                }
                return uk;
            }

            /// <summary>
            /// Adds a check constraint.
            /// </summary>
            /// <param name="constraint"></param>
            public void AddCheckConstraint(String constraint)
            {
                _checkConstraints.Add(constraint);
            }

            /// <summary>
            /// Gets member mapping for column.
            /// </summary>
            /// <param name="columnName">the column name</param>
            /// <returns>the mapping implementation</returns>
            public SqlMapper.IMemberMap GetMember(String columnName)
            {
                Column column = FindColumn(columnName);
                return column == null ? null : column.MemberInfo;
            }

            /// <summary>
            /// Finds best constructor.
            /// </summary>
            /// <param name="names">the column names</param>
            /// <param name="types">the column types</param>
            /// <returns>the matching constructor or default one</returns>
            public ConstructorInfo FindConstructor(String[] names, Type[] types)
            {
                return ReflectHelper.FindConstructor(Type, names, types);
            }

            /// <summary>
            /// Gets mapping for constructor parameter.
            /// </summary>
            /// <param name="constructor">the constructor to resolve</param>
            /// <param name="columnName">the column name</param>
            /// <returns>the mapping implementation</returns>
            public SqlMapper.IMemberMap GetConstructorParameter(ConstructorInfo constructor, String columnName)
            {
                var parameters = constructor.GetParameters();
                
                return new SimpleMemberMap(columnName, Array.Find(parameters,  delegate(ParameterInfo p) { return String.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase); }));
            }

            /// <summary>
            /// Gets the field name mapped with the given column.
            /// </summary>
            /// <param name="columnName">the column name</param>
            /// <returns>the mapped field name</returns>
            public String GetFieldName(String columnName)
            {
                Column column = FindColumn(columnName);
                return column == null ? columnName : column.FieldName;
            }
        }

        /// <summary>
        /// A column of a relational database table
        /// </summary>
        public class Column
        {
            const Int32 DEFAULT_LENGTH = 255;
            const Int32 DEFAULT_PRECISION = 19;
            const Int32 DEFAULT_SCALE = 2;

            private String _name;
            private String _fieldName;

            /// <summary>
            /// Initializes.
            /// </summary>
            public Column()
            {
                Type = DbType.Empty;
                Nullable = true;
                Length = DEFAULT_LENGTH;
                Precision = DEFAULT_PRECISION;
                Scale = DEFAULT_SCALE;
            }

            /// <summary>
            /// Gets or sets the name of this column.
            /// </summary>
            public String ColumnName
            {
                get { return _name; }
                set { Dialect.UnQuote(value, out _name); }
            }

            internal String CanonicalName
            {
                get { return ColumnName.ToLower(); }
            }

            /// <summary>
            /// Gets the quoted name.
            /// </summary>
            public String GetQuotedName(Dialect dialect)
            {
                return (dialect.OpenQuote + ColumnName + dialect.CloseQuote);
            }
            
            /// <summary>
            /// Gets or sets the type of this column.
            /// </summary>
            public DbType Type { get; set; }

            /// <summary>
            /// Gets or sets the member info associated with this column.
            /// </summary>
            public SqlMapper.IMemberMap MemberInfo { get; set; }

            /// <summary>
            /// Gets or sets the name of the mapped field.
            /// </summary>
            public String FieldName
            {
                get
                {
                    if (MemberInfo != null)
                    {
                        if (MemberInfo.Property != null)
                            return MemberInfo.Property.Name;
                        else if (MemberInfo.Field != null)
                            return MemberInfo.Field.Name;
                    }
                    return _fieldName;
                }
                set { _fieldName = value; }
            }

            /// <summary>
            /// Gets or sets the SQL type of this column.
            /// </summary>
            public String SqlType { get; set; }

            /// <summary>
            /// Gets or sets if this column has a UNIQUE constraint.
            /// </summary>
            public Boolean Unique { get; set; }

            /// <summary>
            /// Gets or sets if this column has a NOT NULL constraint.
            /// </summary>
            public Boolean Nullable { get; set; }

            /// <summary>
            /// Gets or sets the comment of this column.
            /// </summary>
            public String Comment { get; set; }

            /// <summary>
            /// Gets or sets the default value of this column.
            /// </summary>
            public String DefaultValue { get; set; }

            /// <summary>
            /// Gets or sets the check constraint of this column.
            /// </summary>
            public String CheckConstraint { get; set; }

            /// <summary>
            /// Gets or sets the length of this column.
            /// </summary>
            public Int32 Length { get; set; }

            /// <summary>
            /// Gets or sets the precision of this column.
            /// </summary>
            public Int32 Precision { get; set; }

            /// <summary>
            /// Gets or sets the scale of this column.
            /// </summary>
            public Int32 Scale { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public Boolean Equals(Column other)
            {
                if (other == null)
                    return false;
                else if (other == this)
                    return true;
                else
                    return String.Equals(ColumnName, other.ColumnName, StringComparison.OrdinalIgnoreCase);
            }

            /// <summary>
            /// 
            /// </summary>
            public override Boolean Equals(Object obj)
            {
                return obj is Column && Equals((Column)obj);
            }

            /// <summary>
            /// 
            /// </summary>
            public override Int32 GetHashCode()
            {
                return ColumnName.ToLower().GetHashCode();
            }

            /// <summary>
            /// 
            /// </summary>
            public override String ToString()
            {
                return GetType().Name + "(" + ColumnName + ")";
            }

            /// <summary>
            /// Gets the SQL type of this column.
            /// Returned type will be the SqlType, or be looked up by the Type property if SqlType is not set.
            /// </summary>
            public String GetSqlType(Dialect dialect)
            {
                return SqlType == null ? dialect.GetTypeName(Type, Length, Precision, Scale) : SqlType;
            }
        }

        #region Relational objects

        /// <summary>
        /// Base class of relational models.
        /// </summary>
        public abstract class RelationalModel : IRelationalModel
        {
            private List<Column> _columns = new List<Column>();

            /// <summary>
            /// Gets associated columns.
            /// </summary>
            public IEnumerable<Column> Columns
            {
                get { return _columns; }
            }

            /// <summary>
            /// Gets the count of associated columns.
            /// </summary>
            public Int32 ColumnSpan
            {
                get { return _columns.Count; }
            }

            /// <summary>
            /// Adds an associated column.
            /// </summary>
            /// <param name="column"></param>
            public void AddColumn(Column column)
            {
                if (!_columns.Contains(column))
                    _columns.Add(column);
            }

            /// <summary>
            /// Adds associated columns.
            /// </summary>
            /// <param name="columns"></param>
            public void AddColumns(IEnumerable<Column> columns)
            {
                foreach (var column in columns)
                {
                    AddColumn(column);
                }
            }

            /// <summary>
            /// Checks if a column is associated with this model.
            /// </summary>
            /// <param name="column"></param>
            /// <returns></returns>
            public Boolean ContainsColumn(Column column)
            {
                return _columns.Contains(column);
            }

            /// <summary>
            /// Generates SQL for creating this model.
            /// </summary>
            /// <param name="dialect"></param>
            /// <param name="defaultCatalog">the default catalog name</param>
            /// <param name="defaultSchema">the default schema name</param>
            /// <returns>an SQL string</returns>
            public String ToSqlCreate(Dialect dialect, String defaultCatalog, String defaultSchema)
            {
                return DoToSqlCreate(dialect, defaultCatalog, defaultSchema);
            }

            /// <summary>
            /// Generates SQL for creating this model.
            /// </summary>
            /// <param name="dialect"></param>
            /// <param name="defaultCatalog">the default catalog name</param>
            /// <param name="defaultSchema">the default schema name</param>
            /// <returns>an SQL string</returns>
            public String ToSqlDrop(Dialect dialect, String defaultCatalog, String defaultSchema)
            {
                return DoToSqlDrop(dialect, defaultCatalog, defaultSchema);
            }

            /// <summary>
            /// Generates SQL for creating this model.
            /// </summary>
            /// <param name="dialect"></param>
            /// <param name="defaultCatalog">the default catalog name</param>
            /// <param name="defaultSchema">the default schema name</param>
            /// <returns>an SQL string</returns>
            protected abstract String DoToSqlCreate(Dialect dialect, String defaultCatalog, String defaultSchema);
            /// <summary>
            /// Generates SQL for creating this model.
            /// </summary>
            /// <param name="dialect"></param>
            /// <param name="defaultCatalog">the default catalog name</param>
            /// <param name="defaultSchema">the default schema name</param>
            /// <returns>an SQL string</returns>
            protected abstract String DoToSqlDrop(Dialect dialect, String defaultCatalog, String defaultSchema);
        }

        /// <summary>
        /// A relational constraint.
        /// </summary>
        public abstract class Constraint : RelationalModel, IRelationalModel
        {
            /// <summary>
            /// Gets or sets the name of this constraint.
            /// </summary>
            public String Name { get; set; }

            /// <summary>
            /// Gets or set the table this constraint belongs to.
            /// </summary>
            public Table Table { get; set; }

            /// <summary>
            /// Generates SQL for creating this model.
            /// </summary>
            /// <param name="dialect"></param>
            /// <param name="defaultCatalog">the default catalog name</param>
            /// <param name="defaultSchema">the default schema name</param>
            /// <returns>an SQL string</returns>
            protected override String DoToSqlCreate(Dialect dialect, String defaultCatalog, String defaultSchema)
            {
                String constraintString = DoToSqlConstraint(dialect, Name);
                return StringHelper.CreateBuilder()
                    .Append("alter table ")
                    .Append(Table.GetQualifiedName(dialect, defaultCatalog, defaultSchema))
                    .Append(constraintString)
                    .ToString();
            }

            /// <summary>
            /// Generates SQL for creating this model.
            /// </summary>
            /// <param name="dialect"></param>
            /// <param name="defaultCatalog">the default catalog name</param>
            /// <param name="defaultSchema">the default schema name</param>
            /// <returns>an SQL string</returns>
            protected override String DoToSqlDrop(Dialect dialect, String defaultCatalog, String defaultSchema)
            {
                return StringHelper.CreateBuilder()
                    .Append("alter table ")
                    .Append(Table.GetQualifiedName(dialect, defaultCatalog, defaultSchema))
                    .Append(" drop constraint ")
                    .Append(dialect.Quote(Name))
                    .ToString();
            }

            /// <summary>
            /// 
            /// </summary>
            public override String ToString()
            {
                return GetType().Name + "(" + Table.Name + Columns + ") as " + Name;
            }

            /// <summary>
            /// Generates SQL for this constraint.
            /// </summary>
            /// <param name="dialect"></param>
            /// <param name="constraintName">the name of this constraint</param>
            /// <returns>an SQL string</returns>
            protected abstract String DoToSqlConstraint(Dialect dialect, String constraintName);
        }

        /// <summary>
        /// A primary key constaint.
        /// </summary>
        public class PrimaryKey : Constraint
        {
            /// <summary>
            /// Generates SQL for this primary key in create/alter table.
            /// </summary>
            /// <param name="dialect"></param>
            /// <returns>an SQL string</returns>
            public String ToSqlConstraintString(Dialect dialect)
            {
                StringBuilder sb = StringHelper.CreateBuilder().Append("primary key (");
                return AppendColumns(sb, dialect).Append(")").ToString();
            }

            /// <summary>
            /// Generates SQL for this constraint.
            /// </summary>
            /// <param name="dialect"></param>
            /// <param name="constraintName">the name of this constraint</param>
            /// <returns>an SQL string</returns>
            protected override String DoToSqlConstraint(Dialect dialect, String constraintName)
            {
                StringBuilder sb = StringHelper.CreateBuilder()
                    .Append(dialect.GetAddPrimaryKeyConstraintString(constraintName))
                    .Append("(");
                return AppendColumns(sb, dialect).Append(")").ToString();
            }

            private StringBuilder AppendColumns(StringBuilder sb, Dialect dialect)
            {
                StringHelper.AppendItemsWithComma(Columns, delegate(Column column)
                {
                    sb.Append(column.GetQuotedName(dialect));
                }, sb);
                return sb;
            }
        }

        /// <summary>
        /// A relational unique key constraint.
        /// </summary>
        public class UniqueKey : Constraint
        {
            /// <summary>
            /// Generates SQL for this unique key in create/alter table.
            /// </summary>
            /// <param name="dialect"></param>
            /// <returns>an SQL string</returns>
            public String ToSqlConstraintString(Dialect dialect)
            {
                StringBuilder sb = StringHelper.CreateBuilder().Append("unique (");
                Boolean hadNullableColumn = AppendColumns(sb, dialect);
                //do not add unique constraint on DB not supporting unique and nullable columns
                return (!hadNullableColumn || dialect.SupportsNotNullUnique) ?
                    sb.Append(")").ToString() :
                    null;
            }

            /// <summary>
            /// Generates SQL for this constraint.
            /// </summary>
            /// <param name="dialect"></param>
            /// <param name="constraintName">the name of this constraint</param>
            /// <returns>an SQL string</returns>
            protected override String DoToSqlConstraint(Dialect dialect, String constraintName)
            {
                StringBuilder sb = StringHelper.CreateBuilder().Append(dialect.GetAddUniqueKeyConstraintString(constraintName))
                    .Append("(");
                Boolean hadNullableColumn = AppendColumns(sb, dialect);
                //do not add unique constraint on DB not supporting unique and nullable columns
                return (!hadNullableColumn || dialect.SupportsNotNullUnique) ?
                    sb.Append(")").ToString() :
                    null;
            }

            /// <summary>
            /// Generates SQL for creating this model.
            /// </summary>
            /// <param name="dialect"></param>
            /// <param name="defaultCatalog">the default catalog name</param>
            /// <param name="defaultSchema">the default schema name</param>
            /// <returns>an SQL string</returns>
            protected override String DoToSqlCreate(Dialect dialect, String defaultCatalog, String defaultSchema)
            {
                if (IsGenerated(dialect))
                {
                    if (dialect.SupportsUniqueConstraintInCreateAlterTable)
                        return base.DoToSqlCreate(dialect, defaultCatalog, defaultSchema);
                    else
                        return Index.BuildSqlCreateIndexString(dialect, Name, Table, Columns, true, defaultCatalog, defaultSchema);
                }
                else
                    return null;
            }

            /// <summary>
            /// Generates SQL for creating this model.
            /// </summary>
            /// <param name="dialect"></param>
            /// <param name="defaultCatalog">the default catalog name</param>
            /// <param name="defaultSchema">the default schema name</param>
            /// <returns>an SQL string</returns>
            protected override String DoToSqlDrop(Dialect dialect, String defaultCatalog, String defaultSchema)
            {
                if (IsGenerated(dialect))
                {
                    if (dialect.SupportsUniqueConstraintInCreateAlterTable)
                        return base.DoToSqlDrop(dialect, defaultCatalog, defaultSchema);
                    else
                        return Index.BuildSqlDropIndexString(dialect, Table, Name, defaultCatalog, defaultSchema);
                }
                else
                    return null;
            }

            private Boolean IsGenerated(Dialect dialect)
            {
                if (dialect.SupportsNotNullUnique)
                    return true;

                foreach (Column column in Columns)
                {
                    if (column.Nullable)
                        return false;
                }

                return true;
            }

            private Boolean AppendColumns(StringBuilder sb, Dialect dialect)
            {
                Boolean hadNullableColumn = false;

                StringHelper.AppendItemsWithComma(Columns, delegate(Column column)
                {
                    if (!hadNullableColumn && column.Nullable)
                        hadNullableColumn = true;
                    sb.Append(column.GetQuotedName(dialect));
                }, sb);

                return hadNullableColumn;
            }
        }

        /// <summary>
        /// A relational table index.
        /// </summary>
        public class Index : RelationalModel, IRelationalModel
        {
            /// <summary>
            /// Gets or set the table this index belongs to.
            /// </summary>
            public Table Table { get; set; }

            /// <summary>
            /// Gets or sets the name of this index.
            /// </summary>
            public String Name { get; set; }

            internal static String BuildSqlCreateIndexString(Dialect dialect, String name, Table table, IEnumerable<Column> columns, Boolean unique, String defaultCatalog, String defaultSchema)
            {
                String tableQualifiedName = table.GetQualifiedName(dialect, defaultCatalog, defaultSchema);
                StringBuilder sb = StringHelper.CreateBuilder()
                    .Append("create")
                    .Append(unique ? " unique" : "")
                    .Append(" index ")
                    //.Append(dialect.QualifyIndexName ? name : StringHelper.Unqualify(name))
                    .Append(dialect.QualifyIndexName ? StringHelper.Qualify(tableQualifiedName, dialect.Quote(name)) : dialect.Quote(name))
                    .Append(" on ")
                    .Append(tableQualifiedName)
                    .Append(" (");

                StringHelper.AppendItemsWithComma(columns, delegate(Column column)
                {
                    sb.Append(column.GetQuotedName(dialect));
                }, sb);

                return sb.Append(")").ToString();
            }

            internal static String BuildSqlDropIndexString(Dialect dialect, Table table, String name, String defaultCatalog, String defaultSchema)
            {
                return "drop index " + (dialect.QualifyIndexName ? StringHelper.Qualify(table.GetQualifiedName(dialect, defaultCatalog, defaultSchema), dialect.Quote(name)) : dialect.Quote(name));
            }

            /// <summary>
            /// Generates SQL for creating this model.
            /// </summary>
            /// <param name="dialect"></param>
            /// <param name="defaultCatalog">the default catalog name</param>
            /// <param name="defaultSchema">the default schema name</param>
            /// <returns>an SQL string</returns>
            protected override String DoToSqlCreate(Dialect dialect, String defaultCatalog, String defaultSchema)
            {
                return BuildSqlCreateIndexString(dialect, Name, Table, Columns, false, defaultCatalog, defaultSchema);
            }

            /// <summary>
            /// Generates SQL for creating this model.
            /// </summary>
            /// <param name="dialect"></param>
            /// <param name="defaultCatalog">the default catalog name</param>
            /// <param name="defaultSchema">the default schema name</param>
            /// <returns>an SQL string</returns>
            protected override String DoToSqlDrop(Dialect dialect, String defaultCatalog, String defaultSchema)
            {
                return BuildSqlDropIndexString(dialect, Table, Name, defaultCatalog, defaultSchema);
            }

            /// <summary>
            /// 
            /// </summary>
            public override String ToString()
            {
                return GetType().Name + "(" + Name + ")";
            }
        }

        #endregion

        #region Attributes

        /// <summary>
        /// Defines properties of a table.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
        public class TableAttribute : Attribute
        {
            /// <summary>
            /// Gets or sets the name of this table.
            /// </summary>
            public String Name { get; set; }
        }

        /// <summary>
        /// Defines properties of a column.
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class ColumnAttribute : Attribute
        {
            /// <summary>
            /// 
            /// </summary>
            public ColumnAttribute()
            {
                Type = DbType.Empty;
            }
            /// <summary>
            /// Gets or sets the name of this column.
            /// </summary>
            public String Name { get; set; }
            /// <summary>
            /// Gets or sets the type of this column.
            /// </summary>
            public DbType Type { get; set; }
        }

        /// <summary>
        /// Determines whether a column is a primary key.
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class PrimaryKeyAttribute : Attribute
        {
        }

        /// <summary>
        /// Determines how a column should be indexed.
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class IndexAttribute : Attribute
        {
            /// <summary>
            /// Gets or sets the name of this index.
            /// </summary>
            public String Name { get; set; }
            /// <summary>
            /// Gets or sets the order of this index.
            /// </summary>
            public Int32 Order { get; set; }
            /// <summary>
            /// Gets or sets whether this index is unique.
            /// </summary>
            public virtual Boolean Unique { get; set; }
        }

        /// <summary>
        /// Defines a unique index.
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class UniqueAttribute : IndexAttribute
        {
            /// <summary>
            /// This is a unique index!
            /// </summary>
            public override Boolean Unique
            {
                get { return true; }
                set { /* throw?  */ }
            }
        }

        #endregion
    }

    static class ReflectHelper
    {
        public static T GetAttribute<T>(MemberInfo mi) where T : Attribute
        {
            Object[] attrs = mi.GetCustomAttributes(typeof(T), true);
            return attrs.Length > 0 ? (T)attrs[0] : default(T);
        }

        public static Boolean HasAttribute<T>(MemberInfo mi) where T : Attribute
        {
            return mi.IsDefined(typeof(T), true);
        }

        public static ConstructorInfo FindConstructor(Type type, String[] names, Type[] types)
        {
            var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Array.Sort(constructors, delegate(ConstructorInfo c1, ConstructorInfo c2)
            {
                if (c1.IsPrivate && c2.IsPublic)
                    return 1;
                else if (c1.IsPublic && c2.IsPrivate)
                    return -1;
                else
                    return c1.GetParameters().Length.CompareTo(c2.GetParameters().Length);
            });
            foreach (ConstructorInfo ctor in constructors)
            {
                ParameterInfo[] ctorParameters = ctor.GetParameters();

                if (ctorParameters.Length == 0)
                    return ctor;

                if (ctorParameters.Length != types.Length)
                    continue;

                Int32 i = 0;
                for (; i < ctorParameters.Length; i++)
                {
                    if (!String.Equals(ctorParameters[i].Name, names[i], StringComparison.OrdinalIgnoreCase))
                        break;
                    if (types[i] == typeof(Byte[]) && ctorParameters[i].ParameterType.FullName == SqlMapper.LinqBinary)
                        continue;
                    if (ctorParameters[i].ParameterType == types[i])
                        continue;
                    var unboxedType = Nullable.GetUnderlyingType(ctorParameters[i].ParameterType) ?? ctorParameters[i].ParameterType;
                    if (unboxedType != types[i]
                        && !(unboxedType.IsEnum && Enum.GetUnderlyingType(unboxedType) == types[i])
                        && !(unboxedType.IsEnum && types[i].IsPrimitive && types[i] != typeof(Boolean))
                        && !(unboxedType == typeof(Char) && types[i] == typeof(String)))
                        break;
                }

                if (i == ctorParameters.Length)
                    return ctor;
            }
            return null;
        }

        public static MethodInfo GetPropertySetter(PropertyInfo propertyInfo, Type type)
        {
            return propertyInfo.DeclaringType == type ?
                propertyInfo.GetSetMethod(true) :
                propertyInfo.DeclaringType.GetProperty(propertyInfo.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetSetMethod(true);
        }

        public static IEnumerable<PropertyInfo> GetSettableProperties(Type type)
        {
            return Array.FindAll(type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), 
                delegate(PropertyInfo p) { return GetPropertySetter(p, type) != null; });
        }

        public static IEnumerable<FieldInfo> GetSettableFields(Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        }

        public static Type GetMemberType(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                default:
                    return null;
            }
        }
    }
}
