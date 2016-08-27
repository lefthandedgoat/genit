module generated_views

open Suave.Html
open helper_html
open helper_bootstrap
open generated_html
open generated_forms
open generated_data_access
open generated_types
open generator
open helper_general

let brand = "defi Pays"

let view_jumbo_home =
  base_html
    "Home"
    (base_header brand)
    [
      divClass "container" [
        divClass "jumbotron" [
          h1 (sprintf "Welcome to defi Pays!")
        ]
      ]
    ]
    scripts.common

let view_register =
  base_middle_html
    "Register"
    (base_header brand)
    [
      common_register_form
        "Register"
        [
          hiddenInput "UserID" "-1"
          icon_label_text "First Name" "" "user"
          icon_label_text "Last Name" "" "user"
          icon_label_text "Email" "" "envelope"
          icon_password_text "Password" "" "lock"
          icon_password_text "Confirm Password" "" "lock"
        ]
    ]
    scripts.common

let view_errored_register errors (registerForm : RegisterForm) =
  base_middle_html
    "Register"
    (base_header brand)
    [
      common_register_form
        "Register"
        [
          hiddenInput "UserID" registerForm.UserID
          errored_icon_label_text "First Name" registerForm.FirstName "user" errors
          errored_icon_label_text "Last Name" registerForm.LastName "user" errors
          errored_icon_label_text "Email" registerForm.Email "envelope" errors
          errored_icon_password_text "Password" registerForm.Password "lock" errors
          errored_icon_password_text "Confirm Password" registerForm.ConfirmPassword "lock" errors
        ]
    ]
    scripts.common

let view_login error email =
  let errorTag =
    if error
    then stand_alone_error "Invalid email or password"
    else emptyText

  base_middle_html
    "Login"
    (base_header brand)
    [
      common_register_form
        "Login"
        [
          errorTag
          hiddenInput "UserID" "-1"
          icon_label_text "Email" email "envelope"
          icon_password_text "Password" "" "lock"
        ]
    ]
    scripts.common

let view_errored_login errors (loginForm : LoginForm) =
  base_middle_html
    "Login"
    (base_header brand)
    [
      common_register_form
        "Login"
        [
          hiddenInput "UserID" loginForm.UserID
          errored_icon_label_text "Email" loginForm.Email "envelope" errors
          errored_icon_password_text "Password" loginForm.Password "lock" errors
        ]
    ]
    scripts.common

let view_create_self =
  base_html
    "Create Self"
    (base_header brand)
    [
      common_form
        "Create Self"
        [
          hiddenInput "SelfID" "-1"
          label_text "First Name" ""
          label_text "Last Name" ""
          label_text "Title" ""
          icon_label_text "Email" "" "envelope"
          label_text "Position Objective 1" ""
          label_text "Position Objective 2" ""
          label_text "Position Objective 3" ""
          label_select "Supervisor" [("0", ""); ("1", "Sally"); ("2", "Bob"); ("3", "Pat"); ("4", "Tom")]
          label_select "Coworker or Client 1" [("0", ""); ("1", "Rex"); ("2", "Sue")]
          label_select "Coworker or Client 2" [("0", ""); ("1", "Rex"); ("2", "Sue")]
          label_select "Coworker or Client 3" [("0", ""); ("1", "Rex"); ("2", "Sue")]
          label_select "Coworker or Client 4" [("0", ""); ("1", "Rex"); ("2", "Sue")]
        ]
    ]
    scripts.common

let view_create_errored_self errors (selfForm : SelfForm) =
  base_html
    "Create Self"
    (base_header brand)
    [
      common_form
        "Create Self"
        [
          hiddenInput "SelfID" selfForm.SelfID
          errored_label_text "First Name" (string selfForm.FirstName) errors
          errored_label_text "Last Name" (string selfForm.LastName) errors
          errored_label_text "Title" (string selfForm.Title) errors
          errored_icon_label_text "Email" selfForm.Email "envelope" errors
          errored_label_text "Position Objective 1" (string selfForm.PositionObjective1) errors
          errored_label_text "Position Objective 2" (string selfForm.PositionObjective2) errors
          errored_label_text "Position Objective 3" (string selfForm.PositionObjective3) errors
          errored_label_select "Supervisor" [("0", ""); ("1", "Sally"); ("2", "Bob"); ("3", "Pat"); ("4", "Tom")] (Some selfForm.Supervisor) errors
          errored_label_select "Coworker or Client 1" [("0", ""); ("1", "Rex"); ("2", "Sue")] (Some selfForm.CoworkerorClient1) errors
          errored_label_select "Coworker or Client 2" [("0", ""); ("1", "Rex"); ("2", "Sue")] (Some selfForm.CoworkerorClient2) errors
          errored_label_select "Coworker or Client 3" [("0", ""); ("1", "Rex"); ("2", "Sue")] (Some selfForm.CoworkerorClient3) errors
          errored_label_select "Coworker or Client 4" [("0", ""); ("1", "Rex"); ("2", "Sue")] (Some selfForm.CoworkerorClient4) errors
        ]
    ]
    scripts.common

