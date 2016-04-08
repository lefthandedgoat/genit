module Main

open Suave
open Suave.Web
open Suave.Filters
open Suave.Operators
open Suave.Successful
open forms

let hello =
  //a warbler will make it re-rerun the function on every page load, so that it is not static
  warbler (fun _ ->
    let now = System.DateTime.Now
    OK <| pages.hello now)

let form =
  choose [
    GET >=> OK pages.form
    POST >=> bindToForm registerForm (fun registerForm ->
      let form = forms.convertRegisterForm registerForm
      let message = sprintf "Parsed form: \r\n%A" form
      OK message)
  ]

let grid =
  warbler (fun _ ->
    let cars = database.getCars()
    OK <| pages.grid cars)

let routes =
  choose
    [
      GET >=> choose [
        path paths.root >=> OK pages.root
        path paths.hello >=> hello
        path paths.grid >=> grid
      ]

      path paths.form >=> form

      pathRegex "(.*)\.(css|png|gif|js|ico|woff|tff)" >=> Files.browseHome
    ]

startWebServer defaultConfig routes
