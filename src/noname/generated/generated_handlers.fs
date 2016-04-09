module generated_handlers

open Suave
open Suave.Filters
open Suave.Successful
open Suave.Operators
open generated_views
open generated_forms
open generated_types
open forms

let register =
  choose
    [
      GET >=> OK get_register
      POST >=> bindToForm registerForm (fun registerForm ->
        let form = convertRegisterForm registerForm
        let message = sprintf "Parsed form: \r\n%A" form
        OK message)
    ]
