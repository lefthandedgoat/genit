module generated_html

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
              li [ aHref "/register" [text "Register"] ]
            ]
          ]
        ]
      ]
    ]
  ]

  