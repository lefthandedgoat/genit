module handlerHelpers

open System.Web
open Suave
open Suave.Successful
open Suave.Redirection
open generalHelpers
open forms

let createOrGenerate req (bundle : Bundle<_>) =
  if hasQueryString req "generate"
  then
    let generate = getQueryStringValue req "generate"
    let parsed, value = System.Int32.TryParse(generate)
    if parsed && value > 1
    then
      bundle.many_fake value
      let data = bundle.getMany ()
      OK <| bundle.get_list data
    else
      let data = bundle.single_fake ()
      OK <| bundle.get_edit data
  else OK bundle.get_create

let searchGET req (bundle : Bundle<_>) =
  if hasQueryString req "field" && hasQueryString req "how" && hasQueryString req "value"
  then
    let field = getQueryStringValue req "field"
    let how = getQueryStringValue req "how"
    let value = getQueryStringValue req "value"
    let data = bundle.getManyWhere field how value
    OK <| bundle.get_search (Some field) (Some how) value data
  else
    OK <| bundle.get_search None None "" []

let searchPOST searchForm (bundle : Bundle<_>) =
  let field = HttpUtility.UrlEncode(searchForm.Field)
  let how = HttpUtility.UrlEncode(searchForm.How)
  let value = HttpUtility.UrlEncode(searchForm.Value)
  FOUND <| sprintf "%s?field=%s&how=%s&value=%s" bundle.searchHref field how value
