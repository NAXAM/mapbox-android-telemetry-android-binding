#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0

// Cake Addins
#addin nuget:?package=Cake.FileHelpers&version=2.0.0

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var VERSION = "3.1.5";
var NUGET_SUFIX = ".1";

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var solutionPath = "./Mapbox.Services.Android.Telemetry.sln";
var artifacts = new [] {
    new Artifact {
        AssemblyInfoPath = "./Naxam.Mapbox.Services.Android.Telemetry/Properties/AssemblyInfo.cs",
        NuspecPath = "./telemetry.nuspec",
        DownloadUrl = "http://central.maven.org/maven2/com/mapbox/mapboxsdk/mapbox-android-telemetry/{0}/mapbox-android-telemetry-{0}.aar",
        JarPath = "./Naxam.Mapbox.Services.Android.Telemetry/Jars/mapbox-android-telemetry.aar"
    }
};

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Downloads")
    .Does(() =>
{
    foreach(var artifact in artifacts) {
        var downloadUrl = string.Format(artifact.DownloadUrl, VERSION);
        var jarPath = string.Format(artifact.JarPath, VERSION);

        DownloadFile(downloadUrl, jarPath);
    }
});

Task("Clean")
    .Does(() =>
{
    CleanDirectory("./packages");

    var nugetPackages = GetFiles("./*.nupkg");

    foreach (var package in nugetPackages)
    {
        DeleteFile(package);
    }
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(solutionPath);
});

Task("Build")
    .IsDependentOn("Downloads")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    MSBuild(solutionPath, settings => settings.SetConfiguration(configuration));
});

Task("UpdateVersion")
    .Does(() => 
{
    foreach(var artifact in artifacts) {
        ReplaceRegexInFiles(artifact.AssemblyInfoPath, "\\[assembly\\: AssemblyVersion([^\\]]+)\\]", string.Format("[assembly: AssemblyVersion(\"{0}\")]", VERSION));
    }
});

Task("Pack")
    .IsDependentOn("UpdateVersion")
    .IsDependentOn("Build")
    .Does(() =>
{
    foreach(var artifact in artifacts) {
        NuGetPack(artifact.NuspecPath, new NuGetPackSettings {
            Version = VERSION+NUGET_SUFIX,
            Dependencies = new []{
                new NuSpecDependency {
                    Id = "Xamarin.Android.Support.v7.AppCompat",
                    Version = "27.0.2.1"
                },
                new NuSpecDependency {
                    Id = "Kwon.Squareup.OkHttp3.OkHttp",
                    Version = "3.10.0.1"
                },
                new NuSpecDependency {
                    Id = "Kwon.Mapbox.Android.Core",
                    Version = "0.2.0.1"
                },
                new NuSpecDependency {
                    Id = "GoogleGson",
                    Version = "2.8.1"
                },
                new NuSpecDependency {
                    Id = "Xamarin.Android.Arch.Lifecycle.Extensions",
                    Version = "1.0.0.1"
                }
            },
            ReleaseNotes = new [] {
                $"Mapbox-Telemetry SDK - DataCollector v{VERSION}"
            }
        });
    }
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

class Artifact {
    public string AssemblyInfoPath { get; set; }

    public string SolutionPath { get; set; }

    public string DownloadUrl  { get; set; }

    public string JarPath { get; set; }

    public string NuspecPath { get; set; }
}