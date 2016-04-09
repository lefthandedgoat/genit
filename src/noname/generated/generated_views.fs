module generated_views

open Suave.Html
open htmlHelpers
open bootstrapHelpers
open generated_html

let brand = "Bob's Burgers"

let get_register =
  base_html
    "Register"
    [
      base_header brand
      common_form
        "Register"
        [
          icon_label_text "First Name" "" "user" 
          icon_label_text "Last Name" "" "user" 
          icon_label_text "Email" "" "envelope" 
          icon_password_text "Password" "" "lock" 
          icon_password_text "Repeat Password" "" "lock" 
        ]
    ]
    scripts.common