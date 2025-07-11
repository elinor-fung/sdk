// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.NET.Build.Tests
{
    public class NupkgWithRuntimeAssetsFixture
    {
        private readonly Dictionary<string, string> _packagePathsByTargetFramework = [];

        public const string PackageId = "TestPackage";

        public static (string, string)[] AssemblyPaths(string targetFramework) =>
        [
            ($"lib/{targetFramework}/lib1.dll", "lib1.dll"),
            ($"lib/{targetFramework}/lib2.dll", "lib2.dll"),
        ];

        public static readonly (string, string)[] NativeLibraryPaths =
        [
            ("runtimes/linux-x64/native/nativelib.so", "runtimes/linux-x64/native/nativelib.so"),
            ("runtimes/osx-x64/native/nativelib.dylib", "runtimes/osx-x64/native/nativelib.dylib"),
            ("runtimes/win-x64/native/nativelib.dll", "runtimes/win-x64/native/nativelib.dll"),
        ];

        public static (string, string)[] ResourcePaths(string targetFramework) =>
        [
            ($"lib/{targetFramework}/fr/testpackage.resources.dll", "fr/testpackage.resources.dll"),
            ($"lib/{targetFramework}/fr/lib.resources.dll", "fr/lib.resources.dll")
        ];

        public string CreatePackage(string targetFramework, TestAssetsManager testAssetsManager)
        {
            if (_packagePathsByTargetFramework.TryGetValue(targetFramework, out string? value))
                return value;

            var packageProject = new TestProject()
            {
                Name = PackageId,
                TargetFrameworks = targetFramework,
            };

            // Add assets.
            (string, string)[] assets = [.. AssemblyPaths(targetFramework), .. NativeLibraryPaths, .. ResourcePaths(targetFramework)];
            foreach ((string PackagePath, string _) in assets)
            {
                packageProject.AddItem("None",
                    new Dictionary<string, string>()
                    {
                        ["Include"] = Path.GetFileName(PackagePath),
                        ["Pack"] = "true",
                        ["PackagePath"] = PackagePath
                    });

                // The tests just need the asset to exist, so just add an empty file
                packageProject.SourceFiles.Add(Path.GetFileName(PackagePath), string.Empty);
            }

            var packCommand = new PackCommand(testAssetsManager.CreateTestProject(packageProject, identifier: targetFramework));
            packCommand.Execute().Should().Pass();

            string packagePath = packCommand.GetNuGetPackage();
            _packagePathsByTargetFramework[targetFramework] = packagePath;
            return packagePath;
        }
    }
}
