module helper_bootstrap
open System

open Suave.Html
open helper_html

(*
   This is just a huge dump of bootstrap helpers that I have collected from other projects
   some may be of no value and can be deleted
*)

let removeSpace (value : string) = value.Replace(" ", "")
let option2String value : string =
  match value with
  | Some(value) -> (value :> obj).ToString()
  | None -> ""

let icon type' = italic ["class", (sprintf "fa fa-%s" type')] emptyText
let wrapper inner = divId "wrapper" inner
let pageWrapper inner = divIdClass "page-wrapper" "gray-bg" inner
let wrapperClass class' inner = divIdClass "cl-wrapper" class' inner
let wrapperContent inner = divClass "wrapper wrapper-content" inner
let navbarRow inner = divClass "row border-bottom white-bg" inner
let sidebar inner = divClass "cl-sidebar" inner
let toggle inner = divClass "cl-toggle" inner
let navblock inner = divClass "cl-navblock" inner
let menu_space inner = divClass "menu-space" inner
let content inner = divClass "content" inner
let container inner = divClass "container" inner
let signup_container inner = divClass "sign-up-container" inner
let form_wrapper inner = divClass "ibox" inner
let form_title inner = divClass "ibox-title" inner
let form_content inner = divClass "ibox-content" inner
let sidebar_logo inner = divClass "sidebar-logo" inner
let logo inner = divClass "logo" inner
let vnavigation inner = ulAttr ["class", "cl-vnavigation"] inner
let row inner = divClass "row" inner
let row_nomargin inner = divClass "row no-margin-top" inner
let m3sm6 inner = divClass "col-md-3 col-sm-6" inner
let m12 inner = divClass "col-md-12" inner
let sm2 inner = divClass "col-sm-2" inner
let sm3 inner = divClass "col-sm-3" inner
let sm6 inner = divClass "col-sm-6" inner
let sm8 inner = divClass "col-sm-8" inner
let sm10 inner = divClass "col-sm-10" inner
let sm12 inner = divClass "col-sm-12" inner
let block_flat inner = divClass "block-flat" inner
let header inner = divClass "header" inner
let labelX x inner = spanAttr ["class", (sprintf "label label-%s" x)] [inner]
let tdColor color inner = tdAttr ["class", (sprintf "color-%s" color)] inner
let table_responsive inner = divClass "table-responsive" inner
let progress_bar type' percent inner = divAttr ["class", (sprintf "progress-bar progress-bar-%s") type'; "style",(sprintf "width: %s" percent) ] [inner]
let tile color inner = divClass (sprintf "fd-tile detail clean tile-%s" color) inner
let sidebar_item inner = spanAttr ["class","sidebar-item"] inner
let form_horizontal inner = formAttr ["class","form-horizontal"] inner
let form_inline inner = formAttr ["class","form-inline"] inner
let form_group inner = divClass "form-group" inner
let input_group inner = divClass "input-group" inner
let input_form_control placeholder name value = inputClassPlaceholderNameType "form-control" placeholder (removeSpace name) "text" value [empty]
let datetime_input_form_control placeholder name value = inputClassPlaceholderNameType "form-control datetime" placeholder (removeSpace name) "text" value [empty]
let password_form_control placeholder name value = inputClassPlaceholderNameType "form-control" placeholder (removeSpace name) "password" value [empty]
let input_form_control_inner placeholder name value inner = inputClassPlaceholderNameType "form-control" placeholder (removeSpace name) "text" value inner
let textarea_form_control placeholder name value = textareaClassPlaceholderName "form-control" (removeSpace name) placeholder value
let select_form_control name inner = selectClassName "form-control" (removeSpace name) inner
let input_group_button inner = spanClass "input-group-btn" inner
let input_group_addon inner = spanClass "input-group-addon" inner
let control_label inner = labelClass "col-sm-2 control-label" inner
let inline_label inner = labelClass "sr-only" inner
let button_plain href inner = aHrefAttr href ["class", "btn"] inner
let button_small_plain href inner = aHrefAttr href ["class", "btn btn-sm"] inner
let button_primary href inner = aHrefAttr href ["class", "btn btn-primary"] inner
let button_success href inner = aHrefAttr href ["class", "btn btn-success"] inner
let button_small_success href inner = aHrefAttr href ["class", "btn btn-sm btn-success"] inner
let button_danger href inner = aHrefAttr href ["class", "btn btn-danger"] inner
let button_small_danger href inner = aHrefAttr href ["class", "btn btn-sm btn-danger"] inner
let button_save = inputAttr [ "value","Save"; "type","submit"; "class","btn btn-success"; ]
let button_submit = inputAttr [ "value","Submit"; "type","submit"; "class","btn btn-success"; ]
let button_run = inputAttr [ "id","run"; "value","Run"; "type","submit"; "class","btn btn-primary"; ]
let button_login = inputAttr [ "value","Login"; "type","submit"; "class","btn btn-success"; ]
let button_register = aHrefAttr "/register" [ "class","btn"; ] [ text "Register" ]
let pull_right inner = spanClass "pull-right" inner
let page_error inner = divClass "page-error" inner
let navbar inner = divIdClass "navbar" "navbar-collapse collapse" inner
let navbar_header inner = divClass "navbar-header" inner
let navbar_toggle inner = buttonClass "navbar-toggle collapsed" inner
let navbar_brand inner = aHrefClass "/" "navbar-brand" inner
let navbar_nav inner = ulClass "nav navbar-nav" inner
let navbar_nav_right inner = ulClass "nav navbar-nav navbar-right" inner
let navbar_button classes href innerText = divClass "navbar-btn" [ aHrefClass href (sprintf "btn %s" classes) [text innerText] ]
let dropdown inner = liClass "dropdown" inner
let dropdown_toggle inner = aHrefAttr "#" ["class","dropdown-toggle"; "data-toggle","dropdown"; "role","button"; "aria-haspopup","true"; "aria-expanded","false"] inner
let dropdown_menu inner = ulClass "dropdown-menu" inner
let caret = spanClass "caret" [emptyText]
let jumbotron inner = divClass "jumbotron" inner

