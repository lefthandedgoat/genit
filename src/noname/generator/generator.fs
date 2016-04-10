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

let lower (value : string) = value.ToLower()
let upperFirst (value : string) = Char.ToUpper(value.[0]).ToString() + value.Substring(1)
let lowerFirst (value : string) = Char.ToLower(value.[0]).ToString() + value.Substring(1)
let spaceToUnderscore (value : string) = value.Replace(" ", "_")
let spaceToNothing (value : string) = value.Replace(" ", "")
let format = lower >> spaceToUnderscore
let typeFormat = spaceToNothing
let camelCase = spaceToNothing >> lowerFirst
let repeat (value : string) times = [1..times] |> List.map (fun _ -> value) |> List.reduce (+)
let flatten values = values |> List.reduce (fun value1 value2 -> sprintf "%s%s%s" value1 Environment.NewLine value2)


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

let fieldToValidation (field : Field) formName =
  let property = sprintf "%sForm.%s" formName (typeFormat field.Name |> upperFirst)
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

let attributeToValidation (field : Field) formName =
  let property = sprintf "%sForm.%s" formName (typeFormat field.Name |> upperFirst)
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
    scripts.common""" (format page.Name) page.Name page.Name (formatFields page.Fields 5)

let pageLinkTemplate (page : Page) = sprintf """li [ aHref "/%s" [text "%s"] ]""" (format page.Name) page.Name |> pad 7

let pathTemplate (page : Page) = sprintf """let path_%s = "/%s" """ (format page.Name) (format page.Name)

let routeTemplate (page : Page) = sprintf """path path_%s >=> %s""" (format page.Name) (camelCase page.Name) |> pad 2

let handlerTemplate (page : Page) =
  let typeName = typeFormat page.Name |> upperFirst
  let pageName = format page.Name
  let camelName= camelCase page.Name
  let formName = sprintf "%sForm" (camelCase page.Name)
  let convertName = sprintf "convert%s" (upperFirst formName)
  match page.PageMode with
  | Edit -> failwith "not done"
  | View -> failwith "not done"
  | List -> failwith "not done"
  | Create | Submit ->
    sprintf """let %s =
  choose
    [
      GET >=> OK get_%s
      POST >=> bindToForm %s (fun %s ->
        let validation = validate%sForm %s
        let form = %s %s
        let message = "Parsed form: \r\n" + (form.ToString())
        OK message)
    ]""" camelName pageName formName formName typeName formName convertName formName

let propertyTemplate (page : Page) =
  page.Fields
  |> List.map (fun field -> sprintf """%s : %s""" (spaceToNothing field.Name) (fieldToProperty field.FieldType))
  |> List.map (pad 2)
  |> flatten

let formPropertyTemplate (page : Page) =
  let formName = sprintf "%sForm" (camelCase page.Name)
  page.Fields
  |> List.map (fun field -> sprintf """%s = %s.%s""" (spaceToNothing field.Name) formName (spaceToNothing field.Name))
  |> List.map (pad 2)
  |> flatten

let converterPropertyTemplate (page : Page) =
  page.Fields
  |> List.map (fun field -> sprintf """%s : %s""" (spaceToNothing field.Name) (fieldToProperty field.FieldType))
  |> List.map (pad 2)
  |> flatten

let typeTemplate (page : Page) =
  let typeName = typeFormat page.Name |> upperFirst
  sprintf """type %s =
  {
%s
  }
  """ typeName (propertyTemplate page)

let converterTemplate (page : Page) =
  let typeName = typeFormat page.Name |> upperFirst
  let formName = camelCase page.Name
  sprintf """let convert%sForm (%sForm : %sForm) =
  {
%s
  }
  """ typeName formName typeName (formPropertyTemplate page)

let formTypeTemplate (page : Page) =
  let typeName = typeFormat page.Name |> upperFirst
  let formName = camelCase page.Name
  sprintf """type %sForm =
  {
%s
  }

let %sForm : Form<%sForm> = Form ([],[])

%s
  """ typeName (propertyTemplate page) formName typeName (converterTemplate page)

let validationTemplate (page : Page) =
  let typeName = typeFormat page.Name |> upperFirst
  let formName = camelCase page.Name
  let validations =
    page.Fields
    |> List.map (fun field -> [fieldToValidation field formName] @ [attributeToValidation field formName])
    |> List.concat
    |> List.choose id
    |> List.map (pad 2)
    |> flatten

  sprintf """let validate%sForm (%sForm : %sForm) =
  [
%s
  ]
  """ typeName formName typeName validations

let generate (site : Site) =
  let html_results = site.Pages |> List.map pageLinkTemplate |> flatten
  let generated_html_result = generated_html_template html_results

  let views_results = site.Pages |> List.map formViewTemplate |> flatten
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
