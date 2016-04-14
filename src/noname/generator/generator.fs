module generator

open System
open dsl
open modules

let write path value = IO.File.WriteAllText(path, value)
let executingDir = IO.Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly().CodeBase)
let destination filename =
  executingDir
  |> UriBuilder
  |> (fun uri -> uri.Path)
  |> IO.Path.GetDirectoryName
  |> (fun path -> path.Replace("/bin", "/generated"))
  |> (fun path -> sprintf "%s/%s" path filename)

let zipOptions (options : string list) =
  //clean out empty strings, append one at the end
  let options = options |> List.filter (fun str -> str <> "")
  let results =
    List.zip [ 1 .. options.Length ] options
    |> List.map (fun (i, s) -> string i, s)
  ["0", ""] @ results

let fieldToHtml (field : Field) =
  let template tag = sprintf """%s "%s" "" """ tag field.Name
  let iconTemplate tag icon = sprintf """%s "%s" "" "%s" """ tag field.Name icon
  match field.FieldType with
  | Id         -> sprintf """hiddenInput "%s" "-1" """ field.AsProperty
  | Text       -> template "label_text"
  | Paragraph  -> template "label_textarea"
  | Number     -> template "label_text"
  | Decimal    -> template "label_text"
  | Date       -> template "label_text"
  | Phone      -> template "label_text"
  | Email      -> iconTemplate "icon_label_text" "envelope"
  | Name       -> iconTemplate "icon_label_text" "user"
  | Password   -> iconTemplate "icon_password_text" "lock"
  | Dropdown options -> sprintf """label_select "%s" %A """ field.Name (zipOptions options)

let fieldToPopulatedHtml page (field : Field) =
  let template tag = sprintf """%s "%s" %s.%s """ tag field.Name page.AsVal field.AsProperty
  let iconTemplate tag icon = sprintf """%s "%s" %s.%s "%s" """ tag field.Name page.AsVal field.AsProperty icon
  match field.FieldType with
  | Id         -> sprintf """hiddenInput "%s" %s.%s """ field.AsProperty page.AsVal field.AsProperty
  | Text       -> template "label_text"
  | Paragraph  -> template "label_textarea"
  | Number     -> template "label_text"
  | Decimal    -> template "label_text"
  | Date       -> template "label_text"
  | Phone      -> template "label_text"
  | Email      -> iconTemplate "icon_label_text" "envelope"
  | Name       -> iconTemplate "icon_label_text" "user"
  | Password   -> iconTemplate "icon_password_text" "lock"
  | Dropdown options -> sprintf """label_select_selected "%s" %A (Some %s.%s)""" field.Name (zipOptions options) page.AsVal field.AsProperty

let fieldToStaticHtml page (field : Field) =
  let template tag = sprintf """%s "%s" %s.%s """ tag field.Name page.AsVal field.AsProperty
  match field.FieldType with
  | Id         -> ""
  | Text       -> template "label_static"
  | Paragraph  -> template "label_static"
  | Number     -> template "label_static"
  | Decimal    -> template "label_static"
  | Date       -> template "label_static"
  | Phone      -> template "label_static"
  | Email      -> template "label_static"
  | Name       -> template "label_static"
  | Password   -> template "label_static"
  | Dropdown _ -> sprintf """label_static "%s" %s.%s """ field.Name page.AsVal field.AsProperty

let fieldToErroredHtml page (field : Field) =
  let template tag = sprintf """%s "%s" (string %s.%s) errors""" tag field.Name page.AsFormVal field.AsProperty
  let iconTemplate tag icon = sprintf """%s "%s" %s.%s "%s" errors""" tag field.Name page.AsFormVal field.AsProperty icon
  match field.FieldType with
  | Id         -> sprintf """hiddenInput "%s" %s.%s """ field.AsProperty page.AsFormVal field.AsProperty
  | Text       -> template "errored_label_text"
  | Paragraph  -> template "errored_label_textarea"
  | Number     -> template "errored_label_text"
  | Decimal    -> template "errored_label_text"
  | Date       -> template "errored_label_text"
  | Phone      -> template "errored_label_text"
  | Email      -> iconTemplate "errored_icon_label_text" "envelope"
  | Name       -> iconTemplate "errored_icon_label_text" "user"
  | Password   -> iconTemplate "errored_icon_password_text" "lock"
  | Dropdown options -> sprintf """errored_label_select "%s" %A (Some %s.%s) errors""" field.Name (zipOptions options) page.AsFormVal field.AsProperty

