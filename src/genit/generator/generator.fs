module generator

open System
open dsl
open modules
open helper_general

let write path value = IO.File.WriteAllText(path, value)
let executingDir = IO.Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly().CodeBase)
let destination filename =
  executingDir
  |> UriBuilder
  |> (fun uri -> uri.Path)
  |> IO.Path.GetDirectoryName
  |> (fun path -> path.Replace("bin", "generated"))
  |> (fun path -> System.IO.Path.Combine(path, filename))

let fieldToHtml (field : Field) =
  let template tag = sprintf """%s "%s" "" """ tag field.Name |> trimEnd
  let iconTemplate tag icon = sprintf """%s "%s" "" "%s" """ tag field.Name icon |> trimEnd
  match field.FieldType with
  | Id                -> sprintf """hiddenInput "%s" "-1" """ field.AsProperty |> trimEnd
  | Text              -> template "label_text"
  | Paragraph         -> template "label_textarea"
  | Number            -> template "label_text"
  | Decimal           -> template "label_text"
  | Date              -> template "label_datetime"
  | Phone             -> template "label_text"
  | Email             -> iconTemplate "icon_label_text" "envelope"
  | Name              -> iconTemplate "icon_label_text" "user"
  | Password          -> iconTemplate "icon_password_text" "lock"
  | ConfirmPassword   -> iconTemplate "icon_password_text" "lock"
  | Dropdown options  -> sprintf """label_select "%s" %A """ field.Name (zipOptions options) |> trimEnd
  | Referenced        -> sprintf """label_select "%s" %s """ field.Name (sprintf "(zipOptions getMany_%s_Names)" (lower field.Name) ) |> trimEnd

let fieldToPopulatedHtml page (field : Field) =
  sql.fieldToPopulatedHtml page field sql.Engine.MicrosoftSQL

let fieldToStaticHtml page (field : Field) =
  let template tag = sprintf """%s "%s" %s.%s """ tag field.Name page.AsVal field.AsProperty
  match field.FieldType with
  | Id                -> ""
  | Text              -> template "label_static"
  | Paragraph         -> template "label_static"
  | Number            -> template "label_static"
  | Decimal           -> template "label_static"
  | Date              -> template "label_static"
  | Phone             -> template "label_static"
  | Email             -> template "label_static"
  | Name              -> template "label_static"
  | Password          -> template "label_static"
  | ConfirmPassword   -> template "label_static"
  | Dropdown _        -> sprintf """label_static "%s" %s.%s """ field.Name page.AsVal field.AsProperty
  | Referenced        -> sprintf """label_static "%s" %s.%s """ field.Name page.AsVal field.AsProperty

let fieldToErroredHtml page (field : Field) =
  let template tag = sprintf """%s "%s" (string %s.%s) errors""" tag field.Name page.AsFormVal field.AsProperty
  let iconTemplate tag icon = sprintf """%s "%s" %s.%s "%s" errors""" tag field.Name page.AsFormVal field.AsProperty icon
  match field.FieldType with
  | Id                -> sprintf """hiddenInput "%s" %s.%s """ field.AsProperty page.AsFormVal field.AsProperty
  | Text              -> template "errored_label_text"
  | Paragraph         -> template "errored_label_textarea"
  | Number            -> template "errored_label_text"
  | Decimal           -> template "errored_label_text"
  | Date              -> template "errored_label_datetime"
  | Phone             -> template "errored_label_text"
  | Email             -> iconTemplate "errored_icon_label_text" "envelope"
  | Name              -> iconTemplate "errored_icon_label_text" "user"
  | Password          -> iconTemplate "errored_icon_password_text" "lock"
  | ConfirmPassword   -> iconTemplate "errored_icon_password_text" "lock"
  | Dropdown options  -> sprintf """errored_label_select "%s" %A (Some %s.%s) errors""" field.Name (zipOptions options) page.AsFormVal field.AsProperty
  | Referenced  -> sprintf """errored_label_select "%s" %s (Some %s.%s) errors""" field.Name (sprintf "(zipOptions getMany_%s_Names)" (lower field.Name) ) page.AsFormVal field.AsProperty

let fieldToConvertProperty page field =
  sql.fieldToConvertProperty page field (sql.Engine.MicrosoftSQL)

