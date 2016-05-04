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

let zipOptions (options : string list) =
  //clean out empty strings, append one at the end
  let options = options |> List.filter (fun str -> str <> "")
  let results =
    List.zip [ 1 .. options.Length ] options
    |> List.map (fun (i, s) -> string i, s)
  ["0", ""] @ results

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

let fieldToPopulatedHtml page (field : Field) =
  let template tag = sprintf """%s "%s" %s.%s """ tag field.Name page.AsVal field.AsProperty |> trimEnd
  let iconTemplate tag icon = sprintf """%s "%s" %s.%s "%s" """ tag field.Name page.AsVal field.AsProperty icon |> trimEnd
  match field.FieldType with
  | Id                -> sprintf """hiddenInput "%s" %s.%s """ field.AsProperty page.AsVal field.AsProperty
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
  | Dropdown options  -> sprintf """label_select_selected "%s" %A (Some %s.%s)""" field.Name (zipOptions options) page.AsVal field.AsProperty

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

let fieldToProperty field =
  match field.FieldType with
  | Id              -> "int64"
  | Text            -> "string"
  | Paragraph       -> "string"
  | Number          -> "int"
  | Decimal         -> "double"
  | Date            -> "System.DateTime"
  | Phone           -> "string"
  | Email           -> "string"
  | Name            -> "string"
  | Password        -> "string"
  | ConfirmPassword -> "string"
  | Dropdown _      -> "int16"

let fieldToConvertProperty page field =
  let property = sprintf "%s.%s" page.AsFormVal field.AsProperty
  let string () = sprintf """%s = %s""" field.AsProperty property
  let int () = sprintf """%s = int %s""" field.AsProperty property
  let int16 () = sprintf """%s = int16 %s""" field.AsProperty property
  let int64 () = sprintf """%s = int64 %s""" field.AsProperty property
  let double () = sprintf """%s = double %s""" field.AsProperty property
  let datetime () = sprintf """%s = System.DateTime.Parse(%s)""" field.AsProperty property
  match field.FieldType with
  | Id              -> int64 ()
  | Text            -> string ()
  | Paragraph       -> string ()
  | Number          -> int ()
  | Decimal         -> double ()
  | Date            -> datetime ()
  | Email           -> string ()
  | Name            -> string ()
  | Phone           -> string ()
  | Password        -> string ()
  | ConfirmPassword -> string ()
  | Dropdown _      -> int16 ()

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

let attributeToValidation field page =
  let property = sprintf "%s.%s" page.AsFormVal field.AsProperty
  match field.Attribute with
  | PK         -> None
  | Null       -> None
  | Required   -> Some (sprintf """validate_required "%s" %s""" field.Name property)
  | Min(min)   -> Some (sprintf """validate_min "%s" %s %i""" field.Name property min)
  | Max(max)   -> Some (sprintf """validate_max "%s" %s %i""" field.Name property max)
  | Range(min,max) -> Some (sprintf """validate_range "%s" %s %i %i""" field.Name property min max)

let attributeToTestName field =
  match field.Attribute with
  | PK         -> None
  | Null       -> None
  | Required   -> Some (sprintf """"%s is required" """ field.Name |> trimEnd)
  | Min(min)   -> Some (sprintf """"%s must be greater than %i" """ field.Name min |> trimEnd)
  | Max(max)   -> Some (sprintf """"%s must be less than %i" """ field.Name max |> trimEnd)
  | Range(min,max) -> Some (sprintf """"%s must be between %i and %i" """ field.Name min max |> trimEnd)

let attributeToTestBody field =
  match field.Attribute with
  | PK         -> None
  | Null       -> None
  | Required   -> Some (sprintf """displayed "%s is required" """ field.Name |> trimEnd)
  | Min(min)   -> Some (sprintf """displayed "%s can not be below %i" """ field.Name min |> trimEnd)
  | Max(max)   -> Some (sprintf """displayed "%s can not be above %i" """ field.Name max |> trimEnd)
  | Range(min,max) -> Some (sprintf """displayed "%s must be between %i and %i" """ field.Name min max |> trimEnd)

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

