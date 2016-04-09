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
let spaceToUnderscore (value : string) = value.Replace(" ", "_")
let format = lower >> spaceToUnderscore
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
  | Phone -> failwith "not done"
  | Password -> sprintf """icon_password_text "%s" "" "lock" """ field.Name
  | Dropdown options -> failwith "not done"

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

let pathTemplate (page : Page) = sprintf """let path_%s = "/%s" """ (format page.Name) (format page.Name)

let routeTemplate (page : Page) = sprintf """path path_%s >=> %s""" (format page.Name) (format page.Name) |> pad 2

let handlerTemplate (page : Page) = sprintf """let %s = choose [ GET >=> OK get_%s]""" (format page.Name) (format page.Name)

let generate (site : Site) =
  let views_results = site.Pages |> List.map formViewTemplate |> flatten
  let generated_views_result = generated_views_template site.Name views_results

  let handler_results = site.Pages |> List.map handlerTemplate |> flatten
  let generated_handlers_result = generated_handlers_template handler_results

  let paths_results = site.Pages |> List.map pathTemplate |> flatten
  let routes_results = site.Pages |> List.map routeTemplate |> flatten
  let generated_paths_result = generated_paths_template paths_results routes_results

  write (destination "generated_views") generated_views_result
  write (destination "generated_handlers") generated_handlers_result
  write (destination "generated_paths") generated_paths_result
