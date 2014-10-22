// include Fake lib
#r "packages/FAKE/tools/FakeLib.dll"
open Fake

RestorePackages()

// Properties
let buildDir = "./build/"
let testDir  = "./test/"
let packageDir = "./package/"
let packageOutput = "./packageOut/"

let revision = int ((System.DateTime.Now - System.DateTime(2000, 1, 1)).TotalMinutes * 2.0)
let version = sprintf "0.8.%i" revision


// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; testDir; packageDir; packageOutput]
)

Target "BuildApp" (fun _ ->
   !! "src/Mvvm/Mvvm/Mvvm.csproj"
   ++ "src/MvvmFS/MvvmFS.fsproj"
     |> MSBuildRelease buildDir "Build"
     |> Log "AppBuild-Output: "
)

Target "BuildTest" (fun _ ->
    !! "src/MvvmTests/**/*.csproj"
    -- "./**/*WP8.csproj"
      |> MSBuildDebug testDir "Build"
      |> Log "TestBuild-Output: "
)

Target "Test" (fun _ ->
    !! (testDir + "*Test*.dll")
      |> MSTest.MSTest (fun p ->
          {p with
             WorkingDir = testDir }))

Target "CreatePackage" (fun _ ->
    NuGet (fun p -> 
        {p with
            Authors = [ "lrieger" ]
            Project = "Mvvm"
            Description = "--todo--"                               
            OutputPath = packageOutput
            Summary = "--todo--"
            WorkingDir = buildDir
            Version = version
            AccessKey = "{6DE0F6A0-2A5B-4467-8A41-41F86A6F3D29}"
            Publish = true
            PublishUrl = "http://nugetfeedlr.azurewebsites.net/"
            References = [ "Mvvm.dll" ; "MvvmFS.dll" ]
            Files = [ (@"**\*.*", Some "lib", None) ] })
            "mvvm.nuspec"
)

Target "Default" (fun _ ->
    trace "Hello World from FAKE"
)

// Dependencies
"Clean"
  ==> "BuildApp"
  ==> "BuildTest"
  ==> "Test"
  ==> "CreatePackage"
  ==> "Default"

// start build
RunTargetOrDefault "Default"