let textEmtpyForNone text' = match text' with Some(t) -> t | None -> ""

//todo paramaterize and such
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
        navbar_nav [ ]
      ]
    ]
  ]

let base_html title navbar content scripts =
  let html' =
    html [
      base_head title
      bodyClass "top-navigation" [
        wrapper [
          pageWrapper [
            navbarRow [ navbar ]
            wrapperContent content
          ]
        ]
      ]
      scripts
    ]
    |> xmlToString
  sprintf "<!DOCTYPE html>%s" html'

let form_group_control_label_sm8 label' inner =
  form_group [
    control_label [ text label' ]
    sm8 inner
  ]

let form_group_inline_label label' inner =
  form_group [
    inline_label [ text label' ]
    flatten inner
  ]

let private errorsOrEmptyText label errors =
  let errors = errors |> List.filter (fun (prop, _) -> (removeSpace prop) = (removeSpace label))
  match errors with
  | [] -> emptyText
  | _ ->
    errors |> List.map (fun (_, errorMessage) -> li [ text errorMessage])
    |> ulClass "parsley-errors-list"

let private base_label_text_ahref_button label' text' button' errors =
  form_group_control_label_sm8 label' [
    input_group [
      input_form_control label' label' text'
      input_group_button [
        button_primary text' [ text button' ]
      ]
    ]
    errorsOrEmptyText label' errors
  ]

let private base_label_text label' text' errors =
  form_group_control_label_sm8 label' [
    input_form_control label' label' text'
    errorsOrEmptyText label' errors
  ]

let private base_inline_label_text label' text' errors =
  form_group_inline_label label' [
    input_form_control label' label' text'
    errorsOrEmptyText label' errors
  ]

let private base_label_datetime label' text' errors =
  form_group_control_label_sm8 label' [
    datetime_input_form_control label' label' text'
    errorsOrEmptyText label' errors
  ]

let private base_label_password label' text' errors =
  form_group_control_label_sm8 label' [
    password_form_control label' label' text'
    errorsOrEmptyText label' errors
  ]

let private base_label_textarea label' text' errors =
  form_group_control_label_sm8 label' [
    textarea_form_control label' label' text'
    errorsOrEmptyText label' errors
  ]

let base_label_select label' (options : (string * string) list) (selected : 'a option) errors =
  form_group_control_label_sm8 label' [
    select_form_control label'
      (options |> List.map (fun (id, value) ->
                                    if selected.IsSome && id = selected.Value.ToString()
                                    then selectedOption id value
                                    else option id value))
    errorsOrEmptyText label' errors
  ]

