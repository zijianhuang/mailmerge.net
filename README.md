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

For testing BulkMailMerge.exe, run "dotnet run" in the project folder to ensure DOTNET_ENVIRONMENT=Development as declared in launchSettings.json. For example:
```ps1
dotnet run -- /CL=somebody1234@gmail.com -- /pl=c:/temp/protocol.log
```

For testing Mail2Tests, by default it will use app secrets.