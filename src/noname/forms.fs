module forms

open Suave
open Suave.Model.Binding
open Suave.Form
open Suave.ServerErrors
open Microsoft.FSharp.Reflection
open loadGenerator

let fromString<'a> s =
  match FSharpType.GetUnionCases typeof<'a> |> Array.filter (fun case -> case.Name = s) with
    | [|case|] -> FSharpValue.MakeUnion(case,[||]) :?> 'a
    | _ -> failwith <| sprintf "Can't convert %s to DU" s

let logAndShow500 error =
  printfn "%A" error
  INTERNAL_ERROR "ERROR"

let bindToForm form handler =
  bindReq (bindForm form) handler logAndShow500

let getQueryStringValue (req : HttpRequest) queryStringKey =
  match (req.queryParam queryStringKey) with
  | Choice1Of2 value -> value
  | _ -> ""

let hasQueryString (req : HttpRequest) queryStringKey =
  match (req.queryParam queryStringKey) with
  | Choice1Of2 _ -> true
  | _ -> false

type SearchForm =
  {
    Field : string
    How : string
    Value : string
  }

let searchForm : Form<SearchForm> = Form ([],[])
let loadTestForm : Form<LoadTestForm> = Form ([],[])
