# Run project

1. Change sql connection string in appsettings.Development.json
2. Update db in cmd

   ```
   dotnet ef database update
   ```

3. run

   ```
   dotnet run
   ```

# Compare time of 3 ways using Postman when update 10000 rows

| Property | EF basic   | EF sql raw | Dapper |
| -------- | ---------- | ---------- | ------ |
| Time     | 1880.86 ms | 37 ms      | 44ms   |

# Conclusion

EF sql raw ~ Dapper and them run faster than EF basic
