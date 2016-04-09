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
if args |> Array.exists (fun arg -> arg = "generate")
then generator.generate <| script.someSite()
else startWebServer defaultConfig routes
