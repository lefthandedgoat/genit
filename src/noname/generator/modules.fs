module modules

let generated_html_template guts =
  sprintf """module generated_html

open System
open Suave.Html
open htmlHelpers
open bootstrapHelpers

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
open htmlHelpers
open bootstrapHelpers
open generated_html
open generated_forms

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
open generated_handlers

%s

let generated_routes =
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
open generated_forms
open generated_types
open generated_validation
open generated_data_access
open forms

%s""" guts

let generated_types_template guts =
  sprintf """module generated_types

%s""" guts

let generated_data_access_template connectionString guts =
  sprintf """module generated_data_access

open generated_types
open generated_forms
open adoHelper
open Npgsql

let connectionString = "%s"

%s""" connectionString guts

let generated_forms_template guts =
  sprintf """module generated_forms

open Suave.Model.Binding
open Suave.Form
open generated_types

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
