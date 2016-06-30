module mssql

open dsl
open helper_general
open System

let createTemplate (dbname : string) =
  String.Format("""
use master;
GO
IF EXISTS(select * from sys.databases where name='{0}')
ALTER DATABASE {0} SET SINGLE_USER
GO
IF EXISTS(select * from sys.databases where name='{0}')
DROP DATABASE {0}
GO
CREATE DATABASE {0};
GO""", dbname )

let initialSetupTemplate site = System.String.Format("""

""", site.AsDatabase)

(*

CREATE TABLES

*)

let columnTypeTemplate field =
  match field.FieldType with
  | Id              -> "bigint IDENTITY (1,1)"
  | Text            -> "nvarchar(1024)"
  | Paragraph       -> "ntext"
  | Number          -> "int"
  | Decimal         -> "float"
  | Date            -> "datetime"
  | Phone           -> "varchar(15)"
  | Email           -> "varchar(128)"
  | Name            -> "varchar(128)"
  | Password        -> "varchar(60)"
  | ConfirmPassword -> ""
  | Dropdown (_)    -> "smallint"

let columnAttributesTemplate (field : Field) =
  match field.Attribute with
  | PK              -> "PRIMARY KEY NOT NULL"
  | Null            -> "NULL"
  | Required        -> "NOT NULL"
  | Min(min)        -> sprintf "CHECK (%s > %i)" field.AsDBColumn min
  | Max(max)        -> sprintf "CHECK (%s < %i)" field.AsDBColumn max
  | Range(min, max) -> sprintf "CHECK (%i < %s < %i)" min field.AsDBColumn max

let createTableTemplate dbname page columns =
  sprintf """
USE %s
GO

CREATE TABLE %s(
%s
);
  """ dbname page.AsTable columns

let grantPrivileges (site : Site) = System.String.Format("""
  """, site)

let createTables guts1 guts2 =
  sprintf """
%s

%s
  """ guts1 guts2

(*

INSERT

*)

let insertColumns page =
  page.Fields
  |> List.filter (fun field -> field.FieldType <> Id && field.FieldType <> ConfirmPassword)
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
  let format field = sprintf "@%s" field.AsDBColumn

  page.Fields
  |> List.filter (fun field -> field.FieldType <> Id && field.FieldType <> ConfirmPassword)
  |> List.map format
  |> List.map (pad 2)
  |> flattenWith ","

let insertParamTemplate page field =
  if field.FieldType = Password
  then sprintf """|> param "%s" password""" field.AsDBColumn
  else
    if field.Attribute = Null && useSome field
    then sprintf """|> paramOption "%s" %s.%s""" field.AsDBColumn page.AsVal field.AsProperty
    else sprintf """|> param "%s" %s.%s""" field.AsDBColumn page.AsVal field.AsProperty

let insertParamsTemplate page =
  page.Fields
  |> List.filter (fun field -> field.FieldType <> Id && field.FieldType <> ConfirmPassword)
  |> List.map (insertParamTemplate page)
  |> List.map (pad 1)
  |> flatten

let insertTemplate site page =
  sprintf """
let insert_%s (%s : %s) =
  let sql = "
INSERT INTO %s.dbo.%s
  (
%s
  ) VALUES (
%s
  ); SELECT SCOPE_IDENTITY()
"
%s
  use connection = connection connectionString
  use command = command connection sql
  command
%s
  |> executeScalar
  |> string |> int64
  """ page.AsVal page.AsVal page.AsType site.AsDatabase page.AsTable (insertColumns page) (insertValues page) (passwordTemplate page) (insertParamsTemplate page)

(*

UPDATE

*)
let updateColumns page =
  page.Fields
  |> List.filter (fun field -> field.FieldType <> Id && field.FieldType <> ConfirmPassword)
  |> List.map (fun field -> sprintf """%s = @%s""" field.AsDBColumn field.AsDBColumn)
  |> List.map (pad 1)
  |> flattenWith ","

let updateParamsTemplate page =
  page.Fields
  |> List.filter (fun field -> field.FieldType <> ConfirmPassword)
  |> List.map (fun field ->
               if field.Attribute = Null && useSome field
               then sprintf """|> paramOption "%s" %s.%s""" field.AsDBColumn page.AsVal field.AsProperty
               else sprintf """|> param "%s" %s.%s""" field.AsDBColumn page.AsVal field.AsProperty)
  |> List.map (pad 1)
  |> flatten

let updateTemplate site page =
  let idField = page.Fields |> List.find (fun field -> field.FieldType = Id)
  sprintf """
let update_%s (%s : %s) =
  let sql = "
UPDATE %s.dbo.%s
SET
%s
WHERE %s = @%s;
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
SELECT * FROM %s.dbo.%s
WHERE %s = @%s
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
SELECT TOP 500 * FROM %s.dbo.%s
"
  use connection = connection connectionString
  use command = command connection sql
  command
  |> read to%s
  """ page.AsVal site.AsDatabase page.AsTable page.AsType

let selectManyWhereTemplate site page =
  sprintf """
let getManyWhere_%s field how value =
  let field = to_sqlserver_dbColumn field
  let search = searchHowToClause how value
  let sql =
    sprintf "SELECT TOP 500 * FROM %s.dbo.%s
WHERE lower(%s) LIKE lower(@search)" field

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
SELECT * FROM %s.dbo.Users
WHERE Email = @Email
"
  use connection = connection connectionString
  use command = command connection sql
  let user =
    command
    |> param "Email" %s.Email
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

let generated_data_access_template connectionString guts =
  sprintf """module generated_data_access

open System.Data
open generated_types
open helper_general
open helper_ado
open helper_sqlado
open System.Data
open dsl
open BCrypt.Net

[<Literal>]
let connectionString = "%s"

%s""" connectionString guts

let createConnectionString site = sprintf @"Data Source=.\SQLEXPRESS;Initial Catalog=%s;Integrated Security=True" site.AsDatabase
