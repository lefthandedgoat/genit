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
  |> List.map (createTableTemplate site.AsDatabase)
  |> flatten
