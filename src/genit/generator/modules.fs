module modules

open sql

let generated_html_template guts =
  sprintf """module generated_html

open System
open Suave.Html
open helper_html
open helper_bootstrap

let base_header brand =
  navClass "navbar navbar-default" [
    container [
      navbar_header [
        buttonAttr
          ["type","button"; "class","navbar-toggle collapsed"; "data-toggle","collapse"; "data-target","#navbar"; "aria-expanded","false"; "aria-controls","navbar" ]
          [
            spanClass "sr-only" [text "Toggle navigation"]
            spanClass "icon-bar" [emptyText]
            spanClass "icon-bar" [emptyText]
            spanClass "icon-bar" [emptyText]
          ]
        navbar_brand [ text brand ]
      ]
      navbar [
        navbar_nav [
          dropdown [
            dropdown_toggle [text "Pages "; caret]
            dropdown_menu [
%s
            ]
          ]
        ]
      ]
    ]
  ]

  """ guts

let generated_views_template brand guts =
  sprintf """module generated_views

open Suave.Html
open helper_html
open helper_bootstrap
open generated_html
open generated_forms
open generated_data_access
open generated_types
open generator
open helper_general

let brand = "%s"
%s""" brand guts

let generated_paths_template paths routes =
  sprintf """module generated_paths

open Suave
open Suave.Filters
open Suave.Successful
open Suave.Operators
open Suave.Model.Binding
open Suave.Form
open Suave.ServerErrors
open forms
open helper_handler
open generated_handlers

type Int64Path = PrintfFormat<(int64 -> string),unit,string,string,int64>

%s

let generated_routes =
  [
%s
  ]""" paths routes

let generated_handlers_template guts =
  sprintf """module generated_handlers

open System.Web
open Suave
open Suave.Authentication
open Suave.State.CookieStateStore
open Suave.Filters
open Suave.Successful
open Suave.Redirection
open Suave.Operators
open generated_views
open generated_forms
open generated_types
open generated_validation
open generated_data_access
open generated_fake_data
open generated_bundles
open helper_html
open helper_handler
open Nessos.FsPickler
open Nessos.FsPickler.Json
open forms

%s""" guts

let generated_types_template guts =
  sprintf """module generated_types

%s""" guts

let generated_data_access_template (engine:Engine) connectionString guts =
  match engine with
  | MicrosoftSQL -> mssql.generated_data_access_template connectionString guts

let generated_forms_template guts =
  sprintf """module generated_forms

open Suave.Model.Binding
open Suave.Form
open generated_types
open generated_data_access

%s""" guts

let generated_validation_template guts =
  sprintf """module generated_validation

open generated_forms
open validators

%s""" guts

let generated_unittests_template guts =
  sprintf """module generated_unittests

open generated_forms
open generated_validation

let run () =
%s
  ()
""" guts

let generated_uitests_template guts =
  sprintf """module generated_uitests

open generated_forms
open generated_validation
open canopy

let run () =
  start firefox

%s

  canopy.runner.run()

  quit()
""" guts

let generated_fake_data_template guts =
  sprintf """module generated_fake_data

open generated_types
open generated_data_access
open helper_general

%s
  """ guts

let generated_bundles_template guts =
  sprintf """module generated_bundles

open forms
open helper_general
open generated_fake_data
open generated_types
open generated_views
open generated_data_access
open generated_forms
open generated_validation

%s
  """ guts

let generated_security_template guts =
  sprintf """module generated_security

%s
  """ guts
