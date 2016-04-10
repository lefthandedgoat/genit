module script

open dsl
open generator

let someSite () =

  site "Bob's Burgers"

  //basic home
  basic registration

  page "Order" Submit
    [
      text "Name" Required
      text "Food" Null
      text "Drinks" Null
      text "Notes" Null
      text "Address" Null
      text "City" Null
      text "Zip" Null
    ]

  page "Confirm Order" Submit
    [
      text "Name" Required
      text "Food" Null
      text "Drinks" Null
      text "Notes" Null
      text "Address" Null
      text "City" Null
      text "Zip" Null
    ]

  currentSite
