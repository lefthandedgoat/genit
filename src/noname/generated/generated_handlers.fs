module generated_handlers

open Suave
open Suave.Filters
open Suave.Successful
open Suave.Operators
open generated_views

let register = choose [ GET >=> OK get_register]