let view_generate_self (self : Self) =
  base_html
    "Generate Self"
    (base_header brand)
    [
      common_form
        "Generate Self"
        [
          hiddenInput "SelfID" self.SelfID
          label_text "First Name" self.FirstName
          label_text "Last Name" self.LastName
          label_text "Title" self.Title
          icon_label_text "Email" self.Email "envelope"
          label_text "Position Objective 1" self.PositionObjective1
          label_text "Position Objective 2" self.PositionObjective2
          label_text "Position Objective 3" self.PositionObjective3
          label_select_selected "Supervisor" [("0", ""); ("1", "Sally"); ("2", "Bob"); ("3", "Pat"); ("4", "Tom")] (Some self.Supervisor)
          label_select_selected "Coworker or Client 1" [("0", ""); ("1", "Rex"); ("2", "Sue")] (Some self.CoworkerorClient1)
          label_select_selected "Coworker or Client 2" [("0", ""); ("1", "Rex"); ("2", "Sue")] (Some self.CoworkerorClient2)
          label_select_selected "Coworker or Client 3" [("0", ""); ("1", "Rex"); ("2", "Sue")] (Some self.CoworkerorClient3)
          label_select_selected "Coworker or Client 4" [("0", ""); ("1", "Rex"); ("2", "Sue")] (Some self.CoworkerorClient4)
        ]
    ]
    scripts.common

let view_generate_errored_self errors (selfForm : SelfForm) =
  base_html
    "Generate Self"
    (base_header brand)
    [
      common_form
        "Generate Self"
        [
          hiddenInput "SelfID" selfForm.SelfID
          errored_label_text "First Name" (string selfForm.FirstName) errors
          errored_label_text "Last Name" (string selfForm.LastName) errors
          errored_label_text "Title" (string selfForm.Title) errors
          errored_icon_label_text "Email" selfForm.Email "envelope" errors
          errored_label_text "Position Objective 1" (string selfForm.PositionObjective1) errors
          errored_label_text "Position Objective 2" (string selfForm.PositionObjective2) errors
          errored_label_text "Position Objective 3" (string selfForm.PositionObjective3) errors
          errored_label_select "Supervisor" [("0", ""); ("1", "Sally"); ("2", "Bob"); ("3", "Pat"); ("4", "Tom")] (Some selfForm.Supervisor) errors
          errored_label_select "Coworker or Client 1" [("0", ""); ("1", "Rex"); ("2", "Sue")] (Some selfForm.CoworkerorClient1) errors
          errored_label_select "Coworker or Client 2" [("0", ""); ("1", "Rex"); ("2", "Sue")] (Some selfForm.CoworkerorClient2) errors
          errored_label_select "Coworker or Client 3" [("0", ""); ("1", "Rex"); ("2", "Sue")] (Some selfForm.CoworkerorClient3) errors
          errored_label_select "Coworker or Client 4" [("0", ""); ("1", "Rex"); ("2", "Sue")] (Some selfForm.CoworkerorClient4) errors
        ]
    ]
    scripts.common

let view_view_self (self : Self) =
  let button = button_small_success_right (sprintf "/self/edit/%i" self.SelfID) [ text "Edit" ]
  base_html
    "Self"
    (base_header brand)
    [
      common_static_form button
        "Self"
        [

          label_static "First Name" self.FirstName
          label_static "Last Name" self.LastName
          label_static "Title" self.Title
          label_static "Email" self.Email
          label_static "Position Objective 1" self.PositionObjective1
          label_static "Position Objective 2" self.PositionObjective2
          label_static "Position Objective 3" self.PositionObjective3
          label_static "Supervisor" self.Supervisor
          label_static "Coworker or Client 1" self.CoworkerorClient1
          label_static "Coworker or Client 2" self.CoworkerorClient2
          label_static "Coworker or Client 3" self.CoworkerorClient3
          label_static "Coworker or Client 4" self.CoworkerorClient4
        ]
    ]
    scripts.common

