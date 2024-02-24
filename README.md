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

# Compare time of some ways using Postman when update 10000 rows

| Property | EF basic   | EF sql raw | Dapper | EF core bulk extension | EF core bulk update after version 7 |
| -------- | ---------- | ---------- | ------ | ---------------------- | ----------------------------------- |
| Time     | 1880.86 ms | 37 ms      | 44ms   |                        | 38ms                                |

# Conclusion

EF sql raw ~ Dapper ~ EF core bulk update after version 7 and them run faster than EF basic

See debug console of any api to see number of roundtrip that cause difference
