module script

open dsl
open generator

let someSite () =

  db SQLServer

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
      boolean   "Free Delivery"
      dropdown  "Free Soda"     Required
        [
          "Cola"
          "Orange"
          "Root Beer"
        ]
    ]

  page "Order Item" CVEL
    [
      text      "Name"          Required
      fk        "Order"
    ]

  api "Order"

  dashboard "Order"
    [
      line "Delivery Date"
      bar  "State"
      pie  "Tip"
    ]

  advancedPage "Reserveration" CVELS RequiresLogin
    [
      text  "Name"         Required
      date  "Date"         Required
      phone "Phone Number" Required
    ]

  currentSite