let view_edit_self (self : Self) =
  base_html
    "Edit Self"
    (base_header brand)
    [
      common_form
        "Edit Self"
        [
          hiddenInput "SelfID" self.SelfID
          label_text "First Name" self.FirstName
          label_text "Last Name" self.LastName
          label_text "Title" self.Title
          icon_label_text "Email" self.Email "envelope"
          label_text "Position Objective 1" self.PositionObjective1
          label_text "Position Objective 2" self.PositionObjective2
          label_text "Position Objective 3" self.PositionObjective3
          label_select_selected "Supervisor" [("0", ""); ("1", "Sally"); ("2", "Bob"); ("3", "Pat"); ("4", "Tom")] (Some self.Supervisor)
          label_select_selected "Coworker or Client 1" [("0", ""); ("1", "Rex"); ("2", "Sue")] (Some self.CoworkerorClient1)
          label_select_selected "Coworker or Client 2" [("0", ""); ("1", "Rex"); ("2", "Sue")] (Some self.CoworkerorClient2)
          label_select_selected "Coworker or Client 3" [("0", ""); ("1", "Rex"); ("2", "Sue")] (Some self.CoworkerorClient3)
          label_select_selected "Coworker or Client 4" [("0", ""); ("1", "Rex"); ("2", "Sue")] (Some self.CoworkerorClient4)
        ]
    ]
    scripts.common

let view_edit_errored_self errors (selfForm : SelfForm) =
  base_html
    "Edit Self"
    (base_header brand)
    [
      common_form
        "Edit Self"
        [
          hiddenInput "SelfID" selfForm.SelfID
          errored_label_text "First Name" (string selfForm.FirstName) errors
          errored_label_text "Last Name" (string selfForm.LastName) errors
          errored_label_text "Title" (string selfForm.Title) errors
          errored_icon_label_text "Email" selfForm.Email "envelope" errors
          errored_label_text "Position Objective 1" (string selfForm.PositionObjective1) errors
          errored_label_text "Position Objective 2" (string selfForm.PositionObjective2) errors
          errored_label_text "Position Objective 3" (string selfForm.PositionObjective3) errors
          errored_label_select "Supervisor" [("0", ""); ("1", "Sally"); ("2", "Bob"); ("3", "Pat"); ("4", "Tom")] (Some selfForm.Supervisor) errors
          errored_label_select "Coworker or Client 1" [("0", ""); ("1", "Rex"); ("2", "Sue")] (Some selfForm.CoworkerorClient1) errors
          errored_label_select "Coworker or Client 2" [("0", ""); ("1", "Rex"); ("2", "Sue")] (Some selfForm.CoworkerorClient2) errors
          errored_label_select "Coworker or Client 3" [("0", ""); ("1", "Rex"); ("2", "Sue")] (Some selfForm.CoworkerorClient3) errors
          errored_label_select "Coworker or Client 4" [("0", ""); ("1", "Rex"); ("2", "Sue")] (Some selfForm.CoworkerorClient4) errors
        ]
    ]
    scripts.common

let view_list_self selfs =
  let toTr (self : Self) inner =
    trLink (sprintf "/self/view/%i" self.SelfID) inner

  let toTd (self : Self) =
    [
        td [ text (string self.SelfID) ]
        td [ text (string self.FirstName) ]
        td [ text (string self.LastName) ]
        td [ text (string self.Title) ]
        td [ text (string self.Email) ]
        td [ text (string self.PositionObjective1) ]
        td [ text (string self.PositionObjective2) ]
        td [ text (string self.PositionObjective3) ]
        td [ text (string self.Supervisor) ]
        td [ text (string self.CoworkerorClient1) ]
        td [ text (string self.CoworkerorClient2) ]
        td [ text (string self.CoworkerorClient3) ]
        td [ text (string self.CoworkerorClient4) ]
    ]

  base_html
    "List Self"
    (base_header brand)
    [
      container [
        row [
          form_wrapper [
            form_title [ h3Inner "List Selfs" [ button_small_success_right "/self/create" [ text "Create"] ] ]
            form_content [
              content [
                table_bordered_linked_tr
                  [
                    "Self ID"
                    "First Name"
                    "Last Name"
                    "Title"
                    "Email"
                    "Position Objective 1"
                    "Position Objective 2"
                    "Position Objective 3"
                    "Supervisor"
                    "Coworker or Client 1"
                    "Coworker or Client 2"
                    "Coworker or Client 3"
                    "Coworker or Client 4"
                  ]
                  selfs toTd toTr
              ]
            ]
          ]
        ]
      ]
    ]
    scripts.datatable_bundle

