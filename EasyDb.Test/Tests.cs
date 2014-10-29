using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Dapper;
using LX.EasyDb.Criterion;
#if !NET20
using System.Linq;
#endif

namespace LX.EasyDb
{
    class Tests
    {
        IConnection connection = Program.GetOpenConnection();
        String castIntegerType = "integer";
        String lengthFunc = "length";

        public Tests()
        {
            if (Program.factory.Dialect is Dialects.MySQLDialect)
            {
                castIntegerType = "signed";
            }
            else if (Program.factory.Dialect is Dialects.SQLServerDialect)
            {
                lengthFunc = "datalength";
            }
        }

        class Parent
        {
            public int Id { get; set; }
            public readonly List<Child> Children = new List<Child>();
        }
        class Child
        {
            public int Id { get; set; }
        }
        
#if !NET20
        public void ParentChildIdentityAssociations()
        {
            var lookup = new Dictionary<int, Parent>();
            var parents = connection.Query<Parent, Child, Parent>(String.Format(@"select 1 as {0}Id{1}, 1 as {0}Id{1} union all select 1,2 union all select 2,3 union all select 1,4 union all select 3,5", Program.factory.Dialect.OpenQuote, Program.factory.Dialect.CloseQuote),
                (parent, child) =>
                {
                    Parent found;
                    if (!lookup.TryGetValue(parent.Id, out found))
                    {
                        lookup.Add(parent.Id, found = parent);
                    }
                    found.Children.Add(child);
                    return found;
                }).Distinct().ToDictionary(p => p.Id);
            Assert.IsEqualTo(parents.Count(), 3);
            Assert.IsEqualTo(parents[1].Children.Select(c => c.Id).SequenceEqual(new[] { 1, 2, 4 }), true);
            Assert.IsEqualTo(parents[2].Children.Select(c => c.Id).SequenceEqual(new[] { 3 }), true);
            Assert.IsEqualTo(parents[3].Children.Select(c => c.Id).SequenceEqual(new[] { 5 }), true);
        }
#endif

        [Mapping.Table(Name = "User_1")]
        class User
        {
            [Mapping.Column(DbType = DbType.Identity)]
            [Mapping.PrimaryKey]
            public Int64 id { get; set; }
            public String username { get; set; }
        }

        class User2
        {
            public Int64 id { get; set; }
            public String username { get; set; }
        }

        public void TestPhantomMapping()
        {
            // Maps User2 with mappings defined in class User.
            Program.factory.Mapping.Phantom(typeof(User2), typeof(User));

            if (!connection.ExistTable<User2>())
                connection.CreateTable<User2>();
            Int64 id = connection.Insert<User2>(new User2() { username = "phantom" });


            User2 u = Enumerable.Single(connection.Query<User2>("select * from User_1 where username = @username", new { username = "phantom" }));
            Assert.IsEqualTo(u.username, "phantom");
            Assert.IsEqualTo(u.id, id);

            u = connection.Get<User2>(id);
            Assert.IsEqualTo(u.username, "phantom");
            Assert.IsEqualTo(u.id, id);

            connection.DropTable<User2>();
        }

        public void TestCreateTable()
        {
            Boolean gotException = false;

            try
            {
                connection.CreateTable<User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                gotException = true;
            }
            Assert.IsEqualTo(gotException, false);

            try
            {
                connection.DropTable<User>();
            }
            catch (Exception)
            {
                gotException = true;
            }
            Assert.IsEqualTo(gotException, false);
        }

        public void TestCriteria()
        {
            if (!connection.ExistTable<User>())
                connection.CreateTable<User>();

            Int64 id = connection.Insert<User>(new User() { username = "user" });
            connection.Insert<User>(new User() { username = "user" });
            connection.Insert<User>(new User() { username = "user" });

            ICriteria<User> criteria = connection.CreateCriteria<User>();
            criteria.Add(Clauses.Eq("id", id));
            User user = criteria.SingleOrDefault();
            Assert.IsEqualTo(user.username, "user");
            Assert.IsEqualTo(user.id, id);

            criteria = connection.CreateCriteria<User>();
            criteria.SetProjection(Projections.List()
                .Add(Projections.CountDistinct("id"), "id")
                .Add(Projections.GroupProperty("username")));
            user = criteria.SingleOrDefault();
            Assert.IsEqualTo(true, user.id >= 3);

            id = connection.Insert<User>(new User() { username = "user2" });
            criteria = connection.CreateCriteria<User>();
            criteria.SetProjection(Projections.List()
                .Add(Projections.Expression(Clauses.Mod("id", id)), "id")
                .Add(Projections.Property("username")))
                .Add(Clauses.Eq("username", "user2"));
            user = criteria.SingleOrDefault();
            Assert.IsEqualTo(user.username, "user2");
            Assert.IsEqualTo(user.id, 0U);

            //ICriteria c1 = connection.CreateCriteria<Int32>().SetProjection(Projections.RowCount());
            //Assert.IsEqualTo(c1.SingleOrDefault(), 1);

            connection.DropTable<User>();
        }

        public void TestMultiThreadConnection()
        {
            IConnection conn1 = Program.GetOpenConnection();
            IConnection conn2 = Program.GetOpenConnection();
            IConnection conn3 = null;

            System.Threading.ThreadPool.QueueUserWorkItem(delegate(Object state) {
                conn3 = Program.GetOpenConnection();
            });

            System.Threading.Thread.Sleep(100);
            Assert.IsNotEqualTo(conn1, conn2);
            Assert.IsNotEqualTo(conn1, conn3);
        }

