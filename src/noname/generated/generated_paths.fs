module generated_paths

open Suave
open Suave.Filters
open Suave.Successful
open Suave.Operators
open Suave.Model.Binding
open Suave.Form
open Suave.ServerErrors
open forms
open generated_handlers

type Int64Path = PrintfFormat<(int64 -> string),unit,string,string,int64>

let generated_routes =
  [
  ]
