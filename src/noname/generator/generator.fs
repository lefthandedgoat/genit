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
  | Dropdown _ -> failwith "not done"

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
  | Dropdown _ -> failwith "not done"

let fieldToProperty field =
  match field with
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

let fieldToValidation (field : Field) (page : Page) =
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
  | Dropdown _ -> failwith "not done"

let fieldToTestName (field : Field) =
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
  | Dropdown _ -> failwith "not done"

let fieldToTestBody (field : Field) =
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
  | Dropdown _ -> failwith "not done"

let attributeToValidation (field : Field) (page : Page) =
  let property = sprintf "%s.%s" page.AsFormVal field.AsProperty
  match field.Attribute with
  | Id         -> None
  | Null       -> None
  | Required   -> Some (sprintf """validate_required "%s" %s""" field.Name property)
  | Min(min)   -> Some (sprintf """validate_min "%s" %s %i""" field.Name property min)
  | Max(max)   -> Some (sprintf """validate_max "%s" %s %i""" field.Name property max)
  | Range(min,max) -> Some (sprintf """validate_range "%s" %s %i %i""" field.Name property min max)

let attributeToTestName (field : Field) =
  match field.Attribute with
  | Id         -> None
  | Null       -> None
  | Required   -> Some (sprintf """"%s is required" """ field.Name)
  | Min(min)   -> Some (sprintf """"%s must be greater than %i" """ field.Name min)
  | Max(max)   -> Some (sprintf """"%s must be less than %i" """ field.Name max)
  | Range(min,max) -> Some (sprintf """"%s must be between %i and %i" """ field.Name min max)

let attributeToTestBody (field : Field) =
  match field.Attribute with
  | Id         -> None
  | Null       -> None
  | Required   -> Some (sprintf """displayed "%s is required" """ field.Name)
  | Min(min)   -> Some (sprintf """displayed "%s can not be below %i" """ field.Name min)
  | Max(max)   -> Some (sprintf """displayed "%s can not be above %i" """ field.Name max)
  | Range(min,max) -> Some (sprintf """displayed "%s must be between %i and %i" """ field.Name min max)

let pad tabs field = sprintf "%s%s" (repeat "  " tabs) field

let formatFields (fields : Field list) tabs =
  fields
  |> List.map fieldToHtml
  |> List.map (pad tabs)
  |> List.reduce (fun field1 field2 -> sprintf "%s%s%s" field1 Environment.NewLine field2)

let formatErroredFields page (fields : Field list) tabs =
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

let formViewTemplate (page : Page) =
  sprintf """
let get_%s =
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
    scripts.common""" page.AsVal page.Name page.Name (formatFields page.Fields 5)

let erroredFormViewTemplate (page : Page) =
  sprintf """
let post_errored_%s errors (%s : %s) =
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

let viewTemplate site page =
  match page.PageMode with
  | Edit      -> failwith "not done"
  | View      -> failwith "not done"
  | List      -> failwith "not done"
  | Jumbotron -> bannerViewTemplate site page
  | Create
  | Submit    -> [formViewTemplate page] @ [erroredFormViewTemplate page] |> flatten

let pageLinkTemplate (page : Page) = sprintf """li [ aHref "%s" [text "%s"] ]""" page.AsHref page.Name |> pad 7

let pathTemplate (page : Page) = sprintf """let path_%s = "%s" """ page.AsVal page.AsHref

let routeTemplate (page : Page) = sprintf """path path_%s >=> %s""" page.AsVal page.AsVal |> pad 2

let handlerTemplate (page : Page) =
  match page.PageMode with
  | Edit      -> failwith "not done"
  | View      -> failwith "not done"
  | List      -> failwith "not done"
  | Jumbotron ->
    sprintf """let %s = GET >=> OK get_%s""" page.AsVal page.AsVal
  | Create
  | Submit    ->
    sprintf """let %s =
  choose
    [
      GET >=> OK get_%s
      POST >=> bindToForm %s (fun %s ->
        let validation = validate%s %s
        if validation = [] then
          let form = convert%s %s
          let message = sprintf "form: %s" form
          OK message
        else
          OK (post_errored_%s validation %s))
    ]""" page.AsVal page.AsVal page.AsFormVal page.AsFormVal page.AsFormType page.AsFormVal page.AsFormType page.AsFormVal "%A" page.AsVal page.AsFormVal

let propertyTemplate (page : Page) =
  page.Fields
  |> List.map (fun field -> sprintf """%s : %s""" field.AsProperty (fieldToProperty field.FieldType))
  |> List.map (pad 2)
  |> flatten

let formPropertyTemplate (page : Page) =
  page.Fields
  |> List.map (fun field -> sprintf """%s : string""" field.AsProperty)
  |> List.map (pad 2)
  |> flatten

let converterPropertyTemplate (page : Page) =
  page.Fields
  |> List.map (fun field -> sprintf """%s = %s.%s""" field.AsProperty page.AsFormVal field.AsProperty)
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

let beforeTemplate (page : Page) =
  sprintf """before (fun _ -> url "http://localhost:8083%s"; click ".btn") """ page.AsHref
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
    """ (contextTemplate page) (beforeTemplate page) tests

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
