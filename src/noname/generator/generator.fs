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
  |> (fun path -> sprintf "%s/%s.fs" path filename)

let zipOptions (options : string list) =
  //clean out empty strings, append one at the end
  let options = options |> List.filter (fun str -> str <> "")
  let results =
    List.zip [ 1 .. options.Length ] options
    |> List.map (fun (i, s) -> string i, s)
  ["0", ""] @ results

let repeat (value : string) times = [1..times] |> List.map (fun _ -> value) |> List.reduce (+)
let flatten values =
  if values = []
  then ""
  else values |> List.reduce (fun value1 value2 -> sprintf "%s%s%s" value1 Environment.NewLine value2)

let fieldToHtml field =
  match field.FieldType with
  | Text       -> sprintf """label_text "%s" "" """ field.Name
  | Paragraph  -> sprintf """label_textarea "%s" "" """ field.Name
  | Number     -> sprintf """label_text "%s" "" """ field.Name
  | Decimal    -> sprintf """label_text "%s" "" """ field.Name
  | Date       -> sprintf """label_text "%s" "" """ field.Name
  | Email      -> sprintf """icon_label_text "%s" "" "envelope" """ field.Name
  | Name       -> sprintf """icon_label_text "%s" "" "user" """ field.Name
  | Phone      -> sprintf """label_text "%s" "" """ field.Name
  | Password   -> sprintf """icon_password_text "%s" "" "lock" """ field.Name
  | Dropdown options -> sprintf """label_select "%s" %A """ field.Name (zipOptions options)

let fieldToPopulatedHtml page field =
  match field.FieldType with
  | Text       -> sprintf """label_text "%s" "%s.%s" """ field.Name page.AsFormVal field.AsProperty
  | Paragraph  -> sprintf """label_textarea "%s" "%s.%s" """ field.Name page.AsFormVal field.AsProperty
  | Number     -> sprintf """label_text "%s" "%s.%s" """ field.Name page.AsFormVal field.AsProperty
  | Decimal    -> sprintf """label_text "%s" "%s.%s" """ field.Name page.AsFormVal field.AsProperty
  | Date       -> sprintf """label_text "%s" "%s.%s" """ field.Name page.AsFormVal field.AsProperty
  | Email      -> sprintf """icon_label_text "%s" "%s.%s" "envelope" """ field.Name page.AsFormVal field.AsProperty
  | Name       -> sprintf """icon_label_text "%s" "%s.%s" "user" """ field.Name page.AsFormVal field.AsProperty
  | Phone      -> sprintf """label_text "%s" "%s.%s" """ field.Name page.AsFormVal field.AsProperty
  | Password   -> sprintf """icon_password_text "%s" "%s.%s" "lock" """ field.Name page.AsFormVal field.AsProperty
  | Dropdown options -> sprintf """label_select_selected "%s" %A (Some "%s.%s")""" field.Name (zipOptions options) page.AsFormVal field.AsProperty

let fieldToErroredHtml page field =
  match field.FieldType with
  | Text       -> sprintf """errored_label_text "%s" %s.%s errors""" field.Name page.AsFormVal field.AsProperty
  | Paragraph  -> sprintf """errored_label_textarea "%s" %s.%s errors""" field.Name page.AsFormVal field.AsProperty
  | Number     -> sprintf """errored_label_text "%s" %s.%s errors""" field.Name page.AsFormVal field.AsProperty
  | Decimal    -> sprintf """errored_label_text "%s" %s.%s errors""" field.Name page.AsFormVal field.AsProperty
  | Date       -> sprintf """errored_label_text "%s" %s.%s errors""" field.Name page.AsFormVal field.AsProperty
  | Email      -> sprintf """errored_icon_label_text "%s" %s.%s "envelope" errors""" field.Name page.AsFormVal field.AsProperty
  | Name       -> sprintf """errored_icon_label_text "%s" %s.%s "user" errors""" field.Name page.AsFormVal field.AsProperty
  | Phone      -> sprintf """errored_label_text "%s" %s.%s errors""" field.Name page.AsFormVal field.AsProperty
  | Password   -> sprintf """errored_icon_password_text "%s" %s.%s "lock" errors""" field.Name page.AsFormVal field.AsProperty
  | Dropdown options -> sprintf """errored_label_select "%s" %A (Some %s.%s) errors""" field.Name (zipOptions options) page.AsFormVal field.AsProperty

let fieldToProperty field =
  match field.FieldType with
  | Text       -> "string"
  | Paragraph  -> "string"
  | Number     -> "int"
  | Decimal    -> "double"
  | Date       -> "System.DateTime"
  | Email      -> "string"
  | Name       -> "string"
  | Phone      -> "string"
  | Password   -> "string"
  | Dropdown _ -> "int"

