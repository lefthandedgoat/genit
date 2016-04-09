module modules

let generated_views_template brand guts =
  sprintf """module generated_views

open Suave.Html
open htmlHelpers
open bootstrapHelpers

let brand = "%s"
%s""" brand guts

let generated_paths_template paths routes =
  sprintf """module generated_paths

open Suave
open Suave.Filters
open Suave.Successful
open Suave.Operators
open generated_handlers

%s

let generated_routes : WebPart<'a> list =
  [
%s
  ]""" paths routes

let generated_handlers_template guts =
  sprintf """module generated_handlers

open Suave
open Suave.Filters
open Suave.Successful
open Suave.Operators
open generated_views

%s""" guts
