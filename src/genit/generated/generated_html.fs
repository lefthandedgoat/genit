module generated_html

open System
open Suave.Html
open helper_html
open helper_bootstrap

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
              li [ aHref "/" [text "Home"] ]
              li [ aHref "/register" [text "Register"] ]
              li [ aHref "/login" [text "Login"] ]
              li [ aHref "/self/create" [text "Create Self"] ]
              li [ aHref "/self/list" [text "List Selfs"] ]
              li [ aHref "/self/search" [text "Search Selfs"] ]
              li [ aHref "/customer/create" [text "Create Customer"] ]
              li [ aHref "/customer/list" [text "List Customers"] ]
              li [ aHref "/employeeSurvey/create" [text "Create Employee Survey"] ]
              li [ aHref "/employeeSurvey/list" [text "List Employee Surveys"] ]
              li [ aHref "/employeeSurvey/search" [text "Search Employee Surveys"] ]
              li [ aHref "/customerSurvey/create" [text "Create Customer Survey"] ]
              li [ aHref "/customerSurvey/list" [text "List Customer Surveys"] ]
              li [ aHref "/customerSurvey/search" [text "Search Customer Surveys"] ]
            ]
          ]
        ]
      ]
    ]
  ]

  