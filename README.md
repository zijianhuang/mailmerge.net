# Fonlow Mail


## [Fonlow.Mail](Fonlow.Mail/README.md)

NuGet package. Common Email functions based on [MailKit](https://github.com/jstedfast/MailKit).

## [BulkMailMerge](BulkMailMerge/README.md)

.NET console app. Bulk Email merge based on `Fonlow.Mail`.

## Testing 

When testing Mail2Tests and BulkMailMerge.exe, please setup SMTP credentials in Secret.json based on:
```xml
<UserSecretsId>Fonlow.Mail2Tests20221223</UserSecretsId>
```

.NET runtime may use the key values of [app secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) in a folder like C:\Users\YourProfile\AppData\Roaming\Microsoft\UserSecrets\Fonlow.Mail2Tests20221223 `Secret.json` to overwrite the ones in `appsettings.json`.

The release build of BulkMailMerge.exe will not use app secrets but only appsettings.json.