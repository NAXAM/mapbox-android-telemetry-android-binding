#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0

// Cake Addins
#addin nuget:?package=Cake.FileHelpers&version=2.0.0

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var VERSION= "4.4.1";
var NUGET_SUFIX = ".0";
var ANDROID_CORE_VERSION = "1.3.0";

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var artifacts = new [] {
    
     new Artifact {
        Version =ANDROID_CORE_VERSION + NUGET_SUFIX,
        NativeVersion = ANDROID_CORE_VERSION,
        ReleaseNotes = new string [] {
            "Mapbox for Android Core - v{0}"
        },
        SolutionPath = "./Mapbox.Services.Android.Telemetry.sln",
        AssemblyInfoPath = "./Naxam.Mapbox.MapboxAndroidCore/Properties/AssemblyInfo.cs",
        NuspecPath = "./mapboxandroidcore.nuspec",
        DownloadUrl = "http://central.maven.org/maven2/com/mapbox/mapboxsdk/mapbox-android-core/{0}/mapbox-android-core-{0}.aar",
        JarPath = "./Naxam.Mapbox.MapboxAndroidCore/Jars/mapbox-android-core.aar",
        Dependencies = new NuSpecDependency[] {}
    },
    new Artifact {
        Version = VERSION + NUGET_SUFIX,
        NativeVersion = VERSION,
        ReleaseNotes = new string [] {
            "Mapbox for Android - SdkCore v{0}"
        },
        SolutionPath = "./Mapbox.Services.Android.Telemetry.sln",
        AssemblyInfoPath = "./Naxam.Mapbox.Services.Android.Telemetry/Properties/AssemblyInfo.cs",
        NuspecPath = "./telemetry.nuspec",
        DownloadUrl = "http://central.maven.org/maven2/com/mapbox/mapboxsdk/mapbox-android-telemetry/{0}/mapbox-android-telemetry-{0}.aar",
        JarPath = "./Naxam.Mapbox.Services.Android.Telemetry/Jars/mapbox-android-telemetry.aar",
        Dependencies = new NuSpecDependency[] {
                new NuSpecDependency {
                    Id = "Xamarin.Android.Support.Annotations",
                    Version = "28.0.0.1"
                },
                new NuSpecDependency {
                    Id = "Xamarin.Android.Support.Core.Utils",
                    Version = "28.0.0.1"
                },
                new NuSpecDependency {
                    Id = "Square.OkHttp3",
                    Version = "3.8.1"
                },
                new NuSpecDependency {
                    Id = "Naxam.Mapbox.MapboxAndroidCore",
                    Version = ANDROID_CORE_VERSION + NUGET_SUFIX
                },
                new NuSpecDependency {
                    Id = "GoogleGson",
                    Version = "2.8.5"
                }
        }
    }
};

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Downloads")
    .Does(() =>
{
    foreach(var artifact in artifacts) {
        var downloadUrl = string.Format(artifact.DownloadUrl, artifact.NativeVersion);
        var jarPath = string.Format(artifact.JarPath, artifact.NativeVersion);

        DownloadFile(downloadUrl, jarPath);
    }
});

Task("Clean")
    .Does(() =>
{
    CleanDirectory("packages");

    CleanDirectory("./nugets");

    var nugetPackages = GetFiles("./nugets/*.nupkg");

    foreach (var package in nugetPackages)
    {
        DeleteFile(package);
    }
});

Task("UpdateVersion")
    .Does(() => 
{
    foreach(var artifact in artifacts) {
        ReplaceRegexInFiles(artifact.AssemblyInfoPath, "\\[assembly\\: AssemblyVersion([^\\]]+)\\]", string.Format("[assembly: AssemblyVersion(\"{0}\")]", artifact.Version));
    }
});

Task("Pack")
    .Does(() =>
{
    foreach(var artifact in artifacts) {
        NuGetRestore(artifact.SolutionPath);
        MSBuild(artifact.SolutionPath, settings => {
            settings.ToolVersion = MSBuildToolVersion.VS2019;
            settings.SetConfiguration(configuration);
        });
        NuGetPack(artifact.NuspecPath, new NuGetPackSettings {
            Version = artifact.Version,
            Dependencies = artifact.Dependencies,
            ReleaseNotes = artifact.ReleaseNotes,
            OutputDirectory = "./nugets"
        });
    }
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Downloads")
    .IsDependentOn("UpdateVersion")
    .IsDependentOn("Clean")
    .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

class Artifact {
    public string Version { get; set; }
    public string NativeVersion { get; set; }

    public string AssemblyInfoPath { get; set; }

    public string SolutionPath { get; set; }

    public string DownloadUrl  { get; set; }

    public string JarPath { get; set; }

    public string NuspecPath { get; set; }

    public string[] ReleaseNotes { get; set; }

    public NuSpecDependency[] Dependencies { get; set; }
}