let fieldToProperty field =
  match field.FieldType with
  | Id         -> "int64"
  | Text       -> "string"
  | Paragraph  -> "string"
  | Number     -> "int"
  | Decimal    -> "double"
  | Date       -> "System.DateTime"
  | Phone      -> "string"
  | Email      -> "string"
  | Name       -> "string"
  | Password   -> "string"
  | Dropdown _ -> "int16"

let fieldToConvertProperty page field =
  let property = sprintf "%s.%s" page.AsFormVal field.AsProperty
  let string () = sprintf """%s = %s""" field.AsProperty property
  let int () = sprintf """%s = int %s""" field.AsProperty property
  let int16 () = sprintf """%s = int16 %s""" field.AsProperty property
  let int64 () = sprintf """%s = int64 %s""" field.AsProperty property
  let double () = sprintf """%s = double %s""" field.AsProperty property
  let datetime () = sprintf """%s = System.DateTime.Parse(%s)""" field.AsProperty property
  match field.FieldType with
  | Id         -> int64 ()
  | Text       -> string ()
  | Paragraph  -> string ()
  | Number     -> int ()
  | Decimal    -> double ()
  | Date       -> datetime ()
  | Email      -> string ()
  | Name       -> string ()
  | Phone      -> string ()
  | Password   -> string ()
  | Dropdown _ -> int16 ()

let fieldToValidation (field : Field) page =
  let template validation = sprintf """%s "%s" %s.%s""" validation field.Name page.AsFormVal field.AsProperty
  match field.FieldType with
  | Id         -> None
  | Text       -> None
  | Paragraph  -> None
  | Number     -> Some (template "validate_integer")
  | Decimal    -> Some (template "validate_double")
  | Date       -> Some (template "validate_datetime")
  | Phone      -> None //parsePhone?
  | Email      -> Some (template "validate_email")
  | Name       -> None
  | Password   -> Some (template "validate_password")
  | Dropdown _ -> None

let fieldToTestName (field : Field) =
  let template text = sprintf """"%s %s" """ field.Name text
  match field.FieldType with
  | Id         -> None
  | Text       -> None
  | Paragraph  -> None
  | Number     -> Some (template "must be a valid integer")
  | Decimal    -> Some (template "must be a valid double")
  | Date       -> Some (template "must be a valid date")
  | Email      -> Some (template "must be a valid email")
  | Name       -> None
  | Phone      -> None //parsePhone?
  | Password   -> Some (template "must be between 6 and 100 characters")
  | Dropdown _ -> None

let fieldToTestBody (field : Field) =
  let template text = sprintf """displayed "%s %s" """ field.Name text
  match field.FieldType with
  | Id         -> None
  | Text       -> None
  | Paragraph  -> None
  | Number     -> Some (template "is not a valid number (int)")
  | Decimal    -> Some (template "is not a valid number (decimal)")
  | Date       -> Some (template "is not a valid date")
  | Email      -> Some (template "is not a valid email")
  | Name       -> None
  | Phone      -> None //parsePhone?
  | Password   -> Some (template "must be between 6 and 100 characters")
  | Dropdown _ -> None

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
  | Required   -> Some (sprintf """"%s is required" """ field.Name)
  | Min(min)   -> Some (sprintf """"%s must be greater than %i" """ field.Name min)
  | Max(max)   -> Some (sprintf """"%s must be less than %i" """ field.Name max)
  | Range(min,max) -> Some (sprintf """"%s must be between %i and %i" """ field.Name min max)

let attributeToTestBody field =
  match field.Attribute with
  | PK         -> None
  | Null       -> None
  | Required   -> Some (sprintf """displayed "%s is required" """ field.Name)
  | Min(min)   -> Some (sprintf """displayed "%s can not be below %i" """ field.Name min)
  | Max(max)   -> Some (sprintf """displayed "%s can not be above %i" """ field.Name max)
  | Range(min,max) -> Some (sprintf """displayed "%s must be between %i and %i" """ field.Name min max)

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
let get_create_%s =
  base_html
    "Create %s"
    [
      base_header brand
      common_form
        "%s"
        [
%s
        ]
    ]
    scripts.common""" page.AsVal page.Name page.Name (formatEditFields page.Fields 5)

let createErroredFormViewTemplate (page : Page) =
  sprintf """
