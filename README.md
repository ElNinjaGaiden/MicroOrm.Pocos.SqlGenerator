MicroOrm.Pocos.SqlGenerator
===========================

If you like your code runs fast, probably you know about Micro ORMs.
They are simple and one of their main goal is be the fastest way to execute your SQL sentences on your data repositories.
However, for some of them you need to write your own SQL sentences. This is the case of two of the most popular Micro ORMs [Dapper](https://code.google.com/p/dapper-dot-net/) and [OrmLite](https://github.com/ServiceStack/ServiceStack.OrmLite)

The idea of this tool is to abstract the generation of the SQL sentence for CRUD operations based on each POCO class "metadata".
We known there are plugins for both Micro ORMs to implement the execution of this kind of tasks,
but that's exactly the difference of this tool. The "SQL Generator" is a generic component
that generates all the CRUD sentences for a POCO class based on its definition with the possibility to override the way the SQL generator builds each sentence.

Goals
-----
*  Avoid to write SQL.
*  Avoid to possibly overwhelm your application by using Reflection on each CRUD operation execution. The best idea about this is handling SQL Generators as singletons.
*  Abstract the SQL generation process and reuse the same implementation with both Micro ORMs [Dapper](https://code.google.com/p/dapper-dot-net/) and [OrmLite](https://github.com/ServiceStack/ServiceStack.OrmLite) or even other kind of tools rather than Micro ORMs

Metadata attributes
-------------------
This is the set of attributes to specify POCOs metadata. All of them are under the "MicroOrm.Pocos.SqlGenerator.Attributes" namespace:

###	[KeyProperty]
For property or properties that compose the primary key of the table. If Identity optional parameter is not specified, its default value will be false.	
Tables with identity primary keys need to set the extra parameter "Identity" to true and only one single property might be decorated with this attribute, like this `[KeyProperty(Identity = true)]`.  

###	[StoredAs("<value>table or column name<value>")]
For classes or properties that don't match name with its corresponding table or column.	
Use this attribute to specify the table or column name that the SQL Generator has to use.	

###	[Scheme("<value>table scheme</value>")]
Use this attribute to decorate those tables that does not belong to the default database scheme.  

###	[NonStored]
For "logical" properties that does not have a corresponding column and have to be ignored by the SQL Generator.	

###	[StatusProperty]
For tables that implements "logical deletes" instead of physical deletes. This attribute can decorate only `enum` properties and one of the possible values for that enumeration has to be decorated with the "Deleted" attribute

###	[Deleted]
Brother of the previous attribute. Use this to decorate the enum value that specifies the logical delete value for the status property.	

Some notes
----------

*  By default the SQL Generator is going to map the POCO name with the table name, and each public property to a column.
*  If the `StatusProperty` is used on a certain POCO, the "delete" sentence will be an update instead of a delete.
*  KeyProperties with the "Identity" parameter set to true is going to generate "insert" statements including the new generated id as return value.
*  Complex primary keys are supported.

SQL Sentences
=============

Lets see some SQL sentences examples that this tool will create. "Users" POCO:

	[StoredAs("Users")]
	public class User
	{
		[KeyProperty(Identity = true)]
		public int Id { get; set; }
		
		public string Login { get; set;}
		
		[StoredAs("FName")]
		public string FirstName { get; set; }
		
		[StoredAs("LName")]
		public string LastName { get; set; }
		
		public string Email { get; set; }
		
		[StatusProperty]
		public UserStatus Status { get; set; }
		
		[NonStored]
		public string FullName
		{
			get
			{
				return string.Format("{0} {1}", FirstName, LastName);
			}
		}
	}

UserStatus enum, this supports logical deletes for the "User" POCO:

	public enum UserStatus : byte
	{
		Registered = 1,
		
		Active = 2,
		
		[Deleted]
		Inactive = 3
	}

The entity "analysis" takes place when the SQL Generator is created, like this:

	ISqlGenerator<User> sqlGenerator = new SqlGenerator<User>();
	
When the SQL Generator is created, it executes all the analysis of the entity based on each property and it is ready
to generate whatever CRUD sentence for the given POCO. That's why handling the SQL Generators as singletons could be considered a good practice, because that way you will reduce the frequent use of Reflection. Something like this:

	//Singleton implementation of a SQL Generator for "User" POCO
	public static class UserSqlGenerator : SqlGenerator<User>
	{
		//Private instance
		private static UserSqlGenerator _instance = new UserSqlGenerator();
		
		//Public instance (singleton)
		public static UserSqlGenerator Instance
		{
			get
			{
				return _instance;
			}
		}
		
		//Private constructor
		private UserSqlGenerator()
		{
		}
	}

You can go furthermore and use [Dependency Injection](http://en.wikipedia.org/wiki/Dependency_injection) in order to configure your SQL Generators.
This pseudo code example is using [Autofac](http://autofac.org/) as container implementation:

	//Registering your SQL Generators into the DI container
	DI.Container.RegisterType<SqlGenerator<User>>().As<ISqlGenerator<User>>().Singleton();
	
	//How to get your "User" SQL Generator
	var userSqlGenerator = DI.Container.Resolve<ISqlGenerator<User>>();

Based on the previous "User" POCO, this are the generated sentences:

Insert
------
	//Using DI
	var sqlGenerator = DI.Container.Resolve<ISqlGenerator<User>>();
	var insert = sqlGenerator.GetInsert();
	
	//Or using the normal singleton
	var insert = UserSqlGenerator.Instance.GetInsert();
	
Generates:
	
	INSERT INTO [Users] ([Users].[Login], [Users].[FName], [Users].[LName], [Users].[Email], [Users].[Status])
	VALUES (@Login, @FirstName, @LastName, @Email, @Status)
	DECLARE @NEWID NUMERIC(38, 0)
	SET @NEWID = SCOPE_IDENTITY()
	SELECT @NEWID

Update
------
	//Using DI
    var sqlGenerator = DI.Container.Resolve<ISqlGenerator<User>>();
    var update = sqlGenerator.GetUpdate();
	
	//Or using the normal singleton
	var update = UserSqlGenerator.Instance.GetUpdate();
    
Generates:

	UPDATE 	[Users]
	SET 	[Users].[Login] = @Login,
			[Users].[FName] = @FirstName,
			[Users].[LName] = @LastName,
			[Users].[Email] = @Email,
			[Users].[Status] = @Status
	WHERE 	[Users].[Id] = @Id

Delete
------
	//Using DI
	var sqlGenerator = DI.Container.Resolve<ISqlGenerator<User>>();
    var delete = sqlGenerator.GetDelete();
	
	//Or using the normal singleton
	var delete = UserSqlGenerator.Instance.GetDelete();
	
Generates:

	//Logical delete
	UPDATE 	[Users]
	SET 	[Users].[Status] = 3
	WHERE 	[Users].[Id] = @Id
	
Select All
----------
	//Using DI
	var sqlGenerator = DI.Container.Resolve<ISqlGenerator<User>>();
    var selectAll = sqlGenerator.GetSelectAll();
	
	//Or using the normal singleton
	var selectAll = UserSqlGenerator.Instance.GetSelectAll();
	
Generates:

	SELECT 	[Users].[Id],
			[Users].[Login],
			[Users].[FName] AS [FirstName],
			[Users].[LName] AS [LastName],
			[Users].[Email],
			[Users].[Status]
	FROM 	[Users] WITH (NOLOCK)
	WHERE 	[Users].[Status] != 3
	
Select By
---------
	//Using DI
	var sqlGenerator = DI.Container.Resolve<ISqlGenerator<User>>();
    var selectBy = sqlGenerator.GetSelect(new { FirstName = "" });
	
	//Or using the normal singleton
	var selectBy = UserSqlGenerator.Instance.GetSelect(new { FirstName = "" });
	
Generates:

	SELECT 	[Users].[Id],
			[Users].[Login],
			[Users].[FName] AS [FirstName],
			[Users].[LName] AS [LastName],
			[Users].[Email],
			[Users].[Status]
	FROM 	[Users] WITH (NOLOCK)
	WHERE 	[Users].[FName] = @FirstName
	AND 	[Users].[Status] != 3

A Practical Usage Example
=========================
At this point we have: our POCOS, Micro ORMs, something to generate our SQL sentences and the [Repository Pattern](http://msdn.microsoft.com/en-us/library/ff649690.aspx), lets make all this work together. Lets build a "Users" data repository using [Dapper](https://code.google.com/p/dapper-dot-net/):	

The POCO (again):

	[StoredAs("Users")]
	public class User
	{
		[KeyProperty(Identity = true)]
		public int Id { get; set; }
		
		public string Login { get; set;}
		
		[StoredAs("FName")]
		public string FirstName { get; set; }
		
		[StoredAs("LName")]
		public string LastName { get; set; }
		
		public string Email { get; set; }
		
		[StatusProperty]
		public UserStatus Status { get; set; }
		
		[NonStored]
		public string FullName
		{
			get
			{
				return string.Format("{0} {1}", FirstName, LastName);
			}
		}
	}
	
	public enum UserStatus : byte
	{
		Registered = 1,
		
		Active = 2,
		
		[Deleted]
		Inactive = 3
	}

The repository contract:

	public interface IUsersRepository
	{
		IEnumerable<User> GetAll();
		
		IEnumerable<User> GetWhere(object filters);
		
		User GetFirst(object filters);
		
		bool Insert(User instance);
		
		bool Update(User instance);
		
		bool Delete(object key);
	}
	
The repository implementation:

	using Dapper;

	public class UsersRepository : IUsersRepository
	{
		//Static SQL Generator
		private static SqlGenerator<User> SqlGenerator = new SqlGenerator<User>();
		
		//Database connection
		protected IDbConnection Connection { get; private set; }
		
		//Constructor
		public UsersRepository(IDbConnection connection)
		{
			this.Connection = connection;
		}
		
		//Select all
		public IEnumerable<User> GetAll()
		{
			return Connection.Query<User>(SqlGenerator.GetSelectAll());
		}
		
		//Select by
		public IEnumerable<User> GetWhere(object filters)
		{
			return Connection.Query<User>(SqlGenerator.GetSelect(filters), filters);
		}
		
		//Select first by
		public User GetFirst(object filters)
		{
			return GetWhere(filters).FirstOrDefault();
		}
		
		//Insert
		public bool Insert(User instance)
		{
			var newId = Connection.Query<decimal>(SqlGenerator.GetInsert(), instance).Single();
			var inserted = newId > 0;
			
			if(inserted)
			{
				instance.Id = (int)newId;
			}
			
			return inserted;
		}
		
		//Update
		public bool Update(User instance)
		{
			return Connection.Execute(SqlGenerator.GetUpdate(), instance) > 0;
		}
		
		//Delete
		public bool Delete(int id)
		{
			return Connection.Execute(SqlGenerator.GetDelete(), id) > 0;
		}
	}
	
And then, use it like this:

	//POCO instance
	var user = new User 
	{ 
		FirstName = "John",
		LastName = "Smith",
		Login = "johnsmith@somedomain.com",
		Email = "johnsmith@somedomain.com",
		Status = UserStatus.Registered
	};
	
	//Repository instance
	IUsersRepository usersRepository = new UsersRepository();
	
	//Insert the user
	var inserted = usersRepository.Insert(user);
	
	if(inserted)
	{
		//Retrieve the new user
		var sameUser = usersRepository.GetFirst(new { Email = "johnsmith@somedomain.com" });
	
		if(user != null)
		{
			//Update the new user
			user.Email = "otheremail@somedomain.com";
			user.Status = UserStatus.Active;
			usersRepository.Update(user);
		}
	}
	
	//Select all
	var allUsers = usersRepository.GetAll();
	
	//Select filtered
	var allActiveJohns = usersRepository.GetWhere(new { FirstName = "John", Status = UserStatus.Active });
	
	//Delete each active John
	foreach (var activeJohn in allActiveJohns)
	{
		usersRepository.Delete(activeJohn.Id);
	}

Practically the same code will work using [OrmLite](https://github.com/ServiceStack/ServiceStack.OrmLite) instead of [Dapper](https://code.google.com/p/dapper-dot-net/), that's one of the main benefits of this tool.  
And... you are no writing any SQL.	

License
-------
The MIT License (MIT)

Copyright (c) 2014 Diego García

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
