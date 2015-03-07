#r "packages/FAKE/tools/FakeLib.dll"

open System.Text.RegularExpressions
open Fake

Target "RestorePackages" (fun _ ->
    RestorePackageId (fun p ->
        { p with
             Sources = ["https://api.nuget.org/v2/"]
             IncludePreRelease = true
             ExcludeVersion = true })
        "NuGet.Protocol.V2V3"

    let lib package libDir =
        sprintf @".\packages\%s\lib\%s\*.dll" package libDir
    
    let files =
        !! (lib "Microsoft.Web.Xdt" "net40")
        ++ (lib "Newtonsoft.Json" "net45")
        ++ (lib "Nuget.Core" "net40-Client")
        ++ (lib "NuGet.Versioning" "portable-net40+win")
        ++ (lib "NuGet.Configuration" "net45")
        ++ (lib "NuGet.Packaging" "net45")
        ++ (lib "NuGet.Protocol.Types" "net45")
        ++ (lib "NuGet.Protocol.V2V3" "net45")

    CreateDir "lib"
    CopyFiles "lib" files
)

Target "CreatePackage" (fun _ ->
    let files = [
        "NuGetCalc.psd1", Some @"tools\NuGetCalc", None
        "NuGetCalc.psm1", Some @"tools\NuGetCalc", None
        "Init.ps1", Some "tools", None]
    let libs = !! @".\lib\*.dll" |> Seq.map (fun file -> file, Some @"tools\NuGetCalc\lib", None)

    let version =
        Regex.Match((ReadFileAsString "NuGetCalc.psd1"), "ModuleVersion = '([\d\.]+)'")
            .Groups.[1].Value

    NuGetPack (fun p ->
        { p with
            OutputPath = "."
            WorkingDir = "."
            Version = version
            Files = files |> List.append (Seq.toList libs) })
        "NuGetCalc.nuspec"
)

"RestorePackages" ==> "CreatePackage"

RunTargetOrDefault "CreatePackage"
