//
// LX.EasyDb.IConnection.cs
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
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using LX.EasyDb.Criterion;
using Dapper;

namespace LX.EasyDb
{
    /// <summary>
    /// Represents a connection to the data source.
    /// </summary>
    public interface IConnection : System.Data.IDbConnection, IGenericQuery, ITypeQuery, IEntityQuery
    {
        /// <summary>
        /// Gets the original associated connection.
        /// </summary>
        IDbConnection Connection { get; }
        /// <summary>
        /// Gets the <see cref="System.Data.IDbTransaction"/> associated with this connection.
        /// </summary>
        IDbTransaction Transaction { get; }
        /// <summary>
        /// Executes an command statement and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">the text command to run against the data source</param>
        /// <param name="param">the object which contains parameters</param>
        /// <param name="commandTimeout">the wait time before terminating the attempt to execute a command and generating an error</param>
        /// <param name="commandType">the type indicates or specifies how the command is interpreted</param>
        /// <returns>the number of rows affected</returns>
        Int32 ExecuteNonQuery(String sql, Object param = null, Int32? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// Executes an command statement and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="sql">the text command to run against the data source</param>
        /// <param name="param">the object which contains parameters</param>
        /// <param name="commandTimeout">the wait time before terminating the attempt to execute a command and generating an error</param>
        /// <param name="commandType">the type indicates or specifies how the command is interpreted</param>
        /// <returns>the first column of the first row in the resultset</returns>
        Object ExecuteScalar(String sql, Object param, Int32? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// Executes an command statement and builds an <see cref="System.Data.IDataReader"/>.
        /// </summary>
        /// <param name="sql">the text command to run against the data source</param>
        /// <param name="param">the object which contains parameters</param>
        /// <param name="commandTimeout">the wait time before terminating the attempt to execute a command and generating an error</param>
        /// <param name="commandType">the type indicates or specifies how the command is interpreted</param>
        /// <param name="behavior">one of the <see cref="System.Data.CommandBehavior"/> values</param>
        /// <returns><see cref="System.Data.IDataReader"/></returns>
        IDataReader ExecuteReader(String sql, Object param, Int32? commandTimeout = null, CommandType? commandType = null, CommandBehavior? behavior = null);
        /// <summary>
        /// Executes a query and returns enumerable data typed as <see cref="System.Collections.Generic.IDictionary&lt;String, Object&gt;"/>.
        /// </summary>
        /// <param name="sql">the text command to run against the data source</param>
        /// <param name="param">the object which contains parameters</param>
        /// <param name="buffered">buffer the result or not</param>
        /// <param name="commandTimeout">the wait time before terminating the attempt to execute a command and generating an error</param>
        /// <param name="commandType">the type indicates or specifies how the command is interpreted</param>
        /// <returns>an <see cref="System.Collections.Generic.IEnumerable&lt;IDictionary&gt;"/></returns>
        IEnumerable<IDictionary<String, Object>> QueryDirect(String sql, Object param = null, Boolean buffered = true, Int32? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// Creates a criteria query.
        /// </summary>
        /// <typeparam name="T">the type of entities</typeparam>
        /// <returns>a <see cref="LX.EasyDb.ICriteria"/> to query entities</returns>
        ICriteria<T> CreateCriteria<T>();
        /// <summary>
        /// Creates a criteria query.
        /// </summary>
        /// <param name="type">the type of entities</param>
        /// <returns>a <see cref="LX.EasyDb.ICriteria"/> to query entities</returns>
        ICriteria CreateCriteria(Type type);
        /// <summary>
        /// Creates a criteria query.
        /// </summary>
        /// <param name="entity">the entity</param>
        /// <returns>a <see cref="LX.EasyDb.ICriteria"/> to query entities</returns>
        ICriteria CreateCriteria(String entity);
        /// <summary>
        /// Commits the transaction.
        /// </summary>
        void CommitTransaction();
        /// <summary>
        /// Rolls back the transaction.
        /// </summary>
        void RollbackTransaction();
    }

    class DbConnectionWrapper : IConnection
    {
        public DbConnectionWrapper(System.Data.IDbConnection connection)
        {
            Connection = connection;
        }

        public IConnectionFactorySupport Factory { get; set; }

        public System.Data.IDbConnection Connection { get; private set; }

        public IDbTransaction Transaction { get; private set; }

        #region Standard operations

        public Int32 ExecuteNonQuery(String sql, Object param = null, Int32? commandTimeout = null, CommandType? commandType = null)
        {
            IEnumerable multiExec = param as IEnumerable;
            SqlMapper.Identity identity;
            SqlMapper.CacheInfo info = null;

            if (multiExec != null && !(multiExec is String))
            {
                Int32 total = 0;
                Boolean first = true;
                using (IDbCommand cmd = SqlMapper.SetupCommand(Connection, Transaction, sql, null, null, commandTimeout, commandType))
                {
                    String masterSql = null; 
                    foreach (var obj in multiExec)
                    {
                        if (first)
                        {
                            masterSql = cmd.CommandText;
                            first = false;
                            identity = new SqlMapper.Identity(sql, cmd.CommandType, Connection, null, obj.GetType(), null);
                            info = GetCacheInfo(identity);
                        }
                        else
                        {
                            cmd.CommandText = masterSql; // because we do magic replaces on "in" etc
                            cmd.Parameters.Clear(); // current code is Add-tastic
                        }
                        info.ParamReader(cmd, obj);
                        total += cmd.ExecuteNonQuery();
                    }
                }
                return total;
            }
            else
            {
                // nice and simple
                if (param != null)
                {
                    identity = new SqlMapper.Identity(sql, commandType, Connection, null, param.GetType(), null);
                    info = GetCacheInfo(identity);
                }

                return SqlMapper.ExecuteCommand(Connection, Transaction, sql, param == null ? null : info.ParamReader, param, commandTimeout, commandType);
            }
        }

        public Object ExecuteScalar(String sql, Object param = null, Int32? commandTimeout = null, CommandType? commandType = null)
        {
            SqlMapper.CacheInfo info = null;
            if (param != null)
            {
                SqlMapper.Identity identity = new SqlMapper.Identity(sql, commandType, Connection, null, param.GetType(), null);
                info = GetCacheInfo(identity);
            }

            using (IDbCommand cmd = SqlMapper.SetupCommand(Connection, Transaction, sql, param == null ? null : info.ParamReader, param, commandTimeout, commandType))
            {
                return cmd.ExecuteScalar();
            }
        }

        public IDataReader ExecuteReader(String sql, Object param = null, Int32? commandTimeout = null, CommandType? commandType = null, CommandBehavior? behavior = null)
        {
            SqlMapper.CacheInfo info = null;
            if (param != null)
            {
                SqlMapper.Identity identity = new SqlMapper.Identity(sql, commandType, Connection, null, param.GetType(), null);
                info = GetCacheInfo(identity);
            }

            using (IDbCommand cmd = SqlMapper.SetupCommand(Connection, Transaction, sql, param == null ? null : info.ParamReader, param, commandTimeout, commandType))
            {
                return cmd.ExecuteReader(behavior.HasValue ? behavior.Value : CommandBehavior.Default);
            }
        }

        #endregion

        #region Generic query

        public IEnumerable<T> Query<T>(String sql, Object param = null, Boolean buffered = true, Int32? commandTimeout = null, CommandType? commandType = null)
        {
            var data = QueryInternal<T>(Connection, sql, param as object, Transaction, commandType, commandTimeout, Factory);
            return buffered ? Enumerable.ToList(data) : data;
        }

        public Boolean ExistTable<T>()
        {
            return ExistTable(typeof(T));
        }

        public void CreateTable<T>()
        {
            CreateTable(typeof(T));
        }

        public void DropTable<T>()
        {
            DropTable(typeof(T));
        }

        public Int64 Insert<T>(T item, Int32? commandTimeout = null)
        {
            return Insert(typeof(T), item, commandTimeout);
        }

        public T Find<T>(Object id, Int32? commandTimeout = null)
        {
            return (T)Find(typeof(T), id, commandTimeout);
        }

        public T Get<T>(Object id, Int32? commandTimeout = null)
        {
            return (T)Get(typeof(T), id, commandTimeout);
        }

        public Boolean Update<T>(T item, Int32? commandTimeout = null)
        {
            return Update(typeof(T), item, commandTimeout);
        }

        public Boolean Delete<T>(T item, Int32? commandTimeout = null)
        {
            return Delete(typeof(T), item, commandTimeout);
        }

        #endregion

        #region Type-based query

        public IEnumerable Query(Type type, String sql, Object param = null, Boolean buffered = true, Int32? commandTimeout = null, CommandType? commandType = null)
        {
            var data = QueryInternal(type, Connection, sql, param, Transaction, commandType, commandTimeout, Factory);
            return buffered ? Enumerable.ToList(data) : data;
        }

        public Boolean ExistTable(Type type)
        {
            return ExistTable(Factory.Mapping.FindTable(type));
        }

        public void CreateTable(Type type)
        {
            CreateTable(Factory.Mapping.FindTable(type));
        }

        public void DropTable(Type type)
        {
            DropTable(Factory.Mapping.FindTable(type));
        }

        public Int64 Insert(Type type, Object item, Int32? commandTimeout = null)
        {
            return Insert(Factory.Mapping.FindTable(type), item, commandTimeout);
        }

        public Object Find(Type type, Object id, Int32? commandTimeout = null)
        {
            return Find(type, Factory.Mapping.FindTable(type), id, commandTimeout);
        }

        public Object Get(Type type, Object id, Int32? commandTimeout = null)
        {
            return Get(type, Factory.Mapping.FindTable(type), id, commandTimeout);
        }

        public Boolean Update(Type type, Object item, Int32? commandTimeout = null)
        {
            return Update(Factory.Mapping.FindTable(type), item, commandTimeout);
        }

        public Boolean Delete(Type type, Object item, Int32? commandTimeout = null)
        {
            return Delete(Factory.Mapping.FindTable(type), item, commandTimeout);
        }

        #endregion

        #region Entity-based query

        public IEnumerable<IDictionary<String, Object>> Query(String entity, String sql, Object param = null, Boolean buffered = true, Int32? commandTimeout = null, CommandType? commandType = null)
        {
            var data = QueryInternal(Factory.Mapping.FindTable(entity), Connection, sql, param as object, Transaction, commandType, commandTimeout, Factory);
            return buffered ? Enumerable.ToList(data) : data;
        }

        public Int64 Insert(String entity, Object item, Int32? commandTimeout = null)
        {
            return Insert(Factory.Mapping.FindTable(entity), item, commandTimeout);
        }

        public IDictionary<String, Object> Find(String entity, Object id, Int32? commandTimeout = null)
        {
            return (IDictionary<String, Object>)Find(null, Factory.Mapping.FindTable(entity), id, commandTimeout);
        }

        public IDictionary<String, Object> Get(String entity, Object id, Int32? commandTimeout = null)
        {
            return (IDictionary<String, Object>)Get(null, Factory.Mapping.FindTable(entity), id, commandTimeout);
        }

        public Boolean Update(String entity, Object item, Int32? commandTimeout = null)
        {
            return Update(Factory.Mapping.FindTable(entity), item, commandTimeout);
        }

        public Boolean Delete(String entity, Object item, Int32? commandTimeout = null)
        {
            return Delete(Factory.Mapping.FindTable(entity), item, commandTimeout);
        }

        public Boolean ExistTable(String entity)
        {
            return ExistTable(Factory.Mapping.FindTable(entity));
        }

        public void CreateTable(String entity)
        {
            CreateTable(Factory.Mapping.FindTable(entity));
        }

        public void DropTable(String entity)
        {
            DropTable(Factory.Mapping.FindTable(entity));
        }

        #endregion

        #region Weak-typed

        public IEnumerable<IDictionary<String, Object>> QueryDirect(String sql, Object param = null, Boolean buffered = true, Int32? commandTimeout = null, CommandType? commandType = null)
        {
            return Query<IDictionary<String, Object>>(sql, param, buffered, commandTimeout, commandType);
        }

        /// <summary>
        /// Creates a new instance of an <see cref="System.Data.IDbDataAdapter"/> object.
        /// </summary>
        /// <param name="selectCommandText">the text command to run against the data source</param>
        /// <param name="param">the collection of parameters</param>
        /// <param name="commandTimeout">the wait time before terminating the attempt to execute a command and generating an error</param>
        /// <param name="commandType">the type indicates or specifies how the command is interpreted</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <returns><see cref="System.Data.IDbDataAdapter"/></returns>
        public IDbDataAdapter CreateDataAdapter(String selectCommandText, Object param = null, Int32? commandTimeout = null, CommandType? commandType = null)
        {
            var identity = new SqlMapper.Identity(selectCommandText, commandType, Connection, null, param == null ? null : param.GetType(), null);
            var cache = GetCacheInfo(identity);
            IDbDataAdapter ada = Factory.DbProviderFactory.CreateDataAdapter();
            ada.SelectCommand = SqlMapper.SetupCommand(Connection, Transaction, selectCommandText, cache.ParamReader, param, commandTimeout, commandType);
            return ada;
        }

        #endregion

        public ICriteria<T> CreateCriteria<T>()
        {
            return new Criterion.Criteria<T>(this, Factory);
        }

        public ICriteria CreateCriteria(Type type)
        {
            return new Criterion.Criteria(type, this, Factory);
        }

        public ICriteria CreateCriteria(String entity)
        {
            return new Criterion.Criteria(entity, this, Factory);
        }

        #region Mapped

        private static readonly Type mapType = typeof(SqlMapper.DapperRow);

        private static IEnumerable<T> QueryInternal<T>(System.Data.IDbConnection connection, String sql, Object param, IDbTransaction transaction, CommandType? commandType, Int32? commandTimeout, IConnectionFactorySupport factory)
        {
            Type type = typeof(T);
            foreach (var item in QueryInternal(type, factory.Mapping.FindTable(type), connection, sql, param, transaction, commandType, commandTimeout))
            {
                yield return (T)item;
            }
        }

        private static IEnumerable QueryInternal(Type type, System.Data.IDbConnection connection, String sql, Object param, IDbTransaction transaction, CommandType? commandType, Int32? commandTimeout, IConnectionFactorySupport factory)
        {
            return QueryInternal(type == null ? mapType : type, type == null ? null : factory.Mapping.FindTable(type), connection, sql, param as object, transaction, commandType, commandTimeout);
        }

        private static IEnumerable<IDictionary<String, Object>> QueryInternal(Mapping.Table table, System.Data.IDbConnection connection, String sql, Object param, IDbTransaction transaction, CommandType? commandType, Int32? commandTimeout, IConnectionFactorySupport factory)
        {
            foreach (var item in QueryInternal(mapType, table, connection, sql, param, transaction, commandType, commandTimeout))
            {
                yield return (IDictionary<String, Object>)item;
            }
        }

        private static IEnumerable QueryInternal(Type type, Mapping.Table table, System.Data.IDbConnection connection, String sql, Object param, IDbTransaction transaction, CommandType? commandType, Int32? commandTimeout)
        {
            var identity = new SqlMapper.Identity(sql, commandType, connection, type, param == null ? null : param.GetType(), null);
            var cache = GetCacheInfo(identity);

            IDbCommand cmd = null;
            IDataReader reader = null;

            Boolean wasClosed = connection.State == ConnectionState.Closed;
            try
            {
                cmd = SqlMapper.SetupCommand(connection, transaction, sql, cache.ParamReader, param, commandTimeout, commandType);

                if (wasClosed) connection.Open();
                reader = cmd.ExecuteReader(wasClosed ? CommandBehavior.CloseConnection : CommandBehavior.Default);
                wasClosed = false;  // *if* the connection was closed and we got this far, then we now have a reader
                                    // with the CloseConnection flag, so the reader will deal with the connection; we
                                    // still need something in the "finally" to ensure that broken SQL still results
                                    // in the connection closing itself
                var tuple = cache.Deserializer;
                Int32 hash = GetColumnHash(reader);
                if (tuple.Func == null || tuple.Hash != hash)
                {
                    tuple = cache.Deserializer = new SqlMapper.DeserializerState(hash, GetDeserializer(type, reader, 0, -1, false, table));
                    SetQueryCache(identity, cache);
                }

                var func = tuple.Func;

                while (reader.Read())
                {
                    yield return func(reader);
                }
                // happy path; close the reader cleanly - no
                // need for "Cancel" etc
                reader.Dispose();
                reader = null;
            }
            finally
            {
                if (reader != null)
                {
                    if (!reader.IsClosed)
                        try { cmd.Cancel(); }
                        catch { /* don't spol the existing exception */ }
                    reader.Dispose();
                }
                if (wasClosed) connection.Close();
                if (cmd != null) cmd.Dispose();
            }
        }

        private Boolean ExistTable(Mapping.Table table)
        {
            if (table == null)
                return false;
            try
            {
                ExecuteScalar("select 1 from " + table.GetQualifiedName(Factory.Dialect, Factory.Mapping.Catalog, Factory.Mapping.Schema), null);
            }
            catch (System.Data.Common.DbException)
            {
                return false;
            }
            return true;
        }

        private void CreateTable(Mapping.Table table)
        {
            if (table == null)
                return;
            ExecuteNonQuery(table.ToSqlCreate(Factory.Dialect, Factory.Mapping.Catalog, Factory.Mapping.Schema));
        }

        private void DropTable(Mapping.Table table)
        {
            if (table == null)
                return;
            ExecuteNonQuery(table.ToSqlDrop(Factory.Dialect, Factory.Mapping.Catalog, Factory.Mapping.Schema));
        }

        private Int64 Insert(Mapping.Table table, Object item, Int32? commandTimeout = null)
        {
            ExecuteNonQuery(table.ToSqlInsert(Factory.Dialect, Factory.Mapping.Catalog, Factory.Mapping.Schema), item, commandTimeout);

            Int64 r = 0;
            Mapping.Column idCol = table.IdColumn;
            if (idCol != null && Factory.Dialect.SelectIdentityString != null)
            {
                r = Enumerable.FirstOrDefault<Int64>(Query<Int64>(Factory.Dialect.SelectIdentityString, null, false, commandTimeout));
                if (idCol.MemberInfo != null && table.EntityType != null && table.EntityType.IsInstanceOfType(item))
                {
                    Object val = Convert.ChangeType(r, idCol.MemberInfo.Property.PropertyType);
                    if (idCol.MemberInfo.Property != null)
                        idCol.MemberInfo.Property.SetValue(item, val, null);
                    else if (idCol.MemberInfo.Field != null)
                        idCol.MemberInfo.Field.SetValue(item, val);
                }
            }

            return r;
        }

        private Object Find(Type type, Mapping.Table table, Object id, Int32? commandTimeout = null)
        {
            return GetInternal(type, table, id, commandTimeout, Enumerable.FirstOrDefault);
        }

        private Object Get(Type type, Mapping.Table table, Object id, Int32? commandTimeout = null)
        {
            return GetInternal(type, table, id, commandTimeout, Enumerable.First);
        }

        private Object GetInternal(Type type, Mapping.Table table, Object id, Int32? commandTimeout, Func<IEnumerable, Object> findStrategy)
        {
            if (!table.HasPrimaryKey)
                throw new Exception(String.Format("The type {0} has no primary-key property.", type == null ? table.Name : type.FullName));

            if (table.PrimaryKey.ColumnSpan > 1 && id.GetType().IsPrimitive)
                throw new Exception(String.Format("The type {0} has more than one primary-key property and cannot be queried by a single id value.", type == null ? table.Name : type.FullName));

            String sql = table.ToSqlSelect(Factory.Dialect, Factory.Mapping.Catalog, Factory.Mapping.Schema, true);

            var args = new DynamicParameters();

            Type idType = id.GetType();
            if (table.PrimaryKey.ColumnSpan == 1 && (idType.IsPrimitive || idType == typeof(String) || idType == typeof(DateTime)))
                args.Add(Enumerable.First(table.PrimaryKey.Columns).FieldName, id, null, null, null);
            else
                args.AddDynamicParams(id);

            Object obj = null;

            if (type == null)
            {
                obj = findStrategy(Query(table.Name, sql, args, false, commandTimeout));
            }
            else if (type.IsInterface)
            {
                var res = findStrategy(Query(table.Name, sql, args, false, commandTimeout)) as IDictionary<String, Object>;
                if (res == null)
                    return null;

                obj = ProxyGenerator.GetInterfaceProxy(type);
                foreach (var property in ReflectHelper.GetSettableProperties(type))
                {
                    var val = res[property.Name];
                    property.SetValue(obj, val, null);
                }

                ((SqlMapperExtensions.IProxy)obj).IsDirty = false;   //reset change tracking and return
            }
            else
            {
                obj = findStrategy(Query(type, sql, args, false, commandTimeout));
            }

            return obj;
        }

        private Boolean Update(Mapping.Table table, Object item, Int32? commandTimeout = null)
        {
            if (!table.HasPrimaryKey)
                throw new Exception("Entity should have at least one primary-key property");

            return ExecuteNonQuery(table.ToSqlUpdate(Factory.Dialect, Factory.Mapping.Catalog, Factory.Mapping.Schema), item, commandTimeout) > 0;
        }

        private Boolean Delete(Mapping.Table table, Object item, Int32? commandTimeout = null)
        {
            if (!table.HasPrimaryKey)
                throw new Exception("Entity should have at least one primary-key property");

            return ExecuteNonQuery(table.ToSqlDelete(Factory.Dialect, Factory.Mapping.Catalog, Factory.Mapping.Schema), item, commandTimeout) > 0;
        }

        #endregion

        #region Helper methods
        
        static Int32 GetColumnHash(IDataReader reader)
        {
            unchecked
            {
                Int32 colCount = reader.FieldCount, hash = colCount;
                for (Int32 i = 0; i < colCount; i++)
                {   // binding code is only interested in names - not types
                    Object tmp = reader.GetName(i);
                    hash = (hash * 31) + (tmp == null ? 0 : tmp.GetHashCode());
                }
                return hash;
            }
        }

        static Func<IDataReader, Object> GetDeserializer(Type type, IDataReader reader, Int32 startBound, Int32 length, Boolean returnNullIfFirstMissing, Mapping.Table table)
        { 
#if NET20
            if (type == typeof(Object)
                || type.IsAssignableFrom(typeof(Dictionary<String, Object>)))
                return GetDictionaryDeserializer(table, reader, startBound, length, returnNullIfFirstMissing);
#else
            if (type == typeof(object)
                || type == typeof(SqlMapper.DapperRow))
            {
                return SqlMapper.GetDapperRowDeserializer(reader, startBound, length, returnNullIfFirstMissing);
            }
#endif
            
            Type underlyingType = null;
            if (!(SqlMapper.HasDbType(type) || type.IsEnum ||  type.FullName == SqlMapper.LinqBinary ||
               (type.IsValueType && (underlyingType = Nullable.GetUnderlyingType(type)) != null && underlyingType.IsEnum)))
            {
                return SqlMapper.GetTypeDeserializer(type, reader, startBound, length, returnNullIfFirstMissing);
            }
            return SqlMapper.GetStructDeserializer(type, underlyingType ?? type, startBound);
        }

#if NET20
        static Func<IDataReader, Object> GetDictionaryDeserializer(Mapping.Table table, IDataReader reader, Int32 startBound, Int32 length, Boolean returnNullIfFirstMissing)
        {
            var fieldCount = reader.FieldCount;
            if (length == -1)
                length = fieldCount - startBound;

            if (fieldCount <= startBound)
                throw new ArgumentException("When using the multi-mapping APIs ensure you set the splitOn param if you have keys other than Id", "splitOn");

            Boolean hasTable = table != null;

            return r =>
            {
                IDictionary<String, Object> row = new Dictionary<String, Object>();
                for (var i = startBound; i < startBound + length; i++)
                {
                    var tmp = r.GetValue(i);
                    if (tmp == DBNull.Value)
                        tmp = null;
                    if (returnNullIfFirstMissing && i == startBound && tmp == null)
                        return null;
                    String fieldName = r.GetName(i);
                    if (hasTable)
                        fieldName = table.GetFieldName(fieldName);
                    row[fieldName] = tmp;
                }
                return row;
            };
        }

        static Func<IDataReader, Object> GetObjectDeserializer(Type type, IConnectionFactorySupport factory)
        {
            return delegate(IDataReader reader)
            {
                Object obj = null;
                var fieldCount = reader.FieldCount;

                if (type.IsPrimitive)
                {
                    // If the type is a primitive type, takes only the first column as the result.
                    if (fieldCount > 0)
                    {
                        obj = Convert.ChangeType(reader.GetValue(0), type);
                    }
                }
                else
                {
                    Mapping.Table table = factory.Mapping.FindTable(type);
                    obj = Activator.CreateInstance(type);
                    for (var i = 0; i < fieldCount; i++)
                    {
                        var tmp = reader.GetValue(i);
                        if (tmp == DBNull.Value)
                            continue;
                        Mapping.Column column = table.FindColumnByColumnName(reader.GetName(i));
                        if (column != null)
                        {
                            PropertyInfo pi = (column.MemberInfo == null || column.MemberInfo.Property == null) ? type.GetProperty(column.FieldName) : column.MemberInfo.Property;
                            pi.SetValue(obj, System.Convert.ChangeType(tmp, pi.PropertyType), null);
                        }
                    }
                }

                return obj;
            };
        }
#endif

        #endregion

        #region Proxy

        class ProxyGenerator
        {
            private static readonly Dictionary<Type, Type> proxyTypeCache = new Dictionary<Type, Type>();

            public static Object GetInterfaceProxy(Type type)
            {
                Type proxyType = null;

                if (!proxyTypeCache.TryGetValue(type, out proxyType))
                {
                    var assemblyBuilder = GetAsmBuilder(type.Name);

                    var moduleBuilder = assemblyBuilder.DefineDynamicModule("SqlMapperExtensions." + type.Name); //NOTE: to save, add "asdasd.dll" parameter

                    var interfaceType = typeof(SqlMapperExtensions.IProxy);
                    var typeBuilder = moduleBuilder.DefineType(type.Name + "_" + Guid.NewGuid(),
                        TypeAttributes.Public | TypeAttributes.Class);
                    typeBuilder.AddInterfaceImplementation(type);
                    typeBuilder.AddInterfaceImplementation(interfaceType);

                    //create our _isDirty field, which implements IProxy
                    var setIsDirtyMethod = CreateIsDirtyProperty(typeBuilder);

                    // Generate a field for each property, which implements the T
                    foreach (var property in type.GetProperties())
                    {
                        var isId = property.IsDefined(typeof(Mapping.PrimaryKey), true);
                        CreateProperty(type, typeBuilder, property.Name, property.PropertyType, setIsDirtyMethod, isId);
                    }

                    proxyType = typeBuilder.CreateType();

                    //assemblyBuilder.Save(name + ".dll");  //NOTE: to save, uncomment

                    proxyTypeCache.Add(type, proxyType);
                }

                return Activator.CreateInstance(proxyType);
            }

            private static AssemblyBuilder GetAsmBuilder(String name)
            {
                var assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(new AssemblyName { Name = name },
                    AssemblyBuilderAccess.Run);       //NOTE: to save, use RunAndSave

                return assemblyBuilder;
            }

            private static MethodInfo CreateIsDirtyProperty(TypeBuilder typeBuilder)
            {
                var propType = typeof(Boolean);
                var field = typeBuilder.DefineField("_" + "IsDirty", propType, FieldAttributes.Private);
                var property = typeBuilder.DefineProperty("IsDirty",
                                               System.Reflection.PropertyAttributes.None,
                                               propType,
                                               new Type[] { propType });

                const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.NewSlot | MethodAttributes.SpecialName |
                                                    MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig;

                // Define the "get" and "set" accessor methods
                var currGetPropMthdBldr = typeBuilder.DefineMethod("get_" + "IsDirty",
                                             getSetAttr,
                                             propType,
                                             Type.EmptyTypes);
                var currGetIL = currGetPropMthdBldr.GetILGenerator();
                currGetIL.Emit(OpCodes.Ldarg_0);
                currGetIL.Emit(OpCodes.Ldfld, field);
                currGetIL.Emit(OpCodes.Ret);
                var currSetPropMthdBldr = typeBuilder.DefineMethod("set_" + "IsDirty",
                                             getSetAttr,
                                             null,
                                             new Type[] { propType });
                var currSetIL = currSetPropMthdBldr.GetILGenerator();
                currSetIL.Emit(OpCodes.Ldarg_0);
                currSetIL.Emit(OpCodes.Ldarg_1);
                currSetIL.Emit(OpCodes.Stfld, field);
                currSetIL.Emit(OpCodes.Ret);

                property.SetGetMethod(currGetPropMthdBldr);
                property.SetSetMethod(currSetPropMthdBldr);
                var getMethod = typeof(SqlMapperExtensions.IProxy).GetMethod("get_" + "IsDirty");
                var setMethod = typeof(SqlMapperExtensions.IProxy).GetMethod("set_" + "IsDirty");
                typeBuilder.DefineMethodOverride(currGetPropMthdBldr, getMethod);
                typeBuilder.DefineMethodOverride(currSetPropMthdBldr, setMethod);

                return currSetPropMthdBldr;
            }

            private static void CreateProperty(Type type, TypeBuilder typeBuilder, String propertyName, Type propType, MethodInfo setIsDirtyMethod, Boolean isIdentity)
            {
                //Define the field and the property 
                var field = typeBuilder.DefineField("_" + propertyName, propType, FieldAttributes.Private);
                var property = typeBuilder.DefineProperty(propertyName,
                                               System.Reflection.PropertyAttributes.None,
                                               propType,
                                               new Type[] { propType });

                const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.Virtual |
                                                    MethodAttributes.HideBySig;

                // Define the "get" and "set" accessor methods
                var currGetPropMthdBldr = typeBuilder.DefineMethod("get_" + propertyName,
                                             getSetAttr,
                                             propType,
                                             Type.EmptyTypes);

                var currGetIL = currGetPropMthdBldr.GetILGenerator();
                currGetIL.Emit(OpCodes.Ldarg_0);
                currGetIL.Emit(OpCodes.Ldfld, field);
                currGetIL.Emit(OpCodes.Ret);

                var currSetPropMthdBldr = typeBuilder.DefineMethod("set_" + propertyName,
                                             getSetAttr,
                                             null,
                                             new Type[] { propType });

                //store value in private field and set the isdirty flag
                var currSetIL = currSetPropMthdBldr.GetILGenerator();
                currSetIL.Emit(OpCodes.Ldarg_0);
                currSetIL.Emit(OpCodes.Ldarg_1);
                currSetIL.Emit(OpCodes.Stfld, field);
                currSetIL.Emit(OpCodes.Ldarg_0);
                currSetIL.Emit(OpCodes.Ldc_I4_1);
                currSetIL.Emit(OpCodes.Call, setIsDirtyMethod);
                currSetIL.Emit(OpCodes.Ret);

                //TODO: Should copy all attributes defined by the interface?
                if (isIdentity)
                {
                    var keyAttribute = typeof(Mapping.PrimaryKey);
                    var myConstructorInfo = keyAttribute.GetConstructor(new Type[] { });
                    var attributeBuilder = new CustomAttributeBuilder(myConstructorInfo, new object[] { });
                    property.SetCustomAttribute(attributeBuilder);
                }

                property.SetGetMethod(currGetPropMthdBldr);
                property.SetSetMethod(currSetPropMthdBldr);
                var getMethod = type.GetMethod("get_" + propertyName);
                var setMethod = type.GetMethod("set_" + propertyName);
                typeBuilder.DefineMethodOverride(currGetPropMthdBldr, getMethod);
                typeBuilder.DefineMethodOverride(currSetPropMthdBldr, setMethod);
            }
        }

        #endregion

        #region Cache

        private const Int32 COLLECT_PER_ITEMS = 1000, COLLECT_HIT_COUNT_MIN = 0;
        static readonly Dictionary<SqlMapper.Identity, SqlMapper.CacheInfo> _queryCache = new Dictionary<SqlMapper.Identity, SqlMapper.CacheInfo>();
        static readonly Object _queryCacheSync = ((IDictionary)_queryCache).SyncRoot;
        private static Int32 _collect;

        private static Boolean TryGetQueryCache(SqlMapper.Identity key, out SqlMapper.CacheInfo value)
        {
            Boolean found;
            lock (_queryCacheSync)
            {
                found = _queryCache.TryGetValue(key, out value);
            }
            if (found)
                value.RecordHit();
            else
                value = null;
            return found;
        }

        private static void PurgeQueryCacheByType(Type type)
        {
            List<SqlMapper.Identity> keysToRemove = new List<SqlMapper.Identity>();
            lock (_queryCacheSync)
            {
                foreach (var pair in _queryCache)
                {
                    if (pair.Key.type == type)
                        keysToRemove.Add(pair.Key);
                }
                foreach (var key in keysToRemove)
                {
                    _queryCache.Remove(key);
                }
            }
        }

        private static void SetQueryCache(SqlMapper.Identity key, SqlMapper.CacheInfo value)
        {
            if (Interlocked.Increment(ref _collect) == COLLECT_PER_ITEMS)
                CollectCacheGarbage();
            lock (_queryCacheSync)
            {
                _queryCache[key] = value;
            }
        }

        private static void CollectCacheGarbage()
        {
            List<SqlMapper.Identity> keysToRemove = new List<SqlMapper.Identity>();
            lock (_queryCacheSync)
            {
                foreach (var pair in _queryCache)
                {
                    if (pair.Value.GetHitCount() <= COLLECT_HIT_COUNT_MIN)
                        keysToRemove.Add(pair.Key);
                }
                try
                {
                    foreach (var key in keysToRemove)
                    {
                        _queryCache.Remove(key);
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref _collect, 0);
                }
            }
        }

        private static SqlMapper.CacheInfo GetCacheInfo(SqlMapper.Identity identity)
        {
            SqlMapper.CacheInfo info;
            if (!TryGetQueryCache(identity, out info))
            {
                info = new SqlMapper.CacheInfo();
                if (identity.parametersType != null)
                {
                    if (typeof(SqlMapper.IDynamicParameters).IsAssignableFrom(identity.parametersType))
                        info.ParamReader = (cmd, obj) => { (obj as SqlMapper.IDynamicParameters).AddParameters(cmd, identity); };
                    else if (typeof(IEnumerable<KeyValuePair<String, Object>>).IsAssignableFrom(identity.parametersType))
                        info.ParamReader = (cmd, obj) => { (new DynamicParameters(obj) as SqlMapper.IDynamicParameters).AddParameters(cmd, identity); };
                    else if (identity.parametersType.IsValueType && !identity.parametersType.IsPrimitive)
                        info.ParamReader = (cmd, obj) =>
                        {
                            foreach (PropertyInfo pi in obj.GetType().GetProperties())
                            {
                                IDbDataParameter param = cmd.CreateParameter();
                                param.ParameterName = pi.Name;
                                param.Value = pi.GetValue(obj, null);
                                cmd.Parameters.Add(param);
                            }
                        };
                    else
                        info.ParamReader = SqlMapper.CreateParamInfoGenerator(identity, false);
                }
                SetQueryCache(identity, info);
            }
            return info;
        }

        #endregion

        #region System.Data.IDbConnection

        private Int32 _transactionCounter = 0;

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            Transaction = Connection.BeginTransaction(il);
            return Transaction;
        }

        public IDbTransaction BeginTransaction()
        {
            if (_transactionCounter++ == 0)
                Transaction = Connection.BeginTransaction();
            return Transaction;
        }

        public void CommitTransaction()
        {
            if (_transactionCounter > 0 && --_transactionCounter == 0)
            {
                Transaction.Commit();
                Transaction.Dispose();
                Transaction = null;
            }
        }

        public void RollbackTransaction()
        {
            if (_transactionCounter > 0 && --_transactionCounter == 0)
            {
                Transaction.Rollback();
                Transaction.Dispose();
                Transaction = null;
            }
        }

        public void ChangeDatabase(String databaseName)
        {
            Connection.ChangeDatabase(databaseName);
        }

        public void Close()
        {
            Connection.Close();
        }

        public String ConnectionString
        {
            get { return Connection.ConnectionString; }
            set { Connection.ConnectionString = value; }
        }

        public Int32 ConnectionTimeout
        {
            get { return Connection.ConnectionTimeout; }
        }

        public IDbCommand CreateCommand()
        {
            return Connection.CreateCommand();
        }

        public String Database
        {
            get { return Connection.Database; }
        }

        public void Open()
        {
            Connection.Open();
        }

        public ConnectionState State
        {
            get { return Connection.State; }
        }

        public void Dispose()
        {
            Connection.Dispose();
        }

        #endregion
    }
}