let fieldToValidation (field : Field) page =
  let template validation = sprintf """%s "%s" %s.%s""" validation field.Name page.AsFormVal field.AsProperty
  let confirmPasswordTemplate () =
    let password = page.Fields |> List.tryFind (fun field -> field.FieldType = Password)
    match password with
    | Some(password) ->
       let validate1 = template "validate_password"
       let validate2 = sprintf """validate_equal "%s" "%s" %s.%s %s.%s""" password.Name field.Name page.AsFormVal password.AsProperty page.AsFormVal field.AsProperty |> pad 2
       [validate1; validate2] |> flatten |> Some
    | None -> Some (template "validate_password")

  match field.FieldType with
  | Id              -> None
  | Text            -> None
  | Paragraph       -> None
  | Number          -> Some (template "validate_integer")
  | Decimal         -> Some (template "validate_double")
  | Date            -> Some (template "validate_datetime")
  | Phone           -> None //parsePhone?
  | Email           -> Some (template "validate_email")
  | Name            -> None
  | Password        -> Some (template "validate_password")
  | ConfirmPassword -> confirmPasswordTemplate ()
  | Dropdown _      -> None
  | Referenced      -> None

let fieldToTestName (field : Field) =
  let template text = sprintf """"%s %s" """ field.Name text |> trimEnd
  match field.FieldType with
  | Id              -> None
  | Text            -> None
  | Paragraph       -> None
  | Number          -> Some (template "must be a valid integer")
  | Decimal         -> Some (template "must be a valid double")
  | Date            -> Some (template "must be a valid date")
  | Email           -> Some (template "must be a valid email")
  | Name            -> None
  | Phone           -> None //parsePhone?
  | Password        -> Some (template "must be between 6 and 100 characters")
  | ConfirmPassword -> Some (template "must be between 6 and 100 characters")
  | Dropdown _      -> None
  | Referenced      -> None

let fieldToTestBody (field : Field) =
  let template text = sprintf """displayed "%s %s" """ field.Name text |> trimEnd
  match field.FieldType with
  | Id              -> None
  | Text            -> None
  | Paragraph       -> None
  | Number          -> Some (template "is not a valid number (int)")
  | Decimal         -> Some (template "is not a valid number (decimal)")
  | Date            -> Some (template "is not a valid date")
  | Email           -> Some (template "is not a valid email")
  | Name            -> None
  | Phone           -> None //parsePhone?
  | Password        -> Some (template "must be between 6 and 100 characters")
  | ConfirmPassword -> Some (template "must be between 6 and 100 characters")
  | Dropdown _      -> None
  | Referenced      -> None

let attributeToValidation field page =
  let property = sprintf "%s.%s" page.AsFormVal field.AsProperty
  match field.Attribute with
  | PK         -> None
  | Null       -> None
  | Required   -> Some (sprintf """validate_required "%s" %s""" field.Name property)
  | Min(min)   -> Some (sprintf """validate_min "%s" %s %i""" field.Name property min)
  | Max(max)   -> Some (sprintf """validate_max "%s" %s %i""" field.Name property max)
  | Range(min,max) -> Some (sprintf """validate_range "%s" %s %i %i""" field.Name property min max)
  | Reference(page,required) -> Some (sprintf """validate_reference "%s" "%s" %s %b""" page field.Name property required)

let attributeToTestName (field : Field) =
  match field.Attribute with
  | PK         -> None
  | Null       -> None
  | Required   -> Some (sprintf """"%s is required" """ field.Name |> trimEnd)
  | Min(min)   -> Some (sprintf """"%s must be greater than %i" """ field.Name min |> trimEnd)
  | Max(max)   -> Some (sprintf """"%s must be less than %i" """ field.Name max |> trimEnd)
  | Range(min,max) -> Some (sprintf """"%s must be between %i and %i" """ field.Name min max |> trimEnd)
  | Reference(page,required) -> None

let attributeToTestBody (field : Field) =
  match field.Attribute with
  | PK         -> None
  | Null       -> None
  | Required   -> Some (sprintf """displayed "%s is required" """ field.Name |> trimEnd)
  | Min(min)   -> Some (sprintf """displayed "%s can not be below %i" """ field.Name min |> trimEnd)
  | Max(max)   -> Some (sprintf """displayed "%s can not be above %i" """ field.Name max |> trimEnd)
  | Range(min,max) -> Some (sprintf """displayed "%s must be between %i and %i" """ field.Name min max |> trimEnd)
  | Reference(page,required) -> None

let formatPopulatedEditFields page (fields : Field list) tabs =
  fields
  |> List.map (fieldToPopulatedHtml page)
  |> List.map (pad tabs)
  |> List.reduce (fun field1 field2 -> sprintf "%s%s%s" field1 Environment.NewLine field2)

let formatStaticFields page (fields : Field list) tabs =
  fields
  |> List.map (fieldToStaticHtml page)
  |> List.map (pad tabs)
  |> List.reduce (fun field1 field2 -> sprintf "%s%s%s" field1 Environment.NewLine field2)

let formatEditFields (fields : Field list) tabs =
  fields
  |> List.map fieldToHtml
  |> List.map (pad tabs)
  |> List.reduce (fun field1 field2 -> sprintf "%s%s%s" field1 Environment.NewLine field2)

