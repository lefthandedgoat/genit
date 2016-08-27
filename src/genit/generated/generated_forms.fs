module generated_forms

open Suave.Model.Binding
open Suave.Form
open generated_types

type RegisterForm =
  {
    UserID : string
    FirstName : string
    LastName : string
    Email : string
    Password : string
    ConfirmPassword : string
  }

let registerForm : Form<RegisterForm> = Form ([],[])

let convert_registerForm (registerForm : RegisterForm) : Register =
  {
    UserID = int64 registerForm.UserID
    FirstName = registerForm.FirstName
    LastName = registerForm.LastName
    Email = registerForm.Email
    Password = registerForm.Password
  }
  
type LoginForm =
  {
    UserID : string
    Email : string
    Password : string
  }

let loginForm : Form<LoginForm> = Form ([],[])

let convert_loginForm (loginForm : LoginForm) : Login =
  {
    UserID = int64 loginForm.UserID
    Email = loginForm.Email
    Password = loginForm.Password
  }
  
type SelfForm =
  {
    SelfID : string
    FirstName : string
    LastName : string
    Title : string
    Email : string
    PositionObjective1 : string
    PositionObjective2 : string
    PositionObjective3 : string
    Supervisor : string
    CoworkerorClient1 : string
    CoworkerorClient2 : string
    CoworkerorClient3 : string
    CoworkerorClient4 : string
  }

let selfForm : Form<SelfForm> = Form ([],[])

let convert_selfForm (selfForm : SelfForm) : Self =
  {
    SelfID = int64 selfForm.SelfID
    FirstName = selfForm.FirstName
    LastName = selfForm.LastName
    Title = selfForm.Title
    Email = selfForm.Email
    PositionObjective1 = selfForm.PositionObjective1
    PositionObjective2 = selfForm.PositionObjective2
    PositionObjective3 = selfForm.PositionObjective3
    Supervisor = int16 selfForm.Supervisor
    CoworkerorClient1 = int16 selfForm.CoworkerorClient1
    CoworkerorClient2 = int16 selfForm.CoworkerorClient2
    CoworkerorClient3 = int16 selfForm.CoworkerorClient3
    CoworkerorClient4 = int16 selfForm.CoworkerorClient4
  }
  
type CustomerForm =
  {
    CustomerID : string
    CustomerName : string
    ContactName : string
    Email : string
  }

let customerForm : Form<CustomerForm> = Form ([],[])

let convert_customerForm (customerForm : CustomerForm) : Customer =
  {
    CustomerID = int64 customerForm.CustomerID
    CustomerName = customerForm.CustomerName
    ContactName = customerForm.ContactName
    Email = customerForm.Email
  }
  
type EmployeeSurveyForm =
  {
    EmployeeSurveyID : string
    EmployeeName : string
    PositionObjective1 : string
    PositionObjective2 : string
    PositionObjective3 : string
    Rating : string
  }

let employeeSurveyForm : Form<EmployeeSurveyForm> = Form ([],[])

let convert_employeeSurveyForm (employeeSurveyForm : EmployeeSurveyForm) : EmployeeSurvey =
  {
    EmployeeSurveyID = int64 employeeSurveyForm.EmployeeSurveyID
    EmployeeName = employeeSurveyForm.EmployeeName
    PositionObjective1 = employeeSurveyForm.PositionObjective1
    PositionObjective2 = employeeSurveyForm.PositionObjective2
    PositionObjective3 = employeeSurveyForm.PositionObjective3
    Rating = int16 employeeSurveyForm.Rating
  }
  
type CustomerSurveyForm =
  {
    CustomerSurveyID : string
    Customer : string
    Comment : string
    HowLikely : string
  }

let customerSurveyForm : Form<CustomerSurveyForm> = Form ([],[])

let convert_customerSurveyForm (customerSurveyForm : CustomerSurveyForm) : CustomerSurvey =
  {
    CustomerSurveyID = int64 customerSurveyForm.CustomerSurveyID
    Customer = customerSurveyForm.Customer
    Comment = customerSurveyForm.Comment
    HowLikely = int16 customerSurveyForm.HowLikely
  }
  