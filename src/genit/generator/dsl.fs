module dsl

open System
open System.Text.RegularExpressions
open helper_general

type Database =
  | Postgres
  | SQLServer

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

type PageAttribute =
  | Standard
  | RequiresLogin

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

type FieldAttribute =
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
    Attribute : FieldAttribute
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

let fieldEx name attribute fieldType asProperty asDBColumn =
  {
    Name = name
    FieldType = fieldType
    Attribute = attribute
    AsProperty = to_property asProperty
    AsDBColumn = to_dbColumn asDBColumn
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
    Attribute : PageAttribute
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
    Database : Database
  }

let private defaultSite =
  {
    Name = "Demo"
    AsDatabase = ""
    Pages = []
    APIs = []
    Database = Postgres
  }

let mutable currentSite = defaultSite

let db database = currentSite <- { currentSite with Database = database }

let site name = currentSite <- { currentSite with Name = name; AsDatabase = to_database name }

let private page_ name pageMode tableName attribute createTable fields =
  let page =
    {
      Name = name
      PageMode = pageMode
      Fields = fields
      Attribute = attribute
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
  page_ "Home" Jumbotron "" Standard DoNotCreateTable []

let precannedRegister () =
  page_ "Register" Register "User" Standard CreateTable
    [
      id_pk "User"
      name "First Name" Required
      name "Last Name" Required
      email "Email"
      password "Password"
      confirm "Confirm Password"
    ]

let precannedLogin () =
  page_ "Login" Login "" Standard DoNotCreateTable
    [
      id_pk "User"
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
  page_ name pageMode name Standard CreateTable fields

let advancedPage name pageMode attribute fields =
  //auto add id
  let id = id_pk name
  let fields = id :: fields
  page_ name pageMode name attribute CreateTable fields

let hasFields page = page.Fields <> []
let isCreate page = page.PageMode = Create || page.PageMode = CVEL || page.PageMode = CVELS
let isEdit page = page.PageMode = Edit || page.PageMode = CVEL || page.PageMode = CVELS
let isView page = page.PageMode = View || page.PageMode = CVEL || page.PageMode = CVELS
let isList page = page.PageMode = List || page.PageMode = CVEL || page.PageMode = CVELS
let isSearch page = page.PageMode = Search || page.PageMode = CVELS
let isRegister page = page.PageMode = Register
let isLogin page = page.PageMode = Login
let isEditView page = isEdit page || isView page
let isCreateEdit page = isCreate page || isEdit page
let isCreateEditRegister page = isCreate page || isEdit page || isRegister page
let isCreateEditRegisterLogin page = isCreate page || isEdit page || isRegister page || isLogin page
let isCreateHasFields page = isCreate page && hasFields page
let isCreateEditHasFields page = isCreateEdit page && hasFields page
let isCreateEditRegisterHasFields page = isCreateEditRegister page && hasFields page
let isCreateEditRegisterLoginHasFields page = isCreateEditRegisterLogin page && hasFields page
let isNotRegisterLoginJumbotron page = not (page.PageMode = Register || page.PageMode = Login || page.PageMode = Jumbotron)
let isNotLoginJumbotron page = not (page.PageMode = Login || page.PageMode = Jumbotron)
let isNotRegisterJumbotron page = not (page.PageMode = Register || page.PageMode = Jumbotron)
let isNotJumbotron page = not (page.PageMode = Jumbotron)

let needsBundle = isNotRegisterLoginJumbotron
let needsFormType = isCreateEditRegisterLoginHasFields
let needsType = isNotJumbotron
let needsValidation = isCreateEditRegisterLoginHasFields
let needsConvert = isCreateEditRegisterLoginHasFields
let needsFakeData = isCreateHasFields
let needsTryById = isEditView
let needsGetMany = isList
let needsGetManyWhere = isSearch
let needsInsert = isCreate
let needsUpdate = isEdit
let needsViewList = isList
let needsViewEdit = isEdit
let needsViewCreate = isCreate
let needsViewView = isView
let needsViewSearch = isSearch
let needsUITests = isCreateEditHasFields
let needsDataReader = isNotRegisterJumbotron