let formatErroredFields page (fields : Field list) tabs =
  fields
  |> List.map (fieldToErroredHtml page)
  |> List.map (pad tabs)
  |> List.reduce (fun field1 field2 -> sprintf "%s%s%s" field1 Environment.NewLine field2)

let createFormViewTemplate (page : Page) =
  sprintf """
let view_create_%s =
  base_html
    "Create %s"
    [
      base_header brand
      common_form
        "Create %s"
        [
%s
        ]
    ]
    scripts.common""" page.AsVal page.Name page.Name (formatEditFields page.Fields 5)

let createErroredFormViewTemplate (page : Page) =
  sprintf """
let view_create_errored_%s errors (%s : %s) =
  base_html
    "Create %s"
    [
      base_header brand
      common_form
        "Create %s"
        [
%s
        ]
    ]
    scripts.common""" page.AsVal page.AsFormVal page.AsFormType page.Name page.Name (formatErroredFields page page.Fields 5)

let editFormViewTemplate (page : Page) =
  sprintf """
let view_edit_%s (%s : %s) =
  base_html
    "Edit %s"
    [
      base_header brand
      common_form
        "Edit %s"
        [
%s
        ]
    ]
    scripts.common""" page.AsVal page.AsVal page.AsType page.Name page.Name (formatPopulatedEditFields page page.Fields 5)

let editErroredFormViewTemplate (page : Page) =
  sprintf """
let view_edit_errored_%s errors (%s : %s) =
  base_html
    "Edit %s"
    [
      base_header brand
      common_form
        "Edit %s"
        [
%s
        ]
    ]
    scripts.common""" page.AsVal page.AsFormVal page.AsFormType page.Name page.Name (formatErroredFields page page.Fields 5)

let editButtonTemplate (page : Page) idField =
  if isEdit page
  then sprintf """[ button_small_success (sprintf "%s" %s.%s) [ text "Edit"] ]""" page.AsEditHref page.AsVal idField.AsProperty
  else "[]"

let viewFormViewTemplate (page : Page) =
  let idField = page.Fields |> List.find (fun field -> field.FieldType = Id)
  sprintf """
let view_view_%s (%s : %s) =
  let button = %s
  base_html
    "%s"
    [
      base_header brand
      common_static_form button
        "%s"
        [
%s
        ]
    ]
    scripts.common""" page.AsVal page.AsVal page.AsType (editButtonTemplate page idField) page.Name page.Name (formatStaticFields page page.Fields 5)

let fieldsToHeaders (page : Page) =
    page.Fields
    |> List.map (fun field -> sprintf """"%s" """ field.Name |> trimEnd)
    |> List.map (pad 10)
    |> flatten

let fieldsToTd (page : Page) =
    page.Fields
    |> List.map (fun field -> sprintf """td [ text (string %s.%s) ]""" page.AsVal field.AsProperty)
    |> List.map (pad 4)
    |> flatten

let toTrLinkTemplate (page : Page) idField =
  if isEdit page
  then sprintf """trLink (sprintf "%s" %s.%s) inner""" page.AsViewHref page.AsVal idField.AsProperty
  else """trLink "" inner"""

let createButtonTemplate (page : Page) =
  if isCreate page
  then sprintf """pull_right [ button_small_success "%s" [ text "Create"] ]""" page.AsCreateHref
  else ""

let listFormViewTemplate (page : Page) =
  let idField = page.Fields |> List.find (fun field -> field.FieldType = Id)
  sprintf """
let view_list_%s %ss =
  let toTr (%s : %s) inner =
    %s

  let toTd (%s : %s) =
    [
%s
    ]

  base_html
    "List %s"
    [
      base_header brand
      container [
        row [
          mcontent [
            block_flat [
              header [ h3Inner "List %ss" [ %s ] ]
              content [
                table_bordered_linked_tr
                  [
%s
                  ]
                  %ss toTd toTr
              ]
            ]
          ]
        ]
      ]
    ]
    scripts.datatable_bundle""" page.AsVal page.AsVal page.AsVal page.AsType (toTrLinkTemplate page idField) page.AsVal page.AsType (fieldsToTd page) page.Name page.Name (createButtonTemplate page) (fieldsToHeaders page) page.AsVal

