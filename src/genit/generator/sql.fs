module sql

open dsl
open helper_general

let createTemplate dbname database =
  match database with
  | Postgres  -> psql.createTemplate dbname
  | SQLServer -> mssql.createTemplate dbname

let initialSetupTemplate site =
  match site.Database with
  | Postgres  -> psql.initialSetupTemplate site
  | SQLServer -> mssql.initialSetupTemplate site

(*

CREATE TABLES

*)

let columnTypeTemplate site field =
  match site.Database with
  | Postgres  -> psql.columnTypeTemplate field
  | SQLServer -> mssql.columnTypeTemplate field

//http://www.postgresql.org/docs/9.5/static/ddl-constraints.html
let columnAttributesTemplate site field =
  match site.Database with
  | Postgres  -> psql.columnAttributesTemplate field
  | SQLServer -> mssql.columnAttributesTemplate field

let columnTemplate site namePad typePad field =
 sprintf "%s %s %s" (rightPad namePad field.AsDBColumn) (rightPad typePad (columnTypeTemplate site field)) (columnAttributesTemplate site field)

let createColumns site page =
  let maxName = page.Fields |> List.map (fun field -> field.AsDBColumn.Length) |> List.max
  let maxName = if maxName > 25 then maxName else 25
  let maxType = page.Fields |> List.map (fun field -> (columnTypeTemplate site field).Length) |> List.max
  let maxType = if maxType > 25 then maxType else 25

  page.Fields
  |> List.filter (fun field -> field.FieldType <> ConfirmPassword)
  |> List.map (columnTemplate site maxName maxType)
  |> List.map (pad 1)
  |> flattenWith ","

let createTableTemplate site page =
  let columns = createColumns site page
  match site.Database with
  | Postgres  -> psql.createTableTemplate site.AsDatabase page columns
  | SQLServer -> mssql.createTableTemplate site.AsDatabase page columns

let shouldICreateTable page =
  match page.PageMode with
  | CVELS
  | CVEL
  | Create
  | Edit
  | View
  | List
  | Register    -> true
  | Login       -> false
  | Search      -> true
  | Jumbotron   -> false

let createTableTemplates site =
  site.Pages
  |> List.filter shouldICreateTable
  |> List.filter (fun page -> page.CreateTable = CreateTable)
  |> List.map (createTableTemplate site)
  |> flatten

let grantPrivileges site =
  match site.Database with
  | Postgres  -> psql.grantPrivileges site
  | SQLServer -> mssql.grantPrivileges site

let createTables guts1 guts2 =
  sprintf """
%s

%s
  """ guts1 guts2

(*

DATA READERS

*)

let conversionTemplate field =
  let result =
    match field.FieldType with
    | Id              -> "getInt64"
    | Text            -> "getString"
    | Paragraph       -> "getString"
    | Number          -> "getInt32"
    | Decimal         -> "getDouble"
    | Date            -> "getDateTime"
    | Phone           -> "getString"
    | Email           -> "getString"
    | Name            -> "getString"
    | Password        -> "getString"
    | ConfirmPassword -> ""
    | Dropdown (_)    -> "getInt16"
  if field.Attribute = Null && useSome field
  then result + "Option"
  else result

let dataReaderPropertyTemplate field =
 sprintf """%s = %s "%s" reader""" field.AsProperty (conversionTemplate field) field.AsDBColumn

let dataReaderPropertiesTemplate page =
  page.Fields
  |> List.filter (fun field -> field.FieldType <> ConfirmPassword)
  |> List.map (fun field -> dataReaderPropertyTemplate field)
  |> List.map (pad 3)
  |> flatten

let dataReaderTemplate page =
  sprintf """let to%s (reader : IDataReader) : %s list =
  [ while reader.Read() do
    yield {
%s
    }
  ]
  """ page.AsType page.AsType (dataReaderPropertiesTemplate page)

(*

INSERT

*)

let insertTemplate site page =
  match site.Database with
  | Postgres  -> psql.insertTemplate site page
  | SQLServer -> mssql.insertTemplate site page

(*

UPDATE

*)

let updateTemplate site page =
  match site.Database with
  | Postgres  -> psql.updateTemplate site page
  | SQLServer -> mssql.updateTemplate site page

(*

SELECT

*)

