Install postgres / PG4 admin
Install nuget package: Npgsql.EntityFrameworkCore.PostgreSQL
Update connection string in appsettings.json
Update Program.cs to use new database/connection string
npm console update-database to test
*ERROR* nvarchar(256) in AspNetRoles throwing error
deleted old migrations and used npm: add-migration "Initial" -o "Data/Migrations"
	view in pgAdmin to verify

