// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r @"packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open Fake.UserInputHelper
open System
open System.IO

// --------------------------------------------------------------------------------------
// START TODO: Provide project-specific details below
// --------------------------------------------------------------------------------------

// Information about the project are used
//  - for version and project name in generated AssemblyInfo file
//  - by the generated NuGet package
//  - to run tests and to publish documentation on GitHub gh-pages
//  - for documentation, you also need to edit info in "docs/tools/generate.fsx"

// The name of the project
// (used by attributes in AssemblyInfo, name of a NuGet package and directory in 'src')
let project = "genit"

// Short summary of the project
// (used as description in AssemblyInfo and as a short summary for NuGet package)
let summary = "A self contained basic load testing tool"

// Longer description of the project
// (used as a description for NuGet package; line breaks are automatically cleaned up)
let description = "A self contained basic load testing tool"

// List of author names (for NuGet package)
let authors = [ "Chris Holt" ]

// Tags for your project (for NuGet package)
let tags = "f# load testing"

// File system information
let solutionFile  = "genit.sln"

// Pattern specifying assemblies to be tested using NUnit
let exe = currentDirectory @@ "/bin/"

// --------------------------------------------------------------------------------------
// END TODO: The rest of the file includes standard build steps
// --------------------------------------------------------------------------------------

// Helper active pattern for project types
let (|Fsproj|Csproj|Vbproj|) (projFileName:string) =
    match projFileName with
    | f when f.EndsWith("fsproj") -> Fsproj
    | f when f.EndsWith("csproj") -> Csproj
    | f when f.EndsWith("vbproj") -> Vbproj
    | _                           -> failwith (sprintf "Project file %s not supported. Unknown project type." projFileName)

// Generate assembly info files with the right version & up-to-date information
Target "AssemblyInfo" (fun _ ->
    let getAssemblyInfoAttributes projectName =
        [ Attribute.Title (projectName)
          Attribute.Product project
          Attribute.Description summary
          Attribute.Version "1.0"
          Attribute.FileVersion "1.0" ]

    let getProjectDetails projectPath =
        let projectName = System.IO.Path.GetFileNameWithoutExtension(projectPath)
        ( projectPath,
          projectName,
          System.IO.Path.GetDirectoryName(projectPath),
          (getAssemblyInfoAttributes projectName)
        )

    !! "**/*.??proj"
    |> Seq.map getProjectDetails
    |> Seq.iter (fun (projFileName, projectName, folderName, attributes) ->
        match projFileName with
        | Fsproj -> CreateFSharpAssemblyInfo (folderName @@ "AssemblyInfo.fs") attributes
        | Csproj -> CreateCSharpAssemblyInfo ((folderName @@ "Properties") @@ "AssemblyInfo.cs") attributes
        | Vbproj -> CreateVisualBasicAssemblyInfo ((folderName @@ "My Project") @@ "AssemblyInfo.vb") attributes
        )
)

// Copies binaries from default VS location to expected bin folder
// But keeps a subdirectory structure for each project in the
// src folder to support multiple project outputs
Target "CopyBinaries" (fun _ ->
    let target = currentDirectory @@ "/bin"
    let source = currentDirectory @@ "/src/genit/bin/Debug"
    FileHelper.CopyDir target source (fun _ -> true)
)

Target "CopyAssets" (fun _ ->
    let target = currentDirectory @@ "/bin/css"
    let source = currentDirectory @@ "/src/genit/css"
    FileHelper.CopyDir target source (fun _ -> true)

    let target = currentDirectory @@ "/bin/js"
    let source = currentDirectory @@ "/src/genit/js"
    FileHelper.CopyDir target source (fun _ -> true)

    //let target = currentDirectory @@ "/bin/fonts"
    //let source = currentDirectory @@ "/src/genit/fonts"
    //FileHelper.CopyDir target source (fun _ -> true)
)

// --------------------------------------------------------------------------------------
// Clean build results

Target "Clean" (fun _ ->
    CleanDirs ["bin"; "temp"]
)

// --------------------------------------------------------------------------------------
// Build library & test project

Target "Build" (fun _ ->
    !! solutionFile
    |> MSBuildDebug "" "Rebuild"
    |> ignore
)


// --------------------------------------------------------------------------------------
// Run stuff
let execProcess arg =
  ExecProcess
    (fun info ->
     info.FileName <- (exe @@ "genit.exe")
     info.Arguments <- arg
    ) (System.TimeSpan.FromMinutes 60.)
  |> ignore

Target "RunSite" (fun _ -> execProcess "")

Target "Generate" (fun _ -> execProcess "generate")

Target "Test" (fun _ -> execProcess "test")

Target "CreateDB" (fun _ ->
  let path template = String.Format(template, System.IO.Path.DirectorySeparatorChar)
  let dbtype = System.IO.File.ReadAllText(path "src{0}genit{0}generated{0}generated_dbtype.txt")

  if dbtype = "Postgres" then
    let dbname = System.IO.File.ReadAllText(path "src{0}genit{0}generated{0}generated_dbname.txt")
    let command = "psql"
    let args = path "-a -f src{0}genit{0}generated{0}generated_sql_createdb.sql"
    Shell.Exec(command, args) |> ignore

    let args = path (sprintf "-d %s -a -f src{0}genit{0}generated{0}generated_sql_initialSetup.sql" dbname)
    Shell.Exec(command, args) |> ignore

    let args = path (sprintf "-d %s -a -f src{0}genit{0}generated{0}generated_sql_createTables.sql" dbname)
    Shell.Exec(command, args) |> ignore

  else if dbtype = "SQLServer" then
    let command = "sqlcmd"
    let args = path "-S (localDB)\MSSQLLocalDB -i src{0}genit{0}generated{0}generated_sql_createdb.sql"
    Shell.Exec(command, args) |> ignore

    let args = path "-S (localDB)\MSSQLLocalDB -i src{0}genit{0}generated{0}generated_sql_initialSetup.sql"
    Shell.Exec(command, args) |> ignore

    let args = path "-S (localDB)\MSSQLLocalDB -i src{0}genit{0}generated{0}generated_sql_createTables.sql"
    Shell.Exec(command, args) |> ignore

  else failwith (sprintf "Database type %s is not supported" dbtype)
)

Target "Help" (fun _ ->
  printfn "build.cmd [<target>] [options]"
  printfn @"for FAKE help: packages\FAKE\tools\FAKE.exe --help"
  printfn "targets:"
  printfn "  * `Clean` deletes bin and temp directories"
  printfn "  * `Build` builds the site generator tool `genit.exe`"
  printfn "  * `Generate` runs the generator to generate a site"
  printfn "  * `CreateDB` creates a PostgreSQL database from the generated SQL schema"
  printfn "  * `RunSite` runs the generated web site"
  printfn "  * `Test` runs the test suite"
  printfn "  * `Help` prints this message"
)

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Target "Bin" DoNothing

Target "All" DoNothing

"Clean"
  ==> "AssemblyInfo"
  ==> "Build"

"Build"
  ==> "CopyBinaries"
  ==> "Bin"

"Bin"
  ==> "Generate"

"Bin"
  ==> "CreateDB"

"Bin"
  ==> "Test"

"Bin"
  ==> "CopyAssets"
  ==> "RunSite"

RunTargetOrDefault "Help"
