module dsl

open System

let flatten values =
  if values = []
  then ""
  else values |> List.reduce (fun value1 value2 -> sprintf "%s%s%s" value1 Environment.NewLine value2)

let flattenWith delimeter values =
  if values = []
  then ""
  else values |> List.reduce (fun value1 value2 -> sprintf "%s%s%s%s" value1 delimeter Environment.NewLine value2)

let repeat (value : string) times = [1..times] |> List.map (fun _ -> value) |> List.reduce (+)
let pad tabs field = sprintf "%s%s" (repeat "  " tabs) field
let rightPad upto field = sprintf "%s%s" field (repeat " " (upto - field.Length))
let clean (value : string) = value.Replace("'", "").Replace("\"", "").Replace("-", "")
let lower (value : string) = value.ToLower()
let upperFirst (value : string) = Char.ToUpper(value.[0]).ToString() + value.Substring(1)
let lowerFirst (value : string) = Char.ToLower(value.[0]).ToString() + value.Substring(1)
let spaceToNothing (value : string) = value.Replace(" ", "")
let spaceToUnderscore (value : string) = value.Replace(" ", "_")
let camelCase = spaceToNothing >> lowerFirst
let typeCase = spaceToNothing >> upperFirst
let form value = sprintf "%sForm" value
let pluralize = sprintf "%ss" //trrrrble

let to_val = camelCase
let to_type = typeCase
let to_formVal = camelCase >> form
let to_formType = typeCase >> form
let to_href = camelCase >> sprintf "/%s"
let to_createHref = camelCase >> sprintf "/%s/create"
let to_viewHref = camelCase >> (fun page -> sprintf "/%s/%s" page "%i")
let to_editHref = camelCase >> (fun page -> sprintf "/%s/edit/%s" page "%i")
let to_listHref = camelCase >> sprintf "/%s/list"

let to_property = typeCase

let to_database = clean >> lower >> spaceToUnderscore
let to_tableName = clean >> lower >> spaceToUnderscore >> pluralize
let to_dbColumn = clean >> lower >> spaceToUnderscore

type PageMode =
  | CVEL
  | Create
  | Edit
  | View
  | List
  | Submit
  | Login
  | Jumbotron

type CreateTable =
  | CreateTable
  | DoNotCreateTable

type FieldType =
  | Id
  | Text
  | Paragraph
  | Number
  | Decimal
  | Date
  | Phone
  | Email
  | Name
  | Password
  | Dropdown of options:string list

type Attribute =
  | PK
  | Null
  | Required
  | Min of value:int
  | Max of value:int
  | Range of min:int * max:int

type Field =
  {
    Name : string
    FieldType : FieldType
    Attribute : Attribute
    AsProperty : string
    AsDBColumn : string
  }

let field name attribute fieldType =
  {
    Name = name
    FieldType = fieldType
    Attribute = attribute
    AsProperty = to_property name
    AsDBColumn = to_dbColumn name
  }

let id_pk name = field (sprintf "%s ID" name) PK Id
let text name attribute = field name attribute Text
let paragraph name attribute = field name attribute Paragraph
let number name attribute = field name attribute Number
let dollar name attribute = field name attribute Decimal
let date name attribute = field name attribute Date
let email name = field name Required Email
let name name attribute = field name attribute Name
let phone name attribute = field name attribute Phone
let password name = field name Required Password
let dropdown name options = field name Null (Dropdown(options))

type Page =
  {
    Name : string
    PageMode : PageMode
    Fields : Field list
    CreateTable : CreateTable
    AsVal : string
    AsType : string
    AsFormVal : string
    AsFormType : string
    AsHref : string
    AsCreateHref : string
    AsViewHref : string
    AsEditHref : string
    AsListHref : string
    AsTable : string
  }

type Site =
  {
    Name : string
    AsDatabase : string
    Pages : Page list
  }

let private defaultSite =
  {
    Name = "Demo"
    AsDatabase = ""
    Pages = []
  }

let mutable currentSite = defaultSite

let site name = currentSite <- { currentSite with Name = name; AsDatabase = to_database name }

let private page_ name pageMode tableName createTable fields =
  let page =
    {
      Name = name
      PageMode = pageMode
      Fields = fields
      CreateTable = createTable
      AsVal = to_val name
      AsType = to_type name
      AsFormVal = to_formVal name
      AsFormType = to_formType name
      AsHref = to_href name
      AsCreateHref = to_createHref name
      AsViewHref = to_viewHref name
      AsEditHref = to_editHref name
      AsListHref = to_listHref name
      AsTable = to_tableName tableName
    }

  currentSite <- { currentSite with Pages = currentSite.Pages @ [page] }

//precanned pages

type Precanned =
  | HomePage
  | RegisterPage
  | LoginPage

let home = HomePage
let registration = RegisterPage
let login = LoginPage

let precannedHome () =
  page_ "Home" Jumbotron "" DoNotCreateTable []

let precannedRegister () =
  page_ "Register" Submit "User" CreateTable
    [
      id_pk "User"
      name "First Name" Required
      name "Last Name" Required
      email "Email"
      password "Password"
      password "Repeat Password"
    ]

let precannedLogin () =
  page_ "Login" Login "" DoNotCreateTable
    [
      email "Email"
      password "Password"
    ]

let basic precanned =
  match precanned with
  | HomePage     -> precannedHome()
  | RegisterPage -> precannedRegister()
  | LoginPage    -> precannedLogin()

let page name pageMode fields =
  //auto add id
  let id = id_pk name
  let fields = id :: fields
  page_ name pageMode name CreateTable fields
