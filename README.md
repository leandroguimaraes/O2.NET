# O2.NET

## Intro

O2 stands for "Object Oriented", and started as a PHP project aiming to make it easier to save, recover, update and delete data from databases.

O2.NET version supports SQL Server, MySQL, Firebird and PostgreSQL, and is a working in progress. And O2 have a [PHP version](https://github.com/leandroguimaraes/O2.PHP) too, with a very similar syntax.

## How to

Create a new .NET project (2.0 or higher), and add O2.dll as a reference.

Create a database table (MySQL sample below):

```sql
CREATE TABLE `o2_clients` ( /* "o2_" will be a common prefix for all database tables on your project scope */
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT, /* all tables must have an "id" autoincrement column */
  `given_name` varchar(100) DEFAULT NULL,
  `surname` varchar(150) DEFAULT NULL,
  `date_of_birth` date DEFAULT NULL,
  `sample_integer` int(11) DEFAULT NULL,
  `sample_decimal` decimal(10,2) DEFAULT NULL,
  PRIMARY KEY (`id`)
);
```

Create a class (C# sample below):

```c#
using O2.Includes;

class Client : SysObject
{
    public string table = "clients"; //all classes must reference its correspondent table this way, without table prefix

    //you don't need to declare an "id" property because it's inherited from SysObject parent class
    public string given_name { get; set; }
    public string surname { get; set; }
    public DateTime date_of_birth { get; set; }
    public int sample_integer { get; set; }
    public decimal sample_decimal { get; set; }
}
```

Add a config file to your project with something like the following:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <!-- MySQL data connector: https://dev.mysql.com/downloads/connector/net/ -->
  <system.data>
    <DbProviderFactories>
      <add name="MySQL Data Provider" invariant="MySql.Data.MySqlClient" description=".Net Framework Data Provider for MySQL" type="MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data, Version=6.8.3.0, Culture=neutral, PublicKeyToken=c5687fc88969c44d" />
    </DbProviderFactories>
  </system.data>
  <connectionStrings>
    <clear/>
    <!-- database connection info -->
    <add name="MySQL" connectionString="Server=dbhost;Database=dbname;Uid=dbuser;Pwd=dbpassword;" providerName="MySql.Data.MySqlClient"/>
  </connectionStrings>
  <appSettings>
    <!-- tables' prefix -->
    <add key="TBPREFIX" value="o2_"/>
  </appSettings>
</configuration>
```

And now you're ready for the CRUD show!

## CREATE (Insert)

```c#
Client client = new Client();

client.given_name = "Homer";
client.surname = "Simpson";
client.date_of_birth = new DateTime(1956, 5, 12);
client.sample_integer = 123;
client.sample_decimal = 123.45M;

client.Insert();

Console.WriteLine(client.id); //database autoincrement gift
```

## READ (Select / Load)

```c#
Client client = new Client();

client.Load(1); //or any other id you may need

Console.WriteLine(client.given_name); //you can read all object loaded data this way
```

## UPDATE (Update)

```c#
Client client = new Client();
client.Load(1); //or any other id you may need
client.given_name = "Bart";
client.surname = "Simpson";
client.date_of_birth = new DateTime(1980, 4, 1);
client.sample_integer = 456;
client.sample_decimal = 456.78M;

client.Update();
```

## DELETE (Delete)

```c#
Client client = new Client();
client.Load(1); //or any other id you may need

client.Delete();
```

## CUSTOM QUERYs

And finally, you can execute general purpose SQL querys with O2 as shown below.

### SELECT

```c#
using O2.Includes.DataBaseAccess;

(...)

Client client = new Client();

Query query = new Query();
query.AddParameter("@given_name", "%Bart%");
IDataReader reader = query.ExecuteReader("SELECT * FROM " + client.get_table() + " WHERE given_name LIKE @given_name");
try
{
  while(reader.Read())
  {
      client = new Client();
      //load database info into a object
      client.LoadBy_array(reader);

      //read data straigth from reader
      Console.WriteLine(reader["id"] + " - " + reader["given_name"]);
      //read data from object
      Console.WriteLine(client.id + " - " + client.given_name);
  }
}
finally
{
  reader.Close();
}
```

### UPDATE, INSERT or DELETE

```c#
Client client = new Client();

Query query = new Query();

query.AddParameter("@id", 1);
query.ExecuteNonQuery("DELETE FROM " + client.get_table() + " WHERE id = @id");
```

Enjoy it! :)