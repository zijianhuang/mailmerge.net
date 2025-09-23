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

.NEt runtime will use the key values in `Secret.json` to overwrite the ones in `appsettings.json`.