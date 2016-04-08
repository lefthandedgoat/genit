module htmlHelpers

open System

open Suave
open Suave.Html

(*
   This is just a huge dump of html helpers that I have collected from other projects
   some may be of no value and can be deleted
*)

//emptyText is a work around for Suave.Experimental thinking
//that something is a leaf node and rendering wrong
let emptyText = text ""
let bodyClass class' inner = tag "body" ["class",class'] (flatten inner)
let divId id = divAttr ["id", id]
let divClass c = divAttr ["class", c]
let divIdClass id c = divAttr ["id", id; "class", c]
let h1Class class' xml = tag "h1" ["class",class'] xml
let h1 s = tag "h1" [] (text s)
let h2Class class' xml = tag "h2" ["class",class'] xml
let h2 s = tag "h2" [] (text s)
let h3Class class' xml = tag "h3" ["class",class'] xml
let h3 s = tag "h3" [] (text s)
let h3ClassInner class' s inner = tag "h3" ["class",class'] (flatten (text s :: inner))
let h3Inner s inner = tag "h3" [] (flatten (text s :: inner))
let h4 s = tag "h4" [] (text s)
let h5 s = tag "h5" [] (text s)
let h6 s = tag "h6" [] (text s)
let aHref href inner = tag "a" ["href", href] (flatten inner)
let aHrefAttr href attr inner = tag "a" (("href", href) :: attr) (flatten inner)
let aHrefClass href class' inner = tag "a" ([("href", href); ("class", class')]) (flatten inner)
let cssLink href = linkAttr [ "href", href; " rel", "stylesheet"; " type", "text/css";  ]
let ul inner = tag "ul" [] (flatten inner)
let ulAttr attr inner = tag "ul" attr (flatten inner)
let ulClass class' inner = ulAttr ["class", class'] inner
let li inner = tag "li" [] (flatten inner)
let liAttr attr inner = tag "li" attr (flatten inner)
let liClass class' inner = liAttr ["class", class'] inner
let imgSrc src = imgAttr [ "src", src ]
let em s = tag "em" [] (text s)
let strong s = tag "strong" [] (text s)
let meta attr = tag "meta" attr empty
let spanAttr attr inner = tag "span" attr (flatten inner)
let spanClass class' inner = spanAttr ["class", class'] inner
let span inner = spanAttr [] inner
let italic attr inner = tag "i" attr inner
let p inner = tag "p" [] (flatten inner)
let pClass class' inner = tag "p" ["class", class'] (flatten inner)
let selectClassName class' name inner = tag "select" ["class", class'; "name", name] (flatten inner)
let option (value : obj) innertext = tag "option" ["value", (string value)] (text innertext)
let option_data_section (value : obj) data_section innertext = tag "option" ["value", (string value); "data-section", data_section] (text innertext)
let selectedOption (value : obj) innertext = tag "option" ["value", (string value); "selected", ""] (text innertext)

let form inner = tag "form" ["method", "POST"] (flatten inner)
let formAttr attr inner = tag "form" (("method", "POST") :: attr) (flatten inner)
let fieldset inner = tag "fieldset" [] (flatten inner)
let legend txt = tag "legend" [] (text txt)
let headerId id inner = tag "header" ["id", id] (flatten inner)
let footer inner = tag "footer" [] (flatten inner)
let submitInput value = inputAttr ["type", "submit"; "value", value]

let tableClass class' inner = tag "table" ["class", class'] (flatten inner)
let tbodyClass class' inner = tag "tbody" ["class", class'] (flatten inner)
let th inner = tag "th" [] (flatten inner)
let thead inner = tag "thead" [] (flatten inner)
let tbody inner = tag "tbody" [] (flatten inner)
let tr inner = tag "tr" [] (flatten inner)
let trClass class' inner = tag "tr" ["class", class'] (flatten inner)
let td inner = tag "td" [] (flatten inner)
let tdAttr attr inner = tag "td" attr (flatten inner)
let labelClass class' inner = tag "label" ["class", class'] (flatten inner)
let buttonClass class' inner = tag "button" ["class", class'] (flatten inner)
let buttonAttr attr inner = tag "button" attr (flatten inner)
let buttonClassHref class' href inner = tag "button" ["class", class'; "href", href] (flatten inner)
let inputAttrInner attr inner = tag "input" attr (flatten inner)
let inputClassPlaceholderNameType class' placeholder name type' value inner =
  tag "input" ["class", class'; "placeholder", placeholder; "name", name; "value", (string value); "type", type'] (flatten inner)
let textareaClassPlaceholder class' placeholder text' = tag "textarea" ["class", class'; "placeholder", placeholder; "rows", "4"] (text text')
let textareaClassPlaceholderName class' name placeholder text' = tag "textarea" ["class", class'; "name", name; "placeholder", placeholder; "rows", "4"] (text text')
let sectionId id inner = tag "section" ["id", id] (flatten inner)
let navClass class' inner = tag "nav" ["class",class'] (flatten inner)

let base_head title' =
  head [
    meta ["charset","utf-8"]
    meta ["name","viewport"; "content","width=device-width, initial-scale=1.0"]
    meta ["name","description"; "content",""]
    meta ["name","author"; "content",""]
    cssLink "http://fonts.googleapis.com/css?family=Open+Sans:400,300,600,400italic,700,800"
    cssLink "http://fonts.googleapis.com/css?family=Raleway:300,200,100"
    link
    title title'
    cssLink "https://maxcdn.bootstrapcdn.com/bootstrap/3.3.6/css/bootstrap.min.css"
  ]

let base_html title content scripts =
  let html' =
    html [
      base_head title
      body content
      scripts
    ]
    |> xmlToString
  sprintf "<!DOCTYPE html>%s" html'
