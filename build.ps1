$fakePath = '.\packages\FAKE\tools\FAKE.exe'
if (!(Test-Path $fakePath))
{
    .\.nuget\NuGet install FAKE -Source https://api.nuget.org/v2/ -ExcludeVersion -OutputDirectory packages
}

& $fakePath .\build.fsx $args
