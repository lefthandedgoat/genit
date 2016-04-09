module generated_views

open Suave.Html
open htmlHelpers
open bootstrapHelpers

let brand = "Bob's Burgers"

let get_register =
  base_html
    "Register"
    [
      base_header brand
      common_form
        "Register"
        [
          label_text "First Name" ""
          label_text "Last Name" ""
          icon_label_text "Email" "" "envelope"
          icon_password_text "Password" "" "lock"
          icon_password_text "Repeat Password" "" "lock"
          label_text "Nickname" ""
        ]
    ]
    scripts.common
