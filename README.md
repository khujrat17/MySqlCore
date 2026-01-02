# MySqlCore

[![.NET](https://github.com/khujrat17/MySqlCore/actions/workflows/dotnet.yml/badge.svg)](https://github.com/khujrat17/MySqlCore/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/MySqlCore.svg)](https://www.nuget.org/packages/MySqlCore)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MySqlCore.svg)](https://www.nuget.org/packages/MySqlCore)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

**MySqlCore** is a lightweight, modern .NET library that simplifies working with MySQL databases.
It provides async ADO.NET operations, EF Core integration, bulk insert/upsert, pagination, transactions, and logging support.

> ⚡ All public classes and methods include XML documentation for IntelliSense.

---

## Features

* Async/await support for ADO.NET operations
* EF Core integration (.NET 6.0)
* Transactions & error handling
* Pagination & async streaming for large datasets
* Bulk insert & upsert
* Query logging support
* Multi-framework support: .NET Framework 4.6.2+, .NET Standard 2.0, .NET 6.0

---

## Installation

### NuGet Package Manager

```powershell
Install-Package MySqlCore -Version 1.0.0
```

### .NET CLI

```bash
dotnet add package MySqlCore --version 1.0.0
```

> Compatible with .NET Framework 4.6.2+, .NET Standard 2.0, and .NET 6.0.

---

## Quick Start

```csharp
using MySqlCore;

var db = new MySqlHelper("your_connection_string");

// Select records
var users = await db.SelectAsync<User>("Users", "Id = 1");

// Insert record
await db.InsertAsync("Users", new User { Name = "John Doe" });
```

---

## Usage Examples

### ADO.NET CRUD

```csharp
using MySqlCore;
using MySql.Data.MySqlClient;

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

### Bulk Insert / Upsert

```csharp
var users = new List<User> {
    new User { Name = "Alice" },
    new User { Name = "Bob" }
};

// Bulk Insert
await MySqlHelper.BulkInsertAsync(conn, "Users", users);

// Bulk Upsert (insert or update based on primary key)
await MySqlHelper.BulkUpsertAsync(conn, "Users", users, "Id");
```

### EF Core Integration (.NET 6.0)

```csharp
using Microsoft.EntityFrameworkCore;

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

---

## Quick Reference Table

| Method                | Description                           |
| --------------------- | ------------------------------------- |
| `InsertAsync`         | Insert a record into a table          |
| `SelectAsync`         | Select records asynchronously         |
| `UpdateAsync`         | Update records                        |
| `DeleteAsync`         | Delete records                        |
| `RunTransactionAsync` | Run multiple queries in a transaction |
| `QueryLogger`         | Log executed SQL statements           |
| `InsertEFAsync`       | Insert a record via EF Core           |
| `QueryEF`             | Query records via EF Core             |
| `UpdateEFAsync`       | Update a record via EF Core           |
| `DeleteEFAsync`       | Delete a record via EF Core           |
| `BulkInsertAsync`     | Insert multiple records efficiently   |
| `BulkUpsertAsync`     | Insert or update multiple records     |

---

## Contributing

Contributions are welcome! To contribute:

1. Fork the repository
2. Create a new branch for your feature/bugfix
3. Submit a pull request with tests and examples
4. Ensure all builds pass via GitHub Actions

See [CONTRIBUTING.md](CONTRIBUTING.md) for full guidelines.

---

## Getting Help

* File issues via [GitHub Issues](https://github.com/khujrat17/MySqlCore/issues)
* Check usage examples above
* Ask questions in GitHub Discussions (if enabled)

---

## License

This project is licensed under the **MIT License**. See [LICENSE](LICENSE) for details.

---

## Repository

[GitHub Repository](https://github.com/khujrat17/MySqlCore)
