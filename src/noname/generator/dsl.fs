module dsl

open System
open System.Text.RegularExpressions
open generalHelpers

type PageMode =
  | CVELS
  | CVEL
  | Create
  | Edit
  | View
  | List
  | Search
  | Register
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
  | ConfirmPassword
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
let confirm name = field name Required ConfirmPassword
let dropdown name options = field name Null (Dropdown(options))

type API =
  {
    Name : string
    AsVal : string
    AsType : string
    AsViewHref : string
  }

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
    AsSearchHref : string
    AsTable : string
  }

type Site =
  {
    Name : string
    AsDatabase : string
    Pages : Page list
    APIs : API list
  }

let private defaultSite =
  {
    Name = "Demo"
    AsDatabase = ""
    Pages = []
    APIs = []
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
      AsSearchHref = to_searchHref name
      AsTable = to_tableName tableName
    }

  currentSite <- { currentSite with Pages = currentSite.Pages @ [page] }

let api name =
  let api : API =
    {
      Name = name
      AsViewHref = to_apiViewHref name
      AsVal = to_val name
      AsType = to_type name
    }

  currentSite <- { currentSite with APIs = currentSite.APIs @ [api] }

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
  page_ "Register" Register "User" CreateTable
    [
      id_pk "User"
      name "First Name" Required
      name "Last Name" Required
      email "Email"
      password "Password"
      confirm "Confirm Password"
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