let tryByIdTemplate site page =
  match site.Database with
  | Postgres  -> psql.tryByIdTemplate site page
  | SQLServer -> mssql.tryByIdTemplate site page

let selectManyTemplate site page =
  match site.Database with
  | Postgres  -> psql.selectManyTemplate site page
  | SQLServer -> mssql.selectManyTemplate site page

let selectManyWhereTemplate site page =
  match site.Database with
  | Postgres  -> psql.selectManyWhereTemplate site page
  | SQLServer -> mssql.selectManyWhereTemplate site page

(*

Authentication

*)

let authenticateTemplate site page =
  match site.Database with
  | Postgres  -> psql.authenticateTemplate site page
  | SQLServer -> mssql.authenticateTemplate site page

(*

Everything else

*)

let createQueriesForPage site page =
  let rec createQueriesForPage pageMode =
    match pageMode with
    | CVELS     -> [Create; Edit; List; Search] |> List.map createQueriesForPage |> flatten
    | CVEL      -> [Create; Edit; List] |> List.map createQueriesForPage |> flatten
    | Create    -> insertTemplate site page
    | Edit      -> [updateTemplate site page; tryByIdTemplate site page] |> flatten
    | View      -> tryByIdTemplate site page
    | List      -> selectManyTemplate site page
    | Search    -> selectManyWhereTemplate site page
    | Register  -> insertTemplate site page
    | Login     -> authenticateTemplate site page
    | Jumbotron -> ""

  let queries = createQueriesForPage page.PageMode
  if needsDataReader page
  then sprintf "%s%s%s" (dataReaderTemplate page) System.Environment.NewLine queries
  else queries

let createQueries site =
  site.Pages
  |> List.map (createQueriesForPage site)
  |> flatten

let fieldToProperty field =
  let result =
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
  if field.Attribute = Null && useSome field
  then result + " option"
  else result

let fieldLine (field : Field ) =
  sprintf """%s : %s""" field.AsProperty (fieldToProperty field)

let fieldToConvertProperty page field =
  let property = sprintf "%s.%s" page.AsFormVal field.AsProperty
  let string () = sprintf """%s = %s""" field.AsProperty property
  let int () =
    if field.Attribute = Null
    then sprintf """%s = Some(int %s)""" field.AsProperty property
    else sprintf """%s = int %s""" field.AsProperty property
  let int16 () =
    if field.Attribute = Null
    then sprintf """%s = Some(int16 %s)""" field.AsProperty property
    else sprintf """%s = int16 %s""" field.AsProperty property
  let int64 () =
    if field.Attribute = Null
    then sprintf """%s = Some(int64 %s)""" field.AsProperty property
    else sprintf """%s = int64 %s""" field.AsProperty property
  let decimal () =
    if field.Attribute = Null
    then sprintf """%s = Some(double %s)""" field.AsProperty property
    else sprintf """%s = double %s""" field.AsProperty property
  let datetime () =
    if field.Attribute = Null
    then sprintf """%s = Some(System.DateTime.Parse(%s))""" field.AsProperty property
    else sprintf """%s = System.DateTime.Parse(%s)""" field.AsProperty property
  match field.FieldType with
  | Id              -> int64 ()
  | Text            -> string ()
  | Paragraph       -> string ()
  | Number          -> int ()
  | Decimal         -> decimal ()
  | Date            -> datetime ()
  | Email           -> string ()
  | Name            -> string ()
  | Phone           -> string ()
  | Password        -> string ()
  | ConfirmPassword -> string ()
  | Dropdown _      -> int16 ()

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
    | Date            -> "System.DateTime.Now.AddDays(random.Next(7) |> float)"
    | Phone           -> """sprintf "%i-%i-%i" (random.Next(200,800)) (random.Next(200,800)) (random.Next(2000,8000))"""
    | Email           -> """sprintf "%s@%s.com" (randomItem words) (randomItem words)"""
    | Name            -> pickAppropriateName """randomItem names"""
    | Password        -> """"123123" """ |> trimEnd
    | ConfirmPassword -> """"123123" """ |> trimEnd
    | Dropdown _      -> "1s"
  if field.Attribute = Null && useSome field
  then sprintf """%s = Some(%s) """ field.AsProperty value
  else sprintf """%s = %s """ field.AsProperty value

let createConnectionString site =
  match site.Database with
  | Postgres  -> psql.createConnectionString site
  | SQLServer -> mssql.createConnectionString site
