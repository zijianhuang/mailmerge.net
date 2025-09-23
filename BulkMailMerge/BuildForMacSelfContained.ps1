Set-Location $PSScriptRoot
$target="../Release/BulkMailMerge/MacSelfContained"

Remove-Item -Recurse $target*

dotnet publish -r osx-x64 --output $target --configuration release --self-contained true

