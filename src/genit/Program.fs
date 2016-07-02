module Main

open Suave
open Suave.Web
open Suave.Filters
open Suave.Operators
open Suave.Successful
open forms
open loadGenerator

let mutable results : Result list = []
let loadtest =
  choose [
    GET >=> OK (loadGeneratorView.html defaultLoadTest [])

    POST >=> bindToForm loadTestForm (fun loadTestForm ->
      let loadTest = convertLoadTest loadTestForm
      let manager = newManager()
      let result = manager.PostAndReply(fun channel -> Initialize(loadTest, Process loadTest.URI, channel))
      //keep history
      results <- result::results
      OK (loadGeneratorView.html loadTest results))
    ]

let routes =
  choose
    (generated_paths.generated_routes @
    [
      path "/loadtest" >=> loadtest
      Files.browseHome
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
  let config = { defaultConfig with serverKey = generated_security.serverKey }
  startWebServer config routes
