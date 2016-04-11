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
let to_href = camelCase >> sprintf "/%s"

let to_property = typeCase

type PageMode =
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
  | NotNull
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

let text name attribute = { Name = name; FieldType = Text; Attribute = attribute; AsProperty = to_property name }
let name name attribute = { Name = name; FieldType = Name; Attribute = attribute; AsProperty = to_property name }
let email name = { Name = name; FieldType = Email; Attribute = Required; AsProperty = to_property name }
let password name = { Name = name; FieldType = Password; Attribute = Required; AsProperty = to_property name }

type Page =
  {
    Name : string
    PageMode : PageMode
    Fields : Field list
    AsVal : string
    AsType : string
    AsFormVal : string
    AsFormType : string
    AsHref : string
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
      AsHref = to_href name
    }
  currentSite <- { currentSite with Pages = currentSite.Pages @ [page] }

//precanned pages

type Precanned =
  | Home
  | Register

let home = Home
let registration = Register

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

let basic precanned =
  match precanned with
  | Home     -> precannedHome()
  | Register -> precannedRegister()
