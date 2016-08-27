module generated_paths

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

let path_home = "/"
let path_register = "/register"
let path_login = "/login"
let path_create_self = "/self/create"
let path_generate_self : Int64Path = "/self/generate/%i"
let path_view_self : Int64Path = "/self/view/%i"
let path_edit_self : Int64Path = "/self/edit/%i"
let path_list_self = "/self/list"
let path_search_self = "/self/search"
let path_create_customer = "/customer/create"
let path_generate_customer : Int64Path = "/customer/generate/%i"
let path_view_customer : Int64Path = "/customer/view/%i"
let path_edit_customer : Int64Path = "/customer/edit/%i"
let path_list_customer = "/customer/list"
let path_create_employeeSurvey = "/employeeSurvey/create"
let path_generate_employeeSurvey : Int64Path = "/employeeSurvey/generate/%i"
let path_view_employeeSurvey : Int64Path = "/employeeSurvey/view/%i"
let path_edit_employeeSurvey : Int64Path = "/employeeSurvey/edit/%i"
let path_list_employeeSurvey = "/employeeSurvey/list"
let path_search_employeeSurvey = "/employeeSurvey/search"
let path_create_customerSurvey = "/customerSurvey/create"
let path_generate_customerSurvey : Int64Path = "/customerSurvey/generate/%i"
let path_view_customerSurvey : Int64Path = "/customerSurvey/view/%i"
let path_edit_customerSurvey : Int64Path = "/customerSurvey/edit/%i"
let path_list_customerSurvey = "/customerSurvey/list"
let path_search_customerSurvey = "/customerSurvey/search"

let generated_routes =
  [
    path path_home >=> home
    path path_register >=> register
    path path_login >=> login
    path path_create_self >=> create_self
    pathScan path_generate_self generate_self
    pathScan path_view_self view_self
    pathScan path_edit_self edit_self
    path path_list_self >=> list_self
    path path_search_self >=> search_self
    path path_create_customer >=> loggedOn path_login create_customer
    pathScan path_generate_customer (fun id -> loggedOn path_login (generate_customer id))
    pathScan path_view_customer (fun id -> loggedOn path_login (view_customer id))
    pathScan path_edit_customer (fun id -> loggedOn path_login (edit_customer id))
    path path_list_customer >=> loggedOn path_login list_customer
    path path_create_employeeSurvey >=> create_employeeSurvey
    pathScan path_generate_employeeSurvey generate_employeeSurvey
    pathScan path_view_employeeSurvey view_employeeSurvey
    pathScan path_edit_employeeSurvey edit_employeeSurvey
    path path_list_employeeSurvey >=> list_employeeSurvey
    path path_search_employeeSurvey >=> search_employeeSurvey
    path path_create_customerSurvey >=> create_customerSurvey
    pathScan path_generate_customerSurvey generate_customerSurvey
    pathScan path_view_customerSurvey view_customerSurvey
    pathScan path_edit_customerSurvey edit_customerSurvey
    path path_list_customerSurvey >=> list_customerSurvey
    path path_search_customerSurvey >=> search_customerSurvey
  ]