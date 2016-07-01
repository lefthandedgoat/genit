module helper_handler

open System.Web
open Suave
open Suave.Authentication
open Suave.Cookie
open Suave.State.CookieStateStore
open Suave.Successful
open Suave.Redirection
open Suave.Operators
open helper_general
open helper_html
open forms

let withParam (key,value) path = sprintf "%s?%s=%s" path key value

let sessionStore setF = context (fun x ->
    match HttpContext.state x with
    | Some state -> setF state
    | None -> never)

let redirectWithReturnPath redirection =
  request (fun x ->
    let path = x.url.AbsolutePath
    Redirection.FOUND (redirection |> withParam ("returnPath", path)))

let reset loginPath =
  unsetPair SessionAuthCookie
  >=> unsetPair StateCookie
  >=> FOUND loginPath

let loggedOn loginPath f_success =
  authenticate
    Cookie.CookieLife.Session
    false
    (fun () -> Choice2Of2 (redirectWithReturnPath loginPath))
    (fun _ -> Choice2Of2 (reset loginPath))
    f_success

let setAuthCookieAndRedirect id redirectTo =
  authenticated Cookie.CookieLife.Session false
  >=> statefulForSession
  >=> sessionStore (fun store -> store.set "user_id" id)
  >=> request (fun _ -> FOUND redirectTo)

(*
   NOTE
   These are just helpers used by the generated code
   They may not be exactly what you like, or do what you need
   Feel free to copy/paste/modify them into what you need
   They have a focus on preventing runtime exceptions by checking
   all optional values for data, thus they are kind of ugly
*)

let viewGET id (bundle : Bundle<_,_>) =
  if bundle.tryById.IsNone || bundle.view_view.IsNone then
    OK error_404
  else
    let data = bundle.tryById.Value id
    match data with
    | None -> OK error_404
    | Some(data) -> OK <| bundle.view_view.Value data

let editGET id (bundle : Bundle<_,_>) =
  if bundle.tryById.IsNone || bundle.view_edit.IsNone then
    OK error_404
  else
    let data = bundle.tryById.Value id
    match data with
    | None -> OK error_404
    | Some(data) -> OK <| bundle.view_edit.Value data

let editPOST (id : int64) form (bundle : Bundle<_,_>) =
  if bundle.validateForm.IsNone || bundle.convertForm.IsNone || bundle.update.IsNone || bundle.view_edit_errored.IsNone then
    OK error_404
  else
    let validation = bundle.validateForm.Value form
    if validation = [] then
      let converted = bundle.convertForm.Value form
      bundle.update.Value converted
      if bundle.view_view.IsSome then
        FOUND <| sprintf bundle.href_view id
      else
        FOUND <| sprintf bundle.href_edit id
    else
      OK (bundle.view_edit_errored.Value validation form)

let createGET (bundle : Bundle<_,_>) =
  if bundle.view_create.IsNone then
    OK error_404
  else
    OK bundle.view_create.Value

let createPOST form (bundle : Bundle<_,_>) =
  if bundle.validateForm.IsNone || bundle.convertForm.IsNone || bundle.insert.IsNone || bundle.view_create_errored.IsNone then
    OK error_404
  else
    let validation = bundle.validateForm.Value form
    if validation = [] then
      let converted = bundle.convertForm.Value form
      let id = bundle.insert.Value converted
      if bundle.view_view.IsSome then
        FOUND <| sprintf bundle.href_view id
      else
        FOUND bundle.href_create
    else
      OK (bundle.view_create_errored.Value validation form)

let generateGET (count : int64) (bundle : Bundle<_,_>) =
  let count = int count
  if bundle.fake_many.IsNone || bundle.fake_single.IsNone || bundle.view_generate.IsNone then
    OK error_404
  else
    if count > 1
    then
      bundle.fake_many.Value count
      if bundle.getMany.IsNone || bundle.view_list.IsNone then
        OK bundle.href_generate
      else
        FOUND bundle.href_list
    else
      let data = bundle.fake_single.Value ()
      OK <| bundle.view_generate.Value data

let generatePOST form (bundle : Bundle<_,_>) =
  if bundle.validateForm.IsNone || bundle.convertForm.IsNone || bundle.insert.IsNone || bundle.view_generate_errored.IsNone then
    OK error_404
  else
    let validation = bundle.validateForm.Value form
    if validation = [] then
      let converted = bundle.convertForm.Value form
      let id = bundle.insert.Value converted
      if bundle.view_view.IsSome then
        FOUND <| sprintf bundle.href_view id
      else
        FOUND bundle.href_generate
    else
      OK (bundle.view_generate_errored.Value validation form)

let searchGET req (bundle : Bundle<_,_>) =
  if bundle.getManyWhere.IsNone || bundle.view_search.IsNone then
    OK error_404
  else
    if hasQueryString req "field" && hasQueryString req "how" && hasQueryString req "value"
    then
      let field = getQueryStringValue req "field"
      let how = getQueryStringValue req "how"
      let value = getQueryStringValue req "value"
      let data = bundle.getManyWhere.Value field how value
      OK <| bundle.view_search.Value (Some field) (Some how) value data
    else
      OK <| bundle.view_search.Value None None "" []

let searchPOST searchForm (bundle : Bundle<_,_>) =
  let field = HttpUtility.UrlEncode(searchForm.Field)
  let how = HttpUtility.UrlEncode(searchForm.How)
  let value = HttpUtility.UrlEncode(searchForm.Value)
  FOUND <| sprintf "%s?field=%s&how=%s&value=%s" bundle.href_search field how value
