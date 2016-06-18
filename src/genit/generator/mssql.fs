module mssql

open dsl
open helper_general
open System

let createTemplate (dbname:string) =
  String.Format("""
use master;
GO
ALTER DATABASE {0} SET SINGLE_USER
GO
DROP DATABASE {0}
GO
CREATE DATABASE {0};""", dbname )

let initialSetupTemplate (dbname : string) = System.String.Format("""

""", dbname)

(*

CREATE TABLES

*)

let columnTypeTemplate field =
  match field.FieldType with
  | Id              -> "int IDENTITY (1,1)"
  | Text            -> "nvarchar(1024)"
  | Paragraph       -> "ntext"
  | Number          -> "int"
  | Decimal         -> "decimal(12, 2)"
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

DATA READERS

*)

let readConversionTemplate field =
  match field.FieldType with
  | Id              -> "int64"
  | Text            -> ""
  | Paragraph       -> ""
  | Number          -> ""
  | Decimal         -> ""
  | Date            -> ""
  | Phone           -> ""
  | Email           -> ""
  | Name            -> ""
  | Password        -> ""
  | ConfirmPassword -> ""
  | Dropdown (_)    -> ""

let dataReaderPropertyTemplate field =
 sprintf """%s = %s record.%s;""" field.AsProperty (readConversionTemplate field)  field.AsDBColumn

let dataReaderPropertiesTemplate page =
  page.Fields
  |> List.filter (fun field -> field.FieldType <> ConfirmPassword)
  |> List.map (fun field -> dataReaderPropertyTemplate field)
  |> List.map (pad 3)
  |> flatten

let dataReaderTemplate page =
  let idField = page.Fields |> List.find (fun field -> field.FieldType = Id)
  System.String.Format(
    sprintf """
[<Literal>]
let {0}QuerySQL = "DECLARE @id int = @p0; SELECT TOP 500 * FROM %s WHERE %s=(CASE WHEN @id=0 THEN %s ELSE @id END )"
type {0}Query = SqlCommandProvider<{0}QuerySQL, connectionString>

let to{0} (a:{0}Query.Record seq )  : %s list =
  a |> Seq.map( fun record ->
  {{
    %s
  }} )
  |> Seq.toList
    """ (if page.AsVal="login" then "users" else page.AsTable) idField.AsDBColumn idField.AsDBColumn page.AsType (dataReaderPropertiesTemplate page), page.AsVal)

(*

INSERT

*)

let insertColumns page =
  page.Fields
  |> List.filter (fun field -> field.FieldType <> ConfirmPassword && field.FieldType <> Id )
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
    sprintf "@%s" field.AsDBColumn

  page.Fields
  |> List.filter (fun field -> field.FieldType <> ConfirmPassword && field.FieldType <> Id )
  |> List.map format
  |> List.map (pad 2)
  |> flattenWith ","

let insertParamTemplate page field =
  if field.FieldType = Password
  then sprintf """password"""
  else if field.Attribute = Null then
    sprintf """option2Val %s.%s""" page.AsVal field.AsProperty
  else
    sprintf """%s.%s""" page.AsVal field.AsProperty

let insertParamsTemplate page =
  page.Fields
  |> List.filter (fun field -> field.FieldType <> Id && field.FieldType <> ConfirmPassword)
  |> List.map (insertParamTemplate page)
  |> List.map (pad 1)
  |> flattenWith ","

let insertTemplate _ page =
  String.Format(
    sprintf """

[<Literal>]
let sql_insert_{0} = "
INSERT INTO %s
    (
  %s
    ) VALUES (
  %s
    ); SELECT SCOPE_IDENTITY()
  "
let insert_{0} ({0} : %s) =
  %s
  use command = new SqlCommandProvider<sql_insert_{0}, connectionString>(connectionString)
  command.Execute(%s) |> Seq.head |> Option.get |> int64
    """ page.AsTable (insertColumns page) (insertValues page) page.AsType  (passwordTemplate page) (insertParamsTemplate page), page.AsVal )

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

let updateTemplate _ page =
  let idField = page.Fields |> List.find (fun field -> field.FieldType = Id)
  String.Format(
    sprintf """
[<Literal>]
let sql_update_{0} = "
  DECLARE @id int = @%s;
  UPDATE %s
  SET
  %s
  WHERE %s = @id;
  "
let update_{0} ({0} : %s) =
    use command = new SqlCommandProvider<sql_update_{0}, connectionString>(connectionString)
    command.Execute(int %s) |> ignore
    """  idField.AsDBColumn page.AsTable (updateColumns page) idField.AsDBColumn page.AsType  (updateParamsTemplate page), page.AsVal )

(*

SELECT

*)

let tryByIdTemplate _ page =
  System.String.Format(
    sprintf """

let tryById_{0} (id:int64) =
  use cmd = new SqlCommandProvider<{0}QuerySQL, connectionString>(connectionString)
  cmd.Execute(int id) |> to{0} |> List.tryHead

let get_{0}ById (id:int) =
  (tryById_{0} (int64 id)).Value

let get_{0}BySId (id:string) =
  (tryById_{0} (int64 (System.Int32.Parse(id)))).Value

    """, page.AsVal )

let selectManyTemplate _ page =
  System.String.Format(
    sprintf """
let getMany_{0} ()=
  use cmd = new SqlCommandProvider<{0}QuerySQL, connectionString>(connectionString)
  cmd.Execute(0) |> to{0}

let getMany_{0}_Names =
  getMany_{0} () |> List.map ( fun p-> p.ToString() )
    """ , page.AsVal )