let post_create_errored_%s errors (%s : %s) =
  base_html
    "Create %s"
    [
      base_header brand
      common_form
        "%s"
        [
%s
        ]
    ]
    scripts.common""" page.AsVal page.AsFormVal page.AsFormType page.Name page.Name (formatErroredFields page page.Fields 5)

let editFormViewTemplate (page : Page) =
  sprintf """
let get_edit_%s (%s : %s) =
  base_html
    "Edit %s"
    [
      base_header brand
      common_form
        "%s"
        [
%s
        ]
    ]
    scripts.common""" page.AsVal page.AsVal page.AsType page.Name page.Name (formatPopulatedEditFields page page.Fields 5)

let editErroredFormViewTemplate (page : Page) =
  sprintf """
let post_edit_errored_%s errors (%s : %s) =
  base_html
    "%s"
    [
      base_header brand
      common_form
        "%s"
        [
%s
        ]
    ]
    scripts.common""" page.AsVal page.AsFormVal page.AsFormType page.Name page.Name (formatErroredFields page page.Fields 5)

let viewFormViewTemplate (page : Page) =
  let idField = page.Fields |> List.find (fun field -> field.FieldType = Id)
  sprintf """
let get_%s (%s : %s) =
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
    |> List.map (fun field -> sprintf """"%s" """ field.Name)
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
let get_list_%s () =
  let %ss = getMany_%s ()
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
              header [ h3Inner "%s" [ pull_right [ button_small_success "%s" [ text "Create"] ] ] ]
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
    scripts.datatable_bundle""" page.AsVal page.AsVal page.AsType page.AsVal page.AsType page.AsViewHref page.AsVal idField.AsProperty page.AsVal page.AsType (fieldsToTd page) page.Name page.Name page.AsCreateHref (fieldsToHeaders page) page.AsVal

let submitFormViewTemplate (page : Page) =
  sprintf """
let get_submit_%s =
  base_html
    "%s"
    [
      base_header brand
      common_submit_form
        "%s"
        [
%s
        ]
    ]
    scripts.common""" page.AsVal page.Name page.Name (formatEditFields page.Fields 5)

let submitErroredFormViewTemplate (page : Page) =
  sprintf """
let post_submit_errored_%s errors (%s : %s) =
  base_html
    "%s"
    [
      base_header brand
      common_submit_form
        "%s"
        [
%s
        ]
    ]
    scripts.common""" page.AsVal page.AsFormVal page.AsFormType page.Name page.Name (formatErroredFields page page.Fields 5)

let loginFormViewTemplate (page : Page) =
  sprintf """
let get_login_%s =
  base_html
    "%s"
    [
      base_header brand
      common_submit_form
        "%s"
        [
%s
        ]
    ]
    scripts.common""" page.AsVal page.Name page.Name (formatEditFields page.Fields 5)

let loginErroredFormViewTemplate (page : Page) =
  sprintf """
let post_login_errored_%s errors (%s : %s) =
  base_html
    "%s"
    [
      base_header brand
      common_submit_form
        "%s"
        [
%s
        ]
    ]
    scripts.common""" page.AsVal page.AsFormVal page.AsFormType page.Name page.Name (formatErroredFields page page.Fields 5)

let jumbotronViewTemplate (site : Site) (page : Page) =
  sprintf """
let get_%s =
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
    | CVEL      -> [Create; View; Edit; List] |> List.map (viewTemplate site page) |> flatten
    | Create    -> [createFormViewTemplate page; createErroredFormViewTemplate page] |> flatten
    | Edit      -> [editFormViewTemplate page; editErroredFormViewTemplate page] |> flatten
    | View      -> viewFormViewTemplate page
    | List      -> listFormViewTemplate page
    | Submit    -> [submitFormViewTemplate page; submitErroredFormViewTemplate page] |> flatten
    | Login     -> [loginFormViewTemplate page; loginErroredFormViewTemplate page] |> flatten
    | Jumbotron -> jumbotronViewTemplate site page

  viewTemplate site page page.PageMode

