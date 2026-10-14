# AdvancedEFCore

A small ASP.NET Core Web API that demonstrates advanced Entity Framework Core patterns with SQL Server.

## Features

- **Optimistic Concurrency** – `RowVersion` (`rowversion`) token on `Product`; updates return `409 Conflict` if the row was changed by another request (`DbUpdateConcurrencyException`).
- **Soft Delete** – `IsDeleted` flag combined with a global query filter (`HasQueryFilter`) so deleted rows are excluded automatically.
- **Custom Index** – non-clustered index `IX_Products_Name` on `Product.Name`.
- **Seed Data** – categories and products seeded via `HasData` and applied through migrations.
- **Pagination** – paged `GET` with `AsNoTracking` and projection straight to DTOs (no tracking overhead, minimal payload).
- **Relationships** – one-to-many between `Category` and `Product` with required foreign key.
- **OpenAPI/Swagger** – interactive API docs in the development environment.

## Tech Stack

| Component | Version |
|---|---|
| .NET | 10.0 |
| Entity Framework Core (SqlServer) | 10.0.13 |
| Swashbuckle.AspNetCore | 10.3.0 |
| Database | SQL Server (e.g. `localhost\SQLEXPRESS`) |

## Project Structure

```
AdvancedEFCore/
├── Controllers/
│   └── ProductsController.cs   # Products API endpoints
├── Data/
│   └── ApplicationDbContext.cs # DbContext, model config, seed data
├── DTOs/Products/              # Request/response contracts
├── Migrations/                 # EF Core migrations
├── Models/                     # Product, Category entities
└── Program.cs                  # App bootstrap & DI wiring
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express, or full instance)
- `dotnet-ef` tool:

  ```bash
  dotnet tool install --global dotnet-ef
  ```

## Getting Started

1. **Configure the connection string** in `appsettings.json`:

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Data Source=localhost\\SQLEXPRESS;Initial Catalog=AdvancedEFCoreDb;Integrated Security=True;Encrypt=True;Trust Server Certificate=True"
   }
   ```

2. **Create and apply migrations** (the database is created and seeded by the migrations):

   ```bash
   dotnet ef migrations add <MigrationName>
   dotnet ef database update
   ```

3. **Run the app**:

   ```bash
   dotnet run
   ```

4. **Open Swagger UI** (Development):
   - HTTP: `http://localhost:5105/swagger`
   - HTTPS: `https://localhost:7095/swagger`

## API Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/Products?pageNumber=1&pageSize=10` | Paged list of products (soft-deleted excluded). `pageSize` is capped at 100. |
| `PUT` | `/api/Products/{id}` | Update a product. Requires `RowVersion` in the body; returns `409` on concurrency conflict. |
| `DELETE` | `/api/Products/{id}` | Soft-delete a product (sets `IsDeleted = true`). |

### Example: Update a Product

```http
PUT /api/Products/1
Content-Type: application/json

{
  "name": "Mechanical Keyboard",
  "price": 2500,
  "categoryId": 1,
  "rowVersion": "AAAAAAAAB9E="
}
```

- `204 No Content` – update succeeded.
- `400 Bad Request` – missing/invalid `RowVersion` or category does not exist.
- `404 Not Found` – product does not exist (or is soft-deleted).
- `409 Conflict` – the product was modified by another request; reload and retry.

## How the Concurrency Flow Works

1. `GET` returns each product with its current `RowVersion`.
2. `PUT` sends that `RowVersion` back; the controller assigns it to the entity's `OriginalValue`.
3. On save, EF Core compares original vs. current DB value.
4. If another request already updated the row, SQL Server affects 0 rows and EF throws `DbUpdateConcurrencyException`, which is mapped to `409 Conflict`.
