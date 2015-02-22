//            DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//   TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
//
//  0. You just DO WHAT THE FUCK YOU WANT TO.
//
// See http://www.wtfpl.net/

module NuGetCalc

open System
open System.IO
open System.Linq.Expressions
open System.Net
open System.Reflection
open System.Runtime.Versioning
open NuGet

[<assembly: AssemblyTitle("NuGetCalc")>]
[<assembly: AssemblyDescription("Tool to find which assembly NuGet adds to your project")>]
[<assembly: AssemblyProduct("NuGetCalc")>]
[<assembly: AssemblyCopyright("Copyright © 2014 azyobuzin")>]
[<assembly: AssemblyVersion("1.2.0.0")>]
[<assembly: AssemblyFileVersion("1.2.0.0")>]
()

exception ArgumentError

let printHelp() =
    printfn
        @"NuGetCalc %s

Usage: NuGetCalc [-v] PackageName TargetFramework
  PackageName: The package name to download or the file path that ends with "".nupkg""
  TargetFramework: The framework name you want to check
  -v: If -v, it will show you the calculated score inside NuGet

Example: NuGetCalc CoreTweet win8"
        (Assembly.GetExecutingAssembly().GetName().Version.ToString())

let getProfileCompatibility =
    let arg0 = Expression.Parameter(typeof<FrameworkName>)
    let arg1 = Expression.Parameter(typeof<FrameworkName>)
    Expression.Lambda<Func<FrameworkName, FrameworkName, int64>>(
        Expression.Call(
            typeof<VersionUtility>.GetMethod("GetProfileCompatibility", BindingFlags.NonPublic ||| BindingFlags.Static),
            arg0,
            arg1
        ),
        arg0, arg1).Compile()

let core argv =
    let len = argv |> Array.length
    if len <> 2 && len <> 3 then raise ArgumentError
    let verbose = argv.[0] = "-v"
    if verbose && len <> 3 then raise ArgumentError

    let packageName = argv.[if verbose then 1 else 0]
    let target = VersionUtility.ParseFrameworkName(argv.[if verbose then 2 else 1])

    let (packageFile, tempFile) =
        match packageName with
            | s when s.EndsWith(".nupkg") -> (s, None)
            | s when not (s |> Seq.exists (fun c -> c = '/' || c = '\\')) ->
                let tmp = Path.GetTempFileName()
                use wc = new WebClient()
                wc.DownloadFile(
                    "http://www.nuget.org/api/v2/package/" + Uri.EscapeDataString(s),
                    tmp
                )
                (tmp, Some tmp)
            | _ -> raise ArgumentError

    try
        let package = new ZipPackage(packageFile)
        let assemblies = package.AssemblyReferences
        let (b, result) = VersionUtility.TryGetCompatibleItems(target, assemblies)
        if b then
            if verbose then
                assemblies
                    |> Seq.map (fun item ->
                        match item.SupportedFrameworks with
                            | null -> Seq.empty
                            | f -> f)
                    |> Seq.collect (fun f -> f)
                    |> Seq.distinct
                    |> Seq.filter (fun f -> f <> null && VersionUtility.IsCompatible(target, [| f |]))
                    |> Seq.map (fun f -> (f, getProfileCompatibility.Invoke(target, f)))
                    |> Seq.sortBy (fun (f, c) -> -c)
                    |> Seq.iter (fun (f, c) ->
                        printfn "%s" (f.ToString())
                        printfn "%d" c)
            else
                for f in (result |> Seq.head).SupportedFrameworks do
                    printfn "%s" (f.ToString())
                    let dependencies = package.GetCompatiblePackageDependencies(f)
                    if not (dependencies |> Seq.isEmpty) then
                        printfn "Dependencies:"
                        for d in dependencies do
                            printfn "  %O" d
        else
            printfn "No compatible assembly"
    finally
        match tempFile with
            | Some(s) -> File.Delete(s)
            | None -> ()

[<EntryPoint>]
let main argv =
    try
        core argv
    with 
        | ArgumentError -> printHelp()
        | ex -> printfn "%s" (ex.ToString())
    0
