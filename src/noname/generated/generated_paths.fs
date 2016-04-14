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

type IntPath = PrintfFormat<(int -> string),unit,string,string,int>

let generated_routes =
  [
  ]