let searchFormViewTemplate (page : Page) =
  let idField = page.Fields |> List.find (fun field -> field.FieldType = Id)
  sprintf """
let view_search_%s field how value %ss =
  let fields = ["Name", "Name"; "Food","Food"; "City", "City"]
  let hows = ["Equals", "Equals"; "Begins With","Begins With"]
  let toTr (%s : %s) inner =
    trLink (sprintf "%s" %s.%s) inner

  let toTd (%s : %s) =
    [
%s
    ]

  base_html
    "Search %s"
    [
      base_header brand
      container [
        row [
          mcontent [
            block_flat [
              header [
                h3Inner "Search %ss" [ ]
              ]
              div [
                form_inline [
                  content [
                    inline_label_select_selected "Field" fields field
                    inline_label_select_selected"How" hows how
                    inline_label_text "Value" value
                    pull_right [ button_submit ]
                  ]
                ]
              ]
              content [
                table_bordered_linked_tr
                  [
%s
                  ]
                  %ss toTd toTr
              ]
            ]
          ]
        ]
      ]
    ]
    scripts.datatable_bundle""" page.AsVal page.AsVal page.AsVal page.AsType page.AsViewHref page.AsVal idField.AsProperty page.AsVal page.AsType (fieldsToTd page) page.Name page.Name (fieldsToHeaders page) page.AsVal

let registerFormViewTemplate (page : Page) =
  sprintf """
let view_register =
  base_html
    "%s"
    [
      base_header brand
      common_register_form
        "%s"
        [
%s
        ]
    ]
    scripts.common""" page.Name page.Name (formatEditFields page.Fields 5)

let registerErroredFormViewTemplate (page : Page) =
  sprintf """
let view_errored_register errors (%s : %s) =
  base_html
    "%s"
    [
      base_header brand
      common_register_form
        "%s"
        [
%s
        ]
    ]
    scripts.common""" page.AsFormVal page.AsFormType page.Name page.Name (formatErroredFields page page.Fields 5)

let loginFormViewTemplate =
  sprintf """
let view_login error email =
  let errorTag =
    if error
    then stand_alone_error "Invalid email or password"
    else emptyText

  base_html
    "Login"
    [
      base_header brand
      common_register_form
        "Login"
        [
          errorTag
          hiddenInput "UserID" "-1"
          icon_label_text "Email" email "envelope"
          icon_password_text "Password" "" "lock"
        ]
    ]
    scripts.common"""

let loginErroredFormViewTemplate (page : Page) =
  sprintf """
let view_errored_login errors (%s : %s) =
  base_html
    "%s"
    [
      base_header brand
      common_register_form
        "%s"
        [
%s
        ]
    ]
    scripts.common""" page.AsFormVal page.AsFormType page.Name page.Name (formatErroredFields page page.Fields 5)

let jumbotronViewTemplate (site : Site) (page : Page) =
  sprintf """
let view_jumbo_%s =
  base_html
    "%s"
    [
      base_header brand
      divClass "container" [
        divClass "jumbotron" [
          h1 (sprintf "Welcome to %s!")
        ]
      ]
    ]
    scripts.common""" page.AsVal page.Name site.Name

let viewTemplate site page =
  let rec viewTemplate site page pageMode =
    match pageMode with
    | CVELS     -> [Create; View; Edit; List; Search] |> List.map (viewTemplate site page) |> flatten
    | CVEL      -> [Create; View; Edit; List] |> List.map (viewTemplate site page) |> flatten
    | Create    -> [createFormViewTemplate page; createErroredFormViewTemplate page] |> flatten
    | Edit      -> [editFormViewTemplate page; editErroredFormViewTemplate page] |> flatten
    | View      -> viewFormViewTemplate page
    | List      -> listFormViewTemplate page
    | Search    -> searchFormViewTemplate page
    | Register  -> [registerFormViewTemplate page; registerErroredFormViewTemplate page] |> flatten
    | Login     -> [loginFormViewTemplate; loginErroredFormViewTemplate page] |> flatten
    | Jumbotron -> jumbotronViewTemplate site page

  viewTemplate site page page.PageMode

let pageLinkTemplate (page : Page) =
  let template href text = sprintf """li [ aHref "%s" [text "%s"] ]""" href text |> pad 7
  let rec pageLinkTemplate page pageMode =
    match pageMode with
    | CVELS     -> [Create; View; Edit; List; Search] |> List.map (pageLinkTemplate page) |> flatten
    | CVEL      -> [Create; View; Edit; List] |> List.map (pageLinkTemplate page) |> flatten
    | Create    -> template page.AsCreateHref (sprintf "Create %s" page.Name)
    | Edit      -> ""
    | View      -> ""
    | List      -> template page.AsListHref (sprintf "List %ss" page.Name)
    | Search    -> template page.AsSearchHref (sprintf "Search %ss" page.Name)
    | Register  -> template page.AsHref page.Name
    | Login     -> template page.AsHref page.Name
    | Jumbotron -> template "/" page.Name

  pageLinkTemplate page page.PageMode

