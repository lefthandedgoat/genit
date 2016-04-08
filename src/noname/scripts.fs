module scripts

open Suave.Html
open htmlHelpers

let jquery_1_11_3_min = """<script src="//code.jquery.com/jquery-1.11.3.min.js"></script>"""
let bootstrap = """<script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.6/js/bootstrap.min.js"></script>"""

let none = emptyText

let common =
  [
    jquery_1_11_3_min
    bootstrap
  ]
  |> List.map (fun script -> text script) |> flatten