        public void TestNestedTransactions()
        {
            IConnection conn1 = Program.GetOpenConnection();
            IConnection conn2 = Program.GetOpenConnection();

            try { conn1.ExecuteNonQuery("drop table TransactionTest;"); }
            catch { }

            try
            {
                conn1.ExecuteNonQuery("create table TransactionTest (ID integer, Value varchar(32));");

                conn1.BeginTransaction();

                try
                {
                    //conn2.ExecuteNonQuery("create table TransactionTest (ID integer, Value varchar(32));");

                    conn2.BeginTransaction();
                    conn2.ExecuteNonQuery("insert into TransactionTest (ID, Value) values (1, 'ABC');");
                    conn2.CommitTransaction();

                    Assert.IsEqualTo(Enumerable.Single(connection.Query<int>("select count(*) from TransactionTest;")), 1);
                }
                finally
                {
                    //conn2.ExecuteNonQuery("drop table TransactionTest;");
                }

                conn1.ExecuteNonQuery("insert into TransactionTest (ID, Value) values (1, 'ABC');");
                conn1.CommitTransaction();

                Assert.IsEqualTo(Enumerable.Single(connection.Query<int>("select count(*) from TransactionTest;")), 2);
            }
            finally
            {
                conn1.ExecuteNonQuery("drop table TransactionTest;");
            }
        }

        public void TestThreadLocalConnection()
        {
            IConnection conn1 = Program.GetOpenConnection();

            try { conn1.ExecuteNonQuery("drop table TransactionTest;"); }
            catch { }

            try
            {
                conn1.ExecuteNonQuery("create table TransactionTest (ID integer, Value varchar(32));");

                conn1.BeginTransaction();

                conn1.ExecuteNonQuery("insert into TransactionTest (ID, Value) values (1, 'ABC');");
                conn1.CommitTransaction();

                Assert.IsEqualTo(Enumerable.Single(connection.Query<int>("select count(*) from TransactionTest;")), 1);
            }
            finally
            {
                conn1.ExecuteNonQuery("drop table TransactionTest;");
            }

            conn1.Close();

            IConnection conn2 = Program.GetOpenConnection();
            try
            {
                conn2.ExecuteNonQuery("create table TransactionTest (ID integer, Value varchar(32));");

                conn2.BeginTransaction();
                conn2.ExecuteNonQuery("insert into TransactionTest (ID, Value) values (1, 'ABC');");
                conn2.CommitTransaction();

                Assert.IsEqualTo(Enumerable.Single(connection.Query<int>("select count(*) from TransactionTest;")), 1);
            }
            finally
            {
                conn2.ExecuteNonQuery("drop table TransactionTest;");
            }
        }

        #region Test constructors

        public void TestMultipleConstructors()
        {
            MultipleConstructors value = Enumerable.FirstOrDefault(connection.Query<MultipleConstructors>("select 0 A, 'Easy' b"));
            Assert.IsEqualTo(value.A, 0);
            Assert.IsEqualTo(value.B, "Easy");
        }

        public void TestConstructorsWithAccessModifiers()
        {
            ConstructorsWithAccessModifiers value = Enumerable.FirstOrDefault(connection.Query<ConstructorsWithAccessModifiers>("select 0 A, 'Easy' b"));
            Assert.IsEqualTo(value.A, 1);
            Assert.IsEqualTo(value.B, "Easy!");
        }

        public void TestNoDefaultConstructor()
        {
            var guid = Guid.NewGuid();
            NoDefaultConstructor nodef = Enumerable.First(connection.Query<NoDefaultConstructor>("select CAST(NULL AS integer) A1, CAST(NULL AS integer) b1, CAST(NULL AS real) f1, 'Easy' s1, @id G1", new { Id = guid }));
            Assert.IsEqualTo(nodef.A, 0);
            Assert.IsEqualTo(nodef.B, null);
            Assert.IsEqualTo(nodef.F, 0);
            Assert.IsEqualTo(nodef.S, "Easy");
            Assert.IsEqualTo(nodef.G, guid);
        }

        public void TestNoDefaultConstructorWithChar()
        {
            const char c1 = 'ą';
            const char c3 = 'ó';
            NoDefaultConstructorWithChar nodef = Enumerable.FirstOrDefault(connection.Query<NoDefaultConstructorWithChar>("select @c1 c1, @c2 c2, @c3 c3", new { c1 = c1, c2 = (char?)null, c3 = c3 }));
            Assert.IsEqualTo(nodef.Char1, c1);
            Assert.IsEqualTo(nodef.Char2, null);
            Assert.IsEqualTo(nodef.Char3, c3);
        }

        public void TestNoDefaultConstructorWithEnum()
        {
            NoDefaultConstructorWithEnum nodef = Enumerable.First(connection.Query<NoDefaultConstructorWithEnum>("select cast(2 as smallint) E1, cast(5 as smallint) n1, cast(null as smallint) n2"));
            Assert.IsEqualTo(nodef.E, ShortEnum.Two);
            Assert.IsEqualTo(nodef.NE1, ShortEnum.Five);
            Assert.IsEqualTo(nodef.NE2, null);
        }

        #endregion

        public void TestAbstractInheritance()
        {
            var order = Enumerable.First(connection.Query<AbstractInheritance.ConcreteOrder>("select 1 Internal,2 Protected,3 PublicVal,4 Concrete"));

            Assert.IsEqualTo(order.Internal, 1);
            Assert.IsEqualTo(order.ProtectedVal, 2);
            Assert.IsEqualTo(order.PublicVal, 3);
            Assert.IsEqualTo(order.Concrete, 4);
        }

        public void TestListOfAnsiStrings()
        {
            var results = connection.Query<string>("select * from (select 'a' str union select 'b' union select 'c') X where str in @strings",
                new { strings = new[] { new DbString { IsAnsi = true, Value = "a" }, new DbString { IsAnsi = true, Value = "b" } } });

            Int32 i = 0;
            String[] values = new[] { "a", "b" };
            foreach (var item in results)
            {
                Assert.IsEqualTo(item, values[i++]);
            }
        }

        public void TestNullableGuidSupport()
        {
            var guid = Enumerable.FirstOrDefault(connection.Query<Guid?>("select null"));
            Assert.IsNull(guid);

            guid = Guid.NewGuid();
            var guid2 = Enumerable.FirstOrDefault(connection.Query<Guid?>("select @guid", new { guid }));
            Assert.IsEqualTo(guid, guid2);
        }

        public void TestNonNullableGuidSupport()
        {
            var guid = Guid.NewGuid();
            var guid2 = Enumerable.FirstOrDefault(connection.Query<Guid>("select @guid", new { guid }));
            Assert.IsEqualTo(guid, guid2);
        }