let pagePathTemplate (page : Page) =
  let template extra href withId =
    match withId with
    | false -> sprintf """let path_%s%s = "%s" """ extra page.AsVal href |> trimEnd
    | true -> sprintf """let path_%s%s : Int64Path = "%s" """ extra page.AsVal href |> trimEnd
  let rec pagePathTemplate page pageMode =
    match pageMode with
    | CVELS     -> [Create; View; Edit; List; Search] |> List.map (pagePathTemplate page) |> flatten
    | CVEL      -> [Create; View; Edit; List] |> List.map (pagePathTemplate page) |> flatten
    | Create    -> template "create_" page.AsCreateHref false
    | Edit      -> template "edit_" page.AsEditHref true
    | View      -> template "view_" page.AsViewHref true
    | List      -> template "list_" page.AsListHref false
    | Search    -> template "search_" page.AsSearchHref false
    | Register  -> template "" page.AsHref false
    | Login     -> template "" page.AsHref false
    | Jumbotron -> template "" "/" false

  pagePathTemplate page page.PageMode

let apiPathTemplate (api : API) =
  sprintf """let path_api_%s : Int64Path = "%s" """ api.AsVal api.AsViewHref |> trimEnd

let pageRouteTemplate (page : Page) =
  let template extra withId =
    match withId with
    | false ->
        match page.Attribute with
        | Standard      -> sprintf """path path_%s%s >=> %s%s""" extra page.AsVal extra page.AsVal |> pad 2
        | RequiresLogin -> sprintf """path path_%s%s >=> loggedOn path_login %s%s""" extra page.AsVal extra page.AsVal |> pad 2
    | true ->
        match page.Attribute with
        | Standard     -> sprintf """pathScan path_%s%s %s%s""" extra page.AsVal extra page.AsVal |> pad 2
        | RequiresLogin -> sprintf """pathScan path_%s%s (fun id -> loggedOn path_login (%s%s id))""" extra page.AsVal extra page.AsVal |> pad 2

  let rec pageRouteTemplate page pageMode =
    match pageMode with
    | CVELS     -> [Create; View; Edit; List; Search] |> List.map (pageRouteTemplate page) |> flatten
    | CVEL      -> [Create; View; Edit; List] |> List.map (pageRouteTemplate page) |> flatten
    | Create    -> template "create_" false
    | Edit      -> template "edit_" true
    | View      -> template "view_" true
    | List      -> template "list_" false
    | Search    -> template "search_" false
    | Register  -> template "" false
    | Login     -> template "" false
    | Jumbotron -> template "" false

  pageRouteTemplate page page.PageMode

let apiRouteTemplate (api : API) =
  sprintf """pathScan path_api_%s api_%s""" api.AsVal api.AsVal |> pad 2

let pageHandlerTemplate page =
  let rec pageHandlerTemplate page pageMode =
    match pageMode with
    | CVELS -> [Create; View; Edit; List; Search] |> List.map (pageHandlerTemplate page) |> flatten
    | CVEL  -> [Create; View; Edit; List] |> List.map (pageHandlerTemplate page) |> flatten
    | Edit  ->
      sprintf """
let edit_%s id =
  choose
    [
      GET >=> warbler (fun _ -> editGET id bundle_%s)
      POST >=> bindToForm %s (fun %s -> editPOST id %s bundle_%s)
    ]""" page.AsVal page.AsVal page.AsFormVal page.AsFormVal page.AsFormVal page.AsVal
    | View  ->
      sprintf """
let view_%s id =
  GET >=> warbler (fun _ -> viewGET id bundle_%s)""" page.AsVal page.AsVal
    | List  ->
      sprintf """
let list_%s =
  GET >=> warbler (fun _ -> getMany_%s () |> view_list_%s |> OK)""" page.AsVal page.AsVal page.AsVal
    | Search ->
      sprintf """
let search_%s =
  choose
    [
      GET >=> request (fun req -> searchGET req bundle_%s)
      POST >=> bindToForm searchForm (fun searchForm -> searchPOST searchForm bundle_%s)
    ]""" page.AsVal page.AsVal page.AsVal
    | Jumbotron ->
      sprintf """
let %s = GET >=> OK view_jumbo_%s""" page.AsVal page.AsVal
    | Create ->
      sprintf """
let create_%s =
  choose
    [
      GET >=> request (fun req -> createOrGenerateGET req bundle_%s)
      POST >=> bindToForm %s (fun form -> createPOST form bundle_%s)
    ]""" page.AsVal page.AsVal page.AsFormVal page.AsVal
    | Register    ->
      sprintf """
let register =
  choose
    [
      GET >=> OK view_register
      POST >=> bindToForm %s (fun %s ->
        let validation = validation_%s %s
        if validation = [] then
          let converted = convert_%s %s
          let id = insert_%s converted
          setAuthCookieAndRedirect id "/"
        else
          OK (view_errored_%s validation %s))
    ]""" page.AsFormVal page.AsFormVal page.AsFormVal page.AsFormVal page.AsFormVal page.AsFormVal page.AsVal page.AsVal page.AsFormVal
    | Login    ->
      sprintf """
let login =
  choose
    [
      GET >=> (OK <| view_login false "")
      POST >=> request (fun req ->
        bindToForm %s (fun %s ->
        let validation = validation_%s %s
        if validation = [] then
          let converted = convert_%s %s
          let loginAttempt = authenticate converted
          match loginAttempt with
            | Some(loginAttempt) ->
              let returnPath = getQueryStringValue req "returnPath"
              let returnPath = if returnPath = "" then "/" else returnPath
              setAuthCookieAndRedirect id returnPath
            | None -> OK <| view_login true loginForm.Email
        else
          OK (view_errored_%s validation %s)))
    ]"""  page.AsFormVal page.AsFormVal page.AsFormVal page.AsFormVal page.AsFormVal page.AsFormVal page.AsVal page.AsFormVal

  pageHandlerTemplate page page.PageMode

