# Version of the samples that works with KeyCloak

## KeyCloak configuration

Just create a realm called Demo, add some user into that realm and configure all claim you want. Create a simple client application and set to confidential mode then add all corresponding information in application.json configuration file

```json
  "KeyCloak": {
    "Authority": "http://localhost:8080/realms/Demo",
    "ClientId": "imageapi",
    "ClientSecret": "o94stnaYjxrhEOgzP50PBi9M1B2xzvpO"
  },
```

This will ensure that all parts of the application will be able to access KeyCloak settings.

## Certificate and https

Add these two lines (or change accordingly to your configuration) to the c:\windows\system32\drivers\etc\hosts file

```
127.0.0.4               oauth2demo.local
127.0.0.5               api.oauth2demo.local
```

Then create a certificate with [mkcert](https://github.com/FiloSottile/mkcert), my actual command line is

```
mkcert -pkcs12 127.0.0.4 127.0.0.5 oauth2demo.local api.oauth2demo.local
```

Then add the certificate into computer store, and if you run your computer not as admin (As you should do) from certificate manager console manage private key and give read access to your current user (the user that will run the application).

If you use different Ip configurations or different mkcert command you should change Kestrel section accordingly in the application.json configuration file.


## create datatabase

Be sure that you have installed ef tooling

```
dotnet tool install --global dotnet-ef
```

Then locate the SeedDatabaseMigration class and change the id to match the id of the user in keycloak, if you failed to do so sub claim will not match any id in the database.

When everything is ready just run the migration.

```
dotnet ef database update --project .\src\ImageGallery.API
```

Verify that everythign went well, then check with Sql Server Object explorer the content of the local db to verify that everything was loaded correctly.

## Endpoint to test

Endpoint https://oauth2demo.local/Gallery/dumptokens will dump all tokens in the response.