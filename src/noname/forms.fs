module forms

open Suave.Model.Binding
open Suave.Form
open Suave.ServerErrors
open Microsoft.FSharp.Reflection

let fromString<'a> s =
  match FSharpType.GetUnionCases typeof<'a> |> Array.filter (fun case -> case.Name = s) with
    | [|case|] -> FSharpValue.MakeUnion(case,[||]) :?> 'a
    | _ -> failwith <| sprintf "Can't convert %s to DU" s

let logAndShow500 error =
  printfn "%A" error
  INTERNAL_ERROR "ERROR"

let bindToForm form handler =
  bindReq (bindForm form) handler logAndShow500


type Visibility =
  | Public
  | Private

type RegisterForm =
  {
    FirstName : string
    LastName : string
    Email : string
    Password : string
    RepeatPassword : string
    Visibility : string
    Age : decimal
  }

type Register =
  {
    FirstName : string
    LastName : string
    Email : string
    Password : string
    RepeatPassword : string
    Visibility : Visibility
    Age : int
  }

let convertRegisterForm (registerForm : RegisterForm) =
  {
    FirstName = registerForm.FirstName
    LastName = registerForm.LastName
    Email = registerForm.Email
    Password = registerForm.Password
    RepeatPassword = registerForm.RepeatPassword
    Visibility = fromString<Visibility> registerForm.Visibility
    Age = int registerForm.Age
  }

let registerForm : Form<RegisterForm> = Form ([],[])

type Car =
  {
    Make : string
    Model : string
    Year : int
    Price : int
  }
