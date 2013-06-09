EasyDb.NET - An Easy Database Helper (Typed Query &amp; Simple ORM) for .NET
============================================================================

EasyDb.NET is a simple project that helps you with features:

- **Easy**. IDbConnection is extended by a new interface IConnection with 
enhanced query methods. Similar code, different experience. Helper methods 
may still be used to get rid of the repeating trivia such as opening and
closing connections and make coding clean and simple.
- **Cross-database**. EasyDb.NET works across all .NET ADO providers. Most 
times you don't need to concern details of DB specific differences since 
there is the hibernate-like dialecting.
- **Typed query**. Query results can be mapped into strongly typed or dynamic
objects. ORMs can also be customized with help of Mapping attributes.
- Support OO-style **Criterion query**.
- Other features from [**Dapper**] (https://github.com/SamSaffron/dapper-dot-net).
  EasyDb.NET is internally driven by Dapper, an amazing project by Sam Saffron.
- Support .NET 2.0, 3.0, 3.5, 4.0.

Build
-----

Define a compile symbol NET20 to build for .NET 2.0, CSHARP30 for .NET 3.0.
By default the project is targeted to .NET 3.5 and above.

Usage
-----

### Setup

A **[IConnectionFactory] (EasyDb.NET/IConnectionFactory.cs)** or the static helper class
**[EasyDbHelper] (EasyDb.NET/EasyDbHelper.cs)** is needed for open querying connections.

#### Using **[IConnectionFactory] (EasyDb.NET/IConnectionFactory.cs)**

```csharp
  IConnectionFactory factory;
  
  // build factory with given DbProviderFactory and Dialect.
  factory = ConnectionFactoryBuilder.NewBuilder(
      System.Data.SQLite.SQLiteFactory.Instance,
      "Data Source=test.db;Pooling=true;FailIfMissing=false",
      "SQLiteFactory",
      new SQLiteDialect()
  ).Build();
  
  // or build from strings
  factory = ConnectionFactoryBuilder.NewBuilder(
      "mysql.data",
      "Server=127.0.0.1;Uid=root;Pwd=asdf;Database=sample;",
      "MySQLFactory",
      "LX.EasyDb.Dialects.MySQLDialect"
  ).Build();
```

Connections could be accquired from the factory then:

```csharp
  IConnection connection = factory.OpenConnection();
```

#### Or using the static **[EasyDbHelper] (EasyDb.NET/EasyDbHelper.cs)**

The EasyDbHelper should also be initialized before any use. By programming
it will be like:

```csharp
  EasyDbHelper.Initialize("mysql.data",
      "Server=127.0.0.1;Uid=root;Pwd=asdf;Database=sample;",
      "LX.EasyDb.Dialects.MySQLDialect");
```

Or by app configuration:

```xml
<configuration>
  <configSections>
    <section name="EasyDb" type="LX.EasyDb.Configuration.EasyDbConfiguration, EasyDb.NET"/>
  </configSections>
  
  <EasyDb>
    <add name="SampleSqlServer" connectionString="sqlServerConnStr"
      provider="System.Data.SqlClient, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"/>
  </EasyDb>
    
  <connectionStrings>
    <add name="sqlServerConnStr" connectionString="Data Source=localhost;Initial Catalog=Test;Integrated Security=True"/>
  </connectionStrings>
</configuration>
```

Then the static helper methods are ready to use.

```csharp
  int rowsAffected = EasyDbHelper.ExecuteNonQuery(sql);
  
  Object result = EasyDbHelper.ExecuteScalar(sql);
  
  IDataReader reader = EasyDbHelper.ExecuteReader(sql);
  
  IConnection connection = EasyDbHelper.OpenConnection();
```

### Query objects with **[IConnection] (EasyDb.NET/IConnection.cs)**

Besides the conventional executing methods (ExecuteNonQuery, ExecuteScalar and
ExecuteReader), IConnection provides 4 kinds of querying. Advanced features 
and performance could be found at **[Readme-Dapper] (Readme-Dapper.md)**.

#### Generic query

```csharp
  class User
  {
      public Int32 Id { get; set; }
      public String Username { get; set; }
      public String Password { get; set; }
  }
  
  connection.insert(new User() { Username = "user", Password = "pass" });
  
  User user = connection.Find<User>(1);
```

#### Entity query

```csharp
  connection.insert("User", new User() { Username = "user", Password = "pass" });
  
  IDictionary<String, Object> obj = connection.Find("User", 1);
```

#### Type query

```csharp
  public interface IUser
  {
      Int32 Id { get; set; }
      String Username { get; set; }
      String Password { get; set; }
  }

  connection.Insert(typeof(IUser), new { Username = "user", Password = "pass" });
  
  Object user = connection.Find(typeof(IUser), 1);
```

#### Criterion query

EasyDb.NET implements a set of hibernate-like criterion query interfaces, which
allows OO style query.

```csharp
  User user = connection.CreateCriteria<User>()
      .Add(Clauses.Eq("id", id))
      .SingleOrDefault();
```

Object/Relation Mapping
-----------------------

By default objects will be mapped to relational tables with the same names defined
in classes and properties. EasyDb.NET providers a few attributes to customize the
mapping.

For example, the class User mentioned above will be mapped to the table "User" by
default. To map it to the table "sample_users", add attributes as below:

```csharp
  [Mapping.Table(Name = "sample_users")]
  class User
  {
      [Mapping.Column(Name = "id", DbType = DbType.Identity)]
      [Mapping.PrimaryKey]
      public Int32 Id { get; set; }
      
      [Mapping.Column(Name = "username")]
      public String Username { get; set; }
      
      [Mapping.Column(Name = "pwd")]
      public String Password { get; set; }
  }
```

Experimental - Master/Slave Mode
--------------------------------

EasyDb.NET used to have a feature of multi-databases manipulation, support 
master/slave databases operations, which has been removed because lack of support.

License
-------

See [License] (License.txt) for more info.

Acknowledgements
----------------

EasyDb.NET is internally driven by [**Dapper**] (https://github.com/SamSaffron/dapper-dot-net),
an amazing object mapper for .Net by Sam Saffron. 
Thanks to the author and the great job.
