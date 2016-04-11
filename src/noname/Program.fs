module Main

open Suave
open Suave.Web
open Suave.Filters
open Suave.Operators
open Suave.Successful
open forms

let routes =
  choose
    (generated_paths.generated_routes @
    [
      pathRegex "(.*)\.(css|png|gif|js|ico|woff|tff)" >=> Files.browseHome
    ])

let args = System.Environment.GetCommandLineArgs()
let generate = args |> Array.exists (fun arg -> arg = "generate")
let test = args |> Array.exists (fun arg -> arg = "test")

if generate then
  generator.generate <| script.someSite()
else if test then
  generated_unittests.run()
  generated_uitests.run()
else
  startWebServer defaultConfig routes