let apiHandlerTemplate (api : API) =
  sprintf """
let api_%s id =
  GET >=> request (fun req ->
    let data = tryById_%s id
    match data with
    | None -> OK error_404
    | Some(data) ->
      match (getQueryStringValue req "format").ToLower() with
      | "xml" ->
         let serializer = FsPickler.CreateXmlSerializer(indent = true)
         Writers.setMimeType "application/xml"
         >=> OK (serializer.PickleToString(data))
      | "json" | _ ->
         let serializer = FsPickler.CreateJsonSerializer(indent = true)
         Writers.setMimeType "application/json"
         >=> OK (serializer.PickleToString(data)))""" api.AsVal api.AsVal

let fieldLine (field : Field ) =
  sql.fieldLine field (sql.Engine.MicrosoftSQL)

let propertyTemplate (page : Page) =
  page.Fields
  |> List.filter (fun field -> field.FieldType <> ConfirmPassword)
  |> List.map (fun field -> fieldLine field)
  |> List.map (pad 2)
  |> flatten

let formPropertyTemplate (page : Page) =
  page.Fields
  |> List.map (fun field -> sprintf """%s : string""" field.AsProperty)
  |> List.map (pad 2)
  |> flatten

let converterPropertyTemplate (page : Page) =
  page.Fields
  |> List.filter (fun field -> field.FieldType <> ConfirmPassword)
  |> List.map (fieldToConvertProperty page)
  |> List.map (pad 2)
  |> flatten

let typeTemplate (page : Page) =
  if needsType page |> not then ""
  else
    sprintf """type %s =
  {
%s
  }
  """ page.AsType (propertyTemplate page)

let bundleSecondTypeTemplate page = if needsFormType page then sprintf "%s" page.AsFormType else sprintf "DummyForm"
let bundleValidateFormTemplate page = if needsValidation page then sprintf "Some validation_%s" page.AsFormVal else "None"
let bundleConvertFormTemplate page = if needsConvert page then sprintf "Some convert_%s" page.AsFormVal else "None"
let bundleFakeSingleTemplate page = if needsFakeData page then sprintf "Some fake_%s" page.AsVal else "None"
let bundleFakeManyTemplate page = if needsFakeData page then sprintf "Some fake_many_%s" page.AsVal else "None"
let bundleTryByIdTemplate page = if needsTryById page then sprintf "Some tryById_%s" page.AsVal else "None"
let bundleGetManyTemplate page = if needsGetMany page then sprintf "Some getMany_%s" page.AsVal else "None"
let bundleGetManyWhereTemplate page = if needsGetManyWhere page then sprintf "Some getManyWhere_%s" page.AsVal else "None"
let bundleInsertTemplate page = if needsInsert page then sprintf "Some insert_%s" page.AsVal else "None"
let bundleUpdateTemplate page = if needsUpdate page then sprintf "Some update_%s" page.AsVal else "None"
let bundleViewListTemplate page = if needsViewList page then sprintf "Some view_list_%s" page.AsVal else "None"
let bundleViewEditTemplate page = if needsViewEdit page then sprintf "Some view_edit_%s" page.AsVal else "None"
let bundleViewCreateTemplate page = if needsViewCreate page then sprintf "Some view_create_%s" page.AsVal else "None"
let bundleViewViewTemplate page = if needsViewView page then sprintf "Some view_view_%s" page.AsVal else "None"
let bundleViewSearchTemplate page = if needsViewSearch page then sprintf "Some view_search_%s" page.AsVal else "None"
let bundleViewEditErroredTemplate page = if needsViewEdit page then sprintf "Some view_edit_errored_%s" page.AsVal else "None"
let bundleViewCreateErroredTemplate page = if needsViewCreate page then sprintf "Some view_create_errored_%s" page.AsVal else "None"

