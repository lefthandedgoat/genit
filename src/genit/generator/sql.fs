module sql

open dsl
open helper_general
open psql

type Engine =
  | PostgreSQL
  | MicrosoftSQL

let createTemplate dbname engine =
  match engine with
  | PostgreSQL -> psql.createTemplate dbname
  | MicrosoftSQL -> mssql.createTemplate dbname


let initialSetupTemplate (dbname : string) engine =
  match engine with
  | PostgreSQL -> psql.initialSetupTemplate dbname
  | MicrosoftSQL -> mssql.initialSetupTemplate dbname

(*

CREATE TABLES

*)

let columnTypeTemplate field engine =
  match engine with
  | PostgreSQL -> psql.columnTypeTemplate field
  | MicrosoftSQL -> mssql.columnTypeTemplate field

//http://www.postgresql.org/docs/9.5/static/ddl-constraints.html
let columnAttributesTemplate (field : Field) engine =
  match engine with
  | PostgreSQL -> psql.columnAttributesTemplate field
  | MicrosoftSQL -> mssql.columnAttributesTemplate field

let columnTemplate namePad typePad engine (field : Field)  =
 sprintf "%s %s %s" (rightPad namePad field.AsDBColumn) (rightPad typePad (columnTypeTemplate field engine)) (columnAttributesTemplate field engine)

let createColumns (page : Page) engine =
  let maxName = page.Fields |> List.map (fun field -> field.AsDBColumn.Length) |> List.max
  let maxName = if maxName > 20 then maxName else 20
  let maxType = page.Fields |> List.map (fun field -> (columnTypeTemplate field engine).Length) |> List.max
  let maxType = if maxType > 20 then maxType else 20

  page.Fields
  |> List.filter (fun field -> field.FieldType <> ConfirmPassword)
  |> List.map (columnTemplate maxName maxType engine)
  |> List.map (pad 1)
  |> flattenWith ","

let createTableTemplate (dbname : string) (engine:Engine) (page : Page) =
  let columns = createColumns page engine
  sprintf """
USE %s
GO

CREATE TABLE %s(
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
  | Login       -> false
  | Search      -> true
  | Jumbotron   -> false

let createTableTemplates (site : Site) (engine:Engine) =
  site.Pages
  |> List.filter (fun page -> shouldICreateTable page)
  |> List.filter (fun page -> page.CreateTable = CreateTable)
  |> List.map (createTableTemplate site.AsDatabase engine)
  |> flatten

let grantPrivileges (site : Site) (engine:Engine)  =
  match engine with
  | PostgreSQL -> psql.grantPrivileges site
  | MicrosoftSQL -> mssql.grantPrivileges site

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
  | Referenced      -> "getInt64"

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
  """ page.AsVal page.AsVal page.AsType site.AsDatabase page.AsTable (insertColumns page) (insertValues page) idField.AsDBColumn (passwordTemplate page) (insertParamsTemplate page)

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
  """ page.AsVal page.AsVal page.AsType site.AsDatabase page.AsTable (updateColumns page) idField.AsDBColumn idField.AsDBColumn (updateParamsTemplate page)

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
  |> firstOrNone""" page.AsVal site.AsDatabase page.AsTable idField.AsDBColumn idField.AsDBColumn idField.AsDBColumn page.AsType

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
  """ page.AsVal site.AsDatabase page.AsTable page.AsType

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
  """ page.AsVal site.AsDatabase page.AsTable "%s" page.AsType

(*

Authentication

*)

let authenticateTemplate site page =
  sprintf """
let authenticate (%s : %s) =
  let sql = "
SELECT * FROM %s.users
WHERE email = :email
"
  use connection = connection connectionString
  use command = command connection sql
  let user =
    command
    |> param "email" %s.Email
    |> read toLogin
    |> firstOrNone
  match user with
    | None -> None
    | Some(user) ->
      let verified = BCrypt.Verify(%s.Password, user.Password)
      if verified
      then Some(user)
      else None
  """ page.AsVal page.AsType site.AsDatabase page.AsVal page.AsVal

(*

Everything else

*)

let createQueriesForPage site (engine:Engine) page =
  match engine with
  | PostgreSQL -> psql.createQueriesForPage site page
  | MicrosoftSQL -> mssql.createQueriesForPage site page

let createQueries (site : Site) (engine:Engine) =
  site.Pages
  |> List.map (createQueriesForPage site engine)
  |> flatten

let fieldLine (field : Field ) (engine:Engine) =
  match engine with
  | PostgreSQL -> psql.fieldLine field
  | MicrosoftSQL -> mssql.fieldLine field


let fieldToConvertProperty page field (engine:Engine) =
  match engine with
  | PostgreSQL -> psql.fieldToConvertProperty page field
  | MicrosoftSQL -> mssql.fieldToConvertProperty page field

let fakePropertyTemplate (field : Field) (engine:Engine) =
  match engine with
  | PostgreSQL -> psql.fakePropertyTemplate field
  | MicrosoftSQL -> mssql.fakePropertyTemplate field

let fieldToPopulatedHtml page (field : Field) (engine:Engine) =
  match engine with
  | PostgreSQL -> psql.fieldToPopulatedHtml page field
  | MicrosoftSQL -> mssql.fieldToPopulatedHtml page field