        public void TestStructs()
        {
            var car = Enumerable.First(connection.Query<Car>("select 'Ford' Name, 21 Age, 2 Trap"));
            Assert.IsEqualTo(car.Age, 21);
            Assert.IsEqualTo(car.Name, "Ford");
            Assert.IsEqualTo((int)car.Trap, 2);
        }

        public void TestEnumWithWrongTypes()
        {
            var car = Enumerable.FirstOrDefault(connection.Query<Car>("select 2 Trap"));

            Assert.IsEqualTo(((int)car.Trap), 2);
        }

        public void TestEmptyClass()
        {
            var empty = Enumerable.FirstOrDefault(connection.Query<EmptyClass>("select null"));
            Assert.IsNotNull(empty);
        }

        public void SelectListInt()
        {
            Assert.IsSequenceEqualTo(connection.Query<int>("select 1 union all select 2 union all select 3"), new[] { 1, 2, 3 });
        }

        public void SelectBinary()
        {
            Assert.IsSequenceEqualTo(Enumerable.First(connection.Query<byte[]>("select cast(1 as binary(4))")), new byte[4] { 0, 0, 0, 1 });
        }

        public void PassInIntArray()
        {
            Assert.IsSequenceEqualTo(connection.Query<int>("select * from (select 1 as Id union all select 2 union all select 3) as X where Id in @Ids", new { Ids = new int[] { 1, 2, 3 } })
             , new[] { 1, 2, 3 });
        }

        public void PassInEmptyIntArray()
        {
            Assert.IsSequenceEqualTo(connection.Query<int>("select * from (select 1 as Id union all select 2 union all select 3) as X where Id in @Ids", new { Ids = new int[0] })
             , new int[0]);
        }

        public void TestSchemaChanged()
        {
            try
            {
                connection.ExecuteNonQuery("drop table dog");
            }
            catch { }
            connection.ExecuteNonQuery("create table dog(Age int, Name nvarchar(32))");
            connection.ExecuteNonQuery("insert into dog values(1, 'Alf')");

            var d = Enumerable.Single(connection.Query<Dog>("select * from dog"));
            Assert.IsEqualTo(d.Name, "Alf");
            Assert.IsEqualTo(d.Age, 1);
            connection.ExecuteNonQuery("alter table dog drop column Name");
            d = Enumerable.Single(connection.Query<Dog>("select * from dog"));
            Assert.IsNull(d.Name);
            Assert.IsEqualTo(d.Age, 1);
            connection.ExecuteNonQuery("drop table dog");
        }

        public void TestExtraFields()
        {
            var guid = Guid.NewGuid();
            var dog = Enumerable.ToList(connection.Query<Dog>("select '' as Extra, 1 as Age, 0.1 as Name1 , @id as Id", new { id = guid }));

            Assert.IsEqualTo(dog.Count, 1);
            Assert.IsEqualTo(dog[0].Age, 1);
            Assert.IsEqualTo(dog[0].Id, guid);
        }

        public void TestStrongType()
        {
            var guid = Guid.NewGuid();
            var dog = Enumerable.ToList(connection.Query<Dog>("select @Age Age, @Id Id", new { Age = (int?)null, Id = guid }));

            Assert.IsEqualTo(dog.Count, 1);
            Assert.IsNull(dog[0].Age);
            Assert.IsEqualTo(dog[0].Id, guid);
        }

        public void TestSimpleNull()
        {
            Assert.IsNull(Enumerable.FirstOrDefault(connection.Query<DateTime?>("select null")));
        }

        public void TestStringList()
        {
            Assert.IsSequenceEqualTo(connection.Query<string>("select * from (select 'a' as x union all select 'b' union all select 'c') as T where x in @strings", new { strings = new[] { "a", "b", "c" } })
                , new[] { "a", "b", "c" });

            Assert.IsSequenceEqualTo(connection.Query<string>("select * from (select 'a' as x union all select 'b' union all select 'c') as T where x in @strings", new { strings = new string[0] })
                   , new string[0]);
        }

        #region Test executing SQL

        public void TestExecuteCommand()
        {
            try
            {
                connection.ExecuteNonQuery("drop table t");
            }
            catch { }
            connection.ExecuteNonQuery("create table t(i int)");
            Assert.IsEqualTo(connection.ExecuteNonQuery("insert into t select @a a union all select @b", new { a = 1, b = 2 }), 2);
            connection.ExecuteNonQuery("drop table t");
        }

        public void TestExecuteCommandWithHybridParameters()
        {
            var p = new DynamicParameters(new { a = 1, b = 2 });
            p.Add("c", null, System.Data.DbType.Int32, ParameterDirection.Output, null);
            connection.ExecuteNonQuery(@"set @c = @a + @b", p);
            Assert.IsEqualTo(p.Get<int>("@c"), 3);
        }

        public void TestExecuteMultipleCommand()
        {
            try
            {
                connection.ExecuteNonQuery("drop table t");
            }
            catch { }
            connection.ExecuteNonQuery("create table t(i int)");
            int tally = connection.ExecuteNonQuery(@"insert into t (i) values(@a)", new[] { new { a = 1 }, new { a = 2 }, new { a = 3 }, new { a = 4 } });
            int sum = Enumerable.FirstOrDefault(connection.Query<int>("select sum(i) from t"));
            connection.ExecuteNonQuery("drop table t");
            Assert.IsEqualTo(tally, 4);
            Assert.IsEqualTo(sum, 10);
        }

        public void TestExecuteMultipleCommandStrongType()
        {
            try
            {
                connection.ExecuteNonQuery("drop table t");
            }
            catch { }
            connection.ExecuteNonQuery("create table t(Name nvarchar(32), Age int)");
            int tally = connection.ExecuteNonQuery(@"insert into t (Name,Age) values(@Name, @Age)", new List<Student> 
            {
                new Student{Age = 1, Name = "sam"},
                new Student{Age = 2, Name = "bob"}
            });
            int sum = Enumerable.FirstOrDefault(connection.Query<int>("select sum(Age) from t"));
            connection.ExecuteNonQuery("drop table t");
            Assert.IsEqualTo(tally, 2);
            Assert.IsEqualTo(sum, 3);
        }

