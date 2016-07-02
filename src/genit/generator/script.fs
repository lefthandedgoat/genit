module script

open dsl
open generator

let someSite () =

  db Postgres

  dbPassword "NOTSecure1234"

  site "Bob's Burgers"

  basic home

  basic registration

  basic login

  page "Order" CVELS
    [
      text      "Name"          Required
      text      "Food"          Null
      text      "Drinks"        Null
      dollar    "Tip"           Null
      paragraph "Notes"         Null
      date      "Delivery Date" Required
      phone     "Phone Number"  Required
      text      "Address"       Null
      text      "City"          Null
      text      "State"         Null
      text      "Zip"           Null
      dropdown  "Free Soda"
        [
          "Cola"
          "Orange"
          "Root Beer"
        ]
    ]

  api "Order"

  dashboard "Order"

  advancedPage "Reserveration" CVELS RequiresLogin
    [
      text  "Name"         Required
      date  "Date"         Required
      phone "Phone Number" Required
    ]

  currentSite
