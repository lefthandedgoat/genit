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

let fieldToHtml (field : Field) =
  let template tag = sprintf """%s "%s" "" """ tag field.Name
  let iconTemplate tag icon = sprintf """%s "%s" "" "%s" """ tag field.Name icon
  match field.FieldType with
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
  let template tag = sprintf """%s "%s" "%s.%s" """ tag field.Name page.AsFormVal field.AsProperty
  let iconTemplate tag icon = sprintf """%s "%s" "%s.%s" "%s" """ tag field.Name page.AsFormVal field.AsProperty icon
  match field.FieldType with
  | Text       -> template "label_text"
  | Paragraph  -> template "label_textarea"
  | Number     -> template "label_text"
  | Decimal    -> template "label_text"
  | Date       -> template "label_text"
  | Phone      -> template "label_text"
  | Email      -> iconTemplate "icon_label_text" "envelope"
  | Name       -> iconTemplate "icon_label_text" "user"
  | Password   -> iconTemplate "icon_password_text" "lock"
  | Dropdown options -> sprintf """label_select_selected "%s" %A (Some "%s.%s")""" field.Name (zipOptions options) page.AsFormVal field.AsProperty

let fieldToErroredHtml page (field : Field) =
  let template tag = sprintf """%s "%s" %s.%s errors""" tag field.Name page.AsFormVal field.AsProperty
  let iconTemplate tag icon = sprintf """%s "%s" %s.%s "%s" errors""" tag field.Name page.AsFormVal field.AsProperty icon
  match field.FieldType with
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
  | Text       -> "string"
  | Paragraph  -> "string"
  | Number     -> "int"
  | Decimal    -> "double"
  | Date       -> "System.DateTime"
  | Phone      -> "string"
  | Email      -> "string"
  | Name       -> "string"
  | Password   -> "string"
  | Dropdown _ -> "int"

let fieldToConvertProperty page field =
  let property = sprintf "%s.%s" page.AsFormVal field.AsProperty
  let string () = sprintf """%s = %s""" field.AsProperty property
  let int () = sprintf """%s = int %s""" field.AsProperty property
  let double () = sprintf """%s = double %s""" field.AsProperty property
  let datetime () = sprintf """%s = System.DateTime.Parse(%s)""" field.AsProperty property
  match field.FieldType with
  | Text       -> string ()
  | Paragraph  -> string ()
  | Number     -> int ()
  | Decimal    -> double ()
  | Date       -> datetime ()
  | Email      -> string ()
  | Name       -> string ()
  | Phone      -> string ()
  | Password   -> string ()
  | Dropdown _ -> int ()

let fieldToValidation (field : Field) page =
  let template validation = sprintf """%s "%s" %s.%s""" validation field.Name page.AsFormVal field.AsProperty
  match field.FieldType with
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
      common_submit_form
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
      common_submit_form
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
  let template extra href = sprintf """let path_%s%s = "%s" """ extra page.AsVal href
  let rec pathTemplate page pageMode =
    match pageMode with
    | CVEL      -> [Create; View; Edit; List] |> List.map (pathTemplate page) |> flatten
    | Create    -> template "create_" page.AsCreateHref
    | Edit      -> template "edit_" page.AsEditHref
    | View      -> template "" page.AsViewHref //add id
    | List      -> template "list_" page.AsListHref
    | Submit    -> template "submit_" page.AsCreateHref
    | Jumbotron -> template "" page.AsViewHref

  pathTemplate page page.PageMode

let routeTemplate page =
  let template extra = sprintf """path path_%s%s >=> %s%s""" extra page.AsVal extra page.AsVal |> pad 2
  let rec routeTemplate page pageMode =
    match pageMode with
    | CVEL      -> [Create; View; Edit; List] |> List.map (routeTemplate page) |> flatten
    | Create    -> template "create_"
    | Edit      -> template "edit_"
    | View      -> template ""
    | List      -> template "list_"
    | Submit    -> template "submit_"
    | Jumbotron -> template ""

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