let view_search_self field how value selfs =
  let fields = ["Name", "Name"; "Food","Food"; "City", "City"]
  let hows = ["Equals", "Equals"; "Begins With","Begins With"]
  let toTr (self : Self) inner =
    trLink (sprintf "/self/view/%i" self.SelfID) inner

  let toTd (self : Self) =
    [
        td [ text (string self.SelfID) ]
        td [ text (string self.FirstName) ]
        td [ text (string self.LastName) ]
        td [ text (string self.Title) ]
        td [ text (string self.Email) ]
        td [ text (string self.PositionObjective1) ]
        td [ text (string self.PositionObjective2) ]
        td [ text (string self.PositionObjective3) ]
        td [ text (string self.Supervisor) ]
        td [ text (string self.CoworkerorClient1) ]
        td [ text (string self.CoworkerorClient2) ]
        td [ text (string self.CoworkerorClient3) ]
        td [ text (string self.CoworkerorClient4) ]
    ]

  base_html
    "Search Self"
    (base_header brand)
    [
      container [
        row [
          form_wrapper [
            form_title [ h3Inner "Search Selfs" [ ] ]
            form_content [
              divClass "search-bar" [
                form_inline [
                  content [
                    inline_label_select_selected "Field" fields field
                    inline_label_select_selected"How" hows how
                    inline_label_text "Value" value
                    button_submit_right
                  ]
                ]
              ]
              content [
                table_bordered_linked_tr
                  [
                    "Self ID"
                    "First Name"
                    "Last Name"
                    "Title"
                    "Email"
                    "Position Objective 1"
                    "Position Objective 2"
                    "Position Objective 3"
                    "Supervisor"
                    "Coworker or Client 1"
                    "Coworker or Client 2"
                    "Coworker or Client 3"
                    "Coworker or Client 4"
                  ]
                  selfs toTd toTr
              ]
            ]
          ]
        ]
      ]
    ]
    scripts.datatable_bundle

let view_create_customer =
  base_html
    "Create Customer"
    (base_header brand)
    [
      common_form
        "Create Customer"
        [
          hiddenInput "CustomerID" "-1"
          label_text "Customer Name" ""
          label_text "Contact Name" ""
          icon_label_text "Email" "" "envelope"
        ]
    ]
    scripts.common

let view_create_errored_customer errors (customerForm : CustomerForm) =
  base_html
    "Create Customer"
    (base_header brand)
    [
      common_form
        "Create Customer"
        [
          hiddenInput "CustomerID" customerForm.CustomerID
          errored_label_text "Customer Name" (string customerForm.CustomerName) errors
          errored_label_text "Contact Name" (string customerForm.ContactName) errors
          errored_icon_label_text "Email" customerForm.Email "envelope" errors
        ]
    ]
    scripts.common

let view_generate_customer (customer : Customer) =
  base_html
    "Generate Customer"
    (base_header brand)
    [
      common_form
        "Generate Customer"
        [
          hiddenInput "CustomerID" customer.CustomerID
          label_text "Customer Name" customer.CustomerName
          label_text "Contact Name" customer.ContactName
          icon_label_text "Email" customer.Email "envelope"
        ]
    ]
    scripts.common

let view_generate_errored_customer errors (customerForm : CustomerForm) =
  base_html
    "Generate Customer"
    (base_header brand)
    [
      common_form
        "Generate Customer"
        [
          hiddenInput "CustomerID" customerForm.CustomerID
          errored_label_text "Customer Name" (string customerForm.CustomerName) errors
          errored_label_text "Contact Name" (string customerForm.ContactName) errors
          errored_icon_label_text "Email" customerForm.Email "envelope" errors
        ]
    ]
    scripts.common

let view_view_customer (customer : Customer) =
  let button = button_small_success_right (sprintf "/customer/edit/%i" customer.CustomerID) [ text "Edit" ]
  base_html
    "Customer"
    (base_header brand)
    [
      common_static_form button
        "Customer"
        [

          label_static "Customer Name" customer.CustomerName
          label_static "Contact Name" customer.ContactName
          label_static "Email" customer.Email
        ]
    ]
    scripts.common

let view_edit_customer (customer : Customer) =
  base_html
    "Edit Customer"
    (base_header brand)
    [
      common_form
        "Edit Customer"
        [
          hiddenInput "CustomerID" customer.CustomerID
          label_text "Customer Name" customer.CustomerName
          label_text "Contact Name" customer.ContactName
          icon_label_text "Email" customer.Email "envelope"
        ]
    ]
    scripts.common