let bundleTemplate (page : Page) =
  if needsBundle page |> not then ""
  else
    sprintf """let bundle_%s : Bundle<%s, %s> =
    {
      validateForm = %s
      convertForm = %s
      fake_single = %s
      fake_many = %s
      tryById = %s
      getMany = %s
      getManyWhere = %s
      insert = %s
      update = %s
      view_list = %s
      view_edit = %s
      view_create = %s
      view_view = %s
      view_search = %s
      view_edit_errored = %s
      view_create_errored = %s
      href_create = "%s"
      href_search = "%s"
      href_view = "%s"
      href_edit = "%s"
    }
    """
      page.AsVal
      page.AsType
      (bundleSecondTypeTemplate page)
      (bundleValidateFormTemplate page)
      (bundleConvertFormTemplate page)
      (bundleFakeSingleTemplate page)
      (bundleFakeManyTemplate page)
      (bundleTryByIdTemplate page)
      (bundleGetManyTemplate page)
      (bundleGetManyWhereTemplate page)
      (bundleInsertTemplate page)
      (bundleUpdateTemplate page)
      (bundleViewListTemplate page)
      (bundleViewEditTemplate page)
      (bundleViewCreateTemplate page)
      (bundleViewViewTemplate page)
      (bundleViewSearchTemplate page)
      (bundleViewEditErroredTemplate page)
      (bundleViewCreateErroredTemplate page)
      page.AsCreateHref
      page.AsSearchHref
      page.AsViewHref
      page.AsEditHref

let converterTemplate (page : Page) =
  if needsConvert page |> not then ""
  else
    sprintf """let convert_%s (%s : %s) : %s =
  {
%s
  }
  """ page.AsFormVal page.AsFormVal page.AsFormType page.AsType (converterPropertyTemplate page)

let formTypeTemplate (page : Page) =
  if needsFormType page |> not then ""
  else
    sprintf """type %s =
  {
%s
  }

let %s : Form<%s> = Form ([],[])

%s""" page.AsFormType (formPropertyTemplate page) page.AsFormVal page.AsFormType (converterTemplate page)

let validationTemplate (page : Page) =
  if needsValidation page |> not then ""
  else
    let validations =
      page.Fields
      |> List.collect (fun field -> [fieldToValidation field page] @ [attributeToValidation field page])
      |> List.choose id
      |> List.map (pad 2)
      |> flatten

    sprintf """let validation_%s (%s : %s) =
  [
%s
  ] |> List.choose id
  """ page.AsFormVal page.AsFormVal page.AsFormType validations

let contextTemplate (page : Page) = sprintf """context "%s" """ page.Name |> trimEnd |> pad 1

let onceTemplate (page : Page) =
  let href =
   match page.PageMode with
   | CVELS     -> page.AsCreateHref
   | CVEL      -> page.AsCreateHref
   | Create    -> page.AsCreateHref
   | Edit      -> page.AsEditHref
   | View      -> page.AsViewHref
   | List      -> page.AsSearchHref
   | Search    -> page.AsSearchHref
   | Register
   | Login     -> page.AsHref
   | Jumbotron -> "/"

  sprintf """once (fun _ -> url "http://localhost:8083%s"; click ".btn") """ href
  |> pad 1

let attributeUITestTemplate name field =
  match name with
  | None -> None
  | Some(name) ->
    let top = (sprintf """%s &&& fun _ ->""" name) |> pad 1
    let body = (attributeToTestBody field).Value |> pad 2
    Some (sprintf """%s
%s
    """ top body)

let fieldUITestTemplate name field =
  match name with
  | None -> None
  | Some(name) ->
    let top = (sprintf """%s &&& fun _ ->""" name) |> pad 1
    let body = (fieldToTestBody field).Value |> pad 2
    Some (sprintf """%s
%s
    """ top body)

let toTest (field : Field) =
  let name = attributeToTestName field
  let name2 = fieldToTestName field
  [attributeUITestTemplate name field; fieldUITestTemplate name2 field]

let uitestTemplate (page : Page) =
  if needsUITests page |> not then ""
  else
    let tests = page.Fields |> List.collect toTest |> List.choose id |> flatten
    sprintf """%s

%s

%s
    """ (contextTemplate page) (onceTemplate page) tests

//let cityStateZip = randomItem citiesSatesZips
let fakePropertyTemplate (field : Field) =
  sql.fakePropertyTemplate field (sql.Engine.MicrosoftSQL)

let fakePropertiesTemplate (page : Page) =
  page.Fields
  |> List.map fakePropertyTemplate
  |> List.map (pad 2)
  |> flatten

let fakeComplexValues (page : Page) =
  let cityStateZipTriggers = [ "city"; "state"; "zip"; "postal" ]
  let exists =
    page.Fields
    |> List.exists (fun field ->
                      let lowered = field.Name.ToLower()
                      cityStateZipTriggers
                      |> List.exists (fun trigger -> lowered.Contains(trigger)))
  if exists then
    "
  let cityStateZip = randomItem citiesSatesZips"
  else ""