let viewFormViewTemplate (page : Page) =
  let idField = page.Fields |> List.find (fun field -> field.FieldType = Id)
  sprintf """
let view_view_%s (%s : %s) =
  let button = [ button_small_success (sprintf "%s" %s.%s) [ text "Edit"] ]
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
    scripts.common""" page.AsVal page.AsVal page.AsType page.AsEditHref page.AsVal idField.AsProperty page.Name page.Name (formatStaticFields page page.Fields 5)

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

let listFormViewTemplate (page : Page) =
  let idField = page.Fields |> List.find (fun field -> field.FieldType = Id)
  sprintf """
let view_list_%s %ss =
  let toTr (%s : %s) inner =
    trLink (sprintf "%s" %s.%s) inner

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
              header [ h3Inner "List %ss" [ pull_right [ button_small_success "%s" [ text "Create"] ] ] ]
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
    scripts.datatable_bundle""" page.AsVal page.AsVal page.AsVal page.AsType page.AsViewHref page.AsVal idField.AsProperty page.AsVal page.AsType (fieldsToTd page) page.Name page.Name page.AsCreateHref (fieldsToHeaders page) page.AsVal

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

let loginFormViewTemplate (page : Page) =
  sprintf """
let view_login =
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
    | Login     -> [loginFormViewTemplate page; loginErroredFormViewTemplate page] |> flatten
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
    | false -> sprintf """path path_%s%s >=> %s%s""" extra page.AsVal extra page.AsVal |> pad 2
    | true -> sprintf """pathScan path_%s%s %s%s""" extra page.AsVal extra page.AsVal |> pad 2
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
  GET >=> warbler (fun _ -> getMany_%s () |> view_list_%s |> OK)""" page.AsVal page.AsType page.AsVal
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
        let validation = validate%s %s
        if validation = [] then
          let converted = convert%s %s
          let id = insert_%s converted
          FOUND "/"
        else
          OK (view_errored_%s validation %s))
    ]""" page.AsFormVal page.AsFormVal page.AsFormType page.AsFormVal page.AsFormType page.AsFormVal page.AsType page.AsVal page.AsFormVal
    | Login    ->
      sprintf """
let login =
  choose
    [
      GET >=> OK view_login
      POST >=> bindToForm %s (fun %s ->
        let validation = validate%s %s
        if validation = [] then
          let converted = convert%s %s
          ignore converted
          OK ""
        else
          OK (view_errored_%s validation %s))
    ]"""  page.AsFormVal page.AsFormVal page.AsFormType page.AsFormVal page.AsFormType page.AsFormVal page.AsVal page.AsFormVal

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
         >=> OK (serializer.PickleToString(data)))""" api.AsVal api.AsType

let propertyTemplate (page : Page) =
  page.Fields
  |> List.filter (fun field -> field.FieldType <> ConfirmPassword)
  |> List.map (fun field -> sprintf """%s : %s""" field.AsProperty (fieldToProperty field))
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
  if page.Fields = [] then ""
  else
    sprintf """type %s =
  {
%s
  }
  """ page.AsType (propertyTemplate page)

let bundleTemplate (page : Page) =
  if (not (page.PageMode = Create || page.PageMode = CVEL || page.PageMode = CVELS) ) || page.Fields = [] then ""
  else
    String.Format("""let bundle_{0} : Bundle<{1}, {1}Form> =
  {{
    validateForm = validate{1}Form
    convertForm = convert{1}Form
    fake_single = fake_{0}
    fake_many = fake_many_{0}
    tryById = tryById_{1}
    getMany = getMany_{1}
    getManyWhere = getManyWhere_{1}
    insert = insert_{1}
    update = update_{1}
    view_list = view_list_{0}
    view_edit = view_edit_{0}
    view_create = view_create_{0}
    view_view = view_view_{0}
    view_search = view_search_{0}
    view_edit_errored = view_edit_errored_{0}
    view_create_errored = view_create_errored_{0}
    href_search = "{2}"
    href_view = "{3}"
  }}
  """, page.AsVal, page.AsType, page.AsSearchHref, page.AsViewHref)

let converterTemplate (page : Page) =
  if page.Fields = [] then ""
  else
    sprintf """let convert%s (%s : %s) : %s =
  {
%s
  }
  """ page.AsFormType page.AsFormVal page.AsFormType page.AsType (converterPropertyTemplate page)

