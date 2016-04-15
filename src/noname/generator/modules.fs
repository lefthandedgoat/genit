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
open generated_data_access
open generated_types

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

type IntPath = PrintfFormat<(int -> string),unit,string,string,int>

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
open Suave.Redirection
open Suave.Operators
open generated_views
open generated_forms
open generated_types
open generated_validation
open generated_data_access
open generated_fake_data
open htmlHelpers

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

let generated_fake_data_template guts =
  sprintf """module generated_fake_data

open generated_types

let concat values =
  if values = []
  then ""
  else values |> List.reduce (fun value1 value2 -> value1 + " " + value2)

let random = System.Random()
let randomItem (items : 'a list) = items.[random.Next(items.Length)]
let randomItems number items =
  if number = 1
  then randomItem items
  else
    let number = random.Next(number)
    [ 1 .. (number + 1 ) ] |> List.map (fun _ -> items.[random.Next(items.Length)]) |> concat

let words = ["At"; "vero"; "eos"; "et"; "accusamus"; "et"; "iusto"; "odio"; "dignissimos"; "ducimus"; "qui"; "blanditiis"; "praesentium"; "voluptatum"; "atque"; "corrupti"; "quos"; "dolores"; "et"; "quas"; "molestias"; "excepturi"; "sint"; "occaecati"; "cupiditate"; "non"; "provident"; "similique"; "sunt"; "in"; "culpa"; "qui"; "officia"; "deserunt"; "mollitia"; "animi"; "id"; "est"; "laborum"; "omnis"; "dolor"; "repellendus"; "Temporibus"; "autem"; "quibusdam"; "et"; "aut"; "officiis"; "debitis"; "aut"; "rerum"; "necessitatibus"; "saepe"; "eveniet"; "ut"; "et"; "oluptates"; "repudiandae"; "sint"; "et"; "molestiae"; "non"; "recusandae"; "Itaque"; "earum"; "rerum"; "hic"; "tenetur"; "sapiente"; "delectus"; "ut"; "aut"; "reiciendis"; "voluptatibus"; "maiores"; "alias"; "consequatur"; "aut"; "perferendis"; "doloribus"; "asperiores"; "repellat"; ]
let names = [ "James";"Mary";"John";"Patricia";"Robert";"Jennifer";"Michael";"Elizabeth";"William";"Linda";"David";"Barbara";"Richard";"Susan";"Joseph";"Margaret";"Charles";"Jessica";"Thomas";"Sarah";"Christopher";"Dorothy";"Daniel";"Karen";"Matthew";"Nancy";"Donald";"Betty";"Anthony";"Lisa";"Mark";"Sandra";"Paul";"Ashley";"Steven";"Kimberly";"George";"Donna";"Kenneth";"Helen";"Andrew";"Carol";"Edward";"Michelle";"Joshua";"Emily";"Brian";"Amanda";"Kevin";"Melissa";"Ronald";"Deborah";"Timothy";"Laura";"Jason";"Stephanie";"Jeffrey";"Rebecca";"Ryan";"Sharon";"Gary";"Cynthia";"Nicholas";"Kathleen";"Eric";"Anna";"Jacob";"Shirley";"Stephen";"Ruth";"Jonathan";"Amy";"Larry";"Angela";"Frank";"Brenda";"Scott";"Virginia";"Justin";"Pamela";"Catherine";"Raymond";"Katherine";"Gregory";"Nicole";"Samuel";"Christine";"Benjamin";"Samantha";"Patrick";"Janet";"Jack";"Debra";"Dennis";"Carolyn";"Alexander";"Rachel";"Jerry";"Heather"]
let zips = [ 75028; 75220; 75233; 76701; 76531 ]
let cities = [ "Dallas";"Fort Worth";"Arlington";"Plano";"Garland";"Irving";"Grand Prairie";"McKinney";"Mesquite";"Frisco";"Carrollton";"Denton";"Richardson";"Lewisville" ]

%s
  """ guts