let pageLinkTemplate (page : Page) =
  let template href text = sprintf """li [ aHref "%s" [text "%s"] ]""" href text |> pad 7
  let rec pageLinkTemplate page pageMode =
    match pageMode with
    | CVEL      -> [Create; View; Edit; List] |> List.map (pageLinkTemplate page) |> flatten
    | Create    -> template page.AsCreateHref (sprintf "Create %s" page.Name)
    | Edit      -> template page.AsEditHref (sprintf "Edit %s" page.Name)
    | View      -> template page.AsViewHref page.Name
    | List      -> template page.AsListHref (sprintf "List %ss" page.Name)
    | Submit    -> template page.AsCreateHref page.Name
    | Login     -> template page.AsCreateHref page.Name
    | Jumbotron -> template page.AsHref page.Name

  pageLinkTemplate page page.PageMode

let pathTemplate page =
  let template extra href withId =
    match withId with
    | false -> sprintf """let path_%s%s = "%s" """ extra page.AsVal href
    | true -> sprintf """let path_%s%s : IntPath = "%s" """ extra page.AsVal href
  let rec pathTemplate page pageMode =
    match pageMode with
    | CVEL      -> [Create; View; Edit; List] |> List.map (pathTemplate page) |> flatten
    | Create    -> template "create_" page.AsCreateHref false
    | Edit      -> template "edit_" page.AsEditHref true
    | View      -> template "" page.AsViewHref true
    | List      -> template "list_" page.AsListHref false
    | Submit    -> template "submit_" page.AsCreateHref false
    | Login     -> template "login_" page.AsCreateHref false
    | Jumbotron -> template "" page.AsHref false

  pathTemplate page page.PageMode

let routeTemplate page =
  let template extra withId =
    match withId with
    | false -> sprintf """path path_%s%s >=> %s%s""" extra page.AsVal extra page.AsVal |> pad 2
    | true -> sprintf """pathScan path_%s%s %s%s""" extra page.AsVal extra page.AsVal |> pad 2
  let rec routeTemplate page pageMode =
    match pageMode with
    | CVEL      -> [Create; View; Edit; List] |> List.map (routeTemplate page) |> flatten
    | Create    -> template "create_" false
    | Edit      -> template "edit_" true
    | View      -> template "" true
    | List      -> template "list_" false
    | Submit    -> template "submit_" false
    | Login     -> template "login_" false
    | Jumbotron -> template "" false

  routeTemplate page page.PageMode

let handlerTemplate page =
  let rec handlerTemplate page pageMode =
    match pageMode with
    | CVEL -> [Create; View; Edit; List] |> List.map (handlerTemplate page) |> flatten
    | Edit ->
      let idField = page.Fields |> List.find (fun field -> field.FieldType = Id)
      sprintf """let edit_%s id =
    choose
      [
        GET >=> warbler (fun _ ->
          let data = tryById_%s id
          match data with
          | None -> OK error_404
          | Some(data) -> OK <| get_edit_%s data)
        POST >=> bindToForm %s (fun %s ->
          let validation = validate%s %s
          if validation = [] then
            let converted = convert%s %s
            update_%s converted
            FOUND <| sprintf "%s" converted.%s
          else
            OK (post_edit_errored_%s validation %s))
      ]""" page.AsVal page.AsType page.AsVal page.AsFormVal page.AsFormVal page.AsFormType page.AsFormVal page.AsFormType page.AsFormVal page.AsType page.AsViewHref idField.AsProperty page.AsVal page.AsFormVal
    | View ->
      sprintf """let %s id =
  GET >=> warbler (fun _ ->
    let data = tryById_%s id
    match data with
    | None -> OK error_404
    | Some(data) -> OK <| get_%s data)""" page.AsVal page.AsType page.AsVal
    | List ->
      sprintf """let list_%s = GET >=> warbler (fun _ -> OK <| get_list_%s ())""" page.AsVal page.AsVal
    | Jumbotron ->
      sprintf """let %s = GET >=> OK get_%s""" page.AsVal page.AsVal
    | Create ->
      sprintf """let create_%s =
    choose
      [
        GET >=> OK get_create_%s
        POST >=> bindToForm %s (fun %s ->
          let validation = validate%s %s
          if validation = [] then
            let converted = convert%s %s
            let id = insert_%s converted
            OK (string id)
          else
            OK (post_create_errored_%s validation %s))
      ]""" page.AsVal page.AsVal page.AsFormVal page.AsFormVal page.AsFormType page.AsFormVal page.AsFormType page.AsFormVal page.AsType page.AsVal page.AsFormVal
    | Submit    ->
      sprintf """let submit_%s =
    choose
      [
        GET >=> OK get_submit_%s
        POST >=> bindToForm %s (fun %s ->
          let validation = validate%s %s
          if validation = [] then
            let converted = convert%s %s
            let id = insert_%s converted
            OK (string id)
          else
            OK (post_submit_errored_%s validation %s))
      ]""" page.AsVal page.AsVal page.AsFormVal page.AsFormVal page.AsFormType page.AsFormVal page.AsFormType page.AsFormVal page.AsType page.AsVal page.AsFormVal
    | Login    ->
      sprintf """let login_%s =
    choose
      [
        GET >=> OK get_login_%s
        POST >=> bindToForm %s (fun %s ->
          let validation = validate%s %s
          if validation = [] then
            let converted = convert%s %s
            ignore converted
            OK ""
          else
            OK (post_login_errored_%s validation %s))
      ]""" page.AsVal page.AsVal page.AsFormVal page.AsFormVal page.AsFormType page.AsFormVal page.AsFormType page.AsFormVal page.AsVal page.AsFormVal

  handlerTemplate page page.PageMode