let base_inline_label_select label' (options : (string * string) list) (selected : 'a option) errors =
  form_group_inline_label label' [
    select_form_control label'
      (options |> List.map (fun (id, value) ->
                                    if selected.IsSome && id = selected.Value.ToString()
                                    then selectedOption id value
                                    else option id value))
    errorsOrEmptyText label' errors
  ]

let options_data_section (options : (string * string) list) data_section =
  (options |> List.map (fun (id, value) -> option_data_section id data_section value))

let label_static label' (value : 'a) =
  form_group_control_label_sm8 label' [
    static' label' value
  ]

let label_text_ahref_button label' text' button' = base_label_text_ahref_button label' text' button' []
let label_text label' text' = base_label_text label' text' []
let label_datetime label' text' = base_label_datetime label' text' []
let label_password label' text' = base_label_password label' text' []
let label_textarea label' text' = base_label_textarea label' text' []
let label_select label' options = base_label_select label' options None []
let label_select_selected label' options selected = base_label_select label' options selected []

let inline_label_text label' text' = base_inline_label_text label' text' []
let inline_label_select label' options = base_inline_label_select label' options None []
let inline_label_select_selected label' options selected = base_inline_label_select label' options selected []

let table_bordered_linked_tr ths (rows : 'a list) (toTd : 'a -> Xml list) (toTr : 'a -> (Xml list -> Xml)) =
  let table_bordered inner = tableClass "table table-bordered" inner
  table_responsive [
    table_bordered [
      thead [
        tr (ths |> List.map (fun th' -> th [text th']))
      ]
      tbody (rows |> List.map (fun row' -> (toTr row') (toTd row')))
    ]
  ]

let table_bordered ths (rows : 'a list) (toTd : 'a -> Xml list) =
  table_bordered_linked_tr ths rows toTd (fun _ -> tr)

let icon_label_text label' text' icon' =
  form_group [
    sm12 [
      input_group [
        input_group_addon [ icon icon' ]
        input_form_control label' label' text'
      ]
    ]
  ]

let icon_password_text label' text' icon' =
  form_group [
    sm12 [
      input_group [
        input_group_addon [ icon icon' ]
        password_form_control label' label' text'
      ]
    ]
  ]

let stand_alone_error text' =
  form_group [
    sm12 [ ulClass "parsley-errors-list" [ li [ text text'] ] ]
  ]

let errored_label_text_ahref_button label' text' button' errors = base_label_text_ahref_button label' text' button' errors
let errored_label_text label' text' errors = base_label_text label' text' errors
let errored_label_datetime label' text' errors = base_label_datetime label' text' errors
let errored_label_password label' text' errors = base_label_password label' text' errors
let errored_label_textarea label' text' errors = base_label_textarea label' text' errors
let errored_label_select label' options selected errors = base_label_select label' options selected errors

let errored_icon_label_text label' text' icon' errors =
  form_group [
    sm12 [
      input_group [
        input_group_addon [ icon icon' ]
        input_form_control label' label' text'
      ]
      errorsOrEmptyText label' errors
    ]
  ]

let errored_icon_password_text label' text' icon' errors =
  form_group [
    sm12 [
      input_group [
        input_group_addon [ icon icon' ]
        password_form_control label' label' text'
      ]
      errorsOrEmptyText label' errors
    ]
  ]

let common_form decription formElements =
  container [
    row [
      form_wrapper [
        form_title [ h3 decription ]
        form_content [
          div [
            form_horizontal [
              content (formElements @ [form_group [ sm12 [ pull_right [ button_submit ] ] ] ])
            ]
          ]
        ]
      ]
    ]
  ]

let common_static_form button decription formElements =
  container [
    row [
      form_wrapper [
        form_title [ h3Inner decription [ pull_right button ] ]
        form_content [
          div [
            form_horizontal [
              content formElements
            ]
          ]
        ]
      ]
    ]
  ]

let common_register_form decription formElements =
  signup_container [
    divClass "middle-sign-up" [
      form_wrapper [
        form_title [ h3 decription ]
        form_content [
          div [
            form_horizontal [
              content (formElements @ [form_group [ sm12 [ pull_right [ button_submit ] ] ] ])
            ]
          ]
        ]
      ]
    ]
  ]