let fakeManyDataTemplate (page: Page) =
  sprintf """let fake_many_%s number =
  [| 1..number |]
  |> Array.map (fun _ -> fake_%s ()) //no parallel cause of RNG
  |> Array.Parallel.map insert_%s
  |> ignore
 """ page.AsVal page.AsVal page.AsVal

let fakeDataTemplate (page : Page) =
  if needsFakeData page |> not then ""
  else
    sprintf """let fake_%s () =%s
  {
%s
  }

%s
 """ page.AsVal (fakeComplexValues page) (fakePropertiesTemplate page) (fakeManyDataTemplate page)

let generate (site : Site) =
  let html_results = site.Pages |> List.map pageLinkTemplate |> flatten
  let generated_html_result = generated_html_template html_results

  let views_results = site.Pages |> List.map (viewTemplate site) |> flatten
  let generated_views_result = generated_views_template site.Name views_results

  let page_handlers = site.Pages |> List.map pageHandlerTemplate |> flatten
  let api_handlers = site.APIs |> List.map apiHandlerTemplate |> flatten
  let handler_results = [page_handlers; api_handlers] |> flatten
  let generated_handlers_result = generated_handlers_template handler_results

  let forms_results = site.Pages |> List.map formTypeTemplate |> flatten
  let generated_forms_result = generated_forms_template forms_results

  let types_results = site.Pages |> List.map typeTemplate |> flatten
  let generated_types_result = generated_types_template types_results

  let bundles_results = site.Pages |> List.map bundleTemplate |> flatten
  let generated_bundles_result = generated_bundles_template bundles_results

  let page_paths = site.Pages |> List.map pagePathTemplate |> flatten
  let api_paths = site.APIs |> List.map apiPathTemplate |> flatten
  let paths_results = [page_paths; api_paths] |> flatten
  let page_routes = site.Pages |> List.map pageRouteTemplate |> flatten
  let api_routes = site.APIs |> List.map apiRouteTemplate |> flatten
  let routes_results = [page_routes; api_routes] |> flatten
  let generated_paths_result = generated_paths_template paths_results routes_results

  let validations_results = site.Pages |> List.map validationTemplate |> flatten
  let generated_validation_result = generated_validation_template validations_results

  let generated_unittests_result = generated_unittests_template "//nothing"

  let uitests_results = site.Pages |> List.map uitestTemplate |> flatten
  let generated_uitests_result = generated_uitests_template uitests_results

  let fake_data_results = site.Pages |> List.map fakeDataTemplate |> flatten
  let generated_fake_data_result = generated_fake_data_template fake_data_results

  let connectionString = sprintf @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=%s;Integrated Security=True" site.AsDatabase 
  let generated_data_access_result = generated_data_access_template sql.Engine.MicrosoftSQL connectionString (sql.createQueries site sql.Engine.MicrosoftSQL)

  let serverKey = sprintf """let serverKey = %A""" (Suave.Utils.Crypto.generateKey Suave.Http.HttpRuntime.ServerKeyLength)
  let generated_security_result = generated_security_template serverKey

  let generated_sql_createdb_result = sql.createTemplate site.AsDatabase sql.Engine.MicrosoftSQL
  let generated_sql_initialSetup_result = sql.initialSetupTemplate site.AsDatabase sql.Engine.MicrosoftSQL
  let generated_sql_createTables_result = sql.createTables (sql.createTableTemplates site sql.Engine.MicrosoftSQL) (sql.grantPrivileges site sql.Engine.MicrosoftSQL)

  write (destination "generated_html.fs") generated_html_result
  write (destination "generated_views.fs") generated_views_result
  write (destination "generated_handlers.fs") generated_handlers_result
  write (destination "generated_forms.fs") generated_forms_result
  write (destination "generated_types.fs") generated_types_result
  write (destination "generated_bundles.fs") generated_bundles_result
  write (destination "generated_paths.fs") generated_paths_result
  write (destination "generated_validation.fs") generated_validation_result
  write (destination "generated_unittests.fs") generated_unittests_result
  write (destination "generated_uitests.fs") generated_uitests_result
  write (destination "generated_fake_data.fs") generated_fake_data_result

  write (destination "generated_data_access.fs") generated_data_access_result

  write (destination "generated_security.fs") generated_security_result

  write (destination "generated_dbname.txt") site.AsDatabase
  write (destination "generated_sql_createdb.sql") generated_sql_createdb_result
  write (destination "generated_sql_initialSetup.sql") generated_sql_initialSetup_result
  write (destination "generated_sql_createTables.sql") generated_sql_createTables_result
