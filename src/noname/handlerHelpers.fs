module handlerHelpers

open Suave
open Suave.Successful
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
