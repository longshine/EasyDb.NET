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
- Other features from Dapper. EasyDb.NET is internally driven by Dapper, 
an amazing project by Sam Saffron. 