let view_edit_errored_customer errors (customerForm : CustomerForm) =
  base_html
    "Edit Customer"
    (base_header brand)
    [
      common_form
        "Edit Customer"
        [
          hiddenInput "CustomerID" customerForm.CustomerID
          errored_label_text "Customer Name" (string customerForm.CustomerName) errors
          errored_label_text "Contact Name" (string customerForm.ContactName) errors
          errored_icon_label_text "Email" customerForm.Email "envelope" errors
        ]
    ]
    scripts.common

let view_list_customer customers =
  let toTr (customer : Customer) inner =
    trLink (sprintf "/customer/view/%i" customer.CustomerID) inner

  let toTd (customer : Customer) =
    [
        td [ text (string customer.CustomerID) ]
        td [ text (string customer.CustomerName) ]
        td [ text (string customer.ContactName) ]
        td [ text (string customer.Email) ]
    ]

  base_html
    "List Customer"
    (base_header brand)
    [
      container [
        row [
          form_wrapper [
            form_title [ h3Inner "List Customers" [ button_small_success_right "/customer/create" [ text "Create"] ] ]
            form_content [
              content [
                table_bordered_linked_tr
                  [
                    "Customer ID"
                    "Customer Name"
                    "Contact Name"
                    "Email"
                  ]
                  customers toTd toTr
              ]
            ]
          ]
        ]
      ]
    ]
    scripts.datatable_bundle

let view_create_employeeSurvey =
  base_html
    "Create Employee Survey"
    (base_header brand)
    [
      common_form
        "Create Employee Survey"
        [
          hiddenInput "EmployeeSurveyID" "-1"
          label_text "Employee Name" ""
          label_textarea "Position Objective 1" ""
          label_textarea "Position Objective 2" ""
          label_textarea "Position Objective 3" ""
          label_select "Rating" [("0", ""); ("1", "1"); ("2", "2"); ("3", "3"); ("4", "4"); ("5", "5"); ("6", "6"); ("7", "7"); ("8", "8"); ("9", "9"); ("10", "10")]
        ]
    ]
    scripts.common

let view_create_errored_employeeSurvey errors (employeeSurveyForm : EmployeeSurveyForm) =
  base_html
    "Create Employee Survey"
    (base_header brand)
    [
      common_form
        "Create Employee Survey"
        [
          hiddenInput "EmployeeSurveyID" employeeSurveyForm.EmployeeSurveyID
          errored_label_text "Employee Name" (string employeeSurveyForm.EmployeeName) errors
          errored_label_textarea "Position Objective 1" (string employeeSurveyForm.PositionObjective1) errors
          errored_label_textarea "Position Objective 2" (string employeeSurveyForm.PositionObjective2) errors
          errored_label_textarea "Position Objective 3" (string employeeSurveyForm.PositionObjective3) errors
          errored_label_select "Rating" [("0", ""); ("1", "1"); ("2", "2"); ("3", "3"); ("4", "4"); ("5", "5"); ("6", "6"); ("7", "7"); ("8", "8"); ("9", "9"); ("10", "10")] (Some employeeSurveyForm.Rating) errors
        ]
    ]
    scripts.common

let view_generate_employeeSurvey (employeeSurvey : EmployeeSurvey) =
  base_html
    "Generate Employee Survey"
    (base_header brand)
    [
      common_form
        "Generate Employee Survey"
        [
          hiddenInput "EmployeeSurveyID" employeeSurvey.EmployeeSurveyID
          label_text "Employee Name" employeeSurvey.EmployeeName
          label_textarea "Position Objective 1" employeeSurvey.PositionObjective1
          label_textarea "Position Objective 2" employeeSurvey.PositionObjective2
          label_textarea "Position Objective 3" employeeSurvey.PositionObjective3
          label_select_selected "Rating" [("0", ""); ("1", "1"); ("2", "2"); ("3", "3"); ("4", "4"); ("5", "5"); ("6", "6"); ("7", "7"); ("8", "8"); ("9", "9"); ("10", "10")] (Some employeeSurvey.Rating)
        ]
    ]
    scripts.common

let view_generate_errored_employeeSurvey errors (employeeSurveyForm : EmployeeSurveyForm) =
  base_html
    "Generate Employee Survey"
    (base_header brand)
    [
      common_form
        "Generate Employee Survey"
        [
          hiddenInput "EmployeeSurveyID" employeeSurveyForm.EmployeeSurveyID
          errored_label_text "Employee Name" (string employeeSurveyForm.EmployeeName) errors
          errored_label_textarea "Position Objective 1" (string employeeSurveyForm.PositionObjective1) errors
          errored_label_textarea "Position Objective 2" (string employeeSurveyForm.PositionObjective2) errors
          errored_label_textarea "Position Objective 3" (string employeeSurveyForm.PositionObjective3) errors
          errored_label_select "Rating" [("0", ""); ("1", "1"); ("2", "2"); ("3", "3"); ("4", "4"); ("5", "5"); ("6", "6"); ("7", "7"); ("8", "8"); ("9", "9"); ("10", "10")] (Some employeeSurveyForm.Rating) errors
        ]
    ]
    scripts.common

