module dsl

open System

let upperFirst (value : string) = Char.ToUpper(value.[0]).ToString() + value.Substring(1)
let lowerFirst (value : string) = Char.ToLower(value.[0]).ToString() + value.Substring(1)
let spaceToNothing (value : string) = value.Replace(" ", "")
let camelCase = spaceToNothing >> lowerFirst
let typeCase = spaceToNothing >> upperFirst
let form value = sprintf "%sForm" value

let to_val = camelCase
let to_type = typeCase
let to_formVal = camelCase >> form
let to_formType = typeCase >> form
let to_createHref = camelCase >> sprintf "/%s/create"
let to_viewHref = camelCase >> sprintf "/%s"
let to_editHref = camelCase >> sprintf "/%s/edit"
let to_listHref = camelCase >> sprintf "/%s/list"

let to_property = typeCase

type PageMode =
  | CVEL
  | Create
  | Edit
  | View
  | List
  | Submit
  | Jumbotron

type FieldType =
  | Text
  | Paragraph
  | Number
  | Decimal
  | Date
  | Email
  | Name
  | Phone
  | Password
  | Dropdown of options:string list

type Attribute =
  | Id
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
  }

let field name attribute fieldType = { Name = name; FieldType = fieldType; Attribute = attribute; AsProperty = to_property name }
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
    AsVal : string
    AsType : string
    AsFormVal : string
    AsFormType : string
    AsCreateHref : string
    AsViewHref : string
    AsEditHref : string
    AsListHref : string
  }

type Site =
  {
    Name : string
    Pages : Page list
  }

let private defaultSite =
  {
    Name = "Demo"
    Pages = []
  }

let mutable currentSite = defaultSite

let site name = currentSite <- { currentSite with Name = name }

let page name pageMode fields =
  let page =
    {
      Name = name
      PageMode = pageMode
      Fields = fields
      AsVal = to_val name
      AsType = to_type name
      AsFormVal = to_formVal name
      AsFormType = to_formType name
      AsCreateHref = to_createHref name
      AsViewHref = to_viewHref name
      AsEditHref = to_editHref name
      AsListHref = to_listHref name
    }

  currentSite <- { currentSite with Pages = currentSite.Pages @ [page] }

//precanned pages

type Precanned =
  | Home
  | Register
  | Login

let home = Home
let registration = Register
let login = Login

let precannedHome () =
  page "Home" Jumbotron []

let precannedRegister () =
  page "Register" Submit
    [
      name "First Name" Required
      name "Last Name" Required
      email "Email"
      password "Password"
      password "Repeat Password"
    ]

let precannedLogin () =
  page "Login" Submit
    [
      email "Email"
      password "Password"
    ]

let basic precanned =
  match precanned with
  | Home     -> precannedHome()
  | Register -> precannedRegister()
  | Login    -> precannedLogin()
