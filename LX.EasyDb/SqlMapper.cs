/*
 License: http://www.apache.org/licenses/LICENSE-2.0 
 Home page: http://code.google.com/p/dapper-dot-net/

 Note: to build on C# 3.0 + .NET 3.5, include the CSHARP30 compiler symbol (and yes,
 I know the difference between language and runtime versions; this is a compromise).
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace T3dParty.Dapper
{
    /// <summary>
    /// Encapsulates a method that has one parameter and returns a value of the type specified by the <typeparamref name="TResult"/> parameter.
    /// </summary>
    /// <typeparam name="T">the type of the parameter</typeparam>
    /// <typeparam name="TResult">the type of the return value</typeparam>
    /// <param name="arg">the parameter of the method that this delegate encapsulates</param>
    /// <returns></returns>
    public delegate TResult Func<in T, out TResult>(T arg);
    /// <summary>
    /// Encapsulates a method that has two parameters and does not return a value.
    /// </summary>
    /// <typeparam name="T1">the type of the first parameter</typeparam>
    /// <typeparam name="T2">the type of the second parameter</typeparam>
    /// <param name="arg1">the first parameter</param>
    /// <param name="arg2">the second parameter</param>
    public delegate void Action<in T1, in T2>(T1 arg1, T2 arg2);

    /// <summary>
    /// Dapper, a light weight object mapper for ADO.NET
    /// </summary>
    public static class SqlMapper
    {
        /// <summary>
        /// Implement this interface to pass an arbitrary db specific set of parameters to Dapper
        /// </summary>
        public interface IDynamicParameters
        {
            /// <summary>
            /// Add all the parameters needed to the command just before it executes
            /// </summary>
            /// <param name="command">The raw command prior to execution</param>
            /// <param name="identity">Information about the query</param>
            void AddParameters(IDbCommand command, Identity identity);
        }

        /// <summary>
        /// Implements this interface to provide custom type mapping registry.
        /// </summary>
        public interface ITypeMapRegistry
        {
            /// <summary>
            /// Gets type-map for the given type
            /// </summary>
            /// <returns>Type map implementation, DefaultTypeMap instance if no override present</returns>
            ITypeMap GetTypeMap(Type type);
            /// <summary>
            /// Set custom mapping for type deserializers
            /// </summary>
            /// <param name="type">Entity type to override</param>
            /// <param name="map">Mapping rules impementation, null to remove custom map</param>
            void SetTypeMap(Type type, ITypeMap map);
        }

        /// <summary>
        /// Implements this interface to change default mapping of reader columns to type memebers
        /// </summary>
        public interface ITypeMap
        {
            /// <summary>
            /// Finds best constructor
            /// </summary>
            /// <param name="names">DataReader column names</param>
            /// <param name="types">DataReader column types</param>
            /// <returns>Matching constructor or default one</returns>
            ConstructorInfo FindConstructor(String[] names, Type[] types);

            /// <summary>
            /// Gets mapping for constructor parameter
            /// </summary>
            /// <param name="constructor">Constructor to resolve</param>
            /// <param name="columnName">DataReader column name</param>
            /// <returns>Mapping implementation</returns>
            IMemberMap GetConstructorParameter(ConstructorInfo constructor, string columnName);

            /// <summary>
            /// Gets member mapping for column
            /// </summary>
            /// <param name="columnName">DataReader column name</param>
            /// <returns>Mapping implementation</returns>
            IMemberMap GetMember(String columnName);
        }

        /// <summary>
        /// Implements this interface to provide custom member mapping
        /// </summary>
        public interface IMemberMap
        {
            /// <summary>
            /// Source DataReader column name
            /// </summary>
            string ColumnName { get; }

            /// <summary>
            ///  Target member type
            /// </summary>
            Type MemberType { get; }

            /// <summary>
            /// Target property
            /// </summary>
            PropertyInfo Property { get; }

            /// <summary>
            /// Target field
            /// </summary>
            FieldInfo Field { get; }

            /// <summary>
            /// Target constructor parameter
            /// </summary>
            ParameterInfo Parameter { get; }
        }

        internal static Link<Type, Action<IDbCommand, bool>> bindByNameCache;
        internal static Action<IDbCommand, bool> GetBindByName(Type commandType)
        {
            if (commandType == null) return null; // GIGO
            Action<IDbCommand, bool> action;
            if (Link<Type, Action<IDbCommand, bool>>.TryGet(bindByNameCache, commandType, out action))
            {
                return action;
            }
            var prop = commandType.GetProperty("BindByName", BindingFlags.Public | BindingFlags.Instance);
            action = null;
            ParameterInfo[] indexers;
            MethodInfo setter;
            if (prop != null && prop.CanWrite && prop.PropertyType == typeof(bool)
                && ((indexers = prop.GetIndexParameters()) == null || indexers.Length == 0)
                && (setter = prop.GetSetMethod()) != null
                )
            {
                var method = new DynamicMethod(commandType.Name + "_BindByName", null, new Type[] { typeof(IDbCommand), typeof(bool) });
                var il = method.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, commandType);
                il.Emit(OpCodes.Ldarg_1);
                il.EmitCall(OpCodes.Callvirt, setter, null);
                il.Emit(OpCodes.Ret);
                action = (Action<IDbCommand, bool>)method.CreateDelegate(typeof(Action<IDbCommand, bool>));
            }
            // cache it            
            Link<Type, Action<IDbCommand, bool>>.TryAdd(ref bindByNameCache, commandType, ref action);
            return action;
        }
        /// <summary>
        /// This is a micro-cache; suitable when the number of terms is controllable (a few hundred, for example),
        /// and strictly append-only; you cannot change existing values. All key matches are on **REFERENCE**
        /// equality. The type is fully thread-safe.
        /// </summary>
        internal class Link<TKey, TValue> where TKey : class
        {
            public static bool TryGet(Link<TKey, TValue> link, TKey key, out TValue value)
            {
                while (link != null)
                {
                    if ((object)key == (object)link.Key)
                    {
                        value = link.Value;
                        return true;
                    }
                    link = link.Tail;
                }
                value = default(TValue);
                return false;
            }
            public static bool TryAdd(ref Link<TKey, TValue> head, TKey key, ref TValue value)
            {
                bool tryAgain;
                do
                {
                    var snapshot = Interlocked.CompareExchange(ref head, null, null);
                    TValue found;
                    if (TryGet(snapshot, key, out found))
                    { // existing match; report the existing value instead
                        value = found;
                        return false;
                    }
                    var newNode = new Link<TKey, TValue>(key, value, snapshot);
                    // did somebody move our cheese?
                    tryAgain = Interlocked.CompareExchange(ref head, newNode, snapshot) != snapshot;
                } while (tryAgain);
                return true;
            }
            private Link(TKey key, TValue value, Link<TKey, TValue> tail)
            {
                Key = key;
                Value = value;
                Tail = tail;
            }
            public TKey Key { get; private set; }
            public TValue Value { get; private set; }
            public Link<TKey, TValue> Tail { get; private set; }
        }
        internal class CacheInfo
        {
            public DeserializerState Deserializer { get; set; }
            public Func<IDataReader, object>[] OtherDeserializers { get; set; }
            public Action<IDbCommand, object> ParamReader { get; set; }
            private int hitCount;
            public int GetHitCount() { return Interlocked.CompareExchange(ref hitCount, 0, 0); }
            public void RecordHit() { Interlocked.Increment(ref hitCount); }
        }
        internal static int GetColumnHash(IDataReader reader)
        {
            unchecked
            {
                int colCount = reader.FieldCount, hash = colCount;
                for (int i = 0; i < colCount; i++)
                {   // binding code is only interested in names - not types
                    object tmp = reader.GetName(i);
                    hash = (hash * 31) + (tmp == null ? 0 : tmp.GetHashCode());
                }
                return hash;
            }
        }
        internal struct DeserializerState
        {
            public readonly int Hash;
            public readonly Func<IDataReader, object> Func;

            public DeserializerState(int hash, Func<IDataReader, object> func)
            {
                Hash = hash;
                Func = func;
            }
        }

        static readonly Dictionary<Type, DbType> typeMap;

        static SqlMapper()
        {
            enumParse = typeof(Enum).GetMethod("Parse", new Type[] { typeof(Type), typeof(String), typeof(Boolean) });

            foreach (PropertyInfo p in typeof(System.Data.IDataRecord).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (p.GetIndexParameters().Length > 0 && p.GetIndexParameters()[0].ParameterType == typeof(Int32))
                {
                    getItem = p.GetGetMethod();
                    break;
                }
            }

            typeMap = new Dictionary<Type, DbType>();
            typeMap[typeof(byte)] = DbType.Byte;
            typeMap[typeof(sbyte)] = DbType.SByte;
            typeMap[typeof(short)] = DbType.Int16;
            typeMap[typeof(ushort)] = DbType.UInt16;
            typeMap[typeof(int)] = DbType.Int32;
            typeMap[typeof(uint)] = DbType.UInt32;
            typeMap[typeof(long)] = DbType.Int64;
            typeMap[typeof(ulong)] = DbType.UInt64;
            typeMap[typeof(float)] = DbType.Single;
            typeMap[typeof(double)] = DbType.Double;
            typeMap[typeof(decimal)] = DbType.Decimal;
            typeMap[typeof(bool)] = DbType.Boolean;
            typeMap[typeof(string)] = DbType.String;
            typeMap[typeof(char)] = DbType.StringFixedLength;
            typeMap[typeof(Guid)] = DbType.Guid;
            typeMap[typeof(DateTime)] = DbType.DateTime;
            typeMap[typeof(DateTimeOffset)] = DbType.DateTimeOffset;
            typeMap[typeof(TimeSpan)] = DbType.Time;
            typeMap[typeof(byte[])] = DbType.Binary;
            typeMap[typeof(byte?)] = DbType.Byte;
            typeMap[typeof(sbyte?)] = DbType.SByte;
            typeMap[typeof(short?)] = DbType.Int16;
            typeMap[typeof(ushort?)] = DbType.UInt16;
            typeMap[typeof(int?)] = DbType.Int32;
            typeMap[typeof(uint?)] = DbType.UInt32;
            typeMap[typeof(long?)] = DbType.Int64;
            typeMap[typeof(ulong?)] = DbType.UInt64;
            typeMap[typeof(float?)] = DbType.Single;
            typeMap[typeof(double?)] = DbType.Double;
            typeMap[typeof(decimal?)] = DbType.Decimal;
            typeMap[typeof(bool?)] = DbType.Boolean;
            typeMap[typeof(char?)] = DbType.StringFixedLength;
            typeMap[typeof(Guid?)] = DbType.Guid;
            typeMap[typeof(DateTime?)] = DbType.DateTime;
            typeMap[typeof(DateTimeOffset?)] = DbType.DateTimeOffset;
            typeMap[typeof(TimeSpan?)] = DbType.Time;
            typeMap[typeof(Object)] = DbType.Object;
        }

        internal const string LinqBinary = "System.Data.Linq.Binary";
        internal static DbType LookupDbType(Type type, string name)
        {
            DbType dbType;
            var nullUnderlyingType = Nullable.GetUnderlyingType(type);
            if (nullUnderlyingType != null) type = nullUnderlyingType;
            if (type.IsEnum)
            {
                type = Enum.GetUnderlyingType(type);
            }
            if (typeMap.TryGetValue(type, out dbType))
            {
                return dbType;
            }
            if (type.FullName == LinqBinary)
            {
                return DbType.Binary;
            }
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                // use xml to denote its a list, hacky but will work on any DB
                return DbType.Xml;
            }


            throw new NotSupportedException(string.Format("The member {0} of type {1} cannot be used as a parameter value", name, type));
        }

        internal static Boolean HasDbType(Type type)
        {
            return typeMap.ContainsKey(type);
        }

        /// <summary>
        /// Identity of a cached query in Dapper, used for extensability
        /// </summary>
        public class Identity : IEquatable<Identity>
        {
            internal Identity ForGrid(Type primaryType, int gridIndex)
            {
                return new Identity(sql, commandType, connectionString, primaryType, parametersType, null, gridIndex);
            }

            internal Identity ForGrid(Type primaryType, Type[] otherTypes, int gridIndex)
            {
                return new Identity(sql, commandType, connectionString, primaryType, parametersType, otherTypes, gridIndex);
            }
            /// <summary>
            /// Create an identity for use with DynamicParameters, internal use only
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            public Identity ForDynamicParameters(Type type)
            {
                return new Identity(sql, commandType, connectionString, this.type, type, null, -1);
            }

            internal Identity(string sql, CommandType? commandType, IDbConnection connection, Type type, Type parametersType, Type[] otherTypes)
                : this(sql, commandType, connection.ConnectionString, type, parametersType, otherTypes, 0)
            { }
            private Identity(string sql, CommandType? commandType, string connectionString, Type type, Type parametersType, Type[] otherTypes, int gridIndex)
            {
                this.sql = sql;
                this.commandType = commandType;
                this.connectionString = connectionString;
                this.type = type;
                this.parametersType = parametersType;
                this.gridIndex = gridIndex;
                unchecked
                {
                    hashCode = 17; // we *know* we are using this in a dictionary, so pre-compute this
                    hashCode = hashCode * 23 + commandType.GetHashCode();
                    hashCode = hashCode * 23 + gridIndex.GetHashCode();
                    hashCode = hashCode * 23 + (sql == null ? 0 : sql.GetHashCode());
                    hashCode = hashCode * 23 + (type == null ? 0 : type.GetHashCode());
                    if (otherTypes != null)
                    {
                        foreach (var t in otherTypes)
                        {
                            hashCode = hashCode * 23 + (t == null ? 0 : t.GetHashCode());
                        }
                    }
                    hashCode = hashCode * 23 + (connectionString == null ? 0 : connectionString.GetHashCode());
                    hashCode = hashCode * 23 + (parametersType == null ? 0 : parametersType.GetHashCode());
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                return Equals(obj as Identity);
            }
            /// <summary>
            /// The sql
            /// </summary>
            public readonly string sql;
            /// <summary>
            /// The command type 
            /// </summary>
            public readonly CommandType? commandType;

            /// <summary>
            /// 
            /// </summary>
            public readonly int hashCode, gridIndex;
            /// <summary>
            /// 
            /// </summary>
            public readonly Type type;
            /// <summary>
            /// 
            /// </summary>
            public readonly string connectionString;
            /// <summary>
            /// 
            /// </summary>
            public readonly Type parametersType;
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return hashCode;
            }
            /// <summary>
            /// Compare 2 Identity objects
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public bool Equals(Identity other)
            {
                return
                    other != null &&
                    gridIndex == other.gridIndex &&
                    type == other.type &&
                    sql == other.sql &&
                    commandType == other.commandType &&
                    connectionString == other.connectionString &&
                    parametersType == other.parametersType;
            }
        }

        /// <summary>
        /// Internal use only
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method is for internal usage only", false)]
        public static char ReadChar(object value)
        {
            if (value == null || value is DBNull) throw new ArgumentNullException("value");
            string s = value as string;
            if (s == null || s.Length != 1) throw new ArgumentException("A single-character was expected", "value");
            return s[0];
        }

        /// <summary>
        /// Internal use only
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method is for internal usage only", false)]
        public static char? ReadNullableChar(object value)
        {
            if (value == null || value is DBNull) return null;
            string s = value as string;
            if (s == null || s.Length != 1) throw new ArgumentException("A single-character was expected", "value");
            return s[0];
        }

        /// <summary>
        /// Internal use only
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method is for internal usage only", true)]
        public static void PackListParameters(IDbCommand command, string namePrefix, object value)
        {
            // initially we tried TVP, however it performs quite poorly.
            // keep in mind SQL support up to 2000 params easily in sp_executesql, needing more is rare

            var list = value as IEnumerable;
            var count = 0;

            if (list != null)
            {
                if (FeatureSupport.Get(command.Connection).Arrays)
                {
                    var arrayParm = command.CreateParameter();
                    arrayParm.Value = list;
                    arrayParm.ParameterName = namePrefix;
                    command.Parameters.Add(arrayParm);
                }
                else
                {
                    bool isString = value is IEnumerable<string>;
                    bool isDbString = value is IEnumerable<DbString>;
                    foreach (var item in list)
                    {
                        count++;
                        var listParam = command.CreateParameter();
                        listParam.ParameterName = namePrefix + count;
                        listParam.Value = item ?? DBNull.Value;
                        if (isString)
                        {
                            listParam.Size = 4000;
                            if (item != null && ((string)item).Length > 4000)
                            {
                                listParam.Size = -1;
                            }
                        }
                        if (isDbString && item as DbString != null)
                        {
                            var str = item as DbString;
                            str.AddParameter(command, listParam.ParameterName);
                        }
                        else
                        {
                            command.Parameters.Add(listParam);
                        }
                    }

                    if (count == 0)
                    {
                        command.CommandText = Regex.Replace(command.CommandText, @"[?@:]" + Regex.Escape(namePrefix), "(SELECT NULL)"); // WHERE 1 = 0)");
                    }
                    else
                    {
                        command.CommandText = Regex.Replace(command.CommandText, @"[?@:]" + Regex.Escape(namePrefix), match =>
                        {
                            var grp = match.Value;
                            var sb = new StringBuilder("(").Append(grp).Append(1);
                            for (int i = 2; i <= count; i++)
                            {
                                sb.Append(',').Append(grp).Append(i);
                            }
                            return sb.Append(')').ToString();
                        });
                    }
                }
            }
        }

        private static IEnumerable<PropertyInfo> FilterParameters(PropertyInfo[] parameters, string sql)
        {
            return Array.FindAll(parameters, p => Regex.IsMatch(sql, "[@:]" + p.Name + "([^a-zA-Z0-9_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline));
        }

        /// <summary>
        /// Internal use only
        /// </summary>
        public static Action<IDbCommand, object> CreateParamInfoGenerator(Identity identity)
        {
            Type type = identity.parametersType;
            bool filterParams = identity.commandType.GetValueOrDefault(CommandType.Text) == CommandType.Text;
            var dm = new DynamicMethod(string.Format("ParamInfo{0}", Guid.NewGuid()), null, new[] { typeof(IDbCommand), typeof(object) }, type, true);

            var il = dm.GetILGenerator();

            il.DeclareLocal(type); // 0
            bool haveInt32Arg1 = false;
            il.Emit(OpCodes.Ldarg_1); // stack is now [untyped-param]
            il.Emit(OpCodes.Unbox_Any, type); // stack is now [typed-param]
            il.Emit(OpCodes.Stloc_0);// stack is now empty

            il.Emit(OpCodes.Ldarg_0); // stack is now [command]
            il.EmitCall(OpCodes.Callvirt, typeof(IDbCommand).GetProperty("Parameters").GetGetMethod(), null); // stack is now [parameters]

            PropertyInfo[] pis = type.GetProperties();
            Array.Sort(pis, delegate(PropertyInfo pi1, PropertyInfo pi2) { return pi1.Name.CompareTo(pi2.Name); });

            //IEnumerable<PropertyInfo> props = type.GetProperties().OrderBy(p => p.Name);
            IEnumerable<PropertyInfo> props = pis;
            if (filterParams)
            {
                props = FilterParameters(pis, identity.sql);
            }
            foreach (var prop in props)
            {
                if (filterParams)
                {
                    if (identity.sql.IndexOf("@" + prop.Name, StringComparison.InvariantCultureIgnoreCase) < 0
                        && identity.sql.IndexOf(":" + prop.Name, StringComparison.InvariantCultureIgnoreCase) < 0)
                    { // can't see the parameter in the text (even in a comment, etc) - burn it with fire
                        continue;
                    }
                }
                if (prop.PropertyType == typeof(DbString))
                {
                    il.Emit(OpCodes.Ldloc_0); // stack is now [parameters] [typed-param]
                    il.Emit(OpCodes.Callvirt, prop.GetGetMethod()); // stack is [parameters] [dbstring]
                    il.Emit(OpCodes.Ldarg_0); // stack is now [parameters] [dbstring] [command]
                    il.Emit(OpCodes.Ldstr, prop.Name); // stack is now [parameters] [dbstring] [command] [name]
                    il.EmitCall(OpCodes.Callvirt, typeof(DbString).GetMethod("AddParameter"), null); // stack is now [parameters]
                    continue;
                }
                DbType dbType = LookupDbType(prop.PropertyType, prop.Name);
                if (dbType == DbType.Xml)
                {
                    // this actually represents special handling for list types;
                    il.Emit(OpCodes.Ldarg_0); // stack is now [parameters] [command]
                    il.Emit(OpCodes.Ldstr, prop.Name); // stack is now [parameters] [command] [name]
                    il.Emit(OpCodes.Ldloc_0); // stack is now [parameters] [command] [name] [typed-param]
                    il.Emit(OpCodes.Callvirt, prop.GetGetMethod()); // stack is [parameters] [command] [name] [typed-value]
                    if (prop.PropertyType.IsValueType)
                    {
                        il.Emit(OpCodes.Box, prop.PropertyType); // stack is [parameters] [command] [name] [boxed-value]
                    }
                    il.EmitCall(OpCodes.Call, typeof(SqlMapper).GetMethod("PackListParameters"), null); // stack is [parameters]
                    continue;
                }
                il.Emit(OpCodes.Dup); // stack is now [parameters] [parameters]

                il.Emit(OpCodes.Ldarg_0); // stack is now [parameters] [parameters] [command]
                il.EmitCall(OpCodes.Callvirt, typeof(IDbCommand).GetMethod("CreateParameter"), null);// stack is now [parameters] [parameters] [parameter]

                il.Emit(OpCodes.Dup);// stack is now [parameters] [parameters] [parameter] [parameter]
                il.Emit(OpCodes.Ldstr, prop.Name); // stack is now [parameters] [parameters] [parameter] [parameter] [name]
                il.EmitCall(OpCodes.Callvirt, typeof(IDataParameter).GetProperty("ParameterName").GetSetMethod(), null);// stack is now [parameters] [parameters] [parameter]

                if (dbType != DbType.Time) // https://connect.microsoft.com/VisualStudio/feedback/details/381934/sqlparameter-dbtype-dbtype-time-sets-the-parameter-to-sqldbtype-datetime-instead-of-sqldbtype-time
                {
                    il.Emit(OpCodes.Dup);// stack is now [parameters] [parameters] [parameter] [parameter]
                    EmitInt32(il, (int)dbType);// stack is now [parameters] [parameters] [parameter] [parameter] [db-type]

                    il.EmitCall(OpCodes.Callvirt, typeof(IDataParameter).GetProperty("DbType").GetSetMethod(), null);// stack is now [parameters] [parameters] [parameter]
                }

                il.Emit(OpCodes.Dup);// stack is now [parameters] [parameters] [parameter] [parameter]
                EmitInt32(il, (int)ParameterDirection.Input);// stack is now [parameters] [parameters] [parameter] [parameter] [dir]
                il.EmitCall(OpCodes.Callvirt, typeof(IDataParameter).GetProperty("Direction").GetSetMethod(), null);// stack is now [parameters] [parameters] [parameter]

                il.Emit(OpCodes.Dup);// stack is now [parameters] [parameters] [parameter] [parameter]
                il.Emit(OpCodes.Ldloc_0); // stack is now [parameters] [parameters] [parameter] [parameter] [typed-param]
                il.Emit(OpCodes.Callvirt, prop.GetGetMethod()); // stack is [parameters] [parameters] [parameter] [parameter] [typed-value]
                bool checkForNull = true;
                if (prop.PropertyType.IsValueType)
                {
                    il.Emit(OpCodes.Box, prop.PropertyType); // stack is [parameters] [parameters] [parameter] [parameter] [boxed-value]
                    if (Nullable.GetUnderlyingType(prop.PropertyType) == null)
                    { // struct but not Nullable<T>; boxed value cannot be null
                        checkForNull = false;
                    }
                }
                if (checkForNull)
                {
                    if (dbType == DbType.String && !haveInt32Arg1)
                    {
                        il.DeclareLocal(typeof(int));
                        haveInt32Arg1 = true;
                    }
                    // relative stack: [boxed value]
                    il.Emit(OpCodes.Dup);// relative stack: [boxed value] [boxed value]
                    Label notNull = il.DefineLabel();
                    Label? allDone = dbType == DbType.String ? il.DefineLabel() : (Label?)null;
                    il.Emit(OpCodes.Brtrue_S, notNull);
                    // relative stack [boxed value = null]
                    il.Emit(OpCodes.Pop); // relative stack empty
                    il.Emit(OpCodes.Ldsfld, typeof(DBNull).GetField("Value")); // relative stack [DBNull]
                    if (dbType == DbType.String)
                    {
                        EmitInt32(il, 0);
                        il.Emit(OpCodes.Stloc_1);
                    }
                    if (allDone != null) il.Emit(OpCodes.Br_S, allDone.Value);
                    il.MarkLabel(notNull);
                    if (prop.PropertyType == typeof(string))
                    {
                        il.Emit(OpCodes.Dup); // [string] [string]
                        il.EmitCall(OpCodes.Callvirt, typeof(string).GetProperty("Length").GetGetMethod(), null); // [string] [length]
                        EmitInt32(il, 4000); // [string] [length] [4000]
                        il.Emit(OpCodes.Cgt); // [string] [0 or 1]
                        Label isLong = il.DefineLabel(), lenDone = il.DefineLabel();
                        il.Emit(OpCodes.Brtrue_S, isLong);
                        EmitInt32(il, 4000); // [string] [4000]
                        il.Emit(OpCodes.Br_S, lenDone);
                        il.MarkLabel(isLong);
                        EmitInt32(il, -1); // [string] [-1]
                        il.MarkLabel(lenDone);
                        il.Emit(OpCodes.Stloc_1); // [string]
                    }
                    if (prop.PropertyType.FullName == LinqBinary)
                    {
                        il.EmitCall(OpCodes.Callvirt, prop.PropertyType.GetMethod("ToArray", BindingFlags.Public | BindingFlags.Instance), null);
                    }
                    if (allDone != null) il.MarkLabel(allDone.Value);
                    // relative stack [boxed value or DBNull]
                }
                il.EmitCall(OpCodes.Callvirt, typeof(IDataParameter).GetProperty("Value").GetSetMethod(), null);// stack is now [parameters] [parameters] [parameter]

                if (prop.PropertyType == typeof(string))
                {
                    var endOfSize = il.DefineLabel();
                    // don't set if 0
                    il.Emit(OpCodes.Ldloc_1); // [parameters] [parameters] [parameter] [size]
                    il.Emit(OpCodes.Brfalse_S, endOfSize); // [parameters] [parameters] [parameter]

                    il.Emit(OpCodes.Dup);// stack is now [parameters] [parameters] [parameter] [parameter]
                    il.Emit(OpCodes.Ldloc_1); // stack is now [parameters] [parameters] [parameter] [parameter] [size]
                    il.EmitCall(OpCodes.Callvirt, typeof(IDbDataParameter).GetProperty("Size").GetSetMethod(), null);// stack is now [parameters] [parameters] [parameter]

                    il.MarkLabel(endOfSize);
                }

                il.EmitCall(OpCodes.Callvirt, typeof(IList).GetMethod("Add"), null); // stack is now [parameters]
                il.Emit(OpCodes.Pop); // IList.Add returns the new index (int); we don't care
            }
            // stack is currently [command]
            il.Emit(OpCodes.Pop); // stack is now empty
            il.Emit(OpCodes.Ret);
            return (Action<IDbCommand, object>)dm.CreateDelegate(typeof(Action<IDbCommand, object>));
        }

        internal static IDbCommand SetupCommand(IDbConnection cnn, IDbTransaction transaction, string sql, Action<IDbCommand, object> paramReader, object obj, int? commandTimeout, CommandType? commandType)
        {
            var cmd = cnn.CreateCommand();
            var bindByName = GetBindByName(cmd.GetType());
            if (bindByName != null) bindByName(cmd, true);
            if (transaction != null)
                cmd.Transaction = transaction;
            cmd.CommandText = sql;
            if (commandTimeout.HasValue)
                cmd.CommandTimeout = commandTimeout.Value;
            if (commandType.HasValue)
                cmd.CommandType = commandType.Value;
            if (paramReader != null)
            {
                paramReader(cmd, obj);
            }
            return cmd;
        }

        internal static int ExecuteCommand(IDbConnection cnn, IDbTransaction transaction, string sql, Action<IDbCommand, object> paramReader, object obj, int? commandTimeout, CommandType? commandType)
        {
            using (var cmd = SetupCommand(cnn, transaction, sql, paramReader, obj, commandTimeout, commandType))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        internal static Func<IDataReader, object> GetStructDeserializer(Type type, Type effectiveType, int index)
        {
            // no point using special per-type handling here; it boils down to the same, plus not all are supported anyway (see: SqlDataReader.GetChar - not supported!)
#pragma warning disable 618
            if (type == typeof(char))
            { // this *does* need special handling, though
                return r => SqlMapper.ReadChar(r.GetValue(index));
            }
            if (type == typeof(char?))
            {
                return r => SqlMapper.ReadNullableChar(r.GetValue(index));
            }
            if (type.FullName == LinqBinary)
            {
                return r => Activator.CreateInstance(type, r.GetValue(index));
            }
#pragma warning restore 618

            if (effectiveType.IsEnum)
            {   // assume the value is returned as the correct type (int/byte/etc), but box back to the typed enum
                return r =>
                {
                    var val = r.GetValue(index);
                    return val is DBNull ? null : Enum.ToObject(effectiveType, val);
                };
            }
            return r =>
            {
                var val = r.GetValue(index);
                if (val is DBNull)
                    return null;
                else
                {
                    var underlyingType = Nullable.GetUnderlyingType(type);
                    var converter = GetConverter(underlyingType ?? type);
                    return converter == null ? System.Convert.ChangeType(val, type) : converter.Method.Invoke(null, new[] { val });
                }
            };
        }

        static readonly MethodInfo enumParse;
        static readonly MethodInfo getItem;

        /// <summary>
        /// Internal use only
        /// </summary>
        public static Func<IDataReader, object> GetTypeDeserializer(Type type, IDataReader reader, ITypeMap typeMap, int startBound = 0, int length = -1, bool returnNullIfFirstMissing = false)
        {
            var dm = new DynamicMethod(string.Format("Deserialize{0}", Guid.NewGuid()), typeof(object), new[] { typeof(IDataReader) }, true);
            var il = dm.GetILGenerator();
            il.DeclareLocal(typeof(int));
            il.DeclareLocal(type);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stloc_0);

            if (length == -1)
            {
                length = reader.FieldCount - startBound;
            }

            if (reader.FieldCount <= startBound)
            {
                throw new ArgumentException("When using the multi-mapping APIs ensure you set the splitOn param if you have keys other than Id", "splitOn");
            }

            var names = GetColumnNames(reader, startBound, length);

            int index = startBound;

            ConstructorInfo specializedConstructor = null;

            if (type.IsValueType)
            {
                il.Emit(OpCodes.Ldloca_S, (byte)1);
                il.Emit(OpCodes.Initobj, type);
            }
            else
            {
                var types = new Type[length];
                for (int i = startBound; i < startBound + length; i++)
                {
                    IMemberMap member = typeMap.GetMember(names[i]);
                    types[i - startBound] = (member == null) ? reader.GetFieldType(i) : typeMap.GetMember(names[i]).MemberType;
                }

                if (type.IsValueType)
                {
                    il.Emit(OpCodes.Ldloca_S, (byte)1);
                    il.Emit(OpCodes.Initobj, type);
                }
                else
                {
                    var ctor = typeMap.FindConstructor(names, types);
                    if (ctor == null)
                    {
                        List<String> list = new List<String>();
                        for (Int32 i = 0; i < types.Length; i++)
                        {
                            list.Add(types[i].FullName + " " + names[i]);
                        }
                        String proposedTypes = "(" + String.Join(", ", list.ToArray()) + ")";
                        throw new InvalidOperationException(String.Format("A parameterless default constructor or one matching signature {0} is required for {1} materialization", proposedTypes, type.FullName));
                    }

                    if (ctor.GetParameters().Length == 0)
                    {
                        il.Emit(OpCodes.Newobj, ctor);
                        il.Emit(OpCodes.Stloc_1);
                    }
                    else
                        specializedConstructor = ctor;
                }
            }

            il.BeginExceptionBlock();
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Ldloca_S, (byte)1);// [target]
            }
            else if (specializedConstructor == null)
            {
                il.Emit(OpCodes.Ldloc_1);// [target]
            }

            List<IMemberMap> members = new List<IMemberMap>();
            if (specializedConstructor == null)
            {
                foreach (var n in names)
                    members.Add(typeMap.GetMember(n));
            }
            else
            {
                Array.ForEach(names, n => members.Add(typeMap.GetConstructorParameter(specializedConstructor, n)));
            }

            // stack is now [target]

            bool first = true;
            var allDone = il.DefineLabel();
            int enumDeclareLocal = -1;
            foreach (var item in members)
            {
                if (item != null)
                {
                    if (specializedConstructor == null)
                        il.Emit(OpCodes.Dup); // stack is now [target][target]
                    Label isDbNullLabel = il.DefineLabel();
                    Label finishLabel = il.DefineLabel();

                    il.Emit(OpCodes.Ldarg_0); // stack is now [target][target][reader]
                    EmitInt32(il, index); // stack is now [target][target][reader][index]
                    il.Emit(OpCodes.Dup);// stack is now [target][target][reader][index][index]
                    il.Emit(OpCodes.Stloc_0);// stack is now [target][target][reader][index]
                    il.Emit(OpCodes.Callvirt, getItem); // stack is now [target][target][value-as-object]

                    Type memberType = item.MemberType;

                    if (memberType == typeof(char) || memberType == typeof(char?))
                    {
                        il.EmitCall(OpCodes.Call, typeof(SqlMapper).GetMethod(
                            memberType == typeof(char) ? "ReadChar" : "ReadNullableChar", BindingFlags.Static | BindingFlags.Public), null); // stack is now [target][target][typed-value]
                    }
                    else
                    {
                        il.Emit(OpCodes.Dup); // stack is now [target][target][value][value]
                        il.Emit(OpCodes.Isinst, typeof(DBNull)); // stack is now [target][target][value-as-object][DBNull or null]
                        il.Emit(OpCodes.Brtrue_S, isDbNullLabel); // stack is now [target][target][value-as-object]

                        // unbox nullable enums as the primitive, i.e. byte etc

                        var nullUnderlyingType = Nullable.GetUnderlyingType(memberType);
                        var unboxType = nullUnderlyingType != null && nullUnderlyingType.IsEnum ? nullUnderlyingType : memberType;

                        if (unboxType.IsEnum)
                        {
                            if (enumDeclareLocal == -1)
                            {
                                enumDeclareLocal = il.DeclareLocal(typeof(string)).LocalIndex;
                            }

                            Label isNotString = il.DefineLabel();
                            il.Emit(OpCodes.Dup); // stack is now [target][target][value][value]
                            il.Emit(OpCodes.Isinst, typeof(string)); // stack is now [target][target][value-as-object][string or null]
                            il.Emit(OpCodes.Dup);// stack is now [target][target][value-as-object][string or null][string or null]
                            StoreLocal(il, enumDeclareLocal); // stack is now [target][target][value-as-object][string or null]
                            il.Emit(OpCodes.Brfalse_S, isNotString); // stack is now [target][target][value-as-object]

                            il.Emit(OpCodes.Pop); // stack is now [target][target]

                            il.Emit(OpCodes.Ldtoken, unboxType); // stack is now [target][target][enum-type-token]
                            il.EmitCall(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"), null);// stack is now [target][target][enum-type]
                            il.Emit(OpCodes.Ldloc_2); // stack is now [target][target][enum-type][string]
                            il.Emit(OpCodes.Ldc_I4_1); // stack is now [target][target][enum-type][string][true]
                            il.EmitCall(OpCodes.Call, enumParse, null); // stack is now [target][target][enum-as-object]

                            Label enumDone = il.DefineLabel();
                            il.Emit(OpCodes.Unbox_Any, unboxType); // stack is now [target][target][typed-value]
                            il.Emit(OpCodes.Br_S, enumDone);

                            il.MarkLabel(isNotString);

                            il.Emit(OpCodes.Unbox_Any, reader.GetFieldType(index));
                            il.Emit(OpCodes.Conv_Ovf_I4); // stack is now [target][target][i4-value]

                            il.MarkLabel(enumDone);

                            if (nullUnderlyingType != null)
                            {
                                il.Emit(OpCodes.Newobj, memberType.GetConstructor(new[] { nullUnderlyingType })); // stack is now [target][target][enum-value]
                            }
                        }
                        else if (memberType.FullName == LinqBinary)
                        {
                            il.Emit(OpCodes.Unbox_Any, typeof(byte[])); // stack is now [target][target][byte-array]
                            il.Emit(OpCodes.Newobj, memberType.GetConstructor(new Type[] { typeof(byte[]) }));// stack is now [target][target][binary]
                        }
                        else
                        {
                            Type dataType = reader.GetFieldType(index);
                            TypeCode dataTypeCode = Type.GetTypeCode(dataType);
                            TypeCode unboxTypeCode = Type.GetTypeCode(nullUnderlyingType ?? unboxType);
                            if (dataType == unboxType || dataTypeCode == unboxTypeCode)
                            {
                                il.Emit(OpCodes.Unbox_Any, unboxType); // stack is now [target][target][typed-value]
                            }
                            else
                            {
                                // not a direct match; need to tweak the unbox
                                bool handled = true;
                                OpCode opCode = default(OpCode);
                                if (dataTypeCode == TypeCode.Decimal || unboxTypeCode == TypeCode.Decimal)
                                {   // no IL level conversions to/from decimal; I guess we could use the static operators, but
                                    // this feels an edge-case
                                    handled = false;
                                }
                                else
                                {
                                    switch (unboxTypeCode)
                                    {
                                        case TypeCode.Byte:
                                            opCode = OpCodes.Conv_Ovf_I1_Un; break;
                                        case TypeCode.SByte:
                                            opCode = OpCodes.Conv_Ovf_I1; break;
                                        case TypeCode.UInt16:
                                            opCode = OpCodes.Conv_Ovf_I2_Un; break;
                                        case TypeCode.Int16:
                                            opCode = OpCodes.Conv_Ovf_I2; break;
                                        case TypeCode.UInt32:
                                            opCode = OpCodes.Conv_Ovf_I4_Un; break;
                                        case TypeCode.Boolean: // boolean is basically an int, at least at this level
                                        case TypeCode.Int32:
                                            opCode = OpCodes.Conv_Ovf_I4; break;
                                        case TypeCode.UInt64:
                                            opCode = OpCodes.Conv_Ovf_I8_Un; break;
                                        case TypeCode.Int64:
                                            opCode = OpCodes.Conv_Ovf_I8; break;
                                        case TypeCode.Single:
                                            opCode = OpCodes.Conv_R4; break;
                                        case TypeCode.Double:
                                            opCode = OpCodes.Conv_R8; break;
                                        default:
                                            handled = false;
                                            break;
                                    }
                                }
                                if (handled)
                                { // unbox as the data-type, then use IL-level convert
                                    il.Emit(OpCodes.Unbox_Any, dataType); // stack is now [target][target][data-typed-value]
                                    il.Emit(opCode); // stack is now [target][target][typed-value]
                                    if (nullUnderlyingType != null)
                                        il.Emit(OpCodes.Newobj, memberType.GetConstructor(new[] { nullUnderlyingType })); // stack is now [target][target][nullable-value]
                                }
                                else
                                { // use flexible conversion
                                    // try generic converter
                                    var convertDelegate = GetConverter(unboxType);
                                    if (convertDelegate == null)
                                    { // no convert method found, try System.Convert
                                        il.Emit(OpCodes.Ldtoken, unboxType); // stack is now [target][target][value][member-type-token]
                                        il.EmitCall(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"), null); // stack is now [target][target][value][member-type]
                                        il.EmitCall(OpCodes.Call, typeof(Convert).GetMethod("ChangeType", new Type[] { typeof(object), typeof(Type) }), null); // stack is now [target][target][boxed-member-type-value]
                                        il.Emit(OpCodes.Unbox_Any, unboxType); // stack is now [target][target][typed-value]
                                    }
                                    else
                                    { // using custom type converter
                                        il.EmitCall(OpCodes.Call, convertDelegate.Method, null); // stack is now [target][target][typed-value]
                                    }
                                }
                            }
                        }
                    }
                    if (specializedConstructor == null)
                    {
                        // Store the value in the property/field
                        if (item.Property != null)
                        {
                            if (type.IsValueType)
                            {
                                il.Emit(OpCodes.Call, ReflectHelper.GetPropertySetter(item.Property, type)); // stack is now [target]
                            }
                            else
                            {
                                il.Emit(OpCodes.Callvirt, ReflectHelper.GetPropertySetter(item.Property, type)); // stack is now [target]
                            }
                        }
                        else
                        {
                            il.Emit(OpCodes.Stfld, item.Field); // stack is now [target]
                        }
                    }

                    il.Emit(OpCodes.Br_S, finishLabel); // stack is now [target]

                    il.MarkLabel(isDbNullLabel); // incoming stack: [target][target][value]
                    if (specializedConstructor != null)
                    {
                        il.Emit(OpCodes.Pop);
                        if (item.MemberType.IsValueType)
                        {
                            int localIndex = il.DeclareLocal(item.MemberType).LocalIndex;
                            LoadLocalAddress(il, localIndex);
                            il.Emit(OpCodes.Initobj, item.MemberType);
                            LoadLocal(il, localIndex);
                        }
                        else
                        {
                            il.Emit(OpCodes.Ldnull);
                        }
                    }
                    else
                    {
                        il.Emit(OpCodes.Pop); // stack is now [target][target]
                        il.Emit(OpCodes.Pop); // stack is now [target]
                    }

                    if (first && returnNullIfFirstMissing)
                    {
                        il.Emit(OpCodes.Pop);
                        il.Emit(OpCodes.Ldnull); // stack is now [null]
                        il.Emit(OpCodes.Stloc_1);
                        il.Emit(OpCodes.Br, allDone);
                    }

                    il.MarkLabel(finishLabel);
                }
                first = false;
                index += 1;
            }
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Pop);
            }
            else
            {
                if (specializedConstructor != null)
                {
                    il.Emit(OpCodes.Newobj, specializedConstructor);
                }
                il.Emit(OpCodes.Stloc_1); // stack is empty
            }
            il.MarkLabel(allDone);
            il.BeginCatchBlock(typeof(Exception)); // stack is Exception
            il.Emit(OpCodes.Ldloc_0); // stack is Exception, index
            il.Emit(OpCodes.Ldarg_0); // stack is Exception, index, reader
            il.EmitCall(OpCodes.Call, typeof(SqlMapper).GetMethod("ThrowDataException"), null);
            il.EndExceptionBlock();

            il.Emit(OpCodes.Ldloc_1); // stack is [rval]
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Box, type);
            }
            il.Emit(OpCodes.Ret);

            return (Func<IDataReader, object>)dm.CreateDelegate(typeof(Func<IDataReader, object>));
        }

        static Delegate GetConverter(Type type)
        { 
            return (Delegate)typeof(Converter<>).MakeGenericType(type).GetField("Convert", BindingFlags.Static | BindingFlags.Public).GetValue(null);
        }

        static String[] GetColumnNames(IDataReader reader, Int32 start, Int32 count)
        {
            Int32 colCount = reader.FieldCount;
            String[] names = new String[colCount];
            for (Int32 i = start; i < start + count; i++)
            {
                names[i] = reader.GetName(i);
            }
            return names;
        }

        static void LoadLocal(ILGenerator il, int index)
        {
            if (index < 0 || index >= short.MaxValue) throw new ArgumentNullException("index");
            switch (index)
            {
                case 0: il.Emit(OpCodes.Ldloc_0); break;
                case 1: il.Emit(OpCodes.Ldloc_1); break;
                case 2: il.Emit(OpCodes.Ldloc_2); break;
                case 3: il.Emit(OpCodes.Ldloc_3); break;
                default:
                    if (index <= 255)
                    {
                        il.Emit(OpCodes.Ldloc_S, (byte)index);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldloc, (short)index);
                    }
                    break;
            }
        }

        static void StoreLocal(ILGenerator il, int index)
        {
            if (index < 0 || index >= short.MaxValue) throw new ArgumentNullException("index");
            switch (index)
            {
                case 0: il.Emit(OpCodes.Stloc_0); break;
                case 1: il.Emit(OpCodes.Stloc_1); break;
                case 2: il.Emit(OpCodes.Stloc_2); break;
                case 3: il.Emit(OpCodes.Stloc_3); break;
                default:
                    if (index <= 255)
                    {
                        il.Emit(OpCodes.Stloc_S, (byte)index);
                    }
                    else
                    {
                        il.Emit(OpCodes.Stloc, (short)index);
                    }
                    break;
            }
        }

        static void LoadLocalAddress(ILGenerator il, int index)
        {
            if (index < 0 || index >= short.MaxValue) throw new ArgumentNullException("index");

            if (index <= 255)
            {
                il.Emit(OpCodes.Ldloca_S, (byte)index);
            }
            else
            {
                il.Emit(OpCodes.Ldloca, (short)index);
            }
        }

        /// <summary>
        /// Throws a data exception, only used internally
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="index"></param>
        /// <param name="reader"></param>
        public static void ThrowDataException(Exception ex, int index, IDataReader reader)
        {
            Exception toThrow;
            try
            {
                string name = "(n/a)", value = "(n/a)";
                if (reader != null && index >= 0 && index < reader.FieldCount)
                {
                    name = reader.GetName(index);
                    object val = reader.GetValue(index);
                    if (val == null || val is DBNull)
                    {
                        value = "<null>";
                    }
                    else
                    {
                        value = Convert.ToString(val) + " - " + Type.GetTypeCode(val.GetType());
                    }
                }
                toThrow = new DataException(string.Format("Error parsing column {0} ({1}={2})", index, name, value), ex);
            }
            catch
            { // throw the **original** exception, wrapped as DataException
                toThrow = new DataException(ex.Message, ex);
            }
            throw toThrow;
        }

        private static void EmitInt32(ILGenerator il, int value)
        {
            switch (value)
            {
                case -1: il.Emit(OpCodes.Ldc_I4_M1); break;
                case 0: il.Emit(OpCodes.Ldc_I4_0); break;
                case 1: il.Emit(OpCodes.Ldc_I4_1); break;
                case 2: il.Emit(OpCodes.Ldc_I4_2); break;
                case 3: il.Emit(OpCodes.Ldc_I4_3); break;
                case 4: il.Emit(OpCodes.Ldc_I4_4); break;
                case 5: il.Emit(OpCodes.Ldc_I4_5); break;
                case 6: il.Emit(OpCodes.Ldc_I4_6); break;
                case 7: il.Emit(OpCodes.Ldc_I4_7); break;
                case 8: il.Emit(OpCodes.Ldc_I4_8); break;
                default:
                    if (value >= -128 && value <= 127)
                    {
                        il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldc_I4, value);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Dapper, a light weight object mapper for ADO.NET
    /// </summary>
    public static class SqlMapperExtensions
    {
        /// <summary>
        /// Represents generated proxy types.
        /// </summary>
        public interface IProxy
        {
            /// <summary>
            /// Gets or sets a value indicating if this entity has been modified since being retrieved.
            /// </summary>
            bool IsDirty { get; set; }
        }
    }

    /// <summary>
    /// A bag of parameters that can be passed to the Dapper Query and Execute methods
    /// </summary>
    public class DynamicParameters : SqlMapper.IDynamicParameters
    {
        static Dictionary<SqlMapper.Identity, Action<IDbCommand, object>> paramReaderCache = new Dictionary<SqlMapper.Identity, Action<IDbCommand, object>>();

        Dictionary<string, ParamInfo> parameters = new Dictionary<string, ParamInfo>();
        List<object> templates;

        class ParamInfo
        {
            public string Name { get; set; }
            public object Value { get; set; }
            public ParameterDirection ParameterDirection { get; set; }
            public DbType? DbType { get; set; }
            public int? Size { get; set; }
            public IDbDataParameter AttachedParam { get; set; }
        }

        /// <summary>
        /// construct a dynamic parameter bag
        /// </summary>
        public DynamicParameters() { }

        /// <summary>
        /// construct a dynamic parameter bag
        /// </summary>
        /// <param name="template">can be an anonymous type or a DynamicParameters bag</param>
        public DynamicParameters(object template)
        {
            AddDynamicParams(template);
        }

        /// <summary>
        /// Append a whole object full of params to the dynamic
        /// EG: AddDynamicParams(new {A = 1, B = 2}) // will add property A and B to the dynamic
        /// </summary>
        /// <param name="param"></param>
        public void AddDynamicParams(object param)
        {
            var obj = param as object;
            if (obj != null)
            {
                var subDynamic = obj as DynamicParameters;
                if (subDynamic == null)
                {
                    var dictionary = obj as IEnumerable<KeyValuePair<string, object>>;
                    if (dictionary == null)
                    {
                        templates = templates ?? new List<object>();
                        templates.Add(obj);
                    }
                    else
                    {
                        foreach (var kvp in dictionary)
                        {
#if CSHARP30
                            Add(kvp.Key, kvp.Value, null, null, null);
#else
                            Add(kvp.Key, kvp.Value);
#endif
                        }
                    }
                }
                else
                {
                    if (subDynamic.parameters != null)
                    {
                        foreach (var kvp in subDynamic.parameters)
                        {
                            parameters.Add(kvp.Key, kvp.Value);
                        }
                    }

                    if (subDynamic.templates != null)
                    {
                        templates = templates ?? new List<object>();
                        foreach (var t in subDynamic.templates)
                        {
                            templates.Add(t);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add a parameter to this dynamic parameter list
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        /// <param name="direction"></param>
        /// <param name="size"></param>
        public void Add(string name, object value = null, DbType? dbType = null, ParameterDirection? direction = null, int? size = null)
        {
            parameters[Clean(name)] = new ParamInfo() { Name = name, Value = value, ParameterDirection = direction ?? ParameterDirection.Input, DbType = dbType, Size = size };
        }

        static string Clean(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                switch (name[0])
                {
                    case '@':
                    case ':':
                    case '?':
                        return name.Substring(1);
                }
            }
            return name;
        }

        void SqlMapper.IDynamicParameters.AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            AddParameters(command, identity);
        }

        /// <summary>
        /// Add all the parameters needed to the command just before it executes
        /// </summary>
        /// <param name="command">The raw command prior to execution</param>
        /// <param name="identity">Information about the query</param>
        protected void AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            if (templates != null)
            {
                foreach (var template in templates)
                {
                    var newIdent = identity.ForDynamicParameters(template.GetType());
                    Action<IDbCommand, object> appender;

                    lock (paramReaderCache)
                    {
                        if (!paramReaderCache.TryGetValue(newIdent, out appender))
                        {
                            appender = SqlMapper.CreateParamInfoGenerator(newIdent);
                            paramReaderCache[newIdent] = appender;
                        }
                    }

                    appender(command, template);
                }
            }

            foreach (var param in parameters.Values)
            {
                string name = Clean(param.Name);
                bool add = !command.Parameters.Contains(name);
                IDbDataParameter p;
                if (add)
                {
                    p = command.CreateParameter();
                    p.ParameterName = name;
                }
                else
                {
                    p = (IDbDataParameter)command.Parameters[name];
                }
                var val = param.Value;
                p.Value = val ?? DBNull.Value;
                p.Direction = param.ParameterDirection;
                var s = val as string;
                if (s != null)
                {
                    if (s.Length <= 4000)
                    {
                        p.Size = 4000;
                    }
                }
                if (param.Size != null)
                {
                    p.Size = param.Size.Value;
                }
                if (param.DbType != null)
                {
                    p.DbType = param.DbType.Value;
                }
                if (add)
                {
                    command.Parameters.Add(p);
                }
                param.AttachedParam = p;
            }
        }

        /// <summary>
        /// All the names of the param in the bag, use Get to yank them out
        /// </summary>
        public IEnumerable<string> ParameterNames
        {
            get
            {
                return parameters.Keys;
            }
        }

        /// <summary>
        /// Get the value of a parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns>The value, note DBNull.Value is not returned, instead the value is returned as null</returns>
        public T Get<T>(string name)
        {
            var val = parameters[Clean(name)].AttachedParam.Value;
            if (val == DBNull.Value)
            {
                if (default(T) != null)
                {
                    throw new ApplicationException("Attempting to cast a DBNull to a non nullable type!");
                }
                return default(T);
            }
            return (T)val;
        }
    }

    /// <summary>
    /// This class represents a SQL string, it can be used if you need to denote your parameter is a Char vs VarChar vs nVarChar vs nChar
    /// </summary>
    public sealed class DbString
    {
        /// <summary>
        /// Create a new DbString
        /// </summary>
        public DbString() { Length = -1; }
        /// <summary>
        /// Ansi vs Unicode 
        /// </summary>
        public bool IsAnsi { get; set; }
        /// <summary>
        /// Fixed length 
        /// </summary>
        public bool IsFixedLength { get; set; }
        /// <summary>
        /// Length of the string -1 for max
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// The value of the string
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Add the parameter to the command... internal use only
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        public void AddParameter(IDbCommand command, string name)
        {
            if (IsFixedLength && Length == -1)
            {
                throw new InvalidOperationException("If specifying IsFixedLength,  a Length must also be specified");
            }
            var param = command.CreateParameter();
            param.ParameterName = name;
            param.Value = (object)Value ?? DBNull.Value;
            if (Length == -1 && Value != null && Value.Length <= 4000)
            {
                param.Size = 4000;
            }
            else
            {
                param.Size = Length;
            }
            param.DbType = IsAnsi ? (IsFixedLength ? DbType.AnsiStringFixedLength : DbType.AnsiString) : (IsFixedLength ? DbType.StringFixedLength : DbType.String);
            command.Parameters.Add(param);
        }
    }

    /// <summary>
    /// Handles variances in features per DBMS
    /// </summary>
    public class FeatureSupport
    {
        /// <summary>
        /// Dictionary of supported features index by connection type name
        /// </summary>
        private static readonly Dictionary<string, FeatureSupport> FeatureList = new Dictionary<string, FeatureSupport>() {
				{"sqlserverconnection", new FeatureSupport { Arrays = false}},
				{"npgsqlconnection", new FeatureSupport {Arrays = true}}
		};

        /// <summary>
        /// Gets the featureset based on the passed connection
        /// </summary>
        public static FeatureSupport Get(IDbConnection connection)
        {
            string name = connection.GetType().Name.ToLower();
            FeatureSupport features;
            if (!FeatureList.TryGetValue(name, out features))
            {
                foreach (var item in FeatureList.Values)
                {
                    features = item;
                    break;
                }
            }
            return features;
        }

        /// <summary>
        /// True if the db supports array columns e.g. Postgresql
        /// </summary>
        public bool Arrays { get; set; }
    }

    /// <summary>
    /// Represents simple memeber map for one of target parameter or property or field to source DataReader column
    /// </summary>
    public sealed class SimpleMemberMap : SqlMapper.IMemberMap
    {
        private readonly string _columnName;
        private readonly PropertyInfo _property;
        private readonly FieldInfo _field;
        private readonly ParameterInfo _parameter;

        /// <summary>
        /// Creates instance for simple property mapping
        /// </summary>
        /// <param name="columnName">DataReader column name</param>
        /// <param name="property">Target property</param>
        public SimpleMemberMap(string columnName, PropertyInfo property)
        {
            if (columnName == null)
                throw new ArgumentNullException("columnName");

            if (property == null)
                throw new ArgumentNullException("property");

            _columnName = columnName;
            _property = property;
        }

        /// <summary>
        /// Creates instance for simple field mapping
        /// </summary>
        /// <param name="columnName">DataReader column name</param>
        /// <param name="field">Target property</param>
        public SimpleMemberMap(string columnName, FieldInfo field)
        {
            if (columnName == null)
                throw new ArgumentNullException("columnName");

            if (field == null)
                throw new ArgumentNullException("field");

            _columnName = columnName;
            _field = field;
        }

        /// <summary>
        /// Creates instance for simple constructor parameter mapping
        /// </summary>
        /// <param name="columnName">DataReader column name</param>
        /// <param name="parameter">Target constructor parameter</param>
        public SimpleMemberMap(string columnName, ParameterInfo parameter)
        {
            if (columnName == null)
                throw new ArgumentNullException("columnName");

            if (parameter == null)
                throw new ArgumentNullException("parameter");

            _columnName = columnName;
            _parameter = parameter;
        }

        /// <summary>
        /// DataReader column name
        /// </summary>
        public string ColumnName
        {
            get { return _columnName; }
        }

        /// <summary>
        /// Target member type
        /// </summary>
        public Type MemberType
        {
            get
            {
                if (_field != null)
                    return _field.FieldType;

                if (_property != null)
                    return _property.PropertyType;

                if (_parameter != null)
                    return _parameter.ParameterType;

                return null;
            }
        }

        /// <summary>
        /// Target property
        /// </summary>
        public PropertyInfo Property
        {
            get { return _property; }
        }

        /// <summary>
        /// Target field
        /// </summary>
        public FieldInfo Field
        {
            get { return _field; }
        }

        /// <summary>
        /// Target constructor parameter
        /// </summary>
        public ParameterInfo Parameter
        {
            get { return _parameter; }
        }
    }

    static class Converter<T>
    {
        static Converter()
        {
            Converter<Guid>.Convert = o => new Guid(o.ToString());
            Converter<TimeSpan>.Convert = o => TimeSpan.Parse(o.ToString());
        }

        public static Func<Object, T> Convert;
    }

    static class ReflectHelper
    {
        public static MethodInfo GetPropertySetter(PropertyInfo propertyInfo, Type type)
        {
            return propertyInfo.DeclaringType == type ?
                propertyInfo.GetSetMethod(true) :
                propertyInfo.DeclaringType.GetProperty(propertyInfo.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetSetMethod(true);
        }
    }
}
