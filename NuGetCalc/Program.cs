//            DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//   TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
//
//  0. You just DO WHAT THE FUCK YOU WANT TO.
//
// See http://www.wtfpl.net/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.Versioning;
using NuGet;

namespace NuGetCalc
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2 && args.Length != 3)
            {
                ShowHelp();
                return;
            }

            var verbose = args[0] == "-v";

            if (verbose && args.Length != 3)
            {
                ShowHelp();
                return;
            }

            var packageName = verbose ? args[1] : args[0];
            var target = VersionUtility.ParseFrameworkName(verbose ? args[2] : args[1]);

            string packageFile;
            if (packageName.EndsWith(".nupkg"))
            {
                packageFile = packageName;
            }
            else if (!packageName.Any(c => c == '/' || c == '\\'))
            {
                try
                {
                    packageFile = Path.GetTempFileName();
                    using (var wc = new WebClient())
                        wc.DownloadFile("http://www.nuget.org/api/v2/package/" + Uri.EscapeDataString(packageName), packageFile);
                }
                catch
                {
                    Console.WriteLine("Couldn't download the package.");
                    return;
                }
            }
            else
            {
                ShowHelp();
                return;
            }

            var package = new ZipPackage(packageFile);
            var assemblies = package.AssemblyReferences;

            IEnumerable<IPackageAssemblyReference> result;
            if (VersionUtility.TryGetCompatibleItems(target, assemblies, out result))
            {
                if (verbose)
                {
                    // The following code is from NuGet.Core
                    // Copyright 2010-2014 Outercurve Foundation
                    // http://www.apache.org/licenses/LICENSE-2.0
                    var normalizedItems = from item in assemblies
                                          let frameworks = (item.SupportedFrameworks != null && item.SupportedFrameworks.Any()) ? item.SupportedFrameworks : new FrameworkName[] { null }
                                          from framework in frameworks
                                          select new
                                          {
                                              Item = item,
                                              TargetFramework = framework
                                          };
                    var frameworkGroups = normalizedItems.GroupBy(g => g.TargetFramework, g => g.Item).ToList();
                    // End the code from NuGet.Core
                    var list = frameworkGroups.Where(g => g.Key != null && VersionUtility.IsCompatible(target, new[] { g.Key }))
                        .Select(g => Tuple.Create(g.Key, GetProfileCompatibility(target, g.Key)))
                        .OrderByDescending(t => t.Item2);
                    foreach (var t in list)
                        Console.WriteLine("{0}\t{1}", t.Item1.ToString(), t.Item2);
                }
                else
                {
                    foreach (var f in result.First().SupportedFrameworks)
                        Console.WriteLine(f.ToString());
                }
            }
            else
            {
                Console.WriteLine("No compatible assembly.");
            }
        }

        static void ShowHelp()
        {

        }

        private static Func<FrameworkName, FrameworkName, long> getProfileCompatibilityCache;
        static long GetProfileCompatibility(FrameworkName projectFrameworkName, FrameworkName packageTargetFrameworkName)
        {
            if (getProfileCompatibilityCache == null)
            {
                var arg0 = Expression.Parameter(typeof(FrameworkName));
                var arg1 = Expression.Parameter(typeof(FrameworkName));
                getProfileCompatibilityCache = Expression.Lambda<Func<FrameworkName, FrameworkName, long>>(
                    Expression.Call(
                        typeof(VersionUtility).GetMethod("GetProfileCompatibility", BindingFlags.NonPublic | BindingFlags.Static),
                        arg0, arg1, Expression.Constant(NetPortableProfileTable.Default)
                    ),
                    arg0, arg1
                ).Compile();
            }

            return getProfileCompatibilityCache(projectFrameworkName, packageTargetFrameworkName);
        }
    }
}