        public void TestExecuteMultipleCommandObjectArray()
        {
            try
            {
                connection.ExecuteNonQuery("drop table t");
            }
            catch { }
            connection.ExecuteNonQuery("create table t(i int)");
            int tally = connection.ExecuteNonQuery(@"insert into t (i) values(@a)", new object[] { new { a = 1 }, new { a = 2 }, new { a = 3 }, new { a = 4 } });
            int sum = Enumerable.FirstOrDefault(connection.Query<int>("select sum(i) from t"));
            connection.ExecuteNonQuery("drop table t");
            Assert.IsEqualTo(tally, 4);
            Assert.IsEqualTo(sum, 10);
        }

        public void TestSupportForDynamicParameters()
        {
            var p = new DynamicParameters();
            p.Add("name", "bob", null, null, null);
            p.Add("age", null, System.Data.DbType.Int32, ParameterDirection.Output, null);

            Assert.IsEqualTo(Enumerable.FirstOrDefault(connection.Query<string>("set @age = 11; select @name;", p)), "bob");

            Assert.IsEqualTo(p.Get<int>("age"), 11);
        }

        #endregion

        public void TestMassiveStrings()
        {
            var str = new string('X', 20000);
            Assert.IsEqualTo(Enumerable.FirstOrDefault(connection.Query<string>("select @a", new { a = str })), str);
        }

        public void TestNullableProperty()
        {
            var nullable = Enumerable.FirstOrDefault(connection.Query<NullableProperty>("select 3 as isNotNull, 1 as nullable, 2 as NE2"));
            Assert.IsEqualTo(nullable.isNotNull, 3);
            Assert.IsEqualTo(nullable.nullable, 1);
            Assert.IsEqualTo(nullable.NE2, ShortEnum.Two);
        }

        #region Test params

        public void TestDoubleParam()
        {
            Assert.IsEqualTo(Enumerable.FirstOrDefault(connection.Query<double>("select @d", new { d = 0.1d })), 0.1d);
        }

        public void TestBoolParam()
        {
            Assert.IsEqualTo(Enumerable.FirstOrDefault(connection.Query<bool>("select @b", new { b = false })), false);
        }

        // http://code.google.com/p/dapper-dot-net/issues/detail?id=70
        // https://connect.microsoft.com/VisualStudio/feedback/details/381934/sqlparameter-dbtype-dbtype-time-sets-the-parameter-to-sqldbtype-datetime-instead-of-sqldbtype-time
        public void TestTimeSpanParam()
        {
            Assert.IsEqualTo(Enumerable.FirstOrDefault(connection.Query<TimeSpan>("select @ts", new { ts = TimeSpan.FromMinutes(42) })), TimeSpan.FromMinutes(42));
        }

        public void TestEnumParamsWithNullable()
        {
            EnumParam a = EnumParam.A;
            EnumParam? b = EnumParam.B, c = null;
            var obj = Enumerable.Single(connection.Query<EnumParamObject>("select @a as A, @b as B, @c as C", new { a, b, c }));
            Assert.IsEqualTo(obj.A, EnumParam.A);
            Assert.IsEqualTo(obj.B, EnumParam.B);
            Assert.IsEqualTo(obj.C, null);
        }

        public void TestEnumParamsWithoutNullable()
        {
            EnumParam a = EnumParam.A;
            EnumParam b = EnumParam.B, c = 0;
            var obj = Enumerable.Single(connection.Query<EnumParamObjectNonNullable>("select @a as A, @b as B, @c as C", new { a, b, c }));
            Assert.IsEqualTo(obj.A, EnumParam.A);
            Assert.IsEqualTo(obj.B, EnumParam.B);
            Assert.IsEqualTo(obj.C, (EnumParam)0);
        }

        public void TestNakedBigInt()
        {
            long foo = 12345;
            var result = Enumerable.Single(connection.Query<long>("select @foo", new { foo }));
            Assert.IsEqualTo(foo, result);
        }

        public void TestDbString()
        {
            var obj = Enumerable.First(connection.QueryDirect(String.Format("select {0}(@a) as a, {0}(@c) as c, {0}(@b) as b, {0}(@e) as e, {0}(@d) as d, {0}(@f) as f", lengthFunc),
                new
                {
                    a = new DbString { Value = "abcde", IsFixedLength = true, Length = 10, IsAnsi = true },
                    b = new DbString { Value = "abcde", IsFixedLength = true, Length = 10, IsAnsi = false },
                    c = new DbString { Value = "abcde", IsFixedLength = false, Length = 10, IsAnsi = true },
                    d = new DbString { Value = "abcde", IsFixedLength = false, Length = 10, IsAnsi = false },
                    e = new DbString { Value = "abcde", IsAnsi = true },
                    f = new DbString { Value = "abcde", IsAnsi = false },
                }));

            Assert.IsEqualTo(obj["a"], 10);
            Assert.IsEqualTo(obj["b"], 20);
            Assert.IsEqualTo(obj["c"], 5);
            Assert.IsEqualTo(obj["d"], 10);
            Assert.IsEqualTo(obj["e"], 5);
            Assert.IsEqualTo(obj["f"], 10);
        }

        public void TestFastExpandoSupportsIDictionary()
        {
            var row = Enumerable.FirstOrDefault(connection.QueryDirect("select 1 A, 'two' B"));
            Assert.IsEqualTo(Convert.ToInt32(row["A"]), 1);
            Assert.IsEqualTo(row["B"], "two");
        }

