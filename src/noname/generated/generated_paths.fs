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

let path_register = "/register" 

let generated_routes : WebPart<'a> list =
  [
    path path_register >=> register
  ]