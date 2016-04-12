module script

open dsl
open generator

let someSite () =

  site "Bob's Burgers"

  basic home

  basic registration

  basic login

  page "Order" Submit
    [
      text "Name" Required
      text "Food" Null
      text "Drinks" Null
      dollar "Tip" Null
      paragraph "Notes" Null
      date "Delivery Date" Required
      phone "Phone Number" Required
      text "Address" Null
      text "City" Null
      number "Zip" Null
      dropdown "Free Soda"
        [
          "Cola"
          "Orange"
          "Root Beer"
        ]
    ]

  currentSite
