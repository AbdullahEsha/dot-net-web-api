# Dot Net Web Api

# Launch: (Swagger View)

```sh
http://localhost:8080/swagger/index.html
```

<!--
 - docker rm sqlserver2022
 - docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Password@2025" \
   -p 1433:1433 --name sqlserver2022 \
   -d mcr.microsoft.com/mssql/server:2022-latest
-->

# Get all products with pagination

GET /api/products?page=1&pageSize=5

# Filter by price range

GET /api/products?minPrice=10&maxPrice=100

# Get only in-stock products

GET /api/products?inStock=true

# Search products

GET /api/products?search=laptop

# Combine filters with pagination and sorting

GET /api/products?page=1&pageSize=10&minPrice=50&inStock=true&orderBy=price&orderDescending=false

# Create a product

POST /api/products
{
"name": "Gaming Laptop",
"description": "High-performance gaming laptop",
"price": 1299.99,
"stockQuantity": 10
}

# Update a product

PUT /api/products/1
{
"name": "Updated Gaming Laptop",
"price": 1199.99
}
