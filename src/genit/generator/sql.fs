module sql

open dsl
open helper_general
open psql

let createTemplate dbname database =
  match database with
  | Postgres  -> psql.createTemplate dbname
  | SQLServer -> mssql.createTemplate dbname

let initialSetupTemplate dbname database =
  match database with
  | Postgres  -> psql.initialSetupTemplate dbname
  | SQLServer -> mssql.initialSetupTemplate dbname

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
  let maxName = if maxName > 20 then maxName else 20
  let maxType = page.Fields |> List.map (fun field -> (columnTypeTemplate site field).Length) |> List.max
  let maxType = if maxType > 20 then maxType else 20

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
  |> List.filter (fun page -> shouldICreateTable page)
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
  let field = to_postgres_dbColumn field
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

let createQueriesForPage site page =
  match site.Database with
  | Postgres  -> psql.createQueriesForPage site page
  | SQLServer -> mssql.createQueriesForPage site page

let createQueries site =
  site.Pages
  |> List.map (createQueriesForPage site)
  |> flatten

let fieldLine site field =
  match site.Database with
  | Postgres  -> psql.fieldLine field
  | SQLServer -> mssql.fieldLine field

let fieldToConvertProperty site page field =
  match site.Database with
  | Postgres  -> psql.fieldToConvertProperty page field
  | SQLServer -> mssql.fieldToConvertProperty page field

let fakePropertyTemplate site field =
  match site.Database with
  | Postgres  -> psql.fakePropertyTemplate field
  | SQLServer -> mssql.fakePropertyTemplate field

let fieldToPopulatedHtml site page field =
  match site.Database with
  | Postgres  -> psql.fieldToPopulatedHtml page field
  | SQLServer -> mssql.fieldToPopulatedHtml page field

let createConnectionString site =
  match site.Database with
  | Postgres  -> psql.createConnectionString site
  | SQLServer -> mssql.createConnectionString site
