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
  | Text -> sprintf """label_text "%s" "" """ field.Name
  | Paragraph -> failwith "not done"
  | Number -> failwith "not done"
  | Decimal -> failwith "not done"
  | Date -> failwith "not done"
  | Email -> sprintf """icon_label_text "%s" "" "envelope" """ field.Name
  | Name -> sprintf """icon_label_text "%s" "" "user" """ field.Name
  | Phone -> failwith "not done"
  | Password -> sprintf """icon_password_text "%s" "" "lock" """ field.Name
  | Dropdown _ -> failwith "not done"

let fieldToErroredHtml page field =
  match field.FieldType with
  | Text -> sprintf """errored_label_text "%s" %s.%s errors""" field.Name page.AsFormVal field.AsProperty
  | Paragraph -> failwith "not done"
  | Number -> failwith "not done"
  | Decimal -> failwith "not done"
  | Date -> failwith "not done"
  | Email -> sprintf """errored_icon_label_text "%s" %s.%s "envelope" errors""" field.Name page.AsFormVal field.AsProperty
  | Name -> sprintf """errored_icon_label_text "%s" %s.%s "user" errors""" field.Name page.AsFormVal field.AsProperty
  | Phone -> failwith "not done"
  | Password -> sprintf """errored_icon_password_text "%s" %s.%s "lock" errors""" field.Name page.AsFormVal field.AsProperty
  | Dropdown _ -> failwith "not done"

let fieldToProperty field =
  match field with
  | Text       -> "string"
  | Paragraph  -> "string"
  | Number     -> "int"
  | Decimal    -> "float"
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
  | Number     -> None //parseInt
  | Decimal    -> None //parseDecimal
  | Date       -> None //parseDate
  | Email      -> Some (sprintf """email "%s" %s""" field.Name property)
  | Name       -> None
  | Phone      -> None //parsePhone?
  | Password   -> Some (sprintf """password "%s" %s""" field.Name property)
  | Dropdown _ -> None

let attributeToValidation (field : Field) (page : Page) =
  let property = sprintf "%s.%s" page.AsFormVal field.AsProperty
  match field.Attribute with
  | Id         -> None
  | Null       -> None
  | NotNull    -> None
  | Required   -> Some (sprintf """required "%s" %s""" field.Name property)
  | Min(_)     -> None
  | Max(_)     -> None
  | Range(_,_) -> None

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
  |> List.map (fun field -> sprintf """%s = %s.%s""" field.AsProperty page.AsFormVal field.AsProperty)
  |> List.map (pad 2)
  |> flatten

let converterPropertyTemplate (page : Page) =
  page.Fields
  |> List.map (fun field -> sprintf """%s : %s""" field.AsProperty (fieldToProperty field.FieldType))
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
    sprintf """let convert%s (%s : %s) =
  {
%s
  }
  """ page.AsFormType page.AsFormVal page.AsFormType (formPropertyTemplate page)

let formTypeTemplate (page : Page) =
  if page.Fields = [] then ""
  else
    sprintf """type %s =
  {
%s
  }

let %s : Form<%s> = Form ([],[])

%s
  """ page.AsFormType (propertyTemplate page) page.AsFormVal page.AsFormType (converterTemplate page)

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

  write (destination "generated_html") generated_html_result
  write (destination "generated_views") generated_views_result
  write (destination "generated_handlers") generated_handlers_result
  write (destination "generated_forms") generated_forms_result
  write (destination "generated_types") generated_types_result
  write (destination "generated_paths") generated_paths_result
  write (destination "generated_validation") generated_validation_result
