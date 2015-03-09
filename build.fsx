#r "packages/FAKE/tools/FakeLib.dll"

open System.Text.RegularExpressions
open Fake

let libDir = @"NuGetCalc\lib"

Target "RestorePackages" (fun _ ->
    DeleteDir libDir

    RestorePackageId (fun p ->
        { p with
             Sources = ["https://api.nuget.org/v2/"]
             IncludePreRelease = true
             ExcludeVersion = true })
        "NuGet.Protocol.V2V3"

    let lib package framework =
        sprintf @".\packages\%s\lib\%s\*.dll" package framework
    
    let files =
        !! (lib "Microsoft.Web.Xdt" "net40")
        ++ (lib "Newtonsoft.Json" "net45")
        ++ (lib "Nuget.Core" "net40-Client")
        ++ (lib "NuGet.Versioning" "portable-net40+win")
        ++ (lib "NuGet.Configuration" "net45")
        ++ (lib "NuGet.Packaging" "net45")
        ++ (lib "NuGet.Protocol.Types" "net45")
        ++ (lib "NuGet.Protocol.V2V3" "net45")
    
    CreateDir libDir
    CopyFiles libDir files
)

let fetchVersion =
    Regex.Match((ReadFileAsString @"NuGetCalc\NuGetCalc.psd1"), "ModuleVersion = '([\d\.]+)'")
        .Groups.[1].Value

Target "CreatePackage" (fun _ ->
    let files = [
        @"NuGetCalc\NuGetCalc.psd1", Some @"tools\NuGetCalc", None
        @"NuGetCalc\NuGetCalc.psm1", Some @"tools\NuGetCalc", None
        "Init.ps1", Some "tools", None]
    let libs = !! (libDir @@ "*.dll") |> Seq.map (fun file -> file, Some @"tools\NuGetCalc\lib", None)

    NuGetPack (fun p ->
        { p with
            OutputPath = "."
            WorkingDir = "."
            Version = fetchVersion
            Files = files |> List.append (Seq.toList libs) })
        "NuGetCalc.nuspec"
)

Target "CreateZip" (fun _ ->
    Zip "." (sprintf "NuGetCalc.%s.zip" fetchVersion) (!! @"NuGetCalc\**\*" ++ "*.md")
)

Target "Default" DoNothing

"RestorePackages" ==> "CreatePackage" ==> "Default"
"RestorePackages" ==> "CreateZip" ==> "Default"

RunTargetOrDefault "Default"