let view_view_employeeSurvey (employeeSurvey : EmployeeSurvey) =
  let button = button_small_success_right (sprintf "/employeeSurvey/edit/%i" employeeSurvey.EmployeeSurveyID) [ text "Edit" ]
  base_html
    "Employee Survey"
    (base_header brand)
    [
      common_static_form button
        "Employee Survey"
        [

          label_static "Employee Name" employeeSurvey.EmployeeName
          label_static "Position Objective 1" employeeSurvey.PositionObjective1
          label_static "Position Objective 2" employeeSurvey.PositionObjective2
          label_static "Position Objective 3" employeeSurvey.PositionObjective3
          label_static "Rating" employeeSurvey.Rating
        ]
    ]
    scripts.common

let view_edit_employeeSurvey (employeeSurvey : EmployeeSurvey) =
  base_html
    "Edit Employee Survey"
    (base_header brand)
    [
      common_form
        "Edit Employee Survey"
        [
          hiddenInput "EmployeeSurveyID" employeeSurvey.EmployeeSurveyID
          label_text "Employee Name" employeeSurvey.EmployeeName
          label_textarea "Position Objective 1" employeeSurvey.PositionObjective1
          label_textarea "Position Objective 2" employeeSurvey.PositionObjective2
          label_textarea "Position Objective 3" employeeSurvey.PositionObjective3
          label_select_selected "Rating" [("0", ""); ("1", "1"); ("2", "2"); ("3", "3"); ("4", "4"); ("5", "5"); ("6", "6"); ("7", "7"); ("8", "8"); ("9", "9"); ("10", "10")] (Some employeeSurvey.Rating)
        ]
    ]
    scripts.common

let view_edit_errored_employeeSurvey errors (employeeSurveyForm : EmployeeSurveyForm) =
  base_html
    "Edit Employee Survey"
    (base_header brand)
    [
      common_form
        "Edit Employee Survey"
        [
          hiddenInput "EmployeeSurveyID" employeeSurveyForm.EmployeeSurveyID
          errored_label_text "Employee Name" (string employeeSurveyForm.EmployeeName) errors
          errored_label_textarea "Position Objective 1" (string employeeSurveyForm.PositionObjective1) errors
          errored_label_textarea "Position Objective 2" (string employeeSurveyForm.PositionObjective2) errors
          errored_label_textarea "Position Objective 3" (string employeeSurveyForm.PositionObjective3) errors
          errored_label_select "Rating" [("0", ""); ("1", "1"); ("2", "2"); ("3", "3"); ("4", "4"); ("5", "5"); ("6", "6"); ("7", "7"); ("8", "8"); ("9", "9"); ("10", "10")] (Some employeeSurveyForm.Rating) errors
        ]
    ]
    scripts.common

let view_list_employeeSurvey employeeSurveys =
  let toTr (employeeSurvey : EmployeeSurvey) inner =
    trLink (sprintf "/employeeSurvey/view/%i" employeeSurvey.EmployeeSurveyID) inner

  let toTd (employeeSurvey : EmployeeSurvey) =
    [
        td [ text (string employeeSurvey.EmployeeSurveyID) ]
        td [ text (string employeeSurvey.EmployeeName) ]
        td [ text (string employeeSurvey.PositionObjective1) ]
        td [ text (string employeeSurvey.PositionObjective2) ]
        td [ text (string employeeSurvey.PositionObjective3) ]
        td [ text (string employeeSurvey.Rating) ]
    ]

  base_html
    "List Employee Survey"
    (base_header brand)
    [
      container [
        row [
          form_wrapper [
            form_title [ h3Inner "List Employee Surveys" [ button_small_success_right "/employeeSurvey/create" [ text "Create"] ] ]
            form_content [
              content [
                table_bordered_linked_tr
                  [
                    "Employee Survey ID"
                    "Employee Name"
                    "Position Objective 1"
                    "Position Objective 2"
                    "Position Objective 3"
                    "Rating"
                  ]
                  employeeSurveys toTd toTr
              ]
            ]
          ]
        ]
      ]
    ]
    scripts.datatable_bundle

