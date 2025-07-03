# Dot Net Web Api

# Launch: (Swagger View)

```sh
http://localhost:8080/swagger/index.html
```

<!--
 # remove Migrations
 rm -rf Migrations/

 # Drop the database
 dotnet ef database drop --force

 # Create a migration
 dotnet ef migrations add InitialCreate

 # In your project directory
 dotnet ef database update

 # Stop and remove the container
 docker stop sqlserver2022
 docker rm sqlserver2022

 # Start fresh container
 docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Password@2025" \
    -p 1433:1433 --name sqlserver2022 \
    -d mcr.microsoft.com/mssql/server:2022-latest

 # Wait for it to be ready
 sleep 15

 # Check if it's running
 docker ps

 # Test connection
 docker exec -it sqlserver2022 /opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P "Password@2025" -C

 # Check if database exists
 docker exec -it sqlserver2022 /opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P "Password@2025" -C -Q "SELECT name FROM sys.databases WHERE name = 'e_commerce_db'"

 # List all tables in e_commerce_db
 docker exec -it sqlserver2022 /opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P "Password@2025" -C -Q "USE e_commerce_db; SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"

 # Check Users table
 docker exec -it sqlserver2022 /opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P "Password@2025" -C -Q "USE e_commerce_db; SELECT COUNT(*) as UserCount FROM Users"
-->

## API Endpoints

### Authentication

- **POST** `/api/auth/register` - Register new user
- **POST** `/api/auth/login` - Login user
- **POST** `/api/auth/refresh-token` - Refresh access token
- **POST** `/api/auth/revoke-token` - Revoke specific refresh token
- **POST** `/api/auth/revoke-all-tokens` - Revoke all user tokens
- **GET** `/api/auth/me` - Get current user info (requires authentication)

### Products (All require authentication)

- **GET** `/api/products` - Get products with optional filtering/pagination
- **POST** `/api/products` - Create new product (requires admin role)
- **GET** `/api/products/:id` - Get specific product
- **PUT** `/api/products/:id` - Update product (requires admin role)
- **DELETE** `/api/products/:id` - Delete product (requires admin role)

## Usage Examples

### Authentication Examples

#### Register

```json
POST /api/auth/register
{
  "username": "testuser",
  "email": "test@example.com",
  "password": "password123",
  "confirmPassword": "password123"
}
```

#### Login

```json
POST /api/auth/login
{
  "username": "testuser",
  "password": "password123"
}
```

#### Refresh Token

```json
POST /api/auth/refresh-token
{
  "refreshToken": "your-refresh-token-here"
}
```

### Product Examples

#### Get all products with pagination

```
GET /api/products?page=1&pageSize=5
Authorization: Bearer your-access-token-here
```

#### Filter products

```
GET /api/products?minPrice=10&maxPrice=100&inStock=true&search=laptop
Authorization: Bearer your-access-token-here
```

#### Create product (Admin only)

```json
POST /api/products
Authorization: Bearer your-access-token-here
{
  "name": "Gaming Laptop",
  "description": "High-performance gaming laptop",
  "price": 1299.99,
  "stockQuantity": 10
}
```

#### Update product (Admin only)

```json
PUT /api/products/1
Authorization: Bearer your-access-token-here
{
  "name": "Updated Gaming Laptop",
  "price": 1199.99
}
```

#### Delete product (Admin only)

```
DELETE /api/products/1
Authorization: Bearer your-access-token-here
```

## Response Structure

Successful product responses will include:

```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Gaming Laptop",
    "description": "High-performance gaming laptop",
    "price": 1299.99,
    "stockQuantity": 10,
    "createdAt": "2023-07-20T12:00:00Z",
    "updatedAt": "2023-07-20T12:00:00Z"
  }
}
```

For paginated results:

```json
{
  "success": true,
  "data": {
    "items": [...],
    "total": 50,
    "page": 1,
    "pageSize": 10,
    "totalPages": 5
  }
}
```

## Error Responses

Common error responses include:

- `401 Unauthorized` - Missing or invalid token
- `403 Forbidden` - User doesn't have required permissions
- `404 Not Found` - Product not found
- `400 Bad Request` - Invalid input data

Example:

```json
{
  "success": false,
  "error": "Product not found"
}
```