        class WithBizarreData
        {
            public GenericUriParser Foo { get; set; }
            public int Bar { get; set; }
        }
        public void TestUnexpectedDataMessage()
        {
            string msg = null;
            try
            {
                Enumerable.First(connection.Query<int>("select count(1) where 1 = @Foo", new WithBizarreData { Foo = new GenericUriParser(GenericUriParserOptions.Default), Bar = 23 }));
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            Assert.IsEqualTo(msg, "The member Foo of type System.GenericUriParser cannot be used as a parameter value");
        }
        public void TestUnexpectedButFilteredDataMessage()
        {
            int i = Enumerable.Single(connection.Query<int>("select @Bar", new WithBizarreData { Foo = new GenericUriParser(GenericUriParserOptions.Default), Bar = 23 }));

            Assert.IsEqualTo(i, 23);
        }

        class WithCharValue
        {
            public char Value { get; set; }
            public char? ValueNullable { get; set; }
        }
        public void TestCharInputAndOutput()
        {
            const char test = '〠';
            char c = Enumerable.Single(connection.Query<char>("select @c", new { c = test }));

            Assert.IsEqualTo(c, test);

            var obj = Enumerable.Single(connection.Query<WithCharValue>("select @Value as Value", new WithCharValue { Value = c }));

            Assert.IsEqualTo(obj.Value, test);
        }
        public void TestNullableCharInputAndOutputNonNull()
        {
            char? test = '〠';
            char? c = Enumerable.Single(connection.Query<char?>("select @c", new { c = test }));

            Assert.IsEqualTo(c, test);

            var obj = Enumerable.Single(connection.Query<WithCharValue>("select @ValueNullable as ValueNullable", new WithCharValue { ValueNullable = c }));

            Assert.IsEqualTo(obj.ValueNullable, test);
        }
        public void TestNullableCharInputAndOutputNull()
        {
            char? test = null;
            char? c = Enumerable.Single(connection.Query<char?>("select @c", new { c = test }));

            Assert.IsEqualTo(c, test);

            var obj = Enumerable.Single(connection.Query<WithCharValue>("select @ValueNullable as ValueNullable", new WithCharValue { ValueNullable = c }));

            Assert.IsEqualTo(obj.ValueNullable, test);
        }
        public void TestInvalidSplitCausesNiceError()
        {
            //try
            //{
            //    connection.Query<User, User, User>("select 1 A, 2 B, 3 C", (x, y) => x);
            //}
            //catch (ArgumentException)
            //{
            //    // expecting an app exception due to multi mapping being bodged 
            //}

            //try
            //{
            //    connection.Query<dynamic, dynamic, dynamic>("select 1 A, 2 B, 3 C", (x, y) => x);
            //}
            //catch (ArgumentException)
            //{
            //    // expecting an app exception due to multi mapping being bodged 
            //}
        }

        public void TestAppendingAnonClasses()
        {
            DynamicParameters p = new DynamicParameters();
            p.AddDynamicParams(new { A = 1, B = 2 });
            p.AddDynamicParams(new { C = 3, D = 4 });

            var result = Enumerable.Single(connection.QueryDirect("select @A a,@B b,@C c,@D d", p));

            Assert.IsEqualTo(Convert.ToInt32(result["a"]), 1);
            Assert.IsEqualTo(Convert.ToInt32(result["b"]), 2);
            Assert.IsEqualTo(Convert.ToInt32(result["c"]), 3);
            Assert.IsEqualTo(Convert.ToInt32(result["d"]), 4);
        }

        public void TestAppendingADictionary()
        {
            var dictionary = new Dictionary<string, object>();
            dictionary.Add("A", 1);
            dictionary.Add("B", "two");

            DynamicParameters p = new DynamicParameters();
            p.AddDynamicParams(dictionary);

            var result = Enumerable.Single(connection.QueryDirect("select @A a, @B b", p));

            Assert.IsEqualTo(Convert.ToInt32(result["a"]), 1);
            Assert.IsEqualTo(result["b"], "two");
        }

        public void TestAppendingAList()
        {
            DynamicParameters p = new DynamicParameters();
            var list = new int[] { 1, 2, 3 };
            p.AddDynamicParams(new { list });

            var result = Enumerable.ToList(connection.Query<int>("select * from (select 1 A union all select 2 union all select 3) X where A in @list", p));

            Assert.IsEqualTo(result[0], 1);
            Assert.IsEqualTo(result[1], 2);
            Assert.IsEqualTo(result[2], 3);
        }

        public void TestAppendingAListAsDictionary()
        {
            DynamicParameters p = new DynamicParameters();
            var list = new int[] { 1, 2, 3 };
            var args = new Dictionary<string, object>();
            args.Add("ids", list);
            p.AddDynamicParams(args);

            var result = Enumerable.ToList(connection.Query<int>("select * from (select 1 A union all select 2 union all select 3) X where A in @ids", p));

            Assert.IsEqualTo(result[0], 1);
            Assert.IsEqualTo(result[1], 2);
            Assert.IsEqualTo(result[2], 3);
        }

        public void TestAppendingAListByName()
        {
            DynamicParameters p = new DynamicParameters();
            var list = new int[] { 1, 2, 3 };
            p.Add("ids", list, null, null, null);

            var result = Enumerable.ToList(connection.Query<int>("select * from (select 1 A union all select 2 union all select 3) X where A in @ids", p));

            Assert.IsEqualTo(result[0], 1);
            Assert.IsEqualTo(result[1], 2);
            Assert.IsEqualTo(result[2], 3);
        }

        #endregion

        public void TestStrings()
        {
            Assert.IsSequenceEqualTo(connection.Query<string>(@"select 'a' a union select 'b'"), new[] { "a", "b" });
        }

        public void TestSetInternal()
        {
            Assert.IsEqualTo(Enumerable.FirstOrDefault(connection.Query<TestObj>("select 10 as Internal"))._internal, 10);
        }

        public void TestSetPrivate()
        {
            Assert.IsEqualTo(Enumerable.FirstOrDefault(connection.Query<TestObj>("select 10 as Priv"))._priv, 10);
        }

        public void TestEnumeration()
        {
            var en = connection.Query<int>("select 1 as one union all select 2 as one", null, false);
            var i = en.GetEnumerator();
            i.MoveNext();

            bool gotException = false;
            try
            {
                var x = Enumerable.FirstOrDefault(connection.Query<int>("select 1 as one", null));
            }
            catch (Exception)
            {
                gotException = true;
            }

            while (i.MoveNext())
            { }

            // should not exception, since enumertated
            en = connection.Query<int>("select 1 as one", null, false);

            Assert.IsEqualTo(gotException, true);
        }

        public void TestInheritance()
        {
            // Test that inheritance works.
            var obj = Enumerable.FirstOrDefault(connection.Query<InheritanceTest2>("select 'One' as Derived1, 'Two' as Derived2, 'Three' as Base1, 'Four' as Base2"));
            Assert.IsEqualTo(obj.Derived1, "One");
            Assert.IsEqualTo(obj.Derived2, "Two");
            Assert.IsEqualTo(obj.Base1, "Three");
            Assert.IsEqualTo(obj.Base2, "Four");
        }

        public class PostCE
        {
            public int ID { get; set; }
            public string Title { get; set; }
            public string Body { get; set; }

            public AuthorCE Author { get; set; }
        }

        public class AuthorCE
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        public void MultiRSSqlCE()
        {
            if (File.Exists("Test.sdf"))
                File.Delete("Test.sdf");

            var cnnStr = "Data Source = Test.sdf;";
            //var engine = new SqlCeEngine(cnnStr);
            //engine.CreateDatabase();

            //using (var cnn = new SqlCeConnection(cnnStr))
            //{
            //    cnn.Open();

            //    cnn.Execute("create table Posts (ID int, Title nvarchar(50), Body nvarchar(50), AuthorID int)");
            //    cnn.Execute("create table Authors (ID int, Name nvarchar(50))");

            //    cnn.Execute("insert Posts values (1,'title','body',1)");
            //    cnn.Execute("insert Posts values(2,'title2','body2',null)");
            //    cnn.Execute("insert Authors values(1,'sam')");

            //    var data = cnn.Query<PostCE, AuthorCE, PostCE>(@"select * from Posts p left join Authors a on a.ID = p.AuthorID", (post, author) => { post.Author = author; return post; }).ToList();
            //    var firstPost = data.First();
            //    firstPost.Title.IsEqualTo("title");
            //    firstPost.Author.Name.IsEqualTo("sam");
            //    data[1].Author.IsNull();
            //    cnn.Close();
            //}
        }

        enum TestEnum : byte
        {
            Bla = 1
        }
        class TestEnumClass
        {
            public TestEnum? EnumEnum { get; set; }
        }
        class TestEnumClassNoNull
        {
            public TestEnum EnumEnum { get; set; }
        }

        public void TestEnumWeirdness()
        {
            Assert.IsEqualTo(Enumerable.FirstOrDefault(connection.Query<TestEnumClass>("select null as EnumEnum")).EnumEnum, null);
            Assert.IsEqualTo(Enumerable.FirstOrDefault(connection.Query<TestEnumClass>("select 1 as EnumEnum")).EnumEnum, TestEnum.Bla);
        }

        public void TestEnumStrings()
        {
            Assert.IsEqualTo(Enumerable.FirstOrDefault(connection.Query<TestEnumClassNoNull>("select 'BLA' as EnumEnum")).EnumEnum, TestEnum.Bla);
            Assert.IsEqualTo(Enumerable.FirstOrDefault(connection.Query<TestEnumClassNoNull>("select 'bla' as EnumEnum")).EnumEnum, TestEnum.Bla);

            Assert.IsEqualTo(Enumerable.FirstOrDefault(connection.Query<TestEnumClass>("select 'BLA' as EnumEnum")).EnumEnum, TestEnum.Bla);
            Assert.IsEqualTo(Enumerable.FirstOrDefault(connection.Query<TestEnumClass>("select 'bla' as EnumEnum")).EnumEnum, TestEnum.Bla);
        }

        #region Test transaction
        
        public void TestTransactionCommit()
        {
            try
            {
                connection.ExecuteNonQuery("create table TransactionTest (ID integer, Value varchar(32));");

                connection.BeginTransaction();
                connection.ExecuteNonQuery("insert into TransactionTest (ID, Value) values (1, 'ABC');");
                connection.CommitTransaction();

                Assert.IsEqualTo(Enumerable.Single(connection.Query<int>("select count(*) from TransactionTest;")), 1);
            }
            finally
            {
                connection.ExecuteNonQuery("drop table TransactionTest;");
            }
        }

        public void TestTransactionRollback()
        {
            connection.ExecuteNonQuery("create table TransactionTest (ID integer, Value varchar(32));");

            try
            {
                using (var transaction = connection.BeginTransaction())
                {
                    connection.ExecuteNonQuery("insert into TransactionTest (ID, Value) values (1, 'ABC');");

                    connection.RollbackTransaction();
                }

                Assert.IsEqualTo(Enumerable.Single(connection.Query<int>("select count(*) from TransactionTest;")), 0);
            }
            finally
            {
                connection.ExecuteNonQuery("drop table TransactionTest;");
            }
        }

        public void TestCommandWithInheritedTransaction()
        {
            connection.ExecuteNonQuery("create table TransactionTest (ID integer, Value varchar(32));");

            try
            {
                using (var transaction = connection.BeginTransaction())
                {
                    var transactedConnection = new TransactedConnection(connection, transaction);

                    transactedConnection.Execute("insert into TransactionTest (ID, Value) values (1, 'ABC');");

                    connection.RollbackTransaction();
                }

                Assert.IsEqualTo(Enumerable.Single(connection.Query<int>("select count(*) from TransactionTest;")), 0);
            }
            finally
            {
                connection.ExecuteNonQuery("drop table TransactionTest;");
            }
        }
        
        public void TestReaderWhenResultsChange()
        {
            try
            {
                connection.ExecuteNonQuery("create table ResultsChange (X int);create table ResultsChange2 (Y int);insert into ResultsChange (X) values(1);insert into ResultsChange2 (Y) values(1);");

                var obj1 = Enumerable.Single(connection.Query<ResultsChangeType>("select * from ResultsChange"));
                Assert.IsEqualTo(obj1.X, 1);
                Assert.IsEqualTo(obj1.Y, 0);
                Assert.IsEqualTo(obj1.Z, 0);

                var obj2 = Enumerable.Single(connection.Query<ResultsChangeType>("select * from ResultsChange rc inner join ResultsChange2 rc2 on rc2.Y=rc.X"));
                Assert.IsEqualTo(obj2.X, 1);
                Assert.IsEqualTo(obj2.Y, 1);
                Assert.IsEqualTo(obj2.Z, 0);

                connection.ExecuteNonQuery("alter table ResultsChange add Z int null");
                connection.ExecuteNonQuery("update ResultsChange set Z = 2");

                var obj3 = Enumerable.Single(connection.Query<ResultsChangeType>("select * from ResultsChange"));
                Assert.IsEqualTo(obj3.X, 1);
                Assert.IsEqualTo(obj3.Y, 0);
                Assert.IsEqualTo(obj3.Z, 2);

                var obj4 = Enumerable.Single(connection.Query<ResultsChangeType>("select * from ResultsChange rc inner join ResultsChange2 rc2 on rc2.Y=rc.X"));
                Assert.IsEqualTo(obj4.X, 1);
                Assert.IsEqualTo(obj4.Y, 1);
                Assert.IsEqualTo(obj4.Z, 2);
            }
            finally
            {
                connection.ExecuteNonQuery("drop table ResultsChange;drop table ResultsChange2;");
            }
        }
        class ResultsChangeType
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }
        }

