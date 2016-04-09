module generated_forms

open Suave.Model.Binding
open Suave.Form

type RegisterForm =
  {
    FirstName : string
    LastName : string
    Email : string
    Password : string
    RepeatPassword : string
  }

let registerForm : Form<RegisterForm> = Form ([],[])
