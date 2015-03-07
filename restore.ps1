.\.nuget\NuGet install NuGet.Protocol.V2V3 -Source https://api.nuget.org/v2/ -Prerelease -ExcludeVersion -OutputDirectory packages

function Copy-Lib($Package, $LibDir)
{
    Write-Host "Copying $Package"
    Copy-Item ".\packages\$Package\lib\$LibDir\*.dll" .\lib\ -Force
}

mkdir .\lib -Force > $null
Copy-Lib Microsoft.Web.Xdt net40
Copy-Lib Newtonsoft.Json net45
Copy-Lib Nuget.Core net40-Client
Copy-Lib NuGet.Versioning portable-net40+win
Copy-Lib NuGet.Configuration net45
Copy-Lib NuGet.Packaging net45
Copy-Lib NuGet.Protocol.Types net45
Copy-Lib NuGet.Protocol.V2V3 net45