let formTypeTemplate (page : Page) =
  if page.Fields = [] then ""
  else
    sprintf """type %s =
  {
%s
  }

let %s : Form<%s> = Form ([],[])

%s""" page.AsFormType (formPropertyTemplate page) page.AsFormVal page.AsFormType (converterTemplate page)

let validationTemplate (page : Page) =
  if page.Fields = [] then ""
  else
    let validations =
      page.Fields
      |> List.collect (fun field -> [fieldToValidation field page] @ [attributeToValidation field page])
      |> List.choose id
      |> List.map (pad 2)
      |> flatten

    sprintf """let validate%s (%s : %s) =
  [
%s
  ] |> List.choose id
  """ page.AsFormType page.AsFormVal page.AsFormType validations

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
  if page.Fields = [] then ""
  else
    let tests = page.Fields |> List.collect toTest |> List.choose id |> flatten
    sprintf """%s

%s

%s
    """ (contextTemplate page) (onceTemplate page) tests

//let cityStateZip = randomItem citiesSatesZips
let fakePropertyTemplate (field : Field) =
  let lowered = field.Name.ToLower()
  let pickAppropriateText defaultValue =
    if lowered.Contains("last") && lowered.Contains("name")
    then "randomItem lastNames"
    else if lowered.Contains("first") && lowered.Contains("name")
    then "randomItem firstNames"
    else if lowered.Contains("name")
    then """(randomItem firstNames) + " " + (randomItem lastNames)"""
    else if lowered.Contains("city")
    then "cityStateZip.City"
    else if lowered.Contains("state")
    then "cityStateZip.State"
    else if lowered.Contains("zip") || lowered.Contains("postal")
    then "cityStateZip.Zip"
    else if lowered.Contains("address") || lowered.Contains("street")
    then """(string (random.Next(100,9999))) + " " + (randomItem streetNames) + " " + (randomItem streetNameSuffixes)"""
    else defaultValue

  let pickAppropriateNumber defaultValue =
    defaultValue

  let pickAppropriateName defaultValue =
    if lowered.Contains("first")
    then "randomItem firstNames"
    else if lowered.Contains("last")
    then "randomItem lastNames"
    else defaultValue

  let value =
    match field.FieldType with
    | Id              -> "-1L"
    | Text            -> pickAppropriateText "randomItems 6 words"
    | Paragraph       -> "randomItems 40 words"
    | Number          -> pickAppropriateNumber "random.Next(100)"
    | Decimal         -> "random.Next(100) |> double"
    | Date            -> "System.DateTime.Now"
    | Phone           -> """sprintf "%i-%i-%i" (random.Next(200,800)) (random.Next(200,800)) (random.Next(2000,8000))"""
    | Email           -> """sprintf "%s@%s.com" (randomItem words) (randomItem words)"""
    | Name            -> pickAppropriateName """randomItem names"""
    | Password        -> """"123123" """ |> trimEnd
    | ConfirmPassword -> """"123123" """ |> trimEnd
    | Dropdown _      -> "1s"
  sprintf """%s = %s """ field.AsProperty value

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
 """ page.AsVal page.AsVal page.AsType

let fakeDataTemplate (page : Page) =
  if (not (page.PageMode = Create || page.PageMode = CVEL || page.PageMode = CVELS) ) || page.Fields = [] then ""
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

  let connectionString = sprintf "Server=127.0.0.1;User Id=%s; Password=secure123;Database=%s;" site.AsDatabase site.AsDatabase
  let generated_data_result = generated_data_access_template connectionString (sql.createQueries site)

  let generated_sql_createdb_result = sql.createTemplate site.AsDatabase
  let generated_sql_initialSetup_result = sql.initialSetupTemplate site.AsDatabase
  let generated_sql_createTables_result = sql.createTables (sql.createTableTemplates site) (sql.grantPrivileges site)

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

  write (destination "generated_data_access.fs") generated_data_result

  write (destination "generated_dbname.txt") site.AsDatabase
  write (destination "generated_sql_createdb.sql") generated_sql_createdb_result
  write (destination "generated_sql_initialSetup.sql") generated_sql_initialSetup_result
  write (destination "generated_sql_createTables.sql") generated_sql_createTables_result
