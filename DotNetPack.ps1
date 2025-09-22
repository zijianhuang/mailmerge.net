$projList = 'Fonlow.Mail/Fonlow.Mail.csproj'
foreach($name in $projList){
    $packCmd = 'dotnet pack $name --no-build --output C:/NugetLocalFeeds --configuration Release'
    Invoke-Expression $ExecutionContext.InvokeCommand.ExpandString($packCmd)
}