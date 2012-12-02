using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace LX.EasyDb.Samples
{
    class Samples
    {
        //[Active]
        public void UsingEasyDbHelper()
        {
            EasyDbHelper.ExecuteNonQuery("create table t(i int)");
            Int32 count = EasyDbHelper.ExecuteNonQuery(@"insert t (i) values(@a)", new[] { new { a = 1 }, new { a = 2 }, new { a = 3 }, new { a = 4 } });

            Console.WriteLine("Rows added: {0}", count);

            EasyDbHelper.ExecuteNonQuery("drop table t");
        }

        public void ExecuteSQL()
        {
            IConnection connection = Program.GetOpenConnection();

            connection.ExecuteNonQuery("create table t(i int)");
            Int32 count = connection.ExecuteNonQuery(@"insert t (i) values(@a)", new[] { new { a = 1 }, new { a = 2 }, new { a = 3 }, new { a = 4 } });
            Int32 sum = Enumerable.Single(connection.Query<Int32>("select sum(i) from t"));

            Console.WriteLine("Rows added: {0}", count);
            Console.WriteLine("Sum: {0}", sum);

            connection.ExecuteNonQuery("drop table t");

            connection.Close();
        }

        public void QueryUsers()
        {
            IConnection connection = Program.GetOpenConnection();

            if (!connection.ExistTable<User>())
                connection.CreateTable<User>();

            User user1 = new User { username = "un1", password = "pwd1" };
            User user2 = new User { username = "un2", password = "pwd2" };
            User user3 = new User { username = "un3", password = "pwd3" };

            connection.Insert(user1);
            connection.Insert(user2);
            connection.Insert(user3);

            foreach (var user in connection.Query<User>("select * from sample_users"))
            {
                Console.WriteLine(user);
            }

            connection.DropTable<User>();

            connection.Close();
        }

        public void QuerySingleUser()
        {
            IConnection connection = Program.GetOpenConnection();

            if (!connection.ExistTable<User>())
                connection.CreateTable<User>();

            User user1 = new User { username = "un1", password = "pwd1" };
            User user2 = new User { username = "un2", password = "pwd2" };
            User user3 = new User { username = "un3", password = "pwd3" };

            connection.Insert(user1);
            connection.Insert(user2);
            connection.Insert(user3);

            Console.WriteLine(user1);
            Console.WriteLine(user2);
            Console.WriteLine(user3);

            user1 = connection.Get<User>(user2.id);
            Console.WriteLine("Get user 2: " + user1);

            try
            {   // will cause a exception since user with id valued 3 doesn't exist
                user1 = connection.Get<User>(0);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Get user 0: Exception - " + ex.Message);
            }

            // will return null instead of throw a exception
            user1 = connection.Find<User>(0);
            Console.WriteLine("Find user 0: null? " + (user1 == null));

            connection.DropTable<User>();

            connection.Close();
        }

        public void QueryInterface()
        {
            IConnection connection = Program.GetOpenConnection();

            if (!connection.ExistTable<IUser>())
                connection.CreateTable<IUser>();
            Int32 id = connection.Insert(typeof(IUser), new { username = "un1", password = "pwd1" });
            IUser user = connection.Get<IUser>(id);
            Console.WriteLine(String.Format("{3}: id : {0}, username : {1}, password : {2}", user.id, user.username, user.password, user.GetType()));

            connection.DropTable<IUser>();

            connection.Close();
        }

        public void QueryStruct()
        {
            IConnection connection = Program.GetOpenConnection();

            if (!connection.ExistTable<UserStruct>())
                connection.CreateTable<UserStruct>();

            try
            {
                UserStruct user1 = new UserStruct { id = 1, username = "un1", password = "pwd1" };
                UserStruct user2 = new UserStruct { id = 2, username = "un2", password = "pwd2" };

                connection.Insert(user1);
                connection.Insert(user2);

                Console.WriteLine(user1);
                Console.WriteLine(user2);

                user1 = connection.Get<UserStruct>("un2");
                Console.WriteLine("Get user struct 2: " + user1);
            }
            finally
            {
                connection.DropTable<UserStruct>();
                connection.Close();
            }
        }

        public void CRUD()
        {
            IConnection connection = Program.GetOpenConnection();

            if (!connection.ExistTable<User>())
                connection.CreateTable<User>();

            User user1 = new User { username = "un1", password = "pwd1" };

            Int32 id = connection.Insert(user1);
            Console.WriteLine(user1);

            user1.password = "modified";
            connection.Update(user1);
            Console.WriteLine(user1);

            User user2 = connection.Get<User>(id);
            Console.WriteLine(user2);

            connection.Delete(user2);
            user2 = connection.Find<User>(id);
            Console.WriteLine(user2);

            connection.DropTable<User>();

            connection.Close();
        }

        public void Transaction()
        {
            IConnection connection = Program.GetOpenConnection();

            if (!connection.ExistTable<User>())
                connection.CreateTable<User>();

            User user1 = new User { username = "un1", password = "pwd1" };
            Int32 id = connection.Insert(user1);
            Console.WriteLine("Before transaction: " + user1);

            connection.BeginTransaction();

            user1.password = "modified";
            connection.Update(user1);

            User user2 = connection.Get<User>(id);
            Console.WriteLine("In transaction: " + user2);

            connection.RollbackTransaction();

            user2 = connection.Get<User>(id);
            Console.WriteLine("Rollback transaction: " + user2);

            connection.DropTable<User>();

            connection.Close();
        }
    }

    [Mapping.Table(Name = "sample_users_i")]
    public interface IUser
    {
        [Mapping.Column(Type = DbType.Identity)]
        [Mapping.PrimaryKey]
        Int32 id { get; set; }
        String username { get; set; }
        String password { get; set; }
    }

    [Mapping.Table(Name = "sample_users")]
    class User
    {
        [Mapping.Column(Name = "id_1", Type = DbType.Identity)]
        [Mapping.PrimaryKey]
        public Int32 id { get; set; }
        [Mapping.Column(Name = "username_1")]
        public String username { get; set; }
        [Mapping.Column(Name = "password_1")]
        public String password { get; set; }
        public override string ToString()
        {
            return String.Format("Class User - id : {0}, username : {1}, password : {2}", id, username, password);
        }
    }

    [Mapping.Table(Name = "sample_users_s")]
    struct UserStruct
    {
        public Int32 id { get; set; }
        [Mapping.PrimaryKey]
        public String username { get; set; }
        public String password { get; set; }
        public override string ToString()
        {
            return String.Format("Struct User - id : {0}, username : {1}, password : {2}", id, username, password);
        }
    }
}