let view_search_employeeSurvey field how value employeeSurveys =
  let fields = ["Name", "Name"; "Food","Food"; "City", "City"]
  let hows = ["Equals", "Equals"; "Begins With","Begins With"]
  let toTr (employeeSurvey : EmployeeSurvey) inner =
    trLink (sprintf "/employeeSurvey/view/%i" employeeSurvey.EmployeeSurveyID) inner

  let toTd (employeeSurvey : EmployeeSurvey) =
    [
        td [ text (string employeeSurvey.EmployeeSurveyID) ]
        td [ text (string employeeSurvey.EmployeeName) ]
        td [ text (string employeeSurvey.PositionObjective1) ]
        td [ text (string employeeSurvey.PositionObjective2) ]
        td [ text (string employeeSurvey.PositionObjective3) ]
        td [ text (string employeeSurvey.Rating) ]
    ]

  base_html
    "Search Employee Survey"
    (base_header brand)
    [
      container [
        row [
          form_wrapper [
            form_title [ h3Inner "Search Employee Surveys" [ ] ]
            form_content [
              divClass "search-bar" [
                form_inline [
                  content [
                    inline_label_select_selected "Field" fields field
                    inline_label_select_selected"How" hows how
                    inline_label_text "Value" value
                    button_submit_right
                  ]
                ]
              ]
              content [
                table_bordered_linked_tr
                  [
                    "Employee Survey ID"
                    "Employee Name"
                    "Position Objective 1"
                    "Position Objective 2"
                    "Position Objective 3"
                    "Rating"
                  ]
                  employeeSurveys toTd toTr
              ]
            ]
          ]
        ]
      ]
    ]
    scripts.datatable_bundle

let view_create_customerSurvey =
  base_html
    "Create Customer Survey"
    (base_header brand)
    [
      common_form
        "Create Customer Survey"
        [
          hiddenInput "CustomerSurveyID" "-1"
          label_text "Customer" ""
          label_textarea "Comment" ""
          label_select "How Likely" [("0", ""); ("1", "1"); ("2", "2"); ("3", "3"); ("4", "4"); ("5", "5"); ("6", "6"); ("7", "7"); ("8", "8"); ("9", "9"); ("10", "10")]
        ]
    ]
    scripts.common

let view_create_errored_customerSurvey errors (customerSurveyForm : CustomerSurveyForm) =
  base_html
    "Create Customer Survey"
    (base_header brand)
    [
      common_form
        "Create Customer Survey"
        [
          hiddenInput "CustomerSurveyID" customerSurveyForm.CustomerSurveyID
          errored_label_text "Customer" (string customerSurveyForm.Customer) errors
          errored_label_textarea "Comment" (string customerSurveyForm.Comment) errors
          errored_label_select "How Likely" [("0", ""); ("1", "1"); ("2", "2"); ("3", "3"); ("4", "4"); ("5", "5"); ("6", "6"); ("7", "7"); ("8", "8"); ("9", "9"); ("10", "10")] (Some customerSurveyForm.HowLikely) errors
        ]
    ]
    scripts.common

let view_generate_customerSurvey (customerSurvey : CustomerSurvey) =
  base_html
    "Generate Customer Survey"
    (base_header brand)
    [
      common_form
        "Generate Customer Survey"
        [
          hiddenInput "CustomerSurveyID" customerSurvey.CustomerSurveyID
          label_text "Customer" customerSurvey.Customer
          label_textarea "Comment" customerSurvey.Comment
          label_select_selected "How Likely" [("0", ""); ("1", "1"); ("2", "2"); ("3", "3"); ("4", "4"); ("5", "5"); ("6", "6"); ("7", "7"); ("8", "8"); ("9", "9"); ("10", "10")] (Some customerSurvey.HowLikely)
        ]
    ]
    scripts.common

let view_generate_errored_customerSurvey errors (customerSurveyForm : CustomerSurveyForm) =
  base_html
    "Generate Customer Survey"
    (base_header brand)
    [
      common_form
        "Generate Customer Survey"
        [
          hiddenInput "CustomerSurveyID" customerSurveyForm.CustomerSurveyID
          errored_label_text "Customer" (string customerSurveyForm.Customer) errors
          errored_label_textarea "Comment" (string customerSurveyForm.Comment) errors
          errored_label_select "How Likely" [("0", ""); ("1", "1"); ("2", "2"); ("3", "3"); ("4", "4"); ("5", "5"); ("6", "6"); ("7", "7"); ("8", "8"); ("9", "9"); ("10", "10")] (Some customerSurveyForm.HowLikely) errors
        ]
    ]
    scripts.common

