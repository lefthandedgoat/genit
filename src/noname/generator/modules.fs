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
open forms

%s""" guts

let generated_types_template guts =
  sprintf """module generated_types

%s""" guts

let generated_forms_template guts =
  sprintf """module generated_forms

open Suave.Model.Binding
open Suave.Form

%s""" guts
