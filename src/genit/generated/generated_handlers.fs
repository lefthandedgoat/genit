module generated_handlers

open dsl
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


let home = GET >=> OK view_jumbo_home

let register =
  choose
    [
      GET >=> OK view_register
      POST >=> bindToForm registerForm (fun registerForm ->
        let validation = validation_registerForm registerForm
        if validation = [] then
          let converted = convert_registerForm registerForm
          let id = insert_register converted
          setAuthCookieAndRedirect id "/"
        else
          OK (view_errored_register validation registerForm))
    ]

let login =
  choose
    [
      GET >=> (OK <| view_login false "")
      POST >=> request (fun req ->
        bindToForm loginForm (fun loginForm ->
        let validation = validation_loginForm loginForm
        if validation = [] then
          let converted = convert_loginForm loginForm
          let loginAttempt = authenticate converted
          match loginAttempt with
            | Some(loginAttempt) ->
              let returnPath = getQueryStringValue req "returnPath"
              let returnPath = if returnPath = "" then "/" else returnPath
              setAuthCookieAndRedirect id returnPath
            | None -> OK <| view_login true loginForm.Email
        else
          OK (view_errored_login validation loginForm)))
    ]

let create_self =
  choose
    [
      GET >=> warbler (fun _ -> createGET bundle_self)
      POST >=> bindToForm selfForm (fun form -> createPOST form bundle_self)
    ]

let generate_self count =
  choose
    [
      GET >=> warbler (fun _ -> generateGET count bundle_self)
      POST >=> bindToForm selfForm (fun form -> generatePOST form bundle_self)
    ]

let view_self id =
  GET >=> warbler (fun _ -> viewGET id bundle_self)

let edit_self id =
  choose
    [
      GET >=> warbler (fun _ -> editGET id bundle_self)
      POST >=> bindToForm selfForm (fun selfForm -> editPOST id selfForm bundle_self)
    ]

let list_self =
  GET >=> warbler (fun _ -> getMany_self () |> view_list_self |> OK)

let search_self =
  choose
    [
      GET >=> request (fun req -> searchGET req bundle_self)
      POST >=> bindToForm searchForm (fun searchForm -> searchPOST searchForm bundle_self)
    ]

let create_customer =
  choose
    [
      GET >=> warbler (fun _ -> createGET bundle_customer)
      POST >=> bindToForm customerForm (fun form -> createPOST form bundle_customer)
    ]

let generate_customer count =
  choose
    [
      GET >=> warbler (fun _ -> generateGET count bundle_customer)
      POST >=> bindToForm customerForm (fun form -> generatePOST form bundle_customer)
    ]

let view_customer id =
  GET >=> warbler (fun _ -> viewGET id bundle_customer)

let edit_customer id =
  choose
    [
      GET >=> warbler (fun _ -> editGET id bundle_customer)
      POST >=> bindToForm customerForm (fun customerForm -> editPOST id customerForm bundle_customer)
    ]

let list_customer =
  GET >=> warbler (fun _ -> getMany_customer () |> view_list_customer |> OK)

let create_employeeSurvey =
  choose
    [
      GET >=> warbler (fun _ -> createGET bundle_employeeSurvey)
      POST >=> bindToForm employeeSurveyForm (fun form -> createPOST form bundle_employeeSurvey)
    ]

let generate_employeeSurvey count =
  choose
    [
      GET >=> warbler (fun _ -> generateGET count bundle_employeeSurvey)
      POST >=> bindToForm employeeSurveyForm (fun form -> generatePOST form bundle_employeeSurvey)
    ]

let view_employeeSurvey id =
  GET >=> warbler (fun _ -> viewGET id bundle_employeeSurvey)

let edit_employeeSurvey id =
  choose
    [
      GET >=> warbler (fun _ -> editGET id bundle_employeeSurvey)
      POST >=> bindToForm employeeSurveyForm (fun employeeSurveyForm -> editPOST id employeeSurveyForm bundle_employeeSurvey)
    ]

let list_employeeSurvey =
  GET >=> warbler (fun _ -> getMany_employeeSurvey () |> view_list_employeeSurvey |> OK)

let search_employeeSurvey =
  choose
    [
      GET >=> request (fun req -> searchGET req bundle_employeeSurvey)
      POST >=> bindToForm searchForm (fun searchForm -> searchPOST searchForm bundle_employeeSurvey)
    ]

let create_customerSurvey =
  choose
    [
      GET >=> warbler (fun _ -> createGET bundle_customerSurvey)
      POST >=> bindToForm customerSurveyForm (fun form -> createPOST form bundle_customerSurvey)
    ]

let generate_customerSurvey count =
  choose
    [
      GET >=> warbler (fun _ -> generateGET count bundle_customerSurvey)
      POST >=> bindToForm customerSurveyForm (fun form -> generatePOST form bundle_customerSurvey)
    ]

let view_customerSurvey id =
  GET >=> warbler (fun _ -> viewGET id bundle_customerSurvey)

let edit_customerSurvey id =
  choose
    [
      GET >=> warbler (fun _ -> editGET id bundle_customerSurvey)
      POST >=> bindToForm customerSurveyForm (fun customerSurveyForm -> editPOST id customerSurveyForm bundle_customerSurvey)
    ]

let list_customerSurvey =
  GET >=> warbler (fun _ -> getMany_customerSurvey () |> view_list_customerSurvey |> OK)

let search_customerSurvey =
  choose
    [
      GET >=> request (fun req -> searchGET req bundle_customerSurvey)
      POST >=> bindToForm searchForm (fun searchForm -> searchPOST searchForm bundle_customerSurvey)
    ]