let propertyTemplate (page : Page) =
  page.Fields
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

%s
  """ page.AsFormType (formPropertyTemplate page) page.AsFormVal page.AsFormType (converterTemplate page)

let validationTemplate (page : Page) =
  if page.Fields = [] then ""
  else
    let validations =
      page.Fields
      |> List.map (fun field -> [fieldToValidation field page] @ [attributeToValidation field page])
      |> List.concat
      |> List.choose id
      |> List.map (pad 2)
      |> flatten

    sprintf """let validate%s (%s : %s) =
  [
%s
  ] |> List.choose id
  """ page.AsFormType page.AsFormVal page.AsFormType validations

let contextTemplate (page : Page) = sprintf """context "%s" """ page.Name |> pad 1

let onceTemplate (page : Page) =
  sprintf """once (fun _ -> url "http://localhost:8083%s"; click ".btn") """ page.AsCreateHref
  |> pad 1

let attributeUITestTemplate name field =
  match name with
  | None -> None
  | Some(name) ->
    let top = (sprintf """%s&&& fun _ ->""" name) |> pad 1
    let body = (attributeToTestBody field).Value |> pad 2
    Some (sprintf """%s
%s
    """ top body)

let fieldUITestTemplate name field =
  match name with
  | None -> None
  | Some(name) ->
    let top = (sprintf """%s&&& fun _ ->""" name) |> pad 1
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
    let tests = page.Fields |> List.map toTest |> List.concat |> List.choose id |> flatten
    sprintf """%s

%s

%s
    """ (contextTemplate page) (onceTemplate page) tests

let generate (site : Site) =
  let html_results = site.Pages |> List.map pageLinkTemplate |> flatten
  let generated_html_result = generated_html_template html_results

  let views_results = site.Pages |> List.map (viewTemplate site) |> flatten
  let generated_views_result = generated_views_template site.Name views_results

  let handler_results = site.Pages |> List.map handlerTemplate |> flatten
  let generated_handlers_result = generated_handlers_template handler_results

  let forms_results = site.Pages |> List.map formTypeTemplate |> flatten
  let generated_forms_result = generated_forms_template forms_results

  let types_results = site.Pages |> List.map typeTemplate |> flatten
  let generated_types_result = generated_types_template types_results

  let paths_results = site.Pages |> List.map pathTemplate |> flatten
  let routes_results = site.Pages |> List.map routeTemplate |> flatten
  let generated_paths_result = generated_paths_template paths_results routes_results

  let validations_results = site.Pages |> List.map validationTemplate |> flatten
  let generated_validation_result = generated_validation_template validations_results

  let generated_unittests_result = generated_unittests_template "//nothing"

  let uitests_results = site.Pages |> List.map uitestTemplate |> flatten
  let generated_uitests_result = generated_uitests_template uitests_results

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
  write (destination "generated_paths.fs") generated_paths_result
  write (destination "generated_validation.fs") generated_validation_result
  write (destination "generated_unittests.fs") generated_unittests_result
  write (destination "generated_uitests.fs") generated_uitests_result

  write (destination "generated_data_access.fs") generated_data_result

  write (destination "generated_dbname.txt") site.AsDatabase
  write (destination "generated_sql_createdb.sql") generated_sql_createdb_result
  write (destination "generated_sql_initialSetup.sql") generated_sql_initialSetup_result
  write (destination "generated_sql_createTables.sql") generated_sql_createTables_result
