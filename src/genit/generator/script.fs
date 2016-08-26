module script

open dsl
open generator

let someSite () =

  db Postgres

  dbPassword "NOTSecure1234"

  site "defi Pays"

  basic home

  basic registration

  basic login

  page "Self" CVELS
    [
      text      "First Name"           Required
      text      "Last Name"            Required
      text      "Title"                Required
      email     "Email"
      text      "Position Objective 1" Required
      text      "Position Objective 2" Null
      text      "Position Objective 3" Null
      dropdown  "Supervisor"           [ "Sally"; "Bob"; "Pat"; "Tom" ]
      dropdown  "Coworker or Client 1" [ "Rex"; "Sue" ]
      dropdown  "Coworker or Client 2" [ "Rex"; "Sue" ]
      dropdown  "Coworker or Client 3" [ "Rex"; "Sue" ]
      dropdown  "Coworker or Client 4" [ "Rex"; "Sue" ]
    ]

  advancedPage "Customer" CVEL RequiresLogin
    [
      text  "Customer Name"            Required
      text  "Contact Name"             Required
      email "Email"
    ]

  page "Employee Survey" CVELS
    [
      text      "Employee Name"        Null
      paragraph "Position Objective 1" Null
      paragraph "Position Objective 2" Null
      paragraph "Position Objective 3" Null
      dropdown  "Rating"               [ "1"; "2"; "3"; "4"; "5"; "6"; "7"; "8"; "9"; "10"; ]
    ]

  page "Customer Survey" CVELS
    [
      text      "Customer"             Null
      paragraph "Comment"              Required
      dropdown  "How Likely"           [ "1"; "2"; "3"; "4"; "5"; "6"; "7"; "8"; "9"; "10"; ]
    ]


  currentSite