let fieldToConvertProperty page field =
  let string field page = sprintf """%s = %s.%s""" field.AsProperty page.AsFormVal field.AsProperty
  let int field page = sprintf """%s = int %s.%s""" field.AsProperty page.AsFormVal field.AsProperty
  let double field page = sprintf """%s = double %s.%s""" field.AsProperty page.AsFormVal field.AsProperty
  let datetime field page = sprintf """%s = System.DateTime.Parse(%s.%s)""" field.AsProperty page.AsFormVal field.AsProperty
  match field.FieldType with
  | Text       -> string field page
  | Paragraph  -> string field page
  | Number     -> int field page
  | Decimal    -> double field page
  | Date       -> datetime field page
  | Email      -> string field page
  | Name       -> string field page
  | Phone      -> string field page
  | Password   -> string field page
  | Dropdown _ -> int field page

let fieldToValidation field page =
  let property = sprintf "%s.%s" page.AsFormVal field.AsProperty
  match field.FieldType with
  | Text       -> None
  | Paragraph  -> None
  | Number     -> Some (sprintf """validate_integer "%s" %s""" field.Name property)
  | Decimal    -> Some (sprintf """validate_double "%s" %s""" field.Name property)
  | Date       -> Some (sprintf """validate_datetime "%s" %s""" field.Name property)
  | Email      -> Some (sprintf """validate_email "%s" %s""" field.Name property)
  | Name       -> None
  | Phone      -> None //parsePhone?
  | Password   -> Some (sprintf """validate_password "%s" %s""" field.Name property)
  | Dropdown _ -> None

let fieldToTestName field =
  match field.FieldType with
  | Text       -> None
  | Paragraph  -> None
  | Number     -> Some (sprintf """"%s must be a valid integer" """ field.Name)
  | Decimal    -> Some (sprintf """"%s must be a valid double" """ field.Name)
  | Date       -> Some (sprintf """"%s must be a valid date" """ field.Name)
  | Email      -> Some """"must be a valid email" """
  | Name       -> None
  | Phone      -> None //parsePhone?
  | Password   -> Some (sprintf """"%s must be between 6 and 100 characters" """ field.Name)
  | Dropdown _ -> None

let fieldToTestBody field =
  match field.FieldType with
  | Text       -> None
  | Paragraph  -> None
  | Number     -> Some (sprintf """displayed "%s is not a valid number (int)" """ field.Name)
  | Decimal    -> Some (sprintf """displayed "%s is not a valid number (decimal)" """ field.Name)
  | Date       -> Some (sprintf """displayed "%s is not a valid date" """ field.Name)
  | Email      -> Some """displayed " is not a valid email" """
  | Name       -> None
  | Phone      -> None //parsePhone?
  | Password   -> Some (sprintf """displayed "%s must be between 6 and 100 characters" """ field.Name)
  | Dropdown _ -> None

let attributeToValidation field page =
  let property = sprintf "%s.%s" page.AsFormVal field.AsProperty
  match field.Attribute with
  | Id         -> None
  | Null       -> None
  | Required   -> Some (sprintf """validate_required "%s" %s""" field.Name property)
  | Min(min)   -> Some (sprintf """validate_min "%s" %s %i""" field.Name property min)
  | Max(max)   -> Some (sprintf """validate_max "%s" %s %i""" field.Name property max)
  | Range(min,max) -> Some (sprintf """validate_range "%s" %s %i %i""" field.Name property min max)

let attributeToTestName field =
  match field.Attribute with
  | Id         -> None
  | Null       -> None
  | Required   -> Some (sprintf """"%s is required" """ field.Name)
  | Min(min)   -> Some (sprintf """"%s must be greater than %i" """ field.Name min)
  | Max(max)   -> Some (sprintf """"%s must be less than %i" """ field.Name max)
  | Range(min,max) -> Some (sprintf """"%s must be between %i and %i" """ field.Name min max)

let attributeToTestBody field =
  match field.Attribute with
  | Id         -> None
  | Null       -> None
  | Required   -> Some (sprintf """displayed "%s is required" """ field.Name)
  | Min(min)   -> Some (sprintf """displayed "%s can not be below %i" """ field.Name min)
  | Max(max)   -> Some (sprintf """displayed "%s can not be above %i" """ field.Name max)
  | Range(min,max) -> Some (sprintf """displayed "%s must be between %i and %i" """ field.Name min max)

let pad tabs field = sprintf "%s%s" (repeat "  " tabs) field

let formatEditFields page (fields : Field list) tabs =
  fields
  |> List.map (fieldToPopulatedHtml page)
  |> List.map (pad tabs)
  |> List.reduce (fun field1 field2 -> sprintf "%s%s%s" field1 Environment.NewLine field2)

let formatSubmitFields (fields : Field list) tabs =
  fields
  |> List.map fieldToHtml
  |> List.map (pad tabs)
  |> List.reduce (fun field1 field2 -> sprintf "%s%s%s" field1 Environment.NewLine field2)

let formatSubmitErroredFields page (fields : Field list) tabs =
  fields
  |> List.map (fieldToErroredHtml page)
  |> List.map (pad tabs)
  |> List.reduce (fun field1 field2 -> sprintf "%s%s%s" field1 Environment.NewLine field2)

let bannerViewTemplate (site : Site) (page : Page) =
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

let editFormViewTemplate (page : Page) =
  sprintf """
let get_edit_%s =
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
    scripts.common""" page.AsVal page.Name page.Name (formatEditFields page page.Fields 5)