        #endregion

        public class WrongTypes
        {
            public int A { get; set; }
            public double B { get; set; }
            public long C { get; set; }
            public bool D { get; set; }
        }

        public void TestWrongTypes_WithRightTypes()
        {
            var item = Enumerable.Single(connection.Query<WrongTypes>(String.Format("select 1 as A, 2.0 as B, 3 as C, cast(1 as {0}) as D", castIntegerType)));
            Assert.IsEqualTo(item.A, 1);
            Assert.IsEqualTo(item.B, 2.0);
            Assert.IsEqualTo(item.C, 3L);
            Assert.IsEqualTo(item.D, true);
        }

        public void TestWrongTypes_WithWrongTypes()
        {
            var item = Enumerable.Single(connection.Query<WrongTypes>("select 1.0 as A, 2 as B, 3 as C, 1 as D"));
            Assert.IsEqualTo(item.A, 1);
            Assert.IsEqualTo(item.B, 2.0);
            Assert.IsEqualTo(item.C, 3L);
            Assert.IsEqualTo(item.D, true);
        }

        public void Test_AddDynamicParametersRepeatedShouldWork()
        {
            var args = new DynamicParameters();
            args.AddDynamicParams(new { Foo = 123 });
            args.AddDynamicParams(new { Foo = 123 });
            int i = Enumerable.Single(connection.Query<int>("select @Foo", args));
            Assert.IsEqualTo(i, 123);
        }

