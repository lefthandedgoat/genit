module dsl

type PageMode =
  | Create
  | Edit
  | View
  | List
  | Submit

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
  }

let text name attribute = { Name = name; FieldType = Text; Attribute = attribute }
let name name attribute = { Name = name; FieldType = Name; Attribute = attribute }
let email name = { Name = name; FieldType = Email; Attribute = Required }
let password name = { Name = name; FieldType = Password; Attribute = Required }

type Page =
  {
    Name : string
    PageMode : PageMode
    Fields : Field list
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
  let page = { Name = name; PageMode = pageMode; Fields = fields }
  currentSite <- { currentSite with Pages = currentSite.Pages @ [page] }


//precanned pages

type Precanned =
  | Register

let registration = Register

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
  | Register -> precannedRegister()