let submitFormViewTemplate (page : Page) =
  sprintf """
let get_submit_%s =
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
    scripts.common""" page.AsVal page.Name page.Name (formatSubmitFields page.Fields 5)

let submitErroredFormViewTemplate (page : Page) =
  sprintf """
let post_submit_errored_%s errors (%s : %s) =
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
    scripts.common""" page.AsVal page.AsFormVal page.AsFormType page.Name page.Name (formatSubmitErroredFields page page.Fields 5)

let viewTemplate site page =
  let rec viewTemplate site page pageMode =
    match pageMode with
    | CVEL      -> [viewTemplate site page Edit] |> flatten
    | Edit      -> editFormViewTemplate page
    | View      -> failwith "not done"
    | List      -> failwith "not done"
    | Jumbotron -> bannerViewTemplate site page
    | Create
    | Submit    -> [submitFormViewTemplate page] @ [submitErroredFormViewTemplate page] |> flatten

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
    | Jumbotron -> template page.AsViewHref page.Name

  pageLinkTemplate page page.PageMode

let pathTemplate page =
  let rec pathTemplate page pageMode =
    match pageMode with
    | CVEL      -> [Create; View; Edit; List] |> List.map (pathTemplate page) |> flatten
    | Create    -> sprintf """let path_create_%s = "%s" """ page.AsVal page.AsCreateHref
    | Edit      -> sprintf """let path_edit_%s = "%s" """ page.AsVal page.AsEditHref
    | View      -> sprintf """let path_%s = "%s" """ page.AsVal page.AsViewHref //add id
    | List      -> sprintf """let path_list_%s = "%s" """ page.AsVal page.AsListHref //add s for cheap pural
    | Submit    -> sprintf """let path_submit_%s = "%s" """ page.AsVal page.AsCreateHref //add id
    | Jumbotron -> sprintf """let path_%s = "%s" """ page.AsVal page.AsViewHref //add id

  pathTemplate page page.PageMode

let routeTemplate page =
  let rec routeTemplate page pageMode =
    match pageMode with
    | CVEL      -> [Create; View; Edit; List] |> List.map (routeTemplate page) |> flatten
    | Create    -> sprintf """path path_create_%s >=> create_%s""" page.AsVal page.AsVal |> pad 2
    | Edit      -> sprintf """path path_edit_%s >=> edit_%s""" page.AsVal page.AsVal |> pad 2
    | View      -> sprintf """path path_%s >=> %s""" page.AsVal page.AsVal |> pad 2
    | List      -> sprintf """path path_list_%s >=> list_%s""" page.AsVal page.AsVal |> pad 2
    | Submit    -> sprintf """path path_submit_%s >=> submit_%s""" page.AsVal page.AsVal |> pad 2
    | Jumbotron -> sprintf """path path_%s >=> %s""" page.AsVal page.AsVal |> pad 2

  routeTemplate page page.PageMode

let handlerTemplate page =
  let rec handlerTemplate page pageMode =
    match pageMode with
    | CVEL -> [Create; View; Edit; List] |> List.map (handlerTemplate page) |> flatten
    | Edit      ->
      sprintf """let edit_%s =
    choose
      [
        GET >=> OK get_edit_%s
        POST >=> bindToForm %s (fun %s ->
          let validation = validate%s %s
          if validation = [] then
            let form = convert%s %s
            let message = sprintf "form: %s" form
            OK message
          else
            OK "") //(post_submit_errored_%s validation %s))
      ]""" page.AsVal page.AsVal page.AsFormVal page.AsFormVal page.AsFormType page.AsFormVal page.AsFormType page.AsFormVal "%A" page.AsVal page.AsFormVal
    | View      -> sprintf """let %s = GET >=> OK "todo" """ page.AsVal
    | List      -> sprintf """let list_%s = GET >=> OK "todo" """ page.AsVal
    | Jumbotron ->
      sprintf """let %s = GET >=> OK get_%s""" page.AsVal page.AsVal
    | Create    -> sprintf """let create_%s = GET >=> OK "todo" """ page.AsVal
    | Submit    ->
      sprintf """let submit_%s =
    choose
      [
        GET >=> OK get_submit_%s
        POST >=> bindToForm %s (fun %s ->
          let validation = validate%s %s
          if validation = [] then
            let form = convert%s %s
            let message = sprintf "form: %s" form
            OK message
          else
            OK (post_submit_errored_%s validation %s))
      ]""" page.AsVal page.AsVal page.AsFormVal page.AsFormVal page.AsFormType page.AsFormVal page.AsFormType page.AsFormVal "%A" page.AsVal page.AsFormVal

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

  write (destination "generated_html") generated_html_result
  write (destination "generated_views") generated_views_result
  write (destination "generated_handlers") generated_handlers_result
  write (destination "generated_forms") generated_forms_result
  write (destination "generated_types") generated_types_result
  write (destination "generated_paths") generated_paths_result
  write (destination "generated_validation") generated_validation_result
  write (destination "generated_unittests") generated_unittests_result
  write (destination "generated_uitests") generated_uitests_result
