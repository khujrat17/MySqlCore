# MySqlCore

Single-class MySQL helper library for .NET that provides ADO.NET and EF Core CRUD operations, async streaming, pagination, bulk insert/upsert, transactions, and logging.

## Features

* ADO.NET CRUD operations (Insert, Select, Update, Delete, Upsert)
* EF Core integration for .NET 6.0
* Async streaming of large datasets
* Pagination support
* Transaction support
* Bulk insert/upsert
* Query logging support
* Multi-framework support: .NET Framework 4.6.2+, .NET Standard 2.0, .NET 6.0

## Installation

Install via NuGet Package Manager:

```bash
Install-Package MySqlCore -Version 1.0.0
```

Or using .NET CLI:

```bash
dotnet add package MySqlCore --version 1.0.0
```

## Supported Frameworks

* .NET Framework 4.6.2+
* .NET Standard 2.0
* .NET 6.0

## Usage Examples

### ADO.NET CRUD

```csharp
using System.Data.Common;
using MySql.Data.MySqlClient;
using MySqlHelper;

var conn = new MySqlConnection("your_connection_string");
await conn.OpenAsync();

// Insert
await MySqlHelper.InsertAsync(conn, "Users", new User { Id = 1, Name = "John" });

// Select
var users = await MySqlHelper.SelectAsync<User>(conn, "Users", "Id = 1");

// Update
await MySqlHelper.UpdateAsync(conn, "Users", new User { Id = 1, Name = "Jane" }, "Id");

// Delete
await MySqlHelper.DeleteAsync(conn, "Users", "Id", 1);
```

### EF Core Integration (for .NET 6.0)

```csharp
using Microsoft.EntityFrameworkCore;
using MySqlHelper;

var context = new AppDbContext();

// Insert
await MySqlHelper.InsertEFAsync(context, new User { Name = "John" });

// Query
var allUsers = MySqlHelper.QueryEF<User>(context).ToList();

// Update
await MySqlHelper.UpdateEFAsync(context, user);

// Delete
await MySqlHelper.DeleteEFAsync(context, user);
```

### Transactions

```csharp
await MySqlHelper.RunTransactionAsync(conn, async tx => {
    await MySqlHelper.InsertAsync(conn, "Users", new User { Id = 2, Name = "Alice" });
    await MySqlHelper.UpdateAsync(conn, "Users", new User { Id = 2, Name = "Bob" }, "Id");
});
```

### Logging

```csharp
MySqlHelper.QueryLogger = async sql => {
    Console.WriteLine($"Executing SQL: {sql}");
};
```

## License

MIT License. See [LICENSE](LICENSE) for more information.

## Repository

[GitHub Repository](https://github.com/khujrat17/MySqlCore)