let selectManyWhereTemplate _ page =
  sprintf """
let getManyWhere_%s field how value =
  getMany_%s ()
  """ page.AsVal page.AsVal

(*

Authentication

*)

let authenticateTemplate _ page =
  sprintf """
[<Literal>]
let sql_authenticate = "
SELECT * FROM users
WHERE email = @email
"
type authenticateQuery = SqlCommandProvider<sql_authenticate, connectionString>
let toLogin (a:authenticateQuery.Record seq )  : Login list =
  a |> Seq.map( fun record ->
  {
          UserID = int64 record.user_id;
      Email =  record.email;
      Password =  record.password;
  } )
  |> Seq.toList
let authenticate (%s : %s) =
  use cmd = new authenticateQuery(connectionString)

  let user =
    cmd.Execute(%s.Email)
    |> toLogin
    |> Seq.tryHead
  match user with
    | None -> None
    | Some(user) ->
      let verified = BCrypt.Verify(%s.Password, user.Password)
      if verified
      then Some(user)
      else None
  """ page.AsVal page.AsType  page.AsVal page.AsVal

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

let createQueries (site : Site) =
  site.Pages
  |> List.map (createQueriesForPage site)
  |> flatten

let generated_data_access_template connectionString guts =
  sprintf """module generated_data_access

open generated_types
open helper_general
open helper_ado
open System.Data
open dsl
open BCrypt.Net

[<Literal>]
let connectionString = "%s"

%s""" connectionString guts

let fieldToProperty field =
  let result =
    match field.FieldType with
    | Id              -> "int64"
    | Text            -> "string"
    | Paragraph       -> "string"
    | Number          -> "int"
    | Decimal         -> "decimal"
    | Date            -> "System.DateTime"
    | Phone           -> "string"
    | Email           -> "string"
    | Name            -> "string"
    | Password        -> "string"
    | ConfirmPassword -> "string"
    | Dropdown _      -> "int16"
  if field.Attribute = Null then
    result + " option"
  else
    result

let fieldLine (field : Field ) =
  match field.Attribute with
  | _ -> sprintf """%s : %s""" field.AsProperty (fieldToProperty field)

let fieldToConvertProperty page (field:Field) =
  let property = sprintf "%s.%s" page.AsFormVal field.AsProperty
  let string () =
    if field.Attribute = Null then
      sprintf """%s = Some(%s)""" field.AsProperty property
    else
      sprintf """%s = %s""" field.AsProperty property
  let int () =
    if field.Attribute = Null then
      sprintf """%s = Some(int %s)""" field.AsProperty property
    else
      sprintf """%s = int %s""" field.AsProperty property
  let int16 () =
    if field.Attribute = Null then
       sprintf """%s = Some(int16 %s)""" field.AsProperty property
    else
       sprintf """%s = int16 %s""" field.AsProperty property
  let int64 () =
    if field.Attribute = Null then
      sprintf """%s = Some(int64 %s)""" field.AsProperty property
    else
      sprintf """%s = int64 %s""" field.AsProperty property
  let decimal () =
    if field.Attribute = Null then
      sprintf """%s = Some(decimal %s)""" field.AsProperty property
    else
      sprintf """%s = decimal %s""" field.AsProperty property
  let datetime () =
    if field.Attribute = Null then
      sprintf """%s = Some(System.DateTime.Parse(%s))""" field.AsProperty property
    else
      sprintf """%s = System.DateTime.Parse(%s)""" field.AsProperty property
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
    | Decimal         -> "random.Next(100) |> decimal"
    | Date            -> "System.DateTime.Now"
    | Phone           -> """sprintf "%i-%i-%i" (random.Next(200,800)) (random.Next(200,800)) (random.Next(2000,8000))"""
    | Email           -> """sprintf "%s@%s.com" (randomItem words) (randomItem words)"""
    | Name            -> pickAppropriateName """randomItem names"""
    | Password        -> """"123123" """ |> trimEnd
    | ConfirmPassword -> """"123123" """ |> trimEnd
    | Dropdown _      -> "1s"
  if field.Attribute = Null then
    sprintf """%s = Some(%s) """ field.AsProperty value
  else
    sprintf """%s = %s """ field.AsProperty value


let fieldToPopulatedHtml page (field : Field) =
  let template tag =
    if field.Attribute = Null then
      sprintf """%s "%s" (option2Val %s.%s) """ tag field.Name page.AsVal field.AsProperty |> trimEnd
    else
      sprintf """%s "%s" %s.%s """ tag field.Name page.AsVal field.AsProperty |> trimEnd
  let iconTemplate tag icon = sprintf """%s "%s" %s.%s "%s" """ tag field.Name page.AsVal field.AsProperty icon |> trimEnd
  match field.FieldType with
  | Id                -> sprintf """hiddenInput "%s" %s.%s """ field.AsProperty page.AsVal field.AsProperty
  | Text              -> template "label_text"
  | Paragraph         -> template "label_textarea"
  | Number            -> template "label_text"
  | Decimal           -> template "label_text"
  | Date              -> template "label_datetime"
  | Phone             -> template "label_text"
  | Email             -> iconTemplate "icon_label_text" "envelope"
  | Name              -> iconTemplate "icon_label_text" "user"
  | Password          -> iconTemplate "icon_password_text" "lock"
  | ConfirmPassword   -> iconTemplate "icon_password_text" "lock"
  | Dropdown options  -> sprintf """label_select_selected "%s" %A (Some %s.%s)""" field.Name (zipOptions options) page.AsVal field.AsProperty

let createConnectionString site = sprintf @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=%s;Integrated Security=True" site.AsDatabase
