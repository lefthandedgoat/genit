module generated_validation

open generated_forms
open validators

let validation_registerForm (registerForm : RegisterForm) =
  [
    validate_required "First Name" registerForm.FirstName
    validate_required "Last Name" registerForm.LastName
    validate_email "Email" registerForm.Email
    validate_required "Email" registerForm.Email
    validate_password "Password" registerForm.Password
    validate_required "Password" registerForm.Password
    validate_password "Confirm Password" registerForm.ConfirmPassword
    validate_equal "Password" "Confirm Password" registerForm.Password registerForm.ConfirmPassword
    validate_required "Confirm Password" registerForm.ConfirmPassword
  ] |> List.choose id
  
let validation_loginForm (loginForm : LoginForm) =
  [
    validate_email "Email" loginForm.Email
    validate_required "Email" loginForm.Email
    validate_password "Password" loginForm.Password
    validate_required "Password" loginForm.Password
  ] |> List.choose id
  
let validation_selfForm (selfForm : SelfForm) =
  [
    validate_required "First Name" selfForm.FirstName
    validate_required "Last Name" selfForm.LastName
    validate_required "Title" selfForm.Title
    validate_email "Email" selfForm.Email
    validate_required "Email" selfForm.Email
    validate_required "Position Objective 1" selfForm.PositionObjective1
    validate_required "Supervisor" selfForm.Supervisor
    validate_required "Coworker or Client 1" selfForm.CoworkerorClient1
    validate_required "Coworker or Client 2" selfForm.CoworkerorClient2
    validate_required "Coworker or Client 3" selfForm.CoworkerorClient3
    validate_required "Coworker or Client 4" selfForm.CoworkerorClient4
  ] |> List.choose id
  
let validation_customerForm (customerForm : CustomerForm) =
  [
    validate_required "Customer Name" customerForm.CustomerName
    validate_required "Contact Name" customerForm.ContactName
    validate_email "Email" customerForm.Email
    validate_required "Email" customerForm.Email
  ] |> List.choose id
  
let validation_employeeSurveyForm (employeeSurveyForm : EmployeeSurveyForm) =
  [
    validate_required "Rating" employeeSurveyForm.Rating
  ] |> List.choose id
  
let validation_customerSurveyForm (customerSurveyForm : CustomerSurveyForm) =
  [
    validate_required "Comment" customerSurveyForm.Comment
    validate_required "How Likely" customerSurveyForm.HowLikely
  ] |> List.choose id
  