        public class ParameterWithIndexer
        {
            public int A { get; set; }
            public virtual string this[string columnName]
            {
                get { return null; }
                set { }
            }
        }

        public void TestParameterWithIndexer()
        {
            connection.ExecuteNonQuery(@"create proc #TestProcWithIndexer 
	@A int
as 
begin
	select @A
end");
            var item = Enumerable.Single(connection.Query<int>("#TestProcWithIndexer", new ParameterWithIndexer(), commandType: CommandType.StoredProcedure));
        }


        [Mapping.Table(Name = "sample_users_s")]
        struct UserStruct
        {
            public Int32 id { get; set; }
            public String username { get; set; }
            public String password { get; set; }
        }

        public void TestInsertStruct()
        {
            var user = Enumerable.Single(connection.Query<UserStruct>("select @username as username, @password as password", new UserStruct { username = "un", password = "pwd" }));
            Assert.IsEqualTo(user.username, "un");
            Assert.IsEqualTo(user.password, "pwd");
        }

        public void TestFindAndSingle()
        {
            String sql = "select 1 id union all select 1 union all select 1";
            IDictionary<String, Object> result = Enumerable.First(connection.QueryDirect(sql));
            Assert.IsEqualTo(Convert.ToInt32(result["id"]), 1);

            Exception ex = null;
            try
            {
                result = Enumerable.Single(connection.QueryDirect(sql));
            }
            catch (Exception e)
            {
                ex = e;
            }
            Assert.IsNotNull(ex);
            Assert.IsEqualTo(ex.Message, "The enumerable source has more than one element.");
        }

        class MultiPrimaryKey
        {
            [Mapping.PrimaryKey]
            public Int32 Key1 { get; set; }
            [Mapping.PrimaryKey]
            public Int32 Key2 { get; set; }
        }

        public void TestMultiPrimaryKey()
        {
            if (connection.ExistTable<MultiPrimaryKey>())
                connection.DropTable<MultiPrimaryKey>();
            connection.CreateTable<MultiPrimaryKey>();
            connection.Insert<MultiPrimaryKey>(new MultiPrimaryKey { Key1 = 1, Key2 = 2 });
            connection.Insert<MultiPrimaryKey>(new MultiPrimaryKey { Key1 = 3, Key2 = 4 });
            Exception ex = null;
            MultiPrimaryKey ret = null;
            try
            {
                ret = connection.Get<MultiPrimaryKey>(1);
            }
            catch (Exception e)
            {
                ex = e;
            }
            Assert.IsNotNull(ex);
            Assert.IsEqualTo(ex.Message, String.Format("The type {0} has more than one primary-key property and cannot be queried by a single id value.", typeof(MultiPrimaryKey).FullName));

            ret = connection.Get<MultiPrimaryKey>(new { Key1 = 3, Key2 = 4 });
            Assert.IsEqualTo(ret.Key1, 3);
            Assert.IsEqualTo(ret.Key2, 4);

            connection.DropTable<MultiPrimaryKey>();
        }

        public void TestFindAndGet()
        {
            if (connection.ExistTable<MultiPrimaryKey>())
                connection.DropTable<MultiPrimaryKey>();
            connection.CreateTable<MultiPrimaryKey>();
            connection.Insert<MultiPrimaryKey>(new MultiPrimaryKey { Key1 = 1, Key2 = 2 });

            Exception ex = null;
            MultiPrimaryKey ret = null;
            try
            {
                ret = connection.Find<MultiPrimaryKey>(new { Key1 = 3, Key2 = 4 });
            }
            catch (Exception e)
            {
                ex = e;
            }
            Assert.IsNull(ex);

            try
            {
                ret = connection.Get<MultiPrimaryKey>(new { Key1 = 3, Key2 = 4 });
            }
            catch (Exception e)
            {
                ex = e;
            }
            Assert.IsNotNull(ex);
            Assert.IsEqualTo(ex.Message, "The enumerable source is empty.");

            ret = connection.Get<MultiPrimaryKey>(new { Key1 = 1, Key2 = 2 });
            Assert.IsEqualTo(ret.Key1, 1);
            Assert.IsEqualTo(ret.Key2, 2);

            connection.DropTable<MultiPrimaryKey>();
        }

        #region Types

        struct Car
        {
            public enum TrapEnum : int
            {
                A = 1,
                B = 2
            }
#pragma warning disable 0649
            public string Name;
#pragma warning restore 0649
            public int Age { get; set; }
            public TrapEnum Trap { get; set; }
        }

        class EmptyClass
        {
        }

        class NullableProperty
        {
            public Int32? nullable { get; set; }
            public Int32 isNotNull { get; set; }
            public ShortEnum? NE2 { get; set; }
        }

        class AbstractInheritance
        {
            public abstract class Order
            {
                internal int Internal { get; set; }
                protected int Protected { get; set; }
                public int PublicVal { get; set; }

                public int ProtectedVal { get { return Protected; } }
            }

            public class ConcreteOrder : Order
            {
                public int Concrete { get; set; }
            }
        }

        class UserWithConstructor
        {
            public UserWithConstructor(int id, string name)
            {
                Ident = id;
                FullName = name;
            }
            public int Ident { get; set; }
            public string FullName { get; set; }
        }

        class PostWithConstructor
        {
            public PostWithConstructor(int id, int ownerid, string content)
            {
                Ident = id;
                FullContent = content;
            }

            public int Ident { get; set; }
            public UserWithConstructor Owner { get; set; }
            public string FullContent { get; set; }
            public Comment Comment { get; set; }
        }

        class Comment
        {
            public int Id { get; set; }
            public string CommentData { get; set; }
        }

        class MultipleConstructors
        {
            public MultipleConstructors()
            {

            }
            public MultipleConstructors(int a, string b)
            {
                A = a + 1;
                B = b + "!";
            }
            public int A { get; set; }
            public string B { get; set; }
        }

        class ConstructorsWithAccessModifiers
        {
            private ConstructorsWithAccessModifiers()
            {
            }
            public ConstructorsWithAccessModifiers(int a, string b)
            {
                A = a + 1;
                B = b + "!";
            }
            public int A { get; set; }
            public string B { get; set; }
        }

        class NoDefaultConstructor
        {
            public NoDefaultConstructor(int a1, int? b1, float f1, string s1, Guid G1)
            {
                A = a1;
                B = b1;
                F = f1;
                S = s1;
                G = G1;
            }
            public NoDefaultConstructor(int a1, int? b1, Decimal f1, string s1, String G1)
            {
                A = a1;
                B = b1;
                F = (float)f1;
                S = s1;
                G = new Guid(G1);
            }
            public int A { get; set; }
            public int? B { get; set; }
            public float F { get; set; }
            public string S { get; set; }
            public Guid G { get; set; }
        }

        class NoDefaultConstructorWithChar
        {
            public NoDefaultConstructorWithChar(char c1, char? c2, char? c3)
            {
                Char1 = c1;
                Char2 = c2;
                Char3 = c3;
            }
            public char Char1 { get; set; }
            public char? Char2 { get; set; }
            public char? Char3 { get; set; }
        }

        class NoDefaultConstructorWithEnum
        {
            public NoDefaultConstructorWithEnum(ShortEnum e1, ShortEnum? n1, ShortEnum? n2)
            {
                E = e1;
                NE1 = n1;
                NE2 = n2;
            }
            public ShortEnum E { get; set; }
            public ShortEnum? NE1 { get; set; }
            public ShortEnum? NE2 { get; set; }
        }

        public enum ShortEnum : short
        {
            Zero = 0, One = 1, Two = 2, Three = 3, Four = 4, Five = 5, Six = 6
        }

        public class Dog
        {
            public int? Age { get; set; }
            public Guid Id { get; set; }
            public string Name { get; set; }
            public float? Weight { get; set; }

            public int IgnoredProperty { get { return 1; } }
        }

        enum EnumParam : short
        {
            None, A, B
        }

        class EnumParamObject
        {
            public EnumParam A { get; set; }
            public EnumParam? B { get; set; }
            public EnumParam? C { get; set; }
        }

        class EnumParamObjectNonNullable
        {
            public EnumParam A { get; set; }
            public EnumParam? B { get; set; }
            public EnumParam? C { get; set; }
        }

        class Student
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        class TestObj
        {
            public int _internal;
            internal int Internal { set { _internal = value; } }

            public int _priv;
            private int Priv { set { _priv = value; } }
        }

        class InheritanceTest1
        {
            public string Base1 { get; set; }
            public string Base2 { get; private set; }
        }

        class InheritanceTest2 : InheritanceTest1
        {
            public string Derived1 { get; set; }
            public string Derived2 { get; private set; }
        }

        class TransactedConnection : IDbConnection
        {
            IDbConnection _conn;
            IDbTransaction _tran;

            public TransactedConnection(IDbConnection conn, IDbTransaction tran)
            {
                _conn = conn;
                _tran = tran;
            }

            public string ConnectionString { get { return _conn.ConnectionString; } set { _conn.ConnectionString = value; } }
            public int ConnectionTimeout { get { return _conn.ConnectionTimeout; } }
            public string Database { get { return _conn.Database; } }
            public ConnectionState State { get { return _conn.State; } }

            public IDbTransaction BeginTransaction(IsolationLevel il)
            {
                throw new NotImplementedException();
            }

            public IDbTransaction BeginTransaction()
            {
                return _tran;
            }

            public void ChangeDatabase(string databaseName)
            {
                _conn.ChangeDatabase(databaseName);
            }

            public void Close()
            {
                _conn.Close();
            }

            public IDbCommand CreateCommand()
            {
                // The command inherits the "current" transaction.
                var command = _conn.CreateCommand();
                command.Transaction = _tran;
                return command;
            }

            public void Dispose()
            {
                _conn.Dispose();
            }

            public void Open()
            {
                _conn.Open();
            }

            public int Execute(String sql)
            {
                return (_conn as IConnection).ExecuteNonQuery(sql);
            }
        }

        #endregion
    }
}
