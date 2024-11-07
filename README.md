# DOTNET CORE WEBAPI

# MIGRATION
1. copy paste models
2. register di db context (db context)
3. dotnet ef migrations add InitialCreate --> dotnet ef migrations script > script.sql // untuk create script sql nya
4. dotnet ef database update


# RESPONSE CODE
00 success
01 db error
02 invalid token / expired token
03 too many request
04 not found
05 already exist

# RUN APP
dotnet run
dotnet watch
