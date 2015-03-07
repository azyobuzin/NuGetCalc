dir (Join-Path (Split-Path (& { $MyInvocation.ScriptName }) -Parent) "lib\*.dll") |
    foreach { [Reflection.Assembly]::LoadFrom($_.FullName) }

function Get-Nupkg($Path, $Source, $Version)
{
    if ($Path -is [NuGet.Packaging.PackageReaderBase]) { return $Path }
    if ($Path -is [IO.FileInfo]) { $Path = $Path.FullName }

    if ($Source -eq $null -and $Version -eq $null -and
        ($Path -match '(/|\\|\.nupkg)'))
    {
        New-Object NuGet.Packaging.PackageReader([IO.Compression.ZipFile]::Open($Path, [IO.Compression.ZipArchiveMode]::Read))
    }
    else
    {
        if ($Source -eq $null) { $Source = "https://api.nuget.org/v3/index.json" }

        $catalog = New-Object ComponentModel.Composition.Hosting.AggregateCatalog(New-Object ComponentModel.Composition.Hosting.DirectoryCatalog(
            (Join-Path (Split-Path $MyInvocation.ScriptName -Parent) lib), "NuGet.*.dll"))
        try
        {
            $containerType = [ComponentModel.Composition.Hosting.CompositionContainer]
            $container = New-Object $containerType($catalog)
            $getExports = ($containerType.GetMethods() |
                where { $_.Name -eq "GetExports" -and $_.GetParameters().Length -eq 0 -and $_.GetGenericArguments().Length -eq 2 } |
                select -Index 0).MakeGenericMethod([NuGet.Client.INuGetResourceProvider], [NuGet.Client.INuGetResourceProviderMetadata])
            $providers = $getExports.Invoke($container, @())
        }
        finally
        {
            $catalog.Dispose()
        }
        $repo = [NuGet.Client.SourceRepository].GetConstructor(([NuGet.Configuration.PackageSource], [Collections.Generic.IEnumerable[Lazy[NuGet.Client.INuGetResourceProvider, NuGet.Client.INuGetResourceProviderMetadata]]])).Invoke(
            ([NuGet.Configuration.PackageSource](New-Object NuGet.Configuration.PackageSource($Source)), $providers))
        if ($Version -eq $null)
        {
            $metares = [NuGet.Client.SourceRepository].GetMethod("GetResource", [Type[]]@()).MakeGenericMethod([NuGet.Client.MetadataResource]).Invoke($repo, @())
            $nugetVersion = $metares.GetLatestVersion($Path, $true, $false, [Threading.CancellationToken]::None).Result
        }
        else
        {
            $nugetVersion = New-Object NuGet.Versioning.NuGetVersion($Version)
        }
        $dlres = [NuGet.Client.SourceRepository].GetMethod("GetResource", [Type[]]@()).MakeGenericMethod([NuGet.Client.DownloadResource]).Invoke($repo, @())
        $resStream = $dlres.GetStream((New-Object NuGet.PackagingCore.PackageIdentity($Path, $nugetVersion)), [Threading.CancellationToken]::None).Result
        $memStream = New-Object IO.MemoryStream
        $resStream.CopyTo($memStream)
        $resStream.Close()
        New-Object NuGet.Packaging.PackageReader($memStream)
    }
}

function Find-MostCompatibleGroup($Groups, $TargetFramework)
{
    $reducer = New-Object NuGet.Frameworks.FrameworkReducer
    $f = $reducer.GetNearest(
        [NuGet.Frameworks.NuGetFramework]::Parse($TargetFramework),
        [NuGet.Frameworks.NuGetFramework[]]($groups | foreach TargetFramework))
    if ($f -ne $null)
    {
        $groups | where { $_.TargetFramework.Equals($f) } | select -Index 0
    }
}

function Get-ReferenceGroups
{
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true, Position=0)]
        $Path,
        $Source,
        $Version
    )

    $pkg = Get-Nupkg $Path $Source $Version
    try
    {
        $pkg.GetReferenceItems()
    }
    finally
    {
        $pkg.Dispose()
    }
}

function Find-MostCompatibleReferenceGroup
{
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true, Position=0)]
        $Path,
        $Source,
        $Version,
        [Parameter(Mandatory=$true, Position=1)]
        $TargetFramework
    )

    $groups = Get-ReferenceGroups $Path -Source $Source -Version $Version
    Find-MostCompatibleGroup $groups $TargetFramework
}

function Get-DependencyGroups
{
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true, Position=0)]
        $Path,
        $Source,
        $Version
    )

    $pkg = Get-Nupkg $Path $Source $Version
    try
    {
        $pkg.GetPackageDependencies()
    }
    finally
    {
        $pkg.Dispose()
    }
}

function Find-MostCompatibleDependencyGroup
{
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true, Position=0)]
        $Path,
        $Source,
        $Version,
        [Parameter(Mandatory=$true, Position=1)]
        $TargetFramework
    )

    $groups = Get-DependencyGroups $Path -Source $Source -Version $Version
    Find-MostCompatibleGroup $groups $TargetFramework
}

Export-ModuleMember -Function 'Get-ReferenceGroups', 'Find-MostCompatibleReferenceGroup', 'Get-DependencyGroups', 'Find-MostCompatibleDependencyGroup'