let view_view_customerSurvey (customerSurvey : CustomerSurvey) =
  let button = button_small_success_right (sprintf "/customerSurvey/edit/%i" customerSurvey.CustomerSurveyID) [ text "Edit" ]
  base_html
    "Customer Survey"
    (base_header brand)
    [
      common_static_form button
        "Customer Survey"
        [

          label_static "Customer" customerSurvey.Customer
          label_static "Comment" customerSurvey.Comment
          label_static "How Likely" customerSurvey.HowLikely
        ]
    ]
    scripts.common

let view_edit_customerSurvey (customerSurvey : CustomerSurvey) =
  base_html
    "Edit Customer Survey"
    (base_header brand)
    [
      common_form
        "Edit Customer Survey"
        [
          hiddenInput "CustomerSurveyID" customerSurvey.CustomerSurveyID
          label_text "Customer" customerSurvey.Customer
          label_textarea "Comment" customerSurvey.Comment
          label_select_selected "How Likely" [("0", ""); ("1", "1"); ("2", "2"); ("3", "3"); ("4", "4"); ("5", "5"); ("6", "6"); ("7", "7"); ("8", "8"); ("9", "9"); ("10", "10")] (Some customerSurvey.HowLikely)
        ]
    ]
    scripts.common

let view_edit_errored_customerSurvey errors (customerSurveyForm : CustomerSurveyForm) =
  base_html
    "Edit Customer Survey"
    (base_header brand)
    [
      common_form
        "Edit Customer Survey"
        [
          hiddenInput "CustomerSurveyID" customerSurveyForm.CustomerSurveyID
          errored_label_text "Customer" (string customerSurveyForm.Customer) errors
          errored_label_textarea "Comment" (string customerSurveyForm.Comment) errors
          errored_label_select "How Likely" [("0", ""); ("1", "1"); ("2", "2"); ("3", "3"); ("4", "4"); ("5", "5"); ("6", "6"); ("7", "7"); ("8", "8"); ("9", "9"); ("10", "10")] (Some customerSurveyForm.HowLikely) errors
        ]
    ]
    scripts.common

let view_list_customerSurvey customerSurveys =
  let toTr (customerSurvey : CustomerSurvey) inner =
    trLink (sprintf "/customerSurvey/view/%i" customerSurvey.CustomerSurveyID) inner

  let toTd (customerSurvey : CustomerSurvey) =
    [
        td [ text (string customerSurvey.CustomerSurveyID) ]
        td [ text (string customerSurvey.Customer) ]
        td [ text (string customerSurvey.Comment) ]
        td [ text (string customerSurvey.HowLikely) ]
    ]

  base_html
    "List Customer Survey"
    (base_header brand)
    [
      container [
        row [
          form_wrapper [
            form_title [ h3Inner "List Customer Surveys" [ button_small_success_right "/customerSurvey/create" [ text "Create"] ] ]
            form_content [
              content [
                table_bordered_linked_tr
                  [
                    "Customer Survey ID"
                    "Customer"
                    "Comment"
                    "How Likely"
                  ]
                  customerSurveys toTd toTr
              ]
            ]
          ]
        ]
      ]
    ]
    scripts.datatable_bundle

let view_search_customerSurvey field how value customerSurveys =
  let fields = ["Name", "Name"; "Food","Food"; "City", "City"]
  let hows = ["Equals", "Equals"; "Begins With","Begins With"]
  let toTr (customerSurvey : CustomerSurvey) inner =
    trLink (sprintf "/customerSurvey/view/%i" customerSurvey.CustomerSurveyID) inner

  let toTd (customerSurvey : CustomerSurvey) =
    [
        td [ text (string customerSurvey.CustomerSurveyID) ]
        td [ text (string customerSurvey.Customer) ]
        td [ text (string customerSurvey.Comment) ]
        td [ text (string customerSurvey.HowLikely) ]
    ]

  base_html
    "Search Customer Survey"
    (base_header brand)
    [
      container [
        row [
          form_wrapper [
            form_title [ h3Inner "Search Customer Surveys" [ ] ]
            form_content [
              divClass "search-bar" [
                form_inline [
                  content [
                    inline_label_select_selected "Field" fields field
                    inline_label_select_selected"How" hows how
                    inline_label_text "Value" value
                    button_submit_right
                  ]
                ]
              ]
              content [
                table_bordered_linked_tr
                  [
                    "Customer Survey ID"
                    "Customer"
                    "Comment"
                    "How Likely"
                  ]
                  customerSurveys toTd toTr
              ]
            ]
          ]
        ]
      ]
    ]
    scripts.datatable_bundle
