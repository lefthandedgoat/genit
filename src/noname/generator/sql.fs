module sql

open dsl

let createTemplate dbname =
  sprintf """
DROP DATABASE IF EXISTS %s;
CREATE DATABASE %s;""" dbname dbname

let initialSetupTemplate (dbname : string) = System.String.Format("""
DROP OWNED BY {0};
DROP USER IF EXISTS {0};

DROP SCHEMA IF EXISTS {0};
CREATE SCHEMA {0};

CREATE USER {0} WITH ENCRYPTED PASSWORD 'secure123';
GRANT USAGE ON SCHEMA {0} to {0};
ALTER DEFAULT PRIVILEGES IN SCHEMA {0} GRANT SELECT ON TABLES TO {0};
GRANT CONNECT ON DATABASE "{0}" to {0};""", dbname)

let columnTypeTemplate field =
  match field.FieldType with
  | Id           -> "SERIAL"
  | Text         -> "varchar(1024)"
  | Paragraph    -> "text"
  | Number       -> "integer"
  | Decimal      -> "decimal(12, 2)"
  | Date         -> "timestamptz"
  | Phone        -> "varchar(15)"
  | Email        -> "varchar(128)"
  | Name         -> "varchar(128)"
  | Password     -> "varchar(60)"
  | Dropdown (_) -> "smallint"

//http://www.postgresql.org/docs/9.5/static/ddl-constraints.html
let columnAttributesTemplate field =
  match field.Attribute with
  | PK              -> "PRIMARY KEY NOT NULL"
  | Null            -> "NULL"
  | Required        -> "NOT NULL"
  | Min(min)        -> sprintf "CHECK (%s > %i)" field.AsDBColumn min
  | Max(max)        -> sprintf "CHECK (%s < %i)" field.AsDBColumn max
  | Range(min, max) -> sprintf "CHECK (%i < %s < %i)" min field.AsDBColumn max

let columnTemplate namePad typePad (field : Field) =
 sprintf "%s %s %s" (rightPad namePad field.AsDBColumn) (rightPad typePad (columnTypeTemplate field)) (columnAttributesTemplate field)

let createColumns (page : Page) =
  let maxName = page.Fields |> List.map (fun field -> field.AsDBColumn.Length) |> List.max
  let maxName = if maxName > 20 then maxName else 20
  let maxType = page.Fields |> List.map (fun field -> (columnTypeTemplate field).Length) |> List.max
  let maxType = if maxType > 20 then maxType else 20

  page.Fields
  |> List.map (columnTemplate maxName maxType)
  |> List.map (pad 1)
  |> flattenWith ","

let createTableTemplate (dbname : string) (page : Page) =
  let columns = createColumns page
  sprintf """
CREATE TABLE %s.%s(
%s
);
  """ dbname page.AsTableName columns

let shouldICreateTable page =
  match page.PageMode with
  | CVEL
  | Create
  | Edit
  | View
  | List
  | Submit    -> true
  | Jumbotron -> false

let createTableTemplates (site : Site) =
  site.Pages
  |> List.filter (fun page -> shouldICreateTable page)
  |> List.filter (fun page -> page.CreateTable = CreateTable)
  |> List.map (createTableTemplate site.AsDatabase)
  |> flatten

let conversionTemplate field =
  match field.FieldType with
  | Id           -> "getInt64"
  | Text         -> "getString"
  | Paragraph    -> "getString"
  | Number       -> "getInt32"
  | Decimal      -> "getDouble"
  | Date         -> "getDateTime"
  | Phone        -> "getString"
  | Email        -> "getString"
  | Name         -> "getString"
  | Password     -> "getString"
  | Dropdown (_) -> "getInt16"

let dataReaderPropertyTemplate field =
 sprintf """%s = %s "%s" reader""" field.AsProperty (conversionTemplate field) field.AsDBColumn

let dataReaderPropertiesTemplate page =
  page.Fields
  |> List.map (fun field -> dataReaderPropertyTemplate field)
  |> List.map (pad 3)
  |> flatten

let dataReaderTemplate page =
  sprintf """let to%s (reader : NpgsqlDataReader) : %s list =
  [ while reader.Read() do
    yield {
%s
    }
  ]
  """ page.AsType page.AsType (dataReaderPropertiesTemplate page)

let insertTemplate site page =
  ""

let updateTemplate site page =
  ""

let tryByIdTemplate site page =
  let idField = page.Fields |> List.find (fun field -> field.FieldType = Id)
  sprintf """
let tryById_%s id =
  let sql = "
SELECT * FROM %s.%s
WHERE %s = :%s
"
  use connection = connection connectionString
  use command = command connection sql
  command
  |> param "%s" id
  |> read to%s
  |> firstOrNone""" page.AsVal site.AsDatabase page.AsTableName idField.AsDBColumn idField.AsDBColumn idField.AsDBColumn page.AsType

let selectManyTemplate site page =
  ""

let createQueriesForPage site page =
  let rec createQueriesForPage pageMode =
    match pageMode with
    | CVEL      -> [Create; View; Edit; List] |> List.map createQueriesForPage |> flatten
    | Create    -> insertTemplate site page
    | Edit      -> updateTemplate site page
    | View      -> tryByIdTemplate site page
    | List      -> selectManyTemplate site page
    | Submit    -> insertTemplate site page
    | Jumbotron -> ""

  let queries = createQueriesForPage page.PageMode
  if page.PageMode = CVEL || page.PageMode = View || page.PageMode = List
  then sprintf "%s%s%s" (dataReaderTemplate page) System.Environment.NewLine queries
  else queries

let createQueries (site : Site) =
  site.Pages
  |> List.map (createQueriesForPage site)
  |> flatten
