module sql

open dsl
open helper_general

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

(*

CREATE TABLES

*)

let columnTypeTemplate field =
  match field.FieldType with
  | Id              -> "SERIAL"
  | Text            -> "varchar(1024)"
  | Paragraph       -> "text"
  | Number          -> "integer"
  | Decimal         -> "decimal(12, 2)"
  | Date            -> "timestamptz"
  | Phone           -> "varchar(15)"
  | Email           -> "varchar(128)"
  | Name            -> "varchar(128)"
  | Password        -> "varchar(60)"
  | ConfirmPassword -> ""
  | Dropdown (_)    -> "smallint"

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
  |> List.filter (fun field -> field.FieldType <> ConfirmPassword)
  |> List.map (columnTemplate maxName maxType)
  |> List.map (pad 1)
  |> flattenWith ","

let createTableTemplate (dbname : string) (page : Page) =
  let columns = createColumns page
  sprintf """
CREATE TABLE %s.%s(
%s
);
  """ dbname page.AsTable columns

let shouldICreateTable page =
  match page.PageMode with
  | CVELS
  | CVEL
  | Create
  | Edit
  | View
  | List
  | Register    -> true
  | Login
  | Search
  | Jumbotron -> false

let createTableTemplates (site : Site) =
  site.Pages
  |> List.filter (fun page -> shouldICreateTable page)
  |> List.filter (fun page -> page.CreateTable = CreateTable)
  |> List.map (createTableTemplate site.AsDatabase)
  |> flatten

let grantPrivileges (site : Site) =
  sprintf """
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA %s TO %s;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA %s TO %s;
  """ site.AsDatabase site.AsDatabase site.AsDatabase site.AsDatabase

let createTables guts1 guts2 =
  sprintf """
%s

%s
  """ guts1 guts2

(*

DATA READERS

*)

let conversionTemplate field =
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

let dataReaderPropertyTemplate field =
 sprintf """%s = %s "%s" reader""" field.AsProperty (conversionTemplate field) field.AsDBColumn

let dataReaderPropertiesTemplate page =
  page.Fields
  |> List.filter (fun field -> field.FieldType <> ConfirmPassword)
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

(*

INSERT

*)

let insertColumns page =
  page.Fields
  |> List.filter (fun field -> field.FieldType <> ConfirmPassword)
  |> List.map (fun field -> field.AsDBColumn)
  |> List.map (pad 2)
  |> flattenWith ","

let passwordTemplate page =
  let password = page.Fields |> List.tryFind (fun field -> field.FieldType = Password)
  match password with
  | Some(password) ->
    sprintf """
  let bCryptScheme = getBCryptScheme currentBCryptScheme
  let salt = BCrypt.GenerateSalt(bCryptScheme.WorkFactor)
  let password = BCrypt.HashPassword(%s.%s, salt)
    """ page.AsVal password.AsProperty
  | None -> ""

let insertValues page =
  let format field =
    if field.FieldType = Id
    then "DEFAULT"
    else sprintf ":%s" field.AsDBColumn

  page.Fields
  |> List.filter (fun field -> field.FieldType <> ConfirmPassword)
  |> List.map format
  |> List.map (pad 2)
  |> flattenWith ","

let insertParamTemplate page field =
  if field.FieldType = Password
  then sprintf """|> param "%s" password""" field.AsDBColumn
  else sprintf """|> param "%s" %s.%s""" field.AsDBColumn page.AsVal field.AsProperty

let insertParamsTemplate page =
  page.Fields
  |> List.filter (fun field -> field.FieldType <> Id && field.FieldType <> ConfirmPassword)
  |> List.map (insertParamTemplate page)
  |> List.map (pad 1)
  |> flatten

let insertTemplate site page =
  let idField = page.Fields |> List.find (fun field -> field.FieldType = Id)
  sprintf """
let insert_%s (%s : %s) =
  let sql = "
INSERT INTO %s.%s
  (
%s
  ) VALUES (
%s
  ) RETURNING %s;
"
%s
  use connection = connection connectionString
  use command = command connection sql
  command
%s
  |> executeScalar
  |> string |> int64
  """ page.AsType page.AsVal page.AsType site.AsDatabase page.AsTable (insertColumns page) (insertValues page) idField.AsDBColumn (passwordTemplate page) (insertParamsTemplate page)

(*

UPDATE

*)

let updateColumns page =
  page.Fields
  |> List.filter (fun field -> field.FieldType <> ConfirmPassword)
  |> List.map (fun field -> sprintf """%s = :%s""" field.AsDBColumn field.AsDBColumn)
  |> List.map (pad 1)
  |> flattenWith ","

let updateParamsTemplate page =
  page.Fields
  |> List.filter (fun field -> field.FieldType <> ConfirmPassword)
  |> List.map (fun field -> sprintf """|> param "%s" %s.%s""" field.AsDBColumn page.AsVal field.AsProperty)
  |> List.map (pad 1)
  |> flatten

let updateTemplate site page =
  let idField = page.Fields |> List.find (fun field -> field.FieldType = Id)
  sprintf """
let update_%s (%s : %s) =
  let sql = "
UPDATE %s.%s
SET
%s
WHERE %s = :%s;
"
  use connection = connection connectionString
  use command = command connection sql
  command
%s
  |> executeNonQuery
  """ page.AsType page.AsVal page.AsType site.AsDatabase page.AsTable (updateColumns page) idField.AsDBColumn idField.AsDBColumn (updateParamsTemplate page)

(*

SELECT

*)

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
  |> firstOrNone""" page.AsType site.AsDatabase page.AsTable idField.AsDBColumn idField.AsDBColumn idField.AsDBColumn page.AsType

let selectManyTemplate site page =
  sprintf """
let getMany_%s () =
  let sql = "
SELECT * FROM %s.%s
LIMIT 500
"
  use connection = connection connectionString
  use command = command connection sql
  command
  |> read to%s
  """ page.AsType site.AsDatabase page.AsTable page.AsType

let selectManyWhereTemplate site page =
  sprintf """
let getManyWhere_%s field how value =
  let field = to_dbColumn field
  let search = searchHowToClause how value
  let sql =
    sprintf "SELECT * FROM %s.%s
WHERE lower(%s) LIKE lower(:search)
LIMIT 500" field

  use connection = connection connectionString
  use command = command connection sql
  command
  |> param "search" search
  |> read to%s
  """ page.AsType site.AsDatabase page.AsTable "%s" page.AsType

let createQueriesForPage site page =
  let rec createQueriesForPage pageMode =
    match pageMode with
    | CVELS     -> [Create; View; Edit; List; Search] |> List.map createQueriesForPage |> flatten
    | CVEL      -> [Create; View; Edit; List] |> List.map createQueriesForPage |> flatten
    | Create    -> insertTemplate site page
    | Edit      -> updateTemplate site page
    | View      -> tryByIdTemplate site page
    | List      -> selectManyTemplate site page
    | Search    -> selectManyWhereTemplate site page
    | Register  -> insertTemplate site page
    | Login
    | Jumbotron -> ""

  let queries = createQueriesForPage page.PageMode
  if page.PageMode = CVEL || page.PageMode = CVELS || page.PageMode = View || page.PageMode = List
  then sprintf "%s%s%s" (dataReaderTemplate page) System.Environment.NewLine queries
  else queries

let createQueries (site : Site) =
  site.Pages
  |> List.filter (fun page -> page.CreateTable = CreateTable)
  |> List.map (createQueriesForPage site)
  |> flatten
